using System.Threading;
using Cysharp.Threading.Tasks;
using Game.UI;

namespace RotateRings.GamePlay.Services;

internal sealed class PrepareLevelGameplayCameraStep : ILevelEnterPipelineStep
{
	public int Priority => 90;

	public UniTask ExecuteAsync(LevelEnterPipelineContext ctx, CancellationToken ct)
	{
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		ctx.MainCamera = GameplayCameraManager.GameplayCamera;
		UICameraOverlayHandler.ValidateAndRepair($"OnLevelLoaded:{ctx.Level}");
		return UniTask.CompletedTask;
	}
}
