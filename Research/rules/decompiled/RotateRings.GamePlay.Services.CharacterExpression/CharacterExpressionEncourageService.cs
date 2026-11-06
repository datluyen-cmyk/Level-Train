using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.CompilerServices;
using RotateRings.UI.Game;
using Rotaterings.GM;
using UnityEngine;
using UnlockRing;
using UnlockRing.Presentation;
using XingYun.Experiment;

namespace RotateRings.GamePlay.Services.CharacterExpression;

public sealed class CharacterExpressionEncourageService : GameplayServiceBase<GameplayManager>
{
	[StructLayout(LayoutKind.Auto)]
	[CompilerGenerated]
	private struct _003CFlushBombBatchAfterAcornCollectAsync_003Ed__28 : IAsyncStateMachine
	{
		public int _003C_003E1__state;

		public AsyncUniTaskVoidMethodBuilder _003C_003Et__builder;

		public CancellationToken ct;

		public CharacterExpressionEncourageService _003C_003E4__this;

		private Awaiter<bool> _003C_003Eu__1;

		private void MoveNext()
		{
			//IL_0060: Unknown result type (might be due to invalid IL or missing references)
			//IL_0065: Unknown result type (might be due to invalid IL or missing references)
			//IL_006d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0019: Unknown result type (might be due to invalid IL or missing references)
			//IL_001e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0022: Unknown result type (might be due to invalid IL or missing references)
			//IL_0027: Unknown result type (might be due to invalid IL or missing references)
			//IL_002b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0030: Unknown result type (might be due to invalid IL or missing references)
			//IL_0045: Unknown result type (might be due to invalid IL or missing references)
			//IL_0047: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ef: Unknown result type (might be due to invalid IL or missing references)
			int num = _003C_003E1__state;
			CharacterExpressionEncourageService characterExpressionEncourageService = _003C_003E4__this;
			try
			{
				Awaiter<bool> val2;
				if (num != 0)
				{
					UniTask val = UniTask.Yield((PlayerLoopTiming)13, ct);
					val2 = ((UniTask)(ref val)).SuppressCancellationThrow().GetAwaiter();
					if (!val2.IsCompleted)
					{
						num = (_003C_003E1__state = 0);
						_003C_003Eu__1 = val2;
						((AsyncUniTaskVoidMethodBuilder)(ref _003C_003Et__builder)).AwaitUnsafeOnCompleted<Awaiter<bool>, _003CFlushBombBatchAfterAcornCollectAsync_003Ed__28>(ref val2, ref this);
						return;
					}
				}
				else
				{
					val2 = _003C_003Eu__1;
					_003C_003Eu__1 = default(Awaiter<bool>);
					num = (_003C_003E1__state = -1);
				}
				if (!val2.GetResult() && !characterExpressionEncourageService.IsDisposed && characterExpressionEncourageService.waitingBombBreakFlush && characterExpressionEncourageService.bombBatchConditions.Count != 0)
				{
					characterExpressionEncourageService.matchedConditions.Clear();
					characterExpressionEncourageService.matchedConditions.AddRange(characterExpressionEncourageService.bombBatchConditions);
					Vector3 bombBatchWorldPosition = characterExpressionEncourageService.bombBatchWorldPosition;
					bool hasBombBatchWorldPosition = characterExpressionEncourageService.hasBombBatchWorldPosition;
					int effectiveFeedback = ResolveEffectiveFeedback();
					characterExpressionEncourageService.waitingBombBreakFlush = false;
					characterExpressionEncourageService.bombBatchConditions.Clear();
					characterExpressionEncourageService.hasBombBatchWorldPosition = false;
					if (hasBombBatchWorldPosition)
					{
						characterExpressionEncourageService.TryPlayFromWorldPosition(bombBatchWorldPosition, characterExpressionEncourageService.matchedConditions, effectiveFeedback);
					}
				}
			}
			catch (Exception exception)
			{
				_003C_003E1__state = -2;
				((AsyncUniTaskVoidMethodBuilder)(ref _003C_003Et__builder)).SetException(exception);
				return;
			}
			_003C_003E1__state = -2;
			((AsyncUniTaskVoidMethodBuilder)(ref _003C_003Et__builder)).SetResult();
		}

		void IAsyncStateMachine.MoveNext()
		{
			//ILSpy generated this explicit interface implementation from .override directive in MoveNext
			this.MoveNext();
		}

		[DebuggerHidden]
		private void SetStateMachine(IAsyncStateMachine stateMachine)
		{
			((AsyncUniTaskVoidMethodBuilder)(ref _003C_003Et__builder)).SetStateMachine(stateMachine);
		}

