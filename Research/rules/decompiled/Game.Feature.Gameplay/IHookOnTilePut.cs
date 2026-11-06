using Game.Feature.Demo;
using XingYun.Feature;

namespace Game.Feature.Gameplay;

public interface IHookOnTilePut
{
	FeatureResult OnTilePut(DemoTilePutInfo info);
}
