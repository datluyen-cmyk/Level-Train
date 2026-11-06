using System;
using System.Collections.Generic;
using Example.Cfg;
using UnityEngine;
using XingYun.UI.Core;

namespace RotateRings.DailyFirstWin;

public sealed class DailyFirstWinService
{
	private const string TAG = "[DailyFirstWinService] ";

	private const string RewardReasonId = "每日首胜奖励";

	private const string MultiRewardReasonId = "每日首胜多倍领奖";

	private static readonly DailyFirstWinService _instance = new DailyFirstWinService();

	public static DailyFirstWinService Instance => _instance;

	public bool IsEnabled()
	{
		return GetProxy()?.IsEnabled() ?? false;
	}

	public long GetRemainSecondsUntilReset()
	{
		return GetProxy()?.GetRemainSecondsUntilNextRefresh() ?? 0;
	}

	public void RefreshOnHome()
	{
		GetProxy()?.TryPerformMidnightCheck();
	}

	public void RecordLevelVictory(bool isMainMode, int winLevelNum)
	{
		if (isMainMode && IsEnabled())
		{
			_ = GetProxy()?.RecordFirstWin() ?? false;
		}
	}

	public bool HasFirstWinThisSession()
	{
		return GetProxy()?.HasFirstWinThisSession ?? false;
	}

	public bool HasPendingReward()
	{
		return GetProxy()?.HasPendingReward() ?? false;
	}

	public bool TryAutoClaimPendingRewardSilent(out int grantedCoin, bool notifyShowCoin = false)
	{
		grantedCoin = 0;
		if (!CanAutoClaimPendingReward())
		{
			return false;
		}
		DailyFirstWinProxy proxy = GetProxy();
		if (proxy == null)
		{
			return false;
		}
		DailyFirstWinRewardInfo pendingRewardInfo = proxy.GetPendingRewardInfo();
		if (pendingRewardInfo.RewardConfigId > 0)
		{
			grantedCoin = DailyFirstWinModule.Host.SumRewardCoinCount(pendingRewardInfo.RewardConfigId);
		}
		if (!TryClaimPendingRewardToBackpack(notifyShowCoin))
		{
			return false;
		}
		ClearRewardClaimFlow();
		return true;
	}

	public bool TryAutoClaimOnLevelWinSettlement(out int grantedCoin)
	{
		grantedCoin = 0;
		if (!IsRewardClaimFlowActive())
		{
			return false;
		}
		return TryAutoClaimPendingRewardSilent(out grantedCoin);
	}

	public void TryRecoverInterruptedClaimOnHomeInit()
	{
		if (!HasPendingReward() && IsRewardClaimFlowActive())
		{
			ClearRewardClaimFlow();
		}
	}

	public void MarkRewardClaimFlowStarted()
	{
		DailyFirstWinData dailyFirstWinData = GetProxy()?.Data;
		if (dailyFirstWinData != null && !dailyFirstWinData.IsRewardClaimFlowActive)
		{
			dailyFirstWinData.IsRewardClaimFlowActive = true;
		}
	}

	public void ClearRewardClaimFlow()
	{
		DailyFirstWinData dailyFirstWinData = GetProxy()?.Data;
		if (dailyFirstWinData != null && dailyFirstWinData.IsRewardClaimFlowActive)
		{
			dailyFirstWinData.IsRewardClaimFlowActive = false;
		}
	}

	public bool IsRewardClaimFlowActive()
	{
		return GetProxy()?.Data?.IsRewardClaimFlowActive == true;
	}

	public bool OpenPendingRewardRecoveryUI(Action onClosed = null)
	{
		if (!IsEnabled() || !HasPendingReward() || IsRewardClaimFlowActive())
		{
			onClosed?.Invoke();
			return false;
		}
		MarkRewardClaimFlowStarted();
		OpenDailyFirstWinPanel(CreateOpenData(DailyFirstWinOpenMode.Settlement, onClosed));
		return true;
	}

	public bool OpenHomeUI()
	{
		if (!IsEnabled())
		{
			return false;
		}
		OpenDailyFirstWinPanel(CreateOpenData(DailyFirstWinOpenMode.Home));
		return true;
	}

