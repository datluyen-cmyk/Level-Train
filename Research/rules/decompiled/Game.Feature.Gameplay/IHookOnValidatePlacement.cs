using Game.Feature.Demo;
using XingYun.Feature;

namespace Game.Feature.Gameplay;

public interface IHookOnValidatePlacement
{
	FeatureResult OnValidatePlacement(DemoCard card, DemoSlotColumn column);
}
