using System;
using Game.Common.Services;
using RotateRing.GamePlay;
using RotateRings.Ad;
using RotateRings.Model;
using RotateRings.Services;
using RotateRings.UI.Game;
using RotateRings.UI.Home;
using UniLog;
using UnityEngine;
using XingYun.Command;
using XingYun.UI.Controller;
using XingYun.UI.Core;

namespace RotateRings.UI.Level;

public class EnterLevelCommand : BaseCommand
{
	private static bool isEnteringLevel;

	private const float GameOpenLeadTimeBeforeHomeClose = 0.1f;

	public static bool IsEnteringLevel => isEnteringLevel;

	public static void ReleaseEnterGate()
	{
		if (isEnteringLevel)
		{
			isEnteringLevel = false;
		}
	}

	public override void Execute(object param = null)
	{
		if (!isEnteringLevel)
		{
			bool flag = param is EnterLevelParam enterLevelParam && enterLevelParam.SkipHomeCloseAnimation;
			isEnteringLevel = true;
			HomePopupPipeline.Abort();
			if (flag)
			{
				HideHomeImmediately();
			}
			LevelModelProxy levelModelProxy = GameApp.Proxy.Get<LevelModelProxy>();
			levelModelProxy?.PrepareEnterGame();
			int level = levelModelProxy?.Level ?? 1;
			GameplayCameraManager.IsPlayFromLoading = true;
			AdService.RequestHomeEnterBeforeInterstitial(InterstitialTrigger.NewGame, RRAdContext.FromLevelProxy());
			ProceedEnterLevel(level, flag);
		}
	}

	public static void PrepareOverlayEnterLevel()
	{
		HideHomeImmediately();
	}

	private static void HideHomeImmediately()
	{
		IController val = default(IController);
		if (GameApp.UI.IsOpen("UIHome", ref val) && val is UIHomeController uIHomeController)
		{
			uIHomeController.HideImmediately();
		}
	}

	private static void ProceedEnterLevel(int level, bool skipHomeCloseAnimation)
	{
		if (GameplaySceneLoader.IsSceneReady)
		{
			EnterLevelInExistingScene(level, skipHomeCloseAnimation);
		}
		else
		{
			EnterLevelWithSceneLoad(level, skipHomeCloseAnimation);
		}
	}

	private static void EnterLevelInExistingScene(int level, bool skipHomeCloseAnimation)
	{
		IGameplayManager active = Gameplay.Active;
		if ((Object)(object)active.CurrentLevelObject == (Object)null && !active.IsLoadingLevel)
		{
			FinishEnterLevelUI(level, loadLevel: true, null, skipHomeCloseAnimation);
		}
		else
		{
			FinishEnterLevelUI(level, loadLevel: false, ReleaseEnterGate, skipHomeCloseAnimation);
		}
	}

	private static void EnterLevelWithSceneLoad(int level, bool skipHomeCloseAnimation)
	{
		EnsureAudioListener();
		GameplaySceneLoader.LoadSceneAsync(delegate
		{
			FinishEnterLevelUI(level, loadLevel: true, null, skipHomeCloseAnimation);
		}, delegate
		{
			Log.LogError("EnterLevelCommand", "场景加载失败: {0}", new object[1] { "GameScene" });
			AdService.ClearPendingFrontAd("SceneLoadFailed");
			ReleaseEnterGate();
		});
	}

	private static void CloseHomeIfNeeded(Action onComplete, bool skipHomeCloseAnimation)
	{
		IController val = default(IController);
		if (GameApp.UI.IsOpen("UIHome", ref val) && val is UIHomeController uIHomeController)
		{
			if (skipHomeCloseAnimation)
			{
				uIHomeController.CloseImmediately(onComplete);
			}
			else
			{
				uIHomeController.CloseWithAnimation(onComplete);
			}
			return;
		}
		if (GameApp.UI.IsOpen("UIHome"))
		{
			GameApp.UI.CloseView("UIHome", (object)null, false);
		}
		onComplete?.Invoke();
	}

	private static void OpenGameAndLockOverlay()
	{
		IController val = default(IController);
		if (GameApp.UI.IsOpen("UIGame", ref val) && val is UIGameController uIGameController)
		{
			uIGameController.PlayEnterTransitionIfNeeded();
		}
		else if (!GameApp.UI.IsOpen("UIGame"))
		{
			GameApp.UI.Open("UIGame", (UILayer)1, (object)null, (Action)null);
		}
		if (!GameApp.UI.IsOpen("UILockOverlay"))
		{
			GameApp.UI.Open("UILockOverlay", (UILayer)4, (object)null, (Action)null);
		}
	}

	private static void FinishEnterLevelUI(int level, bool loadLevel, Action onHomeClosed = null, bool skipHomeCloseAnimation = false)
	{
		IController val = default(IController);
		if (GameApp.UI.IsOpen("UIGame", ref val) && val is UIGameController uIGameController)
		{
			uIGameController.PlayEnterTransitionIfNeeded();
			if (!GameApp.UI.IsOpen("UILockOverlay"))
			{
				GameApp.UI.Open("UILockOverlay", (UILayer)4, (object)null, (Action)null);
			}
			if (loadLevel)
			{
				GameplaySceneLoader.LoadLevel(level);
			}
			CloseHomeIfNeeded(onHomeClosed, skipHomeCloseAnimation);
			return;
		}
		UIHomeController uIHomeController = null;
		IController val2 = default(IController);
		if (GameApp.UI.IsOpen("UIHome", ref val2) && val2 is UIHomeController uIHomeController2)
		{
			uIHomeController = uIHomeController2;
		}
		float num = ((skipHomeCloseAnimation || uIHomeController == null) ? 0f : Mathf.Max(0f, uIHomeController.GetHomeCloseAnimationDuration() - 0.1f));
		CloseHomeIfNeeded(onHomeClosed, skipHomeCloseAnimation);
		if (num <= 0f)
		{
			OpenGameAndLoadLevel();
			return;
		}
		MonoBehaviour val3 = (MonoBehaviour)(object)((uIHomeController != null) ? ((UIBaseView)uIHomeController.View).Behaviour : null);
		if ((Object)(object)val3 != (Object)null)
		{
			GameApp.Timer.Delay((Object)(object)val3, num, (Action)OpenGameAndLoadLevel);
		}
		else
		{
			GameApp.Timer.Delay(num, (Action)OpenGameAndLoadLevel);
		}
		void OpenGameAndLoadLevel()
		{
			OpenGameAndLockOverlay();
			if (loadLevel)
			{
				GameplaySceneLoader.LoadLevel(level);
			}
		}
	}

	private static void EnsureAudioListener()
	{
		AudioService.EnsureAudioListener();
	}
}
