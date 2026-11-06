namespace UnlockRing.Solver;

public sealed class DeadEndAnalysis
{
	public bool IsDeadEnd { get; }

	public string Reason { get; }

	public ReliableConflictSet? ReliableConflict { get; }

	public DeadEndAnalysis(bool isDeadEnd, string reason = "", ReliableConflictSet? reliableConflict = null)
	{
		IsDeadEnd = isDeadEnd;
		Reason = reason ?? string.Empty;
		ReliableConflict = reliableConflict;
	}
}
