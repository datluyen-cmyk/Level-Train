using System;

namespace XingYun.Track.Events;

[Serializable]
public class SAdBannerRevenue : BaseTrackConfig
{
	public const string EventName = "s_ad_banner_revenue";

	public double revenue;

	public override void Reset()
	{
		((BaseTrackConfig)this).Reset();
		revenue = 0.0;
	}
}
