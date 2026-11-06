using System;

namespace XingYun.Track.Events;

[Serializable]
public class GGameLog : BaseTrackConfig
{
	public const string EventName = "g_game_log";

	public string log_info;

	public int level;

	public int game_mode;

	public int mode_num;

	public int mode_num_all;

	public int index;

	public override void Reset()
	{
		((BaseTrackConfig)this).Reset();
		log_info = null;
		level = 0;
		game_mode = 0;
		mode_num = 0;
		mode_num_all = 0;
		index = 0;
	}
}
