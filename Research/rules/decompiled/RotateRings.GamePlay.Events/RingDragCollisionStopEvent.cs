using UnityEngine;

namespace RotateRings.GamePlay.Events;

public sealed class RingDragCollisionStopEvent
{
	public Component MovingRoot { get; }

	public Collider2D OtherCollider { get; }

	public Vector2 PosWorld { get; }

	public RingDragMoveKind MoveKind { get; }

	public RingDragOtherKind OtherKind { get; }

	public IGameplayObject OtherOwner { get; }

	public float MoveDirection { get; }

	public RingDragCollisionStopEvent(Component movingRoot, Collider2D otherCollider, Vector2 posWorld, RingDragMoveKind moveKind, float moveDirection, RingDragOtherKind otherKind = RingDragOtherKind.Unknown, IGameplayObject otherOwner = null)
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		MovingRoot = movingRoot;
		OtherCollider = otherCollider;
		PosWorld = posWorld;
		MoveKind = moveKind;
		MoveDirection = moveDirection;
		OtherKind = otherKind;
		OtherOwner = otherOwner;
	}
}
