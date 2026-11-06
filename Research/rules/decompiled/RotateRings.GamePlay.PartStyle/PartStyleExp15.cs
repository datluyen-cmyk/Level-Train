using System;
using RotateRings.Ad;
using RotateRings.Model;
using XingYun.Experiment;
using XingYun.Utility;

namespace RotateRings.GamePlay.PartStyle;

public static class PartStyleExp15
{
	private const int Unset = -1;

	private const string TAG = "[PartStyle][Exp15] ";

	private static int lastColdLaunchId = -1;

	private static bool hasColdLaunchFirstEnter;

	public static PartStyleType Resolve(in PartStyleResolveContext context)
	{
		GameBordPartStyleModelData gameBordPartStyleSave = PartStyleExpCommon.GetGameBordPartStyleSave();
		EnsureLocalCalendarBaseline(gameBordPartStyleSave);
		int localCalendarYmd = GetLocalCalendarYmd();
		int lastLocalCalendarYmd = gameBordPartStyleSave?.Exp15LastLocalCalendarYmd ?? 0;
		bool flag = IsNewNaturalDayCrossing(localCalendarYmd, lastLocalCalendarYmd);
		bool flag2 = IsEnteringNextLevelAfterNewDay(in context, gameBordPartStyleSave, flag, localCalendarYmd);
		int localNaturalDayIndex = GetLocalNaturalDayIndex(gameBordPartStyleSave);
		if (AdBridgeHelper.IsFirstInstallDay() && !flag2 && localNaturalDayIndex <= 1)
		{
			if (!PartStyleExpCommon.TryParseStyle(gameBordPartStyleSave?.Exp7DayStyle ?? (-1), out var _))
			{
				SetNaturalDayStyles(PartStyleType.GlossyMetal, PartStyleType.GlossyMetal);
			}
			return ReturnResolved("FirstInstallDay", ReturnWithPersistentLevelLock(in context, gameBordPartStyleSave, PartStyleType.GlossyMetal, localCalendarYmd), gameBordPartStyleSave);
		}
		bool flag3 = TryTakeColdLaunchFirstEnter();
		bool flag4 = flag && flag3;
		if (flag4)
		{
			if (!context.IsNewGame)
			{
				PartStyleResumeStore.ClearActiveResume();
			}
			ClearPersistentLevelLock(gameBordPartStyleSave);
		}
		if (gameBordPartStyleSave != null)
		{
			ApplyPersistentLevelLockOnNewLevelEnter(gameBordPartStyleSave, in context);
		}
		if (flag2)
		{
			ClearPersistentLevelLock(gameBordPartStyleSave);
		}
		if (TryReadPersistentLevelLock(gameBordPartStyleSave, in context, localCalendarYmd, out var style2))
		{
			return ReturnResolved("PersistentLevelLock", ReturnWithPersistentLevelLock(in context, gameBordPartStyleSave, style2, localCalendarYmd), gameBordPartStyleSave);
		}
		if (context.IsNewGame && flag && !flag4 && !flag2 && TryRestoreSameLevelStyleAfterCrossDayWithoutColdLaunch(in context, out var style3))
		{
			return ReturnResolved("CrossDaySameLevelNoColdLaunch", ReturnWithPersistentLevelLock(in context, gameBordPartStyleSave, style3, localCalendarYmd), gameBordPartStyleSave);
		}
		if (!flag4 && PartStyleExpCommon.TryGetResumeStyle(in context, out var style4))
		{
			return ReturnResolved("ResumeStyle", ReturnWithPersistentLevelLock(in context, gameBordPartStyleSave, style4, localCalendarYmd), gameBordPartStyleSave);
		}
		PartStyleType style5 = EnsureCurrentNaturalDayStyle(flag || flag4 || context.IsNewGame || flag2, flag2);
		return ReturnResolved("NaturalDayStyle", ReturnWithPersistentLevelLock(in context, gameBordPartStyleSave, style5, localCalendarYmd), gameBordPartStyleSave);
	}

	private static bool IsEnteringNextLevelAfterNewDay(in PartStyleResolveContext context, GameBordPartStyleModelData data, bool isNewNaturalDay, int localCalendarYmd)
	{
		if (data == null || !isNewNaturalDay || !context.IsNewGame || !IsMainOrEndless(in context))
		{
			return false;
		}
		int globalProgressionLevel = GetGlobalProgressionLevel(in context);
		if (data.Exp15LockedGlobalLevel != -1 && data.Exp15LockedGlobalLevel != globalProgressionLevel)
		{
			return true;
		}
		if (HasProgressedBeyondResumeSlot(in context, data))
		{
			return true;
		}
		return false;
	}

