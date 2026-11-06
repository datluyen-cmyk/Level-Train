using System;

namespace XingYun.Track.Events;

[Serializable]
public class GCrashAnr : BaseTrackConfig
{
	public const string EventName = "g_crash_anr";

	public string log_error_type;

	public string error_info;

	public string stack;

	public override void Reset()
	{
		((BaseTrackConfig)this).Reset();
		log_error_type = null;
		error_info = null;
		stack = null;
	}
}
