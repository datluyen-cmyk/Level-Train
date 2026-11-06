using System;

namespace XingYun.Track.Events;

[Serializable]
public class GameUserRatingFeedbackSubmit : BaseTrackConfig
{
	public const string EventName = "game_user_rating_feedback_submit";

	public string comment;

	public override void Reset()
	{
		((BaseTrackConfig)this).Reset();
		comment = null;
	}
}
