using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace RotateRings.GamePlay.Services;

internal sealed class AssignPaletteLockKeyColorsStep : ILevelEnterPipelineStep
{
	public int Priority => 144;

	public UniTask ExecuteAsync(LevelEnterPipelineContext ctx, CancellationToken ct)
	{
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
		GameplayManager manager = ctx.Manager;
		RingPaletteLockService paletteLock = manager.PaletteLock;
		if (paletteLock == null || (Object)(object)ctx.LevelInfo == (Object)null)
		{
			return UniTask.CompletedTask;
		}
		IReadOnlyList<RingObject> readOnlyList = manager.ObjectsService?.Rings;
		if (readOnlyList == null || readOnlyList.Count == 0)
		{
			paletteLock.BuildKeyColorAssignment(ctx.LevelInfo, null);
			return UniTask.CompletedTask;
		}
		Dictionary<int, GameplayPartData> dictionary = new Dictionary<int, GameplayPartData>();
		for (int i = 0; i < readOnlyList.Count; i++)
		{
			RingObject ringObject = readOnlyList[i];
			if (!((Object)(object)ringObject == (Object)null))
			{
				int id = ringObject.PartData.Id;
				if (id > 0)
				{
					dictionary[id] = ringObject.PartData;
				}
			}
		}
		paletteLock.BuildKeyColorAssignment(ctx.LevelInfo, dictionary);
		return UniTask.CompletedTask;
	}
}
