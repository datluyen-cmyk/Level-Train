using System;

namespace XingYun.Track.Events;

[Serializable]
public class GScreenData : BaseTrackConfig
{
	public const string EventName = "g_screen_data";

	public double physics_width;

	public double physics_height;

	public double physics_unit_per100;

	public double screen_unit_per100;

	public string screen_data_is_trust;

	public override void Reset()
	{
		((BaseTrackConfig)this).Reset();
		physics_width = 0.0;
		physics_height = 0.0;
		physics_unit_per100 = 0.0;
		screen_unit_per100 = 0.0;
		screen_data_is_trust = null;
	}
}
