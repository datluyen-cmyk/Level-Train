using System;

namespace XingYun.Track.Events;

[Serializable]
public class GameUserRatingStarSubmit : BaseTrackConfig
{
	public const string EventName = "game_user_rating_star_submit";

	public int rating;

	public override void Reset()
	{
		((BaseTrackConfig)this).Reset();
		rating = 0;
	}
}
