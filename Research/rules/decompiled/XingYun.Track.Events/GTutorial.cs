using System;

namespace XingYun.Track.Events;

[Serializable]
public class GTutorial : BaseTrackConfig
{
	public const string EventName = "g_tutorial";

	public int game_mode;

	public int stage;

	public int stage_type;

	public string stage_name;

	public override void Reset()
	{
		((BaseTrackConfig)this).Reset();
		game_mode = 0;
		stage = 0;
		stage_type = 0;
		stage_name = null;
	}
}
