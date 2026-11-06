using System;
using Game.Common.Services;
using Game.DataTrack;
using Game.UI;
using Game.UI.Language;
using Hot.Model;
using RotateRings.Common;
using RotateRings.GamePlay.Save;
using RotateRings.Model;
using RotateRings.Services;
using RotateRings.StartEntry;
using RotateRings.UI.Level;
using RotateRings.UI.Setting;
using RotateRings.UI.Shop;
using RotateRings.UI.User;
using Rotaterings.Rank;
using Spine.Unity;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using XingYun.UI.Controller;
using XingYun.UI.Core;

namespace RotateRings.UI.Home;

[UIPath("UIHome")]
public class UIHomeController : GameBaseController<UIHomeView>
{
	private const float StartEntryAutoEnterLockSlackSec = 2f;

	private int startEntryAutoEnterDeferTimerId = -1;

	private int startEntryAutoEnterWaitTimerId = -1;

	private bool startEntryAutoEnterWaitLockHeld;

	public UIHomeController(UIHomeView view)
		: base(view)
	{
	}

	public override void OnRegister(object param = null)
	{
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Expected O, but got Unknown
		//IL_00d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00da: Expected O, but got Unknown
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Expected O, but got Unknown
		//IL_0106: Unknown result type (might be due to invalid IL or missing references)
		//IL_0110: Expected O, but got Unknown
		RankActivityEntry.Service?.TryResetRankStateIfBelowOpenLevel();
		SetHomeInteraction(interactable: true);
		((BaseController<UIHomeView>)this).view.InitBtnOK(playIdleAnimation: true);
		((BaseController<UIHomeView>)this).view.InitBtnAvatar();
		if ((Object)(object)((BaseController<UIHomeView>)this).view.BtnOKView?.btnOK != (Object)null)
		{
			((UnityEvent)((BaseController<UIHomeView>)this).view.BtnOKView.btnOK.onClick).AddListener(new UnityAction(OnOKClick));
		}
		if ((Object)(object)((BaseController<UIHomeView>)this).view.BtnAvatarView?.btnAvatar != (Object)null)
		{
			((UnityEvent)((BaseController<UIHomeView>)this).view.BtnAvatarView.btnAvatar.onClick).AddListener(new UnityAction(OnAvatarClick));
		}
		((UnityEvent)((BaseController<UIHomeView>)this).view.btnSetting.onClick).AddListener(new UnityAction(OnSettingClick));
		bool flag = ShopService.ShopToggle();
		((BaseController<UIHomeView>)this).view.SetShopEntryVisible(flag);
		if (flag)
		{
			((UnityEvent)((BaseController<UIHomeView>)this).view.btnShop.onClick).AddListener(new UnityAction(OnShopClick));
		}
		((BaseController<UIHomeView>)this).ListenEvent<LanguageChangeSuccess>((Action<LanguageChangeSuccess>)OnLanguageChangeSuccess);
		((BaseController<UIHomeView>)this).ListenEvent<UserNameSaveSuccess>((Action<UserNameSaveSuccess>)OnUserNameSaveSuccess);
		((BaseController<UIHomeView>)this).ListenEvent<UserSaveSuccess>((Action<UserSaveSuccess>)OnUserSaveSuccess);
		((BaseController<UIHomeView>)this).ListenEvent<UserAvatarSaveSuccess>((Action<UserAvatarSaveSuccess>)OnUserAvatarSaveSuccess);
		((BaseController<UIHomeView>)this).ListenEvent<CloudSaveDataAppliedEvent>((Action<CloudSaveDataAppliedEvent>)OnCloudSaveDataApplied);
		bool flag2 = param is bool && (bool)param;
		StartEntryHomeLaunchParam startEntryHomeLaunchParam = param as StartEntryHomeLaunchParam;
		ActivityModuleInit.EnsureInitialized();
		ReissueRewardService.ReserveColdStartRewards();
		StartEntryColdLaunch.ApplyOnHomeRegister(startEntryHomeLaunchParam);
		bool flag3 = ActivityModuleInit.AnyHomePopupWouldRun(flag2);
		ActivityModuleInit.BuildAndExecuteHomePopups(flag2);
		if (flag)
		{
			SkeletonGraphic btnShopSpine = ((BaseController<UIHomeView>)this).view.btnShopSpine;
			if (btnShopSpine != null)
			{
				btnShopSpine.AnimationState.SetAnimation(0, "idle", true);
			}
		}
		((BaseController<UIHomeView>)this).view.InitHalloweenHomeBackground();
		((BaseController<UIHomeView>)this).view.InitActivityEntries();
		((BaseController<UIHomeView>)this).view.InitLogo();
		RefreshUI();
		float homeOpenAnimationDuration = ((BaseController<UIHomeView>)this).view.PlayHomeOpenAnimation();
		if (startEntryHomeLaunchParam != null && startEntryHomeLaunchParam.AutoEnterLevelAfterPopups && !flag2 && !flag3)
		{
			InitStartEntryAutoEnterIfNeeded(startEntryHomeLaunchParam, homeOpenAnimationDuration);
		}
		((BaseController<UIHomeView>)this).view.StartIconSpineLoop();
		BgmController.EnterHome();
	}

