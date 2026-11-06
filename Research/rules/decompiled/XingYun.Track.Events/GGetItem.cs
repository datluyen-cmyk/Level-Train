using System;

namespace XingYun.Track.Events;

[Serializable]
public class GGetItem : BaseTrackConfig
{
	public const string EventName = "g_get_item";

	public string item_id;

	public int number;

	public int before_num;

	public int after_num;

	public int level;

	public int game_mode;

	public int mode_num;

	public int mode_num_all;

	public string reason_id;

	public string reason_info;

	public override void Reset()
	{
		((BaseTrackConfig)this).Reset();
		item_id = null;
		number = 0;
		before_num = 0;
		after_num = 0;
		level = 0;
		game_mode = 0;
		mode_num = 0;
		mode_num_all = 0;
		reason_id = null;
		reason_info = null;
	}
}
