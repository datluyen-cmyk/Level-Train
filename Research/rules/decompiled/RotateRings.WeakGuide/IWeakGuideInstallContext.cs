using UnityEngine;

namespace RotateRings.WeakGuide;

public interface IWeakGuideInstallContext
{
	GameObject LevelRoot { get; }

	GameLevelInfo LevelInfo { get; }

	GameplayManager GameplayManager { get; }
}
