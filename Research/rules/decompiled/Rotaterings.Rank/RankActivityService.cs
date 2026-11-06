using System;
using System.Collections.Generic;
using Example.Cfg;
using Game.DataTrack;
using RotateRings.Common;
using RotateRings.Halloween;
using RotateRings.Model;
using RotateRings.SpecialLevel;
using Rotaterings.SO;
using Rotaterings.UI.RankList;
using UniLog;
using XingYun.Aot;
using XingYun.Utility;

namespace Rotaterings.Rank;

public sealed class RankActivityService
{
	private const string Tag = "Rank";

	private const int OpenScoreRequired = 3;

	private const long OfflineOpenScoreBypassSeconds = 43200L;

	private const int NormalLevelOpenScore = 1;

	private const int HardLevelOpenScore = 2;

	private readonly RankModelData data;

	private readonly ExampleTables tables;

	private readonly RankRobotScoreConfig robotScoreCfg;

	public bool IsPeriodSettlePending => data.IsPeriodSettlePending;

	public RankActivityService(RankModelData data, ExampleTables tables, RankRobotScoreConfig robotScoreCfg)
	{
		this.data = data ?? throw new ArgumentNullException("data");
		this.tables = tables ?? throw new ArgumentNullException("tables");
		this.robotScoreCfg = robotScoreCfg ?? throw new ArgumentNullException("robotScoreCfg");
	}

	public bool CanOpenRank()
	{
		if (!data.HasActivityRecord && SpecialLevelModule.IsSpecialLevelMode())
		{
			return false;
		}
		return (GameApp.Proxy.Get<LevelModelProxy>()?.MainLevel ?? 0) >= RankFunctionResolver.GetRankOpenLevel();
	}

	public bool TryResetRankStateIfBelowOpenLevel()
	{
		if (data == null || !data.HasActivityRecord)
		{
			return false;
		}
		if ((GameApp.Proxy.Get<LevelModelProxy>()?.MainLevel ?? 0) >= RankFunctionResolver.GetRankOpenLevel())
		{
			return false;
		}
		data.ResetToInitial();
		return true;
	}

	public void EnsureActivityState()
	{
		if (GetActivityState() == EnsureResult.Ongoing)
		{
			TryAutoUpdateRobots(Time.GetCurrentTimestamp());
		}
	}

	public EnsureResult GetActivityState(bool ignoreOpenScoreRequirement = false)
	{
		long currentTimestamp = Time.GetCurrentTimestamp();
		if (IsActivityOngoing(currentTimestamp))
		{
			return EnsureResult.Ongoing;
		}
		if (!HasActivityRecord())
		{
			return EnsureResult.None;
		}
		if (!data.IsEndSettlementProcessed)
		{
			if (!HasReward(data))
			{
				return EnsureResult.EndedNoReward;
			}
			return EnsureResult.EndedNeedReward;
		}
		if (data.NextOpenTimeStamp <= 0)
		{
			SetNextOpenTime();
		}
		if (!CanOpenNextActivity(currentTimestamp, ignoreOpenScoreRequirement))
		{
			return EnsureResult.InCooldown;
		}
		return EnsureResult.OpenedNew;
	}

	public bool IsActivityOngoing(long nowTimestamp)
	{
		if (data.EndTimeStamp > 0)
		{
			return nowTimestamp < data.EndTimeStamp;
		}
		return false;
	}

	public bool HasActivityRecord()
	{
		return data.HasActivityRecord;
	}

	public bool CanOpenNextActivity(long nowTimestamp, bool ignoreOpenScoreRequirement = false)
	{
		if (RankFunctionResolver.GetRankOpenInterval() == 1)
		{
			return data.WinsAfterPeriodSettled > 0;
		}
		if (RankFunctionResolver.GetRankOpenInterval() == 2)
		{
			if (!ignoreOpenScoreRequirement)
			{
				return data.OpenScoreAfterPeriodSettled >= 3;
			}
			return true;
		}
		if (data.NextOpenTimeStamp > 0)
		{
			return nowTimestamp >= data.NextOpenTimeStamp;
		}
		return true;
	}

