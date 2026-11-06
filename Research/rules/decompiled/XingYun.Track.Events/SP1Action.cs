using System;

namespace XingYun.Track.Events;

[Serializable]
public class SP1Action : BaseTrackConfig
{
	public const string EventName = "s_p1_action";

	public string s_stage;

	public string version;

	public override void Reset()
	{
		((BaseTrackConfig)this).Reset();
		s_stage = null;
		version = null;
	}
}
