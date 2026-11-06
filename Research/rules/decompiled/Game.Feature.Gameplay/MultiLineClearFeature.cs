using System.Collections;
using Game.Feature.Demo;
using UnityEngine;
using XingYun.Feature;

namespace Game.Feature.Gameplay;

public class MultiLineClearFeature : BaseFeature, IHookOnMatchMap
{
	public override string Name => "多行消除彩虹";

	public override int Priority => 80;

	public override bool CheckEnabled()
	{
		return true;
	}

	public FeatureResult OnMatchMap(DemoMatchData data)
	{
		if (data.ClearedLines < 3)
		{
			return FeatureResult.None;
		}
		return FeatureResult.Yield(PlayRainbow());
	}

	private IEnumerator PlayRainbow()
	{
		yield return (object)new WaitForSeconds(2f);
	}
}