	private void OnShopClick()
	{
		GameApp.UI.Open("UIShop", (UILayer)2, (object)null, (Action)null);
	}

	public override void OnRemove()
	{
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Expected O, but got Unknown
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Expected O, but got Unknown
		//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cd: Expected O, but got Unknown
		BackExitService.Reset(BackExitScene.Home);
		CancelStartEntryAutoEnterFlow();
		((BaseController<UIHomeView>)this).view.StopIconSpineLoop();
		((BaseController<UIHomeView>)this).view.ShutdownActivityEntries();
		if ((Object)(object)((BaseController<UIHomeView>)this).view.BtnOKView?.btnOK != (Object)null)
		{
			((UnityEvent)((BaseController<UIHomeView>)this).view.BtnOKView.btnOK.onClick).RemoveListener(new UnityAction(OnOKClick));
		}
		((UnityEvent)((BaseController<UIHomeView>)this).view.btnSetting.onClick).RemoveListener(new UnityAction(OnSettingClick));
		if ((Object)(object)((BaseController<UIHomeView>)this).view.BtnAvatarView?.btnAvatar != (Object)null)
		{
			((UnityEvent)((BaseController<UIHomeView>)this).view.BtnAvatarView.btnAvatar.onClick).RemoveListener(new UnityAction(OnAvatarClick));
		}
		if ((Object)(object)((BaseController<UIHomeView>)this).view.btnShop != (Object)null)
		{
			((UnityEventBase)((BaseController<UIHomeView>)this).view.btnShop.onClick).RemoveAllListeners();
		}
		((BaseController<UIHomeView>)this).UnlistenEvent<LanguageChangeSuccess>((Action<LanguageChangeSuccess>)OnLanguageChangeSuccess);
		((BaseController<UIHomeView>)this).UnlistenEvent<UserNameSaveSuccess>((Action<UserNameSaveSuccess>)OnUserNameSaveSuccess);
		((BaseController<UIHomeView>)this).UnlistenEvent<UserSaveSuccess>((Action<UserSaveSuccess>)OnUserSaveSuccess);
		((BaseController<UIHomeView>)this).UnlistenEvent<UserAvatarSaveSuccess>((Action<UserAvatarSaveSuccess>)OnUserAvatarSaveSuccess);
		((BaseController<UIHomeView>)this).UnlistenEvent<CloudSaveDataAppliedEvent>((Action<CloudSaveDataAppliedEvent>)OnCloudSaveDataApplied);
		base.OnRemove();
	}

	public override void OnActive(bool isActive)
	{
		((BaseController<UIHomeView>)this).OnActive(isActive);
		if (isActive)
		{
			RankActivityEntry.Service?.TryResetRankStateIfBelowOpenLevel();
			RefreshUI();
		}
	}

