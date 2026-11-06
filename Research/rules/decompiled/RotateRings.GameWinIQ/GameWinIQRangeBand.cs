using Example.Cfg;

namespace RotateRings.GameWinIQ;

public readonly struct GameWinIQRangeBand
{
	public GameWinIQRange Row { get; }

	public int MinInclusive { get; }

	public int MaxScore { get; }

	public bool MaxIsInclusive { get; }

	public GameWinIQRangeBand(GameWinIQRange row, int minInclusive, int maxScore, bool maxIsInclusive)
	{
		Row = row;
		MinInclusive = minInclusive;
		MaxScore = maxScore;
		MaxIsInclusive = maxIsInclusive;
	}

	public bool Contains(int score)
	{
		if (score < MinInclusive)
		{
			return false;
		}
		if (!MaxIsInclusive)
		{
			return score < MaxScore;
		}
		return score <= MaxScore;
	}
}
