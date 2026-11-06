namespace RotateRings.GamePlay.Events;

public class LevelStepLimitChangedEvent
{
	public int RemainingSteps { get; set; }

	public int MaxSteps { get; set; }

	public bool IsConsumed { get; set; }

	public int AddedSteps { get; set; }
}
