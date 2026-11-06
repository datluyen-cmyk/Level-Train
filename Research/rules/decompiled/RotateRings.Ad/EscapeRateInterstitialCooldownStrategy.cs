using RotateRings.Ad.Cd;
using XingYun.Ad;

namespace RotateRings.Ad;

public sealed class EscapeRateInterstitialCooldownStrategy : IInterstitialCooldownStrategy
{
	public InterstitialCdStrategyKind Kind => InterstitialCdStrategyKind.EscapeRate;

	public float GetCooldownSeconds(IAdConfiguration config)
	{
		return InterstitialEscapeTracker.ResolveCooldownSeconds();
	}

	public void CalculateCooldown()
	{
	}

	public string BuildCalculationLog()
	{
		return InterstitialEscapeTracker.BuildEscapeRateCooldownCalculationLog();
	}
}
