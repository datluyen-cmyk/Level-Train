using System;

namespace XingYun.Track.Events;

[Serializable]
public class GSubcontractStart : BaseTrackConfig
{
	public const string EventName = "g_subcontract_start";

	public string g_subcontract_name;

	public string g_down_version;

	public string g_size;

	public override void Reset()
	{
		((BaseTrackConfig)this).Reset();
		g_subcontract_name = null;
		g_down_version = null;
		g_size = null;
	}
}
