using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnlockRing.Presentation;

namespace RotateRings.GamePlay.Services;

internal sealed class ApplyLevelConfigAndStyleStep : ILevelEnterPipelineStep
{
	public int Priority => 80;

	public UniTask ExecuteAsync(LevelEnterPipelineContext ctx, CancellationToken ct)
	{
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		GameplayManager manager = ctx.Manager;
		GameLevelInfo.ResolveInputCamera(ctx.LevelRoot, out var gameLevelInfo);
		ctx.LevelInfo = gameLevelInfo;
		manager.AssignCurrentLevelInfo(gameLevelInfo);
		manager.LevelConfig?.ApplyFromLevelInfo(gameLevelInfo, ctx.Level);
		PresentationMode.SuppressByPaletteLock = (Object)(object)gameLevelInfo != (Object)null && gameLevelInfo.PaletteLockKeyBindings.Count > 0;
		return UniTask.CompletedTask;
	}
}
