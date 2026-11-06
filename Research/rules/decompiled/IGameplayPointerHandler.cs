public interface IGameplayPointerHandler
{
	bool CanReceivePointer();

	bool IsPointerCaptured();

	void OnPointerDown(GameplayPointerData data);

	void OnPointerDrag(GameplayPointerData data);

	void OnPointerUp(GameplayPointerData data);
}
