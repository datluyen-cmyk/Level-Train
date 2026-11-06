using RotateRings.GamePlay.Services.LevelStepLimit;
using UniLog;
using XingYun.Command;

namespace RotateRings.Command.LevelStepLimit;

public class LevelStepLimitAddStepsCommand : BaseCommand
{
	public override void Execute(object param = null)
	{
		LevelStepLimitService levelStepLimitService = SingletonComponent<GameplayManager>.Instance?.GetGameService<LevelStepLimitService>();
		if (levelStepLimitService == null)
		{
			Log.LogError("[LevelStepLimitAddStepsCommand]", "LevelStepLimitService 未注册，无法加步");
		}
		else
		{
			levelStepLimitService.AddStepsFromAd();
		}
	}
}
