using System;

namespace XingYun.Track.Events;

[Serializable]
public class TaAppCrash : BaseTrackConfig
{
	public const string EventName = "ta_app_crash";

	public override void Reset()
	{
		((BaseTrackConfig)this).Reset();
	}
}
