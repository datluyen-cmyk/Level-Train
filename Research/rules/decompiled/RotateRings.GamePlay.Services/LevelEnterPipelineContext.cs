using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace RotateRings.GamePlay.Services;

public sealed class LevelEnterPipelineContext
{
	public GameplayManager Manager { get; set; }

	public int Level { get; set; }

	public bool IsNewGame { get; set; }

	public bool ReleaseGroupOnClear { get; set; } = true;

	public string LoadSource { get; set; } = "YooAssets";

	public GameObject Prefab { get; set; }

	public GameObject LevelRoot { get; set; }

	public GameLevelInfo LevelInfo { get; set; }

	public Camera MainCamera { get; set; }

	public bool ShouldWaitPreEnter { get; set; }

	public CancellationToken LoadSessionToken { get; set; }

	public bool Aborted { get; set; }

	public IReadOnlyList<IGameplayObject> LevelEnterObjects { get; set; }
}
