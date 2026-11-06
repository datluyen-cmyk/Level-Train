using System;

namespace XingYun.Track.Events;

[Serializable]
public class SGameAdRevenue : BaseTrackConfig
{
	public const string EventName = "s_game_ad_revenue";

	public string ad_type;

	public double revenue;

	public string ad_position;

	public int mode_num;

	public int mode_num_all;

	public int game_score;

	public int game_time;

	public int game_time_real;

	public int game_mode;

	public override void Reset()
	{
		((BaseTrackConfig)this).Reset();
		ad_type = null;
		revenue = 0.0;
		ad_position = null;
		mode_num = 0;
		mode_num_all = 0;
		game_score = 0;
		game_time = 0;
		game_time_real = 0;
		game_mode = 0;
	}
}
