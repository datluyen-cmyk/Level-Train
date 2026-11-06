namespace RotateRings.GamePlay.Events;

public class RingPointerUpEvent
{
	public RingObject Ring { get; }

	public RingPointerUpEvent(RingObject ring)
	{
		Ring = ring;
	}
}
