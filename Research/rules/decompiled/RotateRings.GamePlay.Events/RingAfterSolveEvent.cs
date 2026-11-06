namespace RotateRings.GamePlay.Events;

public sealed class RingAfterSolveEvent
{
	public IGameplayObject GameplayObject { get; }

	public bool IsFromBomb { get; }

	public bool HasChainBreak { get; set; }

	public int BreakRingOrder { get; set; }

	public RingAfterSolveEvent(IGameplayObject gameplayObject, bool isFromBomb)
	{
		GameplayObject = gameplayObject;
		IsFromBomb = isFromBomb;
	}
}
