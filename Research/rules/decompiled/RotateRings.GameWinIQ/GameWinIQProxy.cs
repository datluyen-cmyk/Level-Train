using System;
using Hot.Model;
using RotateRings.Common;
using RotateRings.Model;
using RotateRings.Prop;
using RotateRings.Services;
using UnityEngine;
using XingYun.Proxy;

namespace RotateRings.GameWinIQ;

public class GameWinIQProxy : BaseProxy
{
	private const string Tag = "[GameWinIQProxy] ";

	private const string GameWinIQAudioVariantId = "variant_25e3e6ce";

	private GameWinIQSettlementSnapshot lastSnapshot = GameWinIQSettlementSnapshot.Empty;

	private readonly GameWinIQSessionTracker sessionTracker = new GameWinIQSessionTracker();

	public GameWinIQData Data { get; }

	public GameWinIQSessionTracker SessionTracker => sessionTracker;

	public GameWinIQProxy()
		: base("GameWinIQProxy")
	{
		Data = GameApp.Save.GetOrCreate<GameWinIQData>();
	}

	public GameWinIQSettlementSnapshot GetLastSettlementSnapshot()
	{
		return lastSnapshot;
	}

	public void ApplyCrossDayRulesIfNeeded()
	{
		if (!IsExperimentEnabled() || GameWinIQGmScoreLock.Enabled || !GameWinIQConfigProvider.TryGetConfig(out var config))
		{
			return;
		}
		int localDateYyyyMmDd = GetLocalDateYyyyMmDd();
		if (Data.LastActiveDate == 0)
		{
			Data.LastActiveDate = localDateYyyyMmDd;
		}
		else
		{
			if (Data.LastActiveDate == localDateYyyyMmDd)
			{
				return;
			}
			_ = Data.LastActiveDate;
			_ = config.MaxBaseScore;
			int baseScore = Data.BaseScore;
			int lastDisplayScore = Data.LastDisplayScore;
			if (baseScore >= config.MaxBaseScore)
			{
				Data.ReachedPeakInCareer = true;
			}
			bool reachedPeakInCareer = Data.ReachedPeakInCareer;
			int num = baseScore;
			int num2 = lastDisplayScore;
			bool flag = false;
			if (reachedPeakInCareer)
			{
				if (baseScore <= config.CrossDayDecayThreshold)
				{
					_ = $"基础分<={config.CrossDayDecayThreshold}不衰减";
				}
				else
				{
					num = GameWinIQCalculator.ApplyCrossDayDecayToBase(config, baseScore);
					num2 = GameWinIQCalculator.ApplyCrossDayDecayToDisplay(config, lastDisplayScore);
					Data.BaseScore = num;
					Data.LastDisplayScore = num2;
					flag = num != baseScore;
				}
			}
			Data.LastActiveDate = localDateYyyyMmDd;
			if (flag)
			{
				lastSnapshot = new GameWinIQSettlementSnapshot(shouldShowUi: false, lastDisplayScore, num2, baseScore, num, 0, num2 >= config.PeakGeniusDisplayThreshold, playGrowthAnimation: false, appliedCrossDayDecay: true, lastDisplayScore, num2, default(GameWinIQSettlementDetail));
			}
		}
	}

	public void RecordLevelRestart(int currentLevel)
	{
		if (IsExperimentEnabled() && (Data.FeatureUnlocked || MeetsUnlockThresholdWhilePlaying()))
		{
			sessionTracker.ResetPerformanceStats();
			int baseScoreAtLevelEnter = sessionTracker.BaseScoreAtLevelEnter;
			_ = GameApp.Proxy.Get<LevelModelProxy>()?.TryLevelNum;
			if (GameWinIQConfigProvider.TryGetConfig(out var config))
			{
				GameWinIQCalculator.GetRangeIndex(config, baseScoreAtLevelEnter);
				config.GetRangeBand(baseScoreAtLevelEnter);
			}
		}
	}

	public void OnEnterLevelCampaign(int level)
	{
		sessionTracker.Reset();
		sessionTracker.SetBaseScoreAtLevelEnter(Data.BaseScore);
		sessionTracker.BeginSession();
		if (GameWinIQConfigProvider.TryGetConfig(out var config))
		{
			int baseScoreAtLevelEnter = sessionTracker.BaseScoreAtLevelEnter;
			config.GetRangeBand(baseScoreAtLevelEnter);
		}
	}

	public void OnReplayLevelLoaded()
	{
		sessionTracker.BeginSession();
	}

