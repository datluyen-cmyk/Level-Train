namespace RotateRings.GamePlay.Events;

public class RingSolvedEvent
{
	public IGameplayObject GameplayObject { get; set; }

	public RingSolvedEvent(IGameplayObject gameplayObject)
	{
		GameplayObject = gameplayObject;
	}
}
