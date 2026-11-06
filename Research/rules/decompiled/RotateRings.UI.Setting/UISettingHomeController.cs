using System;
using Game.DataTrack;
using Game.UI.Language;
using RotateRings.Common;
using RotateRings.UI.HowToPlay;
using RotateRings.UI.User;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using XingYun.Core.Singletons;
using XingYun.GM;
using XingYun.UI.Controller;
using XingYun.UI.Core;

namespace RotateRings.UI.Setting;

[UIPath("UISettingHome")]
public class UISettingHomeController : BaseController<UISettingHomeView>
{
	private const float GmClickWindowSeconds = 5f;

	private const int GmClickTriggerCount = 20;

	private float gmFirstClickTime;

	private int gmClickCount;

	private string currentName;

	private UISetting_IconToggleItem musicToggleItem;

	private UISetting_IconToggleItem soundToggleItem;

	private UISetting_IconToggleItem vibrationToggleItem;

	private bool isClosing;

	private PlayerNameEditPresenter namePresenter;

	public UISettingHomeController(UISettingHomeView view)
		: base(view)
	{
	}

	public override void OnRegister(object param = null)
	{
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Expected O, but got Unknown
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Expected O, but got Unknown
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Expected O, but got Unknown
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		//IL_009d: Expected O, but got Unknown
		//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c4: Expected O, but got Unknown
		//IL_013b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0145: Expected O, but got Unknown
		//IL_0162: Unknown result type (might be due to invalid IL or missing references)
		//IL_016c: Expected O, but got Unknown
		gmFirstClickTime = 0f;
		gmClickCount = 0;
		isClosing = false;
		((UnityEvent)base.view.BtnClose.onClick).AddListener(new UnityAction(OnCloseClick));
		((UnityEvent)base.view.BtnGM.onClick).AddListener(new UnityAction(OnGMClick));
		((UnityEvent)base.view.btnHowtoPlay.onClick).AddListener(new UnityAction(OnHowtoplayClick));
		((UnityEvent)base.view.btnAboutUs.onClick).AddListener(new UnityAction(OnAboutClick));
		Button btnLanguage = base.view.btnLanguage;
		if (btnLanguage != null)
		{
			((UnityEvent)btnLanguage.onClick).AddListener(new UnityAction(OnLanguageClick));
		}
		base.ListenEvent<SettingHomeInit>((Action<SettingHomeInit>)OnSettingHomeInit);
		base.ListenEvent<UserSaveSuccess>((Action<UserSaveSuccess>)OnUserSaveSuccess);
		base.ListenEvent<UserNameSaveSuccess>((Action<UserNameSaveSuccess>)OnUserNameSaveSuccess);
		base.ListenEvent<UserAvatarSaveSuccess>((Action<UserAvatarSaveSuccess>)OnUserAvatarSaveSuccess);
		base.ListenEvent<LanguageChangeSuccess>((Action<LanguageChangeSuccess>)OnLanguageChangeSuccess);
		Button btnRandom = base.view.btnRandom;
		if (btnRandom != null)
		{
			((UnityEvent)btnRandom.onClick).AddListener(new UnityAction(OnSettingHomeRandomNameClick));
		}
		Button btnConvert = base.view.btnConvert;
		if (btnConvert != null)
		{
			((UnityEvent)btnConvert.onClick).AddListener(new UnityAction(OnSettingHomeChangeAvatarClick));
		}
		namePresenter = new PlayerNameEditPresenter();
		namePresenter.Bind(base.view.InputField, base.view.btnRandom, base.view.btnEdit, base.view.btnConvert, SaveNameFromPresenter, ReportSettingHomeEditNickname);
		currentName = PlayerService.GetName();
		base.ExecuteCommand<SettingHomeInitCommand>((object)null);
	}

	private void OnSettingHomeInit(SettingHomeInit arg)
	{
		currentName = arg.PlayerName;
		base.view.SetHeadInfoVariant(UserSettingService.GetHeadInfoType());
		base.view.Init(arg);
		if (musicToggleItem == null)
		{
			musicToggleItem = new UISetting_IconToggleItem(base.view.btn_Music, base.view.SetMusicToggleVisual);
		}
		if (soundToggleItem == null)
		{
			soundToggleItem = new UISetting_IconToggleItem(base.view.btn_Sound, base.view.SetSoundToggleVisual);
		}
		if (vibrationToggleItem == null)
		{
			vibrationToggleItem = new UISetting_IconToggleItem(base.view.btn_Vibration, base.view.SetVibrationToggleVisual);
		}
		musicToggleItem.Initialize(arg.IsMusicOn, OnMusicToggle);
		soundToggleItem.Initialize(arg.IsSoundOn, OnSoundToggle);
		vibrationToggleItem.Initialize(arg.IsVibrateOn, OnVibrationToggle);
		DisableNameTabNameEditing();
		WireNameTabOpenUIUser();
		namePresenter?.Refresh(currentName);
		namePresenter?.TryAutoFocusOnFirstOpen();
		base.view.PlayOpenAnimation();
	}

