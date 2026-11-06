namespace RotateRings.GameWinIQ;

public readonly struct GameWinIQSettlementSnapshot
{
	public static readonly GameWinIQSettlementSnapshot Empty = new GameWinIQSettlementSnapshot(_: false);

	public bool ShouldShowUi { get; }

	public int PreviousDisplayScore { get; }

	public int DisplayScore { get; }

	public int PreviousBaseScore { get; }

	public int NewBaseScore { get; }

	public int SessionPerformanceScore { get; }

	public bool IsPeakGenius { get; }

	public bool PlayGrowthAnimation { get; }

	public bool AppliedCrossDayDecay { get; }

	public int CrossDayDecayFromDisplay { get; }

	public int CrossDayDecayToDisplay { get; }

	public GameWinIQSettlementDetail Detail { get; }

	public GameWinIQSettlementSnapshot(bool shouldShowUi, int previousDisplayScore, int displayScore, int previousBaseScore, int newBaseScore, int sessionPerformanceScore, bool isPeakGenius, bool playGrowthAnimation, bool appliedCrossDayDecay, int crossDayDecayFromDisplay, int crossDayDecayToDisplay, GameWinIQSettlementDetail detail)
	{
		ShouldShowUi = shouldShowUi;
		PreviousDisplayScore = previousDisplayScore;
		DisplayScore = displayScore;
		PreviousBaseScore = previousBaseScore;
		NewBaseScore = newBaseScore;
		SessionPerformanceScore = sessionPerformanceScore;
		IsPeakGenius = isPeakGenius;
		PlayGrowthAnimation = playGrowthAnimation;
		AppliedCrossDayDecay = appliedCrossDayDecay;
		CrossDayDecayFromDisplay = crossDayDecayFromDisplay;
		CrossDayDecayToDisplay = crossDayDecayToDisplay;
		Detail = detail;
	}

	private GameWinIQSettlementSnapshot(bool _)
	{
		ShouldShowUi = false;
		PreviousDisplayScore = 0;
		DisplayScore = 0;
		PreviousBaseScore = 0;
		NewBaseScore = 0;
		SessionPerformanceScore = 0;
		IsPeakGenius = false;
		PlayGrowthAnimation = false;
		AppliedCrossDayDecay = false;
		CrossDayDecayFromDisplay = 0;
		CrossDayDecayToDisplay = 0;
		Detail = default(GameWinIQSettlementDetail);
	}
}
