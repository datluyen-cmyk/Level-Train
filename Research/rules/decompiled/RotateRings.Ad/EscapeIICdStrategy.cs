using RotateRings.Ad.Cd;
using XingYun.Ad;

namespace RotateRings.Ad;

public sealed class EscapeIICdStrategy : IInterstitialCooldownStrategy
{
	public InterstitialCdStrategyKind Kind => InterstitialCdStrategyKind.EscapeIntensity;

	public float GetCooldownSeconds(IAdConfiguration config)
	{
		return EscapeIntensityCdExperiment.ResolveCooldownSeconds();
	}

	public void CalculateCooldown()
	{
	}

	public string BuildCalculationLog()
	{
		return EscapeIntensityCdExperiment.BuildCalculationLog();
	}
}
