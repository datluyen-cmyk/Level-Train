using System.Collections.Generic;
using XingYun.Aot;

namespace RotateRings.Ad;

public static class ColdStartEcpmTracker
{
	private const int MaxCount = 3;

	private static readonly List<double> ecpmList = new List<double>(3);

	public static IReadOnlyList<double> EcpmList => ecpmList;

	public static void Record(double ecpm)
	{
		if (ecpmList.Count >= 3)
		{
			ecpmList.RemoveAt(0);
		}
		ecpmList.Add(ecpm);
	}

	public static double GetMedian()
	{
		switch (ecpmList.Count)
		{
		case 0:
			return 0.0;
		case 1:
			return ecpmList[0];
		case 2:
			return (ecpmList[0] + ecpmList[1]) / 2.0;
		default:
		{
			double num = ecpmList[0];
			double num2 = ecpmList[1];
			double num3 = ecpmList[2];
			if (num > num2)
			{
				double num4 = num2;
				double num5 = num;
				num = num4;
				num2 = num5;
			}
			if (num2 > num3)
			{
				double num6 = num3;
				double num5 = num2;
				num2 = num6;
				num3 = num5;
			}
			if (num > num2)
			{
				double num7 = num2;
				double num5 = num;
				num = num7;
				num2 = num5;
			}
			return num2;
		}
		}
	}

	public static double GetMax()
	{
		int count = ecpmList.Count;
		if (count == 0)
		{
			return 0.0;
		}
		double num = ecpmList[0];
		for (int i = 1; i < count; i++)
		{
			if (ecpmList[i] > num)
			{
				num = ecpmList[i];
			}
		}
		return num;
	}

	public static bool GetAdWorth()
	{
		if (!TryGetSdkInterWorth(out var worth))
		{
			return false;
		}
		if (worth <= 0.0)
		{
			worth = GetMax();
		}
		double median = GetMedian();
		if (median <= 0.0 || worth <= 0.0)
		{
			return false;
		}
		double num;
		double num2;
		switch (InterstitialFrequencyTracker.CurrentTier)
		{
		case InterstitialFrequencyTier.High:
			num = 1.1;
			num2 = 1.25;
			break;
		case InterstitialFrequencyTier.Medium:
			num = 1.15;
			num2 = 1.3;
			break;
		default:
			num = 1.2;
			num2 = 1.35;
			break;
		}
		if (worth >= median * num2)
		{
			return true;
		}
		if (worth >= median * num)
		{
			return true;
		}
		return false;
	}

	public static void Clear()
	{
		ecpmList.Clear();
	}

	private static bool TryGetSdkInterWorth(out double worth)
	{
		try
		{
			return InterstitialCacheHelper.TryParseMaxEcpmFromCachedAdsInfo(SDKManager.GetCachedAdsInfo("interAd"), out worth, "[GP39-Worth]");
		}
		catch
		{
			worth = 0.0;
			return false;
		}
	}
}
