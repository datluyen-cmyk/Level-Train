using System;

namespace XingYun.Track.Events;

[Serializable]
public class GGameAdShow : BaseTrackConfig
{
	public const string EventName = "g_game_ad_show";

	public string ad_type;

	public string ad_position;

	public long ad_start_timestamp;

	public long ad_end_timestamp;

	public int ad_show_time;

	public int game_mode;

	public int level;

	public int mode_num;

	public int mode_num_all;

	public override void Reset()
	{
		((BaseTrackConfig)this).Reset();
		ad_type = null;
		ad_position = null;
		ad_start_timestamp = 0L;
		ad_end_timestamp = 0L;
		ad_show_time = 0;
		game_mode = 0;
		level = 0;
		mode_num = 0;
		mode_num_all = 0;
	}
}
