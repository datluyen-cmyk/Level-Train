namespace RotateRings.Ad.Strategy.Interstitial;

public sealed class ARRChannelExemptionPolicy : InterstitialPolicyBase
{
	public override string Name => "AccelerateChannelExemption";

	public override InterAdCheckResult Evaluate(InterstitialPolicyContext context)
	{
		int installDays = AdBridgeHelper.GetInstallDays();
		if (!ARRExperiment.ShouldExemptByChannel(context.IsFrontAd, context.MainLevel, context.CompletedLevel, installDays))
		{
			return InterAdCheckResult.Allow;
		}
		return InterAdCheckResult.Block("ARR策略渠道免插屏");
	}

	public override string BuildDebugState()
	{
		return $"group={ARRExperiment.GetGroup()} preinstall={PreinstallChannelHelper.IsCurrentUserPreinstall()} installDays={AdBridgeHelper.GetInstallDays()}";
	}
}
