using XingYun.Feature;

namespace Game.Feature.Gameplay;

public class SmallRewardFeature : BaseFeature, IHookOnSettle
{
	public override string Name => "小奖励";

	public override int Priority => 100;

	public int SettleOrder => 1;

	public override bool CheckEnabled()
	{
		return true;
	}

	public void Settle()
	{
	}
}