	public GameWinIQSettlementSnapshot RecordOnLevelWin(int completedLevel, int propUseCount, int restartCount)
	{
		lastSnapshot = GameWinIQSettlementSnapshot.Empty;
		ApplyCrossDayRulesIfNeeded();
		GameWinIQConfigProvider.GetExperimentValue();
		if (!IsExperimentEnabled())
		{
			sessionTracker.Reset();
			return lastSnapshot;
		}
		if (!GameWinIQConfigProvider.TryGetConfig(out var config))
		{
			sessionTracker.Reset();
			return lastSnapshot;
		}
		if (!Data.FeatureUnlocked && !MeetsUnlockThresholdForWin(completedLevel))
		{
			sessionTracker.Reset();
			return lastSnapshot;
		}
		if (!Data.FeatureUnlocked)
		{
			Data.FeatureUnlocked = true;
		}
		if (GameWinIQGmScoreLock.Enabled)
		{
			int lastDisplayScore = Data.LastDisplayScore;
			int baseScore = Data.BaseScore;
			lastSnapshot = new GameWinIQSettlementSnapshot(shouldShowUi: true, lastDisplayScore, lastDisplayScore, baseScore, baseScore, 0, lastDisplayScore >= config.PeakGeniusDisplayThreshold, playGrowthAnimation: false, appliedCrossDayDecay: false, 0, 0, default(GameWinIQSettlementDetail));
			sessionTracker.Reset();
			return lastSnapshot;
		}
		int lastDisplayScore2 = Data.LastDisplayScore;
		int baseScore2 = Data.BaseScore;
		ELevelDifficulty difficulty = LevelDifficultyUtil.GetDifficulty(GameInfo.CurrentGameMode, completedLevel);
		int baseScoreAtLevelEnter = ((sessionTracker.BaseScoreAtLevelEnter > 0) ? sessionTracker.BaseScoreAtLevelEnter : baseScore2);
		GameWinIQCalculator.EvaluateLevel(config, baseScore2, difficulty, propUseCount, baseScoreAtLevelEnter, restartCount, sessionTracker.UnlockReleaseCounts, sessionTracker.InvalidAndWrongStepCount, completedLevel, out var newBaseScore, out var sessionPerformanceScore, out var detail);
		int num = GameWinIQCalculator.ComposeDisplayScore(config, detail.MappedDisplayFromBase, sessionPerformanceScore);
		bool isPeakGenius = num >= config.PeakGeniusDisplayThreshold;
		if (newBaseScore >= config.MaxBaseScore)
		{
			Data.ReachedPeakInCareer = true;
		}
		Data.BaseScore = newBaseScore;
		bool playGrowthAnimation = num > lastDisplayScore2;
		Data.LastDisplayScore = num;
		lastSnapshot = new GameWinIQSettlementSnapshot(shouldShowUi: true, lastDisplayScore2, num, baseScore2, newBaseScore, sessionPerformanceScore, isPeakGenius, playGrowthAnimation, appliedCrossDayDecay: false, 0, 0, detail);
		LogSettlement(config, lastSnapshot, propUseCount, restartCount, baseScoreAtLevelEnter);
		sessionTracker.Reset();
		return lastSnapshot;
	}

	public static bool IsExperimentEnabled()
	{
		return GameWinIQConfigProvider.IsExperimentEnabled();
	}

	public string GetTrackBaseAndDisplayScore()
	{
		if (!IsExperimentEnabled())
		{
			return string.Empty;
		}
		return $"{Data.BaseScore},{Data.LastDisplayScore}";
	}

	public void SyncGameWinIQAudioExperiment()
	{
		AudioForgeRuntime instance = AudioForgeRuntime.Instance;
		if (!((Object)(object)instance == (Object)null) && instance.IsReady)
		{
			if (IsExperimentEnabled())
			{
				AudioService.ActivateExperiment("variant_25e3e6ce");
			}
			else
			{
				AudioService.DeactivateExperiment("variant_25e3e6ce");
			}
		}
	}

	private static bool MeetsUnlockThresholdForWin(int completedLevel)
	{
		return completedLevel >= 6;
	}

	private static bool MeetsUnlockThresholdWhilePlaying()
	{
		return GetCompletedMainLevel() >= 6;
	}

	private static int GetCompletedMainLevel()
	{
		int num = GameApp.Proxy.Get<LevelModelProxy>()?.MainLevel ?? 1;
		return Math.Max(0, num - 1);
	}

	public static int CountCurrentLevelPropUses()
	{
		LevelModelProxy levelModelProxy = GameApp.Proxy.Get<LevelModelProxy>();
		if (levelModelProxy == null)
		{
			return 0;
		}
		return 0 + levelModelProxy.GetPropUseCount(EPropType.Hammer) + levelModelProxy.GetPropUseCount(EPropType.Skyrocket);
	}

	private static void LogSettlement(GameWinIQRuntimeConfig config, GameWinIQSettlementSnapshot snapshot, int propUseCount, int restartCount, int baseScoreAtLevelEnter)
	{
		_ = snapshot.Detail;
		config.GetRangeBand(baseScoreAtLevelEnter);
		GameWinIQCalculator.GetRangeIndex(config, baseScoreAtLevelEnter);
	}

	private static int GetLocalDateYyyyMmDd()
	{
		DateTime now = DateTime.Now;
		return now.Year * 10000 + now.Month * 100 + now.Day;
	}
}
