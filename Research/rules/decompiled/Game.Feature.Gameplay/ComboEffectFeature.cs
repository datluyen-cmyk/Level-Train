using System.Collections;
using Game.Feature.Demo;
using UnityEngine;
using XingYun.Feature;

namespace Game.Feature.Gameplay;

public class ComboEffectFeature : BaseFeature, IHookOnMatchMap
{
	public override string Name => "Combo连击";

	public override int Priority => 50;

	public override bool CheckEnabled()
	{
		return true;
	}

	public FeatureResult OnMatchMap(DemoMatchData data)
	{
		if (data.ComboCount < 3)
		{
			return FeatureResult.None;
		}
		return FeatureResult.Yield(PlayCombo(data.ComboCount));
	}

	private IEnumerator PlayCombo(int combo)
	{
		yield return (object)new WaitForSeconds(1f);
	}
}
