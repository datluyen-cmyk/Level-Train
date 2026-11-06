using System;

namespace XingYun.Track.Events;

[Serializable]
public class GGameResourceUpdateInfo : BaseTrackConfig
{
	public const string EventName = "g_game_resource_update_info";

	public int duration_ms;

	public string request_result;

	public string error_code;

	public string error_msg;

	public string resource_version;

	public override void Reset()
	{
		((BaseTrackConfig)this).Reset();
		duration_ms = 0;
		request_result = null;
		error_code = null;
		error_msg = null;
		resource_version = null;
	}
}
