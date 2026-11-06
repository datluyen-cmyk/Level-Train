using XingYun.Ad;

namespace RotateRings.Ad.Strategy;

public interface IRewardAdStrategy : IRewardAdLifecycle
{
	RewardAdCheckResult CanShowReward(IAdConfiguration config);
}
