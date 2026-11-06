using RotateRings.Ad.Cd;
using XingYun.Ad;

namespace RotateRings.Ad;

public sealed class ARRInterstitialCooldownStrategy : IInterstitialCooldownStrategy
{
	public InterstitialCdStrategyKind Kind => InterstitialCdStrategyKind.ARR;

	public float GetCooldownSeconds(IAdConfiguration config)
	{
		return 0f;
	}

	public void CalculateCooldown()
	{
	}

	public string BuildCalculationLog()
	{
		return "--- 插屏 CD 运算 ---\n策略=加快收入回收策略固定 CD=0s（每关结算都展示后插屏）";
	}
}
