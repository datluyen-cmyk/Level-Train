using RotateRings.UI.RewardedLuckyDraw;

namespace Game.Common.Services;

public sealed class RewardedLuckyDrawPostSettlementStep : IPostSettlementStep, IActivityStep
{
	public int Priority => 100;

	public bool ShouldRun()
	{
		return RewardedLuckyDrawService.HasPreparedPopup;
	}

	public void Execute()
	{
		RewardedLuckyDrawService.OpenPreparedPopup(PostSettlementPipeline.ContinueNext);
	}
}
