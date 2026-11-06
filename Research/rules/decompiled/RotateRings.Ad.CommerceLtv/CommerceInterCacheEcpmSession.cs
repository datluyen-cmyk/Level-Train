using System;
using UnityEngine;
using XingYun.Aot;

namespace RotateRings.Ad.CommerceLtv;

public static class CommerceInterCacheEcpmSession
{
	private const float EMaxCacheTtlSeconds = 5f;

	private static double cachedEMax = -1.0;

	private static float cachedEMaxTime = -1f;

	public static void ClearOnColdStart()
	{
		InvalidateEMaxCache();
	}

	public static void SyncFromSdk()
	{
		InvalidateEMaxCache();
		GetEMax();
	}

	public static double GetEMax()
	{
		float realtimeSinceStartup = Time.realtimeSinceStartup;
		if (cachedEMax >= 0.0 && realtimeSinceStartup - cachedEMaxTime < 5f)
		{
			return cachedEMax;
		}
		double result = (cachedEMax = FetchMaxEcpmFromSdk());
		cachedEMaxTime = realtimeSinceStartup;
		return result;
	}

	private static double FetchMaxEcpmFromSdk()
	{
		try
		{
			double maxEcpm;
			return InterstitialCacheHelper.TryParseMaxEcpmFromCachedAdsInfo(SDKManager.GetCachedAdsInfo("interAd"), out maxEcpm, "[CommerceLtv-EMax]") ? maxEcpm : 0.0;
		}
		catch (Exception)
		{
			return 0.0;
		}
	}

	private static void InvalidateEMaxCache()
	{
		cachedEMax = -1.0;
		cachedEMaxTime = -1f;
	}
}
