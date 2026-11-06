namespace RotateRings.Ad.Strategy.Interstitial;

public interface IInterstitialPolicy
{
	string Name { get; }

	InterAdCheckResult Evaluate(InterstitialPolicyContext context);

	void OnInit();

	void OnInterstitialFinished(InterstitialFinishedContext context);

	void OnRewardAdFinished(RewardAdFinishedContext context);

	void OnRestartClicked(int level);

	void OnMidGameExit(int level);

	void ResetForDebug();

	string BuildDebugState();
}
