using RotateRings.GamePlay.Events;
using RotateRings.GamePlay.Objects;
using UnityEngine;

public class ColliderRoleMarker : MonoBehaviour
{
	[SerializeField]
	private RingDragOtherKind role;

	[SerializeField]
	private Component owner;

	private RingRelative ringRelative;

	private bool ringRelativeResolved;

	private FixedButterflyRelative fixedButterflyRelative;

	private bool fixedButterflyRelativeResolved;

	public RingDragOtherKind Role => role;

	public IGameplayObject Owner => owner as IGameplayObject;

	public RingRelative GetRingRelative()
	{
		if (ringRelativeResolved)
		{
			return ringRelative;
		}
		ringRelativeResolved = true;
		if (role == RingDragOtherKind.RingRelativeBody || role == RingDragOtherKind.RingRelativeWall)
		{
			ringRelative = ((Component)this).GetComponentInParent<RingRelative>();
		}
		return ringRelative;
	}

	public FixedButterflyRelative GetFixedButterflyRelative()
	{
		if (fixedButterflyRelativeResolved)
		{
			return fixedButterflyRelative;
		}
		fixedButterflyRelativeResolved = true;
		if (role == RingDragOtherKind.CatchGuideBody || role == RingDragOtherKind.CatchGuideWall)
		{
			fixedButterflyRelative = ((Component)this).GetComponentInParent<FixedButterflyRelative>();
		}
		return fixedButterflyRelative;
	}

	public void AssignRole(RingDragOtherKind kind)
	{
		role = kind;
	}

	public void AssignOwner(Component target)
	{
		owner = target;
	}
}
