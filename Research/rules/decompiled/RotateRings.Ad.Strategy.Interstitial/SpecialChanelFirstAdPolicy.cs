using System;
using RotateRings.Common;
using RotateRings.Model;

namespace RotateRings.Ad.Strategy.Interstitial;

public sealed class SpecialChanelFirstAdPolicy : InterstitialPolicyBase
{
	private const string Tag = "<Ad>.FB&TKFirstAdPolicy";

	private const string FacebookAdsChannel = "Facebook Ads";

	private const string TikTokGlobalChannel = "tiktokglobal_int";

	private static readonly int[][] SchemeThresholds = new int[6][]
	{
		new int[2] { 30, 3 },
		new int[2] { 20, 10 },
		new int[2] { 10, 6 },
		new int[2] { 5, 3 },
		new int[2] { 7, 5 },
		new int[2] { 3, 2 }
	};

	public override string Name => "SpecialChanelFirstAd";

	public override string BuildDebugState()
	{
		return $"scheme={GetConfiguredScheme()} channel={AdBridgeHelper.GetEffectiveChannel()} postSuccess={InterstitialEscapeTracker.ShowAdPostSuccessfulCount}";
	}

	private static int GetConfiguredScheme()
	{
		return FeatureConfig.InterstitialFunction?.SpecialChanelFirstAdScheme ?? 0;
	}

	private static bool IsSupportedScheme(int scheme)
	{
		int num;
		if (scheme >= 1)
		{
			num = ((scheme <= SchemeThresholds.Length) ? 1 : 0);
			if (num != 0)
			{
			}
		}
		else
		{
			num = 0;
		}
		return (byte)num != 0;
	}

	private static bool IsSpecialChannel(string channel)
	{
		if (string.IsNullOrWhiteSpace(channel))
		{
			return false;
		}
		if (!string.Equals(channel, "Facebook Ads", StringComparison.OrdinalIgnoreCase))
		{
			return string.Equals(channel, "tiktokglobal_int", StringComparison.OrdinalIgnoreCase);
		}
		return true;
	}

	public static bool IsFirstSuccessfulInterstitialPending()
	{
		return InterstitialEscapeTracker.SuccessfulInterstitialCount == 0;
	}

	public static bool ShouldTakeOverLevelGate(string channel)
	{
		if (IsSupportedScheme(GetConfiguredScheme()) && IsSpecialChannel(channel))
		{
			return IsFirstSuccessfulInterstitialPending();
		}
		return false;
	}

	public override InterAdCheckResult Evaluate(InterstitialPolicyContext context)
	{
		return Evaluate(context.Channel, context.IsPostAd);
	}

	public InterAdCheckResult Evaluate(string channel, bool isPostAd)
	{
		int configuredScheme = GetConfiguredScheme();
		if (!IsSupportedScheme(configuredScheme))
		{
			return InterAdCheckResult.Allow;
		}
		int coldLaunchCount = AdBridgeHelper.GetColdLaunchCount();
		bool flag = coldLaunchCount <= 1;
		int num = SchemeThresholds[configuredScheme - 1][(!flag) ? 1u : 0u];
		LevelModelProxy levelModelProxy = GameApp.Proxy.Get<LevelModelProxy>();
		if (levelModelProxy == null)
		{
			return InterAdCheckResult.Block("FB&TK首次插屏等级门槛");
		}
		int mainLevel = levelModelProxy.MainLevel;
		int num2 = mainLevel - 1;
		if (!(isPostAd ? (num2 >= num) : (mainLevel >= num)))
		{
			return InterAdCheckResult.Block(AdBlockReason.FormatSpecialChanelFirstAdBlocked(configuredScheme, channel, coldLaunchCount, isPostAd, num, mainLevel, num2));
		}
		return InterAdCheckResult.Allow;
	}
}