	public void RecordCooldownExitTimestamp()
	{
		if (GetActivityState() == EnsureResult.InCooldown)
		{
			data.CooldownExitTimestamp = Time.GetCurrentTimestamp();
		}
	}

	public bool HasCooldownOfflineDurationReached()
	{
		if (GetActivityState() != EnsureResult.InCooldown || data.CooldownExitTimestamp <= 0)
		{
			return false;
		}
		return Time.GetCurrentTimestamp() - data.CooldownExitTimestamp >= 43200;
	}

	public bool EnsureBuildActivityData(bool ignoreOpenScoreRequirement = false)
	{
		switch (GetActivityState(ignoreOpenScoreRequirement))
		{
		case EnsureResult.Ongoing:
			return false;
		case EnsureResult.EndedNeedReward:
		case EnsureResult.EndedNoReward:
			return false;
		case EnsureResult.InCooldown:
			return false;
		default:
			return BuildActivityData();
		}
	}

	public bool BuildActivityData()
	{
		if (!TryAdvancePeriodConfig())
		{
			Log.LogError("Rank", "[RankActivityService] TbRankListActivity 为空，无法开启排行榜");
			return false;
		}
		long currentTimestamp = Time.GetCurrentTimestamp();
		long num = RankFunctionResolver.GetRankPeriodSeconds();
		long num2 = currentTimestamp + num;
		(int activeGamesTotal, int activeDays, int totalGames) recentActivityData = RankActivityDataHelper.GetRecentActivityData(robotScoreCfg.ACTIVE_DAYS_COUNT);
		int item = recentActivityData.activeGamesTotal;
		int item2 = recentActivityData.activeDays;
		int item3 = recentActivityData.totalGames;
		bool isHalloweenTheme = HalloweenFeatureService.IsRankEnabled();
		data.ResetForNewPeriod();
		data.IsHalloweenTheme = isHalloweenTheme;
		data.RankRewardConfig = RankFunctionResolver.GetRankRewardConfig();
		data.Period++;
		data.EndTimeStamp = num2;
		data.NextOpenTimeStamp = num2;
		data.ExpGames = RankRobotScoreCalculator.CalcExpGames(item, item2, item3, robotScoreCfg.ACTIVE_DAYS_COUNT, robotScoreCfg);
		data.ActivityActiveGamesTotal = item;
		data.ActivityActiveDays = item2;
		data.TimeRefresh = currentTimestamp;
		RankRobotFactory.CreateRobotsForNewActivity(RankConst.MaxRankBot, tables, data, robotScoreCfg);
		RRDataTrack.Instance.ReportChampaignStart(data.Period, SDKManager.GetActiveDays(), (GameApp.Proxy.Get<LevelModelProxy>()?.Data?.GetAllModeNum()).GetValueOrDefault(), -1);
		return true;
	}

	public void TryReportCampaignEnd()
	{
		if (!data.HasReportedCampaignEnd && data.Period > 0 && data.IsActivityEnd)
		{
			List<int> sortedScores = BuildSortedScores();
			int champaignEndRank = ((data.PlayerRankScore > 0) ? CalcPlayerRanking(data) : (-1));
			RRDataTrack.Instance.ReportChampaignEnd(data.Period, SDKManager.GetActiveDays(), (GameApp.Proxy.Get<LevelModelProxy>()?.Data?.GetAllModeNum()).GetValueOrDefault(), champaignEndRank, data.PlayerRankScore, GetScoreAtRank(sortedScores, 1), GetScoreAtRank(sortedScores, 2), GetScoreAtRank(sortedScores, 3), GetScoreAtRank(sortedScores, 10), data.ClearLevels, data.TotalCollectCards);
			data.HasReportedCampaignEnd = true;
		}
	}

