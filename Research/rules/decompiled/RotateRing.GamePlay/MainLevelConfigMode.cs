using System;
using System.Collections.Generic;
using Example.Cfg;
using RotateRings.Model;

namespace RotateRing.GamePlay;

public sealed class MainLevelConfigMode : LevelConfigModeBase
{
	private const string TAG = "[GamePlay][MainLevelConfigMode] ";

	private readonly Func<ExampleTables, ILevelTable>[] mainLevelTableGetters = new Func<ExampleTables, ILevelTable>[21]
	{
		(ExampleTables t) => t.TbLevelConfig0,
		(ExampleTables t) => t.TbLevelConfig1,
		(ExampleTables t) => t.TbLevelConfig2,
		(ExampleTables t) => t.TbLevelConfig3,
		(ExampleTables t) => t.TbLevelConfig4,
		(ExampleTables t) => t.TbLevelConfig5,
		(ExampleTables t) => t.TbLevelConfig6,
		(ExampleTables t) => t.TbLevelConfig7,
		(ExampleTables t) => t.TbLevelConfig8,
		(ExampleTables t) => t.TbLevelConfig9,
		(ExampleTables t) => t.TbLevelConfig10,
		(ExampleTables t) => t.TbLevelConfig11,
		(ExampleTables t) => t.TbLevelConfig12,
		(ExampleTables t) => t.TbLevelConfig13,
		(ExampleTables t) => t.TbLevelConfig14,
		(ExampleTables t) => t.TbLevelConfig15,
		(ExampleTables t) => t.TbLevelConfig16,
		(ExampleTables t) => t.TbLevelConfig17,
		(ExampleTables t) => t.TbLevelConfig18,
		(ExampleTables t) => t.TbLevelConfig19,
		(ExampleTables t) => t.TbLevelConfig20
	};

	private const int FallbackCoreLevelCount = 100;

	internal const int LoopStartLevel = 21;

	private int coreLevelCountOverride;

	public const int RandomExperimentStartLevel = 1;

	public override GameMode Mode => GameMode.Main;

	public int GetCoreLevelCount()
	{
		if (coreLevelCountOverride > 0)
		{
			return coreLevelCountOverride;
		}
		return ReadCoreLevelCountFromTable();
	}

	public void SetCoreLevelCount(int count)
	{
		if (count == 0)
		{
			coreLevelCountOverride = 0;
			return;
		}
		if (count < 21)
		{
			count = 21;
		}
		coreLevelCountOverride = count;
	}

	private int ReadCoreLevelCountFromTable()
	{
		ExampleTables tables = GetTables();
		if (tables == null)
		{
			return 100;
		}
		int gapIndex = LevelGapDegExp.Execute();
		int tableRowCount = GetMainLevelTable(tables, gapIndex).TableRowCount;
		if (tableRowCount < 21)
		{
			return 100;
		}
		return tableRowCount;
	}

	public override int GetConfigLevelId(int playerLevel)
	{
		if (playerLevel <= 0)
		{
			return 1;
		}
		int coreLevelCount = GetCoreLevelCount();
		if (playerLevel <= coreLevelCount)
		{
			return playerLevel;
		}
		int num = coreLevelCount - 21 + 1;
		return 21 + (playerLevel - coreLevelCount - 1) % num;
	}

	public override LevelConfig GetLevelConfig(int levelId)
	{
		ExampleTables tables = GetTables();
		int gapIndex = LevelGapDegExp.Execute();
		return GetMainLevelTable(tables, gapIndex).Get(levelId);
	}

	private ILevelTable GetMainLevelTable(ExampleTables tables, int gapIndex)
	{
		return mainLevelTableGetters[gapIndex](tables);
	}

	internal bool TryGetLevelIdByCoreLevelPath(string coreLevelPath, out int levelId)
	{
		levelId = -1;
		if (string.IsNullOrEmpty(coreLevelPath))
		{
			return false;
		}
		ExampleTables tables = GetTables();
		if (tables == null)
		{
			return false;
		}
		List<LevelConfig> list = GetMainLevelTable(tables, LevelGapDegExp.Execute())?.DataList;
		if (list == null || list.Count == 0)
		{
			return false;
		}
		bool flag = false;
		for (int i = 0; i < list.Count; i++)
		{
			LevelConfig levelConfig = list[i];
			if (levelConfig != null && string.Equals(levelConfig.CoreLevelPath, coreLevelPath, StringComparison.Ordinal) && (!flag || levelConfig.LevelID < levelId))
			{
				levelId = levelConfig.LevelID;
				flag = true;
			}
		}
		return flag;
	}
}
