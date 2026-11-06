using System;

namespace XingYun.Track.Events;

[Serializable]
public class GHotfixFlow : BaseTrackConfig
{
	public const string EventName = "g_hotfix_flow";

	public string hotfix_step;

	public string status;

	public string error;

	public override void Reset()
	{
		((BaseTrackConfig)this).Reset();
		hotfix_step = null;
		status = null;
		error = null;
	}
}
