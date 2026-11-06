using UnityEngine;

namespace RotateRings.WeakGuide;

public sealed class WeakGuideInstallContext : IWeakGuideInstallContext
{
	public GameObject LevelRoot { get; }

	public GameLevelInfo LevelInfo { get; }

	public GameplayManager GameplayManager { get; }

	public WeakGuideInstallContext(GameObject levelRoot, GameLevelInfo levelInfo, GameplayManager gameplayManager)
	{
		LevelRoot = levelRoot;
		LevelInfo = levelInfo;
		GameplayManager = gameplayManager;
	}
}
