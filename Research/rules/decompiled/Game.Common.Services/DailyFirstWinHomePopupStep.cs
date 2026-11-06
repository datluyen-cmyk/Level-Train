using RotateRings.DailyFirstWin;

namespace Game.Common.Services;

public class DailyFirstWinHomePopupStep : IHomePopupStep, IActivityStep
{
	private const string Tag = "[DailyFirstWinHomePopupStep] ";

	public int Priority => 152;

	public bool ShouldRun()
	{
		bool flag = DailyFirstWinService.Instance.HasPendingReward();
		bool flag2 = DailyFirstWinService.Instance.IsRewardClaimFlowActive();
		if (DailyFirstWinService.Instance.IsEnabled() && flag)
		{
			return !flag2;
		}
		return false;
	}

	public void Execute()
	{
		DailyFirstWinService.Instance.OpenPendingRewardRecoveryUI(HomePopupPipeline.ContinueNext);
	}
}
