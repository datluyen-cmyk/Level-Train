using System.Threading;
using Cysharp.Threading.Tasks;

namespace RotateRings.GamePlay.Services;

internal sealed class CollectLevelGameplayObjectsStep : ILevelEnterPipelineStep
{
	public int Priority => 70;

	public UniTask ExecuteAsync(LevelEnterPipelineContext ctx, CancellationToken ct)
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		ctx.Manager.ObjectsService.CollectFromLevel(ctx.LevelRoot);
		return UniTask.CompletedTask;
	}
}
