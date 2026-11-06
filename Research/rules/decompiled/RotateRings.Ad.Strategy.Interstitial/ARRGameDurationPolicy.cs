namespace RotateRings.Ad.Strategy.Interstitial;

public sealed class ARRGameDurationPolicy : InterstitialPolicyBase
{
	private const string Tag = "<Ad>.AccelerateRevenueRecovery";

	public override string Name => "AccelerateGameDuration";

	public override InterAdCheckResult Evaluate(InterstitialPolicyContext context)
	{
		if (context.IsFrontAd)
		{
			return InterAdCheckResult.Allow;
		}
		if (!ARRExperiment.ShouldExemptByGameDuration())
		{
			return InterAdCheckResult.Allow;
		}
		return InterAdCheckResult.Block("加快收入回收：ARR游戏时长不足免后插屏");
	}

	public override string BuildDebugState()
	{
		return $"group={ARRExperiment.GetGroup()} coldLaunchDuration={AdBridgeHelper.GetEffectiveColdLaunchValidDurationSeconds():F0}s";
	}
}
