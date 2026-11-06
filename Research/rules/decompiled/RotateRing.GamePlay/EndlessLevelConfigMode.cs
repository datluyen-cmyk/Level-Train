using System;
using Example.Cfg;
using RotateRings.Model;

namespace RotateRing.GamePlay;

public sealed class EndlessLevelConfigMode : LevelConfigModeBase
{
	private readonly Func<ExampleTables, ILevelTable>[] endlessLevelTableGetters = new Func<ExampleTables, ILevelTable>[21]
	{
		(ExampleTables t) => t.TbEndLessLevelConfig0,
		(ExampleTables t) => t.TbEndLessLevelConfig1,
		(ExampleTables t) => t.TbEndLessLevelConfig2,
		(ExampleTables t) => t.TbEndLessLevelConfig3,
		(ExampleTables t) => t.TbEndLessLevelConfig4,
		(ExampleTables t) => t.TbEndLessLevelConfig5,
		(ExampleTables t) => t.TbEndLessLevelConfig6,
		(ExampleTables t) => t.TbEndLessLevelConfig7,
		(ExampleTables t) => t.TbEndLessLevelConfig8,
		(ExampleTables t) => t.TbEndLessLevelConfig9,
		(ExampleTables t) => t.TbEndLessLevelConfig10,
		(ExampleTables t) => t.TbEndLessLevelConfig11,
		(ExampleTables t) => t.TbEndLessLevelConfig12,
		(ExampleTables t) => t.TbEndLessLevelConfig13,
		(ExampleTables t) => t.TbEndLessLevelConfig14,
		(ExampleTables t) => t.TbEndLessLevelConfig15,
		(ExampleTables t) => t.TbEndLessLevelConfig16,
		(ExampleTables t) => t.TbEndLessLevelConfig17,
		(ExampleTables t) => t.TbEndLessLevelConfig18,
		(ExampleTables t) => t.TbEndLessLevelConfig19,
		(ExampleTables t) => t.TbEndLessLevelConfig20
	};

	public override GameMode Mode => GameMode.Endless;

	public override int GetConfigLevelId(int playerLevel)
	{
		if (playerLevel <= 0)
		{
			return 1;
		}
		int endlessLoopCount = GetEndlessLoopCount();
		if (endlessLoopCount <= 0)
		{
			return 1;
		}
		return (playerLevel - 1) % endlessLoopCount + 1;
	}

	public override LevelConfig GetLevelConfig(int levelId)
	{
		ExampleTables tables = GetTables();
		int gapIndex = LevelGapDegExp.Execute();
		return GetEndlessLevelTable(tables, gapIndex).Get(levelId);
	}

	private int GetEndlessLoopCount()
	{
		return GetEndlessTableRowCount() / 10 * 10;
	}

	private int GetEndlessTableRowCount()
	{
		ExampleTables tables = GetTables();
		int gapIndex = LevelGapDegExp.Execute();
		return GetEndlessLevelTable(tables, gapIndex).TableRowCount;
	}

	private ILevelTable GetEndlessLevelTable(ExampleTables tables, int gapIndex)
	{
		return endlessLevelTableGetters[gapIndex](tables);
	}
}
