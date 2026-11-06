using RotateRings.Ad.Strategy.Interstitial;
using RotateRings.Common;

namespace RotateRings.Ad;

public static class ARRExperiment
{
	public const int MinTakeOverGroup = 1;

	public const int MaxTakeOverGroup = 4;

	public const int IncrementalGroup = 5;

	public const int RewardWatchedSkipPostGroup = 1;

	public const int GameDurationExemptGroup = 2;

	public const int RewardTimedWindowGroup = 4;

	public const int NonPreinstallExemptLevel = 25;

	public const float GameDurationExemptSeconds = 200f;

	public const int FirstInterstitialLevelNonPreinstall = 10;

	public const int FirstInterstitialLevelPreinstall = 5;

	private static bool hasLoggedExclusion;

	private static int lastLoggedGroup;

	private static bool lastLoggedExcluded;

	public static int GetGroup()
	{
		int configuredGroup = GetConfiguredGroup();
		if (configuredGroup == 0)
		{
			return 0;
		}
		bool flag = GpPreinstallFirstAdTimePolicy.IsUserInExperiment();
		LogExclusionIfChanged(configuredGroup, flag);
		if (!flag)
		{
			return configuredGroup;
		}
		return 0;
	}

	public static int GetConfiguredGroup()
	{
		return FeatureConfig.InterstitialFunction?.AccelerateRevenueRecovery ?? 0;
	}

	private static void LogExclusionIfChanged(int configuredGroup, bool excludedByGp)
	{
		if (!hasLoggedExclusion || configuredGroup != lastLoggedGroup || excludedByGp != lastLoggedExcluded)
		{
			hasLoggedExclusion = true;
			lastLoggedGroup = configuredGroup;
			lastLoggedExcluded = excludedByGp;
		}
	}

	public static bool IsTakeOverGroup(int group)
	{
		if (group >= 1)
		{
			return group <= 4;
		}
		return false;
	}

	public static bool IsTakeOverActive()
	{
		return IsTakeOverGroup(GetGroup());
	}

	public static bool ShouldRemoveFrontAd()
	{
		return IsTakeOverActive();
	}

	public static bool ShouldExemptByChannel(bool isFrontAd, int mainLevel, int completedLevel, int installDays)
	{
		if (!IsTakeOverGroup(GetGroup()))
		{
			return false;
		}
		int num = (isFrontAd ? mainLevel : completedLevel);
		bool num2 = PreinstallChannelHelper.IsCurrentUserPreinstall();
		string text = null;
		if (!num2)
		{
			if (installDays == 1)
			{
				text = "非预装安装第1天";
			}
			else if (num <= 25)
			{
				text = $"非预装生涯前{25}关免插屏";
			}
		}
		else if (AdBridgeHelper.IsFirstColdLaunch())
		{
			text = "预装首个session";
		}
		if (text == null)
		{
			return false;
		}
		return true;
	}

	public static bool ShouldExemptByGameDuration()
	{
		if (GetGroup() != 2)
		{
			return false;
		}
		return AdBridgeHelper.GetEffectiveColdLaunchValidDurationSeconds() < 200f;
	}

	public static int GetRewardExemptSeconds(int installDays)
	{
		int num = ((installDays < 1) ? 1 : installDays);
		if (num <= 2)
		{
			return 70;
		}
		if (num <= 5)
		{
			return 60;
		}
		if (num <= 10)
		{
			return 50;
		}
		if (num <= 14)
		{
			return 40;
		}
		return 30;
	}

	public static float GetCdMultiplier()
	{
		if (GetGroup() != 5 || InstallCampaignPolicy.IsTakeOverActive())
		{
			return 1f;
		}
		return GetCdMultiplier(AdBridgeHelper.GetInstallDays());
	}

	private static float GetCdMultiplier(int installDays)
	{
		int num = ((installDays < 1) ? 1 : installDays);
		if (num <= 2)
		{
			return 1.1f;
		}
		if (num <= 5)
		{
			return 1f;
		}
		if (num <= 10)
		{
			return 0.9f;
		}
		if (num <= 14)
		{
			return 0.8f;
		}
		return 0.7f;
	}

	public static int GetFirstInterstitialLevel()
	{
		if (!PreinstallChannelHelper.IsCurrentUserPreinstall())
		{
			return 10;
		}
		return 5;
	}

	public static string BuildDebugSummary()
	{
		int num = GetGroup();
		string text = $"配置组={GetConfiguredGroup()} GP互斥={GpPreinstallFirstAdTimePolicy.IsUserInExperiment()} 有效组={num} 预装={PreinstallChannelHelper.IsCurrentUserPreinstall()} 安装天={AdBridgeHelper.GetInstallDays()} " + $"冷启={AdBridgeHelper.GetColdLaunchCount()} 接管={IsTakeOverActive()}";
		if (num == 2)
		{
			text = text + $" 冷启真实时长={AdBridgeHelper.GetEffectiveColdLaunchValidDurationSeconds():F1}s" + $" 豁免阈值={200f:F0}s";
		}
		if (num == 5)
		{
			text += $" 首插屏门槛={GetFirstInterstitialLevel()} CD倍率={GetCdMultiplier():F2}";
		}
		return text;
	}
}
