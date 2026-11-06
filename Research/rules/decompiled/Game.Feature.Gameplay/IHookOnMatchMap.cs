using Game.Feature.Demo;
using XingYun.Feature;

namespace Game.Feature.Gameplay;

public interface IHookOnMatchMap
{
	FeatureResult OnMatchMap(DemoMatchData data);
}