	public void HandleLevelVictory(GameMode gameMode, int completedLevel)
	{
		TryResetRankStateIfBelowOpenLevel();
		EnsureResult activityState = GetActivityState();
		int rankOpenInterval = RankFunctionResolver.GetRankOpenInterval();
		if (activityState == EnsureResult.InCooldown && (rankOpenInterval == 1 || rankOpenInterval == 2))
		{
			if (rankOpenInterval == 1)
			{
				data.WinsAfterPeriodSettled++;
			}
			else
			{
				data.OpenScoreAfterPeriodSettled += GetOpenScoreForVictory(gameMode, completedLevel);
			}
			activityState = GetActivityState();
		}
		if ((activityState == EnsureResult.None || activityState == EnsureResult.OpenedNew) && CanOpenRank())
		{
			EnsureBuildActivityData();
		}
		SettleOnLevelVictory();
	}

	public void SettleOnLevelVictory()
	{
		long currentTimestamp = Time.GetCurrentTimestamp();
		RankModelProxy rankModelProxy = GameApp.Proxy.Get<RankModelProxy>();
		if (rankModelProxy == null || !rankModelProxy.IsRankActiveThisRound())
		{
			if (data.IsPeriodSettlePending)
			{
				data.CurrentSettleScore = 0;
				data.CurrentSettleRanking = CalcPlayerRanking(data);
			}
			return;
		}
		int preRank = CalcPlayerRanking(data);
		int num = RankScoreService.SettleOnVictory(data);
		int playerRankScore = data.PlayerRankScore;
		if (data.ClearLevels == 0)
		{
			RankRobotScoreCalculator.UpdateOnFirstClear(data.Robots, robotScoreCfg, new RankRobotScoreCalculator.RankRobotRefreshLogContext("首次通关", data.ExpGames, data.ActivityActiveGamesTotal, data.ActivityActiveDays, data.ClearLevels, data.ExpGames - data.ClearLevels, 0, 0, playerRankScore, preRank, num));
		}
		else
		{
			int levelGap = data.ExpGames - data.ClearLevels;
			RankRobotScoreCalculator.UpdateOnNonFirstClear(data.Robots, robotScoreCfg, new RankRobotScoreCalculator.RankRobotRefreshLogContext("非首次通关", data.ExpGames, data.ActivityActiveGamesTotal, data.ActivityActiveDays, data.ClearLevels, levelGap, 0, 0, playerRankScore, preRank, num));
		}
		data.CurrentSettleScore = num;
		data.PlayerRankScore += num;
		TryKickLowestRobot();
		data.ClearLevels++;
		data.TotalCollectCards += data.StarCountOneRound;
		data.TimeRefresh = currentTimestamp;
		data.CurrentSettleRanking = CalcPlayerRanking(data);
	}

	public void SetNextOpenTime()
	{
		if (data.IsActivityEnd)
		{
			data.NextOpenTimeStamp = data.EndTimeStamp;
		}
	}

	public RankListActivity GetCurRankListCfg()
	{
		List<RankListActivity> list = tables.TbRankListActivity?.DataList;
		if (list == null || list.Count == 0)
		{
			return null;
		}
		int lastPeriodIndex = data.LastPeriodIndex;
		if (lastPeriodIndex < 0 || lastPeriodIndex >= list.Count)
		{
			return null;
		}
		return list[lastPeriodIndex];
	}

	public long GetRemainSeconds()
	{
		if (data.EndTimeStamp <= 0)
		{
			return 0L;
		}
		long num = data.EndTimeStamp - Time.GetCurrentTimestamp();
		if (num <= 0)
		{
			return 0L;
		}
		return num;
	}

	public List<int> BuildSortedScores()
	{
		if (data.Robots == null)
		{
			return new List<int>(0);
		}
		List<int> list = new List<int>(data.Robots.Count + 1);
		for (int i = 0; i < data.Robots.Count; i++)
		{
			if (data.Robots[i] != null)
			{
				list.Add(data.Robots[i].Score);
			}
		}
		if (data.PlayerRankScore > 0)
		{
			list.Add(data.PlayerRankScore);
		}
		list.Sort((int a, int b) => b.CompareTo(a));
		return list;
	}

	public int CalcRankingByScore(int score)
	{
		return CalcRankingByScore(score, data);
	}

