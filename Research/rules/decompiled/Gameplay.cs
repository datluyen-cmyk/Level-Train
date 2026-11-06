public static class Gameplay
{
	private static IGameplayManager active;

	public static IGameplayManager Active => active;

	public static GameplayMode Mode => active?.Mode ?? GameplayMode.RotateRing;

	public static void Register(IGameplayManager manager)
	{
		active = manager;
	}

	public static void Unregister(IGameplayManager manager)
	{
		if (active == manager)
		{
			active = null;
		}
	}
}
