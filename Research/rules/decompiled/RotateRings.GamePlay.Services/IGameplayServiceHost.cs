namespace RotateRings.GamePlay.Services;

public interface IGameplayServiceHost : IGameplayManager
{
	void RegisterGameService(IGameplayService service);

	void UnregisterGameService(IGameplayService service);

	T GetGameService<T>() where T : class, IGameplayService;

	void DisposeGameServices();
}
