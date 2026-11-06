using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using Example.Cfg;
using UniLog;
using XingYun.Config;
using XingYun.Experiment;

namespace RotateRings.GameWinIQ;

public static class GameWinIQConfigProvider
{
	private const string Tag = "[GameWinIQConfig] ";

	private static readonly float[] DefaultSliderAnchors = new float[5] { 0.33f, 1.2f, 1.8f, 2.3f, 3.3f };

	private static int lastLoggedGroup = int.MinValue;

	public static int GetExperimentValue()
	{
		return FunctionDataCenter.Get<LevelEndFunction>()?.GameWinIQ ?? 0;
	}

	public static bool IsExperimentEnabled()
	{
		return GetExperimentValue() != 0;
	}

	public static bool TryGetConfig(out GameWinIQRuntimeConfig config)
	{
		config = null;
		int experimentValue = GetExperimentValue();
		if (experimentValue == 0)
		{
			return false;
		}
		ConfigManager config2 = GameApp.Config;
		ExampleTables exampleTables = ((config2 != null) ? config2.GetTable<ExampleTables>() : null);
		if (exampleTables == null)
		{
			return false;
		}
		int num = experimentValue;
		bool usedFallback = false;
		if (!HasGroup(exampleTables, num))
		{
			num = 1;
			usedFallback = true;
			if (!HasGroup(exampleTables, num))
			{
				Log.LogError("[GameWinIQConfig] ", "GameWinIQ 配表无 Group={0} 且无默认 Group=1 实验值={1}", new object[2] { experimentValue, experimentValue });
				return false;
			}
		}
		config = BuildConfig(experimentValue, num, usedFallback, exampleTables);
		if (num != lastLoggedGroup)
		{
			lastLoggedGroup = num;
			LogConfigLoaded(config);
		}
		return true;
	}

	private static bool HasGroup(ExampleTables tables, int group)
	{
		TbGameWinIQRange tbGameWinIQRange = tables.TbGameWinIQRange;
		if (tbGameWinIQRange == null)
		{
			return false;
		}
		return tbGameWinIQRange.DataList?.Any((GameWinIQRange r) => r.Group == group) == true;
	}

	private static T ResolveRowByGroup<T>(IReadOnlyList<T> rows, int effectiveGroup, int fallbackGroup, string tableName) where T : class
	{
		if (rows == null || rows.Count == 0)
		{
			return null;
		}
		T val = rows.FirstOrDefault((T r) => GetGroupId(r) == effectiveGroup);
		if (val != null || effectiveGroup == fallbackGroup)
		{
			return val;
		}
		val = rows.FirstOrDefault((T r) => GetGroupId(r) == fallbackGroup);
		_ = val;
		return val;
	}

	private static int GetGroupId<T>(T row)
	{
		if (!(row is GameWinIQCalcRules { Group: var result }))
		{
			if (!(row is GameWinIQLevelSuffixCoe { Group: var result2 }))
			{
				if (!(row is GameWinIQMaxDecay { Group: var result3 }))
				{
					if (!(row is GameWinIQRange { Group: var result4 }))
					{
						throw new InvalidOperationException("GameWinIQ 不支持的配表类型 " + typeof(T).Name);
					}
					return result4;
				}
				return result3;
			}
			return result2;
		}
		return result;
	}

