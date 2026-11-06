using System.Text;
using RotateRings.Ad.Cd;
using XingYun.Ad;

namespace RotateRings.Ad;

public class FixedInterstitialCooldownStrategy : IInterstitialCooldownStrategy
{
	public InterstitialCdStrategyKind Kind => InterstitialCdStrategyKind.Fixed;

	public float GetCooldownSeconds(IAdConfiguration config)
	{
		return 70f;
	}

	public void CalculateCooldown()
	{
	}

	public string BuildCalculationLog()
	{
		StringBuilder stringBuilder = new StringBuilder(256);
		stringBuilder.AppendLine("--- 插屏 CD 运算 ---");
		stringBuilder.AppendLine("策略=固定CD（AdBaseboardDefaults.InsertCd）").Append(" InsertCd=").Append(70)
			.Append('s')
			.AppendLine();
		stringBuilder.Append("finalCd=").Append(70).Append('s');
		stringBuilder.AppendLine();
		stringBuilder.Append("门控=elapsed>=interval（完整间隔，无 GP39 70% 提前放行）");
		return stringBuilder.ToString();
	}
}
