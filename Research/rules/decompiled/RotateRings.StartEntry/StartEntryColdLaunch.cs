using System;
using RotateRings.GamePlay.Save;
using RotateRings.Model;
using Rotaterings.Rank;
using XingYun.Experiment;

namespace RotateRings.StartEntry;

public static class StartEntryColdLaunch
{
	private enum Route
	{
		LevelPathFirst,
		HomeFirst
	}

	private enum Target
	{
		Home,
		EnterLevel,
		HomeThenAutoEnterLevel
	}

	private enum LaunchStep
	{
		EnterHome,
		EnterLevelResume,
		EnterLevelNewGame
	}

	private readonly struct SchemeRow
	{
		public readonly Route Route;

		public readonly StartEntryCdKind CdKind;

		public readonly Target FreshTarget;

		public readonly Target PuzzleInCdTarget;

		public readonly Target PuzzleOutCdTarget;

		public readonly bool PuzzleOutCdRestartFirst;

		public SchemeRow(Route route, StartEntryCdKind cdKind, Target freshTarget, Target puzzleInCdTarget, Target puzzleOutCdTarget, bool puzzleOutCdRestartFirst)
		{
			Route = route;
			CdKind = cdKind;
			FreshTarget = freshTarget;
			PuzzleInCdTarget = puzzleInCdTarget;
			PuzzleOutCdTarget = puzzleOutCdTarget;
			PuzzleOutCdRestartFirst = puzzleOutCdRestartFirst;
		}
	}

	private sealed class ResolvedPlan
	{
		public int EntryType;

		public LaunchStep InitialStep;

		public StartEntryHomeLaunchParam HomeFollowUp;
	}

	private const string Tag = "[StartEntry]";

	public const float DefaultHomeAutoEnterDelaySec = 0f;

	private static readonly SchemeRow[] SchemeRows = new SchemeRow[9]
	{
		new SchemeRow(Route.LevelPathFirst, StartEntryCdKind.None, Target.Home, Target.Home, Target.Home, puzzleOutCdRestartFirst: false),
		new SchemeRow(Route.LevelPathFirst, StartEntryCdKind.CalendarDay, Target.Home, Target.EnterLevel, Target.Home, puzzleOutCdRestartFirst: false),
		new SchemeRow(Route.LevelPathFirst, StartEntryCdKind.Hours24, Target.Home, Target.EnterLevel, Target.Home, puzzleOutCdRestartFirst: false),
		new SchemeRow(Route.LevelPathFirst, StartEntryCdKind.Hours36, Target.Home, Target.EnterLevel, Target.Home, puzzleOutCdRestartFirst: false),
		new SchemeRow(Route.LevelPathFirst, StartEntryCdKind.None, Target.EnterLevel, Target.EnterLevel, Target.EnterLevel, puzzleOutCdRestartFirst: true),
		new SchemeRow(Route.HomeFirst, StartEntryCdKind.None, Target.Home, Target.Home, Target.Home, puzzleOutCdRestartFirst: true),
		new SchemeRow(Route.LevelPathFirst, StartEntryCdKind.Hours36, Target.EnterLevel, Target.EnterLevel, Target.EnterLevel, puzzleOutCdRestartFirst: true),
		new SchemeRow(Route.LevelPathFirst, StartEntryCdKind.Hours36, Target.Home, Target.EnterLevel, Target.Home, puzzleOutCdRestartFirst: true),
		new SchemeRow(Route.HomeFirst, StartEntryCdKind.None, Target.HomeThenAutoEnterLevel, Target.HomeThenAutoEnterLevel, Target.HomeThenAutoEnterLevel, puzzleOutCdRestartFirst: true)
	};

	public static void RunAfterPrivacyGate(Func<bool> hasCompletedRequiredLevel, Action<StartEntryHomeLaunchParam> enterHome, Action enterLevelScene)
	{
		if (!hasCompletedRequiredLevel())
		{
			enterLevelScene();
			return;
		}
		int level = GameApp.Proxy.Get<LevelModelProxy>().Level;
		ResolvedPlan resolvedPlan = Resolve(GameApp.Proxy.Get<RingLevelSaveProxy>().HasValidSave(level));
		switch (resolvedPlan.InitialStep)
		{
		case LaunchStep.EnterHome:
			enterHome(resolvedPlan.HomeFollowUp);
			break;
		case LaunchStep.EnterLevelResume:
			enterLevelScene();
			break;
		case LaunchStep.EnterLevelNewGame:
			ClearPuzzleSaveForRestart();
			enterLevelScene();
			break;
		}
	}

	public static void ApplyOnHomeRegister(StartEntryHomeLaunchParam param)
	{
		if (param != null && param.RestartLevelOnRegister)
		{
			ClearPuzzleSaveForRestart();
		}
	}

	public static void RecordLevelExitNow()
	{
		PlayerModelProxy playerModelProxy = GameApp.Proxy.Get<PlayerModelProxy>();
		if (playerModelProxy?.Data != null)
		{
			playerModelProxy.Data.LastLevelExitUtcSeconds = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
		}
	}

	public static void ClearLevelExitRecord()
	{
		PlayerModelProxy playerModelProxy = GameApp.Proxy.Get<PlayerModelProxy>();
		if (playerModelProxy?.Data != null && playerModelProxy.Data.LastLevelExitUtcSeconds != 0L)
		{
			playerModelProxy.Data.LastLevelExitUtcSeconds = 0L;
		}
	}

