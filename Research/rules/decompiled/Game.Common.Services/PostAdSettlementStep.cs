using RotateRings.Ad;
using RotateRings.UI.Rating;
using RotateRings.UI.RewardedLuckyDraw;

namespace Game.Common.Services;

public class PostAdSettlementStep : ISettlementStep, IActivityStep
{
	private const string TAG = "[PostAdSettlementStep] ";

	private const int PriorityValue = 20;

	public int Priority => 20;

	public bool ShouldRun()
	{
		bool num = RatingPopupService.PrepareForSettlement();
		if (num)
		{
			RewardedLuckyDrawService.AbortSettlement();
		}
		return !num;
	}

	public void Execute()
	{
		AdService.ShowPostAd(delegate
		{
			SettlementPipeline.ContinueNext();
		});
	}
}
