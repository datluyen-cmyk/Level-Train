using System;
using System.Text;
using Game.DataTrack;
using RotateRings.Ad.CommerceLtv;
using RotateRings.Ad.Strategy;
using RotateRings.Model;
using RotateRings.Prop;
using RotateRings.Tool;
using XingYun.Ad;

namespace RotateRings.Ad.Bridge;

public class GameAdBridge
{
	private PlayerModelProxy playerModelProxy;

	private GameAdStrategy gameAdStrategy;

	private int interStrategyId;

	private readonly AdSdkGateway sdkGateway;

	private readonly InterstitialPlaybackFlow interstitialFlow;

	private readonly RewardAdPlaybackFlow rewardFlow;

	public GameAdBridge(Func<string> buildInterstitialGateSnapshot = null)
	{
		sdkGateway = new AdSdkGateway();
		rewardFlow = new RewardAdPlaybackFlow(sdkGateway);
		interstitialFlow = new InterstitialPlaybackFlow(sdkGateway);
	}

	public void Init()
	{
		playerModelProxy = GameApp.Proxy.Get<PlayerModelProxy>();
		sdkGateway.InitAd();
		sdkGateway.SetAdEventHandler((IAdEventHandler)(object)new RotateRingsAdEventHandler());
		sdkGateway.SetAdDataTracker((IAdDataTracker)(object)new RotateRingsAdDataTracker());
		sdkGateway.InitBanner();
		CountryGdpGroupResolver.ApplyEffectiveCountryToTag();
		InterstitialFrequencyTracker.Init();
		CommerceLtvCooldownHooks.OnColdStart();
		InterstitialEscapeTracker.RestoreFromSave();
		InterstitialBackgroundEscapeTracker.CheckFirstInterstitialLevelAfterEscape();
		PropWinInterExemptAdHook.EnsureListening();
		ApplyStrategy(1);
		if (InterstitialEscapeTracker.HandlePendingKillEscape(recordEscape: true))
		{
			gameAdStrategy?.SetClosedInterstitial(isClosed: true);
			if (playerModelProxy?.Data != null && playerModelProxy.Data.LastInterAdTimestamp <= 0)
			{
				playerModelProxy.Data.LastInterAdTimestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
			}
			ApplyInterstitialCooldownAfterClose();
		}
		GameApp.Proxy.Get<AdStrategyRuntimeProxy>()?.OnColdStart();
		sdkGateway.LoadInterAd();
	}

	public void Release()
	{
		gameAdStrategy = null;
		playerModelProxy = null;
	}

	public void ShowInterstitial(InterstitialTrigger trigger, RRAdContext context, InterstitialFlowContext flowContext, Action<RRShowAdResult> callback)
	{
		interstitialFlow.Show(() => gameAdStrategy, () => playerModelProxy, trigger, context, flowContext, callback);
	}

	public void NotifyRestartClicked(int level)
	{
		gameAdStrategy?.NotifyRestartClicked(level);
	}

	public void NotifyMidGameExit(int level)
	{
		gameAdStrategy?.NotifyMidGameExit(level);
	}

	public bool TryShowRewardAd(RewardAdRequest request)
	{
		return rewardFlow.TryShow(request, () => gameAdStrategy, () => playerModelProxy);
	}

	public void PlayRewardAdDirect(string adPosition, Action<RewardAdResult> callback)
	{
		rewardFlow.PlayRewardAdDirect(adPosition, callback, gameAdStrategy, () => playerModelProxy);
	}

	public void ShowBanner(bool isBottom = true)
	{
		PropModelProxy propModelProxy = GameApp.Proxy.Get<PropModelProxy>();
		if ((propModelProxy == null || !propModelProxy.NoAd) && AdService.CanShowBanner)
		{
			RRAdConfigurationFactory.CreateBanner(RRBannerAdType.Game, RRAdContext.FromLevelProxy());
			sdkGateway.SetAdStrategy((IAdStrategy)(object)gameAdStrategy);
			sdkGateway.SetBannerPosition(isBottom);
			sdkGateway.ShowBanner();
		}
	}

	public void HideBanner()
	{
		sdkGateway.HideBanner();
		RRDataTrack.Instance.ReportBannerRevenue();
	}

	public bool IsInterstitialReady()
	{
		return AdBridgeSupport.IsInterstitialReady(sdkGateway);
	}

	public bool IsRewardReady()
	{
		return AdBridgeSupport.IsRewardReady(sdkGateway);
	}

	public bool IsAnyAdShowing()
	{
		return sdkGateway.IsShowingAd;
	}

	internal void ReapplyInterAdStrategy()
	{
		ApplyStrategy(interStrategyId);
	}

	internal string BuildInterstitialGateSnapshot()
	{
		InterstitialEscapeRecord orCreate = GameApp.Save.GetOrCreate<InterstitialEscapeRecord>();
		int value = GameApp.Proxy.Get<LevelModelProxy>()?.Level ?? (-1);
		string text = gameAdStrategy?.InterAdStrategy?.BuildExperimentPolicyDiagnostics() ?? "实验Policy状态不可用";
		StringBuilder stringBuilder = new StringBuilder(512);
		stringBuilder.Append("level=").Append(value).Append(" coldLaunch=")
			.Append(AdColdLaunchTracker.GetCountForDebug())
			.Append(" openLevel=")
			.Append(AdService.GetInterstitialOpenLv())
			.Append(" lastBlock=")
			.Append(AdService.LastInterstitialBlockReason ?? "none")
			.AppendLine();
		stringBuilder.Append("bypassGuards=").Append(AdDebugSettings.BypassBusinessGuards).Append(" bypassCd=")
			.Append(AdDebugSettings.BypassInterstitialCd)
			.Append(" bypassNet=")
			.Append(AdDebugSettings.BypassNetworkCheck)
			.AppendLine()
			.Append(" loginIgnoreDays=")
			.Append(3)
			.AppendLine();
		stringBuilder.Append("bgEscapeLevelSkip=").Append(AdService.IsBackgroundEscapeLevelSkipEnabled()).Append(" firstLevelAfterEscape=")
			.Append(orCreate.FirstInterstitialLevelAfterEscape)
			.Append(" escapeSkipPeriod=")
			.Append(orCreate.IsInEscapeSkipPeriod)
			.Append(" escapeSkipCount=")
			.Append(orCreate.EscapeSkipCount)
			.AppendLine();
		stringBuilder.Append("interReady=").Append(AdService.IsInterstitialReady).Append(" hasNet=")
			.Append(NetUtil.IsHaveNet)
			.AppendLine();
		stringBuilder.Append("experimentPolicies=").Append(text.Replace('\n', '|'));
		return stringBuilder.ToString();
	}

	private void ApplyStrategy(int strategyId)
	{
		interStrategyId = strategyId;
		gameAdStrategy = AdStrategyFactory.Create();
		AdStrategyRuntimeProxy proxy = GameApp.Proxy.Get<AdStrategyRuntimeProxy>();
		gameAdStrategy.SetContext(proxy, playerModelProxy);
		gameAdStrategy.InitInterstitial((playerModelProxy?.Data?.LastInterAdTimestamp).GetValueOrDefault());
		sdkGateway.SetAdStrategy((IAdStrategy)(object)gameAdStrategy);
	}

	private void ApplyInterstitialCooldownAfterClose()
	{
		if (!InterstitialEscapeTracker.IsInActiveEscapeSkipPeriod())
		{
			gameAdStrategy?.StartInterstitialCooldown();
		}
	}
}
