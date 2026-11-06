using System;
using System.Globalization;
using UnityEngine;
using XingYun.Experiment;

namespace RotateRings.Halloween;

public static class HalloweenFeatureService
{
	public enum Feature
	{
		Loading,
		Home,
		Game,
		Settlement,
		Bgm,
		FirstWin,
		Rank,
		Level
	}

	public const int ActivityDayCycle = 7;

	private const string Tag = "[HalloweenFeatureService] ";

	private const string LoadingEnabledPrefsKey = "Halloween.LoadingEnabled";

	private const string ResourceAvailabilityLocation = "HalloweenGameBG";

	private const int NoOverride = -1;

	private const int ForceDisabled = 0;

	private const int ForceEnabled = 1;

	private static readonly int[] featureOverrides = new int[8] { -1, -1, -1, -1, -1, -1, -1, -1 };

	private static int totalOverride = -1;

	private static bool isInitialized;

	private static bool hasLoggedResourceUnavailable;

	private static HalloweenFunction config;

	private static DateTime startDate;

	private static DateTime endDate;

	private static bool isActivityRangeValid;

	public static bool IsLoadingEnabled()
	{
		return IsFeatureEnabled(Feature.Loading, GetConfig()?.EnableLoading ?? 0);
	}

	public static void SaveLoadingEnabledForNextLaunch()
	{
		PlayerPrefs.SetInt("Halloween.LoadingEnabled", IsLoadingEnabled() ? 1 : 0);
		PlayerPrefs.Save();
	}

	public static bool IsHomeEnabled()
	{
		return IsFeatureEnabled(Feature.Home, GetConfig()?.EnableHome ?? 0);
	}

	public static bool IsGameEnabled()
	{
		return IsFeatureEnabled(Feature.Game, GetConfig()?.EnableGame ?? 0);
	}

	public static bool IsSettlementEnabled()
	{
		return IsFeatureEnabled(Feature.Settlement, GetConfig()?.EnableSettlement ?? 0);
	}

	public static bool IsBgmEnabled()
	{
		return IsFeatureEnabled(Feature.Bgm, GetConfig()?.EnableBGM ?? 0);
	}

	public static bool IsFirstWinEnabled()
	{
		return IsFeatureEnabled(Feature.FirstWin, GetConfig()?.EnableFirstWin ?? 0);
	}

	public static bool IsRankEnabled()
	{
		return IsFeatureEnabled(Feature.Rank, GetConfig()?.EnableRank ?? 0);
	}

	public static bool IsLevelEnabled()
	{
		GetConfig();
		if (!IsHalloweenResourceAvailable())
		{
			return false;
		}
		if (totalOverride == 0)
		{
			return false;
		}
		if (featureOverrides[7] == 1)
		{
			return true;
		}
		return IsLevelDefaultEnabled();
	}

	public static bool IsLevelForceEnabled()
	{
		GetConfig();
		if (IsHalloweenResourceAvailable() && totalOverride != 0)
		{
			return featureOverrides[7] == 1;
		}
		return false;
	}

	private static bool IsLevelDefaultEnabled()
	{
		if (!IsWithinActivityPeriod())
		{
			return false;
		}
		if ((GetConfig()?.EnableLevel ?? 0) != 1)
		{
			return false;
		}
		return true;
	}

	public static bool TryGetActivityStartDate(out DateTime activityStartDate)
	{
		GetConfig();
		activityStartDate = startDate;
		return isActivityRangeValid;
	}

	public static bool IsWithinActivityPeriod()
	{
		GetConfig();
		if (!isActivityRangeValid)
		{
			return false;
		}
		DateTime now = DateTime.Now;
		if (now >= startDate)
		{
			return now < endDate;
		}
		return false;
	}

	public static string DescribeActivityRange()
	{
		GetConfig();
		if (!isActivityRangeValid)
		{
			return "invalid";
		}
		return $"{startDate:yyyy-MM-dd} ~ {endDate:yyyy-MM-dd}";
	}

	public static void SetTotalOverride(int overrideValue)
	{
		totalOverride = ((overrideValue == 1) ? 1 : 0);
		SaveLoadingEnabledForNextLaunch();
	}

	public static int GetTotalOverride()
	{
		if (totalOverride != -1)
		{
			return totalOverride;
		}
		return GetDefaultOverrideValue(Feature.Loading);
	}

	public static void SetFeatureOverride(Feature feature, int overrideValue)
	{
		int num = ((overrideValue == 1) ? 1 : 0);
		featureOverrides[(int)feature] = num;
		if (feature == Feature.Loading)
		{
			SaveLoadingEnabledForNextLaunch();
		}
	}

