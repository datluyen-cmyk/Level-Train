using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using XingYun.Level;

namespace Game.Level;

public class RandomLevelMatchAdapter : DefaultLevelMatchAdapter, ILevelMatchAdapter
{
	private const string TAG = "[RL][MatchAdapter] ";

	public const int TierMin = 1;

	public const int TierMax = 8;

	public string Match(LevelContext ctx)
	{
		int num = ResolveTier(ctx);
		SetFullRandomFlag(ctx, value: false);
		if (HasAvailableForTier(ctx, num))
		{
			string text = FilterAndPick(ctx);
			if (!string.IsNullOrEmpty(text))
			{
				return text;
			}
		}
		for (int i = 1; i <= 7; i++)
		{
			string text2 = TryTier(ctx, num, num - i);
			if (!string.IsNullOrEmpty(text2))
			{
				return text2;
			}
			text2 = TryTier(ctx, num, num + i);
			if (!string.IsNullOrEmpty(text2))
			{
				return text2;
			}
		}
		SetTier(ctx, num);
		SetFullRandomFlag(ctx, value: true);
		string text3 = PickFileForTier(ctx, num);
		if (string.IsNullOrEmpty(text3))
		{
			text3 = PickRandomWholeRepository(ctx);
		}
		else
		{
			((DefaultLevelMatchAdapter)this).UpdateCurrentIntervalId(ctx, text3);
		}
		return text3;
	}

	private string TryTier(LevelContext ctx, int originTier, int t)
	{
		if (t < 1 || t > 8 || !HasAvailableForTier(ctx, t))
		{
			return null;
		}
		SetTier(ctx, t);
		string text = FilterAndPick(ctx);
		if (!string.IsNullOrEmpty(text))
		{
			return text;
		}
		return null;
	}

	private static bool HasAvailableForTier(LevelContext ctx, int tier)
	{
		List<string> tierFilesUnderKind = GetTierFilesUnderKind(ctx, tier);
		for (int i = 0; i < tierFilesUnderKind.Count; i++)
		{
			if (RandomLevelSelector.HasAvailableEntry(ctx, tierFilesUnderKind[i]))
			{
				return true;
			}
		}
		return false;
	}

	private static List<string> GetTierFilesUnderKind(LevelContext ctx, int tier)
	{
		List<string> list = new List<string>();
		LevelRepository repository = LevelSystem.GetRepository();
		if (repository == null)
		{
			return list;
		}
		List<string> filesWithDimensionValue = repository.GetFilesWithDimensionValue("Tier", tier.ToString("D2"));
		if (filesWithDimensionValue == null || filesWithDimensionValue.Count == 0)
		{
			return list;
		}
		List<string> list2 = repository.AllCsvIds().ToList();
		foreach (IDimensionStrategy dimension in LevelSystem.GetDimensions())
		{
			if (dimension is LevelTierDimension)
			{
				break;
			}
			list2 = dimension.FilterFiles(ctx, repository, list2);
			if (list2.Count == 0)
			{
				return list;
			}
		}
		for (int i = 0; i < filesWithDimensionValue.Count; i++)
		{
			if (list2.Contains(filesWithDimensionValue[i]))
			{
				list.Add(filesWithDimensionValue[i]);
			}
		}
		return list;
	}

	private static string PickFileForTier(LevelContext ctx, int tier)
	{
		List<string> tierFilesUnderKind = GetTierFilesUnderKind(ctx, tier);
		if (tierFilesUnderKind.Count == 0)
		{
			return null;
		}
		return tierFilesUnderKind[Random.Range(0, tierFilesUnderKind.Count)];
	}

	private static void SetFullRandomFlag(LevelContext ctx, bool value)
	{
		if (ctx?.Data != null)
		{
			ctx.Data["RL_FullRandomPick"] = value;
		}
	}

	private string FilterAndPick(LevelContext ctx)
	{
		LevelRepository repository = LevelSystem.GetRepository();
		if (repository == null)
		{
			return null;
		}
		List<string> list = repository.AllCsvIds().ToList();
		foreach (IDimensionStrategy dimension in LevelSystem.GetDimensions())
		{
			list = dimension.FilterFiles(ctx, repository, list);
			if (list.Count == 0)
			{
				break;
			}
		}
		if (list.Count == 0)
		{
			return null;
		}
		string text = ((list.Count == 1) ? list[0] : list[Random.Range(0, list.Count)]);
		((DefaultLevelMatchAdapter)this).UpdateCurrentIntervalId(ctx, text);
		return text;
	}

	private string PickRandomWholeRepository(LevelContext ctx)
	{
		LevelRepository repository = LevelSystem.GetRepository();
		if (repository == null)
		{
			return null;
		}
		List<string> list = repository.AllCsvIds().ToList();
		if (list.Count == 0)
		{
			return null;
		}
		string text = list[Random.Range(0, list.Count)];
		((DefaultLevelMatchAdapter)this).UpdateCurrentIntervalId(ctx, text);
		return text;
	}

	private static int ResolveTier(LevelContext ctx)
	{
		string key = "Tier_Fixed";
		if (ctx != null && ctx.Data.TryGetValue(key, out var value) && value != null && int.TryParse(value.ToString(), out var result))
		{
			return Mathf.Clamp(result, 1, 8);
		}
		return 4;
	}

	private static void SetTier(LevelContext ctx, int tier)
	{
		if (ctx != null)
		{
			ctx.Data["Tier_Fixed"] = tier.ToString("D2");
		}
	}
}
