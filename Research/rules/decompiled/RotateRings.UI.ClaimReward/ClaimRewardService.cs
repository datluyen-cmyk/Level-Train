using System.Collections.Generic;
using Example.Cfg;
using RotateRings.Prop;
using UniLog;

namespace RotateRings.UI.ClaimReward;

public static class ClaimRewardService
{
	private const string Tag = "ClaimReward";

	public static bool Grant(UIClaimRewardParam param, int targetMultiplier = 1)
	{
		if (param == null)
		{
			Log.LogError("ClaimReward", "Grant failed: param is null");
			return false;
		}
		int num = targetMultiplier - param.GrantedMultiplier;
		if (num <= 0)
		{
			return true;
		}
		if (param.RewardId <= 0 && (param.RewardItems == null || param.RewardItems.Count == 0) && (param.ExtraRewardItems == null || param.ExtraRewardItems.Count == 0))
		{
			Log.LogError("ClaimReward", $"Grant failed: no reward data, RewardId={param.RewardId}");
			return false;
		}
		PropModelProxy propModelProxy = GameApp.Proxy.Get<PropModelProxy>();
		if (propModelProxy == null)
		{
			Log.LogError("ClaimReward", "Grant failed: PropModelProxy is null");
			return false;
		}
		string rewardReasonId = param.RewardReasonId;
		string reasonInfo = BuildReasonInfo(param, targetMultiplier);
		Dictionary<int, int> dictionary = null;
		if (param.RewardId > 0)
		{
			dictionary = GetRewardConfig(param.RewardId);
			if (dictionary != null)
			{
				GrantConfigReward(propModelProxy, dictionary, num, rewardReasonId, reasonInfo);
			}
		}
		GrantRewardItems(propModelProxy, param, num, dictionary, rewardReasonId, reasonInfo);
		GrantExtraRewardItems(propModelProxy, param, num, rewardReasonId, reasonInfo);
		param.GrantedMultiplier = targetMultiplier;
		return true;
	}

	public static void RollbackGrant(UIClaimRewardParam param, int previousGrantedMultiplier)
	{
		if (param == null)
		{
			return;
		}
		int num = param.GrantedMultiplier - previousGrantedMultiplier;
		if (num <= 0)
		{
			return;
		}
		PropModelProxy propModelProxy = GameApp.Proxy.Get<PropModelProxy>();
		if (propModelProxy == null)
		{
			return;
		}
		Dictionary<int, int> dictionary = null;
		if (param.RewardId > 0)
		{
			dictionary = GetRewardConfig(param.RewardId);
			if (dictionary != null)
			{
				RollbackConfigReward(propModelProxy, dictionary, num);
			}
		}
		RollbackRewardItems(propModelProxy, param, num, dictionary);
		RollbackExtraRewardItems(propModelProxy, param, num);
		param.GrantedMultiplier = previousGrantedMultiplier;
	}

	private static string BuildReasonInfo(UIClaimRewardParam param, int targetMultiplier)
	{
		if (string.IsNullOrEmpty(param.RewardReasonId) || string.IsNullOrEmpty(param.RewardReasonDetail))
		{
			return null;
		}
		if (targetMultiplier <= 1)
		{
			return param.RewardReasonId + "_" + param.RewardReasonDetail;
		}
		string text = (param.IsFreeDoubleReward ? "免费翻倍" : "看广告翻倍");
		return param.RewardReasonId + "_" + text + "_" + param.RewardReasonDetail;
	}

	private static void GrantConfigReward(PropModelProxy propProxy, Dictionary<int, int> rewardItems, int deltaMultiplier, string reasonId, string reasonInfo)
	{
		foreach (KeyValuePair<int, int> rewardItem in rewardItems)
		{
			if (rewardItem.Key > 0 && rewardItem.Value > 0)
			{
				int addCount = rewardItem.Value * deltaMultiplier;
				EPropType rewardPropType = propProxy.GetRewardPropType(rewardItem.Key);
				if (rewardPropType != EPropType.None)
				{
					propProxy.AddProp(rewardPropType, addCount, TrackPropGainReason.LevelSuccessReward, reasonId, reasonInfo);
				}
			}
		}
	}

