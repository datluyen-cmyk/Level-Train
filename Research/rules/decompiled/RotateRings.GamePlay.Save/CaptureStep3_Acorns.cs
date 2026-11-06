using System;
using UnityEngine;

namespace RotateRings.GamePlay.Save;

internal sealed class CaptureStep3_Acorns : IInLevelSaveStep<RotateRingCaptureContext>
{
	public void Execute(RotateRingCaptureContext ctx)
	{
		RotateRingInLevelSaveData save = ctx.Save;
		GameplayManager gameplayManager = ctx.GameplayManager;
		if (save != null && !((Object)(object)gameplayManager == (Object)null))
		{
			RingAcornCollectService acornCollect = gameplayManager.AcornCollect;
			if (acornCollect == null)
			{
				save.AcornPartIds = Array.Empty<string>();
				return;
			}
			string[] currentAcornPartIds = acornCollect.GetCurrentAcornPartIds();
			save.AcornPartIds = currentAcornPartIds ?? Array.Empty<string>();
		}
	}
}
