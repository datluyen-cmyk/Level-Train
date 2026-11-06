using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using RotateRings.GamePlay.Objects;

namespace RotateRings.GamePlay.Services;

internal sealed class RegisterButterflySlotGuidesStep : ILevelEnterPipelineStep
{
	public int Priority => 130;

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
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		GameplayManager manager = ctx.Manager;
		IReadOnlyList<IGameplayObject> gameplayObjects = manager.ObjectsService.GameplayObjects;
		for (int i = 0; i < gameplayObjects.Count; i++)
		{
			if (gameplayObjects[i] is FixedButterflyObject fixedButterflyObject)
			{
				fixedButterflyObject.RegisterSlotGuideBindings(manager.ObjectsService);
			}
		}
		return UniTask.CompletedTask;
	}
}
