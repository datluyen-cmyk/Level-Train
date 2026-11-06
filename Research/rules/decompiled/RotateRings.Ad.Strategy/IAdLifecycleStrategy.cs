namespace RotateRings.Ad.Strategy;

public interface IAdLifecycleStrategy
{
	void OnInterstitialFinished(InterstitialFinishedContext context);

	void OnRewardAdFinished(RewardAdFinishedContext context);
}
