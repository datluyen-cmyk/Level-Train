namespace RotateRings.Ad.Strategy.Interstitial;

public sealed class ARRFrontAdPolicy : InterstitialPolicyBase
{
	public override string Name => "AccelerateFrontAd";

	public override InterAdCheckResult Evaluate(InterstitialPolicyContext context)
	{
		if (!context.IsFrontAd)
		{
			return InterAdCheckResult.Allow;
		}
		if (!ARRExperiment.ShouldRemoveFrontAd())
		{
			return InterAdCheckResult.Allow;
		}
		return InterAdCheckResult.Block("ARR策略删除前插屏");
	}

	public override string BuildDebugState()
	{
		int num = ARRExperiment.GetGroup();
		return $"group={num} removeFrontAd={ARRExperiment.ShouldRemoveFrontAd()}";
	}
}
