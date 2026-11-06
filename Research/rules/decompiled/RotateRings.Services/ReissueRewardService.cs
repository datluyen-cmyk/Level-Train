using System;
using System.Collections.Generic;
using Example.Cfg;
using RotateRings.Chest;
using RotateRings.DailyFirstWin;
using RotateRings.Prop;
using RotateRings.RewardedLuckyDraw;
using Rotaterings.Rank;
using UniLog;
using XingYun.Experiment;
using XingYun.Utility;

namespace RotateRings.Services;

public static class ReissueRewardService
{
	private const string Tag = "[ReissueRewardService]";

	private static UnclaimedRewardData Data
	{
		get
		{
			UnclaimedRewardData orCreate;
			UnclaimedRewardData result = (orCreate = GameApp.Save.GetOrCreate<UnclaimedRewardData>());
			if (orCreate.Transactions == null)
			{
				orCreate.Transactions = new List<ReissueRewardTransaction>();
			}
			return result;
		}
	}

	private static List<string> GrantedIds
	{
		get
		{
			PropModelData orCreate;
			PropModelData propModelData = (orCreate = GameApp.Save.GetOrCreate<PropModelData>());
			if (orCreate.GrantedRewardTransactionIds == null)
			{
				orCreate.GrantedRewardTransactionIds = new List<string>();
			}
			return propModelData.GrantedRewardTransactionIds;
		}
	}

	public static bool HasPendingReissueReward()
	{
		return GetPendingTransactions().Count > 0;
	}

	public static long GetNextRewardedLuckyDrawSequence(long currentSequence)
	{
		long lastSequence = Math.Max(0L, currentSequence);
		foreach (ReissueRewardTransaction transaction in Data.Transactions)
		{
			IncludeSequence(transaction?.TransactionId);
		}
		foreach (string grantedId in GrantedIds)
		{
			IncludeSequence(grantedId);
		}
		return checked(lastSequence + 1);
		void IncludeSequence(string transactionId)
		{
			if (!string.IsNullOrEmpty(transactionId) && transactionId.StartsWith("rewarded_lucky_draw_", StringComparison.Ordinal) && long.TryParse(transactionId.Substring("rewarded_lucky_draw_".Length), out var result))
			{
				lastSequence = Math.Max(lastSequence, result);
			}
		}
	}

	public static string GetChestTransactionId(int cycleIndex)
	{
		int stageRewardId = FunctionDataCenter.Get<ChestFunction>().StageRewardId;
		return $"chest:{stageRewardId}:{cycleIndex}";
	}

	public static void ReserveColdStartRewards()
	{
		GameApp.Proxy.Get<RewardedLuckyDrawProxy>().ReserveCompletedReward();
		ReserveClaimableChestReward(GameApp.Proxy.Get<ChestProxy>());
		ReserveDailyFirstWinForceQuitReward();
		TryAutoSettleInterruptedRankEnd();
	}

	private static void TryAutoSettleInterruptedRankEnd()
	{
		RankModelProxy rankModelProxy = GameApp.Proxy.Get<RankModelProxy>();
		RankModelData rankModelData = rankModelProxy?.Data;
		if (rankModelData == null || !rankModelData.IsPeriodSettlePending || !rankModelData.HasEnteredRewardPage)
		{
			return;
		}
		RankActivityEntry.Service?.TryReportCampaignEnd();
		int num = RankActivityService.CalcPlayerRanking(rankModelData);
		int rewardId = RewardService.GetRewardId(rankModelData);
		Dictionary<int, int> dictionary = ((rewardId > 0) ? GetRewardItems(rewardId) : new Dictionary<int, int>());
		if (rewardId <= 0 || dictionary.Count == 0)
		{
			rankModelProxy.MarkPeriodSettled();
			GameApp.Save.SaveAll();
			return;
		}
		string rankTransactionId = GetRankTransactionId(rankModelData, num, rewardId);
		ReissueRewardTransaction reissueRewardTransaction = ReserveTransaction(rankTransactionId, ReissueRewardSource.Rank, num.ToString(), rewardId, dictionary, 7, "排行榜活动奖励", $"第{num}名自动补发");
		if (GrantedIds.Contains(rankTransactionId))
		{
			reissueRewardTransaction.Status = ReissueRewardStatus.Consumed;
			rankModelProxy.MarkPeriodSettled();
			GameApp.Save.SaveAll();
		}
		else if (GrantTransactionById(rankTransactionId, 1))
		{
			rankModelProxy.MarkPeriodSettled();
			GameApp.Save.SaveAll();
		}
	}

	private static string GetRankTransactionId(RankModelData rankData, int rank, int rewardId)
	{
		return $"rank:{rankData.Period}:{rank}:{rewardId}";
	}

	private static void ReserveDailyFirstWinForceQuitReward()
	{
		DailyFirstWinService.Instance.TryRecoverInterruptedClaimOnHomeInit();
		if (DailyFirstWinService.Instance.IsRewardClaimFlowActive())
		{
			ReserveDailyFirstWinPendingReward(GameApp.Proxy.Get<DailyFirstWinProxy>());
		}
	}

