using Game.Feature.Ad;
using Hot.Model;
using RotateRing.GamePlay;
using RotateRings.Prop;
using RotateRings.RewardedLuckyDraw;
using XingYun.Core.Singletons;
using XingYun.Proxy;

namespace RotateRings.Model;

public class LevelModelProxy : BaseProxy
{
	private GameModeData Gmd => Data.GameModeData;

	public LevelModelData Data { get; }

	public int MainLevel => Data.GetGameModeData(GameMode.Main).level;

	public int Level => Gmd.level;

	public int ModeSessionNum => Gmd.mode_num;

	public int TryLevelNum => Gmd.TryLevelNum;

	public int GlobalLevelCount => Gmd.GlobalLevelCount;

	public LevelModelProxy()
		: base("LevelModelProxy")
	{
		Data = GameApp.Save.GetOrCreate<LevelModelData>();
		Data.RefreshCurrentGameMode(LevelConfigService.MainLevelMax, LevelConfigService.CurrentEndlessSeason);
		SyncGameInfo();
	}

	public bool TryGetRandomPick(int level, out int coreLevelPath, out int parts, out int bonus, out bool specified, out string csv)
	{
		coreLevelPath = 0;
		parts = 0;
		bonus = 0;
		specified = false;
		csv = null;
		GameModeData gmd = Gmd;
		if (level <= 0 || gmd == null || gmd.RandomPickLevel != level || gmd.RandomPickCoreLevelPath <= 0)
		{
			return false;
		}
		coreLevelPath = gmd.RandomPickCoreLevelPath;
		parts = gmd.RandomPickParts;
		bonus = gmd.RandomPickBonus;
		specified = gmd.RandomPickSpecified != 0;
		csv = gmd.RandomPickCsv;
		return true;
	}

	public void SetRandomPick(int level, int coreLevelPath, int parts, int bonus, bool specified, string csv, int teachElement)
	{
		GameModeData gmd = Gmd;
		if (gmd == null)
		{
			return;
		}
		string text = csv ?? string.Empty;
		int num = (specified ? 1 : 0);
		if (gmd.RandomPickLevel != level || gmd.RandomPickCoreLevelPath != coreLevelPath || gmd.RandomPickParts != parts || gmd.RandomPickBonus != bonus || gmd.RandomPickSpecified != num || gmd.RandomPickTeachElement != teachElement || !string.Equals(gmd.RandomPickCsv, text))
		{
			gmd.RandomPickLevel = level;
			gmd.RandomPickCoreLevelPath = coreLevelPath;
			gmd.RandomPickParts = parts;
			gmd.RandomPickBonus = bonus;
			gmd.RandomPickSpecified = num;
			gmd.RandomPickCsv = text;
			gmd.RandomPickTeachElement = teachElement;
			if (GameApp.Save.Storage != null && GameApp.Save.Storage.IsOpened)
			{
				GameApp.Save.SaveAll();
			}
		}
	}

	public int GetRandomPickTeachElement(int level)
	{
		GameModeData gmd = Gmd;
		if (gmd == null || gmd.RandomPickLevel != level)
		{
			return 0;
		}
		return gmd.RandomPickTeachElement;
	}

	public void PrepareEnterGame()
	{
		SyncGameInfo();
	}

	public void SyncLevel(int levelSlotId)
	{
		Gmd.LevelSlotId = levelSlotId;
		SyncGameInfo();
	}

	public void SyncGameInfo()
	{
		GameInfo.LevelId = Level;
		GameInfo.CurrentGameMode = (GameMode)Gmd.game_mode;
	}

	public void AddLevel(int deltaLevel = 1)
	{
		Gmd.level += deltaLevel;
		Gmd.TryLevelNum = 0;
		Data.AddGameModelWinNum(GameInfo.CurrentGameMode);
		GameApp.Proxy.Get<PlayerModelProxy>()?.Data?.IncrementColdStartLevelWin();
		if (GameInfo.CurrentGameMode == GameMode.Main)
		{
			Data.RefreshCurrentGameMode(LevelConfigService.MainLevelMax, LevelConfigService.CurrentEndlessSeason);
		}
		SyncGameInfo();
	}

