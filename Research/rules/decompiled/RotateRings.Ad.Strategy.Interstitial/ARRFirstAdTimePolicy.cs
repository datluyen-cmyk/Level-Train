namespace RotateRings.Ad.Strategy.Interstitial;

public sealed class ARRFirstAdTimePolicy : InterstitialPolicyBase
{
	private const string Tag = "<Ad>.AccelerateRevenueRecovery";

	public override string Name => "AccelerateFirstAdTime";

	public static bool ShouldTakeOver()
	{
		return ARRExperiment.GetGroup() == 5;
	}

	public override InterAdCheckResult Evaluate(InterstitialPolicyContext context)
	{
		if (!ShouldTakeOver())
		{
			return InterAdCheckResult.Allow;
		}
		int firstInterstitialLevel = ARRExperiment.GetFirstInterstitialLevel();
		int num = (context.IsPostAd ? context.CompletedLevel : context.MainLevel);
		if (num >= firstInterstitialLevel)
		{
			return InterAdCheckResult.Allow;
		}
		return InterAdCheckResult.Block(AdBlockReason.FormatARRFirstInterstitialLevelBlocked(firstInterstitialLevel, num, context.IsPostAd));
	}

	public override string BuildDebugState()
	{
		return $"group={ARRExperiment.GetGroup()} threshold={ARRExperiment.GetFirstInterstitialLevel()} takeOver={ShouldTakeOver()}";
	}
}
