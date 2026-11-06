using System;
using System.Collections.Generic;
using System.Text;
using RotateRings.Ad.Cd;
using RotateRings.Model;
using UnityEngine;
using XingYun.Ad;
using XingYun.Aot;
using XingYun.Tag;

namespace RotateRings.Ad;

public class Gp39InterstitialCooldownStrategy : IInterstitialCooldownStrategy
{
	private const string Tag = "<Ad>";

	private const float AdjustedCdIncrement = 20f;

	public float MinCooldownSeconds;

	public float MaxCooldownSeconds;

	public InterstitialCdStrategyKind Kind => InterstitialCdStrategyKind.Gp39;

	public float AdjustedMinCooldownSeconds => MinCooldownSeconds + ((InterstitialEscapeTracker.SessionInterstitialPlayCount >= 5) ? 20f : 0f);

	public float ColdStartLevelPercentile { get; private set; }

	public float EscapeRatePercentile { get; private set; }

	public float EcpmPercentile { get; private set; }

	public float ToleranceScore { get; private set; }

	public float SigmoidTolerance { get; private set; }

	public float FinalCooldownSeconds { get; private set; }

	public Gp39InterstitialCooldownStrategy()
	{
		CalculateCooldown();
	}

	public float GetCooldownSeconds(IAdConfiguration config)
	{
		return FinalCooldownSeconds;
	}

	public void CalculateCooldown()
	{
		int installDays = GetInstallDays();
		ApplyInstallDayCooldownRange(installDays);
		ColdStartLevelPercentile = ComputeColdStartLevelPercentile();
		EscapeRatePercentile = ComputeEscapeRatePercentile();
		EcpmPercentile = ComputeEcpmPercentile();
		ToleranceScore = 0.3f * ColdStartLevelPercentile + 0.4f * EscapeRatePercentile + 0.3f * EcpmPercentile;
		SigmoidTolerance = 1f / (1f + Mathf.Exp(-5f * (ToleranceScore - 0.5f)));
		FinalCooldownSeconds = AdjustedMinCooldownSeconds + (MaxCooldownSeconds - AdjustedMinCooldownSeconds) * (1f - SigmoidTolerance);
	}

