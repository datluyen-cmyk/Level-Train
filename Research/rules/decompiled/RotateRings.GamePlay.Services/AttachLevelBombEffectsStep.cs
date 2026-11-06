using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace RotateRings.GamePlay.Services;

internal sealed class AttachLevelBombEffectsStep : ILevelEnterPipelineStep
{
	public int Priority => 143;

	public UniTask ExecuteAsync(LevelEnterPipelineContext ctx, CancellationToken ct)
	{
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		GameplayManager manager = ctx.Manager;
		RingBoomService ringBoom = manager.RingBoom;
		if ((Object)(object)manager.CurrentLevelObject == (Object)null || ringBoom == null)
		{
			return UniTask.CompletedTask;
		}
		IReadOnlyList<BombRelative> bombs = manager.ObjectsService.Bombs;
		for (int i = 0; i < bombs.Count; i++)
		{
			BombRelative bombRelative = bombs[i];
			if (!((Object)(object)bombRelative == (Object)null))
			{
				ringBoom.AttachToBomb(bombRelative);
			}
		}
		return UniTask.CompletedTask;
	}
}
