using System.Collections.Generic;
using Hot.Model;
using RotateRings.Halloween;
using RotateRings.Model;

namespace RotateRings.SpecialLevel;

public static class SpecialLevelModule
{
	private const string Tag = "[SpecialLevelModule] ";

	private static readonly List<SpecialLevelService> services = new List<SpecialLevelService>();

	private static bool isInitialized;

	public static void Initialize()
	{
		if (!isInitialized)
		{
			isInitialized = true;
			Register(new HalloweenLevelService());
		}
	}

	public static void Register(SpecialLevelService service)
	{
		if (service != null && !services.Contains(service))
		{
			services.Add(service);
		}
	}

	public static bool IsSpecialLevelMode()
	{
		return FindActiveService() != null;
	}

	public static bool IsSpecialLevelModeFor(GameMode gameMode)
	{
		Initialize();
		for (int i = 0; i < services.Count; i++)
		{
			if (services[i].IsModeMatched(gameMode))
			{
				return true;
			}
		}
		return false;
	}

	public static int ResolveEnterLevel(int requestedLevel)
	{
		Initialize();
		int result = requestedLevel;
		for (int i = 0; i < services.Count; i++)
		{
			SpecialLevelService specialLevelService = services[i];
			int num = specialLevelService.ResolveEnterLevel(requestedLevel);
			if (specialLevelService.IsSpecialLevelMode())
			{
				return num;
			}
			if (num != requestedLevel)
			{
				result = num;
			}
		}
		return result;
	}

	public static void CommitInsertOnLevelLoaded()
	{
		Initialize();
		for (int i = 0; i < services.Count; i++)
		{
			services[i].CommitInsertOnLevelLoaded();
		}
	}

	public static void DropExpiredSessionOnStartup()
	{
		Initialize();
		for (int i = 0; i < services.Count; i++)
		{
			services[i].DropExpiredSessionOnStartup();
		}
	}

	public static void RefreshNextLevelGameModeAfterVictory(int nextBaseLevel)
	{
		Initialize();
		GameMode currentGameMode = GameInfo.CurrentGameMode;
		if (currentGameMode != GameMode.Main && currentGameMode != GameMode.Endless)
		{
			return;
		}
		for (int i = 0; i < services.Count; i++)
		{
			if (services[i].TryApplyEligibleInsertGameMode(nextBaseLevel))
			{
				GameApp.Proxy.Get<LevelModelProxy>()?.SyncGameInfo();
				break;
			}
		}
	}

	public static void ApplyActiveUncompletedSessionGameMode()
	{
		Initialize();
		SpecialLevelProxy specialLevelProxy = GameApp.Proxy.Get<SpecialLevelProxy>();
		if (specialLevelProxy != null)
		{
			for (int i = 0; i < services.Count && !services[i].TryApplyActiveSessionGameMode(specialLevelProxy); i++)
			{
			}
		}
	}

	public static void MarkCompletedAndRestoreBaseMode()
	{
		SpecialLevelService specialLevelService = FindActiveService();
		if (specialLevelService != null)
		{
			specialLevelService.MarkCompleted();
			specialLevelService.RestoreBaseMode(endSession: true);
		}
	}

	public static int ResolveCurrentLevelResourceId()
	{
		SpecialLevelService specialLevelService = FindActiveService();
		if (specialLevelService == null)
		{
			return 0;
		}
		int activityDayIndex = specialLevelService.ResolveActivityDayIndex();
		return specialLevelService.ResolveLevelResourceId(activityDayIndex);
	}

	public static bool TryGetActiveBanner(out int styleId, out string langKey)
	{
		SpecialLevelService specialLevelService = FindActiveService();
		if (specialLevelService == null)
		{
			styleId = 0;
			langKey = string.Empty;
			return false;
		}
		styleId = specialLevelService.BannerStyleId;
		langKey = specialLevelService.BannerLangKey;
		return true;
	}

	public static bool TryGetDisplayName(GameMode gameMode, out string displayName)
	{
		Initialize();
		for (int i = 0; i < services.Count; i++)
		{
			if (services[i].IsModeMatched(gameMode))
			{
				displayName = services[i].DisplayName;
				return true;
			}
		}
		displayName = string.Empty;
		return false;
	}

	private static SpecialLevelService FindActiveService()
	{
		Initialize();
		for (int i = 0; i < services.Count; i++)
		{
			if (services[i].IsSpecialLevelMode())
			{
				return services[i];
			}
		}
		return null;
	}
}
