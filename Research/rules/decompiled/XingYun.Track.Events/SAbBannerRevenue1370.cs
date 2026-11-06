using System;

namespace XingYun.Track.Events;

[Serializable]
public class SAbBannerRevenue1370 : BaseTrackConfig
{
	public const string EventName = "s_ab_banner_revenue_1370";

	public double revenue;

	public override void Reset()
	{
		((BaseTrackConfig)this).Reset();
		revenue = 0.0;
	}
}
