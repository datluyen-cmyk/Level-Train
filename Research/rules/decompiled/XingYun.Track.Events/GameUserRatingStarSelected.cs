using System;

namespace XingYun.Track.Events;

[Serializable]
public class GameUserRatingStarSelected : BaseTrackConfig
{
	public const string EventName = "game_user_rating_star_selected";

	public int rating;

	public override void Reset()
	{
		((BaseTrackConfig)this).Reset();
		rating = 0;
	}
}
