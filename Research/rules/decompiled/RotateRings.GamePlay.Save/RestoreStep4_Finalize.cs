using UnityEngine;

namespace RotateRings.GamePlay.Save;

internal sealed class RestoreStep4_Finalize : IInLevelSaveStep<RotateRingRestoreContext>
{
	public void Execute(RotateRingRestoreContext ctx)
	{
		if (ctx.IsValid)
		{
			Physics2D.SyncTransforms();
		}
	}
}
