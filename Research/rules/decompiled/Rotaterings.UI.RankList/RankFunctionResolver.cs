using RotateRings.Model;
using XingYun.Experiment;

namespace Rotaterings.UI.RankList;

public static class RankFunctionResolver
{
	private const int DefaultRankSettleSkip = 0;

	private const int DefaultBoxExperimentOff = 0;

	private const int DefaultRankOpenLevel = 11;

	private const int DefaultRankRewardConfig = 0;

	private const int DefaultRankOpenInterval = 0;

	private const int RankOpenIntervalCooldown = 0;

	private const int RankOpenIntervalAfterVictory = 1;

	private const int RankOpenIntervalAfterScore = 2;

	private const float DefaultRankPeriodHours = 24f;

	private const bool DefaultRankTopAni = false;

	public static int GetRankOpenLevel()
	{
		int rankOpenLevelLocked = GameApp.Save.GetOrCreate<PlayerModelData>().RankOpenLevelLocked;
		if (rankOpenLevelLocked > 0)
		{
			return rankOpenLevelLocked;
		}
		return NormalizeRankOpenLevel(FunctionDataCenter.Get<RankFunction>()?.RankOpenLevel ?? 11);
	}

	public static int GetRankOpenInterval()
	{
		return NormalizeRankOpenInterval(FunctionDataCenter.Get<RankFunction>()?.RankOpenInterval ?? 0);
	}

	public static bool ShouldPlayBottomAnimationOnSettleOpen()
	{
		return GetRankSettleSkip() == 1;
	}

	public static bool ShouldShowBoxOutline()
	{
		return GetBoxOutline() == 1;
	}

	public static bool ShouldShowBoxLight()
	{
		return GetBoxLight() == 1;
	}

	public static int GetBoxOutline()
	{
		return NormalizeBoxExperimentFlag(FunctionDataCenter.Get<RankFunction>()?.BoxOutline ?? 0);
	}

	public static int GetBoxLight()
	{
		return NormalizeBoxExperimentFlag(FunctionDataCenter.Get<RankFunction>()?.BoxLight ?? 0);
	}

	public static int GetRankSettleSkip()
	{
		return NormalizeRankSettleSkip(FunctionDataCenter.Get<RankFunction>()?.RankSettleSkip ?? 0);
	}

	public static int GetRankPlayerName()
	{
		return FunctionDataCenter.Get<RankFunction>()?.RankPlayerName ?? 0;
	}

	public static int GetRankRobotNameConfig()
	{
		return NormalizeRankRobotNameConfig(FunctionDataCenter.Get<RankFunction>()?.RankRobotNameConfig ?? 0);
	}

	public static int GetRankRewardConfig()
	{
		return NormalizeRankRewardConfig(FunctionDataCenter.Get<RankFunction>()?.RankRewardConfig ?? 0);
	}

	public static int GetRewardRankLimitByConfig(int rewardConfig)
	{
		switch (rewardConfig)
		{
		case 1:
		case 3:
			return 6;
		case 2:
		case 4:
			return 8;
		default:
			return 3;
		}
	}

	public static float GetRankPeriodHours()
	{
		return NormalizeRankPeriodHours(FunctionDataCenter.Get<RankFunction>()?.RankPeriodHours ?? 24f);
	}

	public static int GetRankPeriodSeconds()
	{
		return (int)(GetRankPeriodHours() * 3600f);
	}

	public static bool ShouldPlayTopRankAnimation()
	{
		return FunctionDataCenter.Get<RankFunction>()?.RankTopAni ?? false;
	}

	private static float NormalizeRankPeriodHours(float value)
	{
		if (!(value > 0f))
		{
			return 24f;
		}
		return value;
	}

	public static int GetRankRewardClaim()
	{
		return FunctionDataCenter.Get<RankFunction>()?.RankRewardClaim ?? 0;
	}

	private static int NormalizeRankRobotNameConfig(int value)
	{
		if (value < 0 || value > 3)
		{
			return 0;
		}
		return value;
	}

	private static int NormalizeRankRewardConfig(int value)
	{
		if (value < 0 || value > 4)
		{
			return 0;
		}
		return value;
	}

	private static int NormalizeRankSettleSkip(int value)
	{
		if (value != 1)
		{
			return 0;
		}
		return 1;
	}

	private static int NormalizeBoxExperimentFlag(int value)
	{
		if (value != 1)
		{
			return 0;
		}
		return 1;
	}

	private static int NormalizeRankOpenLevel(int value)
	{
		if (value <= 0)
		{
			return 11;
		}
		return value;
	}

	private static int NormalizeRankOpenInterval(int value)
	{
		if (value < 0 || value > 2)
		{
			return 0;
		}
		return value;
	}
}
