using System;

namespace XingYun.Track.Events;

[Serializable]
public class GameUserRatingPopupClose : BaseTrackConfig
{
	public const string EventName = "game_user_rating_popup_close";

	public override void Reset()
	{
		((BaseTrackConfig)this).Reset();
	}
}