	public static int GetFeatureOverride(Feature feature)
	{
		int num = featureOverrides[(int)feature];
		if (num != -1)
		{
			return num;
		}
		return GetDefaultOverrideValue(feature);
	}

	public static void EnableAllOverrides()
	{
		SetTotalOverride(1);
		for (int i = 0; i < featureOverrides.Length; i++)
		{
			SetFeatureOverride((Feature)i, 1);
		}
	}

	public static void ClearAllOverrides()
	{
		totalOverride = -1;
		for (int i = 0; i < featureOverrides.Length; i++)
		{
			featureOverrides[i] = -1;
		}
		SaveLoadingEnabledForNextLaunch();
	}

	private static HalloweenFunction GetConfig()
	{
		Initialize();
		return config;
	}

	private static void Initialize()
	{
		if (!isInitialized)
		{
			isInitialized = true;
			config = FunctionDataCenter.Get<HalloweenFunction>();
			if (config != null)
			{
				isActivityRangeValid = TryParseActivityRange(config.StartDate, config.EndDate, out startDate, out endDate);
				_ = isActivityRangeValid;
			}
		}
	}

	private static int GetDefaultOverrideValue(Feature feature)
	{
		if (!IsFeatureEnabled(feature, GetFeatureConfigValue(feature)))
		{
			return 0;
		}
		return 1;
	}

	private static int GetFeatureConfigValue(Feature feature)
	{
		HalloweenFunction halloweenFunction = GetConfig();
		if (halloweenFunction == null)
		{
			return 0;
		}
		return feature switch
		{
			Feature.Loading => halloweenFunction.EnableLoading, 
			Feature.Home => halloweenFunction.EnableHome, 
			Feature.Game => halloweenFunction.EnableGame, 
			Feature.Settlement => halloweenFunction.EnableSettlement, 
			Feature.Bgm => halloweenFunction.EnableBGM, 
			Feature.FirstWin => halloweenFunction.EnableFirstWin, 
			Feature.Rank => halloweenFunction.EnableRank, 
			_ => 0, 
		};
	}

	private static bool IsHalloweenResourceAvailable()
	{
		int num;
		if ((Object)(object)GameApp.Res != (Object)null)
		{
			num = (GameApp.Res.CheckLocationValid("HalloweenGameBG") ? 1 : 0);
			if (num != 0)
			{
				goto IL_002f;
			}
		}
		else
		{
			num = 0;
		}
		if (!hasLoggedResourceUnavailable)
		{
			hasLoggedResourceUnavailable = true;
		}
		goto IL_002f;
		IL_002f:
		return (byte)num != 0;
	}

	private static bool IsFeatureEnabled(Feature feature, int featureValue)
	{
		if (!IsHalloweenResourceAvailable())
		{
			return false;
		}
		if (totalOverride == 0)
		{
			return false;
		}
		int num = featureOverrides[(int)feature];
		if (num != -1)
		{
			return num == 1;
		}
		if (featureValue != 1 || !isActivityRangeValid)
		{
			return false;
		}
		DateTime now = DateTime.Now;
		if (now >= startDate)
		{
			return now < endDate;
		}
		return false;
	}

	private static bool TryParseActivityRange(string startDateText, string endDateText, out DateTime parsedStartDate, out DateTime parsedEndDate)
	{
		bool num = TryParseDate(startDateText, out parsedStartDate);
		bool flag = TryParseDate(endDateText, out parsedEndDate);
		if (num && flag)
		{
			return parsedEndDate > parsedStartDate;
		}
		return false;
	}

	private static bool TryParseDate(string dateText, out DateTime parsedDate)
	{
		parsedDate = default(DateTime);
		try
		{
			if (string.IsNullOrEmpty(dateText))
			{
				return false;
			}
			string[] array = dateText.Split(',');
			if (array.Length != 3 || !int.TryParse(array[0], NumberStyles.None, CultureInfo.InvariantCulture, out var result) || !int.TryParse(array[1], NumberStyles.None, CultureInfo.InvariantCulture, out var result2) || !int.TryParse(array[2], NumberStyles.None, CultureInfo.InvariantCulture, out var result3))
			{
				return false;
			}
			parsedDate = new DateTime(result, result2, result3, 0, 0, 0, DateTimeKind.Local);
			return true;
		}
		catch (ArgumentOutOfRangeException)
		{
			return false;
		}
	}
}
