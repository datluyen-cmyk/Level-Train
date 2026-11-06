using RotateRings.GamePlay.Services.LevelStepLimit;
using UniLog;
using XingYun.Command;

namespace RotateRings.Command.LevelStepLimit;

public class LevelStepLimitClaimFreeGiftCommand : BaseCommand
{
	public override void Execute(object param = null)
	{
		LevelStepLimitService levelStepLimitService = SingletonComponent<GameplayManager>.Instance?.GetGameService<LevelStepLimitService>();
		if (levelStepLimitService == null)
		{
			Log.LogError("[LevelStepLimitClaimFreeGiftCommand]", "LevelStepLimitService 未注册，无法免费赠送");
		}
		else
		{
			levelStepLimitService.ClaimFreeGift();
		}
	}
}
