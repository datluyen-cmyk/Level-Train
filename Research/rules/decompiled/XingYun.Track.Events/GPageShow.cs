using System;

namespace XingYun.Track.Events;

[Serializable]
public class GPageShow : BaseTrackConfig
{
	public const string EventName = "g_page_show";

	public string show_type;

	public override void Reset()
	{
		((BaseTrackConfig)this).Reset();
		show_type = null;
	}
}
