using UnityEngine;

namespace RotateRings.GamePlay.Events;

public sealed class RingDragCollisionEnterEvent
{
	public Component MovingRoot { get; }

	public Collider2D OtherCollider { get; }

	public Vector2 PosWorld { get; }

	public RingDragMoveKind MoveKind { get; }

	public RingDragOtherKind OtherKind { get; }

	public IGameplayObject OtherOwner { get; }

	public bool IsNewEnter { get; }

	public RingDragCollisionEnterEvent(Component movingRoot, Collider2D otherCollider, Vector2 posWorld, RingDragMoveKind moveKind, bool isNewEnter = true, RingDragOtherKind otherKind = RingDragOtherKind.Unknown, IGameplayObject otherOwner = null)
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		MovingRoot = movingRoot;
		OtherCollider = otherCollider;
		PosWorld = posWorld;
		MoveKind = moveKind;
		IsNewEnter = isNewEnter;
		OtherKind = otherKind;
		OtherOwner = otherOwner;
	}
}