	public void AddGlobalLevelCount()
	{
		Gmd.GlobalLevelCount++;
	}

	public void TryLevelAgain()
	{
		Gmd.TryLevelNum++;
	}

	public void ResetLevel(int levelNum, bool isNewGame = true)
	{
		Gmd.level = levelNum;
		if (isNewGame)
		{
			Gmd.DeadNum = 0;
			Gmd.LevelState = 0;
			Gmd.AllStepNum = 0;
			Gmd.mode_num++;
			TryResetCurrentLevelLoseOnNewLevel(levelNum);
			ResetInLevelPropUseCounters();
		}
		SyncGameInfo();
	}

	public void ResetInLevelPropUseCounters()
	{
		Gmd.ClearData();
	}

	public void AddDeadNum(int num = 1)
	{
		Gmd.DeadNum += num;
	}

	public int GetDeadNum()
	{
		return Gmd.DeadNum;
	}

	public void CommitGameplayStep()
	{
		Gmd.AllStepNum++;
		Gmd.steps++;
	}

	public int GetAllStep()
	{
		return Gmd.AllStepNum;
	}

	public void SetGameBeginState()
	{
		Gmd.LevelState = 1;
	}

	public void SetGameEndState()
	{
		Gmd.LevelState = 2;
	}

	public void SetGameInitState()
	{
		Gmd.LevelState = 0;
	}

	public void SetGameingState()
	{
		Gmd.LevelState = 3;
	}

	public int GetGameIntState()
	{
		return Gmd.LevelState;
	}

	public bool IsGameEndState()
	{
		return Gmd.LevelState == 2;
	}

	public void AddScore(int score)
	{
		if (score > 0)
		{
			Gmd.game_score += score;
			((BaseProxy)this).SendEvent<ScoreAdded>(new ScoreAdded
			{
				Delta = score,
				Score = Gmd.game_score
			});
		}
	}

	public void ResetCurrentScore()
	{
		Gmd.game_score = 0;
		((BaseProxy)this).SendEvent<ScoreReset>(new ScoreReset
		{
			Score = 0
		});
	}

	public int GetCurrentScore()
	{
		return Gmd.game_score;
	}

	public void IncrementCumulativeReleaseCount()
	{
		Gmd.CurrentReleaseCount++;
	}

	public int GetCurrentReleaseCount()
	{
		return Gmd.CurrentReleaseCount;
	}

	public void ResetCurrentReleaseCount()
	{
		Gmd.CurrentReleaseCount = 0;
	}

	public void IncrementCumulativeReleaseTriggeredCount()
	{
		Gmd.CumulativeReleaseTriggeredCount++;
	}

	public int GetCumulativeReleaseTriggeredCount()
	{
		return Gmd.CumulativeReleaseTriggeredCount;
	}

	public void ResetCumulativeReleaseState()
	{
		Gmd.CurrentReleaseCount = 0;
		Gmd.CumulativeReleaseTriggeredCount = 0;
	}

	public void SyncCurrentScoreView()
	{
		((BaseProxy)this).SendEvent<ScoreReset>(new ScoreReset
		{
			Score = Gmd.game_score
		});
	}

	public int GetRewardCoinNum()
	{
		return Gmd.RewardCoinNum;
	}

	public void AddRewardCoinNum(int num = 1)
	{
		Gmd.RewardCoinNum += num;
	}

	public int GetUseCoinNum()
	{
		return Gmd.UseCoinNum;
	}

	public void AddUseCoinNum(int num = 1)
	{
		Gmd.UseCoinNum += num;
	}

