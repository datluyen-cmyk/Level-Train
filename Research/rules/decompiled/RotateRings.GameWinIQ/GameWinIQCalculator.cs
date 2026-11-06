using System;
using System.Collections.Generic;
using Example.Cfg;
using RotateRings.Common;
using UnityEngine;

namespace RotateRings.GameWinIQ;

public static class GameWinIQCalculator
{
	public static int MapBaseToDisplay(GameWinIQRuntimeConfig config, int baseScore, int levelTailDigit, out float randomCoeff)
	{
		randomCoeff = 1f;
		int num = config.ClampBase(baseScore);
		if (num <= config.CalcRule1Max)
		{
			return num;
		}
		int tailDigit = Mathf.Clamp(levelTailDigit, 0, 9);
		(float Min, float Max) levelSuffixCoeff = config.GetLevelSuffixCoeff(tailDigit);
		float item = levelSuffixCoeff.Min;
		float item2 = levelSuffixCoeff.Max;
		randomCoeff = Random.Range(item, item2);
		return RoundToNearestInt((float)(num - config.CalcRule1Max) * randomCoeff + (float)config.CalcRule1Max);
	}

	public static int ApplyCrossDayDecayToBase(GameWinIQRuntimeConfig config, int baseScoreBefore)
	{
		int num = config.ClampBase(baseScoreBefore);
		if (num <= config.CrossDayDecayThreshold)
		{
			return num;
		}
		float value = (float)(num - config.CrossDayDecayThreshold) * config.CrossDayDecayFactor + (float)config.CrossDayDecayThreshold;
		return config.ClampBase(RoundToNearestInt(value));
	}

	public static int ApplyCrossDayDecayToDisplay(GameWinIQRuntimeConfig config, int displayScoreBefore)
	{
		int num = config.ClampDisplayMinimum(displayScoreBefore);
		if (num <= config.CrossDayDecayThreshold)
		{
			return num;
		}
		float value = (float)(num - config.CrossDayDecayThreshold) * config.CrossDayDecayFactor + (float)config.CrossDayDecayThreshold;
		return config.ClampDisplayMinimum(RoundToNearestInt(value));
	}

	public static int MapDifficultyColumn(ELevelDifficulty difficulty)
	{
		return difficulty switch
		{
			ELevelDifficulty.VeryHard => 0, 
			ELevelDifficulty.Hard => 1, 
			_ => 2, 
		};
	}

	public static void EvaluateLevel(GameWinIQRuntimeConfig config, int baseScoreBefore, ELevelDifficulty difficulty, int propUseCount, int baseScoreAtLevelEnter, int restartCount, IReadOnlyList<int> unlockReleaseCounts, int invalidStepCount, int completedLevel, out int newBaseScore, out int sessionPerformanceScore, out GameWinIQSettlementDetail detail)
	{
		GameWinIQRange row = config.GetRangeBand(baseScoreAtLevelEnter).Row;
		int num = MapDifficultyColumn(difficulty);
		int num2 = num switch
		{
			0 => row.ExpertLevel, 
			1 => row.HardLevel, 
			_ => row.NormalLevel, 
		};
		int num3 = Mathf.Min(Mathf.Max(0, propUseCount) * row.ItemCost, row.MaxItemCost);
		int num4 = Mathf.Abs(row.RestartCost);
		int num5 = Mathf.Abs(row.MaxRestartCost);
		int num6 = Mathf.Min(Mathf.Max(0, restartCount) * num4, num5);
		newBaseScore = config.ClampBase(baseScoreBefore + num2 + num3 - num6);
		GameWinIQRange row2 = config.GetRangeBand(newBaseScore).Row;
		int num7 = 0;
		int num8 = 0;
		if (unlockReleaseCounts != null)
		{
			for (int i = 0; i < unlockReleaseCounts.Count; i++)
			{
				int num9 = unlockReleaseCounts[i];
				if (num9 > num7)
				{
					num7 = num9;
				}
				num8 += GetUnlockTierBonus(row2, num9);
			}
		}
		int num10 = Mathf.Min(num8, row2.MaxUnlockedOC);
		int num11 = Mathf.Abs(row2.ErrorInvalidSteps);
		int num12 = Mathf.Abs(row2.MaxErrorInvalidSteps);
		int num13 = 0;
		int invalidStepThreshold = config.InvalidStepThreshold;
		if (invalidStepThreshold > 0 && invalidStepCount >= invalidStepThreshold)
		{
			num13 = Mathf.Min(invalidStepCount / invalidStepThreshold * num11, num12);
		}
		sessionPerformanceScore = num10 - num13;
		int levelTailDigit = Mathf.Abs(completedLevel) % 10;
		float randomCoeff;
		int mappedDisplayFromBase = MapBaseToDisplay(config, newBaseScore, levelTailDigit, out randomCoeff);
		detail = new GameWinIQSettlementDetail(completedLevel, levelTailDigit, num, num2, propUseCount, num3, row.MaxItemCost, restartCount, num6, num5, num7, num10, row2.MaxUnlockedOC, invalidStepCount, num13, Mathf.Abs(row2.MaxErrorInvalidSteps), randomCoeff, mappedDisplayFromBase);
	}

	public static int ComposeDisplayScore(GameWinIQRuntimeConfig config, int mappedBaseScore, int sessionPerformanceScore)
	{
		return config.ClampDisplayMinimum(mappedBaseScore + sessionPerformanceScore);
	}

	public static int GetRangeIndex(GameWinIQRuntimeConfig config, int baseScore)
	{
		return config.GetRangeIndex(baseScore);
	}

	public static float MapDisplayScoreToSlider(GameWinIQRuntimeConfig config, int displayScore)
	{
		return config.MapDisplayScoreToSlider(displayScore);
	}

	public static int ClampBase(int baseScore)
	{
		if (GameWinIQConfigProvider.TryGetConfig(out var config))
		{
			return config.ClampBase(baseScore);
		}
		return Mathf.Clamp(baseScore, 70, 220);
	}

	public static int ClampDisplay(int displayScore)
	{
		if (GameWinIQConfigProvider.TryGetConfig(out var config))
		{
			return config.ClampDisplayMinimum(displayScore);
		}
		return Mathf.Max(displayScore, 70);
	}

	private static int GetUnlockTierBonus(GameWinIQRange row, int releaseCount)
	{
		if (releaseCount > 0)
		{
			return releaseCount switch
			{
				1 => row.UnlockedOC1, 
				2 => row.UnlockedOC2, 
				_ => row.UnlockedOC3, 
			};
		}
		return 0;
	}

	private static int RoundToNearestInt(float value)
	{
		return (int)Math.Round(value, MidpointRounding.AwayFromZero);
	}
}
