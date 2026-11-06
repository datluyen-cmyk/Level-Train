namespace UnlockRing;

public sealed class RingHintFailedEvent
{
	public string Reason { get; }

	public RingHintFailedEvent(string reason = null)
	{
		Reason = reason ?? string.Empty;
	}
}
