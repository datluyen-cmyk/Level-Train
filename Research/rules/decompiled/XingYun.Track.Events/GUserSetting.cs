using System;

namespace XingYun.Track.Events;

[Serializable]
public class GUserSetting : BaseTrackConfig
{
	public const string EventName = "g_user_setting";

	public string language;

	public int vibration;

	public int sound;

	public int music;

	public string avatar_id;

	public string display_mode;

	public override void Reset()
	{
		((BaseTrackConfig)this).Reset();
		language = null;
		vibration = 0;
		sound = 0;
		music = 0;
		avatar_id = null;
		display_mode = null;
	}
}
