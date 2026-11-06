using UnityEngine;

namespace RotateRings.GamePlay.Events;

public sealed class RingDragCollisionResetEvent
{
	public Component MovingRoot { get; }

	public RingDragCollisionResetEvent(Component movingRoot)
	{
		MovingRoot = movingRoot;
	}
}
