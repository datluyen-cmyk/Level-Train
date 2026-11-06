using System.Collections.Generic;
using UnityEngine;

namespace RotateRings.UI.Rating;

public static class RatingRuntimeRules
{
	private const int DefaultHighStarMin = 4;

	private const int DefaultRequiredCompletedLevel = 13;

	private const float DefaultRequiredValidDurationSeconds = 900f;

	private const int DefaultMaxPopupCount = 4;

	private const int DefaultMaxCloseCount = 4;

	private const int DefaultCloseIntervalFirst = 2;

	private const int DefaultCloseIntervalSecond = 3;

	private const int DefaultCloseIntervalThird = 4;

	private const int DefaultMaxLowStarSubmitCount = 2;

	private const int DefaultLowStarInterval = 4;

	private static int highStarMin = 4;

	private static int requiredCompletedLevel = 13;

	private static float requiredValidDurationSeconds = 900f;

	private static int maxPopupCount = 4;

	private static int maxCloseCount = 4;

	private static int[] closeIntervals = new int[3] { 2, 3, 4 };

	private static int maxLowStarSubmitCount = 2;

	private static int lowStarInterval = 4;

	private static int gmActiveDaysOverride = 0;

	public static int HighStarMin
	{
		get
		{
			return highStarMin;
		}
		set
		{
			highStarMin = Mathf.Clamp(value, 1, 5);
		}
	}

	public static int RequiredCompletedLevel
	{
		get
		{
			return requiredCompletedLevel;
		}
		set
		{
			requiredCompletedLevel = Mathf.Max(0, value);
		}
	}

	public static float RequiredValidDurationSeconds
	{
		get
		{
			return requiredValidDurationSeconds;
		}
		set
		{
			requiredValidDurationSeconds = Mathf.Max(0f, value);
		}
	}

	public static int MaxPopupCount
	{
		get
		{
			return maxPopupCount;
		}
		set
		{
			maxPopupCount = Mathf.Max(1, value);
		}
	}

	public static int MaxCloseCount
	{
		get
		{
			return maxCloseCount;
		}
		set
		{
			maxCloseCount = Mathf.Max(1, value);
		}
	}

	public static int[] CloseIntervals
	{
		get
		{
			return closeIntervals;
		}
		set
		{
			closeIntervals = value ?? new int[0];
		}
	}

	public static string CloseIntervalsText => string.Join("|", closeIntervals);

	public static int MaxLowStarSubmitCount
	{
		get
		{
			return maxLowStarSubmitCount;
		}
		set
		{
			maxLowStarSubmitCount = Mathf.Max(1, value);
		}
	}

	public static int LowStarInterval
	{
		get
		{
			return lowStarInterval;
		}
		set
		{
			lowStarInterval = Mathf.Max(0, value);
		}
	}

	public static int GmActiveDaysOverride
	{
		get
		{
			return gmActiveDaysOverride;
		}
		set
		{
			gmActiveDaysOverride = Mathf.Max(0, value);
		}
	}

	public static int GetCloseInterval(int closeIndex)
	{
		if (closeIndex < 0 || closeIndex >= closeIntervals.Length)
		{
			return -1;
		}
		return closeIntervals[closeIndex];
	}

	public static void ParseCloseIntervalsText(string text)
	{
		if (string.IsNullOrWhiteSpace(text))
		{
			closeIntervals = new int[0];
			return;
		}
		string[] array = text.Split('|');
		List<int> list = new List<int>(array.Length);
		string[] array2 = array;
		for (int i = 0; i < array2.Length; i++)
		{
			if (int.TryParse(array2[i].Trim(), out var result))
			{
				list.Add(Mathf.Max(0, result));
			}
		}
		closeIntervals = list.ToArray();
	}

	public static void ResetRules()
	{
		highStarMin = 4;
		requiredCompletedLevel = 13;
		requiredValidDurationSeconds = 900f;
		maxPopupCount = 4;
		maxCloseCount = 4;
		closeIntervals = new int[3] { 2, 3, 4 };
		maxLowStarSubmitCount = 2;
		lowStarInterval = 4;
		gmActiveDaysOverride = 0;
	}
}
