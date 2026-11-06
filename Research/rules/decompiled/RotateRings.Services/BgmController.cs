using RotateRings.Common;
using RotateRings.Halloween;
using RotateRings.UI.Setting;
using XingYun.Experiment;

namespace RotateRings.Services;

public static class BgmController
{
	private const int BgmStrategySwap = 1;

	private const int BgmStrategyNewLevel = 2;

	private const int BgmStrategySwapNew = 3;

	private const int BgmStrategyDefault = 0;

	private static readonly string[] AllBgmEventIds = new string[5]
	{
		AudioKeys.BgmMainTheme.Value,
		AudioKeys.BgmLevel.Value,
		AudioKeys.BgmLevelNew.Value,
		AudioKeys.BgmMainThemeHalloween.Value,
		AudioKeys.BgmLevelHalloween.Value
	};

	private static string MainThemeEventId
	{
		get
		{
			if (IsHalloweenBgmEnabled())
			{
				return AudioKeys.BgmMainThemeHalloween.Value;
			}
			int currentStrategy = CurrentStrategy;
			if (currentStrategy != 1 && currentStrategy != 3)
			{
				return AudioKeys.BgmMainTheme.Value;
			}
			return AudioKeys.BgmLevel.Value;
		}
	}

	private static string LevelBgmEventId
	{
		get
		{
			if (IsHalloweenBgmEnabled())
			{
				return AudioKeys.BgmLevelHalloween.Value;
			}
			switch (CurrentStrategy)
			{
			case 1:
				return AudioKeys.BgmMainTheme.Value;
			case 2:
			case 3:
				return AudioKeys.BgmLevelNew.Value;
			default:
				return AudioKeys.BgmLevel.Value;
			}
		}
	}

	private static int CurrentStrategy => FunctionDataCenter.Get<AudioFunction>()?.BgmStrategy ?? 0;

	private static bool IsHalloweenBgmEnabled()
	{
		return HalloweenFeatureService.IsBgmEnabled();
	}

	public static string GetLevelBgmEventId()
	{
		return LevelBgmEventId;
	}

	public static void StopAllWsgBgm()
	{
		for (int i = 0; i < AllBgmEventIds.Length; i++)
		{
			AudioService.StopSound(AllBgmEventIds[i]);
		}
	}

	public static void EnterHome()
	{
		AudioExptInitCommand.SyncBgmAudioExperiments();
		if (IsMusicOn() && !AudioService.IsBgmSuppressedForAd)
		{
			StopAllExcept(MainThemeEventId);
			AudioService.PlayLoopIfNotPlaying(MainThemeEventId);
		}
	}

	public static void EnterLevel()
	{
		AudioExptInitCommand.SyncBgmAudioExperiments();
		if (IsMusicOn() && !AudioService.IsBgmSuppressedForAd)
		{
			StopAllWsgBgm();
			AudioService.PlayLoopIfNotPlaying(GetLevelBgmEventId());
		}
	}

	public static void RestartLevel()
	{
		AudioExptInitCommand.SyncBgmAudioExperiments();
		if (IsMusicOn() && IsGameOpen() && !AudioService.IsBgmSuppressedForAd)
		{
			AudioService.StopSound(LevelBgmEventId);
			AudioService.PlayLoopIfNotPlaying(GetLevelBgmEventId());
		}
	}

	public static void LeaveLevel()
	{
		AudioService.StopSound(LevelBgmEventId);
		if (IsHomeOpen())
		{
			EnterHome();
		}
	}

	public static void LeaveHome()
	{
		AudioService.StopSound(MainThemeEventId);
	}

	public static void RefreshAfterMusicToggle()
	{
		if (!IsMusicOn())
		{
			StopAllWsgBgm();
		}
		else if (IsGameOpen())
		{
			EnterLevel();
		}
		else if (IsHomeOpen())
		{
			EnterHome();
		}
	}

	public static void RefreshAfterBgmStyleIdChanged()
	{
		RefreshAfterMusicToggle();
	}

	private static bool IsMusicOn()
	{
		return GameApp.Proxy.Get<SettingProxy>().Data.IsMusicOn;
	}

	private static void StopAllExcept(string keepEventId)
	{
		for (int i = 0; i < AllBgmEventIds.Length; i++)
		{
			if (AllBgmEventIds[i] != keepEventId)
			{
				AudioService.StopSound(AllBgmEventIds[i]);
			}
		}
	}

	private static bool IsHomeOpen()
	{
		return GameApp.UI.IsOpen("UIHome");
	}

	private static bool IsGameOpen()
	{
		return GameApp.UI.IsOpen("UIGame");
	}
}
