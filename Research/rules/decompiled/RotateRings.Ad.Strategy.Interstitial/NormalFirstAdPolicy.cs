using RotateRings.Common;
using RotateRings.Model;

namespace RotateRings.Ad.Strategy.Interstitial;

public sealed class NormalFirstAdPolicy : InterstitialPolicyBase
{
	private const string Tag = "<Ad>.FirstDayAdPolicy";

	private const int NonPreinstallThirtyMinuteFirstDaySeconds = 1800;

	private const int NonPreinstallTwentyMinuteNonFirstDaySeconds = 1200;

	private const int NonPreinstallFortyFiveMinuteFirstDaySeconds = 2700;

	private const int NonPreinstallThirtyMinuteNonFirstDaySeconds = 1800;

	private const int PreinstallFifteenMinuteFirstDaySeconds = 900;

	private const int PreinstallTenMinuteNonFirstDaySeconds = 600;

	private const int PreinstallTenMinuteFirstDaySeconds = 600;

	private const int PreinstallFiveMinuteNonFirstDaySeconds = 300;

	private const int FirstLaunchCountOpenLv = 30;

	private const int NonFirstLaunchCountOpenLv = 20;

	public override string Name => "FirstDayAd";

	public override string BuildDebugState()
	{
		return $"trigger={GetConfiguredTriggerValue()}";
	}

	private static bool IsSupported(int triggerValue)
	{
		if (triggerValue >= 1)
		{
			return triggerValue <= 10;
		}
		return false;
	}

	private int GetConfiguredTriggerValue()
	{
		InterstitialFunction interstitialFunction = FeatureConfig.InterstitialFunction;
		if (interstitialFunction == null)
		{
			return 0;
		}
		if (!IsAndroidPreinstallUser())
		{
			return interstitialFunction.NonPreInstalledFirstAdTime;
		}
		return interstitialFunction.PreInstalledFirstAdTime;
	}

	public override InterAdCheckResult Evaluate(InterstitialPolicyContext context)
	{
		int configuredTriggerValue = GetConfiguredTriggerValue();
		if (!IsSupported(configuredTriggerValue))
		{
			return InterAdCheckResult.Allow;
		}
		return Evaluate(configuredTriggerValue, context.IsPostAd);
	}

	public InterAdCheckResult Evaluate(int triggerValue, bool isPostAd)
	{
		string detail;
		if (!IsAndroidPreinstallUser())
		{
			return EvaluateNonPreinstall(triggerValue, isPostAd, out detail);
		}
		return EvaluatePreinstall(triggerValue, isPostAd, out detail);
	}

	private static InterAdCheckResult EvaluateNonPreinstall(int triggerValue, bool isPostAd, out string detail)
	{
		switch (triggerValue)
		{
		case 1:
		case 2:
			return EvaluateLevelThreshold(30, 20, isPostAd, requirePostFirst: false, out detail);
		case 3:
		case 4:
			return EvaluateLevelThreshold(40, 30, isPostAd, requirePostFirst: false, out detail);
		case 5:
		case 6:
			if (AdBridgeHelper.IsFirstColdLaunch())
			{
				detail = "首次冷启不展示插屏";
				return InterAdCheckResult.Block("首冷启免插屏");
			}
			return EvaluateLevelThreshold(30, 30, isPostAd, requirePostFirst: false, out detail);
		case 7:
		case 8:
			return EvaluateDurationThreshold(1800, 1200, out detail);
		case 9:
		case 10:
			return EvaluateDurationThreshold(2700, 1800, out detail);
		default:
			detail = $"未知触发值 {triggerValue}，跳过门槛";
			return InterAdCheckResult.Allow;
		}
	}

	private static InterAdCheckResult EvaluatePreinstall(int triggerValue, bool isPostAd, out string detail)
	{
		switch (triggerValue)
		{
		case 1:
		case 2:
			return EvaluateLevelThreshold(10, 5, isPostAd, requirePostFirst: true, out detail);
		case 3:
		case 4:
			return EvaluateLevelThreshold(5, 3, isPostAd, requirePostFirst: true, out detail);
		case 5:
		case 6:
			if (AdBridgeHelper.IsFirstColdLaunch())
			{
				detail = "首次冷启不展示插屏";
				return InterAdCheckResult.Block("首冷启免插屏");
			}
			return EvaluateLevelThreshold(30, 30, isPostAd, requirePostFirst: true, out detail);
		case 7:
		case 8:
			return EvaluateDurationThreshold(900, 600, out detail);
		case 9:
		case 10:
			return EvaluateDurationThreshold(600, 300, out detail);
		default:
			detail = $"未知触发值 {triggerValue}，跳过门槛";
			return InterAdCheckResult.Allow;
		}
	}

	private static InterAdCheckResult EvaluateLevelThreshold(int firstSessionLevel, int nonFirstSessionLevel, bool isPostAd, bool requirePostFirst, out string detail)
	{
		LevelModelProxy levelModelProxy = GameApp.Proxy.Get<LevelModelProxy>();
		if (levelModelProxy == null)
		{
			detail = "等级门槛无法判定：LevelModelProxy 为空";
			return InterAdCheckResult.Block("插屏开启等级门槛");
		}
		bool flag = AdBridgeHelper.IsFirstColdLaunch();
		int num = (flag ? firstSessionLevel : nonFirstSessionLevel);
		int mainLevel = levelModelProxy.MainLevel;
		int num2 = mainLevel - 1;
		bool flag2;
		string text;
		if (isPostAd)
		{
			flag2 = num2 >= num;
			text = $"completedLevel({num2}) >= threshold({num})";
		}
		else if (requirePostFirst)
		{
			flag2 = mainLevel > num;
			text = $"mainLevel({mainLevel}) > threshold({num})";
		}
		else
		{
			flag2 = mainLevel >= num;
			text = $"mainLevel({mainLevel}) >= threshold({num})";
		}
		detail = $"等级门槛 firstSession={flag} requirePostFirst={requirePostFirst} {text} => {flag2}";
		if (!flag2)
		{
			return InterAdCheckResult.Block(AdBlockReason.FormatInterstitialLevelThresholdBlocked(flag, num, mainLevel, num2, isPostAd, requirePostFirst));
		}
		return InterAdCheckResult.Allow;
	}

	private static InterAdCheckResult EvaluateDurationThreshold(int firstDaySeconds, int nonFirstDaySeconds, out string detail)
	{
		bool flag = AdBridgeHelper.IsFirstInstallDay();
		int num = (flag ? firstDaySeconds : nonFirstDaySeconds);
		float totalValidDurationSeconds = AdBridgeHelper.GetTotalValidDurationSeconds();
		bool flag2 = totalValidDurationSeconds >= (float)num;
		detail = $"时长门槛 firstInstallDay={flag} actualSeconds={totalValidDurationSeconds:F1} >= requiredSeconds({num}) => {flag2}";
		if (!flag2)
		{
			return InterAdCheckResult.Block(AdBlockReason.FormatInterstitialDurationBlocked(num, totalValidDurationSeconds));
		}
		return InterAdCheckResult.Allow;
	}

	private static bool IsAndroidPreinstallUser()
	{
		return PreinstallChannelHelper.IsCurrentUserPreinstall();
	}
}