	public static List<ReissueRewardTransaction> GetPendingTransactions()
	{
		List<ReissueRewardTransaction> list = new List<ReissueRewardTransaction>();
		List<string> grantedIds = GrantedIds;
		foreach (ReissueRewardTransaction transaction in Data.Transactions)
		{
			if (transaction != null && !string.IsNullOrEmpty(transaction.TransactionId) && !grantedIds.Contains(transaction.TransactionId))
			{
				list.Add(transaction);
			}
		}
		return list;
	}

	public static void ReserveClaimableChestReward(ChestProxy proxy, bool isAutoClaim = true, string manualRewardReasonDetail = null)
	{
		if (proxy != null && proxy.IsClaimable())
		{
			int currentCycleIndex = proxy.GetCurrentCycleIndex();
			int currentRewardConfigId = proxy.GetCurrentRewardConfigId();
			ReissueRewardTransaction reissueRewardTransaction = ReserveTransaction(GetChestTransactionId(currentCycleIndex), rewardReasonId: isAutoClaim ? "关卡宝箱自动补发" : "主关卡宝箱奖励", rewardReasonDetail: isAutoClaim ? $"第{currentCycleIndex}轮宝箱玩家未在结算页领取，自动补发单倍奖励" : (manualRewardReasonDetail ?? $"cycle={currentCycleIndex}"), source: ReissueRewardSource.Chest, sourceKey: currentCycleIndex.ToString(), rewardId: currentRewardConfigId, rewardItems: GetRewardItems(currentRewardConfigId), gainReason: 7);
			if (GrantedIds.Contains(reissueRewardTransaction.TransactionId))
			{
				reissueRewardTransaction.Status = ReissueRewardStatus.Consumed;
			}
			proxy.MarkClaimed(isAutoClaim);
			GameApp.Save.SaveAll();
		}
	}

	private static bool ReserveDailyFirstWinPendingReward(DailyFirstWinProxy proxy)
	{
		if (proxy == null || !proxy.HasPendingReward())
		{
			return false;
		}
		DailyFirstWinRewardInfo pendingRewardInfo = proxy.GetPendingRewardInfo();
		int pendingRewardDay = proxy.Data.PendingRewardDay;
		if (pendingRewardInfo.RewardConfigId <= 0)
		{
			proxy.MarkPendingRewardClaimed();
			DailyFirstWinService.Instance.ClearRewardClaimFlow();
			return false;
		}
		long lastWinTimestamp = proxy.Data.LastWinTimestamp;
		ReissueRewardTransaction reissueRewardTransaction = ReserveTransaction($"daily_first_win:{lastWinTimestamp}:{pendingRewardDay}", ReissueRewardSource.DailyFirstWin, pendingRewardDay.ToString(), pendingRewardInfo.RewardConfigId, GetRewardItems(pendingRewardInfo.RewardConfigId), 18, "每日首胜奖励", $"day={pendingRewardDay}");
		if (GrantedIds.Contains(reissueRewardTransaction.TransactionId))
		{
			reissueRewardTransaction.Status = ReissueRewardStatus.Consumed;
		}
		proxy.MarkPendingRewardClaimed();
		DailyFirstWinService.Instance.ClearRewardClaimFlow();
		return true;
	}

	public static bool GrantTransactions(IEnumerable<ReissueRewardTransaction> transactions)
	{
		PropModelProxy propModelProxy = GameApp.Proxy.Get<PropModelProxy>();
		if (propModelProxy == null)
		{
			Log.LogError("[ReissueRewardService]", "Grant failed: PropModelProxy is null");
			return false;
		}
		bool flag = false;
		List<string> grantedIds = GrantedIds;
		long currentTimestamp = Time.GetCurrentTimestamp();
		foreach (ReissueRewardTransaction transaction in transactions)
		{
			if (transaction != null && !string.IsNullOrEmpty(transaction.TransactionId) && GrantTransaction(propModelProxy, grantedIds, transaction, transaction.Multiplier, isFreeDoubleReward: false, currentTimestamp))
			{
				flag = true;
			}
		}
		if (flag)
		{
			GameApp.Save.SaveAll();
		}
		return true;
	}

	public static bool GrantTransactionById(string transactionId, int multiplier, bool isFreeDoubleReward = false)
	{
		PropModelProxy propModelProxy = GameApp.Proxy.Get<PropModelProxy>();
		if (propModelProxy == null)
		{
			Log.LogError("[ReissueRewardService]", "Grant failed: PropModelProxy is null");
			return false;
		}
		ReissueRewardTransaction reissueRewardTransaction = Data.Transactions.Find((ReissueRewardTransaction x) => x != null && x.TransactionId == transactionId);
		if (reissueRewardTransaction == null)
		{
			Log.LogError("[ReissueRewardService]", "Grant failed: transaction not found, id=" + transactionId);
			return false;
		}
		if (GrantTransaction(propModelProxy, GrantedIds, reissueRewardTransaction, multiplier, isFreeDoubleReward, Time.GetCurrentTimestamp()))
		{
			GameApp.Save.SaveAll();
		}
		return true;
	}

