using UnityEngine;
using XingYun.Feature;

namespace Game.Feature.Ad;

public class WinStreakInterAdFeature : BaseFeature, IHookOnLevelEnd
{
	private int winStreak = 4;

	private int winStreakThreshold = 3;

	private float lastShowTime = float.MinValue;

	private float cd = 60f;

	public override string Name => "连胜插屏";

	public override int Priority => 10;

	public override bool CheckEnabled()
	{
		return true;
	}

	public FeatureResult OnLevelEnd()
	{
		if (winStreak < winStreakThreshold)
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
