namespace RotateRings.GamePlay.Services;

public interface IDragLimitStyleHandler
{
	void OnPointerDown(IGameplayObject target, GameplayPointerData data);

	void OnPointerDrag(IGameplayObject target, GameplayPointerData data);

	void OnPointerUp(IGameplayObject target);

	void TickLateUpdate(IGameplayObject activeTarget);

	void ClearSession();
}
