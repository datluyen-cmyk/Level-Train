namespace RotateRings.Ad;

public static class InterstitialExemptConsumeGate
{
	private const string Tag = "<Ad>[InterExemptGate] ";

	public static bool TryConsumeInterstitialExemption(bool isFrontAd, out string blockReason)
	{
		blockReason = null;
		string blockReason2;
		bool flag = RewardInterExemptExperiment.TryConsumeInterstitialExemption(isFrontAd, out blockReason2);
		string blockReason3;
		bool flag2 = PropWinInterExemptExperiment.TryConsumeInterstitialExemption(isFrontAd, out blockReason3);
		if (!flag && !flag2)
		{
			return false;
		}
		blockReason = (flag ? blockReason2 : blockReason3);
		return true;
	}
}
