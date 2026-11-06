using System;
using System.Collections.Generic;
using RotateRings.GamePlay.Objects;
using UnityEngine;
using UnlockRing;
using UnlockRing.Core.Domain;

namespace RotateRings.GamePlay.Services;

internal sealed class RingPaletteLockGameplayStep
{
	private struct ColorLockScoreBatch
	{
		public RingObject KeyRing { get; }

		public PartId BatchPartId { get; }

		public RingObject[] ColorLockReleasedTargets { get; set; }

		public int RemainingFlySegments { get; set; }

		public bool Committed { get; set; }

		public bool DataLayerAndDeferReady { get; set; }

		public bool FlyPresentationCompleted { get; set; }

		public RingObject LastFlyTargetRing { get; set; }

		public ColorLockScoreBatch(RingObject keyRing, PartId batchPartId, int flySegmentCount)
		{
			//IL_0008: Unknown result type (might be due to invalid IL or missing references)
			//IL_0009: Unknown result type (might be due to invalid IL or missing references)
			KeyRing = keyRing;
			BatchPartId = batchPartId;
			RemainingFlySegments = flySegmentCount;
			ColorLockReleasedTargets = Array.Empty<RingObject>();
			Committed = false;
			DataLayerAndDeferReady = false;
			FlyPresentationCompleted = false;
			LastFlyTargetRing = null;
		}
	}

	private const string Tag = "[RingPaletteLockGameplayStep] ";

	private readonly RingPaletteLockService owner;

	private readonly Dictionary<PartId, ColorLockScoreBatch> batchByAnchorPartId = new Dictionary<PartId, ColorLockScoreBatch>();

	public RingPaletteLockGameplayStep(RingPaletteLockService owner)
	{
		this.owner = owner ?? throw new ArgumentNullException("owner");
	}

	public void ClearLevel()
	{
		batchByAnchorPartId.Clear();
	}

