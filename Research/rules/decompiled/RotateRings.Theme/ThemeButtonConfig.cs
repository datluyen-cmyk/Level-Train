using XingYun.Experiment;

namespace RotateRings.Theme;

public static class ThemeButtonConfig
{
	private const string TAG = "[Theme][ThemeButtonConfig] ";

	private const int StyleIdInGameHUD = 0;

	private const int StyleIdInSettingPanel = 1;

	public static ThemeButtonPlacement? GmPlacement;

	public static bool IsDarkModeEnabled
	{
		get
		{
			DayOrNightModeFunction dayOrNightModeFunction = FunctionDataCenter.Get<DayOrNightModeFunction>();
			if (dayOrNightModeFunction == null)
			{
				return false;
			}
			return dayOrNightModeFunction.Enable == 1;
		}
	}

	public static ThemeButtonPlacement CurrentPlacement
	{
		get
		{
			if (!IsDarkModeEnabled)
			{
				return ThemeButtonPlacement.None;
			}
			if (GmPlacement.HasValue)
			{
				return GmPlacement.Value;
			}
			if ((FunctionDataCenter.Get<DayOrNightModeChoose>()?.StyleId ?? 0) != 1)
			{
				return ThemeButtonPlacement.InGameHUD;
			}
			return ThemeButtonPlacement.InSettingPanel;
		}
	}

	public static void SetPlacement(ThemeButtonPlacement placement)
	{
		GmPlacement = placement;
	}
}
