namespace RotateRings.GamePlay.Events;

public sealed class RingPreSolveEvent
{
	public IGameplayObject GameplayObject { get; }

	public bool IsFromBomb { get; }

	public RingPreSolveEvent(IGameplayObject gameplayObject, bool isFromBomb)
	{
		GameplayObject = gameplayObject;
		IsFromBomb = isFromBomb;
	}
}