	private static bool GrantTransaction(PropModelProxy propProxy, List<string> grantedIds, ReissueRewardTransaction transaction, int multiplier, bool isFreeDoubleReward, long now)
	{
		if (grantedIds.Contains(transaction.TransactionId))
		{
			if (transaction.Status != ReissueRewardStatus.Consumed)
			{
				transaction.Status = ReissueRewardStatus.Consumed;
				transaction.ConsumedAt = now;
				return true;
			}
			return false;
		}
		if (transaction.RewardId <= 0 && (transaction.RewardItems == null || transaction.RewardItems.Count == 0))
		{
			Log.LogError("[ReissueRewardService]", "Grant failed: no reward data, transactionId=" + transaction.TransactionId);
			return false;
		}
		int num = Math.Max(1, multiplier);
		string text = (isFreeDoubleReward ? "免费翻倍" : "看广告翻倍");
		ReissueRewardSource source = transaction.Source;
		string customReasonInfo = ((source != ReissueRewardSource.Chest && source != ReissueRewardSource.Rank) ? transaction.RewardReasonDetail : ((num > 1) ? (transaction.RewardReasonId + "_" + text + "_" + transaction.RewardReasonDetail) : (transaction.RewardReasonId + "_" + transaction.RewardReasonDetail)));
		foreach (KeyValuePair<int, int> item in transaction.RewardItems ?? new Dictionary<int, int>())
		{
			if (item.Key > 0 && item.Value > 0)
			{
				EPropType ePropType = ((transaction.Source == ReissueRewardSource.DailyFirstWin) ? ((EPropType)item.Key) : propProxy.GetRewardPropType(item.Key));
				if (ePropType != EPropType.None)
				{
					int addCount = item.Value * num;
					propProxy.AddProp(ePropType, addCount, (TrackPropGainReason)transaction.GainReason, transaction.RewardReasonId, customReasonInfo);
				}
			}
		}
		grantedIds.Add(transaction.TransactionId);
		transaction.Status = ReissueRewardStatus.Consumed;
		transaction.ConsumedAt = now;
		if (transaction.Source == ReissueRewardSource.RewardedLuckyDraw)
		{
			GameApp.Proxy.Get<RewardedLuckyDrawProxy>().MarkRewardGranted(transaction.TransactionId);
		}
		return true;
	}

	private static ReissueRewardTransaction ReserveTransaction(string transactionId, ReissueRewardSource source, string sourceKey, int rewardId, Dictionary<int, int> rewardItems, int gainReason, string rewardReasonId, string rewardReasonDetail)
	{
		UnclaimedRewardData data = Data;
		ReissueRewardTransaction reissueRewardTransaction = data.Transactions.Find((ReissueRewardTransaction x) => x != null && x.TransactionId == transactionId);
		if (reissueRewardTransaction == null)
		{
			reissueRewardTransaction = new ReissueRewardTransaction
			{
				TransactionId = transactionId,
				Source = source,
				SourceKey = sourceKey,
				RewardId = rewardId,
				RewardItems = rewardItems,
				Multiplier = 1,
				GainReason = gainReason,
				RewardReasonId = rewardReasonId,
				RewardReasonDetail = rewardReasonDetail,
				Status = ReissueRewardStatus.Reserved,
				ReservedAt = Time.GetCurrentTimestamp()
			};
			data.Transactions.Add(reissueRewardTransaction);
		}
		else
		{
			reissueRewardTransaction.RewardId = rewardId;
			reissueRewardTransaction.RewardItems = rewardItems;
			reissueRewardTransaction.Multiplier = 1;
			reissueRewardTransaction.GainReason = gainReason;
			reissueRewardTransaction.RewardReasonId = rewardReasonId;
			reissueRewardTransaction.RewardReasonDetail = rewardReasonDetail;
		}
		return reissueRewardTransaction;
	}

	public static void ReserveRewardedLuckyDrawReward(string transactionId, int propType, int count)
	{
		if (string.IsNullOrEmpty(transactionId) || (propType != 201 && propType != 202) || count <= 0)
		{
			throw new ArgumentException("Invalid rewarded lucky draw reward");
		}
		ReserveTransaction(transactionId, ReissueRewardSource.RewardedLuckyDraw, transactionId, 0, new Dictionary<int, int> { [propType] = count }, 3, "结算随机道具奖励", transactionId);
		if (GrantedIds.Contains(transactionId))
		{
			GameApp.Proxy.Get<RewardedLuckyDrawProxy>().MarkRewardGranted(transactionId);
		}
		GameApp.Save.SaveAll();
	}

	private static Dictionary<int, int> GetRewardItems(int rewardId)
	{
		RewardConfig rewardConfig = GameApp.Config.GetTable<ExampleTables>()?.TbRewardConfig?.Get(rewardId);
		if (rewardConfig?.get == null || rewardConfig.get.Count == 0)
		{
			return new Dictionary<int, int>();
		}
		return new Dictionary<int, int>(rewardConfig.get);
	}
}
