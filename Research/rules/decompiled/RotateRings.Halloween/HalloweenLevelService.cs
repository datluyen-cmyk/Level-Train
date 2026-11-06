using System;
using RotateRing.GamePlay;
using RotateRings.Model;
using RotateRings.SpecialLevel;
using RotateRings.UI.GameEnterToast.Core;

namespace RotateRings.Halloween;

public class HalloweenLevelService : SpecialLevelService
{
	private const string LogTagLiteral = "[HalloweenLevelService] ";

	private const int UnlockCompletedMainLevel = 15;

	protected override GameMode SpecialGameMode => GameMode.Halloween;

	protected override string LogTag => "[HalloweenLevelService] ";

	public override int BannerStyleId => 5;

	public override string BannerLangKey => LevelEntryBannerConfig.HalloweenBannerLangKey;

	public override string DisplayName => "万圣节特殊关";

	protected override bool IsSupportedBaseMode(GameMode gameMode)
	{
		if (gameMode != GameMode.Main)
		{
			return gameMode == GameMode.Endless;
		}
		return true;
	}

	public override bool IsActivityAvailable()
	{
		if (!HalloweenFeatureService.IsLevelEnabled())
		{
			return false;
		}
		return IsMainLevelUnlockReached();
	}

	private static bool IsMainLevelUnlockReached()
	{
		return (GameApp.Proxy.Get<LevelModelProxy>()?.MainLevel ?? 0) > 15;
	}

	public override int ResolveActivityDayIndex()
	{
		if (!HalloweenFeatureService.TryGetActivityStartDate(out var activityStartDate))
		{
			return 0;
		}
		int days = (DateTime.Now.Date - activityStartDate.Date).Days;
		if (days < 0)
		{
			return 0;
		}
		return days % 7 + 1;
	}

	public static void ResetGmLevelCursor()
	{
	}

	public override int ResolveLevelResourceId(int activityDayIndex)
	{
		if (activityDayIndex <= 0)
		{
			return 0;
		}
		if (LevelConfigService.GetLevelConfig(GameMode.Halloween, activityDayIndex) == null)
		{
			return 0;
		}
		return activityDayIndex;
	}
}