	private static GameWinIQRuntimeConfig BuildConfig(int experimentValue, int effectiveGroup, bool usedFallback, ExampleTables tables)
	{
		List<GameWinIQRange> list = (from r in tables.TbGameWinIQRange.DataList
			where r.Group == effectiveGroup
			orderby r.Id
			select r).ToList();
		List<GameWinIQRangeBand> list2 = new List<GameWinIQRangeBand>(list.Count);
		for (int num = 0; num < list.Count; num++)
		{
			ParseIqRange(list[num].IQRange, num == list.Count - 1, out var minInclusive, out var maxBound);
			list2.Add(new GameWinIQRangeBand(list[num], minInclusive, maxBound, num == list.Count - 1));
		}
		int fallbackGroup = 1;
		GameWinIQCalcRules gameWinIQCalcRules = ResolveRowByGroup(tables.TbGameWinIQCalcRules.DataList, effectiveGroup, fallbackGroup, "GameWinIQCalcRules");
		GameWinIQLevelSuffixCoe levelSuffixRow = ResolveRowByGroup(tables.TbGameWinIQLevelSuffixCoe.DataList, effectiveGroup, fallbackGroup, "GameWinIQLevelSuffixCoe");
		GameWinIQMaxDecay gameWinIQMaxDecay = ResolveRowByGroup(tables.TbGameWinIQMaxDecay.DataList, effectiveGroup, fallbackGroup, "GameWinIQMaxDecay");
		ParseIqRangePair(gameWinIQCalcRules?.IQCalcRule1, out var minInclusive2, out var maxBound2);
		ParseIqRangePair(gameWinIQCalcRules?.IQCalcRule2, out var minInclusive3, out var maxBound3);
		int minBaseScore = ((list2.Count > 0) ? list2[0].MinInclusive : 70);
		int num2;
		if (list2.Count <= 0)
		{
			num2 = 220;
		}
		else
		{
			num2 = list2[list2.Count - 1].MaxScore;
		}
		int maxBaseScore = num2;
		return new GameWinIQRuntimeConfig
		{
			ExperimentValue = experimentValue,
			EffectiveGroup = effectiveGroup,
			UsedFallbackGroup = usedFallback,
			MinBaseScore = minBaseScore,
			MaxBaseScore = maxBaseScore,
			CalcRule1Min = minInclusive2,
			CalcRule1Max = maxBound2,
			CalcRule2Min = minInclusive3,
			CalcRule2Max = maxBound3,
			InvalidStepThreshold = (gameWinIQCalcRules?.InvalidStepThreshold ?? 3),
			CrossDayDecayThreshold = (gameWinIQMaxDecay?.Arg1 ?? 160),
			CrossDayDecayFactor = (gameWinIQMaxDecay?.Arg2 ?? 0.5f),
			RangeBands = list2,
			CalcRulesRow = gameWinIQCalcRules,
			LevelSuffixRow = levelSuffixRow,
			MaxDecayRow = gameWinIQMaxDecay,
			SliderAnchors = (float[])DefaultSliderAnchors.Clone()
		};
	}

	internal static void ParseIqRange(string raw, bool lastRangeClosed, out int minInclusive, out int maxBound)
	{
		ParseIqRangePair(raw, out minInclusive, out maxBound);
	}

	private static void ParseIqRangePair(string raw, out int minInclusive, out int maxBound)
	{
		minInclusive = 70;
		maxBound = 220;
		if (string.IsNullOrWhiteSpace(raw))
		{
			return;
		}
		string text = raw.Trim();
		int num = text.IndexOf(',');
		if (num >= 0)
		{
			if (num != 0 && num < text.Length - 1)
			{
				minInclusive = int.Parse(text.Substring(0, num), CultureInfo.InvariantCulture);
				maxBound = int.Parse(text.Substring(num + 1), CultureInfo.InvariantCulture);
			}
			return;
		}
		int num2 = ((text.Length <= 5) ? 2 : 3);
		if (text.Length >= num2 + 1)
		{
			minInclusive = int.Parse(text.Substring(0, num2), CultureInfo.InvariantCulture);
			maxBound = int.Parse(text.Substring(num2), CultureInfo.InvariantCulture);
		}
	}

	private static void LogConfigLoaded(GameWinIQRuntimeConfig config)
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.AppendFormat(CultureInfo.InvariantCulture, "GameWinIQ 配置加载 实验值={0} 生效组={1} 回退默认组={2} 基础分=[{3},{4}] CalcRule1={5}-{6} CalcRule2={7}-{8} 无效步阈值={9} 跨天衰减阈值={10} 系数={11:F2} 生涯顶分上限={12}", config.ExperimentValue, config.EffectiveGroup, config.UsedFallbackGroup, config.MinBaseScore, config.MaxBaseScore, config.CalcRule1Min, config.CalcRule1Max, config.CalcRule2Min, config.CalcRule2Max, config.InvalidStepThreshold, config.CrossDayDecayThreshold, config.CrossDayDecayFactor, config.MaxBaseScore);
		if (config.RangeBands == null)
		{
			return;
		}
		foreach (GameWinIQRangeBand rangeBand in config.RangeBands)
		{
			stringBuilder.AppendFormat(CultureInfo.InvariantCulture, " | RangeId={0} IQ={1}~{2}{3} 普通+{4} 重开{5}/{6} 解锁上限{7} 失误{8}/{9}", rangeBand.Row.Id, rangeBand.MinInclusive, rangeBand.MaxScore, rangeBand.MaxIsInclusive ? "(闭)" : "(开)", rangeBand.Row.NormalLevel, rangeBand.Row.RestartCost, rangeBand.Row.MaxRestartCost, rangeBand.Row.MaxUnlockedOC, rangeBand.Row.ErrorInvalidSteps, rangeBand.Row.MaxErrorInvalidSteps);
		}
	}
}
