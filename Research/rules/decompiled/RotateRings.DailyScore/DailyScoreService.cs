namespace RotateRings.DailyScore;

public sealed class DailyScoreService
{
	private static readonly DailyScoreService instance = new DailyScoreService();

	public static DailyScoreService Instance => instance;

	public DailyScoreSettlementSnapshot RecordOnLevelWin(int settlementScore)
	{
		return GetProxy()?.RecordOnLevelWin(settlementScore) ?? DailyScoreSettlementSnapshot.Empty;
	}

	public DailyScoreSettlementSnapshot GetLastSettlementSnapshot()
	{
		return GetProxy()?.GetLastSettlementSnapshot() ?? DailyScoreSettlementSnapshot.Empty;
	}

	private static DailyScoreProxy GetProxy()
	{
		return GameApp.Proxy.Get<DailyScoreProxy>();
	}
}