	private void OnMusicToggle(bool isOn)
	{
		SettingUtil.ToggleMusicState(isOn, userInitiated: true);
		RRDataTrack.Instance.ReportBtnClick("主页设置-背景音乐-[" + (isOn ? "开启" : "关闭") + "]");
	}

	private void OnSoundToggle(bool isOn)
	{
		SettingUtil.ToggleSoundState(isOn);
		RRDataTrack.Instance.ReportBtnClick("主页设置-音效-[" + (isOn ? "开启" : "关闭") + "]");
	}

	private void OnVibrationToggle(bool isOn)
	{
		SettingUtil.ToggleVibrationState(isOn);
		RRDataTrack.Instance.ReportBtnClick("主页设置-震动-[" + (isOn ? "开启" : "关闭") + "]");
	}

	private void OnCloseClick()
	{
		if (isClosing)
		{
			return;
		}
		isClosing = true;
		if (namePresenter != null && !namePresenter.TryCommit())
		{
			isClosing = false;
			return;
		}
		currentName = PlayerService.GetName();
		RRDataTrack.Instance.ReportBtnClick("主页设置-关闭");
		RRDataTrack.Instance.ReportUserSettingHomeCloseSnapshot(UserSettingTrackHelper.ToTrackFlag(SettingUtil.GetMusicState()), UserSettingTrackHelper.ToTrackFlag(SettingUtil.GetSoundState()), UserSettingTrackHelper.ToTrackFlag(SettingUtil.GetVibrationState()), UserSettingTrackHelper.ToAvatarTrackId(PlayerService.GetHeadId()));
		base.view.PlayCloseAnimation(delegate
		{
			base.CloseView(false);
		});
	}

	private void DisableNameTabNameEditing()
	{
		UIUser_NameTab uIUser_NameTab = base.view.UIUser_NameTab;
		if (!((Object)(object)uIUser_NameTab == (Object)null))
		{
			if ((Object)(object)uIUser_NameTab.buttonEdit != (Object)null)
			{
				((Component)uIUser_NameTab.buttonEdit).gameObject.SetActive(false);
			}
			if ((Object)(object)uIUser_NameTab.buttonRandom != (Object)null)
			{
				((Component)uIUser_NameTab.buttonRandom).gameObject.SetActive(false);
			}
			if ((Object)(object)uIUser_NameTab.inputField != (Object)null)
			{
				((Component)uIUser_NameTab.inputField).gameObject.SetActive(false);
			}
		}
	}

	private void WireNameTabOpenUIUser()
	{
		UIUser_NameTab uIUser_NameTab = base.view.UIUser_NameTab;
		if ((Object)(object)uIUser_NameTab == (Object)null)
		{
			return;
		}
		uIUser_NameTab.onBeforeOpenUIUser = delegate
		{
			if (namePresenter != null && !namePresenter.TryCommit())
			{
				return false;
			}
			currentName = PlayerService.GetName();
			base.view.RefreshName(currentName);
			ReportSettingHomeChangeAvatar();
			return true;
		};
	}

	private void ReportSettingHomeEditNickname()
	{
		RRDataTrack.Instance.ReportBtnClick("主页设置-修改昵称");
	}

	private void OnSettingHomeRandomNameClick()
	{
		RRDataTrack.Instance.ReportBtnClick("主页设置-随机昵称");
	}

	private void ReportSettingHomeChangeAvatar()
	{
		RRDataTrack.Instance.ReportBtnClick("主页设置-修改头像");
	}

	private void OnSettingHomeChangeAvatarClick()
	{
		if (!GameApp.UI.IsOpen("UIUser"))
		{
			ReportSettingHomeChangeAvatar();
		}
	}

	private void SaveNameFromPresenter(string newName, bool skipValidation)
	{
		currentName = newName;
		string name = PlayerService.GetName();
		if (!string.Equals(currentName, name, StringComparison.Ordinal))
		{
			base.ExecuteCommand<UserSaveNameCommand>((object)new UserSaveNameRequest
			{
				PlayerName = currentName,
				SkipValidation = skipValidation
			});
		}
	}

	private void OnHowtoplayClick()
	{
		RRDataTrack.Instance.ReportBtnClick("主页设置-教程");
		GameApp.UI.Open("UIHowToPlayView", (UILayer)2, (object)HowToPlayType.Arrow, (Action)null);
	}

	private static bool IsRestorePurchasesSupported()
	{
		return false;
	}

	private void OnAboutClick()
	{
		GameApp.UI.Open(UIPopupPathDefine.UIAboutUs, (UILayer)2, (object)null, (Action)null);
	}

