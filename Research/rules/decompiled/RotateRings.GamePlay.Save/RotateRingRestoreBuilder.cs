using UnityEngine;

namespace RotateRings.GamePlay.Save;

internal sealed class RotateRingRestoreBuilder
{
	private readonly IInLevelSaveStep<RotateRingRestoreContext>[] steps = new IInLevelSaveStep<RotateRingRestoreContext>[4]
	{
		new RestoreStep1_ValidateAndIndex(),
		new RestoreStep2_ApplyPartStates(),
		new RestoreStep3_Bombs(),
		new RestoreStep4_Finalize()
	};

	public void Build(RotateRingRestoreContext ctx)
	{
		if (ctx?.Save == null || !ctx.Save.HasData || (Object)(object)ctx.GameplayManager == (Object)null)
		{
			if (ctx != null)
			{
				ctx.IsValid = false;
			}
			return;
		}
		IInLevelSaveStep<RotateRingRestoreContext>[] array = steps;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].Execute(ctx);
			if (!ctx.IsValid)
			{
				break;
			}
		}
	}
}