	public void BeginBatch(RingObject keyRing, IReadOnlyList<RingObject> lockedRings)
	{
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)keyRing == (Object)null || lockedRings == null || lockedRings.Count == 0)
		{
			return;
		}
		RingObjectsService objectsService = owner.HostInstance.ObjectsService;
		if (objectsService == null || !objectsService.TryGetPartId(keyRing, out var partId) || batchByAnchorPartId.ContainsKey(partId))
		{
			return;
		}
		int num = 0;
		for (int i = 0; i < lockedRings.Count; i++)
		{
			if ((Object)(object)lockedRings[i] != (Object)null)
			{
				num++;
			}
		}
		batchByAnchorPartId[partId] = new ColorLockScoreBatch(keyRing, partId, num);
	}

	public void SetBatchGateTargets(RingObject keyRing, IReadOnlyList<RingObject> colorLockReleasedTargets)
	{
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)keyRing == (Object)null || !TryGetBatch(keyRing, out var anchorPartId, out var batch))
		{
			return;
		}
		if (colorLockReleasedTargets == null || colorLockReleasedTargets.Count == 0)
		{
			batch.ColorLockReleasedTargets = Array.Empty<RingObject>();
			SaveBatch(anchorPartId, batch);
			return;
		}
		RingObject[] array = new RingObject[colorLockReleasedTargets.Count];
		int num = 0;
		for (int i = 0; i < colorLockReleasedTargets.Count; i++)
		{
			RingObject ringObject = colorLockReleasedTargets[i];
			if (!((Object)(object)ringObject == (Object)null) && !((Object)(object)ringObject == (Object)(object)keyRing))
			{
				array[num++] = ringObject;
			}
		}
		if (num == array.Length)
		{
			batch.ColorLockReleasedTargets = array;
		}
		else
		{
			RingObject[] array2 = new RingObject[num];
			Array.Copy(array, array2, num);
			batch.ColorLockReleasedTargets = array2;
		}
		SaveBatch(anchorPartId, batch);
	}

	public void MarkDataLayerReady(RingObject keyRing)
	{
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		if (!((Object)(object)keyRing == (Object)null) && TryGetBatch(keyRing, out var anchorPartId, out var batch))
		{
			batch.DataLayerAndDeferReady = true;
			SaveBatch(anchorPartId, batch);
			TryCommitIfReady(keyRing);
		}
	}

	public void OnFlySegmentCompleted(RingObject flyTargetRing)
	{
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		if (!((Object)(object)flyTargetRing == (Object)null) && flyTargetRing.TryGetColorLockBatchId(out var batchPartId) && batchByAnchorPartId.TryGetValue(batchPartId, out var value) && !value.Committed)
		{
			value.RemainingFlySegments--;
			if (value.RemainingFlySegments > 0)
			{
				SaveBatch(batchPartId, value);
				return;
			}
			value.FlyPresentationCompleted = true;
			value.LastFlyTargetRing = flyTargetRing;
			SaveBatch(batchPartId, value);
			TryCommitIfReady(value.KeyRing);
		}
	}

	private void TryCommitIfReady(RingObject keyRing)
	{
		if (!((Object)(object)keyRing == (Object)null) && TryGetBatch(keyRing, out var _, out var batch) && !batch.Committed && batch.DataLayerAndDeferReady && batch.FlyPresentationCompleted && batch.RemainingFlySegments <= 0)
		{
			CommitBatch(keyRing, batch.LastFlyTargetRing, batch);
		}
	}

	private void CommitBatch(RingObject keyRing, RingObject flyTargetRing, ColorLockScoreBatch batch)
	{
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)keyRing == (Object)null || (Object)(object)flyTargetRing == (Object)null || batch.Committed || batch.RemainingFlySegments > 0)
		{
			return;
		}
		RingObjectsService objectsService = owner.HostInstance.ObjectsService;
		if (objectsService == null)
		{
			return;
		}
		if (!HasSolvedGateRing(batch, objectsService))
		{
			AbandonBatch(batch);
			return;
		}
		if (!keyRing.IsSolved)
		{
			AbandonBatch(batch);
			return;
		}
		List<GameplayReleasedPart> list = CollectBatchScoreParts(batch.BatchPartId, objectsService);
		if (list.Count == 0)
		{
			AbandonBatch(batch);
			return;
		}
		batch.Committed = true;
		SaveBatch(batch.BatchPartId, batch);
		owner.HostInstance.CompleteLegacyOperation(flyTargetRing, LegacyOperationSource.ColorLock, list);
	}

	private void AbandonBatch(ColorLockScoreBatch batch)
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		if (!batch.Committed)
		{
			batch.Committed = true;
			SaveBatch(batch.BatchPartId, batch);
		}
	}

	private void SaveBatch(PartId anchorPartId, ColorLockScoreBatch batch)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		batchByAnchorPartId[anchorPartId] = batch;
	}

	private bool TryGetBatch(RingObject keyRing, out PartId anchorPartId, out ColorLockScoreBatch batch)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		anchorPartId = default(PartId);
		batch = default(ColorLockScoreBatch);
		if ((Object)(object)keyRing == (Object)null)
		{
			return false;
		}
		RingObjectsService objectsService = owner.HostInstance.ObjectsService;
		if (objectsService == null || !objectsService.TryGetPartId(keyRing, out var partId))
		{
			return false;
		}
		anchorPartId = partId;
		return batchByAnchorPartId.TryGetValue(partId, out batch);
	}

	private static List<GameplayReleasedPart> CollectBatchScoreParts(PartId batchPartId, RingObjectsService objectsService)
	{
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		List<GameplayReleasedPart> list = new List<GameplayReleasedPart>(4);
		IReadOnlyList<IGameplayObject> gameplayObjects = objectsService.GameplayObjects;
		for (int i = 0; i < gameplayObjects.Count; i++)
		{
			IGameplayObject gameplayObject = gameplayObjects[i];
			if (gameplayObject != null && !(gameplayObject is RingPaletteLockGapObject) && gameplayObject.IsSolved && gameplayObject is IThreadingScoreProvider && gameplayObject is IGameplaySolveReason gameplaySolveReason && objectsService.TryGetPartId(gameplayObject, out var partId))
			{
				PartId stepBatchPartId = gameplaySolveReason.StepBatchPartId;
				if (((PartId)(ref stepBatchPartId)).Equals(batchPartId) && gameplaySolveReason.SolveReason.IsColorLockBatchStepScorable())
				{
					TryAppendReleasedPart(list, partId, (IThreadingScoreProvider)gameplayObject, gameplaySolveReason.SolveReason);
				}
			}
		}
		return list;
	}

	private static void TryAppendReleasedPart(List<GameplayReleasedPart> releasedParts, PartId partId, IThreadingScoreProvider scoreProvider, GameplaySolveReason solveReason)
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		for (int i = 0; i < releasedParts.Count; i++)
		{
			PartId partId2 = releasedParts[i].PartId;
			if (((PartId)(ref partId2)).Equals(partId))
			{
				return;
			}
		}
		GameplayPartData partData = scoreProvider.PartData;
		int num = 0;
		if (partData.LockedBy != null)
		{
			num += partData.LockedBy.Count;
		}
		if (partData.Locks != null)
		{
			num += partData.Locks.Count;
		}
		releasedParts.Add(new GameplayReleasedPart(partId, solveReason.ToReleasedPartReason(), num));
	}

	private static bool HasSolvedGateRing(ColorLockScoreBatch batch, RingObjectsService objectsService)
	{
		if (batch.ColorLockReleasedTargets == null || objectsService == null)
		{
			return false;
		}
		for (int i = 0; i < batch.ColorLockReleasedTargets.Length; i++)
		{
			RingObject ringObject = batch.ColorLockReleasedTargets[i];
			if ((Object)(object)ringObject != (Object)null && ringObject.IsSolved && objectsService.TryGetPartId(ringObject, out var _))
			{
				return true;
			}
		}
		return false;
	}
}
