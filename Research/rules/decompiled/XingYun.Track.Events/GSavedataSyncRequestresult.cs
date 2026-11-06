using System;

namespace XingYun.Track.Events;

[Serializable]
public class GSavedataSyncRequestresult : BaseTrackConfig
{
	public const string EventName = "g_savedata_sync_requestresult";

	public string trigger_node;

	public string result;

	public string fail_reason;

	public int duration_ms;

	public bool is_conflict;

	public string selected_save_source;

	public int local_level;

	public int cloud_level;

	public int final_level;

	public string local_prop;

	public string cloud_prop;

	public string final_prop;

	public override void Reset()
	{
		((BaseTrackConfig)this).Reset();
		trigger_node = null;
		result = null;
		fail_reason = null;
		duration_ms = 0;
		is_conflict = false;
		selected_save_source = null;
		local_level = 0;
		cloud_level = 0;
		final_level = 0;
		local_prop = null;
		cloud_prop = null;
		final_prop = null;
	}
}
