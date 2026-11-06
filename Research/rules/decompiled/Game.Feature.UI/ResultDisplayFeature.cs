using Game.Feature.Demo;
using XingYun.Feature;

namespace Game.Feature.UI;

public class ResultDisplayFeature : BaseFeature, IHookOnResultShow
{
	private int rankThreshold = 5;

	private int winStreakThreshold = 3;

	public override string Name => "结算显示";

	public override bool CheckEnabled()
	{
		return true;
	}

	public FeatureResult OnResultShow(DemoGameResultData data, DemoResultDisplayConfig display)
	{
		display.ShowRankModule = data.Rank <= rankThreshold;
		display.ShowStreakModule = data.WinStreak >= winStreakThreshold;
		return FeatureResult.Done;
	}
}
