using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace RotateRings.GamePlay.Services;

internal sealed class CollectLevelEnterObjectsStep : ILevelEnterPipelineStep
{
	public bool IncludeInRunPipeline => false;

	public int Priority => 235;

	public UniTask ExecuteAsync(LevelEnterPipelineContext ctx, CancellationToken ct)
	{
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
		GameplayManager manager = ctx.Manager;
		List<IGameplayObject> levelEnterObjectsCache = manager.LevelEnterObjectsCache;
		levelEnterObjectsCache.Clear();
		if ((Object)(object)manager.CurrentLevelObject == (Object)null)
		{
			ctx.LevelEnterObjects = levelEnterObjectsCache;
			return UniTask.CompletedTask;
		}
		IReadOnlyList<IGameplayObject> gameplayObjects = manager.ObjectsService.GameplayObjects;
		int num = 0;
		int num2 = 0;
		for (int i = 0; i < gameplayObjects.Count; i++)
		{
			IGameplayObject gameplayObject = gameplayObjects[i];
			MonoBehaviour val = (MonoBehaviour)((gameplayObject is MonoBehaviour) ? gameplayObject : null);
			if (val == null || (Object)(object)val == (Object)null || gameplayObject.IsSolved)
			{
				if (gameplayObject.IsSolved)
				{
					num++;
				}
				else
				{
					num2++;
				}
			}
			else
			{
				levelEnterObjectsCache.Add(gameplayObject);
			}
		}
		ctx.LevelEnterObjects = levelEnterObjectsCache;
		_ = manager.CurrentLevelObject;
		return UniTask.CompletedTask;
	}
}
