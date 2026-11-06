using System;
using System.Collections.Generic;
using UnityEngine;

namespace RotateRings.GamePlay.Save;

internal sealed class CaptureStep4_Bombs : IInLevelSaveStep<RotateRingCaptureContext>
{
	public void Execute(RotateRingCaptureContext ctx)
	{
		RotateRingInLevelSaveData save = ctx.Save;
		GameplayManager gameplayManager = ctx.GameplayManager;
		if (save == null || (Object)(object)gameplayManager == (Object)null)
		{
			return;
		}
		IReadOnlyList<string> readOnlyList = gameplayManager?.RingSave?.BombManifest;
		if (readOnlyList == null || readOnlyList.Count == 0)
		{
			save.Bombs = Array.Empty<RotateRingBombSave>();
			return;
		}
		IReadOnlyList<BombRelative> bombs = gameplayManager.ObjectsService.Bombs;
		List<RotateRingBombSave> list = new List<RotateRingBombSave>(readOnlyList.Count);
		for (int i = 0; i < readOnlyList.Count; i++)
		{
			string text = readOnlyList[i];
			if (!string.IsNullOrEmpty(text))
			{
				BombRelative bombRelative = FindLiveBomb(bombs, text);
				if ((Object)(object)bombRelative == (Object)null)
				{
					list.Add(new RotateRingBombSave
					{
						BombId = text,
						IsSolved = true
					});
				}
				else
				{
					list.Add(new RotateRingBombSave
					{
						BombId = text,
						IsSolved = bombRelative.IsSolvedForTrack
					});
				}
			}
		}
		save.Bombs = list.ToArray();
	}

	private static BombRelative FindLiveBomb(IReadOnlyList<BombRelative> bombs, string bombId)
	{
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
}
