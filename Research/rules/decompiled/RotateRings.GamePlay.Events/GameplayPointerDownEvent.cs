namespace RotateRings.GamePlay.Events;

public sealed class GameplayPointerDownEvent
{
	public IGameplayPointerHandler PointerHandler { get; }

	public GameplayPointerDownEvent(IGameplayPointerHandler pointerHandler)
	{
		PointerHandler = pointerHandler;
	}
}
