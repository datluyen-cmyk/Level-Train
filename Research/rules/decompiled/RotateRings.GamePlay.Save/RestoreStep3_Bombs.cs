using System;
using System.Collections.Generic;
using UnityEngine;

namespace RotateRings.GamePlay.Save;

internal sealed class RestoreStep3_Bombs : IInLevelSaveStep<RotateRingRestoreContext>
{
	public void Execute(RotateRingRestoreContext ctx)
	{
		if (!ctx.IsValid)
		{
			return;
		}
		RotateRingInLevelSaveData save = ctx.Save;
		GameplayManager gameplayManager = ctx.GameplayManager;
		if (save?.Bombs == null || save.Bombs.Length == 0 || (Object)(object)gameplayManager == (Object)null)
		{
			return;
		}
		RotateRingBombSave[] bombs = save.Bombs;
		foreach (RotateRingBombSave rotateRingBombSave in bombs)
		{
			if (rotateRingBombSave != null && rotateRingBombSave.IsSolved && !string.IsNullOrEmpty(rotateRingBombSave.BombId))
			{
				BombRelative bombRelative = FindLiveBomb(gameplayManager, rotateRingBombSave.BombId);
				if (!((Object)(object)bombRelative == (Object)null))
				{
					RemoveSolvedBombSilently(bombRelative);
				}
			}
		}
	}

	private static BombRelative FindLiveBomb(GameplayManager gameplayManager, string bombId)
	{
		IReadOnlyList<BombRelative> bombs = gameplayManager.ObjectsService.Bombs;
		if (bombs == null)
		{
			return null;
		}
		for (int i = 0; i < bombs.Count; i++)
		{
			BombRelative bombRelative = bombs[i];
			if (!((Object)(object)bombRelative == (Object)null) && string.Equals(bombRelative.GetBombId(), bombId, StringComparison.Ordinal))
			{
				return bombRelative;
			}
		}
		return null;
	}

	private static void RemoveSolvedBombSilently(BombRelative bomb)
	{
		bomb.RemoveWithoutExplosion();
		if ((Object)(object)((Component)bomb).gameObject != (Object)null && Application.isPlaying)
		{
			Object.Destroy((Object)(object)((Component)bomb).gameObject);
		}
	}
}
