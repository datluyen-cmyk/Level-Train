using XingYun.Feature;

namespace Game.Feature.Gameplay;

public class BigRewardFeature : BaseFeature, IHookOnSettle
{
	public override string Name => "大奖励";

	public override int Priority => 10;

	public int SettleOrder => 2;

	public override bool CheckEnabled()
	{
		return true;
	}

	public void Settle()
	{
	}
}
