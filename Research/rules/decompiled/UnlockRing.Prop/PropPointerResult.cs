namespace UnlockRing.Prop;

public readonly struct PropPointerResult
{
	public enum ResultType
	{
		PassThrough,
		BlockQuiet,
		Block
	}

	public static PropPointerResult PassThrough => new PropPointerResult(ResultType.PassThrough, suppressBlockedFeedback: false);

	public static PropPointerResult BlockQuiet => new PropPointerResult(ResultType.BlockQuiet, suppressBlockedFeedback: true);

	public static PropPointerResult Block => new PropPointerResult(ResultType.Block, suppressBlockedFeedback: true);

	public ResultType Type { get; }

	public bool SuppressBlockedFeedback { get; }

	public PropPointerResult(ResultType type, bool suppressBlockedFeedback)
	{
		Type = type;
		SuppressBlockedFeedback = suppressBlockedFeedback;
	}
}
