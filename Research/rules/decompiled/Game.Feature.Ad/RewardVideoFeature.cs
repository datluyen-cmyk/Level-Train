using System;
using XingYun.Feature;

namespace Game.Feature.Ad;

public class RewardVideoFeature : BaseFeature, IHookOnRewardVideoRequest
{
	private int todayVideoCount;

	private int dailyLimit = 5;

	private int rewardCount = 1;

	public override string Name => "激励视频道具";

	public override bool CheckEnabled()
	{
		return true;
	}

	public FeatureResult OnRewardVideoRequest(string itemType, Action<bool> callback)
	{
		if (todayVideoCount >= dailyLimit)
		{
			return FeatureResult.None;
		}
		todayVideoCount++;
		callback?.Invoke(obj: true);
		return FeatureResult.Done;
	}
}
