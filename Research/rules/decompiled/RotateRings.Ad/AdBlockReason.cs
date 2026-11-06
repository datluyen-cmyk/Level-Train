using System;

namespace RotateRings.Ad;

public static class AdBlockReason
{
	public const string SettlementUiOpen = "Settlement UI open";

	public const string NoNetwork = "No network";

	public const string InterstitialNotReady = "Interstitial not ready";

	public const string AnotherAdShowing = "Another ad is showing";

	public const string BlockedByInterStrategy = "Blocked by inter strategy";

	public const string RemoveAdsPurchased = "已购买去广告";

	public const string FirstDayInterstitialBlocked = "安装首日不展示前插屏";

	public const string FirstDayAdBlocked = "安装首日插屏策略不通过";

	public const string FirstLaunchInterstitialBlocked = "首冷启免插屏";

	public const string FirstDayPostAdClosed = "安装首日无后插屏";

	public const string NotShowFrontAdStrategyBlocked = "安装前x天不展示前插屏策略拦截";

	public const string NotShowFrontAdStrategyBlocked1 = "安装首日不展示前插屏(底板)";

	public const string ColdStartFirstLevelInterstitialBlocked = "冷启首局免插屏(前3活跃日含首日)";

	public const string LoginIgnoreFrontInterstitialBlocked = "冷启免前插屏";

	public const string LevelThresholdInterstitialBlocked = "插屏开启等级门槛";

	public const string SpecialChanelFirstAdBlocked = "FB&TK首次插屏等级门槛";

	public const string InstallCampaignFirstMustBePostAd = "install campaign首个插屏必须是后插屏";

	public const string InstallCampaignSchemeBlocked = "install campaign首插屏门槛不通过";

	public const string FailedLevelInterstitialBlocked = "失败当关免插屏";

	public const string EscapeSkipPeriodActive = "逃逸免插屏时间生效";

	public const string EscapeLevelRangeActive = "逃逸免插屏关卡范围生效";

	public const string EscapeSkipCountActive = "逃逸免插屏次数生效";

	public const string MidRewardPostInterstitialExemption = "局内激励通关后免后插屏";

	public const string MidRewardNextFrontInterstitialExemption = "局内激励通关后免下一局前插屏";

	public const string RewardAdTimedInterstitialExemption = "激励广告后免插屏时间窗生效";

	public const string PropWinPostInterstitialExemption = "使用道具通关后免后插屏";

	public const string PropWinNextFrontInterstitialExemption = "使用道具通关后免下一局前插屏";

	public const string RestartSkipNextLevelFrontAdExemption = "重新开始免下局前插屏";

	public const string SetbackInterstitialExemption = "受挫豁免：本关连续重开/中途退出免插屏";

	public const string ARRRemoveFrontAd = "ARR策略删除前插屏";

	public const string ARRChannelExemption = "ARR策略渠道免插屏";

	public const string ARRRewardExemption = "ARR策略激励后免后插屏";

	public const string ARRGameDurationExemption = "加快收入回收：ARR游戏时长不足免后插屏";

	public const string CooldownSkippedByGm = "CD已跳过(GM BypassInterstitialCd)";

	public const string CooldownInactiveWithoutHistory = "CD未生效(无历史关闭记录)";

	private const string CooldownReasonPrefix = "CD";

	public static string FormatGp39CooldownBlocked(float elapsed, float interval, float remaining, int cacheCount, float earlyReleaseThreshold, bool isAdWorth, int maxCacheCount)
	{
		return $"CD未满足 elapsed={elapsed:F1}s interval={interval:F1}s remaining={remaining:F1}s cache={cacheCount} (70%放行需 elapsed>={earlyReleaseThreshold:F1}s 且 (GetAdWorth={isAdWorth} 或 cache>={maxCacheCount}))";
	}

	public static string FormatCommerceLtvCooldownBlocked(float elapsed, float interval, float remaining, float minCd, bool highPriceInstantPass)
	{
		return string.Format("CD-CommerceLtv未满足 elapsed={0:F1}s interval={1:F1}s remaining={2:F1}s min_cd={3:F1}s 高价即时放行={4}", elapsed, interval, remaining, minCd, highPriceInstantPass ? "可放行(未拦截)" : "不满足");
	}

