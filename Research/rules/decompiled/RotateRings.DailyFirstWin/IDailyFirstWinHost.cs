using System;
using System.Collections.Generic;
using Example.Cfg;
using UnityEngine;

namespace RotateRings.DailyFirstWin;

public interface IDailyFirstWinHost
{
	DailyFirstWinConfigSnapshot? GetConfig();

	bool IsExperimentPuzzleEntryMode();

	DailyFirstWinPuzzleConfig GetPuzzleConfig(int puzzleId);

	bool IsHalloweenFeatureEnabled();

	bool IsHalloweenPuzzleUIAvailable();

	DailyFirstWinPuzzleRotation GetPuzzleRotation(DailyFirstWinPuzzleTheme theme);

	IReadOnlyList<int> GetEffectivePuzzlePoolIds();

	IReadOnlyList<int> GetHalloweenPuzzlePoolIds();

	long GetCurrentTimestamp();

	bool IsSameDay(long ts1, long ts2);

	int GetDaysBetween(long startTs, long endTs);

	int GetPlayerCompletedMainLevel();

	bool GrantReward(int rewardConfigId, int multiplier, string reasonId, string reasonInfo, bool isFreeDoubleReward);

	List<RewardItemSnapshot> BuildRewardItems(int rewardConfigId);

	int SumRewardCoinCount(int rewardConfigId);

	bool OpenRewardPopup(int rewardConfigId, Action onClosed);

	bool OpenClaimRewardUI(int rewardConfigId, int rewardStage, int milestoneDay, bool enableMultiplier, Func<bool> claimHandler, Func<int, bool, bool> claimMultiplierHandler, Action onClaimSucceeded, RectTransform collectTarget, RectTransform boxStartTarget, Action onOpen, Action onFlyStart = null, Action onFirstPropArrive = null);

	int GetCurrentCoin();

	void NotifyCoinChanged();

	void NotifyAddShowCoin(int count);

	void ReportBtnClick(string label);
}
