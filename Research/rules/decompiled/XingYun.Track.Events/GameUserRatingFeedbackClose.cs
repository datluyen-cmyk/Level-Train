using System;

namespace XingYun.Track.Events;

[Serializable]
public class GameUserRatingFeedbackClose : BaseTrackConfig
{
	public const string EventName = "game_user_rating_feedback_close";

	public string comment;

	public override void Reset()
	{
		((BaseTrackConfig)this).Reset();
		comment = null;
	}
}