	private static bool HasProgressedBeyondResumeSlot(in PartStyleResolveContext context, GameBordPartStyleModelData data)
	{
		if (data.ActiveResumeLevel == -1 || data.ActiveResumeLevel == context.Level)
		{
			return false;
		}
		PartStyleType style;
		return PartStyleExpCommon.TryParseStyle(data.ActiveResumePartStyle, out style);
	}

	private static PartStyleType EnsureCurrentNaturalDayStyle(bool mayEstablishTodayStyle, bool forceEstablishTodayStyle = false)
	{
		GameBordPartStyleModelData gameBordPartStyleSave = PartStyleExpCommon.GetGameBordPartStyleSave();
		int currentNaturalDayStyleRaw = GetCurrentNaturalDayStyleRaw();
		int localCalendarYmd = GetLocalCalendarYmd();
		int lastLocalCalendarYmd = gameBordPartStyleSave?.Exp15LastLocalCalendarYmd ?? 0;
		bool flag = !IsNewNaturalDayCrossing(localCalendarYmd, lastLocalCalendarYmd);
		if (!forceEstablishTodayStyle && flag && PartStyleExpCommon.TryParseStyle(currentNaturalDayStyleRaw, out var style))
		{
			return style;
		}
		if (!mayEstablishTodayStyle && !forceEstablishTodayStyle)
		{
			if (!PartStyleExpCommon.TryParseStyle(currentNaturalDayStyleRaw, out var style2))
			{
				return PartStyleType.GlossyMetal;
			}
			return style2;
		}
		PartStyleType partStyleType = PartStyleType.GlossyMetal;
		if (IsNewNaturalDayCrossing(localCalendarYmd, lastLocalCalendarYmd) && PartStyleExpCommon.TryParseStyle(currentNaturalDayStyleRaw, out var style3))
		{
			partStyleType = style3;
		}
		PartStyleType num = PartStyleExpCommon.PickRandomExcluding(partStyleType);
		SetNaturalDayStyles(num, partStyleType);
		return num;
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

	private static bool TryReadPersistentLevelLock(GameBordPartStyleModelData data, in PartStyleResolveContext context, int localCalendarYmd, out PartStyleType style)
	{
		style = PartStyleType.GlossyMetal;
		if (data == null || !IsMainOrEndless(in context) || data.Exp15LockedGlobalLevel != GetGlobalProgressionLevel(in context) || !PartStyleExpCommon.TryParseStyle(data.Exp15LockedPartStyle, out style))
		{
			style = PartStyleType.GlossyMetal;
			return false;
		}
		int exp15LockedDayYmd = data.Exp15LockedDayYmd;
		if (exp15LockedDayYmd <= 0 || !TryParseLocalCalendarYmd(exp15LockedDayYmd, out var date))
		{
			return false;
		}
		if (!TryParseLocalCalendarYmd(localCalendarYmd, out date))
		{
			return false;
		}
		if (localCalendarYmd <= exp15LockedDayYmd)
		{
			return true;
		}
		if (HasProgressedBeyondResumeSlot(in context, data))
		{
			return false;
		}
		return true;
	}

	private static void WritePersistentLevelLock(GameBordPartStyleModelData data, in PartStyleResolveContext context, PartStyleType style, int localCalendarDayYmd)
	{
		if (data != null && IsMainOrEndless(in context))
		{
			data.Exp15LockedGlobalLevel = GetGlobalProgressionLevel(in context);
			data.Exp15LockedPartStyle = (int)style;
			if (localCalendarDayYmd > 0 && TryParseLocalCalendarYmd(localCalendarDayYmd, out var _))
			{
				data.Exp15LockedDayYmd = localCalendarDayYmd;
			}
		}
	}

	private static void ClearPersistentLevelLock(GameBordPartStyleModelData data)
	{
		if (data != null)
		{
			data.Exp15LockedGlobalLevel = -1;
			data.Exp15LockedPartStyle = -1;
			data.Exp15LockedDayYmd = 0;
		}
	}

	private static void ApplyPersistentLevelLockOnNewLevelEnter(GameBordPartStyleModelData data, in PartStyleResolveContext context)
	{
		if (context.IsNewGame && IsMainOrEndless(in context) && data.Exp15LockedGlobalLevel != -1 && data.Exp15LockedGlobalLevel != GetGlobalProgressionLevel(in context))
		{
			ClearPersistentLevelLock(data);
		}
	}

	private static bool ShouldWritePersistentLevelLock(in PartStyleResolveContext context, GameBordPartStyleModelData data)
	{
		return IsMainOrEndless(in context);
	}

	private static bool TryRestoreSameLevelStyleAfterCrossDayWithoutColdLaunch(in PartStyleResolveContext context, out PartStyleType style)
	{
		style = PartStyleType.GlossyMetal;
		if (!IsMainOrEndless(in context))
		{
			return false;
		}
		if (PartStyleResumeStore.TryGetMatchingLevelStyle(context.GameMode, context.Level, out var styleRaw) && PartStyleExpCommon.TryParseStyle(styleRaw, out style))
		{
			return true;
		}
		GameBordPartStyleModelData gameBordPartStyleSave = PartStyleExpCommon.GetGameBordPartStyleSave();
		if (gameBordPartStyleSave != null && PartStyleExpCommon.TryParseStyle(gameBordPartStyleSave.Exp7DayStyle, out style))
		{
			return true;
		}
		return false;
	}

	private static PartStyleType ReturnWithPersistentLevelLock(in PartStyleResolveContext context, GameBordPartStyleModelData data, PartStyleType style, int localCalendarDayYmd)
	{
		if (data != null && ShouldWritePersistentLevelLock(in context, data))
		{
			int localCalendarDayYmd2 = ((localCalendarDayYmd > 0) ? localCalendarDayYmd : GetLocalCalendarYmd());
			WritePersistentLevelLock(data, in context, style, localCalendarDayYmd2);
		}
		return style;
	}

	private static bool TryTakeColdLaunchFirstEnter()
	{
		int coldLaunchCount = AdBridgeHelper.GetColdLaunchCount();
		if (coldLaunchCount != lastColdLaunchId)
		{
			lastColdLaunchId = coldLaunchCount;
			hasColdLaunchFirstEnter = true;
		}
		if (!hasColdLaunchFirstEnter)
		{
			return false;
		}
		hasColdLaunchFirstEnter = false;
		return true;
	}

	private static int GetCurrentNaturalDayStyleRaw()
	{
		return PartStyleExpCommon.GetGameBordPartStyleSave()?.Exp7DayStyle ?? (-1);
	}

	private static void SetNaturalDayStyles(PartStyleType currentStyle, PartStyleType previousDayStyle)
	{
		GameBordPartStyleModelData gameBordPartStyleSave = PartStyleExpCommon.GetGameBordPartStyleSave();
		if (gameBordPartStyleSave != null)
		{
			gameBordPartStyleSave.Exp7DayStyle = (int)currentStyle;
			gameBordPartStyleSave.Exp7PrevDayStyle = (int)previousDayStyle;
			int localCalendarYmd = GetLocalCalendarYmd();
			if (gameBordPartStyleSave.Exp15LastLocalCalendarYmd > 0 && !TryParseLocalCalendarYmd(gameBordPartStyleSave.Exp15LastLocalCalendarYmd, out var _))
			{
				gameBordPartStyleSave.Exp15LastLocalCalendarYmd = 0;
			}
			AdvanceExp15LocalNaturalDayIndexOnForwardCross(gameBordPartStyleSave, localCalendarYmd);
			if (gameBordPartStyleSave.Exp15LastLocalCalendarYmd <= 0 || localCalendarYmd >= gameBordPartStyleSave.Exp15LastLocalCalendarYmd)
			{
				gameBordPartStyleSave.Exp15LastLocalCalendarYmd = localCalendarYmd;
			}
		}
	}

	private static int GetLocalCalendarYmd()
	{
		DateTime now = DateTime.Now;
		return now.Year * 10000 + now.Month * 100 + now.Day;
	}

	private static void EnsureLocalCalendarBaseline(GameBordPartStyleModelData data)
	{
		if (data != null)
		{
			if (data.Exp15LastLocalCalendarYmd > 0 && !TryParseLocalCalendarYmd(data.Exp15LastLocalCalendarYmd, out var _))
			{
				data.Exp15LastLocalCalendarYmd = 0;
			}
			if (data.Exp15LastLocalCalendarYmd > 0)
			{
				EnsureExp15LocalNaturalDayIndexInitialized(data);
				return;
			}
			data.Exp15LastLocalCalendarYmd = GetLocalCalendarYmd();
			EnsureExp15LocalNaturalDayIndexInitialized(data);
		}
	}

	private static bool IsNewNaturalDayCrossing(int localCalendarYmd, int lastLocalCalendarYmd)
	{
		if (!TryParseLocalCalendarYmd(localCalendarYmd, out var date) || !TryParseLocalCalendarYmd(lastLocalCalendarYmd, out date))
		{
			return false;
		}
		if (lastLocalCalendarYmd > 0)
		{
			return localCalendarYmd > lastLocalCalendarYmd;
		}
		return false;
	}

	private static int GetFirstLoginLocalCalendarYmd()
	{
		PlayerProxy obj = GameApp.Proxy.Get<PlayerProxy>();
		long num = ((obj != null) ? obj.FirstLoginTime : 0);
		if (num <= 0)
		{
			return 0;
		}
		DateTime dateTime = Time.TimeStamp2DateTime(num);
		int num2 = dateTime.Year * 10000 + dateTime.Month * 100 + dateTime.Day;
		if (!TryParseLocalCalendarYmd(num2, out var _))
		{
			return 0;
		}
		return num2;
	}

	private static int GetLocalNaturalDayIndex(GameBordPartStyleModelData data)
	{
		if (data == null)
		{
			return 1;
		}
		EnsureExp15LocalNaturalDayIndexInitialized(data);
		if (data.Exp15LocalNaturalDayIndex <= 0)
		{
			return 1;
		}
		return data.Exp15LocalNaturalDayIndex;
	}

	private static void EnsureExp15LocalNaturalDayIndexInitialized(GameBordPartStyleModelData data)
	{
		if (data != null && data.Exp15LocalNaturalDayIndex <= 0)
		{
			int firstLoginLocalCalendarYmd = GetFirstLoginLocalCalendarYmd();
			int ymd = ((data.Exp15LastLocalCalendarYmd > 0) ? data.Exp15LastLocalCalendarYmd : GetLocalCalendarYmd());
			if (firstLoginLocalCalendarYmd > 0 && TryParseLocalCalendarYmd(firstLoginLocalCalendarYmd, out var date) && TryParseLocalCalendarYmd(ymd, out var date2))
			{
				int days = (date2.Date - date.Date).Days;
				data.Exp15LocalNaturalDayIndex = Math.Max(1, days + 1);
			}
			else
			{
				data.Exp15LocalNaturalDayIndex = 1;
			}
		}
	}

	private static void AdvanceExp15LocalNaturalDayIndexOnForwardCross(GameBordPartStyleModelData data, int localCalendarYmd)
	{
		if (data != null)
		{
			EnsureExp15LocalNaturalDayIndexInitialized(data);
			int exp15LastLocalCalendarYmd = data.Exp15LastLocalCalendarYmd;
			if (exp15LastLocalCalendarYmd > 0 && TryParseLocalCalendarYmd(exp15LastLocalCalendarYmd, out var date) && TryParseLocalCalendarYmd(localCalendarYmd, out date) && localCalendarYmd > exp15LastLocalCalendarYmd)
			{
				data.Exp15LocalNaturalDayIndex = Math.Max(1, data.Exp15LocalNaturalDayIndex) + 1;
			}
			else if (data.Exp15LocalNaturalDayIndex <= 0)
			{
				data.Exp15LocalNaturalDayIndex = 1;
			}
		}
	}

	private static bool TryParseLocalCalendarYmd(int ymd, out DateTime date)
	{
		date = default(DateTime);
		if (ymd < 10101)
		{
			return false;
		}
		int num = ymd / 10000;
		int num2 = ymd / 100 % 100;
		int num3 = ymd % 100;
		if (num2 < 1 || num2 > 12 || num3 < 1 || num3 > 31)
		{
			return false;
		}
		try
		{
			date = new DateTime(num, num2, num3);
		}
		catch (ArgumentOutOfRangeException)
		{
			return false;
		}
		if (date.Year == num && date.Month == num2)
		{
			return date.Day == num3;
		}
		return false;
	}

	private static PartStyleType ReturnResolved(string branch, PartStyleType style, GameBordPartStyleModelData data)
	{
		return style;
	}
}
