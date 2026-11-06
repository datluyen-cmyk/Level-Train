using System;
using System.Collections.Generic;
using System.Linq;
using XingYun.Level;

namespace Game.Level;

public class CustomLevelSelector : ILevelSelectorStrategy
{
	private const string TAG = "[Game][CustomLevelSelector] ";

	private const int MAX_RECENT_LEVELS = 100;

	public string Match(LevelContext ctx)
	{
		GameLevelProgress gameProgress = GameApp.Proxy.Get<GameLevelProxy>().GetGameLevelProgress();
		string text = ctx.Data["matched_csv"] as string;
		if (string.IsNullOrEmpty(text))
		{
			return "";
		}
		string text2 = "LevelStyleTileDefeat";
		LevelSystem.SwitchRepository("LevelConfig/" + text2 + "/");
		List<LevelEntry> csvEntries = LevelSystem.GetRepository().GetCsvEntries(text);
		if (csvEntries == null || csvEntries.Count == 0)
		{
			ctx.SampledDifficulty = LevelSystem.GetConfig().SamplerStrategy.Sample(ctx);
			text = LevelSystem.GetConfig().MatchAdapter.Match(ctx);
			ctx.Data["matched_csv"] = text;
			ctx.Data["sampled_difficulty"] = ctx.SampledDifficulty;
			ctx.Data["min_difficulty"] = LevelSystem.GetConfig().SamplerConfig.MinDifficulty;
			csvEntries = LevelSystem.GetRepository().GetCsvEntries(text);
			if (csvEntries == null || csvEntries.Count == 0)
			{
				return "";
			}
		}
		List<LevelEntry> list = csvEntries.Where((LevelEntry e) => !IsRecentLevel(gameProgress, e.Index)).ToList();
		LevelEntry val = ((list.Count <= 0) ? GetOldestLevelAndRotate(csvEntries, gameProgress) : list.OrderBy((LevelEntry e) => Math.Abs(e.Difficulty - ctx.SampledDifficulty)).First());
		AddRecentLevel(gameProgress, val.Index);
		ctx.Data["matched_csv"] = text;
		ctx.Data["matched_index"] = val.Index;
		ctx.Data["matched_entry_diff"] = val.Difficulty;
		ctx.MatchedLevelDifficulty = val.Difficulty;
		gameProgress.CurrentLevelDifficulty = val.Difficulty;
		gameProgress.LastLevelID = val.Index;
		return val.Content;
	}

	private bool IsRecentLevel(GameLevelProgress gameProgress, int levelIndex)
	{
		return gameProgress.RecentLevels.Contains(levelIndex);
	}

	private void AddRecentLevel(GameLevelProgress gameProgress, int levelIndex)
	{
		gameProgress.RecentLevels.Add(levelIndex);
		if (gameProgress.RecentLevels.Count > 100)
		{
			gameProgress.RecentLevels.RemoveAt(0);
		}
	}

	private LevelEntry GetOldestLevelAndRotate(List<LevelEntry> entries, GameLevelProgress gameProgress)
	{
		int oldestLevelIndex = gameProgress.RecentLevels.FirstOrDefault();
		LevelEntry? result = ((IEnumerable<LevelEntry>)entries).FirstOrDefault((Func<LevelEntry, bool>)((LevelEntry e) => e.Index == oldestLevelIndex)) ?? entries.First();
		if (gameProgress.RecentLevels.Count > 0)
		{
			gameProgress.RecentLevels.RemoveAt(0);
		}
		return result;
	}
}
