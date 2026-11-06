using RotateRings.Ad.CommerceLtv;
using XingYun.Ad;

namespace RotateRings.Ad.Strategy.Reward;

public class DefaultRewardAdStrategy : AdStrategyBase, IRewardAdStrategy, IRewardAdLifecycle
{
	public RewardAdCheckResult CanShowReward(IAdConfiguration config)
	{
		return RewardAdCheckResult.Allow;
	}

	public void OnRewardAdFinished(RewardAdFinishedContext context)
	{
		if (context.IsSuccess && context.IsFinished)
		{
			InterstitialEscapeTracker.OnRewardAdFinished();
			CommerceLtvCooldownHooks.OnRewardAdFinished();
			RewardInterExemptExperiment.OnRewardAdFinished(context.AdPosition);
		}
	}
}
