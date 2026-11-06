using System.Threading;
using Cysharp.Threading.Tasks;
using RotateRings.Ad;

namespace RotateRings.GamePlay.Services;

internal sealed class FinalizeLevelEnterPostStep : ILevelEnterPipelineStep
{
	public int Priority => 290;

	public UniTask ExecuteAsync(LevelEnterPipelineContext ctx, CancellationToken ct)
	{
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		GameplayManager manager = ctx.Manager;
		if (AdService.IsFrontAdHasPending())
		{
			AdService.NotifyFrontAdLevelReady();
		}
		manager.GetGameService<RingAcornCollectService>()?.Spawn(manager.ObjectsService.Rings, manager.ObjectsService.CollectableObjects, ctx.IsNewGame);
		return UniTask.CompletedTask;
	}
}
