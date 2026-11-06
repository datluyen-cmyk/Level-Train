using System.Threading;
using Cysharp.Threading.Tasks;
using RotateRings.Ad;
using UniLog;
using UnityEngine;

namespace RotateRings.GamePlay.Services;

internal sealed class ValidateLoadedLevelPrefabStep : ILevelEnterPipelineStep
{
	public int Priority => 40;

	public UniTask ExecuteAsync(LevelEnterPipelineContext ctx, CancellationToken ct)
	{
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)ctx.Prefab != (Object)null)
		{
			return UniTask.CompletedTask;
		}
		Log.LogError("GameplayManager", "Failed to load level prefab ({0}): {1}", new object[2] { ctx.LoadSource, ctx.Level });
		AdService.ClearPendingFrontAd("LevelLoadFailed");
		ctx.Aborted = true;
		return UniTask.CompletedTask;
	}
}
