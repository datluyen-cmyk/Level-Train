using System;
using UnityEngine;
using XingYun.Aot;

namespace RotateRings.Ad;

[Serializable]
public sealed class SdkLastAdInfo
{
	public const string InterstitialAdType = "interstitialAd";

	public long escapeAdRealShowDuration;

	public string escapeAdType;

	public static SdkLastAdInfo TryGet()
	{
		try
		{
			string lastAdInfo = SDKManager.GetLastAdInfo();
			return string.IsNullOrEmpty(lastAdInfo) ? null : JsonUtility.FromJson<SdkLastAdInfo>(lastAdInfo);
		}
		catch (Exception)
		{
			return null;
		}
	}
}