	public DailyFirstWinOpenData CreateOpenData(DailyFirstWinOpenMode mode, Action onClosed = null)
	{
		DailyFirstWinProxy proxy = GetProxy();
		proxy?.TryPerformMidnightCheck();
		DailyFirstWinRewardInfo dailyFirstWinRewardInfo = proxy?.GetPendingRewardInfo() ?? default(DailyFirstWinRewardInfo);
		return new DailyFirstWinOpenData
		{
			Mode = mode,
			CurrentDay = (proxy?.GetCurrentDayInCycle() ?? 0),
			PeriodDays = (proxy?.GetPeriodDays() ?? 7),
			MilestoneDays = (proxy?.GetMilestoneDays() ?? Array.Empty<int>()),
			HasReward = (dailyFirstWinRewardInfo.RewardConfigId > 0),
			PendingRewardDay = ((dailyFirstWinRewardInfo.RewardConfigId > 0) ? (proxy?.Data.PendingRewardDay ?? 0) : 0),
			IsMultiplierMotivated = dailyFirstWinRewardInfo.IsMultiplierMotivated,
			HasFirstWinToday = (proxy?.HasFirstWinToday() ?? false),
			PuzzleId = (proxy?.GetCurrentPuzzleId() ?? 0),
			IsHalloweenPuzzleTheme = (proxy?.IsHalloweenPuzzleTheme() ?? false),
			PlayPieceUnlockAnim = false,
			OnClosed = onClosed
		};
	}

	public bool OpenPendingRewardClaimView(Action onClaimSucceeded, RectTransform collectTarget = null, RectTransform boxStartTarget = null, Action onFlyStart = null, Action onFirstPropArrive = null)
	{
		DailyFirstWinProxy proxy = GetProxy();
		if (proxy == null || !proxy.HasPendingReward())
		{
			return false;
		}
		DailyFirstWinRewardInfo pendingRewardInfo = proxy.GetPendingRewardInfo();
		if (pendingRewardInfo.RewardConfigId <= 0)
		{
			proxy.MarkPendingRewardClaimed();
			return false;
		}
		int pendingRewardDay = proxy.Data.PendingRewardDay;
		int rewardStage = ResolveRewardStage(pendingRewardDay, proxy.GetMilestoneDays());
		return DailyFirstWinModule.Host.OpenClaimRewardUI(pendingRewardInfo.RewardConfigId, rewardStage, pendingRewardDay, pendingRewardInfo.IsMultiplierMotivated, () => ClaimPendingReward(), ClaimPendingReward, onClaimSucceeded, collectTarget, boxStartTarget, delegate
		{
			MarkRewardClaimFlowStarted();
			DailyFirstWinModule.Host.ReportBtnClick("每日首胜-领奖界面展现");
		}, onFlyStart, onFirstPropArrive);
	}

	public bool ClaimPendingReward(int multiplier = 1, bool isFreeDoubleReward = false)
	{
		DailyFirstWinProxy proxy = GetProxy();
		if (proxy == null || !proxy.HasPendingReward())
		{
			return false;
		}
		DailyFirstWinRewardInfo pendingRewardInfo = proxy.GetPendingRewardInfo();
		if (pendingRewardInfo.RewardConfigId <= 0)
		{
			proxy.MarkPendingRewardClaimed();
			return false;
		}
		int pendingRewardDay = proxy.Data.PendingRewardDay;
		int num = Math.Max(1, multiplier);
		IDailyFirstWinHost host = DailyFirstWinModule.Host;
		int currentCoin = host.GetCurrentCoin();
		if (!host.GrantReward(pendingRewardInfo.RewardConfigId, num, (num > 1) ? "每日首胜多倍领奖" : "每日首胜奖励", BuildReasonInfo(pendingRewardDay, num), isFreeDoubleReward))
		{
			return false;
		}
		int num2 = host.GetCurrentCoin() - currentCoin;
		if (num2 > 0)
		{
			host.NotifyAddShowCoin(num2);
		}
		proxy.MarkPendingRewardClaimed();
		return true;
	}

