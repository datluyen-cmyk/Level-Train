using System;
using System.Collections.Generic;
using Example.Cfg;
using RotateRings.UI.ClaimReward;
using UniLog;
using XingYun.UI.Core;

namespace Rotaterings.Rank;

public static class RewardService
{
	private const string Tag = "RankReward";

	public static bool HasReward(RankModelData data)
	{
		if (data == null || data.IsEndSettlementProcessed)
		{
			return false;
		}
		return GetRankRewards(RankActivityService.CalcPlayerRanking(data)) != null;
	}

	public static bool ClaimReward(RankModelData data, int multiplier = 1, bool isFreeDoubleReward = false)
	{
		if (data == null || data.IsEndSettlementProcessed)
		{
			return false;
		}
		int num = RankActivityService.CalcPlayerRanking(data);
		if (num < 1 || num > RankConst.CurrentRewardRankLimit)
		{
			return false;
		}
		return GiveReward(GetRankRewards(num), reasonId: isFreeDoubleReward ? "排行榜活动奖励" : null, reasonInfo: isFreeDoubleReward ? $"第{num}名" : null, multiplier: multiplier, onSuccess: delegate
		{
			GameApp.Proxy.Get<RankModelProxy>()?.SetClaimState(isClaim: true);
		}, isFreeDoubleReward: isFreeDoubleReward);
	}

	public static bool GiveReward(RewardConfig rewardCfg, int multiplier = 1, Action onSuccess = null, string reasonId = null, string reasonInfo = null, bool isFreeDoubleReward = false)
	{
		if (rewardCfg?.get == null || multiplier <= 0)
		{
			Log.LogError("RankReward", "GiveReward failed: invalid reward config or multiplier");
			return false;
		}
		if (!ClaimRewardService.Grant(new UIClaimRewardParam
		{
			RewardItems = new Dictionary<int, int>(rewardCfg.get),
			RewardReasonId = reasonId,
			RewardReasonDetail = reasonInfo,
			IsFreeDoubleReward = isFreeDoubleReward
		}, multiplier))
		{
			return false;
		}
		onSuccess?.Invoke();
		return true;
	}

	public static RewardConfig GetRankRewards(int rank)
	{
		RankListReward rankRewardEntry = GetRankRewardEntry(rank);
		if (rankRewardEntry == null)
		{
			return null;
		}
		RewardConfig rewardConfig = GameApp.Config.GetTable<ExampleTables>()?.TbRewardConfig?.Get(rankRewardEntry.Reward);
		if (rewardConfig == null)
		{
			Log.LogError("RankReward", "奖励配置不存在: group={0}, rank={1}, rewardId={2}", new object[3] { rankRewardEntry.RewardGroup, rank, rankRewardEntry.Reward });
		}
		return rewardConfig;
	}

	private static RankListReward GetRankRewardEntry(int rank)
	{
		if (rank < 1 || rank > RankConst.CurrentRewardRankLimit)
		{
			return null;
		}
		List<RankListReward> list = GameApp.Config.GetTable<ExampleTables>()?.TbRankListReward?.DataList;
		if (list == null)
		{
			Log.LogError("RankReward", "排行榜奖励表不可用");
			return null;
		}
		int currentRewardConfig = RankConst.CurrentRewardConfig;
		for (int i = 0; i < list.Count; i++)
		{
			RankListReward rankListReward = list[i];
			if (rankListReward != null && rankListReward.RewardGroup == currentRewardConfig && rankListReward.Rank == rank)
			{
				return rankListReward;
			}
		}
		return null;
	}

	public static int GetRewardId(RankModelData data)
	{
		if (!HasReward(data))
		{
			return 0;
		}
		return GetRankRewardEntry(RankActivityService.CalcPlayerRanking(data))?.Reward ?? 0;
	}

	public static void PopupClaimRewardUI(object param = null)
	{
		GameApp.UI.Open("UIClaimReward", (UILayer)2, param, (Action)null);
	}
}
