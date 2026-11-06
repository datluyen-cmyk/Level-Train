using UnityEngine;
using XingYun.Feature;

namespace Game.Feature.Ad;

public class LevelEndInterAdFeature : BaseFeature, IHookOnLevelEnd
{
	private float lastShowTime = float.MinValue;

	private int minPlayCount = 3;

	private float cd = 30f;

	private int currentLevel = 5;

	public override string Name => "关卡结束插屏";

	public override bool CheckEnabled()
	{
		return true;
	}

	public FeatureResult OnLevelEnd()
	{
		if (currentLevel < minPlayCount)
		{
			return FeatureResult.None;
		}
		if (Time.time - lastShowTime < cd)
		{
			return FeatureResult.None;
		}
		lastShowTime = Time.time;
		return FeatureResult.Done;
	}
}
