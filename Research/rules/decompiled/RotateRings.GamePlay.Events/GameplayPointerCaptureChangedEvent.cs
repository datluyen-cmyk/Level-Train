namespace RotateRings.GamePlay.Events;

public sealed class GameplayPointerCaptureChangedEvent
{
	public bool IsCaptured { get; }

	public IGameplayPointerHandler Handler { get; }

	public GameplayPointerCaptureChangedEvent(bool isCaptured, IGameplayPointerHandler handler = null)
	{
		IsCaptured = isCaptured;
		Handler = handler;
	}
}