		void IAsyncStateMachine.SetStateMachine(IAsyncStateMachine stateMachine)
		{
			//ILSpy generated this explicit interface implementation from .override directive in SetStateMachine
			this.SetStateMachine(stateMachine);
		}
	}

	private const string TAG = "[CharacterExpression][EncourageService] ";

	private const int ConsecutiveEliminationTarget = 3;

	private const int RequiredStepIndexGapForConsecutive = 1;

	private const int MultiReleaseThreshold = 2;

	private const float EliminationIntervalSeconds = 15f;

	private readonly CharacterExpressionEffectPlayer effectPlayer;

	private readonly List<CharacterExpressionTriggerCondition> matchedConditions = new List<CharacterExpressionTriggerCondition>(4);

	private int consecutiveEliminationCount;

	private int lastEliminationStepIndex;

	private float lastEliminationRealtime;

	private bool waitingBombBreakFlush;

	private Vector3 bombBatchWorldPosition;

	private bool hasBombBatchWorldPosition;

	private readonly List<CharacterExpressionTriggerCondition> bombBatchConditions = new List<CharacterExpressionTriggerCondition>(4);

	private int lastPlayFrame = -1;

	private int lastEmojiStepIndex = -1;

	private bool pendingAcornCollectTrigger;

	public Action<IReadOnlyList<CharacterExpressionTriggerCondition>, Vector3, int> OnExpressionEncouragementTriggered { get; set; }

	public CharacterExpressionEncourageService(GameplayManager host)
		: base(host)
	{
		effectPlayer = new CharacterExpressionEffectPlayer(base.DestroyToken);
		GameApp.Event.Listen<RingSolveEvent>((Action<RingSolveEvent>)OnRingSolveEvent);
		GameApp.Event.Listen<BombBreakTickEvent>((Action<BombBreakTickEvent>)OnBombBreakTick);
	}

	public override void ClearLevel()
	{
		effectPlayer.ClearPending();
		UIGameEncouragementLayout.ClearPlacedEncouragements();
		ResetRuntimeState();
	}

	public override void Dispose()
	{
		if (!base.IsDisposed)
		{
			GameApp.Event.Unlisten<RingSolveEvent>((Action<RingSolveEvent>)OnRingSolveEvent);
			GameApp.Event.Unlisten<BombBreakTickEvent>((Action<BombBreakTickEvent>)OnBombBreakTick);
			effectPlayer.Dispose();
			base.Dispose();
		}
	}

	public void PlayExpressionFromResult(int effectiveFeedback, Vector3 worldPosition)
	{
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		if (!base.IsDisposed && effectiveFeedback > 0 && CharacterExpressionEffectCatalog.TryPickEffect(effectiveFeedback, out var effectLocation))
		{
			effectPlayer.Play(effectLocation, worldPosition);
		}
	}

	public void HandleStepCommitted(GameplayStepRecord step)
	{
		//IL_00cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
		if (base.IsDisposed || step == null || step.State != GameplayStepState.UnlockRelease || step.ReleasedPartCount <= 0)
		{
			return;
		}
		bool forceShowOnElimination = GMCharacterExpressionDebugState.ForceShowOnElimination;
		int num = ResolveEffectiveFeedback();
		if (num <= 0)
		{
			UpdateEliminationTracking(step.StepIndex);
			return;
		}
		matchedConditions.Clear();
		if (forceShowOnElimination)
		{
			matchedConditions.Add(CharacterExpressionTriggerCondition.GmForceShow);
			pendingAcornCollectTrigger = false;
		}
		else
		{
			EvaluateConsecutiveElimination(step.StepIndex);
			EvaluateMultiRelease(step.ReleasedPartCount);
			EvaluateLongIntervalElimination();
			MergePendingAcornCollectCondition();
		}
		UpdateEliminationTracking(step.StepIndex);
		if (matchedConditions.Count == 0 || !TryGetStepEncouragementWorldPosition(step, out var worldPosition))
		{
			return;
		}
		if (IsBombStep(step))
		{
			if (!forceShowOnElimination)
			{
				MergePendingAcornCollectCondition();
			}
			QueueBombBatchPlay(worldPosition, matchedConditions);
		}
		else
		{
			TryPlayForStep(step.StepIndex, worldPosition, matchedConditions, num);
		}
	}

