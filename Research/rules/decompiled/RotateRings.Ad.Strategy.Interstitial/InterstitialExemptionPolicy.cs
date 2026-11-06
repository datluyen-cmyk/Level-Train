using XingYun.Experiment;

namespace RotateRings.Ad.Strategy.Interstitial;

public sealed class InterstitialExemptionPolicy : InterstitialPolicyBase
{
	private const string Tag = "<Ad>.AdditionalExemption";

	public override string Name => "AdditionalExemption";

	public int GetConfiguredExperimentId()
	{
		return FunctionDataCenter.Get<InterstitialFunction>()?.AdditionalExemptionLTV ?? 0;
	}

	public static bool IsSupported(int experimentId)
	{
		bool num = InterstitialExemptionMatrix.IsSupportedExperiment(experimentId);
		if (!num)
		{
		}
		return num;
	}

	public override InterAdCheckResult Evaluate(InterstitialPolicyContext context)
	{
		int configuredExperimentId = GetConfiguredExperimentId();
		if (!IsSupported(configuredExperimentId))
		{
			return InterAdCheckResult.Allow;
		}
		return Evaluate(configuredExperimentId);
	}

	public InterAdCheckResult Evaluate(int experimentId)
	{
		int installDays = AdBridgeHelper.GetInstallDays();
		if (!InterstitialExemptionMatrix.TryGetCurrentExemptionSeconds(experimentId, installDays, out var exemptionSeconds))
		{
			return InterAdCheckResult.Allow;
		}
		if (exemptionSeconds == -1)
		{
			return InterAdCheckResult.Allow;
		}
		float effectiveLaunchDuration = AdBridgeHelper.GetEffectiveLaunchDuration();
		if (!(effectiveLaunchDuration < (float)exemptionSeconds))
		{
			return InterAdCheckResult.Allow;
		}
		return InterAdCheckResult.Block(AdBlockReason.FormatInterstitialExemptionBlocked(experimentId, exemptionSeconds, effectiveLaunchDuration));
	}
}
