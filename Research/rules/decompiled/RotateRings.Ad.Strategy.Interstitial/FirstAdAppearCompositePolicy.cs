namespace RotateRings.Ad.Strategy.Interstitial;

public sealed class FirstAdAppearCompositePolicy : InterstitialPolicyBase
{
	private readonly SpecialChanelFirstAdPolicy specialChanelFirstAdPolicy = new SpecialChanelFirstAdPolicy();

	private readonly NormalFirstAdPolicy normalFirstAdPolicy = new NormalFirstAdPolicy();

	private readonly ARRFirstAdTimePolicy arrFirstAdTimePolicy = new ARRFirstAdTimePolicy();

	private readonly GpPreinstallFirstAdTimePolicy gpPreinstallFirstAdTimePolicy = new GpPreinstallFirstAdTimePolicy();

	public override string Name => "FirstAdAppear";

	public override void OnInit()
	{
		specialChanelFirstAdPolicy.OnInit();
		normalFirstAdPolicy.OnInit();
	}

	public override void OnInterstitialFinished(InterstitialFinishedContext context)
	{
		specialChanelFirstAdPolicy.OnInterstitialFinished(context);
		normalFirstAdPolicy.OnInterstitialFinished(context);
		arrFirstAdTimePolicy.OnInterstitialFinished(context);
		gpPreinstallFirstAdTimePolicy.OnInterstitialFinished(context);
	}

	public override void OnRewardAdFinished(RewardAdFinishedContext context)
	{
		specialChanelFirstAdPolicy.OnRewardAdFinished(context);
		normalFirstAdPolicy.OnRewardAdFinished(context);
		arrFirstAdTimePolicy.OnRewardAdFinished(context);
		gpPreinstallFirstAdTimePolicy.OnRewardAdFinished(context);
	}

	public override void OnRestartClicked(int level)
	{
		specialChanelFirstAdPolicy.OnRestartClicked(level);
		normalFirstAdPolicy.OnRestartClicked(level);
		arrFirstAdTimePolicy.OnRestartClicked(level);
		gpPreinstallFirstAdTimePolicy.OnRestartClicked(level);
	}

	public override void ResetForDebug()
	{
		specialChanelFirstAdPolicy.ResetForDebug();
		normalFirstAdPolicy.ResetForDebug();
		arrFirstAdTimePolicy.ResetForDebug();
		gpPreinstallFirstAdTimePolicy.ResetForDebug();
	}

	public override string BuildDebugState()
	{
		if (ARRFirstAdTimePolicy.ShouldTakeOver())
		{
			return "active=ARRFirstAdTime " + arrFirstAdTimePolicy.BuildDebugState();
		}
		if (GpPreinstallFirstAdTimePolicy.ShouldTakeOver())
		{
			return $"active=GpPreinstallFirstAdTime scheme={GpPreinstallFirstAdTimePolicy.GetConfiguredScheme()}";
		}
		if (SpecialChanelFirstAdPolicy.ShouldTakeOverLevelGate(AdBridgeHelper.GetEffectiveChannel()))
		{
			return "active=SpecialChanel " + specialChanelFirstAdPolicy.BuildDebugState();
		}
		return "active=FirstDay " + normalFirstAdPolicy.BuildDebugState();
	}

	public override InterAdCheckResult Evaluate(InterstitialPolicyContext context)
	{
		if (GpPreinstallFirstAdTimePolicy.ShouldTakeOver())
		{
			return gpPreinstallFirstAdTimePolicy.Evaluate(context);
		}
		if (ARRFirstAdTimePolicy.ShouldTakeOver())
		{
			return arrFirstAdTimePolicy.Evaluate(context);
		}
		if (SpecialChanelFirstAdPolicy.ShouldTakeOverLevelGate(context.Channel))
		{
			return specialChanelFirstAdPolicy.Evaluate(context);
		}
		InterAdCheckResult result = normalFirstAdPolicy.Evaluate(context);
		if (!result.Allowed)
		{
			return InterAdCheckResult.Block("安装首日插屏策略不通过");
		}
		return result;
	}
}
