using System;

namespace XingYun.Track.Events;

[Serializable]
public class GBtnClick : BaseTrackConfig
{
	public const string EventName = "g_btn_click";

	public string click_type;

	public int ad_is_ready;

	public override void Reset()
	{
		((BaseTrackConfig)this).Reset();
		click_type = null;
		ad_is_ready = 0;
	}
}