	public static int GetScoreAtRank(List<int> sortedScores, int rank)
	{
		int num = rank - 1;
		if (num < 0 || num >= sortedScores.Count)
		{
			return 0;
		}
		return sortedScores[num];
	}

	public static int CalcPlayerRanking(RankModelData data)
	{
		if (data?.Robots == null || data.Robots.Count == 0)
		{
			return RankConst.MaxRankBot + 1;
		}
		int playerRankScore = data.PlayerRankScore;
		if (playerRankScore <= 0)
		{
			return data.Robots.Count + 1;
		}
		return CalcRankingByScore(playerRankScore, data);
	}

	public static int CalcRankingByScore(int score, RankModelData data)
	{
		if (data?.Robots == null || score <= 0)
		{
			return RankConst.MaxRankBot + 1;
		}
		int num = 0;
		for (int i = 0; i < data.Robots.Count; i++)
		{
			if (data.Robots[i].Score > score)
			{
				num++;
			}
		}
		return num + 1;
	}

	public static string FormatCountdown(long totalSeconds)
	{
		if (totalSeconds <= 0)
		{
			string text = GameApp.Lang.GetText("EventTimer_Finished", Array.Empty<object>());
			if (!string.IsNullOrEmpty(text))
			{
				return text;
			}
			return "00:00:00";
		}
		int num = (int)(totalSeconds / 3600);
		int num2 = (int)(totalSeconds % 3600 / 60);
		int num3 = (int)(totalSeconds % 60);
		return $"{num:D2}:{num2:D2}:{num3:D2}";
	}

	private void TryAutoUpdateRobots(long now)
	{
		if (data.Robots != null && data.Robots.Count != 0)
		{
			long num = data.EndTimeStamp - now;
			long num2 = now - data.TimeRefresh;
			long num3 = RankFunctionResolver.GetRankPeriodSeconds();
			long num4 = (long)(robotScoreCfg.STAG_TIME_CONTROL * (float)num3);
			if (num2 > robotScoreCfg.STAG_TIME_INTERVAL && num > num4)
			{
				int preRank = CalcPlayerRanking(data);
				int num5 = data.ExpGames - data.ClearLevels;
				RankRobotScoreCalculator.ApplyStagnation(data.Robots, robotScoreCfg, checkPrePoint: false, new RankRobotScoreCalculator.RankRobotRefreshLogContext("非通关摆烂", data.ExpGames, data.ActivityActiveGamesTotal, data.ActivityActiveDays, data.ClearLevels, num5, 0, (num5 <= 0) ? RankRobotScoreCalculator.CalcChasingTurn(num5, robotScoreCfg) : 0, data.PlayerRankScore, preRank, 0));
				data.TimeRefresh = now;
			}
		}
	}

	private bool TryAdvancePeriodConfig()
	{
		List<RankListActivity> list = tables.TbRankListActivity?.DataList;
		if (list == null || list.Count == 0)
		{
			return false;
		}
		int num = (data.LastPeriodIndex + 1) % list.Count;
		if (list[num] == null)
		{
			return false;
		}
		data.LastPeriodIndex = num;
		return true;
	}

	private void TryKickLowestRobot()
	{
		if (!data.HasPlayerEnteredRank && data.PlayerRankScore > 0 && data.Robots != null && data.Robots.Count != 0)
		{
			data.Robots.RemoveAt(data.Robots.Count - 1);
			data.HasPlayerEnteredRank = true;
		}
	}

	private static int GetOpenScoreForVictory(GameMode gameMode, int completedLevel)
	{
		ELevelDifficulty difficulty = LevelDifficultyUtil.GetDifficulty(gameMode, completedLevel);
		if (difficulty != ELevelDifficulty.Hard && difficulty != ELevelDifficulty.VeryHard)
		{
			return 1;
		}
		return 2;
	}

	private static bool HasReward(RankModelData data)
	{
		int num = CalcPlayerRanking(data);
		if (num > 0)
		{
			return num <= RankConst.CurrentRewardRankLimit;
		}
		return false;
	}
}
