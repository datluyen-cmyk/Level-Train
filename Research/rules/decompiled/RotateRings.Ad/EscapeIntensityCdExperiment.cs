using System;
using System.Collections.Generic;
using System.Text;
using RotateRings.Common;

namespace RotateRings.Ad;

public static class EscapeIntensityCdExperiment
{
	private const string Tag = "<Ad>[EscapeIntensityCd] ";

	private const float FastEscapeSeconds = 2.7f;

	private const float MediumEscapeSeconds = 8.4f;

	private const float SlowEscapeSeconds = 20f;

	private const float UnknownDwellIntensity = 0.65f;

	public static bool IsActive()
	{
		return EscapeRateCdMatrix.IsValidGroup(GetGroup());
	}

	public static int GetGroup()
	{
		return FeatureConfig.InterstitialFunction?.EscapeIntensityCdGroup ?? 0;
	}

	public static void OnInterstitialClosed(bool isSuccess, bool readEscapeDwellFromSdk = false)
	{
		InterstitialEscapeIntensityRecord record = GetRecord();
		float dwellSeconds = -1f;
		float item = 0f;
		if (!isSuccess)
		{
			item = ((!readEscapeDwellFromSdk || !TryGetEscapeDwellSeconds(out dwellSeconds)) ? 0.65f : ResolveEscapeIntensity(dwellSeconds));
		}
		if (record.EscapeIntensityWindow.Count >= 3)
		{
			record.EscapeIntensityWindow.RemoveAt(0);
		}
		record.EscapeIntensityWindow.Add(item);
		record.RewardBonusSeconds = 0;
		record.RewardRequiredElapsedSeconds = 0f;
	}

	public static void OnRewardAdFinished()
	{
		int num = GetGroup();
		InterstitialEscapeIntensityRecord record = GetRecord();
		if (EscapeRateCdMatrix.IsValidGroup(num) && record.EscapeIntensityWindow.Count != 0)
		{
			float? interstitialCdElapsedSeconds = InterstitialEscapeTracker.GetInterstitialCdElapsedSeconds();
			if (interstitialCdElapsedSeconds.HasValue && TryResolveBaseCooldownSeconds(num, record.EscapeIntensityWindow, out var baseCd))
			{
				float value = interstitialCdElapsedSeconds.Value;
				int installDay = GetInstallDay();
				int rewardBonusSeconds = EscapeRateCdMatrix.GetRewardBonusSeconds(num, installDay);
				record.RewardBonusSeconds += rewardBonusSeconds;
				float num2 = Math.Max(Math.Max(value, baseCd), record.RewardRequiredElapsedSeconds);
				record.RewardRequiredElapsedSeconds = num2 + (float)rewardBonusSeconds;
				Math.Max(0f, record.RewardRequiredElapsedSeconds - value);
			}
		}
	}

	public static float ResolveCooldownSeconds()
	{
		int num = GetGroup();
		InterstitialEscapeIntensityRecord record = GetRecord();
		if (!EscapeRateCdMatrix.IsValidGroup(num) || record.EscapeIntensityWindow.Count == 0)
		{
			return 70f;
		}
		if (!TryResolveBaseCooldownSeconds(num, record.EscapeIntensityWindow, out var baseCd))
		{
			return 70f;
		}
		float num2 = baseCd + (float)record.RewardBonusSeconds;
		if (record.RewardBonusSeconds > 0 && record.RewardRequiredElapsedSeconds > num2)
		{
			num2 = record.RewardRequiredElapsedSeconds;
		}
		return num2;
	}

