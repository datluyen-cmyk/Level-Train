using System.Threading;
using Cysharp.Threading.Tasks;
using RotateRings.GameWinIQ;
using RotateRings.Model;

namespace RotateRings.GamePlay.Services;

internal sealed class RebuildLevelManifestsAndScoreStep : ILevelEnterPipelineStep
{
	public int Priority => 141;

	public bool ShouldRun(LevelEnterPipelineContext ctx)
	{
		if (!ctx.Aborted)
		{
			return ctx.Manager.HasActiveLevelRoot;
		}
		return false;
	}

	public UniTask ExecuteAsync(LevelEnterPipelineContext ctx, CancellationToken ct)
	{
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		GameplayManager manager = ctx.Manager;
		manager.RingSave.RebuildPartManifest(manager.ObjectsService.GameplayObjects);
		manager.RingSave.RebuildBombManifest(manager.ObjectsService.Bombs);
		manager.GetGameService<RotateRingOperationScoreFacade>()?.InitializeScore();
		int level = GameApp.Proxy.Get<LevelModelProxy>()?.Level ?? 1;
		GameWinIQService.Instance.OnLevelLoaded(level);
		return UniTask.CompletedTask;
	}
}
