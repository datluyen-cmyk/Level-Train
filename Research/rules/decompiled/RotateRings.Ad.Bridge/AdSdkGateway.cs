using System;
using XingYun.Ad;
using XingYun.Aot;

namespace RotateRings.Ad.Bridge;

internal sealed class AdSdkGateway
{
	public bool IsInterAdReady => GameApp.Ad.IsInterAdReady;

	public bool IsRewardAdReady => GameApp.Ad.IsRewardAdReady;

	public bool IsShowingAd => GameApp.Ad.IsShowingAd;

	public bool CanShowReward => GameApp.Ad.CanShowReward;

	public void InitAd()
	{
		GameApp.Ad.Init();
	}

	public void SetAdEventHandler(IAdEventHandler handler)
	{
		GameApp.Ad.SetAdEventHandler(handler);
	}

	public void SetAdDataTracker(IAdDataTracker tracker)
	{
		GameApp.Ad.dataTracker = tracker;
	}

	public void InitBanner()
	{
		SDKManager.InitBanner();
	}

	public void SetAdStrategy(IAdStrategy strategy)
	{
		GameApp.Ad.SetAdStrategy(strategy);
	}

	public void LoadInterAd()
	{
		GameApp.Ad.LoadInterAd();
	}

	public void TrackAdIncomeRevenue(string adType, IAdConfiguration config)
	{
		IAdDataTracker dataTracker = GameApp.Ad.dataTracker;
		if (dataTracker != null)
		{
			dataTracker.TrackAdIncomeRevenue(adType, config);
		}
	}

	public void ShowInterAd(IAdConfiguration config, Action<InterstitialAdResult> callback)
	{
		GameApp.Ad.ShowInterAd(config, callback);
	}

	public void ShowRewardAd(IAdConfiguration config, Action<RewardedAdResult> callback)
	{
		GameApp.Ad.ShowRewardAd(config, callback);
	}

	public void SetBannerPosition(bool isBottom)
	{
		GameApp.Ad.SetBannerPosition(isBottom);
	}

	public void ShowBanner()
	{
		GameApp.Ad.ShowBanner();
	}

	public void HideBanner()
	{
		GameApp.Ad.HideBanner();
	}
}
