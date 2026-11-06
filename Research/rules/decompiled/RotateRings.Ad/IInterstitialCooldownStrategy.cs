using RotateRings.Ad.Cd;
using XingYun.Ad;

namespace RotateRings.Ad;

public interface IInterstitialCooldownStrategy
{
	InterstitialCdStrategyKind Kind { get; }

	float GetCooldownSeconds(IAdConfiguration config);

	void CalculateCooldown();
}
