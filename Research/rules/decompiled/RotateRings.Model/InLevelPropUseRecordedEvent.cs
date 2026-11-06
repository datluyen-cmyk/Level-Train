using RotateRings.Prop;

namespace RotateRings.Model;

public class InLevelPropUseRecordedEvent
{
	public EPropType PropType { get; set; }

	public TrackPropUseReason Reason { get; set; }

	public int UseCount { get; set; }
}
