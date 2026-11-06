using RotateRings.Ad.Bridge;
using RotateRings.Common;
using RotateRings.Model;
using XingYun.Ad;

namespace RotateRings.Ad.Strategy.Interstitial;

public sealed class InterstitialPolicyContext
{
	public IAdConfiguration Config { get; }

	public bool IsFrontAd { get; }

	public bool IsPostAd => !IsFrontAd;

	public LevelModelProxy LevelProxy => GameApp.Proxy.Get<LevelModelProxy>();

	public int Level => LevelProxy?.Level ?? 1;

	public int MainLevel => LevelProxy?.MainLevel ?? 0;

	public int CompletedLevel => MainLevel - 1;

	public int ColdLaunchCount => AdBridgeHelper.GetColdLaunchCount();

	public bool IsFirstColdLaunch => ColdLaunchCount <= 1;

	public string Channel => AdBridgeHelper.GetEffectiveChannel();

	public string Campaign => ResolveCampaign();

	public float TotalValidDurationSeconds => AdBridgeHelper.GetTotalValidDurationSeconds();

	public AdStrategyRuntimeProxy RuntimeProxy => GameApp.Proxy.Get<AdStrategyRuntimeProxy>();

	public InterstitialFunction InterstitialConfig => FeatureConfig.InterstitialFunction;

	public InterstitialPolicyContext(IAdConfiguration config, bool isFrontAd)
	{
		Config = config;
		IsFrontAd = isFrontAd;
	}

	public static InterstitialPolicyContext Create(IAdConfiguration config)
	{
		return new InterstitialPolicyContext(config, config.GetIsFrontAd());
	}

	public static string ResolveCampaign()
	{
		if (AdDebugSettings.IsEditorActive && !string.IsNullOrWhiteSpace(AdDebugSettings.GmInstallCampaignSuffix))
		{
			return AdDebugSettings.GmInstallCampaignSuffix;
		}
		return InstallCampaignAttributionHelper.GetAppsflyerCampaign();
	}
}
