using System;
using System.ComponentModel;
using UnityEngine;
using XingYun.Ad;

namespace Game.DemoGM;

public class GameDemoGM
{
	private float lastPropertyUpdateTime;

	private const float PROPERTY_UPDATE_INTERVAL = 0.5f;

	private bool cachedIsInterAdReady;

	private bool cachedCanShowInterAd;

	private bool cachedIsRewardAdReady;

	private bool cachedCanShowRewardAd;

	private bool cachedCanShowBannerAd;

	private bool cachedIsShowingAd;

	[Category("广告测试")]
	[DisplayName("插屏广告是否准备好")]
	public bool IsInterAdReady
	{
		get
		{
			UpdatePropertyCache();
			return cachedIsInterAdReady;
		}
	}

	[Category("广告测试")]
	[DisplayName("是否可以播放插屏")]
	public bool CanShowInterAd
	{
		get
		{
			UpdatePropertyCache();
			return cachedCanShowInterAd;
		}
	}

	[Category("广告测试")]
	[DisplayName("激励广告是否准备好")]
	public bool IsRewardAdReady
	{
		get
		{
			UpdatePropertyCache();
			return cachedIsRewardAdReady;
		}
	}

	[Category("广告测试")]
	[DisplayName("是否可以播放激励")]
	public bool CanShowRewardAd
	{
		get
		{
			UpdatePropertyCache();
			return cachedCanShowRewardAd;
		}
	}

	[Category("广告测试")]
	[DisplayName("是否可以显示Banner")]
	public bool CanShowBannerAd
	{
		get
		{
			UpdatePropertyCache();
			return cachedCanShowBannerAd;
		}
	}

	[Category("广告测试")]
	[DisplayName("是否正在播放广告")]
	public bool IsShowingAd
	{
		get
		{
			UpdatePropertyCache();
			return cachedIsShowingAd;
		}
	}

	private void UpdatePropertyCache()
	{
		float realtimeSinceStartup = Time.realtimeSinceStartup;
		if (!(realtimeSinceStartup - lastPropertyUpdateTime < 0.5f))
		{
			lastPropertyUpdateTime = realtimeSinceStartup;
			if (GameApp.Ad != null)
			{
				cachedIsInterAdReady = GameApp.Ad.IsInterAdReady;
				cachedCanShowInterAd = GameApp.Ad.CanShowInter;
				cachedIsRewardAdReady = GameApp.Ad.IsRewardAdReady;
				cachedCanShowRewardAd = GameApp.Ad.CanShowReward;
				cachedCanShowBannerAd = GameApp.Ad.CanShowBanner;
				cachedIsShowingAd = GameApp.Ad.IsShowingAd;
			}
			else
			{
				cachedIsInterAdReady = false;
				cachedCanShowInterAd = false;
				cachedIsRewardAdReady = false;
				cachedCanShowRewardAd = false;
				cachedCanShowBannerAd = false;
				cachedIsShowingAd = false;
			}
		}
	}

	[Category("广告测试")]
	[DisplayName("加载插屏广告")]
	public void LoadInterAd()
	{
		AdManager ad = GameApp.Ad;
		if (ad != null)
		{
			ad.LoadInterAd();
		}
		lastPropertyUpdateTime = 0f;
	}

	[Category("广告测试")]
	[DisplayName("播放插屏广告")]
	public void ShowInterAd()
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Expected O, but got Unknown
		if (GameApp.Ad != null)
		{
			AdConfiguration val = new AdConfiguration("GM_Test_Inter");
			GameApp.Ad.ShowInterAd((IAdConfiguration)(object)val, (Action<InterstitialAdResult>)delegate
			{
				lastPropertyUpdateTime = 0f;
			});
		}
	}

	[Category("广告测试")]
	[DisplayName("加载激励广告")]
	public void LoadRewardAd()
	{
		AdManager ad = GameApp.Ad;
		if (ad != null)
		{
			ad.LoadRewardAd();
		}
		lastPropertyUpdateTime = 0f;
	}

	[Category("广告测试")]
	[DisplayName("播放激励广告")]
	public void ShowRewardAd()
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Expected O, but got Unknown
		if (GameApp.Ad != null)
		{
			AdConfiguration val = new AdConfiguration("GM_Test_Reward");
			GameApp.Ad.ShowRewardAd((IAdConfiguration)(object)val, (Action<RewardedAdResult>)delegate
			{
				lastPropertyUpdateTime = 0f;
			});
		}
	}

	[Category("广告测试")]
	[DisplayName("显示Banner广告")]
	public void ShowBannerAd()
	{
		AdManager ad = GameApp.Ad;
		if (ad != null)
		{
			ad.ShowBanner();
		}
	}

	[Category("广告测试")]
	[DisplayName("隐藏Banner广告")]
	public void HideBannerAd()
	{
		AdManager ad = GameApp.Ad;
		if (ad != null)
		{
			ad.HideBanner();
		}
	}

	[Category("广告测试")]
	[DisplayName("设置Banner位置(底部)")]
	public void SetBannerBottom()
	{
		AdManager ad = GameApp.Ad;
		if (ad != null)
		{
			ad.SetBannerPosition(true);
		}
	}

	[Category("广告测试")]
	[DisplayName("设置Banner位置(顶部)")]
	public void SetBannerTop()
	{
		AdManager ad = GameApp.Ad;
		if (ad != null)
		{
			ad.SetBannerPosition(false);
		}
	}

	[Category("广告测试")]
	[DisplayName("设置广告静音")]
	public void SetAdMuted()
	{
		AdManager ad = GameApp.Ad;
		if (ad != null)
		{
			ad.SetMuted(true);
		}
	}

	[Category("广告测试")]
	[DisplayName("设置广告有声")]
	public void SetAdUnmuted()
	{
		AdManager ad = GameApp.Ad;
		if (ad != null)
		{
			ad.SetMuted(false);
		}
	}
}