	public string BuildCalculationLog()
	{
		InterstitialEscapeRecord orCreate = GameApp.Save.GetOrCreate<InterstitialEscapeRecord>();
		string text = CountryGdpGroupResolver.ResolveEffectiveGdpGroup();
		List<double> last3InterIncomeList = GameApp.Tag.Last3InterIncomeList;
		double num;
		if (last3InterIncomeList == null || last3InterIncomeList.Count <= 0)
		{
			num = 0.0;
		}
		else
		{
			num = last3InterIncomeList[last3InterIncomeList.Count - 1];
		}
		double lastEcpm = num;
		int totalCallCount = orCreate.TotalCallCount;
		int installDays = GetInstallDays();
		float value = ((InterstitialEscapeTracker.SessionInterstitialPlayCount >= 5) ? 20f : 0f);
		float num2 = 0.3f * ColdStartLevelPercentile;
		float num3 = 0.4f * EscapeRatePercentile;
		float num4 = 0.3f * EcpmPercentile;
		float num5 = -5f * (ToleranceScore - 0.5f);
		float num6 = Mathf.Exp(num5);
		float num7 = MaxCooldownSeconds - AdjustedMinCooldownSeconds;
		float num8 = 1f - SigmoidTolerance;
		StringBuilder stringBuilder = new StringBuilder(1536);
		stringBuilder.AppendLine("--- 插屏 CD 运算 ---");
		stringBuilder.Append("installDays=").Append(installDays).Append(" => minCd=")
			.Append(MinCooldownSeconds)
			.Append(" maxCd=")
			.Append(MaxCooldownSeconds)
			.AppendLine();
		stringBuilder.Append("sessionPlayCount=").Append(InterstitialEscapeTracker.SessionInterstitialPlayCount).Append(" sessionBonus=")
			.Append(value)
			.Append("(session>=5时+20)")
			.AppendLine();
		stringBuilder.Append("adjustedMinCd = minCd + sessionBonus = ").Append(MinCooldownSeconds).Append('+')
			.Append(value)
			.Append('=')
			.Append(AdjustedMinCooldownSeconds)
			.AppendLine();
		stringBuilder.Append(BuildColdStartLevelCalculationDetail()).AppendLine();
		stringBuilder.Append("冷启关卡分位 coldStartLevelPercentile=").Append(ColdStartLevelPercentile.ToString("F4")).AppendLine();
		stringBuilder.Append(BuildEscapeRateCalculationDetail()).AppendLine();
		stringBuilder.Append("逃逸率分位 escapeRatePercentile=").Append(EscapeRatePercentile.ToString("F4")).AppendLine();
		stringBuilder.Append(BuildEcpmPercentileCalculationDetail(text, lastEcpm, totalCallCount)).AppendLine();
		stringBuilder.Append("eCPM分位 ecpmPercentile=").Append(EcpmPercentile.ToString("F4")).Append(" (gdpGroup=")
			.Append(text)
			.Append(" lastEcpm=")
			.Append(lastEcpm.ToString("F4"))
			.Append(" playCount=")
			.Append(totalCallCount)
			.AppendLine(")");
		stringBuilder.Append("容忍度 toleranceScore = 0.3*coldStart + 0.4*escape + 0.3*ecpm = ").Append("0.3*").Append(ColdStartLevelPercentile.ToString("F4"))
			.Append("+0.4*")
			.Append(EscapeRatePercentile.ToString("F4"))
			.Append("+0.3*")
			.Append(EcpmPercentile.ToString("F4"))
			.Append('=')
			.Append(num2.ToString("F4"))
			.Append('+')
			.Append(num3.ToString("F4"))
			.Append('+')
			.Append(num4.ToString("F4"))
			.Append('=')
			.Append(ToleranceScore.ToString("F4"))
			.AppendLine();
		stringBuilder.Append("Sigmoid: exp(-5*(tolerance-0.5)) = exp(").Append(num5.ToString("F4")).Append(")=")
			.Append(num6.ToString("F4"))
			.Append(" => 1/(1+exp)=")
			.Append(SigmoidTolerance.ToString("F4"))
			.AppendLine();
		stringBuilder.Append("finalCd = adjustedMin + (max-adjustedMin)*(1-sigmoid) = ").Append(AdjustedMinCooldownSeconds.ToString("F1")).Append('+')
			.Append('(')
			.Append(MaxCooldownSeconds.ToString("F1"))
			.Append('-')
			.Append(AdjustedMinCooldownSeconds.ToString("F1"))
			.Append(')')
			.Append('*')
			.Append(num8.ToString("F4"))
			.Append('=')
			.Append(AdjustedMinCooldownSeconds.ToString("F1"))
			.Append('+')
			.Append(num7.ToString("F1"))
			.Append('*')
			.Append(num8.ToString("F4"))
			.Append('=')
			.Append(FinalCooldownSeconds.ToString("F1"))
			.Append('s')
			.AppendLine();
		stringBuilder.Append("CD 70% 提前放行: elapsed>=interval*0.7 且 GetAdWorth()= ").Append(ColdStartEcpmTracker.GetAdWorth()).AppendLine();
		return stringBuilder.ToString();
	}

