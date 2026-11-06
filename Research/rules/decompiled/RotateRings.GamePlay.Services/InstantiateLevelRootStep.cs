using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace RotateRings.GamePlay.Services;

internal sealed class InstantiateLevelRootStep : ILevelEnterPipelineStep
{
	public int Priority => 60;

	public UniTask ExecuteAsync(LevelEnterPipelineContext ctx, CancellationToken ct)
	{
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		GameplayManager manager = ctx.Manager;
		GameObject val = Object.Instantiate<GameObject>(ctx.Prefab, manager.LevelContent);
		manager.AssignCurrentLevelRoot(val);
		ctx.LevelRoot = val;
		val.SetActive(false);
		return UniTask.CompletedTask;
	}
}