	public static string BuildCalculationLog()
	{
		int num = GetGroup();
		InterstitialEscapeIntensityRecord record = GetRecord();
		StringBuilder stringBuilder = new StringBuilder(512);
		stringBuilder.AppendLine("--- 插屏 CD 运算 ---");
		stringBuilder.AppendLine("策略=逃逸频次与强度（S=0.5×F+0.5×D）");
		stringBuilder.Append("escapeIntensityCdGroup=").Append(num).AppendLine();
		stringBuilder.Append("window=").Append(FormatWindow(record.EscapeIntensityWindow)).AppendLine();
		if (!EscapeRateCdMatrix.IsValidGroup(num) || record.EscapeIntensityWindow.Count == 0)
		{
			stringBuilder.Append("finalCd=InsertCd=").Append(70).Append('s');
			return stringBuilder.ToString();
		}
		float escapeFrequency = GetEscapeFrequency(record.EscapeIntensityWindow);
		float averageEscapeIntensity = GetAverageEscapeIntensity(record.EscapeIntensityWindow);
		float escapeScore = GetEscapeScore(record.EscapeIntensityWindow);
		int scoreBucket = GetScoreBucket(escapeScore);
		float cdSeconds = EscapeRateCdMatrix.GetCdSeconds(num, GetInstallDay(), scoreBucket);
		float num2 = cdSeconds + (float)record.RewardBonusSeconds;
		if (record.RewardBonusSeconds > 0 && record.RewardRequiredElapsedSeconds > num2)
		{
			num2 = record.RewardRequiredElapsedSeconds;
		}
		stringBuilder.Append("F=").Append(escapeFrequency.ToString("F3")).Append(" D=")
			.Append(averageEscapeIntensity.ToString("F3"))
			.Append(" S=")
			.Append(escapeScore.ToString("F3"))
			.Append(" bucket=")
			.Append(scoreBucket)
			.AppendLine();
		stringBuilder.Append("baseCd=").Append(cdSeconds).Append("s rewardBonus=")
			.Append(record.RewardBonusSeconds)
			.Append("s finalCd=")
			.Append(num2)
			.Append('s');
		return stringBuilder.ToString();
	}

	private static InterstitialEscapeIntensityRecord GetRecord()
	{
		return GameApp.Save.GetOrCreate<InterstitialEscapeIntensityRecord>();
	}

	private static bool TryGetEscapeDwellSeconds(out float dwellSeconds)
	{
		dwellSeconds = 0f;
		SdkLastAdInfo sdkLastAdInfo = SdkLastAdInfo.TryGet();
		if (sdkLastAdInfo != null && sdkLastAdInfo.escapeAdType == "interstitialAd" && sdkLastAdInfo.escapeAdRealShowDuration >= 0)
		{
			dwellSeconds = (float)sdkLastAdInfo.escapeAdRealShowDuration / 1000f;
			return true;
		}
		return false;
	}

	private static float ResolveEscapeIntensity(float dwellSeconds)
	{
		if (dwellSeconds <= 2.7f)
		{
			return 1f;
		}
		if (dwellSeconds <= 8.4f)
		{
			return 0.7f;
		}
		if (!(dwellSeconds <= 20f))
		{
			return 0.3f;
		}
		return 0.45f;
	}

	private static bool TryResolveBaseCooldownSeconds(int group, List<float> window, out float baseCd)
	{
		int scoreBucket = GetScoreBucket(GetEscapeScore(window));
		baseCd = EscapeRateCdMatrix.GetCdSeconds(group, GetInstallDay(), scoreBucket);
		return baseCd >= 0f;
	}

	private static float GetEscapeFrequency(List<float> window)
	{
		if (window == null || window.Count == 0)
		{
			return 0f;
		}
		int num = 0;
		for (int i = 0; i < window.Count; i++)
		{
			if (window[i] > 0f)
			{
				num++;
			}
		}
		return (float)num / (float)window.Count;
	}

	private static float GetAverageEscapeIntensity(List<float> window)
	{
		if (window == null || window.Count == 0)
		{
			return 0f;
		}
		float num = 0f;
		int num2 = 0;
		for (int i = 0; i < window.Count; i++)
		{
			float num3 = window[i];
			if (!(num3 <= 0f))
			{
				num += num3;
				num2++;
			}
		}
		if (num2 != 0)
		{
			return num / (float)num2;
		}
		return 0f;
	}

	private static float GetEscapeScore(List<float> window)
	{
		return 0.5f * GetEscapeFrequency(window) + 0.5f * GetAverageEscapeIntensity(window);
	}

	private static int GetScoreBucket(float score)
	{
		if (score < 0.2f)
		{
			return 0;
		}
		if (score < 0.4f)
		{
			return 1;
		}
		if (score < 0.6f)
		{
			return 2;
		}
		if (!(score < 0.8f))
		{
			return 4;
		}
		return 3;
	}

	private static int GetInstallDay()
	{
		return Math.Max(1, AdBridgeHelper.GetInstallDays());
	}

	private static string FormatWindow(List<float> window)
	{
		if (window == null || window.Count == 0)
		{
			return "[]";
		}
		StringBuilder stringBuilder = new StringBuilder(window.Count * 5 + 2);
		stringBuilder.Append('[');
		for (int i = 0; i < window.Count; i++)
		{
			if (i > 0)
			{
				stringBuilder.Append(',');
			}
			stringBuilder.Append(window[i].ToString("F2"));
		}
		stringBuilder.Append(']');
		return stringBuilder.ToString();
	}
}