	public bool OpenFirstWinSettlement(Action onClosed)
	{
		bool flag = HasFirstWinThisSession();
		if (!flag)
		{
			onClosed?.Invoke();
			return false;
		}
		DailyFirstWinProxy proxy = GetProxy();
		proxy?.ClearSessionFirstWinFlag();
		DailyFirstWinOpenData dailyFirstWinOpenData = CreateOpenData(DailyFirstWinOpenMode.Settlement, onClosed);
		dailyFirstWinOpenData.PlayPieceUnlockAnim = flag;
		if (dailyFirstWinOpenData.HasReward)
		{
			MarkRewardClaimFlowStarted();
		}
		if ((proxy?.UsePuzzleEntryPresentation() ?? false) && flag)
		{
			dailyFirstWinOpenData.RequireNewPieceBeforeProgress = true;
		}
		OpenDailyFirstWinPanel(dailyFirstWinOpenData);
		return true;
	}

	public DailyFirstWinNewPieceOpenData CreateNewPieceOpenData(DailyFirstWinOpenData settlementOpenData, Action onContinue)
	{
		DailyFirstWinProxy proxy = GetProxy();
		int num = settlementOpenData?.CurrentDay ?? 0;
		int puzzleId = settlementOpenData?.PuzzleId ?? 0;
		string pieceSpriteName = ResolvePieceSpriteName(proxy, puzzleId, num);
		return new DailyFirstWinNewPieceOpenData
		{
			PieceSpriteName = pieceSpriteName,
			PieceDay = num,
			OnContinue = onContinue
		};
	}

	public string ResolveDailyFirstWinUIPath()
	{
		DailyFirstWinProxy proxy = GetProxy();
		if (proxy == null || !proxy.UsePuzzleEntryPresentation())
		{
			return "UIDailyFirstWin";
		}
		if (!proxy.IsHalloweenPuzzleTheme())
		{
			return "UIDailyFirstWinPuzzle";
		}
		return "UIDailyFirstWinPuzzleHW";
	}

	private static DailyFirstWinProxy GetProxy()
	{
		return GameApp.Proxy.Get<DailyFirstWinProxy>();
	}

	private static void OpenDailyFirstWinPanel(DailyFirstWinOpenData openData)
	{
		string text = Instance.ResolveDailyFirstWinUIPath();
		GameApp.UI.Open(text, (UILayer)2, (object)openData, (Action)null);
	}

	private static string ResolvePieceSpriteName(DailyFirstWinProxy proxy, int puzzleId, int day)
	{
		if (proxy == null || puzzleId <= 0 || day <= 0)
		{
			return string.Empty;
		}
		DailyFirstWinPuzzleConfig dailyFirstWinPuzzleConfig = DailyFirstWinModule.Host?.GetPuzzleConfig(puzzleId);
		if (dailyFirstWinPuzzleConfig?.Pieces == null || dailyFirstWinPuzzleConfig.Pieces.Count == 0)
		{
			return string.Empty;
		}
		int num = day - 1;
		if (num < 0 || num >= dailyFirstWinPuzzleConfig.Pieces.Count)
		{
			return string.Empty;
		}
		return dailyFirstWinPuzzleConfig.Pieces[num];
	}

	private static bool CanAutoClaimPendingReward()
	{
		if (Instance.IsEnabled())
		{
			return Instance.HasPendingReward();
		}
		return false;
	}

	private bool TryClaimPendingRewardToBackpack(bool notifyShowCoin)
	{
		bool num = ClaimPendingReward();
		if (num && notifyShowCoin)
		{
			DailyFirstWinModule.Host.NotifyCoinChanged();
		}
		return num;
	}

	private static string BuildReasonInfo(int pendingRewardDay, int multiplier)
	{
		if (multiplier <= 1)
		{
			return $"day={pendingRewardDay}";
		}
		return $"day={pendingRewardDay},mult={multiplier}";
	}

	private static int ResolveRewardStage(int rewardDay, IReadOnlyList<int> milestoneDays)
	{
		if (milestoneDays == null || milestoneDays.Count == 0)
		{
			return 1;
		}
		List<int> list = new List<int>(milestoneDays);
		list.Sort();
		int num = list.IndexOf(rewardDay);
		return Math.Max(1, num + 1);
	}
}
