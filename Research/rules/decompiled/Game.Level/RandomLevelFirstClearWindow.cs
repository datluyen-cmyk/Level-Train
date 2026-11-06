using System;
using System.Collections.Generic;
using Example.Cfg;
using Game.DataTrack;
using Hot.Model;
using RotateRings.Common;
using RotateRings.Prop;
using XingYun.Save;

namespace Game.Level;

public static class RandomLevelFirstClearWindow
{
	private const string TAG = "[RL][Window] ";

	private const string SAVE_SLOT = "RandomLevelFirstClearWindow";

	public const int DefaultWindowSize = 10;

	public const float Layer0Rate = 0.95f;

	public const float Layer1Rate = 0.85f;

	public const float Layer2Rate = 0.75f;

	public const string STATUS_WIN = "成功";

	public const string STATUS_RESTART = "重新开始";

	private static readonly int[] SpecifiedLevelsExcluded = new int[2] { 101, 102 };

	public static RandomLevelFirstClearWindowData Data => SaveManager.Instance.GetOrCreate<RandomLevelFirstClearWindowData>("RandomLevelFirstClearWindow");

	private static RandomLevelFirstClearWindowData DataSafe()
	{
		RandomLevelFirstClearWindowData data = Data;
		if (data.Window == null)
		{
			data.Window = new List<int>();
		}
		if (data.GameWindow == null)
		{
			data.GameWindow = new List<int>();
		}
		return data;
	}

	public static void OnLevelEnter(int level, bool isSpecified)
	{
		if (level > 0 && FeatureConfig.LevelRandom.ResolveEnable())
		{
			RandomLevelFirstClearWindowData randomLevelFirstClearWindowData = DataSafe();
			if (randomLevelFirstClearWindowData.CurrentLevel != level)
			{
				randomLevelFirstClearWindowData.CurrentLevel = level;
				randomLevelFirstClearWindowData.CurrentLevelRestarted = false;
				randomLevelFirstClearWindowData.CurrentLevelReported = false;
				randomLevelFirstClearWindowData.CurrentRoundSettled = false;
			}
			randomLevelFirstClearWindowData.CurrentLevelSpecified = isSpecified;
			randomLevelFirstClearWindowData.CurrentRunUsedProp = false;
		}
	}

	public static void OnPropUsed()
	{
		if (FeatureConfig.LevelRandom.ResolveEnable())
		{
			DataSafe().CurrentRunUsedProp = true;
		}
	}

	public static void OnStepsExhausted()
	{
		if (!FeatureConfig.LevelRandom.ResolveEnable())
		{
			return;
		}
		RandomLevelFirstClearWindowData randomLevelFirstClearWindowData = DataSafe();
		if (GameInfo.LevelId != randomLevelFirstClearWindowData.CurrentLevel || randomLevelFirstClearWindowData.CurrentRoundSettled)
		{
			return;
		}
		if (IsSpecifiedExcluded(randomLevelFirstClearWindowData.CurrentLevel))
		{
			randomLevelFirstClearWindowData.CurrentRoundSettled = true;
			return;
		}
		randomLevelFirstClearWindowData.CurrentLevelRestarted = true;
		if (HasClearedAnyThisSession())
		{
			int windowSize = FeatureConfig.LevelRandom.ResolveWindowSize();
			Push(randomLevelFirstClearWindowData.GameWindow, 0, windowSize);
		}
		randomLevelFirstClearWindowData.CurrentRoundSettled = true;
		randomLevelFirstClearWindowData.CurrentRunUsedProp = false;
	}

	public static void OnStepsAdded()
	{
		RandomLevelFirstClearWindowData randomLevelFirstClearWindowData = DataSafe();
		randomLevelFirstClearWindowData.CurrentRoundSettled = false;
		randomLevelFirstClearWindowData.CurrentRunUsedProp = false;
	}

	public static void OnGameEnd(string status)
	{
		if (string.IsNullOrEmpty(status) || !FeatureConfig.LevelRandom.ResolveEnable())
		{
			return;
		}
		RandomLevelFirstClearWindowData randomLevelFirstClearWindowData = DataSafe();
		if (GameInfo.LevelId != randomLevelFirstClearWindowData.CurrentLevel)
		{
			return;
		}
		if (IsSpecifiedExcluded(randomLevelFirstClearWindowData.CurrentLevel))
		{
			randomLevelFirstClearWindowData.CurrentRunUsedProp = false;
			return;
		}
		int windowSize = FeatureConfig.LevelRandom.ResolveWindowSize();
		if (string.Equals(status, "成功", StringComparison.Ordinal))
		{
			if (!randomLevelFirstClearWindowData.CurrentLevelReported)
			{
				int value = ResolveLevelValue(randomLevelFirstClearWindowData);
				Push(randomLevelFirstClearWindowData.Window, value, windowSize);
				randomLevelFirstClearWindowData.CurrentLevelReported = true;
			}
			int value2 = ResolveRoundValue(randomLevelFirstClearWindowData);
			Push(randomLevelFirstClearWindowData.GameWindow, value2, windowSize);
			randomLevelFirstClearWindowData.CurrentRunUsedProp = false;
			randomLevelFirstClearWindowData.CurrentRoundSettled = false;
		}
		else if (string.Equals(status, "重新开始", StringComparison.Ordinal))
		{
			if (randomLevelFirstClearWindowData.CurrentRoundSettled)
			{
				randomLevelFirstClearWindowData.CurrentRoundSettled = false;
				randomLevelFirstClearWindowData.CurrentRunUsedProp = false;
				return;
			}
			if (!HasClearedAnyThisSession())
			{
				randomLevelFirstClearWindowData.CurrentRunUsedProp = false;
				return;
			}
			randomLevelFirstClearWindowData.CurrentLevelRestarted = true;
			Push(randomLevelFirstClearWindowData.GameWindow, 0, windowSize);
			randomLevelFirstClearWindowData.CurrentRunUsedProp = false;
			randomLevelFirstClearWindowData.CurrentRoundSettled = false;
		}
	}

