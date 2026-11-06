using System;
using Game.Feature.Ad;
using RotateRings.Ad.Bridge;
using RotateRings.Ad.Orchestration;
using RotateRings.Ad.Strategy;
using RotateRings.Ad.Strategy.Interstitial;
using RotateRings.Common;
using RotateRings.Model;
using RotateRings.RewardedLuckyDraw;
using UniLog;
using XingYun.Ad;
using XingYun.Core.Singletons;

namespace RotateRings.Ad;

public static class AdService
{
	private static GameAdBridge bridge;

	private static FrontAdCoordinator frontAdCoordinator;

	private static RewardAdRequestFlow rewardAdRequestFlow;

	private static int lastUseArrStrategy = -1;

	private static int gmNoNetCountDown = 3;

	public static string LastInterstitialBlockReason { get; internal set; }

	public static int NoNetCountDown => 30;

	public static int GMNoNetCountDown
	{
		get
		{
			return gmNoNetCountDown;
		}
		set
		{
			gmNoNetCountDown = Math.Max(1, value);
		}
	}

	public static bool IsRewardReady => bridge?.IsRewardReady() ?? false;

	public static bool IsInterstitialReady => bridge?.IsInterstitialReady() ?? false;

	public static bool CanShowBanner => false;

	public static void Init()
	{
		if (bridge == null)
		{
			AdColdLaunchTracker.RecordForSession();
			AdColdLaunchGameTimeTracker.ResetForSession();
			lastUseArrStrategy = -1;
			frontAdCoordinator = new FrontAdCoordinator();
			bridge = new GameAdBridge();
			rewardAdRequestFlow = new RewardAdRequestFlow(bridge);
			bridge.Init();
			lastUseArrStrategy = (AdStrategyFactory.ShouldUseArrStrategy() ? 1 : 0);
		}
	}

	public static bool IsBackgroundEscapeLevelSkipEnabled()
	{
		return false;
	}

	public static void Release()
	{
		AdColdLaunchTracker.Reset();
		frontAdCoordinator?.Clear("Release");
		frontAdCoordinator = null;
		rewardAdRequestFlow = null;
		bridge?.Release();
		bridge = null;
		lastUseArrStrategy = -1;
	}

	public static void OnFeaturesReady()
	{
		Singleton<AdFeatureManager>.Instance?.GetRotateRingsInterstitialAdFeature()?.OnColdStartSession();
		InterstitialBackgroundEscapeTracker.CheckFirstInterstitialLevelAfterEscape();
	}

	public static void NotifyMidGameExit(int level)
	{
		bridge?.NotifyMidGameExit(level);
	}

	public static void ShowRewardAd(RewardAdRequest request, Action onPopClosed = null)
	{
		if (request == null)
		{
			Log.LogError("<Ad>", "ShowRewardAd request is null");
		}
		else
		{
			rewardAdRequestFlow.ShowRewardAd(request, onPopClosed);
		}
	}

	public static void PlayRewardAdDirect(string adPosition, Action<RewardAdResult> callback)
	{
		rewardAdRequestFlow.PlayRewardAdDirect(adPosition, callback);
	}

	public static void ShowRewardDirect(string adPosition, Action<RRShowAdResult> callback)
	{
		rewardAdRequestFlow.ShowRewardDirect(adPosition, callback);
	}

	private static void ShowBeforeInterstitial(InterstitialTrigger trigger, Action<RRShowAdResult> callback = null, RRAdContext context = null, bool waitForEnterPresentation = false, bool requireLevelReady = false)
	{
		IAdConfiguration config = RRAdConfigurationFactory.CreateInterstitial(trigger, context);
		if (GameApp.Proxy.Get<RewardedLuckyDrawProxy>().ConsumeNextFrontAdExemption())
		{
			callback?.Invoke(new RRShowAdResult(isSuccess: false, "RewardedLuckyDrawNextFrontAdExemption", trigger.ToString()));
			return;
		}
		PrepareInstallCampaign();
		if (!CheckFrontAdGate(config))
		{
			if (waitForEnterPresentation || requireLevelReady)
			{
				frontAdCoordinator.Queue(trigger, context, callback, waitForEnterPresentation);
			}
			else
			{
				ShowInterstitialInternal(trigger, callback, context);
			}
		}
	}

	private static bool CheckFrontAdGate(IAdConfiguration config)
	{
		int num = ARRExperiment.GetGroup();
		if (num < 1 || num > 4)
		{
			return false;
		}
		return !InstallCampaignPolicy.ShouldTakeOver(InterstitialPolicyContext.Create(config));
	}

