using System;

namespace XingYun.Track.Events;

[Serializable]
public class TaAppInstall : BaseTrackConfig
{
	public const string EventName = "ta_app_install";

	public override void Reset()
	{
		((BaseTrackConfig)this).Reset();
	}
}