	public static float GetLevelRate()
	{
		return Rate(DataSafe().Window);
	}

	public static float GetGameRate()
	{
		return Rate(DataSafe().GameWindow);
	}

	public static int GetLayer()
	{
		return LayerOf(GetLevelRate());
	}

	public static void Clear()
	{
		RandomLevelFirstClearWindowData randomLevelFirstClearWindowData = DataSafe();
		randomLevelFirstClearWindowData.Window.Clear();
		randomLevelFirstClearWindowData.GameWindow.Clear();
		randomLevelFirstClearWindowData.CurrentLevel = -1;
		randomLevelFirstClearWindowData.CurrentLevelRestarted = false;
		randomLevelFirstClearWindowData.CurrentLevelReported = false;
		randomLevelFirstClearWindowData.CurrentLevelSpecified = false;
		randomLevelFirstClearWindowData.CurrentRunUsedProp = false;
		randomLevelFirstClearWindowData.CurrentRoundSettled = false;
	}

	public static string DescribeState()
	{
		RandomLevelFirstClearWindowData randomLevelFirstClearWindowData = DataSafe();
		int num = Math.Max(1, FeatureConfig.LevelRandom.ResolveWindowSize());
		string text = ((randomLevelFirstClearWindowData.Window.Count > 0) ? string.Join(string.Empty, randomLevelFirstClearWindowData.Window) : "空");
		string text2 = ((randomLevelFirstClearWindowData.GameWindow.Count > 0) ? string.Join(string.Empty, randomLevelFirstClearWindowData.GameWindow) : "空");
		return $"按关=[{text}]({randomLevelFirstClearWindowData.Window.Count}/{num}) rateLevel={GetLevelRate():F2}, " + $"按局=[{text2}]({randomLevelFirstClearWindowData.GameWindow.Count}/{num}) rateGame={GetGameRate():F2}, " + $"layer=L{GetLayer()}, level={randomLevelFirstClearWindowData.CurrentLevel}, specified={randomLevelFirstClearWindowData.CurrentLevelSpecified}, " + $"reported={randomLevelFirstClearWindowData.CurrentLevelReported}, restarted={randomLevelFirstClearWindowData.CurrentLevelRestarted}, usedProp={randomLevelFirstClearWindowData.CurrentRunUsedProp}";
	}

	private static int ResolveRoundValue(RandomLevelFirstClearWindowData data)
	{
		if (FeatureConfig.LevelRandom.ResolveWinCondition() == 1 && data.CurrentRunUsedProp && !IsPropUnlockLevelExempt(data.CurrentLevel))
		{
			return 0;
		}
		return 1;
	}

	private static int ResolveLevelValue(RandomLevelFirstClearWindowData data)
	{
		if (data.CurrentLevelRestarted)
		{
			return 0;
		}
		if (FeatureConfig.LevelRandom.ResolveWinCondition() == 1 && data.CurrentRunUsedProp && !IsPropUnlockLevelExempt(data.CurrentLevel))
		{
			return 0;
		}
		return 1;
	}

	private static bool IsPropUnlockLevelExempt(int level)
	{
		if (level <= 0)
		{
			return false;
		}
		PropModelProxy propModelProxy = GameApp.Proxy.Get<PropModelProxy>();
		if (propModelProxy == null)
		{
			return false;
		}
		List<PropConfig> inGamePropConfigs = propModelProxy.GetInGamePropConfigs();
		if (inGamePropConfigs == null)
		{
			return false;
		}
		for (int i = 0; i < inGamePropConfigs.Count; i++)
		{
			if (propModelProxy.GetPropUnlockLv(inGamePropConfigs[i]) == level)
			{
				return true;
			}
		}
		return false;
	}

	private static bool IsSpecifiedExcluded(int level)
	{
		for (int i = 0; i < SpecifiedLevelsExcluded.Length; i++)
		{
			if (level == SpecifiedLevelsExcluded[i])
			{
				return true;
			}
		}
		return false;
	}

	private static bool HasClearedAnyThisSession()
	{
		return RRDataTrack.Instance.HasClearedAnyThisSession;
	}

	private static float Rate(List<int> window)
	{
		if (window == null || window.Count == 0)
		{
			return 0f;
		}
		int num = Math.Max(1, FeatureConfig.LevelRandom.ResolveWindowSize());
		int num2 = 0;
		for (int i = 0; i < window.Count; i++)
		{
			num2 += window[i];
		}
		return (float)num2 / (float)num;
	}

	private static int LayerOf(float rate)
	{
		if (rate >= 0.95f)
		{
			return 0;
		}
		if (rate >= 0.85f)
		{
			return 1;
		}
		if (rate >= 0.75f)
		{
			return 2;
		}
		return 3;
	}

	private static void Push(List<int> window, int value, int windowSize)
	{
		if (value >= 0)
		{
			window.Add(value);
			while (window.Count > windowSize)
			{
				window.RemoveAt(0);
			}
		}
	}
}
