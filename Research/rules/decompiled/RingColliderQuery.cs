using System.Collections.Generic;
using RotateRings.GamePlay.Events;
using RotateRings.GamePlay.Objects;
using UnityEngine;

public static class RingColliderQuery
{
	private readonly struct OwnerEntry
	{
		public readonly Collider2D ColliderRef;

		public readonly ColliderRoleMarker Marker;

		public readonly IGameplayObject Owner;

		public OwnerEntry(Collider2D colliderRef, ColliderRoleMarker marker, IGameplayObject owner)
		{
			ColliderRef = colliderRef;
			Marker = marker;
			Owner = owner;
		}
	}

	private static readonly Dictionary<int, OwnerEntry> entries = new Dictionary<int, OwnerEntry>();

	public static bool TryGetRingObject(Collider2D collider, out RingObject ring)
	{
		IGameplayObject owner = Resolve(collider).Owner;
		ring = owner as RingObject;
		if ((Object)(object)ring != (Object)null)
		{
			return true;
		}
		if (owner is RingPaletteLockGapObject ringPaletteLockGapObject && (Object)(object)ringPaletteLockGapObject.HostRing != (Object)null && !ringPaletteLockGapObject.IsSolved)
		{
			ring = ringPaletteLockGapObject.HostRing;
			return true;
		}
		ring = null;
		return false;
	}

	public static bool TryGetPullObject(Collider2D collider, out PullObject pull)
	{
		pull = Resolve(collider).Owner as PullObject;
		return (Object)(object)pull != (Object)null;
	}

	public static bool TryGetFixedObject(Collider2D collider, out FixedObject fixedObject)
	{
		fixedObject = Resolve(collider).Owner as FixedObject;
		return (Object)(object)fixedObject != (Object)null;
	}

	public static bool TryGetMarker(Collider2D collider, out ColliderRoleMarker marker)
	{
		marker = Resolve(collider).Marker;
		return (Object)(object)marker != (Object)null;
	}

	public static bool IsRingRelativeBodyCollider(Collider2D collider)
	{
		if ((Object)(object)collider != (Object)null && TryGetMarker(collider, out var marker))
		{
			return marker.Role == RingDragOtherKind.RingRelativeBody;
		}
		return false;
	}

	public static bool IsCatchGuideBodyCollider(Collider2D collider)
	{
		if ((Object)(object)collider != (Object)null && TryGetMarker(collider, out var marker))
		{
			return marker.Role == RingDragOtherKind.CatchGuideBody;
		}
		return false;
	}

	public static void Clear()
	{
		entries.Clear();
	}

	private static OwnerEntry Resolve(Collider2D collider)
	{
		if ((Object)(object)collider == (Object)null)
		{
			return default(OwnerEntry);
		}
		int instanceID = ((Object)collider).GetInstanceID();
		if (entries.TryGetValue(instanceID, out var value))
		{
			if ((Object)(object)value.ColliderRef != (Object)null)
			{
				return value;
			}
			entries.Remove(instanceID);
		}
		ColliderRoleMarker component = ((Component)collider).GetComponent<ColliderRoleMarker>();
		IGameplayObject gameplayObject = (((Object)(object)component != (Object)null) ? component.Owner : null);
		if (gameplayObject == null)
		{
			gameplayObject = ((Component)collider).GetComponentInParent<IGameplayObject>();
		}
		value = new OwnerEntry(collider, component, gameplayObject);
		entries[instanceID] = value;
		return value;
	}
}
