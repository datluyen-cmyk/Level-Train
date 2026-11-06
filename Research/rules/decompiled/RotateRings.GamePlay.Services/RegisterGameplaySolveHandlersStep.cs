using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace RotateRings.GamePlay.Services;

internal sealed class RegisterGameplaySolveHandlersStep : ILevelEnterPipelineStep
{
	public int Priority => 120;

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
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		GameplayManager manager = ctx.Manager;
		RingObjectSolveService solveService = manager.SolveService;
		IReadOnlyList<IGameplayObject> gameplayObjects = manager.ObjectsService.GameplayObjects;
		for (int i = 0; i < gameplayObjects.Count; i++)
		{
			IGameplayObject gameplayObject = gameplayObjects[i];
			if (gameplayObject != null)
			{
				gameplayObject.RegisterOnSolved(manager.OnInteractableSolved);
				manager.ObjectsService.RegisterPartId(gameplayObject);
				solveService.RegisterObject(gameplayObject);
			}
		}
		return UniTask.CompletedTask;
	}
}
