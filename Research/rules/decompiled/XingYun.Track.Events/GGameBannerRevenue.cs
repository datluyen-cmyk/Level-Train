using System;

namespace XingYun.Track.Events;

[Serializable]
public class GGameBannerRevenue : BaseTrackConfig
{
	public const string EventName = "g_game_banner_revenue";

	public double revenue;

	public int game_mode;

	public override void Reset()
	{
		((BaseTrackConfig)this).Reset();
		revenue = 0.0;
		game_mode = 0;
	}
}
