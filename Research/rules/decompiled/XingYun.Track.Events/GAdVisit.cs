using System;

namespace XingYun.Track.Events;

[Serializable]
public class GAdVisit : BaseTrackConfig
{
	public const string EventName = "g_ad_visit";

	public string ad_type;

	public string ad_visit_scene;

	public string ad_visit_action;

	public int ad_is_ready;

	public int session_time;

	public int game_mode;

	public int level;

	public int mode_num;

	public int mode_num_all;

	public int mode_winnum;

	public int mode_winnum_all;

	public int game_time;

	public int game_time_real;

	public override void Reset()
	{
		((BaseTrackConfig)this).Reset();
		ad_type = null;
		ad_visit_scene = null;
		ad_visit_action = null;
		ad_is_ready = 0;
		session_time = 0;
		game_mode = 0;
		level = 0;
		mode_num = 0;
		mode_num_all = 0;
		mode_winnum = 0;
		mode_winnum_all = 0;
		game_time = 0;
		game_time_real = 0;
	}
}
