using System;

namespace XingYun.Track.Events;

[Serializable]
public class GGameOpen : BaseTrackConfig
{
	public const string EventName = "g_game_open";

	public int game_mode;

	public int is_first_open;

	public override void Reset()
	{
		((BaseTrackConfig)this).Reset();
		game_mode = 0;
		is_first_open = 0;
	}
}
