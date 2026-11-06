using XingYun.Experiment;

namespace RotateRings.Ad.Strategy.Interstitial;

public sealed class InterstitialFirstAdTimePolicy : InterstitialPolicyBase
{
	private const string Tag = "<Ad>.InterstitialFirstAdTimePolicy";

	public override string Name => "FirstAdTimeLTV";

	public int GetConfiguredExperimentId()
	{
		return FunctionDataCenter.Get<InterstitialFunction>()?.FirstAdEqulaTimeLTV ?? 0;
	}

	public static bool IsSupported(int experimentId)
	{
		bool num = InterstitialFirstAdTimeMatrix.IsSupportedExperiment(experimentId);
		if (!num)
		{
		}
		return num;
	}

	public override InterAdCheckResult Evaluate(InterstitialPolicyContext context)
	{
		if (GpPreinstallFirstAdTimePolicy.ShouldTakeOver())
		{
			return InterAdCheckResult.Allow;
		}
		int configuredExperimentId = GetConfiguredExperimentId();
		if (!IsSupported(configuredExperimentId))
		{
			return InterAdCheckResult.Allow;
		}
		return Evaluate(configuredExperimentId);
	}

	public InterAdCheckResult Evaluate(int experimentId)
	{
		if (InterstitialEscapeTracker.HasPlayedInterstitial)
		{
			return InterAdCheckResult.Allow;
		}
		int installDays = AdBridgeHelper.GetInstallDays();
		if (!InterstitialFirstAdTimeMatrix.TryGetCurrentThresholdSeconds(experimentId, installDays, out var requiredSeconds))
		{
			return InterAdCheckResult.Allow;
		}
		float effectiveTotalValidDurationSeconds = AdBridgeHelper.GetEffectiveTotalValidDurationSeconds();
		if (!(effectiveTotalValidDurationSeconds < (float)requiredSeconds))
		{
			return InterAdCheckResult.Allow;
		}
		return InterAdCheckResult.Block(AdBlockReason.FormatInterstitialFirstAdTimeBlocked(experimentId, requiredSeconds, effectiveTotalValidDurationSeconds));
	}
}
