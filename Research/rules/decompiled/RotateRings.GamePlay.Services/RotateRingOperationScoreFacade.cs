using System;
using System.Collections.Generic;
using RotateRings.GamePlay.Services.CharacterExpression;
using RotateRings.GameWinIQ;
using RotateRings.Model;
using RotateRings.UI.Game;
using RotateRings.UI.Shop;
using UnityEngine;
using UnlockRing;
using UnlockRing.Core.Domain;
using UnlockRing.Presentation;

namespace RotateRings.GamePlay.Services;

public sealed class RotateRingOperationScoreFacade : GameplayServiceBase<GameplayManager>
{
	private const int InvalidStepThreshold = 3;

	private const int InvalidStepScoreDeduction = 50;

	private const int MinimumReleaseScore = 50;

	private readonly GameplayStepRecorder stepRecorder;

	private readonly GameplayOperationScoreTracker scoreTracker;

	private readonly GameplayComboEffectPlayer comboEffectPlayer;

	private readonly Queue<GameplayOperationEvaluation> pendingEncouragements = new Queue<GameplayOperationEvaluation>();

	private readonly Queue<Vector3> pendingEncouragementPositions = new Queue<Vector3>();

	private GameplayOperationEvaluation currentStepEvaluation;

	private Vector3 currentStepPositionSum;

	private int currentStepPositionCount;

	private bool suppressScore;

	private bool deferPlayback;

	private int invalidStepsSinceLastRelease;

	public Action<GameplayOperationEvaluation, Vector3> OnEncouragementEvaluated { get; set; }

	public int RecentStepCount => scoreTracker.RecentStepCount;

	public RotateRingOperationScoreFacade(GameplayManager host)
		: base(host)
	{
		stepRecorder = new GameplayStepRecorder(OnStepCommitted);
		scoreTracker = new GameplayOperationScoreTracker(OnScoreEvaluated);
		comboEffectPlayer = new GameplayComboEffectPlayer(base.DestroyToken);
		GameApp.Event.Listen<BombBreakTickEvent>((Action<BombBreakTickEvent>)OnBombBreakTick);
	}

	public void InitializeScore()
	{
		scoreTracker.Initialize();
	}

	public void Record(GameplayOperationResult operation)
	{
		if (!base.IsDisposed)
		{
			ShopService.RecordGameplayOperation();
			stepRecorder.Record(operation);
		}
	}

	public int CountRecentReleaseSteps()
	{
		return scoreTracker.CountRecentReleaseSteps();
	}

	public void BeginDeferEncouragements()
	{
		deferPlayback = true;
	}

	public void EndDeferEncouragements()
	{
		deferPlayback = false;
		if (PresentationMode.UseTaskPresentation)
		{
			ClearDeferredEncouragements();
		}
	}

