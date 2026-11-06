using System.Collections.Generic;
using System.Globalization;
using Example.Cfg;
using UnityEngine;

namespace RotateRings.GameWinIQ;

public sealed class GameWinIQRuntimeConfig
{
	internal const int DefaultFallbackGroup = 1;

	internal const int UnlockCompletedLevel = 6;

	internal const float DisplaySliderTrackMin = 0f;

	private const int DisplaySliderHeadroomAbovePeak = 30;

	public int ExperimentValue { get; internal set; }

	public int EffectiveGroup { get; internal set; }

	public bool UsedFallbackGroup { get; internal set; }

	public int MinBaseScore { get; internal set; }

	public int MaxBaseScore { get; internal set; }

	public int PeakGeniusDisplayThreshold => MaxBaseScore;

	public int CalcRule1Min { get; internal set; }

	public int CalcRule1Max { get; internal set; }

	public int CalcRule2Min { get; internal set; }

	public int CalcRule2Max { get; internal set; }

	public int InvalidStepThreshold { get; internal set; }

	public int CrossDayDecayThreshold { get; internal set; }

	public float CrossDayDecayFactor { get; internal set; }

	public IReadOnlyList<GameWinIQRangeBand> RangeBands { get; internal set; }

	public GameWinIQCalcRules CalcRulesRow { get; internal set; }

	public GameWinIQLevelSuffixCoe LevelSuffixRow { get; internal set; }

	public GameWinIQMaxDecay MaxDecayRow { get; internal set; }

	public float[] SliderAnchors { get; internal set; }

	public float DisplaySliderMax => SliderAnchors[^1];

	public float GetSliderTrackMax()
	{
		if (RangeBands.Count == 0 || SliderAnchors.Length < 2)
		{
			return DisplaySliderMax;
		}
		int displayScore = MaxBaseScore + 30;
		return Mathf.Max(DisplaySliderMax, MapDisplayScoreToSlider(displayScore));
	}

	public (float Min, float Max) GetLevelSuffixCoeff(int tailDigit)
	{
		if (LevelSuffixRow == null)
		{
			return (Min: 1f, Max: 1f);
		}
		return ParseCoeffPair(Mathf.Clamp(tailDigit, 0, 9) switch
		{
			0 => LevelSuffixRow.LevelSuffix0, 
			1 => LevelSuffixRow.LevelSuffix1, 
			2 => LevelSuffixRow.LevelSuffix2, 
			3 => LevelSuffixRow.LevelSuffix3, 
			4 => LevelSuffixRow.LevelSuffix4, 
			5 => LevelSuffixRow.LevelSuffix5, 
			6 => LevelSuffixRow.LevelSuffix6, 
			7 => LevelSuffixRow.LevelSuffix7, 
			8 => LevelSuffixRow.LevelSuffix8, 
			_ => LevelSuffixRow.LevelSuffix9, 
		});
	}

	public int GetRangeIndex(int baseScore)
	{
		for (int i = 0; i < RangeBands.Count; i++)
		{
			if (RangeBands[i].Contains(baseScore))
			{
				return i;
			}
		}
		return Mathf.Max(0, RangeBands.Count - 1);
	}

	public GameWinIQRangeBand GetRangeBand(int baseScore)
	{
		int rangeIndex = GetRangeIndex(baseScore);
		return RangeBands[rangeIndex];
	}

	public float MapDisplayScoreToSlider(int displayScore)
	{
		int num = ClampDisplayMinimum(displayScore);
		if (RangeBands.Count == 0 || SliderAnchors.Length < RangeBands.Count + 1)
		{
			return 0f;
		}
		for (int i = 0; i < RangeBands.Count; i++)
		{
			GameWinIQRangeBand gameWinIQRangeBand = RangeBands[i];
			if (gameWinIQRangeBand.Contains(num))
			{
				float num2 = InverseLerp(gameWinIQRangeBand.MinInclusive, gameWinIQRangeBand.MaxScore, num);
				return Mathf.Lerp(SliderAnchors[i], SliderAnchors[i + 1], num2);
			}
		}
		IReadOnlyList<GameWinIQRangeBand> rangeBands = RangeBands;
		GameWinIQRangeBand gameWinIQRangeBand2 = rangeBands[rangeBands.Count - 1];
		int num3 = RangeBands.Count - 1;
		int num4 = gameWinIQRangeBand2.MaxScore - gameWinIQRangeBand2.MinInclusive;
		if (num4 <= 0)
		{
			return SliderAnchors[^1];
		}
		float num5 = (float)(num - gameWinIQRangeBand2.MinInclusive) / (float)num4;
		return Mathf.Lerp(SliderAnchors[num3], SliderAnchors[num3 + 1], num5);
	}

	public int ClampBase(int baseScore)
	{
		return Mathf.Clamp(baseScore, MinBaseScore, MaxBaseScore);
	}

	public int ClampDisplayMinimum(int displayScore)
	{
		return Mathf.Max(displayScore, MinBaseScore);
	}

	internal static (float Min, float Max) ParseCoeffPair(string raw)
	{
		if (string.IsNullOrWhiteSpace(raw))
		{
			return (Min: 1f, Max: 1f);
		}
		string[] array = raw.Split(',');
		if (array.Length < 2)
		{
			return (Min: 1f, Max: 1f);
		}
		float item = float.Parse(array[0], CultureInfo.InvariantCulture);
		float item2 = float.Parse(array[1], CultureInfo.InvariantCulture);
		return (Min: item, Max: item2);
	}

	private static float InverseLerp(int a, int b, int value)
	{
		if (b <= a)
		{
			return 0f;
		}
		return Mathf.Clamp01((float)(value - a) / (float)(b - a));
	}
}
