namespace Game.Experiment;

public struct AdInterDecision
{
	public bool CanShow;

	public string BlockReason;

	public static AdInterDecision Allow()
	{
		return new AdInterDecision
		{
			CanShow = true
		};
	}

	public static AdInterDecision Block(string reason)
	{
		return new AdInterDecision
		{
			CanShow = false,
			BlockReason = reason
		};
	}
}
