using System.Globalization;
using System.Text;
using RotateRings.Ad.Cd;
using RotateRings.Ad.CommerceLtv;
using XingYun.Ad;

namespace RotateRings.Ad;

public sealed class CommerceLtvInterstitialCooldownStrategy : IInterstitialCooldownStrategy
{
	private readonly int experimentGroup;

	public InterstitialCdStrategyKind Kind => InterstitialCdStrategyKind.CommerceLtv;

	public float MinCooldownSeconds { get; private set; }

	public float MaxCooldownSeconds { get; private set; }

	public float PRewardReverseValue { get; private set; }

	public float PR7Value { get; private set; }

	public float ToleranceScore { get; private set; }

	public float SigmoidValue { get; private set; }

	public float FinalCooldownSeconds { get; private set; }

	public CommerceLtvInterstitialCooldownStrategy(int group)
	{
		experimentGroup = group;
		RefreshInstallDayBounds();
		CalculateCooldown();
	}

	public float GetCooldownSeconds(IAdConfiguration config)
	{
		return FinalCooldownSeconds;
	}

	public void CalculateCooldown()
	{
		RefreshInstallDayBounds();
		float rewardExposureSumX = CommerceLtvToleranceTracker.GetRewardExposureSumX();
		PRewardReverseValue = AdToleranceLookup.PRewardReverse(rewardExposureSumX);
		PR7Value = CommerceLtvToleranceTracker.ComputePR7();
		ToleranceScore = 0.4f * PRewardReverseValue + 0.6f * PR7Value;
		AdToleranceLookup.ComputeCooldownSeconds(MinCooldownSeconds, MaxCooldownSeconds, ToleranceScore, out var sigmoidValue, out var finalCdSeconds);
		SigmoidValue = sigmoidValue;
		FinalCooldownSeconds = finalCdSeconds;
	}

	public bool TryHighPriceInstantPass(float elapsedSinceLastCloseSeconds)
	{
		if (elapsedSinceLastCloseSeconds < MinCooldownSeconds)
		{
			return false;
		}
		double eBase = CommerceLtvToleranceTracker.GetEBase();
		double eMax = CommerceInterCacheEcpmSession.GetEMax();
		float num = AdToleranceLookup.NMultiplier(InterstitialEscapeTracker.SuccessfulInterstitialCount);
		if (eBase <= 0.0 || eMax <= 0.0)
		{
			return false;
		}
		return eMax >= eBase * (double)num;
	}

	public string BuildCalculationLog()
	{
		float rewardExposureSumX = CommerceLtvToleranceTracker.GetRewardExposureSumX();
		double eBase = CommerceLtvToleranceTracker.GetEBase();
		double eMax = CommerceInterCacheEcpmSession.GetEMax();
		int successfulInterstitialCount = InterstitialEscapeTracker.SuccessfulInterstitialCount;
		float num = AdToleranceLookup.NMultiplier(successfulInterstitialCount);
		StringBuilder stringBuilder = new StringBuilder(1024);
		stringBuilder.AppendLine("--- 插屏 CD 运算 ---");
		stringBuilder.AppendLine("策略=CommerceLtv（InterstitialFunction.CommerceLTVRatio）");
		stringBuilder.Append("group=").Append(experimentGroup).Append(" installDays=")
			.Append(AdBridgeHelper.GetInstallDays())
			.Append(" minCd=")
			.Append(MinCooldownSeconds.ToString(CultureInfo.InvariantCulture))
			.Append(" maxCd=")
			.Append(MaxCooldownSeconds.ToString(CultureInfo.InvariantCulture))
			.AppendLine();
		stringBuilder.Append("x=").Append(rewardExposureSumX.ToString("F1", CultureInfo.InvariantCulture)).Append(" p_reward_reverse=")
			.Append(PRewardReverseValue.ToString("F4", CultureInfo.InvariantCulture))
			.Append(" p_r7=")
			.Append(PR7Value.ToString("F4", CultureInfo.InvariantCulture))
			.AppendLine();
		stringBuilder.Append("tolerance=0.4*p_reward+0.6*p_r7=").Append(ToleranceScore.ToString("F4", CultureInfo.InvariantCulture)).Append(" sigmoid=")
			.Append(SigmoidValue.ToString("F4", CultureInfo.InvariantCulture))
			.Append(" finalCd=")
			.Append(FinalCooldownSeconds.ToString("F1", CultureInfo.InvariantCulture))
			.Append('s')
			.AppendLine();
		stringBuilder.Append("e_base=").Append(eBase.ToString("F4", CultureInfo.InvariantCulture)).Append(" e_max=")
			.Append(eMax.ToString("F4", CultureInfo.InvariantCulture))
			.Append(" lifetime_inter=")
			.Append(successfulInterstitialCount)
			.Append(" n=")
			.Append(num.ToString("F2", CultureInfo.InvariantCulture))
			.AppendLine();
		stringBuilder.Append("高价即时: e_max>=e_base*n 且 elapsed>=min_cd（在完整 CD 判断前）");
		return stringBuilder.ToString();
	}

	private void RefreshInstallDayBounds()
	{
		CommerceLtvCdMatrix.Resolve(experimentGroup, AdBridgeHelper.GetInstallDays(), out var minCd, out var maxCd);
		MinCooldownSeconds = minCd;
		MaxCooldownSeconds = maxCd;
	}
}
