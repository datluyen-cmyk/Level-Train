namespace RotateRings.GamePlay.Events;

public class GameplayGestureEvent
{
	public IGameplayObject GameplayObject { get; }

	public GameplayGestureEvent(IGameplayObject gameplayObject = null)
	{
		GameplayObject = gameplayObject;
	}
}
