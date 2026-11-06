using System;

namespace XingYun.Track.Events;

[Serializable]
public class TaAppEnd : BaseTrackConfig
{
	public const string EventName = "ta_app_end";

	public double duration;

	public override void Reset()
	{
		((BaseTrackConfig)this).Reset();
		duration = 0.0;
	}
}
