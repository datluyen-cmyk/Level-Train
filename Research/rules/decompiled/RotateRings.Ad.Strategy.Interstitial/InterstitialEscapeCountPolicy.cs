namespace RotateRings.Ad.Strategy.Interstitial;

public sealed class InterstitialEscapeCountPolicy : InterstitialPolicyBase
{
	public override string Name => "EscapeCount";

	public override InterAdCheckResult Evaluate(InterstitialPolicyContext context)
	{
		InterstitialEscapeRecord orCreate = GameApp.Save.GetOrCreate<InterstitialEscapeRecord>();
		if (orCreate.EscapeSkipCount <= 0)
		{
			return InterAdCheckResult.Allow;
		}
		int num = ((orCreate.EscapeSkipBatchTotal > 0) ? orCreate.EscapeSkipBatchTotal : orCreate.EscapeSkipCount);
		int consumedIndex = num - orCreate.EscapeSkipCount + 1;
		orCreate.EscapeSkipCount--;
		if (orCreate.EscapeSkipCount <= 0)
		{
			orCreate.EscapeSkipCount = 0;
			orCreate.EscapeSkipBatchTotal = 0;
		}
		InterstitialEscapeTracker.LogEscapeCountSkipApplied(context.IsFrontAd, orCreate.EscapeSkipCount, num, consumedIndex);
		return InterAdCheckResult.Block("逃逸免插屏次数生效");
	}
}
