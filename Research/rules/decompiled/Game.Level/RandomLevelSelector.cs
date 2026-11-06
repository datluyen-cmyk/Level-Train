using System.Collections.Generic;
using RotateRings.Common;
using RotateRings.UI.NewElementIntro;
using UnityEngine;
using XingYun.Level;

namespace Game.Level;

public class RandomLevelSelector : ILevelSelectorStrategy
{
	private const string TAG = "[RL][Selector] ";

	private const string TAG_TEST = "[RL][随机关卡测试] ";

	public const int DefaultMaxRecentLevels = 100;

	public const int ELEMENT_BOMB = 1;

	public const int ELEMENT_CHAIN = 2;

	public const int ELEMENT_KEY_LOCK = 3;

	public const string DATA_KEY_FULL_RANDOM = "RL_FullRandomPick";

	public static int MaxRecentLevels { get; private set; } = 100;

	public static void SetMaxRecentLevels(int value)
	{
		MaxRecentLevels = ((value >= 0) ? value : 0);
		TrimRecentLevels();
	}

	public static void ResetMaxRecentLevels()
	{
		SetMaxRecentLevels(100);
	}

	private static void TrimRecentLevels()
	{
		GameLevelProxy obj = GameApp.Proxy.Get<GameLevelProxy>();
		GameLevelProgress val = ((obj != null) ? obj.GetGameLevelProgress() : null);
		if (val?.RecentLevels != null)
		{
			while (val.RecentLevels.Count > MaxRecentLevels)
			{
				val.RecentLevels.RemoveAt(0);
			}
		}
	}

	public string Match(LevelContext ctx)
	{
		GameLevelProgress gameLevelProgress = GameApp.Proxy.Get<GameLevelProxy>().GetGameLevelProgress();
		string text = ctx.Data["matched_csv"] as string;
		if (string.IsNullOrEmpty(text))
		{
			return "";
		}
		List<LevelEntry> csvEntries = LevelSystem.GetRepository().GetCsvEntries(text);
		if (csvEntries == null || csvEntries.Count == 0)
		{
			return "";
		}
		bool num = IsFullRandomPick(ctx);
		int mustNotHave = ResolvePartsMustNotHave(ctx);
		List<LevelEntry> list;
		int excludedByParts;
		int excludedByMech;
		if (num)
		{
			list = new List<LevelEntry>(csvEntries);
			excludedByParts = 0;
			excludedByMech = 0;
		}
		else
		{
			list = Filter(csvEntries, mustNotHave, out excludedByParts, out excludedByMech);
			if (list.Count == 0)
			{
				list = FilterByMechanismOnly(csvEntries, out excludedByMech);
				if (list.Count == 0)
				{
					list = csvEntries;
				}
			}
		}
		List<LevelEntry> list2 = new List<LevelEntry>(list.Count);
		for (int i = 0; i < list.Count; i++)
		{
			if (!gameLevelProgress.RecentLevels.Contains(list[i].Index))
			{
				list2.Add(list[i]);
			}
		}
		LevelEntry val = ((list2.Count > 0) ? list2[Random.Range(0, list2.Count)] : GetOldestLevelAndRotate(list, gameLevelProgress));
		AddRecentLevel(gameLevelProgress, val.Index);
		ctx.Data["matched_csv"] = text;
		ctx.Data["matched_index"] = val.Index;
		ctx.Data["matched_entry_diff"] = val.Difficulty;
		ctx.MatchedLevelDifficulty = val.Difficulty;
		gameLevelProgress.CurrentLevelDifficulty = val.Difficulty;
		gameLevelProgress.LastLevelID = val.Index;
		int parts = LevelContentCodec.PartsOf(val.Content);
		LogDecision(ctx, text, val, parts, mustNotHave, list.Count, list2.Count, excludedByParts, excludedByMech);
		return val.Content;
	}

	private static void LogDecision(LevelContext ctx, string csvId, LevelEntry picked, int parts, int mustNotHave, int candidateCount, int availableCount, int excludedByParts, int excludedByMech)
	{
		FeatureConfig.LevelRandom.ResolveWindowSize();
	}

	private static string ReadFixed(LevelContext ctx, string key)
	{
		string key2 = key + "_Fixed";
		if (ctx != null && ctx.Data.TryGetValue(key2, out var value) && value != null)
		{
			return value.ToString();
		}
		return "-";
	}

	private static string ReadOffset(LevelContext ctx)
	{
		string key = "DAWinLose_Offset";
		if (ctx != null && ctx.Data.TryGetValue(key, out var value) && value != null)
		{
			return value.ToString();
		}
		return "-";
	}

	private static string DescribeWindow()
	{
		RandomLevelFirstClearWindowData data = RandomLevelFirstClearWindow.Data;
		if (data == null || data.Window == null || data.Window.Count <= 0)
		{
			return "空";
		}
		return string.Join(string.Empty, data.Window);
	}

