using Game.Feature.Demo;
using XingYun.Feature;

namespace Game.Feature.UI;

public interface IHookOnResultShow
{
	FeatureResult OnResultShow(DemoGameResultData data, DemoResultDisplayConfig display);
}
