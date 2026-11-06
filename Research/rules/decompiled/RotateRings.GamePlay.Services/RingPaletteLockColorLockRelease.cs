using System;
using System.Collections.Generic;
using UnityEngine;
using UnlockRing.Core.Domain;

namespace RotateRings.GamePlay.Services;

internal sealed class RingPaletteLockColorLockRelease
{
	private readonly RingPaletteLockService owner;

	public RingPaletteLockColorLockRelease(RingPaletteLockService owner)
	{
		this.owner = owner ?? throw new ArgumentNullException("owner");
	}

	internal void HandleRingPreSolve(RingObject ring, bool isFlyPending)
	{
		if (!((Object)(object)ring == (Object)null || isFlyPending))
		{
			if (owner.HasColorLock(ring))
			{
				ApplyReleaseState(ring);
			}
			owner.Gaps.TrySolveGapForRing(ring);
		}
	}

	internal bool ApplyReleaseState(RingObject lockedRing)
	{
		if ((Object)(object)lockedRing == (Object)null || lockedRing.IsSolved || !owner.HasColorLock(lockedRing))
		{
			return false;
		}
		owner.ClearColorLock(lockedRing);
		GameplayPartData partData = lockedRing.PartData;
		partData.IsPaletteLock = false;
		lockedRing.PartData = partData;
		owner.HostInstance.LockPassive?.ReconcilePassiveLocksAfterFeatureReleased(lockedRing);
		owner.HostInstance.LockActive?.PruneActiveLocksForLockedRing(lockedRing);
		owner.HostInstance.RefreshRingDragAreaLockZ();
		return true;
	}

	internal void TryFinishUnsolvedAfterFly(RingObject lockedRing)
	{
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		if (!((Object)(object)lockedRing == (Object)null) && !lockedRing.IsSolved)
		{
			lockedRing.TryGetColorLockBatchId(out var batchPartId);
			if (owner.HasColorLock(lockedRing))
			{
				ApplyReleaseState(lockedRing);
			}
			ReleaseAndGap(lockedRing, batchPartId, releaseColorLock: false);
			TrySolveCascade(lockedRing);
		}
	}

	internal void ReleaseAndGap(RingObject ring, PartId stepBatchPartId, bool releaseColorLock = true)
	{
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		if (!((Object)(object)ring == (Object)null))
		{
			if (releaseColorLock)
			{
				ApplyReleaseState(ring);
			}
			if (owner.Gaps.TryGetGapForHost(ring, out var _))
			{
				owner.Gaps.TrySolveGapForRing(ring, null, GameplaySolveReasonContext.ColorLock(stepBatchPartId));
			}
		}
	}

	internal void TrySolveCascade(RingObject lockedRing, PartId stepBatchPartId = default(PartId))
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_00be: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e4: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)lockedRing == (Object)null || lockedRing.IsSolved)
		{
			return;
		}
		if (((PartId)(ref stepBatchPartId)).Equals(default(PartId)))
		{
			lockedRing.TryGetColorLockBatchId(out stepBatchPartId);
		}
		RingObjectSolveService solveService = owner.HostInstance.SolveService;
		RingObjectsService objectsService = owner.HostInstance.ObjectsService;
		if (solveService == null || objectsService == null)
		{
			return;
		}
		objectsService.TryGetPartId(lockedRing, out var partId);
		GameplaySolveReasonContext releaseContext = GameplaySolveReasonContext.ColorLock(stepBatchPartId);
		if (solveService.CanSolve(lockedRing))
		{
			solveService.SolveObject(lockedRing, null, releaseContext);
		}
		List<PartLinkEntry> lockedBy = lockedRing.PartData.LockedBy;
		if (lockedBy == null || lockedBy.Count == 0)
		{
			return;
		}
		for (int i = 0; i < lockedBy.Count; i++)
		{
			string physicPartId = lockedBy[i].PhysicPartId;
			if (!string.IsNullOrEmpty(physicPartId) && objectsService.TryGetPartObject(new PartId(physicPartId), out var gameplayObject) && !gameplayObject.IsSolved && solveService.CanSolve(gameplayObject))
			{
				solveService.SolveObject(gameplayObject, null, GameplaySolveReasonContext.PassiveColorLock(partId, stepBatchPartId));
			}
		}
	}
}