	private static void PrepareInstallCampaign()
	{
		InstallCampaignAttributionHelper.RequestIfNeeded();
		int num = (AdStrategyFactory.ShouldUseArrStrategy() ? 1 : 0);
		if (num != lastUseArrStrategy)
		{
			lastUseArrStrategy = num;
			bridge?.ReapplyInterAdStrategy();
		}
	}

	public static void RequestHomeEnterBeforeInterstitial(InterstitialTrigger trigger, RRAdContext context = null)
	{
		ShowBeforeInterstitial(trigger, null, context, waitForEnterPresentation: true, requireLevelReady: true);
	}

	public static void RequestLevelTransitionBeforeInterstitial(InterstitialTrigger trigger, RRAdContext context = null)
	{
		if (trigger == InterstitialTrigger.Restart || trigger == InterstitialTrigger.FailedRestart)
		{
			int level = context?.Level ?? GameApp.Proxy.Get<LevelModelProxy>()?.Level ?? 0;
			bridge?.NotifyRestartClicked(level);
		}
		ShowBeforeInterstitial(trigger, null, context, waitForEnterPresentation: false, requireLevelReady: true);
	}

	public static bool IsFrontAdHasPending()
	{
		return frontAdCoordinator?.HasPending ?? false;
	}

	public static bool ShouldDeferPendingConsumeToEnterAnimation()
	{
		return frontAdCoordinator?.ShouldDeferToEnterAnimation ?? false;
	}

	public static void NotifyFrontAdLevelReady()
	{
		frontAdCoordinator?.NotifyLevelReady(ConsumeFrontAdPending);
	}

	public static void NotifyFrontAdEnterPresentationReady()
	{
		frontAdCoordinator?.NotifyEnterPresentationReady(ConsumeFrontAdPending);
	}

	public static bool TryShowPendingFrontAd()
	{
		return frontAdCoordinator?.TryConsumeForce(ConsumeFrontAdPending) ?? false;
	}

	public static void ClearPendingFrontAd(string reason = null)
	{
		frontAdCoordinator?.Clear(reason);
	}

	public static void ShowPostAd(Action<RRShowAdResult> callback = null, RRAdContext context = null)
	{
		using (new ProfilerScope("AdService.ShowPostAd"))
		{
			PrepareInstallCampaign();
			ShowInterstitialInternal(InterstitialTrigger.AfterWin, callback, context);
		}
	}

	public static void ShowBanner(RRBannerAdType type = RRBannerAdType.Game, bool isBottom = true)
	{
		bridge?.ShowBanner(isBottom);
	}

	public static void HideBanner()
	{
		bridge?.HideBanner();
	}

	public static int GetInterstitialOpenLv()
	{
		if ((GameApp.Proxy.Get<PlayerModelProxy>()?.Data?.ColdLaunchCount ?? 1) > 1)
		{
			return 20;
		}
		return 30;
	}

	private static void ApplyInterstitialBlockResult(InterstitialFlowContext flowContext)
	{
		if (flowContext == null || !flowContext.IsBlocked)
		{
			return;
		}
		LastInterstitialBlockReason = flowContext.BlockReason;
		if (flowContext.IsFrontAd && !(flowContext.BlockReason == "冷启免前插屏"))
		{
			AdStrategyRuntimeProxy adStrategyRuntimeProxy = GameApp.Proxy.Get<AdStrategyRuntimeProxy>();
			if (adStrategyRuntimeProxy != null && adStrategyRuntimeProxy.FirstRemoveFrontScreenAd)
			{
				adStrategyRuntimeProxy.FirstRemoveFrontScreenAd = false;
			}
		}
	}

	private static bool ConsumeFrontAdPending(FrontAdCoordinator.PendingRequest request)
	{
		ShowInterstitialInternal(request.Trigger, request.Callback, request.Context);
		_ = LastInterstitialBlockReason;
		return true;
	}

	private static void ShowInterstitialInternal(InterstitialTrigger trigger, Action<RRShowAdResult> callback, RRAdContext context)
	{
		LastInterstitialBlockReason = null;
		InterstitialFlowContext flowContext = new InterstitialFlowContext(trigger != InterstitialTrigger.AfterWin, ApplyInterstitialBlockResult);
		bridge.ShowInterstitial(trigger, context, flowContext, callback);
	}
}