	public void AddGameModelPropUseData(EPropType propType, TrackPropUseReason reasonId, int useCount)
	{
		if (reasonId != TrackPropUseReason.None && useCount > 0)
		{
			Data.AddGameModelPropUseData(GameInfo.CurrentGameMode, propType, reasonId, useCount);
			GameApp.Proxy.Get<RewardedLuckyDrawProxy>().RecordPropUse(propType, useCount);
			GameApp.Event.Send<InLevelPropUseRecordedEvent>(new InLevelPropUseRecordedEvent
			{
				PropType = propType,
				Reason = reasonId,
				UseCount = useCount
			});
		}
	}

	public void AddGameModelPropGainData(EPropType propType, TrackPropGainReason reasonId, int gainCount)
	{
		if (reasonId != TrackPropGainReason.None && gainCount > 0)
		{
			Data.AddGameModelPropGainData(GameInfo.CurrentGameMode, propType, reasonId, gainCount);
		}
	}

	public int GetPropUseCount(EPropType propType)
	{
		return Data.GetGameModelPropUseCount(GameInfo.CurrentGameMode, propType);
	}

	public int GetPropUseCount(EPropType propType, TrackPropUseReason reason)
	{
		return Data.GetGameModelPropUseCount(GameInfo.CurrentGameMode, propType, reason);
	}

	public int GetPropGainCount(EPropType propType)
	{
		return Data.GetGameModelPropGainCount(GameInfo.CurrentGameMode, propType);
	}

	public int GetPropGainCount(EPropType propType, TrackPropGainReason reason)
	{
		return Data.GetGameModelPropGainCount(GameInfo.CurrentGameMode, propType, reason);
	}

	public void AddWiningStreak()
	{
		Gmd.WinningStreak++;
	}

	public int GetWiningStreak()
	{
		return Gmd.WinningStreak;
	}

	public void ResetWinStreak()
	{
		Gmd.WinningStreak = 0;
	}

	public bool IsFirstEnterCurrentLevel()
	{
		return Gmd.IsFirstEnterLevel;
	}

	public void ResetCurrentLevelLoseOnRevive()
	{
		Gmd.CurrentLevelLose = 0;
		Gmd.LosingStreakCounted = false;
	}

	public void AddLosingStreak()
	{
		if (!Gmd.LosingStreakCounted)
		{
			Gmd.CurrentLevelLose++;
			Gmd.LosingStreakCounted = true;
			NotifyInterstitialLevelFailed();
		}
	}

	public void AddLosingStreakOnRestart(bool stepRatioReached)
	{
		if (stepRatioReached && !Gmd.LosingStreakCounted)
		{
			Gmd.CurrentLevelLose++;
			NotifyInterstitialLevelFailed();
		}
		Gmd.LosingStreakCounted = false;
	}

	public void OnLevelWin()
	{
		if (Gmd.IsFirstEnterLevel)
		{
			Gmd.LastLevelLoseFlag = false;
		}
		Gmd.CurrentLevelLose = 0;
		Gmd.LosingStreakCounted = false;
		Gmd.GameplayState = GameplayState.Win;
	}

	public void TryResetCurrentLevelLoseOnNewLevel(int levelNum)
	{
		if (Gmd.CurrentLevelLoseForLevel == levelNum)
		{
			Gmd.IsFirstEnterLevel = false;
			Gmd.TotalLevelLose += Gmd.CurrentLevelLose;
			if (Gmd.CurrentLevelLose > 0)
			{
				Gmd.LastLevelLoseFlag = true;
				Gmd.WinningStreak = 0;
			}
		}
		else
		{
			Gmd.IsFirstEnterLevel = true;
			Gmd.LastLevelLoseFlag = false;
			Gmd.TotalLevelLose = 0;
			Gmd.CurrentLevelLoseForLevel = levelNum;
		}
		Gmd.CurrentLevelLose = 0;
		Gmd.LosingStreakCounted = false;
	}

	private static void NotifyInterstitialLevelFailed()
	{
		Singleton<AdFeatureManager>.Instance?.GetRotateRingsInterstitialAdFeature()?.NotifyLevelFailed();
	}
}
