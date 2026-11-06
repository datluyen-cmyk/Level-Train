namespace RotateRings.Ad.Strategy;

public readonly struct InterAdCheckResult
{
	public bool Allowed { get; }

	public string BlockReason { get; }

	public static InterAdCheckResult Allow { get; } = new InterAdCheckResult(allowed: true, null);

	private InterAdCheckResult(bool allowed, string blockReason)
	{
		Allowed = allowed;
		BlockReason = blockReason;
	}

	public static InterAdCheckResult Block(string reason)
	{
		return new InterAdCheckResult(allowed: false, reason);
	}
}
