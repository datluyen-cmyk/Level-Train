using System;
using System.Collections.Generic;
using RotateRings.Model;
using RotateRings.SpecialLevel;
using XingYun.Track.Events;

namespace Game.DataTrack;

public static class GameTrackStatsHelper
{
	private const int RecentWindowSize = 10;

	private const string MainModeName = "主关卡";

	private const int MainGamePlayMethod = 0;

	public const int GamePlayMethod = 0;

	public static int GetGameMode(GameModeData modeData)
	{
		return modeData?.game_mode ?? 0;
	}

	public static string GetGameModeName(GameModeData modeData)
	{
		if (modeData == null)
		{
			return "主关卡";
		}
		GameMode game_mode = (GameMode)modeData.game_mode;
		if (game_mode == GameMode.Endless)
		{
			return "无尽模式";
		}
		if (!SpecialLevelModule.TryGetDisplayName(game_mode, out var displayName))
		{
			return "主关卡";
		}
		return displayName;
	}

	public static void FillModeStats(LevelModelData levelData, out int modeWinnum, out int modeWinnumAll, out double modeLast10WinRate, out double modeWinRate, out double winRate, out int modeWinStreak, out int modeLast10MaxScore, out int modeMaxScore, out int maxScore, out int modeWinStreakDay)
	{
		GameModeData gameModeData = levelData?.GameModeData;
		modeWinnum = gameModeData?.mode_winnum ?? 0;
		modeWinnumAll = levelData?.GetAllModeWinnum() ?? 0;
		modeWinStreak = gameModeData?.TrackModeWinStreak ?? 0;
		modeMaxScore = gameModeData?.TrackModeMaxScore ?? 0;
		maxScore = levelData?.TrackAllMaxScore ?? 0;
		modeWinStreakDay = GetModeWinStreakDay(gameModeData);
		int total = gameModeData?.TrackSettledGameNum ?? 0;
		int wins = gameModeData?.TrackSettledWinNum ?? 0;
		int total2 = levelData?.TrackAllSettledGameNum ?? 0;
		int wins2 = levelData?.TrackAllSettledWinNum ?? 0;
		modeWinRate = FloorWinRate(wins, total);
		winRate = FloorWinRate(wins2, total2);
		modeLast10WinRate = FloorWinRate(CountRecentWins(gameModeData?.TrackRecentSettledWins), CountRecentTotal(gameModeData?.TrackRecentSettledWins));
		modeLast10MaxScore = GetRecentMaxScore(gameModeData?.TrackRecentSettledScores);
	}

	public static void ApplyEndGameMaxScores(GGameEnd endData, int gameScore)
	{
		if (endData != null && gameScore > 0)
		{
			if (gameScore > endData.mode_max_score)
			{
				endData.mode_max_score = gameScore;
			}
			if (gameScore > endData.max_score)
			{
				endData.max_score = gameScore;
			}
			if (gameScore > endData.mode_last_10_max_score)
			{
				endData.mode_last_10_max_score = gameScore;
			}
		}
	}

	public static void IncrementModeWinStreakOnSuccess(LevelModelData levelData)
	{
		GameModeData gameModeData = levelData?.GameModeData;
		if (gameModeData != null)
		{
			gameModeData.TrackModeWinStreak++;
		}
	}

	public static void IncrementModeWinStreakDayOnSuccess(LevelModelData levelData)
	{
		GameModeData gameModeData = levelData?.GameModeData;
		if (gameModeData != null)
		{
			EnsureDayKey(gameModeData);
			gameModeData.TrackModeWinStreakDay++;
		}
	}

	public static void RecordSettledGame(LevelModelData levelData, bool isWin, int gameScore)
	{
		if (levelData != null)
		{
			GameModeData gameModeData = levelData.GameModeData;
			gameModeData.TrackSettledGameNum++;
			if (isWin)
			{
				gameModeData.TrackSettledWinNum++;
			}
			GameModeData gameModeData2 = gameModeData;
			PushRecentBool(gameModeData2.TrackRecentSettledWins ?? (gameModeData2.TrackRecentSettledWins = new List<bool>()), isWin);
			gameModeData2 = gameModeData;
			PushRecentScore(gameModeData2.TrackRecentSettledScores ?? (gameModeData2.TrackRecentSettledScores = new List<int>()), gameScore);
			if (gameScore > gameModeData.TrackModeMaxScore)
			{
				gameModeData.TrackModeMaxScore = gameScore;
			}
			levelData.TrackAllSettledGameNum++;
			if (isWin)
			{
				levelData.TrackAllSettledWinNum++;
			}
			if (gameScore > levelData.TrackAllMaxScore)
			{
				levelData.TrackAllMaxScore = gameScore;
			}
		}
	}

	public static void ResetModeWinStreakOnRestart(LevelModelData levelData)
	{
		GameModeData gameModeData = levelData?.GameModeData;
		if (gameModeData != null)
		{
			gameModeData.TrackModeWinStreak = 0;
		}
	}

	public static void ResetModeWinStreakDayOnRestart(LevelModelData levelData)
	{
		GameModeData gameModeData = levelData?.GameModeData;
		if (gameModeData != null)
		{
			EnsureDayKey(gameModeData);
			gameModeData.TrackModeWinStreakDay = 0;
		}
	}

	private static int GetModeWinStreakDay(GameModeData mode)
	{
		if (mode == null)
		{
			return 0;
		}
		EnsureDayKey(mode);
		return mode.TrackModeWinStreakDay;
	}

	private static void EnsureDayKey(GameModeData mode)
	{
		string text = DateTime.Now.ToString("yyyyMMdd");
		if (!(mode.TrackWinStreakDayKey == text))
		{
			if (!string.IsNullOrEmpty(mode.TrackWinStreakDayKey))
			{
				mode.TrackModeWinStreakDay = 0;
			}
			mode.TrackWinStreakDayKey = text;
		}
	}

	private static void PushRecentBool(List<bool> list, bool value)
	{
		list.Add(value);
		TrimRecent(list);
	}

	private static void PushRecentScore(List<int> list, int value)
	{
		list.Add(value);
		TrimRecent(list);
	}

	private static void TrimRecent<T>(List<T> list)
	{
		while (list.Count > 10)
		{
			list.RemoveAt(0);
		}
	}

	private static int CountRecentWins(List<bool> list)
	{
		if (list == null || list.Count == 0)
		{
			return 0;
		}
		int num = 0;
		for (int i = 0; i < list.Count; i++)
		{
			if (list[i])
			{
				num++;
			}
		}
		return num;
	}

	private static int CountRecentTotal(List<bool> list)
	{
		return list?.Count ?? 0;
	}

	private static int GetRecentMaxScore(List<int> list)
	{
		if (list == null || list.Count == 0)
		{
			return 0;
		}
		int num = 0;
		for (int i = 0; i < list.Count; i++)
		{
			if (list[i] > num)
			{
				num = list[i];
			}
		}
		return num;
	}

	private static double FloorWinRate(int wins, int total)
	{
		if (total <= 0)
		{
			return 0.0;
		}
		return Math.Floor((double)wins / (double)total * 1000.0) / 1000.0;
	}
}
