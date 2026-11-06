using System.Threading;
using Cysharp.Threading.Tasks;
using RotateRing.GamePlay;
using UnityEngine;

namespace RotateRings.GamePlay.Services;

internal sealed class AdaptCoreLevelRenderStep : ILevelEnterPipelineStep
{
	public int Priority => 100;

	public bool ShouldRun(LevelEnterPipelineContext ctx)
	{
		if (!ctx.Aborted && (Object)(object)ctx.LevelInfo != (Object)null)
		{
			return ctx.LevelInfo.isUseCoreLevel;
		}
		return false;
	}

	public UniTask ExecuteAsync(LevelEnterPipelineContext ctx, CancellationToken ct)
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		GameplayManager manager = ctx.Manager;
		GameLevelInfo levelInfo = ctx.LevelInfo;
		Camera mainCamera = ctx.MainCamera;
		manager.WorldBounds = BoardRenderUtils.WorldRectFromAabb(levelInfo.Bounds, 0.01f, manager.CoordScale);
		manager.AssignLevelGameplayCameraState(LevelConfigService.GetPhysicsLevelPathKey(ctx.Level), mainCamera);
		manager.GetGameService<RotateRingBackgroundAdaptation>()?.InitLevel(manager.LevelBackground, mainCamera, manager.BackgroundMaskMaterial);
		manager.GetGameService<RotateCameraAdaptation>()?.BindFromUIGame();
		manager.GetGameService<RotateCameraGestureController>()?.RefreshGestureEnabledState();
		return UniTask.CompletedTask;
	}
}
