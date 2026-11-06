using RotateRings.DailyFirstWin;

namespace Game.Common.Services;

public class DailyFirstWinSettlementStep : ISettlementStep, IActivityStep
{
	public int Priority => 50;

	public bool ShouldRun()
	{
		return DailyFirstWinService.Instance.HasFirstWinThisSession();
	}

	public void Execute()
	{
		DailyFirstWinService.Instance.OpenFirstWinSettlement(SettlementPipeline.ContinueNext);
	}
}
