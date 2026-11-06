using Game.Feature.Ad;
using RotateRings.Common;
using RotateRings.Model;
using RotateRings.Prop;
using XingYun.Core.Singletons;

namespace RotateRings.Ad.Strategy.Interstitial;

public sealed class InterstitialBaseboardGate
{
	private readonly InstallCampaignPolicy installCampaignPolicy = new InstallCampaignPolicy();

	public InterAdCheckResult EvaluateRemoveAds()
	{
		PropModelProxy propModelProxy = GameApp.Proxy.Get<PropModelProxy>();
		if (propModelProxy != null && propModelProxy.NoAd)
		{
			return InterAdCheckResult.Block("已购买去广告");
		}
		return InterAdCheckResult.Allow;
	}

	public InterAdCheckResult Evaluate(InterstitialPolicyContext context, AdStrategyRuntimeProxy proxy, out bool handled)
	{
		handled = false;
		bool isFrontAd = context.IsFrontAd;
		if (isFrontAd && proxy != null && proxy.FirstRemoveFrontScreenAd)
		{
			proxy.FirstRemoveFrontScreenAd = false;
			return InterAdCheckResult.Block("冷启免前插屏");
		}
		RotateRingsInterstitialAdFeature rotateRingsInterstitialAdFeature = Singleton<AdFeatureManager>.Instance?.GetRotateRingsInterstitialAdFeature();
		if (rotateRingsInterstitialAdFeature != null && rotateRingsInterstitialAdFeature.ShouldSkipForColdStart(!isFrontAd))
		{
			return InterAdCheckResult.Block("冷启首局免插屏(前3活跃日含首日)");
		}
		bool flag = AdBridgeHelper.IsFirstInstallDay();
		if (!isFrontAd && flag && !FeatureConfig.InterstitialFunction.FirstDayPostAdToggle)
		{
			return InterAdCheckResult.Block("安装首日无后插屏");
		}
		if (AdBridgeHelper.GetInstallDays() == 1 && context.IsFrontAd)
		{
			return InterAdCheckResult.Block("安装首日不展示前插屏(底板)");
		}
		InterstitialEscapeRecord orCreate = GameApp.Save.GetOrCreate<InterstitialEscapeRecord>();
		if (orCreate.IsInEscapeSkipPeriod)
		{
			float escapeSkipRemainingSeconds = InterstitialEscapeTracker.GetEscapeSkipRemainingSeconds();
			InterstitialEscapeTracker.LogEscapePeriodSkipApplied(isFrontAd, escapeSkipRemainingSeconds);
			return InterAdCheckResult.Block("逃逸免插屏时间生效");
		}
		int num = GameApp.Proxy.Get<LevelModelProxy>()?.Level ?? 1;
		int num2 = (isFrontAd ? num : (num - 1));
		if (AdService.IsBackgroundEscapeLevelSkipEnabled() && orCreate.FirstInterstitialLevelAfterEscape > 0 && num2 <= orCreate.FirstInterstitialLevelAfterEscape)
		{
			return InterAdCheckResult.Block("逃逸免插屏关卡范围生效");
		}
		if (InstallCampaignPolicy.ShouldTakeOver(context))
		{
			handled = true;
			return installCampaignPolicy.Evaluate(context);
		}
		return InterAdCheckResult.Allow;
	}

	public void OnInit()
	{
		installCampaignPolicy.OnInit();
	}

	public void OnInterstitialFinished(InterstitialFinishedContext context)
	{
		installCampaignPolicy.OnInterstitialFinished(context);
	}

	public void OnRewardAdFinished(RewardAdFinishedContext context)
	{
		installCampaignPolicy.OnRewardAdFinished(context);
	}

	public void OnRestartClicked(int level)
	{
		installCampaignPolicy.OnRestartClicked(level);
	}

	public void OnMidGameExit(int level)
	{
		installCampaignPolicy.OnMidGameExit(level);
	}

	public void ResetForDebug()
	{
		installCampaignPolicy.ResetForDebug();
	}

	public string BuildDebugState()
	{
		string text = installCampaignPolicy.BuildDebugState();
		if (!string.IsNullOrEmpty(text))
		{
			return "InstallCampaign: " + text;
		}
		return string.Empty;
	}
}
