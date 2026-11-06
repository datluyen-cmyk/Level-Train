using System;
using Example.Cfg;
using RotateRings.Model;
using RotateRings.Prop;
using RotateRings.Services;
using UniLog;
using XingYun.Experiment;
using XingYun.Proxy;

namespace RotateRings.RewardedLuckyDraw;

public sealed class RewardedLuckyDrawProxy : BaseProxy
{
	private const string Tag = "[RewardedLuckyDraw]";

	private static int Session => GameApp.Proxy.Get<PlayerModelProxy>().Data.ColdLaunchCount;

	public RewardedLuckyDrawData Data { get; }

	public static int TodayLocalDate
	{
		get
		{
			DateTime now = DateTime.Now;
			return now.Year * 10000 + now.Month * 100 + now.Day;
		}
	}

	public bool HasShownToday => Data.LastShownLocalDate == TodayLocalDate;

	public RewardedLuckyDrawProxy()
		: base("RewardedLuckyDrawProxy")
	{
		Data = GameApp.Save.GetOrCreate<RewardedLuckyDrawData>();
	}

	public void RecordPropUse(EPropType propType, int count)
	{
		if (count > 0 && (propType == EPropType.Hammer || propType == EPropType.Skyrocket))
		{
			Data.TotalPropUses = (int)Math.Min(2147483647L, (long)Data.TotalPropUses + (long)count);
			Save();
		}
	}

	public void RecordVictory()
	{
		Data.VictoryCount++;
		Data.ExemptNextFrontAd = false;
		Save();
	}

	public void RecoverInterruptedPopup(int session)
	{
		if (Data.PopupUnresolved && Data.PopupSession != session)
		{
			if (!Data.AdCompleted && Data.LastInterruptedSession != Data.PopupSession)
			{
				Data.LastInterruptedSession = Data.PopupSession;
				RecordDismissal(Data.PopupSession);
			}
			Data.PopupUnresolved = false;
			if (!Data.AdCompleted)
			{
				ClearPendingDraw();
			}
			Save();
		}
	}

	public bool CanShow(int session)
	{
		if (Data.DismissCount < 4 && (Data.BlockedThroughSession <= 0 || session > Data.BlockedThroughSession) && (Data.LastShownVictory < 0 || Data.VictoryCount - Data.LastShownVictory >= 5) && !Data.AdCompleted)
		{
			return !HasShownToday;
		}
		return false;
	}

	public void MarkShown(int session)
	{
		Data.LastShownVictory = Data.VictoryCount;
		Data.LastShownLocalDate = TodayLocalDate;
		Data.PopupUnresolved = true;
		Data.PopupSession = session;
		Data.ExemptNextFrontAd = true;
		Data.ExemptVictory = Data.VictoryCount;
		Save();
	}

	public void Dismiss(int session)
	{
		if (Data.PopupUnresolved && !Data.AdCompleted)
		{
			RecordDismissal(session);
			Data.PopupUnresolved = false;
			ClearPendingDraw();
			Save();
		}
	}

	private void RecordDismissal(int session)
	{
		if (Data.DismissCount < 4)
		{
			Data.DismissCount++;
			int num = ((Data.DismissCount == 2) ? 5 : ((Data.DismissCount == 3) ? 10 : 0));
			if (num > 0)
			{
				Data.BlockedThroughSession = session + num - 1;
			}
		}
	}

	public string LockDraw(int slotId, int propType, int count)
	{
		if (!string.IsNullOrEmpty(Data.PendingTransactionId))
		{
			return Data.PendingTransactionId;
		}
		Data.DrawSequence = ReissueRewardService.GetNextRewardedLuckyDrawSequence(Data.DrawSequence);
		Data.PendingTransactionId = "rewarded_lucky_draw_" + Data.DrawSequence;
		Data.PendingSlotId = slotId;
		Data.PendingPropType = propType;
		Data.PendingCount = count;
		Data.AdCompleted = false;
		Save();
		return Data.PendingTransactionId;
	}

