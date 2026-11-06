using Rotaterings.Rank;
using Rotaterings.UI.RankList;
using XingYun.UI.Core;

namespace Game.Common.Services;

public class RankSettlementStep : ISettlementStep, IActivityStep
{
	public int Priority => 100;

	public bool ShouldRun()
	{
		if (!RankActivityEntry.IsFeatureOpen())
		{
			return false;
		}
		RankModelProxy rankModelProxy = GameApp.Proxy.Get<RankModelProxy>();
		RankModelData rankModelData = rankModelProxy?.Data;
		if (rankModelProxy == null || rankModelData == null)
		{
			return false;
		}
		if (rankModelData.IsPeriodSettlePending)
		{
			return true;
		}
		if (rankModelProxy.IsRankActiveThisRound() && rankModelData.CurrentSettleScore > 0)
		{
			return rankModelData.StarCountOneRound > 0;
		}
		return false;
	}

	public void Execute()
	{
		RankActivityEntry.OpenRankList((UILayer)2, new UIRankListParam
		{
			Content = ERankContent.Settle,
			OnClosed = SettlementPipeline.ContinueNext
		});
	}
}
