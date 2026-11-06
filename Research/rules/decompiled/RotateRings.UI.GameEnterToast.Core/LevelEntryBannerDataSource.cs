using System;
using System.Collections.Generic;
using RotateRings.Common;
using RotateRings.GameWinIQ;
using UnityEngine;

namespace RotateRings.UI.GameEnterToast.Core;

public static class LevelEntryBannerDataSource
{
	private const string TAG = "[LevelEntryBannerDataSource] ";

	private const string IqLangKey = "LevelEntryBanner_6";

	private static readonly Dictionary<BannerDifficultyGroup, string[]> KeyPool = new Dictionary<BannerDifficultyGroup, string[]>
	{
		{
			BannerDifficultyGroup.Simple,
			new string[7] { "LevelEntryBanner_0", "LevelEntryBanner_1", "LevelEntryBanner_2", "LevelEntryBanner_3", "LevelEntryBanner_4", "LevelEntryBanner_5", "LevelEntryBanner_6" }
		},
		{
			BannerDifficultyGroup.Hard,
			new string[4] { "LevelEntryBanner_6", "LevelEntryBanner_7", "LevelEntryBanner_8", "LevelEntryBanner_9" }
		},
		{
			BannerDifficultyGroup.Shape,
			new string[3] { "LevelEntryBanner_10", "LevelEntryBanner_11", "LevelEntryBanner_12" }
		}
	};

	public static (string langKey, object[] args) Pick(int level, ELevelDifficulty difficulty)
	{
		BannerDifficultyGroup bannerDifficultyGroup = ResolveGroup(level, difficulty);
		string[] array = ResolvePool(bannerDifficultyGroup);
		if (array == null || array.Length == 0)
		{
			return (langKey: null, args: null);
		}
		string lastShownKey = LevelEntryBannerLastShownStore.GetLastShownKey(bannerDifficultyGroup);
		string text = PickAvoidRepeat(array, lastShownKey);
		object[] item = BuildArgs(text);
		LevelEntryBannerLastShownStore.SetLastShownKey(bannerDifficultyGroup, text);
		return (langKey: text, args: item);
	}

	private static string[] ResolvePool(BannerDifficultyGroup group)
	{
		string[] array = KeyPool[group];
		if (array == null || array.Length == 0)
		{
			return Array.Empty<string>();
		}
		if (GameWinIQProxy.IsExperimentEnabled())
		{
			return array;
		}
		List<string> list = new List<string>(array.Length);
		string[] array2 = array;
		foreach (string text in array2)
		{
			if (text != "LevelEntryBanner_6")
			{
				list.Add(text);
			}
		}
		_ = list.Count;
		_ = array.Length;
		return list.ToArray();
	}

	private static BannerDifficultyGroup ResolveGroup(int level, ELevelDifficulty difficulty)
	{
		if (LevelEntryBannerConfig.IsShapeLevel(level))
		{
			return BannerDifficultyGroup.Shape;
		}
		if (difficulty == ELevelDifficulty.Hard || difficulty == ELevelDifficulty.VeryHard)
		{
			return BannerDifficultyGroup.Hard;
		}
		return BannerDifficultyGroup.Simple;
	}

	private static string PickAvoidRepeat(string[] pool, string lastKey)
	{
		if (pool.Length <= 1)
		{
			return pool[0];
		}
		string text;
		do
		{
			text = pool[Random.Range(0, pool.Length)];
		}
		while (text == lastKey);
		return text;
	}

	private static object[] BuildArgs(string langKey)
	{
		int tempPlayerIq = LevelEntryBannerConfig.TempPlayerIq;
		switch (langKey)
		{
		case "LevelEntryBanner_4":
		{
			int num6 = Random.Range(45, 66);
			return new object[1] { num6 };
		}
		case "LevelEntryBanner_5":
		{
			int num5 = Random.Range(65, 76);
			return new object[1] { num5 };
		}
		case "LevelEntryBanner_6":
		{
			int num2 = tempPlayerIq - 10;
			int num3 = tempPlayerIq + 11;
			int num4 = Random.Range(num2, num3);
			return new object[1] { num4 };
		}
		case "LevelEntryBanner_7":
		{
			int num = Random.Range(75, 91);
			return new object[1] { num };
		}
		default:
			return Array.Empty<object>();
		}
	}
}
