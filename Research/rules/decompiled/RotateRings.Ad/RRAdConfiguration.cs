using System.Collections.Generic;
using XingYun.Ad;

namespace RotateRings.Ad;

public sealed class RRAdConfiguration : AdConfiguration
{
	public RRAdConfiguration(string adPosition, string adType, string businessType, RRAdContext context = null, bool? isFrontAd = null)
		: base(adPosition)
	{
		((AdConfiguration)this).SetCustomData("ad_type", (object)adType);
		((AdConfiguration)this).SetCustomData("rotate_rings_ad_business_type", (object)businessType);
		((AdConfiguration)this).SetCustomData("ad_position", (object)((AdConfiguration)this).AdPosition);
		ApplyContext(context);
		if (isFrontAd.HasValue)
		{
			((AdConfiguration)this).SetCustomData("is_front_ad", (object)isFrontAd.Value);
		}
	}

	private void ApplyContext(RRAdContext context)
	{
		if (context == null)
		{
			return;
		}
		((AdConfiguration)this).SetCustomData("level", (object)context.Level);
		((AdConfiguration)this).SetCustomData("game_mode", (object)context.GameMode);
		((AdConfiguration)this).SetCustomData("mode_num", (object)context.ModeNum);
		((AdConfiguration)this).SetCustomData("mode_num_all", (object)context.ModeNumAll);
		((AdConfiguration)this).SetCustomData("game_score", (object)context.GameScore);
		if (context.ExtraData == null)
		{
			return;
		}
		foreach (KeyValuePair<string, object> extraDatum in context.ExtraData)
		{
			((AdConfiguration)this).SetCustomData(extraDatum.Key, extraDatum.Value);
		}
	}
}
