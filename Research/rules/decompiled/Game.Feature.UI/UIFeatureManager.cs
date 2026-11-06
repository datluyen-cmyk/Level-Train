using System;
using System.Collections.Generic;
using Game.Feature.Demo;
using XingYun.Feature;

namespace Game.Feature.UI;

public class UIFeatureManager : FeatureManager<UIFeatureManager>
{
	public override string GroupName => "UI";

	protected override IEnumerable<Type> GetFeatureTypes()
	{
		return new Type[1] { typeof(ResultDisplayFeature) };
	}

	public void OnResultShow(DemoGameResultData data, DemoResultDisplayConfig display)
	{
		base.FireAll<IHookOnResultShow>((Func<IHookOnResultShow, FeatureResult>)((IHookOnResultShow f) => f.OnResultShow(data, display)));
	}
}
