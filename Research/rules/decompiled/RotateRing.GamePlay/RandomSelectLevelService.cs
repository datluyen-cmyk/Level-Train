using System;
using System.Collections.Generic;
using Game.Level;
using RotateRings.Common;
using RotateRings.Model;
using XingYun.Level;

namespace RotateRing.GamePlay;

public sealed class RandomSelectLevelService : ILevelSelectService
{
	private readonly struct Pick
	{
		public readonly int CoreLevelPath;

		public readonly int Parts;

		public readonly string Csv;

		public readonly int Bonus;

		public readonly int TeachElement;

		public Pick(int coreLevelPath, int parts, string csv, int bonus, int teachElement)
		{
			CoreLevelPath = coreLevelPath;
			Parts = parts;
			Csv = csv;
			Bonus = bonus;
			TeachElement = teachElement;
		}
	}

	private const string TAG = "[RL][Bridge] ";

	public const int StartLevel = 1;

	private const int CACHE_CAPACITY = 64;

	private static readonly Dictionary<(GameMode mode, int level, int session), int> resolvedCache = new Dictionary<(GameMode, int, int), int>();

	private static readonly Dictionary<int, Pick> pickedByLevel = new Dictionary<int, Pick>();

	private static int lastSkipLevel = int.MinValue;

	private static int CurrentSession()
	{
		return GameApp.Proxy.Get<LevelModelProxy>()?.ModeSessionNum ?? 0;
	}

	public static bool TryGetMatchedContent(int level, out int coreLevelPath, out int parts)
	{
		coreLevelPath = -1;
		parts = 0;
		if (!FeatureConfig.LevelRandom.ResolveEnable())
		{
			return false;
		}
		if (!pickedByLevel.TryGetValue(level, out var value) || value.CoreLevelPath <= 0)
		{
			return false;
		}
		coreLevelPath = value.CoreLevelPath;
		parts = value.Parts;
		return true;
	}

	public static bool TryGetMatchedTeachElement(int level, out int elementId)
	{
		elementId = 0;
		if (!FeatureConfig.LevelRandom.ResolveEnable())
		{
			return false;
		}
		if (!pickedByLevel.TryGetValue(level, out var value) || value.TeachElement <= 0)
		{
			return false;
		}
		elementId = value.TeachElement;
		return true;
	}

	public static string GetMatchedCsvFor(int level)
	{
		if (!FeatureConfig.LevelRandom.ResolveEnable())
		{
			return null;
		}
		if (!pickedByLevel.TryGetValue(level, out var value))
		{
			return null;
		}
		return value.Csv;
	}

	public static bool TryGetMatchedBonus(int level, out int bonus)
	{
		bonus = 0;
		if (!FeatureConfig.LevelRandom.ResolveEnable())
		{
			return false;
		}
		if (!pickedByLevel.TryGetValue(level, out var value))
		{
			return false;
		}
		bonus = value.Bonus;
		return true;
	}

	public bool IsAvailable(GameMode mode, int level)
	{
		if (mode != GameMode.Main || level < 1)
		{
			return false;
		}
		int mainLevelMax = LevelConfigService.MainLevelMax;
		if (mainLevelMax > 0 && level > mainLevelMax)
		{
			LogSkip(level, $"超出主线关卡上限 MainLevelMax={mainLevelMax}（框架不接管，回落底板）");
			return false;
		}
		if (!FeatureConfig.LevelRandom.ResolveEnable())
		{
			LogSkip(level, "总开关 Enable=false（实验未开启）");
			return false;
		}
		bool num = LevelSystemExtensions.HasStrategy();
		if (!num)
		{
			LogSkip(level, "框架未就绪 HasStrategy=false（未接线 / 接线失败 / 已 Detach）");
		}
		return num;
	}

	private static void LogSkip(int level, string reason)
	{
		if (level != lastSkipLevel)
		{
			lastSkipLevel = level;
		}
	}

	public int SelectLevelId(GameMode mode, int level, int boardLevelId, bool isCore)
	{
		int num = CurrentSession();
		if (resolvedCache.TryGetValue((mode, level, num), out var value))
		{
			lastSkipLevel = int.MinValue;
			return value;
		}
		if (TryResolveByFramework(mode, level, boardLevelId, num, out var levelId))
		{
			lastSkipLevel = int.MinValue;
			Cache(mode, level, num, levelId);
			return levelId;
		}
		Cache(mode, level, num, boardLevelId);
		return boardLevelId;
	}

