using Game.Feature.Demo;
using XingYun.Feature;

namespace Game.Feature.Gameplay;

public interface IHookOnCardPlaced
{
	FeatureResult OnCardPlaced(DemoCard card, DemoSlotColumn column);
}
