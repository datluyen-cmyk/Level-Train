using System;
using Game.DataTrack;
using RotateRings.Ad;
using RotateRings.UI.Game;
using XingYun.Command;
using XingYun.UI.Controller;
using XingYun.UI.Core;

namespace RotateRings.UI.Level;

public class LevelBackHomeCommand : BaseCommand
{
	public override void Execute(object param = null)
	{
		if (param is string text && !string.IsNullOrEmpty(text))
		{
			GameApp.UI.CloseView(text, (object)null, false);
		}
		RRDataTrack.Instance.ReportPageShow("关卡进主页");
		Gameplay.Active?.ExitToHome();
		AdService.ClearPendingFrontAd("LevelBackHome");
		CloseUIGameThenOpenHome();
	}

	public static void CloseUIGameThenOpenHome()
	{
		IController val = default(IController);
		if (GameApp.UI.IsOpen("UIGame", ref val) && val is UIGameController uIGameController)
		{
			uIGameController.CloseWithAnimation(OpenHome);
			return;
		}
		if (GameApp.UI.IsOpen("UIGame"))
		{
			GameApp.UI.CloseView("UIGame", (object)null, false);
		}
		OpenHome();
	}

	private static void OpenHome()
	{
		if (!GameApp.UI.IsOpen("UIHome"))
		{
			GameApp.UI.Open("UIHome", (UILayer)1, (object)true, (Action)null);
		}
	}
}
