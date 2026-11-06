using System;
using System.Collections.Generic;
using RotateRings.GamePlay.Events;
using UnityEngine;
using UnlockRing.Core.Domain;

namespace RotateRings.GamePlay.Services;

internal sealed class RingPassiveBlockCheck
{
	private const string TAG = "[GamePlay][RingLockPassive] ";

	private const float InterlockAngleToleranceDeg = 5f;

	private readonly RingPassiveLockDomain domain;

	private readonly Collider2D[] overlapBuffer = (Collider2D[])(object)new Collider2D[32];

	private readonly HashSet<int> tempIds = new HashSet<int>();

	internal RingPassiveBlockCheck(RingPassiveLockDomain domain)
	{
		this.domain = domain;
	}

	internal void OnRingPointerDown(RingObject ring)
	{
		_ = (Object)(object)ring == (Object)null;
	}

	internal bool ShouldIgnoreBlock(RingObject ring, Collider2D col, RingRelative ownerSource = null)
	{
		if (CanSkipBlockForSource(ring, col, ownerSource))
		{
			return true;
		}
		if (!IsInBinds(ring, col, ownerSource))
		{
			return CanIgnorePartnerWallInAngle(ring, col, ownerSource);
		}
		return true;
	}

	internal bool HasNewCollider(RingObject ring, RingRelative ownerSource, Collider2D[] colliders, int count)
	{
		if ((Object)(object)ring == (Object)null || colliders == null || count <= 0)
		{
			return false;
		}
		int num = Math.Min(count, overlapBuffer.Length);
		Array.Copy(colliders, overlapBuffer, num);
		if (!domain.TryGetStaticBindings(ring, out var _))
		{
			if (CountBlockingColliders(ring, ownerSource, overlapBuffer, num) > 0)
			{
				return true;
			}
			return false;
		}
		if (TryFindUnbindCollider(ring, ownerSource, num, out var _))
		{
			return true;
		}
		return false;
	}

	internal bool TryResolveBlocker(RingObject ring, RingRelative ownerSource, out Collider2D blocker)
	{
		blocker = null;
		if ((Object)(object)ring == (Object)null)
		{
			return false;
		}
		tempIds.Clear();
		int num = ring.CollectOverlapCollidersFromSource(ownerSource, overlapBuffer, tempIds);
		if (num <= 0)
		{
			return false;
		}
		if (!domain.TryGetStaticBindings(ring, out var _))
		{
			blocker = overlapBuffer[0];
			return (Object)(object)blocker != (Object)null;
		}
		return TryFindUnbindCollider(ring, ownerSource, num, out blocker);
	}

	private bool CanSkipBlockForSource(RingObject ring, Collider2D col, RingRelative ownerSource)
	{
		if ((Object)(object)ownerSource != (Object)null || !ring.IsOtherRelativeCollider(ring, col))
		{
			return false;
		}
		if (!HasRingRelativeSources(ring))
		{
			return false;
		}
		IReadOnlyList<RingRelative> ringRelatives = ring.RingRelatives;
		if (ringRelatives != null)
		{
			for (int i = 0; i < ringRelatives.Count; i++)
			{
				if (IsInBinds(ring, col, ringRelatives[i]))
				{
					return true;
				}
			}
		}
		if (RingColliderQuery.TryGetMarker(col, out var marker) && (marker.Role == RingDragOtherKind.RingRelativeWall || marker.Role == RingDragOtherKind.RingRelativeBody))
		{
			return false;
		}
		return true;
	}

	private int CountBlockingColliders(RingObject ring, RingRelative ownerSource, Collider2D[] buffer, int count)
	{
		int num = 0;
		for (int i = 0; i < count && i < buffer.Length; i++)
		{
			Collider2D val = buffer[i];
			if ((Object)(object)val != (Object)null && !ShouldIgnoreBlock(ring, val, ownerSource))
			{
				num++;
			}
		}
		return num;
	}

	private bool IsInBinds(RingObject ownerRing, Collider2D col, RingRelative ownerSource)
	{
		if ((Object)(object)ownerRing == (Object)null || (Object)(object)col == (Object)null)
		{
			return false;
		}
		if (!domain.TryGetStaticBindings(ownerRing, out var bindings) || !bindings.TryGetValue(((Object)col).GetInstanceID(), out var value))
		{
			return false;
		}
		if (!IsSourceCompatible(ownerRing, col, ownerSource, value))
		{
			return false;
		}
		return IsWithinAngle(value, col);
	}

