using System;
using XingYun.Ad;

namespace RotateRings.Ad.Bridge;

public static class AdConfigurationExtensions
{
	public const string CustomKeyIsFrontAd = "is_front_ad";

	public static AdConfiguration SetIsFrontAd(this AdConfiguration config, bool isFrontAd)
	{
		return config.SetCustomData("is_front_ad", (object)isFrontAd);
	}

	public static bool GetIsFrontAd(this IAdConfiguration config)
	{
		object value = default(object);
		if (config != null && config.GetCustomData()?.TryGetValue("is_front_ad", out value) == true && value is bool)
		{
			return (bool)value;
		}
		string text = ((config != null) ? config.AdPosition : null);
		if (!string.IsNullOrEmpty(text))
		{
			return text.StartsWith("前插屏", StringComparison.Ordinal);
		}
		return false;
	}
}