	private static bool TryResolveByFramework(GameMode mode, int level, int boardLevelId, int session, out int levelId)
	{
		levelId = boardLevelId;
		try
		{
			LevelModelProxy levelModelProxy = GameApp.Proxy.Get<LevelModelProxy>();
			if (levelModelProxy != null && levelModelProxy.TryGetRandomPick(level, out var coreLevelPath, out var parts, out var bonus, out var specified, out var csv))
			{
				int num = levelModelProxy.GetRandomPickTeachElement(level);
				if (num <= 0)
				{
					num = RandomLevelTutorial.ResolveSpecialPositionElementId(level);
					if (num > 0)
					{
						levelModelProxy.SetRandomPick(level, coreLevelPath, parts, bonus, specified, csv, num);
					}
				}
				SetPick(level, coreLevelPath, parts, csv, bonus, num);
				RandomLevelFirstClearWindow.OnLevelEnter(level, specified);
				levelId = coreLevelPath;
				return true;
			}
			LevelInfo levelInfo = GameApp.Level.GetLevelInfo(level, (Dictionary<string, object>)null);
			if (levelInfo == null || string.IsNullOrEmpty(levelInfo.LevelId))
			{
				Warn($"框架未返回题目，回落底板: mode={mode}, level={level}");
				return false;
			}
			bool flag = IsSpecifiedLevel(levelInfo);
			if (!int.TryParse(LevelContentCodec.CoreLevelPathOf(levelInfo.LevelId), out var result) || result <= 0)
			{
				Warn("题目 Content 不是合法 CoreLevelPath，回落底板: content=\"" + levelInfo.LevelId + "\"");
				return false;
			}
			int parts2 = LevelContentCodec.PartsOf(levelInfo.LevelId);
			int bonus2 = ResolveDifficultyOffset(levelInfo);
			int teachElement = RandomLevelTutorial.ResolveElementId(level, parts2);
			string csv2 = ((string.IsNullOrEmpty(levelInfo.MatchedCsv) || levelInfo.MatchedCsv == "error" || levelInfo.MatchedCsv == "forced") ? null : levelInfo.MatchedCsv);
			SetPick(level, result, parts2, csv2, bonus2, teachElement);
			levelModelProxy?.SetRandomPick(level, result, parts2, bonus2, flag, csv2, teachElement);
			RandomLevelFirstClearWindow.OnLevelEnter(level, flag);
			levelId = result;
			return true;
		}
		catch (Exception ex)
		{
			Warn($"框架投放异常，回落底板: mode={mode}, level={level}, err={ex.Message}");
			levelId = boardLevelId;
			return false;
		}
	}

	private static void SetPick(int level, int coreLevelPath, int parts, string csv, int bonus, int teachElement)
	{
		if (pickedByLevel.Count >= 64)
		{
			pickedByLevel.Clear();
		}
		pickedByLevel[level] = new Pick(coreLevelPath, parts, csv, bonus, teachElement);
	}

	private static int ResolveDifficultyOffset(LevelInfo info)
	{
		if (info?.DebugData == null)
		{
			return 0;
		}
		string key = "DAWinLose_Offset";
		if (info.DebugData.TryGetValue(key, out var value) && value != null && int.TryParse(value.ToString(), out var result))
		{
			return result;
		}
		return 0;
	}

	private static bool IsSpecifiedLevel(LevelInfo info)
	{
		if (info.DebugData == null)
		{
			return false;
		}
		return info.DebugData.ContainsKey("LevelId_Fixed");
	}

	private static void Cache(GameMode mode, int level, int session, int levelId)
	{
		if (resolvedCache.Count >= 64)
		{
			resolvedCache.Clear();
		}
		resolvedCache[(mode, level, session)] = levelId;
	}

	public static void ResetCache()
	{
		resolvedCache.Clear();
		pickedByLevel.Clear();
		lastSkipLevel = int.MinValue;
	}

	private static void Warn(string message)
	{
	}
}
