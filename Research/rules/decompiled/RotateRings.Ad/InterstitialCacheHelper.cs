using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using XingYun.Aot;

namespace RotateRings.Ad;

public static class InterstitialCacheHelper
{
	private const string Tag = "<Ad>";

	public const int MaxCacheCount = 3;

	public static int GetInterstitialCacheCount()
	{
		if (!TryParseCacheCountFromSdk(out var cacheCount))
		{
			cacheCount = ((GameApp.Ad != null && GameApp.Ad.IsInterAdReady) ? 1 : 0);
		}
		if (cacheCount <= 3)
		{
			return cacheCount;
		}
		return 3;
	}

	public static bool IsCacheFull()
	{
		return GetInterstitialCacheCount() >= 3;
	}

	public static bool TryParseMaxEcpmFromCachedAdsInfo(string json, out double maxEcpm, string logPrefix = "")
	{
		maxEcpm = 0.0;
		if (string.IsNullOrWhiteSpace(json))
		{
			return false;
		}
		try
		{
			JToken val = default(JToken);
			if (JObject.Parse(json.Trim()).TryGetValue("adList", ref val))
			{
				JArray val2 = (JArray)(object)((val is JArray) ? val : null);
				if (val2 != null && ((JContainer)val2).Count != 0)
				{
					JToken val4 = default(JToken);
					for (int i = 0; i < ((JContainer)val2).Count; i++)
					{
						JToken obj = val2[i];
						JObject val3 = (JObject)(object)((obj is JObject) ? obj : null);
						if (val3 != null && val3.TryGetValue("ecpm", ref val4))
						{
							double num = Extensions.Value<double>((IEnumerable<JToken>)val4);
							if (num > maxEcpm)
							{
								maxEcpm = num;
							}
						}
					}
					return maxEcpm > 0.0;
				}
			}
			return false;
		}
		catch (Exception)
		{
			return false;
		}
	}

	public static bool TryParseCacheCountFromCachedAdsInfo(string json, out int cacheCount, string logPrefix = "")
	{
		cacheCount = 0;
		if (string.IsNullOrWhiteSpace(json))
		{
			return false;
		}
		try
		{
			JObject val = JObject.Parse(json.Trim());
			JToken val2 = default(JToken);
			if (val.TryGetValue("adUnitMaxNum", ref val2))
			{
				cacheCount = Extensions.Value<int>((IEnumerable<JToken>)val2);
				return true;
			}
			JToken val3 = default(JToken);
			if (val.TryGetValue("adCacheNum", ref val3))
			{
				cacheCount = Extensions.Value<int>((IEnumerable<JToken>)val3);
				return true;
			}
			JToken val4 = default(JToken);
			if (val.TryGetValue("adList", ref val4))
			{
				JArray val5 = (JArray)(object)((val4 is JArray) ? val4 : null);
				if (val5 != null)
				{
					cacheCount = ((JContainer)val5).Count;
					return true;
				}
			}
			return false;
		}
		catch (Exception)
		{
			return false;
		}
	}

	private static bool TryParseCacheCountFromSdk(out int cacheCount)
	{
		cacheCount = 0;
		try
		{
			return TryParseCacheCountFromCachedAdsInfo(SDKManager.GetCachedAdsInfo("interAd"), out cacheCount, "[GP39-Cache]");
		}
		catch (Exception)
		{
			return false;
		}
	}
}
