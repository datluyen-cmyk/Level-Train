using System.Threading;
using Cysharp.Threading.Tasks;
using RotateRings.GamePlay.Events;

namespace RotateRings.GamePlay.Services;

internal sealed class ResetLevelSessionStateStep : ILevelEnterPipelineStep
{
	public int Priority => 110;

	public UniTask ExecuteAsync(LevelEnterPipelineContext ctx, CancellationToken ct)
	{
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		GameplayManager manager = ctx.Manager;
		manager.ResetVictoryFlags();
		manager.GetGameService<RotateRingOperationScoreFacade>()?.Reset();
		manager.ClearLegacyOperationSnapshot();
		GameApp.Event.Send<RingDragSettleResetEvent>(new RingDragSettleResetEvent());
		manager.ObjectsService.ClearPartMaps();
		manager.RingSave.ClearManifest();
		return UniTask.CompletedTask;
	}
}
