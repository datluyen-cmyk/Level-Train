namespace RotateRings.GamePlay.Events;

public sealed class RingBreakReadyEvent
{
	public IGameplayObject GameplayObject { get; }

	public bool IsFromBomb { get; }

	public RingBreakReadyEvent(IGameplayObject gameplayObject, bool isFromBomb)
	{
		GameplayObject = gameplayObject;
		IsFromBomb = isFromBomb;
	}
}
