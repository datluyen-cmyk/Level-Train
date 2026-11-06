using System;
using Game.DataTrack;
using RotateRings.UI.Toast;
using UnityEngine;

namespace RotateRings.Services;

public static class BackExitService
{
	private const float ConfirmWindowSeconds = 3f;

	private const string ExitTipLanguageKey = "TapBtnBack_Exit";

	private static BackExitScene pendingScene;

	private static float firstBackTime = -1f;

	public static void HandleBack(BackExitScene scene)
	{
		float realtimeSinceStartup = Time.realtimeSinceStartup;
		if (pendingScene == scene && firstBackTime >= 0f && realtimeSinceStartup - firstBackTime <= 3f)
		{
			RRDataTrack.Instance.ReportBtnClick("物理返回键-确认退出游戏");
			Reset(scene);
			QuitApplication();
			return;
		}
		pendingScene = scene;
		firstBackTime = realtimeSinceStartup;
		RRDataTrack.Instance.ReportBtnClick("物理返回键-再按一次退出游戏");
		ToastService.Open(new UIToastOpenParam
		{
			Content = GetExitTip(),
			Duration = 3f,
			OnEscape = delegate
			{
				HandleBack(scene);
			},
			OnDismissed = delegate
			{
				Reset(scene);
			}
		});
	}

	public static void Reset(BackExitScene scene)
	{
		if (pendingScene == scene)
		{
			pendingScene = BackExitScene.Home;
			firstBackTime = -1f;
		}
	}

	private static string GetExitTip()
	{
		return GameApp.Lang.GetText("TapBtnBack_Exit", Array.Empty<object>());
	}

	private static void QuitApplication()
	{
		Application.Quit();
	}
}
