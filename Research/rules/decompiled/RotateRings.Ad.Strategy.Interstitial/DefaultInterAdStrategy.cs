using RotateRings.Ad.Cd;

namespace RotateRings.Ad.Strategy.Interstitial;

public class DefaultInterAdStrategy : InterAdStrategyBase
{
	protected override void RegisterGatePolicies(InterstitialPolicyPipeline pipeline)
	{
		pipeline.AddGate(new FrontAdVisibilityPolicy());
		pipeline.AddGate(new SkipNextLevelFrontAdPolicy());
		pipeline.AddGate(new FirstAdAppearCompositePolicy());
		pipeline.AddGate(new InterstitialFirstAdTimePolicy());
		pipeline.AddGate(new InterstitialExemptionPolicy());
	}

	protected override void RegisterConsumePolicies(InterstitialPolicyPipeline pipeline)
	{
		base.RegisterConsumePolicies(pipeline);
		pipeline.AddConsume(new SetbackExemptionPolicy());
	}

	protected override InterstitialCdStrategyKind GetCooldownKind()
	{
		return InterstitialCdStrategyFactory.ResolveActiveKind();
	}

	protected override string BuildDiagnosticsPrefix()
	{
		return "escapeState=" + InterstitialEscapeTracker.BuildEscapeStateSummary();
	}
}
