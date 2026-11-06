using RotateRings.Model;
using RotateRings.UI.Shop;
using XingYun.Ad;

namespace RotateRings.Ad.Strategy;

public class GameAdStrategy : IAdStrategy, IAdLifecycleStrategy
{
	private readonly AdStrategyBase interStrategy;

	private readonly AdStrategyBase rewardStrategy;

	private readonly IInterAdStrategy interAdStrategy;

	private readonly IRewardAdStrategy rewardAdStrategy;

	internal IInterAdStrategy InterAdStrategy => interAdStrategy;

	public GameAdStrategy(AdStrategyBase interStrategy, AdStrategyBase rewardStrategy)
	{
		this.interStrategy = interStrategy;
		this.rewardStrategy = rewardStrategy;
		interAdStrategy = interStrategy as IInterAdStrategy;
		rewardAdStrategy = rewardStrategy as IRewardAdStrategy;
	}

	public void SetContext(AdStrategyRuntimeProxy proxy, PlayerModelProxy playerProxy)
	{
		interStrategy.SetContext(proxy, playerProxy);
		rewardStrategy.SetContext(proxy, playerProxy);
	}

	public InterAdCheckResult CanShowInter(IAdConfiguration config)
	{
		return interAdStrategy?.CanShowInter(config) ?? InterAdCheckResult.Allow;
	}

	public RewardAdCheckResult CanShowReward(IAdConfiguration config)
	{
		return rewardAdStrategy?.CanShowReward(config) ?? RewardAdCheckResult.Allow;
	}

	public void InitInterstitial(long lastInterAdTimestamp)
	{
		interAdStrategy?.InitInterstitial(lastInterAdTimestamp);
	}

	public void StartInterstitialCooldown()
	{
		interAdStrategy?.StartInterstitialCooldown();
	}

	public void OnInterstitialFinished(InterstitialFinishedContext context)
	{
		interAdStrategy?.OnInterstitialFinished(context);
		if (context.IsSuccess)
		{
			GameApp.Proxy.Get<AdStrategyRuntimeProxy>()?.RecordInterAdPlayed();
			AdDailyTaskBridge.OnInterAdWatched();
		}
	}

	public void OnRewardAdFinished(RewardAdFinishedContext context)
	{
		int num;
		if (context.IsSuccess)
		{
			num = (context.IsFinished ? 1 : 0);
			if (num != 0)
			{
				AdDailyTaskBridge.OnRewardAdWatched();
			}
		}
		else
		{
			num = 0;
		}
		rewardAdStrategy?.OnRewardAdFinished(context);
		interAdStrategy?.OnRewardAdFinished(context);
		if (num != 0)
		{
			GameApp.Proxy.Get<AdStrategyRuntimeProxy>()?.RecordRewardAdPlayed(context.Level);
			ShopService.OnRewardAdFinished(context.Level);
		}
	}

	public void SetClosedInterstitial(bool isClosed)
	{
		interAdStrategy?.SetClosedInterstitial(isClosed);
	}

	public string BuildInterstitialBlockDiagnostics(IAdConfiguration config, string reason)
	{
		return interAdStrategy?.BuildInterstitialBlockDiagnostics(config, reason) ?? InterstitialEscapeTracker.BuildEscapeStateSummary();
	}

	public void NotifyRestartClicked(int level)
	{
		interAdStrategy?.NotifyRestartClicked(level);
	}

	public void NotifyMidGameExit(int level)
	{
		interAdStrategy?.NotifyMidGameExit(level);
	}

	public bool CanShowAd(string adType, IAdConfiguration config)
	{
		return true;
	}

	public void OnAdRequested(string adType, IAdConfiguration config)
	{
	}

	public void OnAdShown(string adType, IAdConfiguration config)
	{
	}

	public void OnAdClosed(string adType, AdResult result)
	{
	}
}
