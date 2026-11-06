using Game.Feature.Demo;
using XingYun.Feature;

namespace Game.Feature.Gameplay;

public interface IHookOnLevelStart
{
	FeatureResult OnLevelStart(DemoLevelData data);
}
