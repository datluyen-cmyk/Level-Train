using System;
using System.Collections.Generic;
using RotateRings.Common;
using RotateRings.Services;
using UnityEngine;
using UnlockRing.Core.Domain;

namespace RotateRings.GamePlay.Services;

internal sealed class RingPaletteLockFlyGate
{
	private readonly RingPaletteLockService owner;

	private readonly HashSet<RingObject> pendingRings = new HashSet<RingObject>();

	private readonly Dictionary<RingObject, RingObject> breakGateByDependent = new Dictionary<RingObject, RingObject>();

	public bool HasActivePresentation => pendingRings.Count > 0;

	public RingPaletteLockFlyGate(RingPaletteLockService owner)
	{
		this.owner = owner ?? throw new ArgumentNullException("owner");
	}

	public void ClearLevel()
	{
		pendingRings.Clear();
		breakGateByDependent.Clear();
	}

	public bool IsPending(RingObject ring)
	{
		if ((Object)(object)ring != (Object)null)
		{
			return pendingRings.Contains(ring);
		}
		return false;
	}

	public bool TryGetBreakGate(RingObject dependentRing, out RingObject gateRing)
	{
		gateRing = null;
		if ((Object)(object)dependentRing != (Object)null)
		{
			return breakGateByDependent.TryGetValue(dependentRing, out gateRing);
		}
		return false;
	}

	internal void MarkPending(RingObject ring)
	{
		if (!((Object)(object)ring == (Object)null))
		{
			pendingRings.Add(ring);
			owner.HostInstance.RefreshRingDragAreaLockZ();
		}
	}

	internal void ClearAllPending()
	{
		if (pendingRings.Count != 0)
		{
			pendingRings.Clear();
			owner.HostInstance.RefreshRingDragAreaLockZ();
		}
	}

	internal void RegisterBreakGateForBoundRings(RingObject flyPendingRing)
	{
		if (!((Object)(object)flyPendingRing == (Object)null) && IsPending(flyPendingRing))
		{
			RingObjectsService objectsService = owner.HostInstance.ObjectsService;
			if (objectsService != null)
			{
				RegisterBreakGateFromLinks(flyPendingRing.PartData.Locks, flyPendingRing, objectsService);
				RegisterBreakGateFromLinks(flyPendingRing.PartData.LockedBy, flyPendingRing, objectsService);
			}
		}
	}

	internal void CompleteUnlockForRing(RingObject lockedRing)
	{
		if (!((Object)(object)lockedRing == (Object)null))
		{
			pendingRings.Remove(lockedRing);
			owner.HostInstance.RefreshRingDragAreaLockZ();
			AudioService.PlayUiSound(AudioKeys.ColorLockUnlock.Value);
			owner.GameplayStep.OnFlySegmentCompleted(lockedRing);
			if (owner.RemainingLocks.Get(lockedRing) <= 0)
			{
				owner.ColorRelease.TryFinishUnsolvedAfterFly(lockedRing);
				owner.KeySolve.OnDeferredKeyRingFlyUnlocked(lockedRing);
				owner.KeySolve.TryDismissKeyUiWhenAllLocksSolved(lockedRing);
			}
		}
	}

	internal bool TryGetLockHeartWorldPosition(RingObject host, out Vector3 worldPosition)
	{
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		if (owner.Gaps.TryGetGapForHost(host, out var gap) && (Object)(object)gap != (Object)null)
		{
			worldPosition = gap.GetLockHeartWorldPosition();
			return true;
		}
		worldPosition = (((Object)(object)host != (Object)null) ? ((Component)host).transform.position : Vector3.zero);
		return false;
	}

	private void RegisterBreakGateFromLinks(List<PartLinkEntry> links, RingObject gateRing, RingObjectsService objects)
	{
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		if (links == null)
		{
			return;
		}
		for (int i = 0; i < links.Count; i++)
		{
			string physicPartId = links[i].PhysicPartId;
			if (!string.IsNullOrEmpty(physicPartId) && objects.TryGetPartObject(new PartId(physicPartId), out var gameplayObject) && gameplayObject is RingObject ringObject && !((Object)(object)ringObject == (Object)(object)gateRing) && !ringObject.IsSolved && !IsPending(ringObject))
			{
				breakGateByDependent[ringObject] = gateRing;
			}
		}
	}
}
