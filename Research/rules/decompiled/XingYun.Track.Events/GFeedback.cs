using System;

namespace XingYun.Track.Events;

[Serializable]
public class GFeedback : BaseTrackConfig
{
	public const string EventName = "g_feedback";

	public int rating;

	public string language;

	public string feedback;

	public override void Reset()
	{
		((BaseTrackConfig)this).Reset();
		rating = 0;
		language = null;
		feedback = null;
	}
}
