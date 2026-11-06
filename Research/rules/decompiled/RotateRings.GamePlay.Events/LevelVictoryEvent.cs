namespace RotateRings.GamePlay.Events;

public sealed class LevelVictoryEvent
{
	public int CompletedLevel { get; set; }

	public bool WasResumeSession { get; set; }
}
