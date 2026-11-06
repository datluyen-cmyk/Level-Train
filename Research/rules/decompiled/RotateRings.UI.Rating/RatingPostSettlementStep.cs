using Game.Common.Services;

namespace RotateRings.UI.Rating;

public sealed class RatingPostSettlementStep : IPostSettlementStep, IActivityStep
{
	public int Priority => 2147483646;

	public bool ShouldRun()
	{
		return RatingPopupService.ShouldShowPostSettlement();
	}

	public void Execute()
	{
		RatingPopupService.ShowPostSettlement(PostSettlementPipeline.ContinueNext);
	}
}
