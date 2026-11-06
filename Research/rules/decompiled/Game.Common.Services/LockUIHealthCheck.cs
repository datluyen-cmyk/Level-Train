using RotateRings.Common;
using RotateRings.UI.LockOverlay;
using XingYun.UI.Controller;

namespace Game.Common.Services;

public static class LockUIHealthCheck
{
	private const string Tag = "[LockCheck] ";

	public static void RecheckStuckMask(string trigger)
	{
		if (LockUIService.CurrentLockCount > 0 && !GameApp.UI.IsOpen("UINewElementIntro") && !GameApp.UI.IsOpen("UIPropUnlock"))
		{
			LockUIService.ForceRelease(trigger);
			RequireOverlay()?.ForceReset(trigger);
		}
	}

	public static void CloseStalePreEnterPopups(string trigger)
	{
		CloseIfOpen("UIRankListPopup", trigger);
		CloseIfOpen("UIRankListPopupHalloween", trigger);
		CloseIfOpen("UIGameDifficultyToastView", trigger);
		CloseIfOpen("UIGameDifficultyToastView_1", trigger);
		CloseIfOpen("UIGameDifficultyToastView_2", trigger);
	}

	private static void CloseIfOpen(string location, string trigger)
	{
		if (GameApp.UI.IsOpen(location))
		{
			GameApp.UI.CloseView(location, (object)null, false);
		}
	}

	private static UILockOverlayController RequireOverlay()
	{
		IController val = default(IController);
		if (GameApp.UI.IsOpen("UILockOverlay", ref val) && val is UILockOverlayController result)
		{
			return result;
		}
		return null;
	}
}
