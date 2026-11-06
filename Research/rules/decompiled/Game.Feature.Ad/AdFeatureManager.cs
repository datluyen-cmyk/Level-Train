using System;
using System.Collections.Generic;
using XingYun.Feature;

namespace Game.Feature.Ad;

public class AdFeatureManager : FeatureManager<AdFeatureManager>
{
	public override string GroupName => "Ad";

	protected override IEnumerable<Type> GetFeatureTypes()
	{
		return new Type[4]
		{
			typeof(LevelEndInterAdFeature),
			typeof(WinStreakInterAdFeature),
			typeof(RewardVideoFeature),
			typeof(RotateRingsInterstitialAdFeature)
		};
	}

	public RotateRingsInterstitialAdFeature GetRotateRingsInterstitialAdFeature()
	{
		List<IRotateRingsInterstitialAdGuardQuery> hooks = base.GetHooks<IRotateRingsInterstitialAdGuardQuery>();
		if (hooks.Count <= 0)
		{
			return null;
		}
		return hooks[0] as RotateRingsInterstitialAdFeature;
	}

	public FeatureResult OnLevelEnd()
	{
		return base.FireFirst<IHookOnLevelEnd>((Func<IHookOnLevelEnd, FeatureResult>)((IHookOnLevelEnd f) => f.OnLevelEnd()));
	}

	public FeatureResult OnRewardVideoRequest(string itemType, Action<bool> callback)
	{
		return base.FireFirst<IHookOnRewardVideoRequest>((Func<IHookOnRewardVideoRequest, FeatureResult>)((IHookOnRewardVideoRequest f) => f.OnRewardVideoRequest(itemType, callback)));
	}
}
