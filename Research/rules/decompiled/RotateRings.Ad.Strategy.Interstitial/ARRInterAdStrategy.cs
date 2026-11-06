using RotateRings.Ad.Cd;
using XingYun.Ad;

namespace RotateRings.Ad.Strategy.Interstitial;

public sealed class ARRInterAdStrategy : InterAdStrategyBase
{
	protected override void RegisterGatePolicies(InterstitialPolicyPipeline pipeline)
	{
		pipeline.AddGate(new ARRFrontAdPolicy());
		pipeline.AddGate(new ARRChannelExemptionPolicy());
		pipeline.AddGate(new ARRRewardExemptionPolicy());
		pipeline.AddGate(new ARRGameDurationPolicy());
	}

	protected override InterstitialCdStrategyKind GetCooldownKind()
	{
		return InterstitialCdStrategyKind.ARR;
	}

	protected override string BuildDiagnosticsPrefix()
	{
		return $"accelerateGroup={ARRExperiment.GetGroup()}";
	}

	protected override void OnEvaluateCompleted(IAdConfiguration config, bool isFrontAd, string stage, InterAdCheckResult result)
	{
	}
}
