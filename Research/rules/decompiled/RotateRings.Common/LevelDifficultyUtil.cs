using System.Collections.Generic;
using Example.Cfg;
using RotateRing.GamePlay;
using RotateRings.Model;
using RotateRings.SpecialLevel;

namespace RotateRings.Common;

public static class LevelDifficultyUtil
{
	private const int CfgGameModeMain = 0;

	private const int CfgGameModeEndless = 1;

	private const int ExperimentDifficultyStartLevel = 11;

	public static ELevelDifficulty GetDifficulty(GameMode gameMode, int level)
	{
		if (SpecialLevelModule.IsSpecialLevelModeFor(gameMode))
		{
			return ELevelDifficulty.Normal;
		}
		if (TryGetRandomExperimentDifficulty(gameMode, level, out var difficulty))
		{
			return difficulty;
		}
		return ResolveBoardDifficulty(gameMode, level);
	}

	private static ELevelDifficulty ResolveBoardDifficulty(GameMode gameMode, int level)
	{
		List<LevelDifficult> list = GameApp.Config.GetTable<ExampleTables>()?.TbLevelDifficult?.DataList;
		if (list == null)
		{
			return ELevelDifficulty.Normal;
		}
		int num = ToCfgGameMode(gameMode);
		foreach (LevelDifficult item2 in list)
		{
			if (item2 != null && item2.GameMode == num && level >= item2.Begin && level <= item2.End && item2.Divisor > 0)
			{
				int item = level % item2.Divisor;
				if (item2.Remainder != null && item2.Remainder.Contains(item))
				{
					return (ELevelDifficulty)item2.DifficultMode;
				}
			}
		}
		return ELevelDifficulty.Normal;
	}

	public static bool IsHardLevel(GameMode gameMode, int level)
	{
		return GetDifficulty(gameMode, level) != ELevelDifficulty.Normal;
	}

	private static bool TryGetRandomExperimentDifficulty(GameMode gameMode, int level, out ELevelDifficulty difficulty)
	{
		difficulty = ELevelDifficulty.Normal;
		if (gameMode != GameMode.Main || level < 11 || !FeatureConfig.LevelRandom.ResolveEnable())
		{
			return false;
		}
		int bonus;
		int num = (RandomSelectLevelService.TryGetMatchedBonus(level, out bonus) ? bonus : FallbackDigitBonus(level));
		ELevelDifficulty eLevelDifficulty = ((num >= 2) ? ELevelDifficulty.VeryHard : ((num == 1) ? ELevelDifficulty.Hard : ELevelDifficulty.Normal));
		difficulty = eLevelDifficulty;
		return true;
	}

	private static int FallbackDigitBonus(int level)
	{
		return (level % 10) switch
		{
			5 => 1, 
			9 => 2, 
			_ => 0, 
		};
	}

	private static int ToCfgGameMode(GameMode gameMode)
	{
		if (gameMode != GameMode.Endless)
		{
			return 0;
		}
		return 1;
	}
}