	private string BuildColdStartLevelCalculationDetail()
	{
		PlayerModelData playerModelData = GameApp.Proxy.Get<PlayerModelProxy>()?.Data;
		int num = playerModelData?.ColdLaunchCount ?? 1;
		if (num <= 1)
		{
			return $"冷启关卡运算: coldLaunchCount={num}<=1 => 默认 coldStartLevelPercentile=0.27";
		}
		List<int> list = playerModelData?.RecentColdStartLevelCounts;
		string arg = ((list == null || list.Count == 0) ? "recent=[]" : ("recent=[" + string.Join(",", list) + "]"));
		float num2 = playerModelData?.AverageRecentColdStartLevels ?? 0f;
		if (num2 <= 0f)
		{
			return $"冷启关卡运算: {arg} avgRecent={num2:F4} => 0";
		}
		string arg2;
		if (num2 <= 1f)
		{
			float num3 = num2 / 1f;
			float num4 = 0.27f + 0.07f * (1f - Mathf.Exp(-3f * num3));
			arg2 = $"分段[0,1] t=x/1={num3:F4} => 0.27+0.07*(1-exp(-3*t))={num4:F4}";
		}
		else if (num2 <= 9f)
		{
			float num3 = (num2 - 1f) / 8f;
			float num4 = 0.34f + 0.56f * (Mathf.Log(1f + 4f * num3) / Mathf.Log(5f));
			arg2 = $"分段(1,9] t=(x-1)/8={num3:F4} => 0.34+0.56*log(1+4t)/log(5)={num4:F4}";
		}
		else if (num2 <= 26f)
		{
			float num3 = (num2 - 9f) / 17f;
			float num4 = 0.9f + 0.09f * (1f - Mathf.Exp(-5f * num3));
			arg2 = $"分段(9,26] t=(x-9)/17={num3:F4} => 0.90+0.09*(1-exp(-5*t))={num4:F4}";
		}
		else
		{
			float num3 = num2 - 26f;
			float num4 = Mathf.Min(0.99f + 0.01f * (1f - Mathf.Exp(-0.1f * num3)), 1f);
			arg2 = $"分段(26,+] t=x-26={num3:F4} => min(0.99+0.01*(1-exp(-0.1*t)),1)={num4:F4}";
		}
		return $"冷启关卡运算: {arg} x=avgRecent={num2:F4} {arg2}";
	}

	private string BuildEcpmPercentileCalculationDetail(string gdpGroup, double lastEcpm, int playCount)
	{
		string text = ((playCount <= 4) ? "冷启动表(playCount<=4)" : "已建立表(playCount>4)");
		float num = EcpmPercentileLookup.Lookup(gdpGroup, lastEcpm, playCount);
		string arg = EcpmPercentileLookup.BuildLookupDetail(gdpGroup, lastEcpm, playCount);
		return $"eCPM分位运算: {text} gdpGroup={gdpGroup} lastEcpm={lastEcpm:F4} playCount={playCount} " + $"{arg} => ecpmPercentile={num:F4}";
	}

	private string BuildEscapeRateCalculationDetail()
	{
		Last7AdPlayInfoTag last7AdPlayInfo = GameApp.Tag.Last7AdPlayInfo;
		if (last7AdPlayInfo?.last7_inter_ad_results == null || last7AdPlayInfo.last7_inter_ad_results.Count == 0)
		{
			return "逃逸率运算: last7无数据 => r=0 => escapeRatePercentile=0";
		}
		List<bool> last7_inter_ad_results = last7AdPlayInfo.last7_inter_ad_results;
		int count = last7_inter_ad_results.Count;
		int num = ((count < 3) ? count : 3);
		int num2 = 0;
		for (int i = count - num; i < count; i++)
		{
			if (last7_inter_ad_results[i])
			{
				num2++;
			}
		}
		float num3 = (float)num2 / 3f;
		float num4 = ((num3 <= 0f) ? 0f : ((num3 <= 0.34f) ? 0.21f : ((num3 <= 0.51f) ? 0.31f : ((num3 <= 0.67f) ? 0.4f : 1f))));
		return $"逃逸率运算: last7近{num}次非逃逸数={num2} => r={num2}/3={num3:F4} " + $"=> 映射(r<=0.34=>0.21, <=0.51=>0.31, <=0.67=>0.4, else=>1.0)={num4:F4}";
	}

	protected virtual float GetInstallDayMinCdBonus(int installDays)
	{
		return 0f;
	}

