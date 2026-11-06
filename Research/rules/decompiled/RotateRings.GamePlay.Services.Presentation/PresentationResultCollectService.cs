using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using RotateRings.GamePlay.Events;
using RotateRings.GamePlay.Services.CharacterExpression;
using UnityEngine;
using UnlockRing;
using UnlockRing.Core.Domain;
using UnlockRing.Presentation;

namespace RotateRings.GamePlay.Services.Presentation;

public sealed class PresentationResultCollectService : GameplayServiceBase<GameplayManager>
{
	private readonly struct SolveRecord
	{
		public readonly PartId partId;

		public readonly GameplaySolveReason reason;

		public readonly PartId passiveByPartId;

		public readonly RingBreakResult solveResult;

		public SolveRecord(PartId partId, GameplaySolveReason reason, PartId passiveByPartId, RingBreakResult solveResult)
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			//IL_000f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0010: Unknown result type (might be due to invalid IL or missing references)
			this.partId = partId;
			this.reason = reason;
			this.passiveByPartId = passiveByPartId;
			this.solveResult = solveResult;
		}
	}

	private const string TAG = "[Presentation][ResultCollectService] ";

	private PresentationScheduler scheduler;

	private ResultDataCollector currentCollector;

	private bool isCollecting;

	private SolveCauseForest currentForest;

	private bool isPropOperation;

	private List<SolveRecord> solveRecords;

	private List<BombBoomResult> bombResults;

	public bool HasPresentationPlaying
	{
		get
		{
			if (scheduler != null)
			{
				return !scheduler.IsIdle;
			}
			return false;
		}
	}

	public PresentationResultCollectService(GameplayManager host)
		: base(host)
	{
		scheduler = new PresentationScheduler(base.DestroyToken);
		GameApp.Event.Listen<RingDragSettleResetEvent>((Action<RingDragSettleResetEvent>)OnOperationBegin);
		GameApp.Event.Listen<BombExplodeStartEvent>((Action<BombExplodeStartEvent>)OnBombExplodeStart);
		GameApp.Event.Listen<RingRemoveEffectEvent>((Action<RingRemoveEffectEvent>)OnRingRemoveEffect);
		base.Host.GetGameService<RotateRingOperationScoreFacade>().OnEncouragementEvaluated = OnComboEncouragementEvaluated;
		base.Host.GetGameService<CharacterExpressionEncourageService>().OnExpressionEncouragementTriggered = OnExpressionEncouragementTriggered;
	}

	public UniTask<bool> WaitAllIdleAsync(CancellationToken ct)
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		if (scheduler == null)
		{
			return UniTask.FromResult<bool>(true);
		}
		return scheduler.WaitAllIdleAsync(ct);
	}

	internal void FlushAndSubmit()
	{
		if (!isCollecting || currentCollector == null)
		{
			return;
		}
		isCollecting = false;
		IReadOnlyList<IPresentationResult> readOnlyList = currentCollector.CreateSnapshot();
		currentCollector.Clear();
		currentCollector = null;
		SolveCauseForest solveCauseForest = ((PresentationMode.Mode == PresentationModeKind.FlatTwoBatch) ? BuildCauseForestFlat() : BuildCauseForest());
		_ = solveRecords;
		_ = bombResults;
		_ = isPropOperation;
		currentForest = null;
		solveRecords = null;
		bombResults = null;
		isPropOperation = false;
		if (readOnlyList.Count != 0 || (solveCauseForest != null && solveCauseForest.Roots.Count != 0))
		{
			ResultDataCollector resultDataCollector = new ResultDataCollector();
			for (int i = 0; i < readOnlyList.Count; i++)
			{
				resultDataCollector.Collect(readOnlyList[i]);
			}
			IPresentationTask presentationTask = PresentationTaskFactory.CreateTask(resultDataCollector, base.Host, solveCauseForest);
			if (presentationTask != null)
			{
				scheduler?.Submit(presentationTask);
			}
		}
	}

	private SolveCauseForest BuildCauseForest()
	{
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_010b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0122: Unknown result type (might be due to invalid IL or missing references)
		//IL_0127: Unknown result type (might be due to invalid IL or missing references)
		//IL_0135: Unknown result type (might be due to invalid IL or missing references)
		//IL_013b: Unknown result type (might be due to invalid IL or missing references)
		//IL_019a: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_015a: Unknown result type (might be due to invalid IL or missing references)
		//IL_01aa: Unknown result type (might be due to invalid IL or missing references)
		if (solveRecords == null || solveRecords.Count == 0)
		{
			return null;
		}
		SolveCauseForest solveCauseForest = new SolveCauseForest();
		Dictionary<PartId, CauseNode> solveNodesByPartId = solveCauseForest.SolveNodesByPartId;
		for (int i = 0; i < solveRecords.Count; i++)
		{
			SolveRecord solveRecord = solveRecords[i];
			CauseNode value = CauseNode.ForSolve(solveRecord.solveResult, solveRecord.partId);
			solveNodesByPartId[solveRecord.partId] = value;
		}
		List<CauseNode> list = new List<CauseNode>(bombResults?.Count ?? 0);
		if (bombResults != null)
		{
			for (int j = 0; j < bombResults.Count; j++)
			{
				CauseNode item = CauseNode.ForBomb(bombResults[j]);
				solveCauseForest.Roots.Add(item);
				list.Add(item);
			}
		}
		CauseNode causeNode = null;
		if (isPropOperation)
		{
			causeNode = CauseNode.ForProp(LegacyOperationSource.Hammer);
			solveCauseForest.Roots.Add(causeNode);
		}
		for (int k = 0; k < solveRecords.Count; k++)
		{
			SolveRecord solveRecord2 = solveRecords[k];
			CauseNode item2 = solveNodesByPartId[solveRecord2.partId];
			GameplaySolveReason reason = solveRecord2.reason;
			PartId passiveByPartId = solveRecord2.passiveByPartId;
			bool flag = false;
			if (reason == GameplaySolveReason.PassivePointerUp)
			{
				CauseNode value2;
				if (((PartId)(ref passiveByPartId)).Equals(default(PartId)))
				{
					solveCauseForest.Roots.Add(item2);
					flag = true;
				}
				else if (solveNodesByPartId.TryGetValue(passiveByPartId, out value2))
				{
					value2.Children.Add(item2);
					flag = true;
				}
				else
				{
					solveCauseForest.Roots.Add(item2);
					flag = true;
				}
			}
			else if (reason.IsPassive())
			{
				if (!((PartId)(ref passiveByPartId)).Equals(default(PartId)) && solveNodesByPartId.TryGetValue(passiveByPartId, out var value3))
				{
					value3.Children.Add(item2);
					flag = true;
				}
			}
			else
			{
				switch (reason)
				{
				case GameplaySolveReason.Bomb:
					if (list.Count > 0)
					{
						list[0].Children.Add(item2);
						flag = true;
					}
					break;
				case GameplaySolveReason.Hammer:
					if (causeNode != null)
					{
						causeNode.Children.Add(item2);
						flag = true;
					}
					break;
				}
			}
			if (!flag)
			{
				solveCauseForest.Roots.Add(item2);
			}
		}
		return solveCauseForest;
	}

	private SolveCauseForest BuildCauseForestFlat()
	{
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_013a: Unknown result type (might be due to invalid IL or missing references)
		//IL_015c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0211: Unknown result type (might be due to invalid IL or missing references)
		//IL_0235: Unknown result type (might be due to invalid IL or missing references)
		//IL_023a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0240: Unknown result type (might be due to invalid IL or missing references)
		//IL_0246: Unknown result type (might be due to invalid IL or missing references)
		//IL_025e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0260: Unknown result type (might be due to invalid IL or missing references)
		//IL_029b: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_026c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0282: Unknown result type (might be due to invalid IL or missing references)
		//IL_0276: Unknown result type (might be due to invalid IL or missing references)
		//IL_028d: Unknown result type (might be due to invalid IL or missing references)
		//IL_028f: Unknown result type (might be due to invalid IL or missing references)
		if (solveRecords == null || solveRecords.Count == 0)
		{
			return null;
		}
		SolveCauseForest solveCauseForest = new SolveCauseForest();
		Dictionary<PartId, CauseNode> solveNodesByPartId = solveCauseForest.SolveNodesByPartId;
		Dictionary<PartId, PartId> dictionary = new Dictionary<PartId, PartId>(solveRecords.Count);
		for (int i = 0; i < solveRecords.Count; i++)
		{
			SolveRecord solveRecord = solveRecords[i];
			CauseNode value = CauseNode.ForSolve(solveRecord.solveResult, solveRecord.partId);
			solveNodesByPartId[solveRecord.partId] = value;
			dictionary[solveRecord.partId] = solveRecord.passiveByPartId;
		}
		List<CauseNode> list = new List<CauseNode>(bombResults?.Count ?? 0);
		if (bombResults != null)
		{
			for (int j = 0; j < bombResults.Count; j++)
			{
				CauseNode item = CauseNode.ForBomb(bombResults[j]);
				solveCauseForest.Roots.Add(item);
				list.Add(item);
			}
		}
		CauseNode causeNode = null;
		if (isPropOperation)
		{
			causeNode = CauseNode.ForProp(LegacyOperationSource.Hammer);
			solveCauseForest.Roots.Add(causeNode);
		}
		HashSet<PartId> hashSet = new HashSet<PartId>();
		for (int k = 0; k < solveRecords.Count; k++)
		{
			SolveRecord solveRecord2 = solveRecords[k];
			CauseNode item2 = solveNodesByPartId[solveRecord2.partId];
			GameplaySolveReason reason = solveRecord2.reason;
			if (reason.IsPassive())
			{
				continue;
			}
			hashSet.Add(solveRecord2.partId);
			bool flag = false;
			switch (reason)
			{
			case GameplaySolveReason.Bomb:
				if (list.Count > 0)
				{
					list[0].Children.Add(item2);
					flag = true;
				}
				break;
			case GameplaySolveReason.Hammer:
				if (causeNode != null)
				{
					causeNode.Children.Add(item2);
					flag = true;
				}
				break;
			}
			if (!flag)
			{
				solveCauseForest.Roots.Add(item2);
			}
		}
		bool flag2 = list.Count > 0 || causeNode != null;
		for (int l = 0; l < solveRecords.Count; l++)
		{
			SolveRecord solveRecord3 = solveRecords[l];
			if (!solveRecord3.reason.IsPassive())
			{
				continue;
			}
			CauseNode item3 = solveNodesByPartId[solveRecord3.partId];
			if (!flag2)
			{
				solveCauseForest.Roots.Add(item3);
				continue;
			}
			PartId passiveByPartId = solveRecord3.passiveByPartId;
			if (((PartId)(ref passiveByPartId)).Equals(default(PartId)))
			{
				solveCauseForest.Roots.Add(item3);
				continue;
			}
			PartId val = passiveByPartId;
			CauseNode causeNode2 = null;
			int num = 0;
			while (!((PartId)(ref val)).Equals(default(PartId)) && num < 64)
			{
				if (hashSet.Contains(val))
				{
					causeNode2 = solveNodesByPartId[val];
					break;
				}
				if (!dictionary.TryGetValue(val, out var value2))
				{
					break;
				}
				val = value2;
				num++;
			}
			if (causeNode2 != null)
			{
				causeNode2.Children.Add(item3);
			}
			else
			{
				solveCauseForest.Roots.Add(item3);
			}
		}
		return solveCauseForest;
	}

	private void OnOperationBegin(RingDragSettleResetEvent evt)
	{
		currentCollector?.Clear();
		currentCollector = new ResultDataCollector();
		solveRecords = new List<SolveRecord>(8);
		bombResults = null;
		isPropOperation = evt != null && evt.OperationSource == LegacyOperationSource.Hammer;
		isCollecting = true;
	}

	private void OnBombExplodeStart(BombExplodeStartEvent evt)
	{
		if (isCollecting && currentCollector != null && !((Object)(object)evt?.Bomb == (Object)null))
		{
			BombBoomResult bombBoomResult = new BombBoomResult(evt.Bomb);
			currentCollector.Collect(bombBoomResult);
			if (bombResults == null)
			{
				bombResults = new List<BombBoomResult>(2);
			}
			bombResults.Add(bombBoomResult);
		}
	}

	private void OnRingRemoveEffect(RingRemoveEffectEvent evt)
	{
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00da: Unknown result type (might be due to invalid IL or missing references)
		//IL_00df: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)evt?.PartGo == (Object)null)
		{
			return;
		}
		if (currentCollector == null)
		{
			_ = isCollecting;
			currentCollector = new ResultDataCollector();
			solveRecords = new List<SolveRecord>(8);
			isCollecting = true;
		}
		if (PresentationMode.UseTaskPresentation)
		{
			evt.Handled = true;
		}
		RingBreakResult ringBreakResult = new RingBreakResult(evt.SourcePart, evt.SourceMesh, evt.SourceTransform, evt.PartGo, evt.EffectParent, evt.SourcePart, evt.IsFromBomb);
		currentCollector.Collect(ringBreakResult);
		if (solveRecords == null)
		{
			return;
		}
		PartId partId = default(PartId);
		if (evt.SourcePart != null && evt.SourcePart.TryGetPartId(out partId))
		{
			GameplaySolveReason reason = GameplaySolveReason.None;
			PartId passiveByPartId = default(PartId);
			if (evt.SourcePart is IGameplaySolveReason gameplaySolveReason)
			{
				reason = gameplaySolveReason.SolveReason;
				passiveByPartId = gameplaySolveReason.PassiveByPartId;
			}
			solveRecords.Add(new SolveRecord(partId, reason, passiveByPartId, ringBreakResult));
		}
	}

	private void OnComboEncouragementEvaluated(GameplayOperationEvaluation evaluation, Vector3 worldPosition)
	{
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		if (isCollecting && currentCollector != null && evaluation != GameplayOperationEvaluation.None)
		{
			currentCollector.Collect(new ComboEncouragementResult(evaluation, worldPosition));
		}
	}

	private void OnExpressionEncouragementTriggered(IReadOnlyList<CharacterExpressionTriggerCondition> conditions, Vector3 worldPosition, int effectiveFeedback)
	{
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		if (isCollecting && currentCollector != null && conditions != null && conditions.Count != 0)
		{
			currentCollector.Collect(new CharacterExpressionEncouragementResult(conditions, worldPosition, effectiveFeedback));
		}
	}

	public override void ClearLevel()
	{
		isCollecting = false;
		currentCollector?.Clear();
		currentCollector = null;
		currentForest = null;
		solveRecords = null;
		bombResults = null;
		isPropOperation = false;
	}

	public override void Dispose()
	{
		if (!base.IsDisposed)
		{
			GameApp.Event.Unlisten<RingDragSettleResetEvent>((Action<RingDragSettleResetEvent>)OnOperationBegin);
			GameApp.Event.Unlisten<BombExplodeStartEvent>((Action<BombExplodeStartEvent>)OnBombExplodeStart);
			GameApp.Event.Unlisten<RingRemoveEffectEvent>((Action<RingRemoveEffectEvent>)OnRingRemoveEffect);
			base.Host.GetGameService<RotateRingOperationScoreFacade>().OnEncouragementEvaluated = null;
			base.Host.GetGameService<CharacterExpressionEncourageService>().OnExpressionEncouragementTriggered = null;
			isCollecting = false;
			currentCollector = null;
			currentForest = null;
			solveRecords = null;
			bombResults = null;
			isPropOperation = false;
			scheduler?.ResetAndCancel();
			scheduler?.Dispose();
			scheduler = null;
			base.Dispose();
		}
	}
}
