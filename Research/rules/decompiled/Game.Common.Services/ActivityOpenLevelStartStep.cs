using Rotaterings.Rank;
using XingYun.UI.Core;

namespace Game.Common.Services;

public class ActivityOpenLevelStartStep : ILevelStartStep, IActivityStep
{
	private const string Tag = "[ActivityOpenLevelStartStep] ";

	public int Priority => -10;

	public bool ShouldRun()
	{
		RankModelProxy rankModelProxy = GameApp.Proxy.Get<RankModelProxy>();
		if (rankModelProxy == null || !rankModelProxy.IsRankActiveThisRound())
		{
			return false;
		}
		return RankActivityEntry.CanShowActivityOpenPopup();
	}

	public void Execute()
	{
		RankActivityEntry.CloseAnyRankListPopup();
		int round = LevelStartPreEnterPipeline.CurrentRound;
		if (!RankActivityEntry.TryShowActivityOpenPopup(delegate
		{
			LevelStartPreEnterPipeline.ContinueNextForRound(round);
		}, enterLevelOnSubmit: false, (UILayer)4))
		{
			LevelStartPreEnterPipeline.ContinueNext();
		}
		else if (!RankActivityEntry.IsAnyRankListPopupOpen())
		{
			LevelStartPreEnterPipeline.ContinueNext();
		}
	}
}
