using System;

namespace XingYun.Track.Events;

[Serializable]
public class GSubcontractEnd : BaseTrackConfig
{
	public const string EventName = "g_subcontract_end";

	public string g_result;

	public string g_subcontract_name;

	public string g_down_version;

	public string g_size;

	public string g_size_total;

	public string g_time;

	public string g_time_total;

	public override void Reset()
	{
		((BaseTrackConfig)this).Reset();
		g_result = null;
		g_subcontract_name = null;
		g_down_version = null;
		g_size = null;
		g_size_total = null;
		g_time = null;
		g_time_total = null;
	}
}
