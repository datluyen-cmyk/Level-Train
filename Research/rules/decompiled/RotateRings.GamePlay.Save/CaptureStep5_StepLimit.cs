using RotateRings.GamePlay.Services.LevelStepLimit;

namespace RotateRings.GamePlay.Save;

internal sealed class CaptureStep5_StepLimit : IInLevelSaveStep<RotateRingCaptureContext>
{
	public void Execute(RotateRingCaptureContext ctx)
	{
		LevelStepLimitService gameService = ctx.GameplayManager.GetGameService<LevelStepLimitService>();
		if (gameService == null || !gameService.IsEnabled)
		{
			ctx.Save.StepLimitRemainingSteps = -1;
		}
		else
		{
			ctx.Save.StepLimitRemainingSteps = gameService.RemainingSteps;
		}
	}
}
