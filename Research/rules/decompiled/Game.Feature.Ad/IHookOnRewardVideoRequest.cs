using System;
using XingYun.Feature;

namespace Game.Feature.Ad;

public interface IHookOnRewardVideoRequest
{
	FeatureResult OnRewardVideoRequest(string itemType, Action<bool> callback);
}
