using System.Collections;
using Game.Feature.Demo;
using UnityEngine;
using XingYun.Feature;

namespace Game.Feature.Gameplay;

public class ClearEffectFeature : BaseFeature, IHookOnMatchMap
{
	public override string Name => "清屏特效";

	public override int Priority => 100;

	public override bool CheckEnabled()
	{
		return true;
	}

	public FeatureResult OnMatchMap(DemoMatchData data)
	{
		if (data.ClearedCount < data.TotalCount)
		{
			return FeatureResult.None;
		}
		return FeatureResult.Yield(PlayEffect());
	}

	private IEnumerator PlayEffect()
	{
		yield return (object)new WaitForSeconds(1.5f);
	}
}