	private static void GrantRewardItems(PropModelProxy propProxy, UIClaimRewardParam param, int deltaMultiplier, Dictionary<int, int> configItems, string reasonId, string reasonInfo)
	{
		if (param.RewardItems == null || param.RewardItems.Count == 0)
		{
			return;
		}
		foreach (KeyValuePair<int, int> rewardItem in param.RewardItems)
		{
			if (rewardItem.Key > 0 && rewardItem.Value > 0 && (configItems == null || !configItems.ContainsKey(rewardItem.Key)))
			{
				int addCount = rewardItem.Value * deltaMultiplier;
				EPropType rewardPropType = propProxy.GetRewardPropType(rewardItem.Key);
				if (rewardPropType != EPropType.None)
				{
					propProxy.AddProp(rewardPropType, addCount, TrackPropGainReason.LevelSuccessReward, reasonId, reasonInfo);
				}
			}
		}
	}

	private static void GrantExtraRewardItems(PropModelProxy propProxy, UIClaimRewardParam param, int deltaMultiplier, string reasonId, string reasonInfo)
	{
		if (param.ExtraRewardItems == null || param.ExtraRewardItems.Count == 0)
		{
			return;
		}
		foreach (KeyValuePair<int, int> extraRewardItem in param.ExtraRewardItems)
		{
			if (extraRewardItem.Key > 0 && extraRewardItem.Value > 0)
			{
				int addCount = extraRewardItem.Value * deltaMultiplier;
				EPropType rewardPropType = propProxy.GetRewardPropType(extraRewardItem.Key);
				if (rewardPropType != EPropType.None)
				{
					propProxy.AddProp(rewardPropType, addCount, TrackPropGainReason.LevelSuccessReward, reasonId, reasonInfo);
				}
			}
		}
	}

	private static void RollbackConfigReward(PropModelProxy propProxy, Dictionary<int, int> rewardItems, int deltaMultiplier)
	{
		foreach (KeyValuePair<int, int> rewardItem in rewardItems)
		{
			if (rewardItem.Key > 0 && rewardItem.Value > 0)
			{
				int costCount = rewardItem.Value * deltaMultiplier;
				EPropType rewardPropType = propProxy.GetRewardPropType(rewardItem.Key);
				if (rewardPropType != EPropType.None)
				{
					propProxy.UseProp(rewardPropType, costCount, TrackPropUseReason.None);
				}
			}
		}
	}

	private static void RollbackRewardItems(PropModelProxy propProxy, UIClaimRewardParam param, int deltaMultiplier, Dictionary<int, int> configItems)
	{
		if (param.RewardItems == null || param.RewardItems.Count == 0)
		{
			return;
		}
		foreach (KeyValuePair<int, int> rewardItem in param.RewardItems)
		{
			if (rewardItem.Key > 0 && rewardItem.Value > 0 && (configItems == null || !configItems.ContainsKey(rewardItem.Key)))
			{
				int costCount = rewardItem.Value * deltaMultiplier;
				EPropType rewardPropType = propProxy.GetRewardPropType(rewardItem.Key);
				if (rewardPropType != EPropType.None)
				{
					propProxy.UseProp(rewardPropType, costCount, TrackPropUseReason.None);
				}
			}
		}
	}

	private static void RollbackExtraRewardItems(PropModelProxy propProxy, UIClaimRewardParam param, int deltaMultiplier)
	{
		if (param.ExtraRewardItems == null || param.ExtraRewardItems.Count == 0)
		{
			return;
		}
		foreach (KeyValuePair<int, int> extraRewardItem in param.ExtraRewardItems)
		{
			if (extraRewardItem.Key > 0 && extraRewardItem.Value > 0)
			{
				int costCount = extraRewardItem.Value * deltaMultiplier;
				EPropType rewardPropType = propProxy.GetRewardPropType(extraRewardItem.Key);
				if (rewardPropType != EPropType.None)
				{
					propProxy.UseProp(rewardPropType, costCount, TrackPropUseReason.None);
				}
			}
		}
	}

	private static Dictionary<int, int> GetRewardConfig(int rewardId)
	{
		return (GameApp.Config.GetTable<ExampleTables>()?.TbRewardConfig?.Get(rewardId))?.get;
	}
}
