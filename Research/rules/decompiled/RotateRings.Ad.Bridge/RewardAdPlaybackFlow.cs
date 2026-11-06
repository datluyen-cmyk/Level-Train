using System;
using Game.DataTrack;
using RotateRings.Ad.Strategy;
using RotateRings.Model;
using XingYun.Ad;

namespace RotateRings.Ad.Bridge;

internal sealed class RewardAdPlaybackFlow
{
	private readonly AdSdkGateway sdkGateway;

	public RewardAdPlaybackFlow(AdSdkGateway sdkGateway)
	{
		this.sdkGateway = sdkGateway;
	}

	public bool TryShow(RewardAdRequest request, Func<GameAdStrategy> getGameAdStrategy, Func<PlayerModelProxy> getPlayerModelProxy)
	{
		if (request == null)
		{
			return true;
		}
		IAdConfiguration val = RRAdConfigurationFactory.CreateReward(request.PopPosition, RRAdContext.FromLevelProxy());
		if (AdBridgeSupport.TryHandleRewardRemoveAds(val.AdPosition, isFinishedAd: true, delegate(RRShowAdResult result)
		{
			request.Callback?.Invoke(result.ToRewardAdResult());
		}))
		{
			return true;
		}
		if (sdkGateway.IsShowingAd)
		{
			request.Callback?.Invoke(RewardAdResult.Fail());
			return true;
		}
		if (!AdBridgeSupport.HasNetworkForAd())
		{
			return false;
		}
		string adPosition = val.AdPosition;
		RRDataTrack.Instance.ReportAdVisit(RRDataTrack.AdVisitType.Reward, adPosition);
		GameAdStrategy gameAdStrategy = getGameAdStrategy();
		switch (gameAdStrategy.CanShowReward(val))
		{
		case RewardAdCheckResult.Block:
			request.Callback?.Invoke(RewardAdResult.Fail());
			return true;
		case RewardAdCheckResult.FreeSkip:
			request.Callback?.Invoke(RewardAdResult.Success(isFinishedAd: false));
			return true;
		default:
		{
			AdConfiguration trackingConfig = AdBridgeSupport.CreateTrackingConfig(val.AdPosition);
			PlayRewardAdDirect(trackingConfig, request.Callback, gameAdStrategy, getPlayerModelProxy);
			return true;
		}
		}
	}

	public void PlayRewardAdDirect(string adPosition, Action<RewardAdResult> callback, GameAdStrategy gameAdStrategy, Func<PlayerModelProxy> getPlayerModelProxy)
	{
		PlayRewardAdDirect(AdBridgeSupport.CreateTrackingConfig(adPosition), callback, gameAdStrategy, getPlayerModelProxy);
	}

	private void PlayRewardAdDirect(AdConfiguration trackingConfig, Action<RewardAdResult> callback, GameAdStrategy gameAdStrategy, Func<PlayerModelProxy> getPlayerModelProxy)
	{
		AdBridgeSupport.PrintCustomData(trackingConfig);
		sdkGateway.TrackAdIncomeRevenue("reward", (IAdConfiguration)(object)trackingConfig);
		if (sdkGateway.IsShowingAd)
		{
			callback?.Invoke(RewardAdResult.Fail());
			return;
		}
		if (!sdkGateway.CanShowReward)
		{
			callback?.Invoke(RewardAdResult.Fail());
			return;
		}
		ExecuteRewardAd((IAdConfiguration)(object)trackingConfig, gameAdStrategy, getPlayerModelProxy, delegate(RRShowAdResult result)
		{
			callback?.Invoke(result.ToRewardAdResult());
		});
	}

	private void ExecuteRewardAd(IAdConfiguration config, GameAdStrategy gameAdStrategy, Func<PlayerModelProxy> getPlayerModelProxy, Action<RRShowAdResult> callback)
	{
		sdkGateway.SetAdStrategy((IAdStrategy)(object)gameAdStrategy);
		sdkGateway.ShowRewardAd(config, delegate(RewardedAdResult result)
		{
			//IL_000b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0011: Invalid comparison between Unknown and I4
			//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
			//IL_00bb: Invalid comparison between Unknown and I4
			bool isSuccess = ((AdResult)result).IsSuccess;
			bool flag = isSuccess && (int)result.RewardType == 1;
			if (isSuccess && flag)
			{
				getPlayerModelProxy()?.StatisticalRewardAdCount();
			}
			int level = GameApp.Proxy.Get<LevelModelProxy>()?.Level ?? 0;
			gameAdStrategy?.OnRewardAdFinished(new RewardAdFinishedContext(config, config.AdPosition, isSuccess, flag, level));
			GameApp.Event.Send<RotateRingsAdClosed>(new RotateRingsAdClosed
			{
				AdType = "reward",
				IsSuccess = isSuccess
			});
			callback?.Invoke(new RRShowAdResult(isSuccess, ((AdResult)result).Message, config.AdPosition, isSuccess && (int)result.RewardType == 2, flag));
		});
	}
}
