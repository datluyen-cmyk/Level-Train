using Rotaterings.Rank;
using XingYun.Experiment;

namespace Rotaterings.UI.RankList;

public static class RankCollectPositionExperiment
{
	public static int ReadLayout()
	{
		int num = FunctionDataCenter.Get<RankFunction>()?.InLevelCollectLayout ?? 0;
		if (num >= 0 && num <= 3)
		{
			return num;
		}
		return 0;
	}

	public static int ReadIconSide()
	{
		RankFunction rankFunction = FunctionDataCenter.Get<RankFunction>();
		if (rankFunction == null || rankFunction.RankCollectIconSide != 1)
		{
			return 0;
		}
		return 1;
	}

	public static RectTransformLayoutData ReadAnchor(int layout)
	{
		return layout switch
		{
			1 => RankCollectPositionExperiment1Data.RankStarAnchor, 
			2 => RankCollectPositionExperiment2Data.RankStarAnchor, 
			3 => RankCollectPositionExperiment3Data.RankStarAnchor, 
			_ => RankCollectPositionControlData.RankStarAnchor, 
		};
	}

	public static RankCollectPairPosition ResolveRank(bool topHeader, int formalSide)
	{
		RankModelProxy rankModelProxy = GameApp.Proxy.Get<RankModelProxy>();
		if (rankModelProxy == null || rankModelProxy.Data?.IsHalloweenTheme != true)
		{
			return RankCollectPositionExperiment4Data.Select(topHeader, formalSide);
		}
		return RankCollectPositionExperiment4Data.SelectHalloween(formalSide);
	}
}
