using Rotaterings.Rank;
using Rotaterings.UI.RankList;
using XingYun.UI.Core;

namespace Game.Common.Services;

public class RankClaimHomePopupStep : IHomePopupStep, IActivityStep
{
	public int Priority => 50;

	public bool ShouldRun()
	{
		return IsRankClaimPopupNeeded();
	}

	public void Execute()
	{
		if (!IsRankClaimPopupNeeded())
		{
			HomePopupPipeline.ContinueNext();
			return;
		}
		RankActivityEntry.Service?.TryReportCampaignEnd();
		RankModelData rankModelData = GameApp.Proxy.Get<RankModelProxy>()?.Data;
		if (rankModelData == null || !rankModelData.IsPeriodSettlePending)
		{
			HomePopupPipeline.ContinueNext();
			return;
		}
		RankActivityEntry.OpenRankList((UILayer)2, new UIRankListParam
		{
			Content = ERankContent.Main,
			OnClosed = HomePopupPipeline.ContinueNext
		});
	}

	public static bool IsRankClaimPopupNeeded()
	{
		if (!RankActivityEntry.IsFeatureOpen())
		{
			return false;
		}
		RankActivityService service = RankActivityEntry.Service;
		if (service == null || !service.HasActivityRecord())
		{
			return false;
		}
		return (GameApp.Proxy.Get<RankModelProxy>()?.Data)?.IsPeriodSettlePending ?? false;
	}
}
