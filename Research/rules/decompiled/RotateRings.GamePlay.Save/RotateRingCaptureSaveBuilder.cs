using UnityEngine;

namespace RotateRings.GamePlay.Save;

internal sealed class RotateRingCaptureSaveBuilder
{
	private readonly IInLevelSaveStep<RotateRingCaptureContext>[] steps = new IInLevelSaveStep<RotateRingCaptureContext>[5]
	{
		new CaptureStep1_Header(),
		new CaptureStep2_Parts(),
		new CaptureStep3_Acorns(),
		new CaptureStep4_Bombs(),
		new CaptureStep5_StepLimit()
	};

	public void Build(RotateRingCaptureContext ctx)
	{
		if (ctx?.Save != null && !((Object)(object)ctx.GameplayManager == (Object)null))
		{
			IInLevelSaveStep<RotateRingCaptureContext>[] array = steps;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].Execute(ctx);
			}
		}
	}
}
