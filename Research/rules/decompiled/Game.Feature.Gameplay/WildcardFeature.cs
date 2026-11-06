using System.Collections.Generic;
using Game.Feature.Demo;
using XingYun.Feature;

namespace Game.Feature.Gameplay;

public class WildcardFeature : BaseFeature, IHookOnValidatePlacement, IHookOnCardPlaced
{
	private HashSet<int> wildcardIds;

	public override string Name => "万能牌";

	public override bool CheckEnabled()
	{
		wildcardIds = new HashSet<int> { 1001, 1002 };
		return true;
	}

	public FeatureResult OnValidatePlacement(DemoCard card, DemoSlotColumn column)
	{
		if (!wildcardIds.Contains(card.Id))
		{
			return FeatureResult.None;
		}
		return FeatureResult.Done;
	}

	public FeatureResult OnCardPlaced(DemoCard card, DemoSlotColumn column)
	{
		if (!wildcardIds.Contains(card.Id))
		{
			return FeatureResult.None;
		}
		return FeatureResult.Done;
	}
}