	private void OnRingSolveEvent(RingSolveEvent eventData)
	{
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		if (base.IsDisposed || eventData == null || !eventData.HasCollectScreenPosition || ResolveEffectiveFeedback() <= 0)
		{
			return;
		}
		if (IsInBombCascadeScope())
		{
			waitingBombBreakFlush = true;
			MergeConditions(bombBatchConditions, CharacterExpressionTriggerCondition.AcornCollect);
			if (!hasBombBatchWorldPosition && eventData.HasSolvedPartId && CharacterExpressionWorldAnchor.TryGetPartVisualCenterWorldPosition(base.Host.ObjectsService, eventData.SolvedPartId, out var worldPosition))
			{
				bombBatchWorldPosition = worldPosition;
				hasBombBatchWorldPosition = true;
			}
		}
		else
		{
			pendingAcornCollectTrigger = true;
		}
	}

	private void OnBombBreakTick(BombBreakTickEvent evt)
	{
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		if (!base.IsDisposed && waitingBombBreakFlush && bombBatchConditions.Count != 0)
		{
			UniTaskVoid val = FlushBombBatchAfterAcornCollectAsync(base.DestroyToken);
			((UniTaskVoid)(ref val)).Forget();
		}
	}

	[AsyncStateMachine(typeof(_003CFlushBombBatchAfterAcornCollectAsync_003Ed__28))]
	private UniTaskVoid FlushBombBatchAfterAcornCollectAsync(CancellationToken ct)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		_003CFlushBombBatchAfterAcornCollectAsync_003Ed__28 _003CFlushBombBatchAfterAcornCollectAsync_003Ed__ = default(_003CFlushBombBatchAfterAcornCollectAsync_003Ed__28);
		_003CFlushBombBatchAfterAcornCollectAsync_003Ed__._003C_003Et__builder = AsyncUniTaskVoidMethodBuilder.Create();
		_003CFlushBombBatchAfterAcornCollectAsync_003Ed__._003C_003E4__this = this;
		_003CFlushBombBatchAfterAcornCollectAsync_003Ed__.ct = ct;
		_003CFlushBombBatchAfterAcornCollectAsync_003Ed__._003C_003E1__state = -1;
		((AsyncUniTaskVoidMethodBuilder)(ref _003CFlushBombBatchAfterAcornCollectAsync_003Ed__._003C_003Et__builder)).Start<_003CFlushBombBatchAfterAcornCollectAsync_003Ed__28>(ref _003CFlushBombBatchAfterAcornCollectAsync_003Ed__);
		return ((AsyncUniTaskVoidMethodBuilder)(ref _003CFlushBombBatchAfterAcornCollectAsync_003Ed__._003C_003Et__builder)).Task;
	}

	private bool IsInBombCascadeScope()
	{
		return base.Host.RingBoom?.IsInBombCascadeScope ?? false;
	}

	private void QueueBombBatchPlay(Vector3 worldPosition, List<CharacterExpressionTriggerCondition> conditions)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		waitingBombBreakFlush = true;
		bombBatchWorldPosition = worldPosition;
		hasBombBatchWorldPosition = true;
		MergeConditions(bombBatchConditions, conditions);
	}

	private static void MergeConditions(List<CharacterExpressionTriggerCondition> target, CharacterExpressionTriggerCondition source)
	{
		if (!target.Contains(source))
		{
			target.Add(source);
		}
	}

	private static void MergeConditions(List<CharacterExpressionTriggerCondition> target, IReadOnlyList<CharacterExpressionTriggerCondition> source)
	{
		for (int i = 0; i < source.Count; i++)
		{
			MergeConditions(target, source[i]);
		}
	}

	private void EvaluateConsecutiveElimination(int stepIndex)
	{
		if (consecutiveEliminationCount <= 0)
		{
			consecutiveEliminationCount = 1;
			return;
		}
		if (stepIndex - lastEliminationStepIndex == 1)
		{
			consecutiveEliminationCount++;
		}
		else
		{
			consecutiveEliminationCount = 1;
		}
		if (consecutiveEliminationCount >= 3)
		{
			matchedConditions.Add(CharacterExpressionTriggerCondition.ConsecutiveElimination);
			consecutiveEliminationCount = 0;
		}
	}

	private void EvaluateMultiRelease(int releasedPartCount)
	{
		if (releasedPartCount >= 2)
		{
			matchedConditions.Add(CharacterExpressionTriggerCondition.MultiRelease);
		}
	}

	private void EvaluateLongIntervalElimination()
	{
		if (!(lastEliminationRealtime <= 0f) && Time.realtimeSinceStartup - lastEliminationRealtime >= 15f)
		{
			matchedConditions.Add(CharacterExpressionTriggerCondition.LongIntervalElimination);
		}
	}

	private void UpdateEliminationTracking(int stepIndex)
	{
		lastEliminationStepIndex = stepIndex;
		lastEliminationRealtime = Time.realtimeSinceStartup;
	}

	private bool TryGetStepEncouragementWorldPosition(GameplayStepRecord step, out Vector3 worldPosition)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		worldPosition = Vector3.zero;
		if (step == null)
		{
			return false;
		}
		if (CharacterExpressionWorldAnchor.TryGetReleasedPartsCenterWorldPosition(base.Host.ObjectsService, step.ReleasedParts, out worldPosition))
		{
			return true;
		}
		return CharacterExpressionWorldAnchor.TryGetPartVisualCenterWorldPosition(base.Host.ObjectsService, step.PartId, out worldPosition);
	}

	private void MergePendingAcornCollectCondition()
	{
		if (pendingAcornCollectTrigger)
		{
			pendingAcornCollectTrigger = false;
			MergeConditions(matchedConditions, CharacterExpressionTriggerCondition.AcornCollect);
		}
	}

	private void TryPlayForStep(int stepIndex, Vector3 worldPosition, List<CharacterExpressionTriggerCondition> conditions, int effectiveFeedback)
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		if (stepIndex != lastEmojiStepIndex && TryPlayFromWorldPosition(worldPosition, conditions, effectiveFeedback, stepIndex))
		{
			lastEmojiStepIndex = stepIndex;
		}
	}

	private bool TryPlayFromWorldPosition(Vector3 worldPosition, List<CharacterExpressionTriggerCondition> conditions, int effectiveFeedback, int stepIndex = -1)
	{
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		if (effectiveFeedback <= 0 || conditions == null || conditions.Count == 0)
		{
			return false;
		}
		if (Time.frameCount == lastPlayFrame)
		{
			return false;
		}
		if (!CharacterExpressionEffectCatalog.TryPickEffect(effectiveFeedback, out var effectLocation))
		{
			return false;
		}
		lastPlayFrame = Time.frameCount;
		OnExpressionEncouragementTriggered?.Invoke(conditions, worldPosition, effectiveFeedback);
		if (!PresentationMode.UseTaskPresentation)
		{
			effectPlayer.Play(effectLocation, worldPosition);
		}
		lastEliminationRealtime = Time.realtimeSinceStartup;
		return true;
	}

	private static int ReadFeedbackValue()
	{
		return FunctionDataCenter.Get<CharacterExpressionFunction>().CharacterExpressionFeedback;
	}

	private static int ResolveEffectiveFeedback()
	{
		int num = ReadFeedbackValue();
		if (GMCharacterExpressionDebugState.ForceShowOnElimination)
		{
			return Mathf.Max(num, 1);
		}
		return num;
	}

	private static bool IsBombStep(GameplayStepRecord step)
	{
		IReadOnlyList<LegacyOperationSource> operationSources = step.OperationSources;
		for (int i = 0; i < operationSources.Count; i++)
		{
			if (operationSources[i] == LegacyOperationSource.Bomb)
			{
				return true;
			}
		}
		return false;
	}

	private void ResetRuntimeState()
	{
		consecutiveEliminationCount = 0;
		lastEliminationStepIndex = 0;
		lastEliminationRealtime = 0f;
		waitingBombBreakFlush = false;
		bombBatchConditions.Clear();
		hasBombBatchWorldPosition = false;
		matchedConditions.Clear();
		lastPlayFrame = -1;
		lastEmojiStepIndex = -1;
		pendingAcornCollectTrigger = false;
	}

	private static string FormatConditions(IReadOnlyList<CharacterExpressionTriggerCondition> conditions)
	{
		if (conditions == null || conditions.Count == 0)
		{
			return "无";
		}
		List<string> list = new List<string>(conditions.Count);
		for (int i = 0; i < conditions.Count; i++)
		{
			list.Add(DescribeCondition(conditions[i]));
		}
		return string.Join(" | ", list);
	}

	private static string DescribeCondition(CharacterExpressionTriggerCondition condition)
	{
		return condition switch
		{
			CharacterExpressionTriggerCondition.ConsecutiveElimination => "连续消除三次", 
			CharacterExpressionTriggerCondition.MultiRelease => "单次消除2个以上", 
			CharacterExpressionTriggerCondition.LongIntervalElimination => "距上次消除超15秒", 
			CharacterExpressionTriggerCondition.AcornCollect => "收集物触发", 
			CharacterExpressionTriggerCondition.GmForceShow => "GM必定出现", 
			_ => condition.ToString(), 
		};
	}
}
