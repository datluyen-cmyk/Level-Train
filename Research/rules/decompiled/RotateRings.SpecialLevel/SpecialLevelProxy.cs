using System;
using System.Globalization;
using XingYun.Proxy;

namespace RotateRings.SpecialLevel;

public class SpecialLevelProxy : BaseProxy
{
	private const string DateKeyFormat = "yyyyMMdd";

	public SpecialLevelSaveData Data { get; }

	public string TodayKey => DateTime.Now.ToString("yyyyMMdd", CultureInfo.InvariantCulture);

	public bool HasInsertedToday => string.Equals(Data.LastInsertDateKey, TodayKey, StringComparison.Ordinal);

	public bool HasActiveSession
	{
		get
		{
			if (Data.ActivityDayIndex > 0)
			{
				return string.Equals(Data.SessionDateKey, TodayKey, StringComparison.Ordinal);
			}
			return false;
		}
	}

	public SpecialLevelProxy()
		: base("SpecialLevelProxy")
	{
		Data = GameApp.Save.GetOrCreate<SpecialLevelSaveData>();
	}

	public void BeginSession(int activityDayIndex, int baseGameMode)
	{
		Data.LastInsertDateKey = TodayKey;
		Data.SessionDateKey = TodayKey;
		Data.ActivityDayIndex = activityDayIndex;
		Data.BaseGameMode = baseGameMode;
		Data.IsLevelCompleted = false;
	}

	public void MarkSessionCompleted()
	{
		Data.IsLevelCompleted = true;
	}

	public void EndSession()
	{
		Data.SessionDateKey = string.Empty;
		Data.ActivityDayIndex = 0;
		Data.IsLevelCompleted = false;
	}

	public void DropExpiredSession()
	{
		if (!HasActiveSession && Data.ActivityDayIndex > 0)
		{
			Data.SessionDateKey = string.Empty;
			Data.ActivityDayIndex = 0;
			Data.IsLevelCompleted = false;
		}
	}
}