	public static StartEntryCdKind GetCdKindForEntryType(int entryType)
	{
		if (entryType < 0 || entryType > 8)
		{
			entryType = 0;
		}
		return GetSchemeRow(entryType).CdKind;
	}

	public static bool IsInCdForEntryType(int entryType)
	{
		return IsInCd(GetCdKindForEntryType(entryType));
	}

	public static string DescribeCdKind(StartEntryCdKind kind)
	{
		return kind switch
		{
			StartEntryCdKind.None => "无", 
			StartEntryCdKind.CalendarDay => "自然日", 
			StartEntryCdKind.Hours24 => "24小时", 
			StartEntryCdKind.Hours36 => "36小时", 
			_ => "未知", 
		};
	}

	private static ResolvedPlan Resolve(bool hasPuzzleSave)
	{
		int num = FunctionDataCenter.Get<StartEntryFunction>().EntryType;
		if (num < 0 || num > 8)
		{
			num = 0;
		}
		SchemeRow schemeRow = GetSchemeRow(num);
		ResolvedPlan resolvedPlan = new ResolvedPlan
		{
			EntryType = num
		};
		if (!hasPuzzleSave)
		{
			ApplyTarget(resolvedPlan, schemeRow.FreshTarget, schemeRow.Route == Route.HomeFirst);
			return resolvedPlan;
		}
		if (schemeRow.Route == Route.LevelPathFirst)
		{
			if (schemeRow.CdKind == StartEntryCdKind.None)
			{
				ApplyTarget(resolvedPlan, schemeRow.PuzzleOutCdTarget, routeHomeFirst: false, schemeRow.PuzzleOutCdRestartFirst);
				return resolvedPlan;
			}
			bool flag = IsInCd(schemeRow.CdKind);
			ApplyTarget(resolvedPlan, flag ? schemeRow.PuzzleInCdTarget : schemeRow.PuzzleOutCdTarget, routeHomeFirst: false, !flag && schemeRow.PuzzleOutCdRestartFirst);
			return resolvedPlan;
		}
		if (schemeRow.CdKind == StartEntryCdKind.None)
		{
			ApplyTarget(resolvedPlan, schemeRow.PuzzleOutCdTarget, routeHomeFirst: true, schemeRow.PuzzleOutCdRestartFirst);
			return resolvedPlan;
		}
		bool flag2 = IsInCd(schemeRow.CdKind);
		ApplyTarget(resolvedPlan, flag2 ? schemeRow.PuzzleInCdTarget : schemeRow.PuzzleOutCdTarget, routeHomeFirst: true, !flag2 && schemeRow.PuzzleOutCdRestartFirst);
		return resolvedPlan;
	}

	private static SchemeRow GetSchemeRow(int entryType)
	{
		if (entryType < 0 || entryType >= SchemeRows.Length)
		{
			return SchemeRows[0];
		}
		return SchemeRows[entryType];
	}

	private static bool IsInCd(StartEntryCdKind cdKind)
	{
		if (cdKind == StartEntryCdKind.None)
		{
			return false;
		}
		long valueOrDefault = (GameApp.Proxy.Get<PlayerModelProxy>()?.Data?.LastLevelExitUtcSeconds).GetValueOrDefault();
		if (valueOrDefault <= 0)
		{
			return false;
		}
		if (cdKind == StartEntryCdKind.CalendarDay)
		{
			return DateTimeOffset.FromUnixTimeSeconds(valueOrDefault).ToLocalTime().Date == DateTimeOffset.Now.Date;
		}
		if (cdKind <= StartEntryCdKind.None)
		{
			return false;
		}
		long num = DateTimeOffset.UtcNow.ToUnixTimeSeconds() - valueOrDefault;
		if (num >= 0)
		{
			return num <= (long)cdKind * 3600L;
		}
		return false;
	}

	private static void ApplyTarget(ResolvedPlan plan, Target target, bool routeHomeFirst, bool restartFirst = false)
	{
		switch (target)
		{
		case Target.Home:
			plan.InitialStep = LaunchStep.EnterHome;
			plan.HomeFollowUp = (restartFirst ? new StartEntryHomeLaunchParam
			{
				RestartLevelOnRegister = true
			} : null);
			break;
		case Target.EnterLevel:
			if (routeHomeFirst)
			{
				plan.InitialStep = LaunchStep.EnterHome;
				plan.HomeFollowUp = CreateHomeFollowUpWithAutoEnter(restartFirst);
			}
			else if (restartFirst)
			{
				plan.InitialStep = LaunchStep.EnterLevelNewGame;
			}
			else
			{
				plan.InitialStep = LaunchStep.EnterLevelResume;
			}
			break;
		case Target.HomeThenAutoEnterLevel:
			plan.InitialStep = LaunchStep.EnterHome;
			plan.HomeFollowUp = CreateHomeFollowUpWithAutoEnter(restartFirst);
			break;
		}
	}

	private static StartEntryHomeLaunchParam CreateHomeFollowUpWithAutoEnter(bool restartFirst)
	{
		return new StartEntryHomeLaunchParam
		{
			RestartLevelOnRegister = restartFirst,
			AutoEnterLevelAfterPopups = true
		};
	}

	private static void ClearPuzzleSaveForRestart()
	{
		GameApp.Proxy.Get<RingLevelSaveProxy>()?.ClearSave(writeToDisk: true);
		GameApp.Proxy.Get<LevelModelProxy>()?.TryLevelAgain();
		GameApp.Proxy.Get<RankModelProxy>()?.RestartGame();
	}
}
