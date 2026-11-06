using XingYun.Ad;

namespace RotateRings.Ad.Strategy;

public interface IInterAdStrategy
{
	InterAdCheckResult CanShowInter(IAdConfiguration config);

	void InitInterstitial(long lastInterAdTimestamp);

	void StartInterstitialCooldown();

	void OnInterstitialFinished(InterstitialFinishedContext context);

	void OnRewardAdFinished(RewardAdFinishedContext context);

	void SetClosedInterstitial(bool isClosed);

	string BuildInterstitialBlockDiagnostics(IAdConfiguration config, string reason);

	void NotifyRestartClicked(int level);

	void NotifyMidGameExit(int level);

	void ClearInterstitialCooldown();

	void RecalculateInterstitialCooldown();

	void RebindInterstitialCooldownStrategy();

	void ResetExperimentPolicyStateForDebug();

	string BuildExperimentPolicyDiagnostics();
}
