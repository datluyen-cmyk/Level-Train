using System.Threading;
using Cysharp.Threading.Tasks;

namespace RotateRings.GamePlay.Services;

internal sealed class ResetDragCollisionOnLevelStep : ILevelEnterPipelineStep
{
	public int Priority => 142;

	public UniTask ExecuteAsync(LevelEnterPipelineContext ctx, CancellationToken ct)
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		ctx.Manager.RingDragCollisionEventService?.SendAndClearData();
		return UniTask.CompletedTask;
	}
}
