using RotateRings.Ad;
using RotateRings.Model;

namespace RotateRings.GamePlay.PartStyle;

public static class PartStyleExp14
{
	private const int MainPrefixEndLevel = 20;

	private const int Unset = -1;

	private static int s_coldLaunchMainLevel = -1;

	private static int s_archivedSessionStyleOnLastSync = -1;

	public static PartStyleType Resolve(in PartStyleResolveContext context)
	{
		GameBordPartStyleModelData gameBordPartStyleSave = PartStyleExpCommon.GetGameBordPartStyleSave();
		if (gameBordPartStyleSave != null)
		{
			SyncRestartSessionIfNeeded(gameBordPartStyleSave);
			ApplyLevelStyleLockOnNewLevelEnter(gameBordPartStyleSave, in context);
		}
		if (IsInBrightMetalPrefix(in context))
		{
			return ReturnWithLevelStyleLock(in context, gameBordPartStyleSave, PartStyleType.GlossyMetal);
		}
		if (gameBordPartStyleSave == null)
		{
			return PartStyleType.GlossyMetal;
		}
		if (context.IsNewGame && gameBordPartStyleSave.Exp6PendingRoll)
		{
			gameBordPartStyleSave.Exp6PendingRoll = false;
			return RollAndStoreSessionStyle(gameBordPartStyleSave, excludeActiveSession: true);
		}
		if (TryReadLevelStyleLock(gameBordPartStyleSave, in context, out var style))
		{
			EnsureSessionStyle(gameBordPartStyleSave, style);
			return ReturnWithLevelStyleLock(in context, gameBordPartStyleSave, style);
		}
		if (!context.IsNewGame)
		{
			if (PartStyleExpCommon.TryGetResumeStyle(in context, out var style2))
			{
				EnsureSessionStyle(gameBordPartStyleSave, style2);
				return ReturnWithLevelStyleLock(in context, gameBordPartStyleSave, style2);
			}
			if (TryRestoreStyleFromLastRestartArchive(gameBordPartStyleSave, out var style3))
			{
				return ReturnWithLevelStyleLock(in context, gameBordPartStyleSave, style3);
			}
			return ReturnWithLevelStyleLock(in context, gameBordPartStyleSave, PartStyleType.GlossyMetal);
		}
		if (TryGetSessionStyle(gameBordPartStyleSave, out var style4))
		{
			return style4;
		}
		if (IsColdLaunchMainLevelPastPrefix())
		{
			return RollAndStoreSessionStyle(gameBordPartStyleSave, excludeActiveSession: false);
		}
		return PartStyleType.GlossyMetal;
	}

	private static bool IsInBrightMetalPrefix(in PartStyleResolveContext context)
	{
		return PartStyleExpCommon.GetGlobalProgressionLevel(in context) <= 20;
	}

	private static bool IsMainOrEndless(in PartStyleResolveContext context)
	{
		if (context.GameMode != GameMode.Main)
		{
			return context.GameMode == GameMode.Endless;
		}
		return true;
	}

	private static int GetGlobalProgressionLevel(in PartStyleResolveContext context)
	{
		return PartStyleExpCommon.GetGlobalProgressionLevel(in context);
	}

	private static bool TryReadLevelStyleLock(GameBordPartStyleModelData data, in PartStyleResolveContext context, out PartStyleType style)
	{
		if (!IsMainOrEndless(in context))
		{
			style = PartStyleType.GlossyMetal;
			return false;
		}
		return TryMatchLevelStyleLock(data, GetGlobalProgressionLevel(in context), out style);
	}

	private static bool TryMatchLevelStyleLock(GameBordPartStyleModelData data, int globalProgressionLevel, out PartStyleType style)
	{
		style = PartStyleType.GlossyMetal;
		if (data == null || data.Exp14LockedLevel != globalProgressionLevel)
		{
			return false;
		}
		return PartStyleExpCommon.TryParseStyle(data.Exp14LockedPartStyle, out style);
	}

	private static void WriteLevelStyleLock(GameBordPartStyleModelData data, in PartStyleResolveContext context, PartStyleType style)
	{
		if (IsMainOrEndless(in context))
		{
			data.Exp14LockedLevel = GetGlobalProgressionLevel(in context);
			data.Exp14LockedPartStyle = (int)style;
		}
	}