	private static string DescribeGameWindow()
	{
		RandomLevelFirstClearWindowData data = RandomLevelFirstClearWindow.Data;
		if (data == null || data.GameWindow == null || data.GameWindow.Count <= 0)
		{
			return "空";
		}
		return string.Join(string.Empty, data.GameWindow);
	}

	private static int LevelWindowCount()
	{
		return (RandomLevelFirstClearWindow.Data?.Window?.Count).GetValueOrDefault();
	}

	private static int GameWindowCount()
	{
		return (RandomLevelFirstClearWindow.Data?.GameWindow?.Count).GetValueOrDefault();
	}

	private static int ResolvePartsMustNotHave(LevelContext ctx)
	{
		if (ctx != null && ctx.Data.TryGetValue("Parts_MustNotHave_Fixed", out var value) && value != null && int.TryParse(value.ToString(), out var result))
		{
			return result;
		}
		return 0;
	}

	private static List<LevelEntry> Filter(List<LevelEntry> entries, int mustNotHave, out int excludedByParts, out int excludedByMech)
	{
		List<LevelEntry> list = new List<LevelEntry>(entries.Count);
		excludedByParts = 0;
		excludedByMech = 0;
		for (int i = 0; i < entries.Count; i++)
		{
			LevelEntry val = entries[i];
			int num = LevelContentCodec.PartsOf(val.Content);
			if (mustNotHave != 0 && (num & mustNotHave) != 0)
			{
				excludedByParts++;
			}
			else if (HasLockedMechanism(num))
			{
				excludedByMech++;
			}
			else
			{
				list.Add(val);
			}
		}
		return list;
	}

	private static List<LevelEntry> FilterByMechanismOnly(List<LevelEntry> entries, out int excludedByMech)
	{
		List<LevelEntry> list = new List<LevelEntry>(entries.Count);
		excludedByMech = 0;
		for (int i = 0; i < entries.Count; i++)
		{
			if (!HasLockedMechanism(LevelContentCodec.PartsOf(entries[i].Content)))
			{
				list.Add(entries[i]);
			}
			else
			{
				excludedByMech++;
			}
		}
		return list;
	}

	private static bool HasLockedMechanism(int parts)
	{
		if ((parts & 1) != 0 && !HasIntroduced(1))
		{
			return true;
		}
		if ((parts & 4) != 0 && !HasIntroduced(2))
		{
			return true;
		}
		if ((parts & 8) != 0)
		{
			return !HasIntroduced(3);
		}
		return false;
	}

	private static bool HasIntroduced(int elementId)
	{
		return GameApp.Proxy.Get<NewElementIntroProxy>()?.HasIntroduced(elementId) ?? false;
	}

	private static bool IsFullRandomPick(LevelContext ctx)
	{
		if (ctx != null && ctx.Data.TryGetValue("RL_FullRandomPick", out var value) && value != null)
		{
			if (value is bool)
			{
				return (bool)value;
			}
			return value.ToString() == "True";
		}
		return false;
	}

	public static bool HasAvailableEntry(LevelContext ctx, string csvId)
	{
		if (string.IsNullOrEmpty(csvId))
		{
			return false;
		}
		LevelRepository repository = LevelSystem.GetRepository();
		List<LevelEntry> list = ((repository != null) ? repository.GetCsvEntries(csvId) : null);
		if (list == null || list.Count == 0)
		{
			return false;
		}
		int num = ResolvePartsMustNotHave(ctx);
		GameLevelProxy obj = GameApp.Proxy.Get<GameLevelProxy>();
		GameLevelProgress val = ((obj != null) ? obj.GetGameLevelProgress() : null);
		for (int i = 0; i < list.Count; i++)
		{
			LevelEntry val2 = list[i];
			if (val2 != null)
			{
				int num2 = LevelContentCodec.PartsOf(val2.Content);
				if ((num == 0 || (num2 & num) == 0) && !HasLockedMechanism(num2) && (val == null || !val.RecentLevels.Contains(val2.Index)))
				{
					return true;
				}
			}
		}
		return false;
	}

	private static void AddRecentLevel(GameLevelProgress gameProgress, int levelIndex)
	{
		gameProgress.RecentLevels.Add(levelIndex);
		while (gameProgress.RecentLevels.Count > MaxRecentLevels)
		{
			gameProgress.RecentLevels.RemoveAt(0);
		}
	}

	private static LevelEntry GetOldestLevelAndRotate(List<LevelEntry> entries, GameLevelProgress gameProgress)
	{
		for (int i = 0; i < gameProgress.RecentLevels.Count; i++)
		{
			int num = gameProgress.RecentLevels[i];
			for (int j = 0; j < entries.Count; j++)
			{
				if (entries[j].Index == num)
				{
					gameProgress.RecentLevels.RemoveAt(i);
					return entries[j];
				}
			}
		}
		return entries[Random.Range(0, entries.Count)];
	}
}
