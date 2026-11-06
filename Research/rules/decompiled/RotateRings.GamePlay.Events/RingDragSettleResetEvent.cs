namespace RotateRings.GamePlay.Events;

public sealed class RingDragSettleResetEvent
{
	public IGameplayObject GameplayObject { get; }

	public LegacyOperationSource? OperationSource { get; }

	public RingDragSettleResetEvent(IGameplayObject gameplayObject = null, LegacyOperationSource? operationSource = null)
	{
		GameplayObject = gameplayObject;
		OperationSource = operationSource;
	}
}