	private static void ClearLevelStyleLock(GameBordPartStyleModelData data)
	{
		data.Exp14LockedLevel = -1;
		data.Exp14LockedPartStyle = -1;
	}

	private static void ApplyLevelStyleLockOnNewLevelEnter(GameBordPartStyleModelData data, in PartStyleResolveContext context)
	{
		if (!context.IsNewGame || !IsMainOrEndless(in context) || data.Exp14LockedLevel == -1)
		{
			return;
		}
		int globalProgressionLevel = GetGlobalProgressionLevel(in context);
		if (data.Exp14LockedLevel != globalProgressionLevel)
		{
			if (data.Exp14LockedLevel > 20 && TryMatchLevelStyleLock(data, data.Exp14LockedLevel, out var _))
			{
				data.Exp6PendingRoll = true;
			}
			ClearLevelStyleLock(data);
		}
	}

	private static bool ShouldWriteLevelStyleLock(in PartStyleResolveContext context, GameBordPartStyleModelData data)
	{
		if (!IsMainOrEndless(in context))
		{
			return false;
		}
		if (!context.IsNewGame)
		{
			return true;
		}
		PartStyleType style;
		return TryReadLevelStyleLock(data, in context, out style);
	}

	private static PartStyleType ReturnWithLevelStyleLock(in PartStyleResolveContext context, GameBordPartStyleModelData data, PartStyleType style)
	{
		if (data != null && ShouldWriteLevelStyleLock(in context, data))
		{
			WriteLevelStyleLock(data, in context, style);
		}
		return style;
	}

	private static void EnsureSessionStyle(GameBordPartStyleModelData data, PartStyleType style)
	{
		if (!TryGetSessionStyle(data, out var _))
		{
			data.Exp6SessionStyle = (int)style;
		}
	}

	private static bool TryRestoreStyleFromLastRestartArchive(GameBordPartStyleModelData data, out PartStyleType style)
	{
		if (s_archivedSessionStyleOnLastSync < 0 || data.Exp6LastRestartStyle != s_archivedSessionStyleOnLastSync || !PartStyleExpCommon.TryParseStyle(data.Exp6LastRestartStyle, out style))
		{
			style = PartStyleType.GlossyMetal;
			return false;
		}
		EnsureSessionStyle(data, style);
		return true;
	}

	private static bool IsColdLaunchMainLevelPastPrefix()
	{
		return s_coldLaunchMainLevel > 20;
	}

	private static void SyncRestartSessionIfNeeded(GameBordPartStyleModelData data)
	{
		int coldLaunchCount = AdBridgeHelper.GetColdLaunchCount();
		if (coldLaunchCount != data.Exp6BoundLaunchId)
		{
			s_archivedSessionStyleOnLastSync = -1;
			if (PartStyleExpCommon.TryParseStyle(data.Exp6SessionStyle, out var style))
			{
				data.Exp6LastRestartStyle = (int)style;
				s_archivedSessionStyleOnLastSync = (int)style;
			}
			data.Exp6SessionStyle = -1;
			data.Exp6BoundLaunchId = coldLaunchCount;
			data.Exp6PendingRoll = false;
			s_coldLaunchMainLevel = GameApp.Proxy.Get<LevelModelProxy>()?.MainLevel ?? 0;
		}
	}

	private static bool TryGetSessionStyle(GameBordPartStyleModelData data, out PartStyleType style)
	{
		return PartStyleExpCommon.TryParseStyle(data.Exp6SessionStyle, out style);
	}

	private static PartStyleType RollAndStoreSessionStyle(GameBordPartStyleModelData data, bool excludeActiveSession)
	{
		PartStyleType exclude = PartStyleType.GlossyMetal;
		PartStyleType style2;
		if (excludeActiveSession && TryGetSessionStyle(data, out var style))
		{
			exclude = style;
		}
		else if (PartStyleExpCommon.TryParseStyle(data.Exp6LastRestartStyle, out style2))
		{
			exclude = style2;
		}
		return (PartStyleType)(data.Exp6SessionStyle = (int)PartStyleExpCommon.PickRandomExcluding(exclude));
	}
}
