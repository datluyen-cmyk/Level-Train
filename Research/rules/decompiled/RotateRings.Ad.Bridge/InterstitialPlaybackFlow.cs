using System;
using System.Collections.Generic;
using System.Text;
using Game.DataTrack;
using RotateRings.Ad.CommerceLtv;
using RotateRings.Ad.Strategy;
using RotateRings.Common;
using RotateRings.Model;
using UnityEngine;
using XingYun.Ad;

namespace RotateRings.Ad.Bridge;

internal sealed class InterstitialPlaybackFlow
{
	private readonly AdSdkGateway sdkGateway;

	public InterstitialPlaybackFlow(AdSdkGateway sdkGateway, Func<string> buildInterstitialGateSnapshot = null)
	{
		this.sdkGateway = sdkGateway;
	}

	public void Show(Func<GameAdStrategy> getGameAdStrategy, Func<PlayerModelProxy> getPlayerModelProxy, InterstitialTrigger trigger, RRAdContext context, InterstitialFlowContext flowContext, Action<RRShowAdResult> callback)
	{
		//IL_0113: Unknown result type (might be due to invalid IL or missing references)
		//IL_011e: Expected O, but got Unknown
		GameAdStrategy gameAdStrategy = getGameAdStrategy();
		using (new ProfilerScope("GameAdBridge.ShowInterstitial"))
		{
			IAdConfiguration businessConfig = RRAdConfigurationFactory.CreateInterstitial(trigger, context);
			bool isAfterShow = trigger == InterstitialTrigger.AfterWin;
			bool isFrontAd = !isAfterShow;
			if (AdBridgeSupport.TryHandleRemoveAds(businessConfig.AdPosition, callback))
			{
				return;
			}
			if (AdBridgeSupport.ShouldBlockFrontAd(isFrontAd))
			{
				BlockInterstitial(gameAdStrategy, "Settlement UI open", businessConfig, flowContext, callback);
				return;
			}
			if (!AdBridgeSupport.HasNetworkForAd())
			{
				BlockInterstitial(gameAdStrategy, "No network", businessConfig, flowContext, callback);
				return;
			}
			if (sdkGateway.IsShowingAd)
			{
				callback?.Invoke(new RRShowAdResult(isSuccess: false, "Another ad is showing", businessConfig.AdPosition));
				return;
			}
			AdConfiguration config = AdConfigurationExtensions.SetIsFrontAd(new AdConfiguration(businessConfig.AdPosition), isFrontAd);
			InterAdCheckResult interAdCheckResult = gameAdStrategy.CanShowInter((IAdConfiguration)(object)config);
			if (!interAdCheckResult.Allowed)
			{
				BlockInterstitial(gameAdStrategy, interAdCheckResult.BlockReason ?? "Blocked by inter strategy", (IAdConfiguration)(object)config, flowContext, callback);
				return;
			}
			RRDataTrack.Instance.ReportAdVisit(RRDataTrack.AdVisitType.Inter, businessConfig.AdPosition);
			InterstitialFrequencyTracker.RecordShowAction(Time.realtimeSinceStartup);
			InterstitialEscapeTracker.PrepareNextInterstitialShow(isAfterShow);
			sdkGateway.SetAdStrategy((IAdStrategy)(object)gameAdStrategy);
			AdConfiguration val = AdBridgeSupport.CreateTrackingConfig(businessConfig.AdPosition);
			sdkGateway.TrackAdIncomeRevenue("inter", (IAdConfiguration)(object)val);
			AdBridgeSupport.PrintCustomData(val);
			InterstitialEscapeTracker.TryMarkInterstitialShowingFromPrepared();
			sdkGateway.ShowInterAd(businessConfig, delegate(InterstitialAdResult result)
			{
				ShowInterAdCallBack(gameAdStrategy, getPlayerModelProxy(), result, businessConfig, isAfterShow, callback);
			});
		}
	}

	private void ShowInterAdCallBack(GameAdStrategy gameAdStrategy, PlayerModelProxy playerModelProxy, InterstitialAdResult result, IAdConfiguration config, bool isAfterShow, Action<RRShowAdResult> callback)
	{
		bool isSuccess = ((AdResult)result).IsSuccess;
		if (((AdResult)result).IsSuccess)
		{
			List<double> last3InterIncomeList = GameApp.Tag.Last3InterIncomeList;
			double num;
			if (last3InterIncomeList == null || last3InterIncomeList.Count <= 0)
			{
				num = 0.0;
			}
			else
			{
				num = last3InterIncomeList[last3InterIncomeList.Count - 1];
			}
			double num2 = num;
			if (num2 > 0.0)
			{
				ColdStartEcpmTracker.Record(num2);
			}
		}
		InterstitialEscapeTracker.ClearPreparedShow();
		if (InterstitialEscapeTracker.IsInterstitialPlayConfirmed())
		{
			InterstitialEscapeTracker.ClearInterstitialClosePending();
			InterstitialBackgroundEscapeTracker.NotifyAdClosed();
		}
		CommerceLtvCooldownHooks.OnInterstitialClosed(isSuccess, (config != null) ? config.AdPosition : null);
		InterstitialEscapeTracker.RecordAndEvaluate(isSuccess, isAfterShow);
		if (isSuccess)
		{
			ApplyInterstitialCooldownAfterClose(gameAdStrategy);
			GameApp.Proxy.Get<AdStrategyRuntimeProxy>()?.RecordInterAdPlayed();
			AdDailyTaskBridge.OnInterAdWatched();
			if (playerModelProxy?.Data != null)
			{
				playerModelProxy.Data.LastInterAdTimestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
			}
		}
		gameAdStrategy?.OnInterstitialFinished(new InterstitialFinishedContext((config != null) ? config.AdPosition : null, isSuccess, isAfterShow));
		GameApp.Event.Send<RotateRingsAdClosed>(new RotateRingsAdClosed
		{
			AdType = "inter",
			IsSuccess = isSuccess
		});
		callback?.Invoke(new RRShowAdResult(isSuccess, ((AdResult)result).Message, config.AdPosition));
	}

	private void ApplyInterstitialCooldownAfterClose(GameAdStrategy gameAdStrategy)
	{
		if (!InterstitialEscapeTracker.IsInActiveEscapeSkipPeriod())
		{
			gameAdStrategy?.StartInterstitialCooldown();
		}
	}

	private void BlockInterstitial(GameAdStrategy gameAdStrategy, string reason, IAdConfiguration config, InterstitialFlowContext flowContext, Action<RRShowAdResult> callback)
	{
		flowContext?.MarkBlocked(reason);
		StringBuilder stringBuilder = new StringBuilder(384);
		stringBuilder.Append("ShowInterstitial blocked: ").Append(reason ?? "unknown").Append(" position=")
			.Append(((config != null) ? config.AdPosition : null) ?? string.Empty)
			.Append(" isFrontAd=")
			.Append(config?.GetIsFrontAd() ?? false);
		string value = gameAdStrategy?.BuildInterstitialBlockDiagnostics(config, reason);
		if (!string.IsNullOrEmpty(value))
		{
			stringBuilder.Append('\n').Append(value);
		}
		callback?.Invoke(new RRShowAdResult(isSuccess: false, reason, (config != null) ? config.AdPosition : null));
	}
}