	private void OnGMClick()
	{
		if (gmFirstClickTime <= 0f || Time.realtimeSinceStartup - gmFirstClickTime > 5f)
		{
			gmFirstClickTime = Time.realtimeSinceStartup;
			gmClickCount = 0;
		}
		gmClickCount++;
		if (Time.realtimeSinceStartup - gmFirstClickTime <= 5f && gmClickCount >= 20)
		{
			Singleton<VersionViewHelp>.Instance.OpenVersionUI();
			gmClickCount = 0;
			gmFirstClickTime = 0f;
		}
	}

	private void OnLanguageClick()
	{
		base.CloseView(false);
		GameApp.UI.Open("UILanguage", (UILayer)2, (object)null, (Action)null);
	}

	private void OnUserSaveSuccess(UserSaveSuccess evt)
	{
		currentName = evt.PlayerName;
		base.view.SetUserNameTab(evt.PlayerName, evt.HeadSprite, evt.HeadFrameSprite);
		namePresenter?.Refresh(evt.PlayerName);
	}

	private void OnUserNameSaveSuccess(UserNameSaveSuccess evt)
	{
		currentName = evt.PlayerName;
		base.view.RefreshName(evt.PlayerName);
		if (namePresenter != null && namePresenter.IsEditing)
		{
			namePresenter.EndEdit();
		}
		namePresenter?.Refresh(evt.PlayerName);
	}

	private void OnLanguageChangeSuccess(LanguageChangeSuccess evt)
	{
		namePresenter?.OnLanguageChanged();
		namePresenter?.Refresh(PlayerService.GetName());
	}

	private void OnUserAvatarSaveSuccess(UserAvatarSaveSuccess evt)
	{
		base.view.RefreshNameHead(evt.HeadSprite);
		base.view.RefreshNameFrame(evt.HeadFrameSprite);
	}

	public override void OnActive(bool isActive)
	{
		base.OnActive(isActive);
		base.view.NotifyPopupMaskStackActive(isActive);
		if (isActive)
		{
			RRDataTrack.Instance.ReportBtnClick("主页设置-点击设置展现");
			namePresenter?.Refresh(PlayerService.GetName());
		}
		else
		{
			namePresenter?.PersistDraftIfEditing();
			namePresenter?.SuppressAutoFocusForSession();
		}
	}

	public override void OnRemove()
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Expected O, but got Unknown
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Expected O, but got Unknown
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Expected O, but got Unknown
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Expected O, but got Unknown
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ab: Expected O, but got Unknown
		//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d2: Expected O, but got Unknown
		//IL_00ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f9: Expected O, but got Unknown
		((UnityEvent)base.view.BtnClose.onClick).RemoveListener(new UnityAction(OnCloseClick));
		((UnityEvent)base.view.BtnGM.onClick).RemoveListener(new UnityAction(OnGMClick));
		((UnityEvent)base.view.btnHowtoPlay.onClick).RemoveListener(new UnityAction(OnHowtoplayClick));
		((UnityEvent)base.view.btnAboutUs.onClick).RemoveListener(new UnityAction(OnAboutClick));
		Button btnLanguage = base.view.btnLanguage;
		if (btnLanguage != null)
		{
			((UnityEvent)btnLanguage.onClick).RemoveListener(new UnityAction(OnLanguageClick));
		}
		Button btnRandom = base.view.btnRandom;
		if (btnRandom != null)
		{
			((UnityEvent)btnRandom.onClick).RemoveListener(new UnityAction(OnSettingHomeRandomNameClick));
		}
		Button btnConvert = base.view.btnConvert;
		if (btnConvert != null)
		{
			((UnityEvent)btnConvert.onClick).RemoveListener(new UnityAction(OnSettingHomeChangeAvatarClick));
		}
		musicToggleItem?.Dispose();
		soundToggleItem?.Dispose();
		vibrationToggleItem?.Dispose();
		musicToggleItem = null;
		soundToggleItem = null;
		vibrationToggleItem = null;
		base.UnlistenEvent<SettingHomeInit>((Action<SettingHomeInit>)OnSettingHomeInit);
		base.UnlistenEvent<UserSaveSuccess>((Action<UserSaveSuccess>)OnUserSaveSuccess);
		base.UnlistenEvent<UserNameSaveSuccess>((Action<UserNameSaveSuccess>)OnUserNameSaveSuccess);
		base.UnlistenEvent<UserAvatarSaveSuccess>((Action<UserAvatarSaveSuccess>)OnUserAvatarSaveSuccess);
		base.UnlistenEvent<LanguageChangeSuccess>((Action<LanguageChangeSuccess>)OnLanguageChangeSuccess);
		UIUser_NameTab uIUser_NameTab = base.view.UIUser_NameTab;
		if ((Object)(object)uIUser_NameTab != (Object)null)
		{
			uIUser_NameTab.onBeforeOpenUIUser = null;
		}
		namePresenter?.Dispose();
		namePresenter = null;
	}

	public override void OnEscape()
	{
		if (namePresenter != null && namePresenter.IsEditing)
		{
			namePresenter.EndEdit();
		}
		else
		{
			OnCloseClick();
		}
	}
}