	public override void OnEscape()
	{
		BackExitService.HandleBack(BackExitScene.Home);
	}

	private void OnOKClick()
	{
		HomePopupPipeline.RunWhenIdle(ExecuteOKClick);
	}

	private void InitStartEntryAutoEnterIfNeeded(StartEntryHomeLaunchParam launchParam, float homeOpenAnimationDuration)
	{
		if (launchParam != null && launchParam.AutoEnterLevelAfterPopups)
		{
			CancelStartEntryAutoEnterFlow();
			Mathf.Max(0f, homeOpenAnimationDuration + 0f);
			float homeOpenAnimSec = homeOpenAnimationDuration;
			startEntryAutoEnterDeferTimerId = GameApp.Timer.NextFrame((Object)(object)((UIBaseView)((BaseController<UIHomeView>)this).view).Behaviour, (Action)delegate
			{
				BeginStartEntryAutoEnterWaitAfterOverlayReady(homeOpenAnimSec);
			});
		}
	}

	private void BeginStartEntryAutoEnterWaitAfterOverlayReady(float homeOpenAnimationDuration)
	{
		startEntryAutoEnterDeferTimerId = -1;
		if (!GameApp.UI.IsOpen("UILockOverlay"))
		{
			GameApp.UI.Open("UILockOverlay", (UILayer)4, (object)null, (Action)null);
		}
		float num = Mathf.Max(0f, homeOpenAnimationDuration + 0f);
		LockStartEntryAutoEnterWait(num);
		startEntryAutoEnterWaitTimerId = GameApp.Timer.Delay((Object)(object)((UIBaseView)((BaseController<UIHomeView>)this).view).Behaviour, num, (Action)OnStartEntryAutoEnterWaitFinished);
	}

	private void OnStartEntryAutoEnterWaitFinished()
	{
		startEntryAutoEnterWaitTimerId = -1;
		ExecuteStartEntryAutoEnterLevel();
	}

	private void ExecuteStartEntryAutoEnterLevel()
	{
		HomePopupPipeline.RunWhenIdle(delegate
		{
			ExecuteOKClick();
			UnlockStartEntryAutoEnterWait();
		});
	}

	private void LockStartEntryAutoEnterWait(float waitDurationSec)
	{
		if (startEntryAutoEnterWaitLockHeld)
		{
			UnlockStartEntryAutoEnterWait();
		}
		float duration = waitDurationSec + 2f;
		startEntryAutoEnterWaitLockHeld = true;
		LockUIService.Enable(duration);
	}

	private void UnlockStartEntryAutoEnterWait()
	{
		if (startEntryAutoEnterWaitLockHeld)
		{
			startEntryAutoEnterWaitLockHeld = false;
			LockUIService.Disable();
		}
	}

	private void CancelStartEntryAutoEnterFlow()
	{
		if (startEntryAutoEnterDeferTimerId != -1)
		{
			GameApp.Timer.Kill(startEntryAutoEnterDeferTimerId);
			startEntryAutoEnterDeferTimerId = -1;
		}
		if (startEntryAutoEnterWaitTimerId != -1)
		{
			GameApp.Timer.Kill(startEntryAutoEnterWaitTimerId);
			startEntryAutoEnterWaitTimerId = -1;
		}
		UnlockStartEntryAutoEnterWait();
	}

	private void ExecuteOKClick()
	{
		((BaseController<UIHomeView>)this).ExecuteCommand<EnterLevelCommand>((object)null);
		int level = GameApp.Proxy.Get<LevelModelProxy>().Level;
		RRDataTrack.Instance.ReportBtnClick("主页-第[" + level + "]关");
	}

	private void OnSettingClick()
	{
		string text = (CloudSaveFlowService.IsCloudSaveEnabled() ? "UISettingHomeV2" : "UISettingHome");
		GameApp.UI.Open(text, (UILayer)2, (object)null, (Action)null);
		RRDataTrack.Instance.ReportBtnClick("主页-设置");
	}

