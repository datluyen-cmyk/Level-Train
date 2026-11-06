using System;

namespace XingYun.Track.Events;

[Serializable]
public class GUseItem : BaseTrackConfig
{
	public const string EventName = "g_use_item";

	public string item_id;

	public int number;

	public int before_num;

	public int after_num;

	public int level;

	public int game_mode;

	public int mode_num;

	public int mode_num_all;

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
	}
}
