using System;
using System.Collections.Generic;
using Game.DataTrack;
using UniLog;

namespace RotateRings.Theme;

public static class ThemeService
{
	private const string TAG = "[Theme][ThemeService] ";

	private const string SlotKey = "Theme_State";

	private static readonly List<IThemeElement> elements = new List<IThemeElement>();

	private static ThemeId currentThemeId = ThemeId.Day;

	private static bool initialized;

	public static bool HasInitialized => initialized;

	public static ThemeId CurrentThemeId => currentThemeId;

	public static void Init()
	{
		if (!initialized)
		{
			currentThemeId = (ThemeId)GameApp.Save.GetOrCreate<ThemeState>("Theme_State").CurrentThemeId;
			initialized = true;
			Broadcast();
		}
	}

	public static void Register(IThemeElement element)
	{
		if (element != null && !elements.Contains(element))
		{
			elements.Add(element);
		}
	}

	public static void Unregister(IThemeElement element)
	{
		if (element != null)
		{
			elements.Remove(element);
		}
	}

	public static void SetTheme(ThemeId themeId)
	{
		if (initialized && currentThemeId != themeId)
		{
			currentThemeId = themeId;
			GameApp.Save.GetOrCreate<ThemeState>("Theme_State").CurrentThemeId = (int)currentThemeId;
			GameApp.Save.SaveAll();
			Broadcast();
			RRDataTrack.Instance.ReportUserSettingDisplayMode(ToDisplayMode(currentThemeId));
		}
	}

	private static string ToDisplayMode(ThemeId themeId)
	{
		if (themeId != ThemeId.Night)
		{
			return "light";
		}
		return "dark";
	}

	public static void CycleNextTheme()
	{
		ThemeId[] array = (ThemeId[])Enum.GetValues(typeof(ThemeId));
		SetTheme((ThemeId)((int)(currentThemeId + 1) % array.Length));
	}

	private static void Broadcast()
	{
		IThemeElement[] array = elements.ToArray();
		foreach (IThemeElement themeElement in array)
		{
			try
			{
				themeElement.ApplyTheme(currentThemeId);
			}
			catch (Exception ex)
			{
				Log.LogError("[Theme][ThemeService] ", "元素 ApplyTheme 异常: {0}", new object[1] { ex });
			}
		}
		GameApp.Event.Send<ThemeChangedEvent>(new ThemeChangedEvent
		{
			CurrentThemeId = currentThemeId
		});
	}
}