	protected virtual void ApplyInstallDayCooldownRange(int installDays)
	{
		if (installDays >= 14)
		{
			MinCooldownSeconds = 60f;
			MaxCooldownSeconds = 300f;
		}
		else if (installDays >= 7)
		{
			MinCooldownSeconds = 90f;
			MaxCooldownSeconds = 360f;
		}
		else if (installDays >= 2)
		{
			MinCooldownSeconds = 105f;
			MaxCooldownSeconds = 390f;
		}
		else
		{
			MinCooldownSeconds = 120f;
			MaxCooldownSeconds = 420f;
		}
	}

	private static int GetInstallDays()
	{
		try
		{
			return SDKManager.GetInstallDays();
		}
		catch (Exception)
		{
			return 1;
		}
	}

	private float ComputeColdStartLevelPercentile()
	{
		PlayerModelData playerModelData = GameApp.Proxy.Get<PlayerModelProxy>()?.Data;
		if ((playerModelData?.ColdLaunchCount ?? 1) <= 1)
		{
			return 0.27f;
		}
		float num = playerModelData?.AverageRecentColdStartLevels ?? 0f;
		List<int> list = playerModelData?.RecentColdStartLevelCounts;
		if (list != null && list.Count != 0)
		{
			_ = "[" + string.Join(",", list) + "]";
		}
		if (num <= 0f)
		{
			return 0f;
		}
		float result;
		if (num <= 1f)
		{
			float num2 = num / 1f;
			result = 0.27f + 0.07f * (1f - Mathf.Exp(-3f * num2));
			_ = $"[0,1] t={num2:F4}";
		}
		else if (num <= 9f)
		{
			float num3 = (num - 1f) / 8f;
			result = 0.34f + 0.56f * (Mathf.Log(1f + 4f * num3) / Mathf.Log(5f));
			_ = $"(1,9] t={num3:F4}";
		}
		else if (num <= 26f)
		{
			float num4 = (num - 9f) / 17f;
			result = 0.9f + 0.09f * (1f - Mathf.Exp(-5f * num4));
			_ = $"(9,26] t={num4:F4}";
		}
		else
		{
			float num5 = num - 26f;
			result = Mathf.Min(0.99f + 0.01f * (1f - Mathf.Exp(-0.1f * num5)), 1f);
			_ = $"(26,+] t={num5:F4}";
		}
		return result;
	}

	private float ComputeEscapeRatePercentile()
	{
		Last7AdPlayInfoTag last7AdPlayInfo = GameApp.Tag.Last7AdPlayInfo;
		if (last7AdPlayInfo?.last7_inter_ad_results == null || last7AdPlayInfo.last7_inter_ad_results.Count == 0)
		{
			return 0f;
		}
		List<bool> last7_inter_ad_results = last7AdPlayInfo.last7_inter_ad_results;
		int count = last7_inter_ad_results.Count;
		int num = ((count < 3) ? count : 3);
		int num2 = 0;
		for (int i = count - num; i < count; i++)
		{
			if (last7_inter_ad_results[i])
			{
				num2++;
			}
		}
		float num3 = (float)num2 / 3f;
		if (num3 <= 0f)
		{
			return 0f;
		}
		if (num3 <= 0.34f)
		{
			return 0.21f;
		}
		if (num3 <= 0.51f)
		{
			return 0.31f;
		}
		if (num3 <= 0.67f)
		{
			return 0.4f;
		}
		return 1f;
	}

	private float ComputeEcpmPercentile()
	{
		string countryGdpGroup = CountryGdpGroupResolver.ResolveEffectiveGdpGroup();
		List<double> last3InterIncomeList = GameApp.Tag.Last3InterIncomeList;
		double num;
		if (last3InterIncomeList == null || last3InterIncomeList.Count <= 0)
		{
			num = 0.0;
		}
		else
		{
			num = last3InterIncomeList[last3InterIncomeList.Count - 1];
		}
		double ecpm = num;
		int totalCallCount = GameApp.Save.GetOrCreate<InterstitialEscapeRecord>().TotalCallCount;
		float result = EcpmPercentileLookup.Lookup(countryGdpGroup, ecpm, totalCallCount);
		_ = 4;
		return result;
	}
}
