namespace RotateRings.Ad.Strategy.Interstitial;

public abstract class InterstitialPolicyBase : IInterstitialPolicy
{
	public abstract string Name { get; }

	public abstract InterAdCheckResult Evaluate(InterstitialPolicyContext context);

	public virtual void OnInit()
	{
	}

	public virtual void OnInterstitialFinished(InterstitialFinishedContext context)
	{
	}

	public virtual void OnRewardAdFinished(RewardAdFinishedContext context)
	{
	}

	public virtual void OnRestartClicked(int level)
	{
	}

	public virtual void OnMidGameExit(int level)
	{
	}

	public virtual void ResetForDebug()
	{
	}

	public virtual string BuildDebugState()
	{
		return string.Empty;
	}
}