	private bool IsSourceCompatible(RingObject ownerRing, Collider2D col, RingRelative ownerSource, LockPassiveColliderBinding binding)
	{
		int ownerSourceId = GetOwnerSourceId(ownerSource);
		if (binding.RingRelativeId == ownerSourceId)
		{
			return true;
		}
		if (!ownerRing.IsOtherWhiteListCollider(col))
		{
			return false;
		}
		if (binding.RingRelativeId == 0 && ownerSourceId != 0 && HasRingRelativeSources(ownerRing))
		{
			return true;
		}
		if (ownerSourceId == 0 && binding.RingRelativeId != 0)
		{
			return HasRingRelativeSources(ownerRing);
		}
		return false;
	}

	private bool TryFindUnbindCollider(RingObject ring, RingRelative ownerSource, int colliderCount, out Collider2D result)
	{
		for (int i = 0; i < colliderCount && i < overlapBuffer.Length; i++)
		{
			Collider2D val = overlapBuffer[i];
			if (!((Object)(object)val == (Object)null) && !CanSkipBlockForSource(ring, val, ownerSource) && !IsInBinds(ring, val, ownerSource) && !CanIgnorePartnerWallInAngle(ring, val, ownerSource))
			{
				result = val;
				return true;
			}
		}
		result = null;
		return false;
	}

	private bool CanIgnorePartnerWallInAngle(RingObject ownerRing, Collider2D col, RingRelative ownerSource = null)
	{
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)ownerRing == (Object)null || (Object)(object)col == (Object)null || !IsRingRelativeWallCollider(col))
		{
			return false;
		}
		if (!RingColliderQuery.TryGetMarker(col, out var marker) || !(marker.Owner is RingObject { IsSolved: false } ringObject) || !ringObject.TryGetPartId(out var partId))
		{
			return false;
		}
		if (!domain.TryGetStaticBindings(ownerRing, out var bindings))
		{
			return false;
		}
		foreach (LockPassiveColliderBinding value in bindings.Values)
		{
			LockPassiveColliderBinding current = value;
			if (((PartId)(ref current.PartId)).Equals(partId) && IsSourceCompatible(ownerRing, col, ownerSource, current) && IsPartnerInAngle(current))
			{
				return true;
			}
		}
		return false;
	}

	private bool IsWithinAngle(LockPassiveColliderBinding binding, Collider2D col)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		if (!domain.TryGetRing(binding.PartId, out var ring) || ring.IsSolved)
		{
			return false;
		}
		if (IsPartnerInAngle(binding, ring))
		{
			return true;
		}
		return !IsRingRelativeWallCollider(col);
	}

	private bool IsPartnerInAngle(LockPassiveColliderBinding binding)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		if (!domain.TryGetRing(binding.PartId, out var ring))
		{
			return false;
		}
		return IsPartnerInAngle(binding, ring);
	}

	private bool IsPartnerInAngle(LockPassiveColliderBinding binding, RingObject partner)
	{
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)partner == (Object)null || partner.IsSolved)
		{
			return false;
		}
		return Mathf.Abs(Mathf.DeltaAngle(((Component)partner).transform.localEulerAngles.z, binding.BaselinePartnerAngleZ)) <= 5f;
	}

	private bool IsRingRelativeWallCollider(Collider2D col)
	{
		if ((Object)(object)col != (Object)null && RingColliderQuery.TryGetMarker(col, out var marker))
		{
			return marker.Role == RingDragOtherKind.RingRelativeWall;
		}
		return false;
	}

	private int GetOwnerSourceId(RingRelative ownerSource)
	{
		if (!((Object)(object)ownerSource == (Object)null))
		{
			return ((Object)ownerSource).GetInstanceID();
		}
		return 0;
	}

	private bool HasRingRelativeSources(RingObject ring)
	{
		if ((Object)(object)ring == (Object)null)
		{
			return false;
		}
		IReadOnlyList<RingRelative> ringRelatives = ring.RingRelatives;
		if (ringRelatives != null)
		{
			return ringRelatives.Count > 0;
		}
		return false;
	}

	private string DescribeOwnerSource(RingRelative ownerSource)
	{
		if (!((Object)(object)ownerSource == (Object)null))
		{
			return ((Object)ownerSource).name;
		}
		return "MainBody";
	}
}