	public void MarkAdCompleted(string transactionId)
	{
		if (!(Data.PendingTransactionId != transactionId) && !Data.AdCompleted)
		{
			Data.AdCompleted = true;
			Data.PopupUnresolved = false;
			Save();
		}
	}

	public void MarkRewardGranted(string transactionId)
	{
		Data.FirstRewardClaimed = true;
		if (Data.DismissCount < 4)
		{
			Data.DismissCount = 0;
			Data.BlockedThroughSession = 0;
		}
		if (Data.PendingTransactionId == transactionId)
		{
			ClearPendingDraw();
		}
		Data.PopupUnresolved = false;
	}

	public bool ConsumeNextFrontAdExemption()
	{
		if (!Data.ExemptNextFrontAd)
		{
			return false;
		}
		bool result = Data.ExemptVictory == Data.VictoryCount;
		Data.ExemptNextFrontAd = false;
		Save();
		return result;
	}

	private void ClearPendingDraw()
	{
		Data.PendingTransactionId = null;
		Data.PendingSlotId = 0;
		Data.PendingPropType = 0;
		Data.PendingCount = 0;
		Data.AdCompleted = false;
	}

	public void RestoreOnColdStart()
	{
		RecoverInterruptedPopup(Session);
		ReserveCompletedReward();
	}

	public bool TryGetEligibleConfiguration(int completedPropUses, out RewardedLuckyDrawFunction config)
	{
		config = FunctionDataCenter.Get<RewardedLuckyDrawFunction>();
		if (config == null)
		{
			return false;
		}
		if (!config.FunctionEnable)
		{
			return false;
		}
		if ((config.PresentationType != 0 && config.PresentationType != 1) || (config.ProbabilityGroupId != 0 && config.ProbabilityGroupId != 1))
		{
			Log.LogError("[RewardedLuckyDraw]", "实验字段非法 presentation={0} probability={1}", new object[2] { config.PresentationType, config.ProbabilityGroupId });
			return false;
		}
		LevelModelProxy levelModelProxy = GameApp.Proxy.Get<LevelModelProxy>();
		PropModelProxy propModelProxy = GameApp.Proxy.Get<PropModelProxy>();
		if (levelModelProxy.MainLevel <= 10)
		{
			return false;
		}
		if (Data.TotalPropUses < 10)
		{
			return false;
		}
		int session = Session;
		if (!CanShow(session))
		{
			if (Data.DismissCount < 4 && (Data.BlockedThroughSession <= 0 || session > Data.BlockedThroughSession) && (Data.LastShownVictory < 0 || Data.VictoryCount - Data.LastShownVictory >= 5))
			{
				_ = Data.AdCompleted;
			}
			return false;
		}
		if (propModelProxy.GetPropCount(EPropType.Hammer) + propModelProxy.GetPropCount(EPropType.Skyrocket) > 10 && completedPropUses < 2)
		{
			return false;
		}
		return true;
	}

	public string LockDraw(RewardedLuckyDrawProbability[] slots)
	{
		if (string.IsNullOrEmpty(Data.PendingTransactionId))
		{
			RewardedLuckyDrawProbability rewardedLuckyDrawProbability = RewardedLuckyDrawRewardResolver.Resolve(slots, !Data.FirstRewardClaimed);
			LockDraw(rewardedLuckyDrawProbability.SlotId, rewardedLuckyDrawProbability.PropType, rewardedLuckyDrawProbability.Count);
		}
		return Data.PendingTransactionId;
	}

	public void CompleteAd(string transactionId)
	{
		MarkAdCompleted(transactionId);
		ReserveCompletedReward();
	}

	public void ReserveCompletedReward()
	{
		RewardedLuckyDrawData data = Data;
		if (data.AdCompleted && !string.IsNullOrEmpty(data.PendingTransactionId))
		{
			ReissueRewardService.ReserveRewardedLuckyDrawReward(data.PendingTransactionId, data.PendingPropType, data.PendingCount);
		}
	}

	private static void Save()
	{
		GameApp.Save.SaveAll();
	}
}
