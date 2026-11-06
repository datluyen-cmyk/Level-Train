using System;

namespace XingYun.Track.Events;

[Serializable]
public class GameUserRatingPopupShow : BaseTrackConfig
{
	public const string EventName = "game_user_rating_popup_show";

	public override void Reset()
	{
		((BaseTrackConfig)this).Reset();
	}
}