	private void FlushDeferredEncouragements()
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		while (pendingEncouragements.Count > 0)
		{
			GameplayOperationEvaluation evaluation = pendingEncouragements.Dequeue();
			Vector3 worldPosition = pendingEncouragementPositions.Dequeue();
			comboEffectPlayer.PlayStep(evaluation, worldPosition);
		}
	}

	private void OnBombBreakTick(BombBreakTickEvent evt)
	{
		if (!base.IsDisposed)
		{
			FlushDeferredEncouragements();
		}
	}

	private void ClearDeferredEncouragements()
	{
		pendingEncouragements.Clear();
		pendingEncouragementPositions.Clear();
	}

	public void RecordSilent(GameplayOperationResult operation)
	{
		if (base.IsDisposed)
		{
			return;
		}
		suppressScore = true;
		try
		{
			stepRecorder.Record(operation);
		}
		finally
		{
			suppressScore = false;
		}
	}

	public void Flush()
	{
		if (!base.IsDisposed)
		{
			stepRecorder.Flush();
		}
	}

	public void StopActiveEncouragementEffects()
	{
		if (!base.IsDisposed)
		{
			ClearDeferredEncouragements();
			comboEffectPlayer.StopAndRecycleAllActiveEffects();
			UIGameEncouragementLayout.ClearPlacedEncouragements();
		}
	}

	public void PlayEncouragementFromResult(GameplayOperationEvaluation evaluation, Vector3 worldPosition)
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		if (!base.IsDisposed)
		{
			comboEffectPlayer.PlayStep(evaluation, worldPosition);
		}
	}

	public void Reset()
	{
		if (!base.IsDisposed)
		{
			stepRecorder.Reset();
			scoreTracker.Reset();
			invalidStepsSinceLastRelease = 0;
			comboEffectPlayer.ClearUnbelievableStreak();
			StopActiveEncouragementEffects();
			comboEffectPlayer.OpenPlaybackGate();
			base.Host.GetGameService<CharacterExpressionEncourageService>()?.ClearLevel();
		}
	}

	public override void Dispose()
	{
		if (!base.IsDisposed)
		{
			base.Dispose();
			GameApp.Event.Unlisten<BombBreakTickEvent>((Action<BombBreakTickEvent>)OnBombBreakTick);
			ClearDeferredEncouragements();
			stepRecorder.Reset();
			scoreTracker.Reset();
			comboEffectPlayer.Dispose();
		}
	}

	private void OnStepCommitted(GameplayStepRecord step)
	{
		if (step != null && !base.IsDisposed)
		{
			OnGameplayStepCommitted(step);
			base.Host.GetGameService<CumulativeReleaseService>()?.RecordStep(step);
			base.Host.GetGameService<CharacterExpressionEncourageService>()?.HandleStepCommitted(step);
			if (step.State != GameplayStepState.UnlockRelease || step.ReleasedParts.Count == 0)
			{
				comboEffectPlayer.ClearUnbelievableStreak();
			}
			if (!suppressScore)
			{
				ResetCurrentStepEvaluation();
				scoreTracker.Evaluate(step);
				PlayCurrentStepEvaluation();
			}
		}
	}

	private void OnScoreEvaluated(GameplayOperationScore score)
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		if (score == null || base.IsDisposed)
		{
			return;
		}
		Vector3? val = TryGetLegacyPartWorldPosition(score.PartId);
		if (val.HasValue)
		{
			if (score.Evaluation > currentStepEvaluation)
			{
				currentStepEvaluation = score.Evaluation;
			}
			currentStepPositionSum += val.Value;
			currentStepPositionCount++;
		}
	}

	private void ResetCurrentStepEvaluation()
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		currentStepEvaluation = GameplayOperationEvaluation.None;
		currentStepPositionSum = Vector3.zero;
		currentStepPositionCount = 0;
	}

	private void PlayCurrentStepEvaluation()
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		if (currentStepPositionCount == 0)
		{
			return;
		}
		Vector3 val = currentStepPositionSum / (float)currentStepPositionCount;
		OnEncouragementEvaluated?.Invoke(currentStepEvaluation, val);
		if (!PresentationMode.UseTaskPresentation)
		{
			if (deferPlayback)
			{
				pendingEncouragements.Enqueue(currentStepEvaluation);
				pendingEncouragementPositions.Enqueue(val);
			}
			else
			{
				comboEffectPlayer.PlayStep(currentStepEvaluation, val);
			}
		}
	}

	private Vector3? TryGetLegacyPartWorldPosition(PartId partId)
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		RingObjectsService objectsService = base.Host.ObjectsService;
		if (objectsService != null && objectsService.TryGetPartObject(partId, out var gameplayObject))
		{
			Component val = (Component)((gameplayObject is Component) ? gameplayObject : null);
			if (val != null)
			{
				Renderer component = val.GetComponent<Renderer>();
				if ((Object)(object)component != (Object)null)
				{
					Bounds bounds = component.bounds;
					return ((Bounds)(ref bounds)).center;
				}
				return val.transform.position;
			}
		}
		return null;
	}

	private void OnGameplayStepCommitted(GameplayStepRecord step)
	{
		if (step != null)
		{
			GameApp.Proxy.Get<LevelModelProxy>().CommitGameplayStep();
			GameApp.Proxy.Get<GameWinIQProxy>()?.SessionTracker.RecordStep(step);
			switch (step.State)
			{
			case GameplayStepState.Error:
			case GameplayStepState.NoEffect:
				invalidStepsSinceLastRelease++;
				break;
			case GameplayStepState.UnlockRelease:
				SettleLegacyReleaseBatchScore(step);
				invalidStepsSinceLastRelease = 0;
				break;
			}
		}
	}

	private void SettleLegacyReleaseBatchScore(GameplayStepRecord step)
	{
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		IReadOnlyList<GameplayReleasedPart> releasedParts = step.ReleasedParts;
		if (releasedParts == null || releasedParts.Count == 0)
		{
			return;
		}
		RingObjectsService objectsService = base.Host.ObjectsService;
		bool flag = invalidStepsSinceLastRelease >= 3;
		int num = 0;
		for (int i = 0; i < releasedParts.Count; i++)
		{
			GameplayReleasedPart gameplayReleasedPart = releasedParts[i];
			if (gameplayReleasedPart != null && objectsService != null && objectsService.TryGetPartObject(gameplayReleasedPart.PartId, out var gameplayObject) && gameplayObject is IThreadingScoreProvider threadingScoreProvider)
			{
				int releaseScore = threadingScoreProvider.GetReleaseScore();
				int num2 = (flag ? Math.Max(releaseScore - 50, 50) : releaseScore);
				num += num2;
			}
		}
		if (num > 0)
		{
			GameApp.Proxy.Get<LevelModelProxy>().AddScore(num);
		}
	}
}
