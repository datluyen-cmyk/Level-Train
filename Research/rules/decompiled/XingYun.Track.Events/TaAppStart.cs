using System;

namespace XingYun.Track.Events;

[Serializable]
public class TaAppStart : BaseTrackConfig
{
	public const string EventName = "ta_app_start";

	public override void Reset()
	{
		((BaseTrackConfig)this).Reset();
	}
}