	private void OnAvatarClick()
	{
		UserSettingService.MarkUserSettingOpened();
		GameApp.UI.Open("UIUser", (UILayer)2, (object)null, (Action)null);
		RRDataTrack.Instance.ReportBtnClick("主页-头像");
	}

	public float GetHomeCloseAnimationDuration()
	{
		return ((BaseController<UIHomeView>)this).view.GetHomeCloseAnimationDuration();
	}

	public void HideImmediately()
	{
		SetHomeInteraction(interactable: false);
		((Component)((UIBaseView)((BaseController<UIHomeView>)this).view).Behaviour).gameObject.SetActive(false);
	}

	public void CloseImmediately(Action onComplete = null)
	{
		SetHomeInteraction(interactable: false);
		((Component)((UIBaseView)((BaseController<UIHomeView>)this).view).Behaviour).gameObject.SetActive(false);
		GameApp.UI.CloseView("UIHome", (object)null, false);
		onComplete?.Invoke();
	}

	public void CloseWithAnimation(Action onComplete = null)
	{
		SetHomeInteraction(interactable: false);
		((BaseController<UIHomeView>)this).view.FadeOutLogo(0.2f, null);
		float num = ((BaseController<UIHomeView>)this).view.PlayHomeCloseAnimation();
		if (num <= 0f)
		{
			CloseImmediately(onComplete);
			return;
		}
		GameApp.Timer.Delay((Object)(object)((UIBaseView)((BaseController<UIHomeView>)this).view).Behaviour, num, (Action)delegate
		{
			((Component)((UIBaseView)((BaseController<UIHomeView>)this).view).Behaviour).gameObject.SetActive(false);
			GameApp.UI.CloseView("UIHome", (object)null, false);
			onComplete?.Invoke();
		});
	}

	private void SetHomeInteraction(bool interactable)
	{
		((BaseController<UIHomeView>)this).view.BtnOKView?.SetInteractable(interactable);
		((Selectable)((BaseController<UIHomeView>)this).view.btnSetting).interactable = interactable;
		((BaseController<UIHomeView>)this).view.BtnAvatarView?.SetInteractable(interactable);
	}

	private void OnLanguageChangeSuccess(LanguageChangeSuccess evt)
	{
		RefreshUI();
	}

	public void RefreshAfterGameProgressReset()
	{
		RefreshUI();
	}

	private void RefreshUI()
	{
		int level = GameApp.Proxy.Get<LevelModelProxy>().Level;
		bool hasUnfinishedLevel = GameApp.Proxy.Get<RingLevelSaveProxy>().HasValidSave(level);
		GameMode currentGameMode = GameInfo.CurrentGameMode;
		ELevelDifficulty difficulty = LevelDifficultyUtil.GetDifficulty(currentGameMode, level);
		((BaseController<UIHomeView>)this).view.RefreshPlayButton(difficulty, hasUnfinishedLevel, currentGameMode, level);
		((BaseController<UIHomeView>)this).view.RefreshActivityEntries();
		RefreshAvatarEntry();
	}

	private void RefreshAvatarEntry()
	{
		bool flag = UserInfoService.ShouldShowHomeAvatarEntry() && UserInfoService.IsHomeAvatarUnlocked();
		((BaseController<UIHomeView>)this).view.RefreshAvatarEntry(flag);
		if (flag)
		{
			((BaseController<UIHomeView>)this).view.RefreshAvatarHead();
		}
	}

	private void OnUserNameSaveSuccess(UserNameSaveSuccess evt)
	{
		RefreshAvatarEntry();
	}

	private void OnUserSaveSuccess(UserSaveSuccess evt)
	{
		RefreshAvatarEntry();
	}

	private void OnUserAvatarSaveSuccess(UserAvatarSaveSuccess evt)
	{
		RefreshAvatarEntry();
	}

	private void OnCloudSaveDataApplied(CloudSaveDataAppliedEvent evt)
	{
		RefreshUI();
	}
}