	public static string FormatFixedCooldownBlocked(float elapsed, float interval, float remaining, bool isEscapeSkipActive)
	{
		return string.Format("基础CD未满足 elapsed={0:F1}s interval={1:F1}s remaining={2:F1}s (逃逸180s={3})", elapsed, interval, remaining, isEscapeSkipActive ? "生效" : "未生效");
	}

	public static string FormatLegacyLevelBlocked(int requiredPlayCount)
	{
		return $"Level<{requiredPlayCount}";
	}

	public static string FormatLegacyCooldownBlocked(int cooldownSeconds)
	{
		return $"CD<{cooldownSeconds}s";
	}

	public static string FormatInterstitialDurationBlocked(int requiredSeconds, float actualSeconds)
	{
		return $"插屏有效时长门槛 required={requiredSeconds}s actual={actualSeconds:F0}s";
	}

	public static string FormatInterstitialFirstAdTimeBlocked(int experimentId, int requiredSeconds, float actualSeconds)
	{
		return $"首次插屏等价时长门槛 experimentId={experimentId} required={requiredSeconds}s actual={actualSeconds:F0}s";
	}

	public static string FormatGpPreinstallFirstAdTimeBlocked(int scheme, int requiredSeconds, float actualSeconds)
	{
		return $"GP预装首次插屏门槛 scheme={scheme} required={requiredSeconds}s actual={actualSeconds:F0}s";
	}

	public static string FormatInterstitialExemptionBlocked(int experimentId, int requiredSeconds, float sessionDurationSeconds)
	{
		return $"新用户前期插屏额外豁免 experimentId={experimentId} required={requiredSeconds}s sessionDuration={sessionDurationSeconds:F0}s";
	}

	public static string FormatInterstitialLevelThresholdBlocked(bool isFirstSession, int threshold, int mainLevel, int completedLevel, bool isPostAd, bool requirePostFirst)
	{
		return $"插屏等级门槛 firstSession={isFirstSession} threshold={threshold} mainLevel={mainLevel} completedLevel={completedLevel} isPostAd={isPostAd} requirePostFirst={requirePostFirst}";
	}

	public static string FormatSpecialChanelFirstAdBlocked(int scheme, string channel, int coldLaunchCount, bool isPostAd, int threshold, int mainLevel, int completedLevel)
	{
		return $"FB&TK首次插屏等级门槛 scheme={scheme} channel={channel} coldLaunchCount={coldLaunchCount} " + $"isPostAd={isPostAd} threshold={threshold} mainLevel={mainLevel} completedLevel={completedLevel}";
	}

	public static string FormatInstallCampaignLevelBlocked(string suffix, int scheme, int coldLaunchCount, bool isPostAd, int threshold, int mainLevel, int completedLevel)
	{
		return $"install campaign首插屏等级门槛 suffix={suffix} scheme={scheme} coldLaunchCount={coldLaunchCount} " + $"isPostAd={isPostAd} threshold={threshold} mainLevel={mainLevel} completedLevel={completedLevel}";
	}

	public static string FormatInstallCampaignDurationBlocked(string suffix, int scheme, int requiredSeconds, float actualSeconds)
	{
		return $"install campaign首插屏时长门槛 suffix={suffix} scheme={scheme} " + $"requiredSeconds={requiredSeconds} actualSeconds={actualSeconds:F0}";
	}

	public static string FormatARRFirstInterstitialLevelBlocked(int threshold, int checkLevel, bool isPostAd)
	{
		return $"加快收入回收首插屏等级门槛 threshold={threshold} checkLevel={checkLevel} isPostAd={isPostAd}";
	}

	public static bool HasCooldownReasonPrefix(string reason)
	{
		if (!string.IsNullOrEmpty(reason))
		{
			return reason.StartsWith("CD", StringComparison.Ordinal);
		}
		return false;
	}
}
