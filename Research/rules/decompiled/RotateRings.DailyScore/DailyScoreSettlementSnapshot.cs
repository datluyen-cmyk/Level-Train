namespace RotateRings.DailyScore;

public readonly struct DailyScoreSettlementSnapshot
{
	public bool ShouldShowBestUi { get; init; }

	public int ShowMode { get; init; }

	public static DailyScoreSettlementSnapshot Empty => new DailyScoreSettlementSnapshot
	{
		ShouldShowBestUi = false,
		ShowMode = 0
	};
}
