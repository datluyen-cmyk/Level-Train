using System;
using System.Collections.Generic;
using XingYun.Experiment;

namespace RotateRings.Common;

public static class FeatureConfig
{
	public static class Audio
	{
		public static class AudioFunc
		{
			public static int BGMStyleId { get; set; } = 1;
		}
	}

	public static class UI
	{
		public static class UserSetting
		{
			public static int HeadInfoType { get; set; }
		}
	}

	public static class Gameplay
	{
		public static class GameBord
		{
			public static int CardDragFailedTipsStrategyId { get; set; }
		}
	}

	public static class Chest
	{
		public static int UnlockLevel { get; set; } = 1;

		public static int StageRewardId { get; set; } = 1;

		public static int UI { get; set; } = 1;
	}

	public static class LevelRandom
	{
		public const string DefaultPoolName = "Hand_Library1";

		public const int DefaultWindowSize = 10;

		public const int WinConditionNormal = 0;

		public const int WinConditionNoProp = 1;

		public const int DefaultWinCondition = 0;

		public static bool ResolveEnable()
		{
			return FunctionDataCenter.Get<LevelRandomFunction>()?.Enable ?? false;
		}

		public static int[] ResolveGroupIds()
		{
			int[] array = ResolveLevelGroupIds();
			int[] array2 = ResolveLevelTypeGroupIds();
			int[] array3 = ResolvePartsGroupIds();
			int[] array4 = ResolveDAWinLoseGroupIds();
			if (array.Length == 0 && array2.Length == 0 && array3.Length == 0 && array4.Length == 0)
			{
				return Array.Empty<int>();
			}
			List<int> list = new List<int>(array.Length + array2.Length + array3.Length + array4.Length);
			AppendDistinct(list, array);
			AppendDistinct(list, array2);
			AppendDistinct(list, array3);
			AppendDistinct(list, array4);
			return list.ToArray();
		}

		public static int[] ResolveLevelGroupIds()
		{
			return ParseGroupIds(FunctionDataCenter.Get<LevelRandomFunction>()?.LevelGroup);
		}

		public static int[] ResolveLevelTypeGroupIds()
		{
			return ParseGroupIds(FunctionDataCenter.Get<LevelRandomFunction>()?.LevelTypeGroup);
		}

		public static int[] ResolvePartsGroupIds()
		{
			return ParseGroupIds(FunctionDataCenter.Get<LevelRandomFunction>()?.PartsGroup);
		}

		public static int[] ResolveDAWinLoseGroupIds()
		{
			return ParseGroupIds(FunctionDataCenter.Get<LevelRandomFunction>()?.DAWinLoseGroup);
		}

		private static void AppendDistinct(List<int> target, int[] source)
		{
			for (int i = 0; i < source.Length; i++)
			{
				if (!target.Contains(source[i]))
				{
					target.Add(source[i]);
				}
			}
		}

		public static int[] ParseGroupIds(string text)
		{
			if (string.IsNullOrEmpty(text))
			{
				return Array.Empty<int>();
			}
			string[] array = text.Split(',');
			List<int> list = new List<int>(array.Length);
			for (int i = 0; i < array.Length; i++)
			{
				string text2 = array[i].Trim();
				if (text2.Length > 0 && int.TryParse(text2, out var result) && !list.Contains(result))
				{
					list.Add(result);
				}
			}
			return list.ToArray();
		}

		public static string ResolvePoolName()
		{
			LevelRandomFunction levelRandomFunction = FunctionDataCenter.Get<LevelRandomFunction>();
			if (!string.IsNullOrEmpty(levelRandomFunction?.PoolName))
			{
				return levelRandomFunction.PoolName;
			}
			return "Hand_Library1";
		}

		public static int ResolveWindowSize()
		{
			LevelRandomFunction levelRandomFunction = FunctionDataCenter.Get<LevelRandomFunction>();
			if (levelRandomFunction == null || levelRandomFunction.WindowSize <= 0)
			{
				return 10;
			}
			return levelRandomFunction.WindowSize;
		}

		public static int ResolveWinCondition()
		{
			LevelRandomFunction levelRandomFunction = FunctionDataCenter.Get<LevelRandomFunction>();
			if (levelRandomFunction == null)
			{
				return 0;
			}
			int winCondition = levelRandomFunction.WinCondition;
			if (winCondition != 0 && winCondition != 1)
			{
				return 0;
			}
			return levelRandomFunction.WinCondition;
		}
	}

	public static PushFunctionData PushFunction { get; } = new PushFunctionData();

	public static DailyChallengeGameData DailyChallengeGameFunction { get; } = new DailyChallengeGameData();

	public static InterstitialFunction InterstitialFunction => FunctionDataCenter.Get<InterstitialFunction>();

	public static PropFunction Prop => FunctionDataCenter.Get<PropFunction>();

	public static LevelStepLimitFunction LevelStepLimit => FunctionDataCenter.Get<LevelStepLimitFunction>();

	public static LevelEndFunction LevelEnd => FunctionDataCenter.Get<LevelEndFunction>();

	public static ShopFunction ShopFunction => FunctionDataCenter.Get<ShopFunction>();
}
