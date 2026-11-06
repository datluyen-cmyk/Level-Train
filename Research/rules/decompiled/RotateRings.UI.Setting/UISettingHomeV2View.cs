using System;
using Game.UI;
using RotateRings.UI.User;
using Rotaterings.Tools;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using XingYun.UI.Core;

namespace RotateRings.UI.Setting;

public class UISettingHomeV2View : GameBaseView
{
	private const string OpenAnimName = "anim_popup_open";

	private const string CloseAnimName = "anim_popup_close";

	private const string SignInGoogleLangKey = "Account_SignIn_Google";

	private const string SignInAppleLangKey = "Account_SignIn_Apple";

	private const string SignOutLangKey = "Account_SignOut";

	private UIUser_NameTab cachedNameTab;

	public Button BtnClose;

	public Animation UIAnimation;

	public Button btnLanguage;

	public Button btnHowtoPlay;

	public Button btnAboutUs;

	public UIUser_NameTab nameTab;

	public Button btn_Music;

	public Image img_Music;

	public TMP_Text txtm_Music;

	public Button btn_Sound;

	public Image img_Sound;

	public TMP_Text txtm_Sound;

	public Button btn_Vibration;

	public Image img_Vibration;

	public TMP_Text txtm_Vibration;

	public Button BtnGM;

	public TMP_InputField InputField;

	public Button btnRandom;

	public Button btnEdit;

	public Button btnConvert;

	public Button gp_bind_account;

	public Button ios_bind_account;

	public TMP_Text gp_bind_txt;

	public TMP_Text ios_bind_txt;

	public UIUser_NameTab UIUser_NameTab => ResolveNameTab();

	private UIUser_NameTab ResolveNameTab()
	{
		if ((Object)(object)cachedNameTab != (Object)null)
		{
			return cachedNameTab;
		}
		if ((Object)(object)nameTab != (Object)null)
		{
			cachedNameTab = nameTab;
			return cachedNameTab;
		}
		if ((Object)(object)((UIBaseView)this).Behaviour != (Object)null)
		{
			cachedNameTab = ((Component)((UIBaseView)this).Behaviour).GetComponentInChildren<UIUser_NameTab>(true);
		}
		return cachedNameTab;
	}

	public void NotifyPopupMaskStackActive(bool isActive)
	{
		UIBehaviour behaviour = ((UIBaseView)this).Behaviour;
		if (behaviour != null)
		{
			((Component)behaviour).GetComponent<UIPopupMaskFader>()?.OnUiStackActiveChanged(isActive);
		}
	}

	public void PlayOpenAnimation()
	{
		if (!((Object)(object)UIAnimation == (Object)null) && !((Object)(object)UIAnimation.GetClip("anim_popup_open") == (Object)null))
		{
			UIAnimation.Rewind("anim_popup_open");
			UIAnimation.Play("anim_popup_open");
		}
	}

	public void PlayCloseAnimation(Action onComplete = null)
	{
		if ((Object)(object)UIAnimation == (Object)null || (Object)(object)UIAnimation.GetClip("anim_popup_close") == (Object)null)
		{
			onComplete?.Invoke();
			return;
		}
		UIAnimation.Rewind("anim_popup_close");
		UIAnimation.Play("anim_popup_close");
		float length = UIAnimation.GetClip("anim_popup_close").length;
		if ((Object)(object)((UIBaseView)this).NodeRect != (Object)null)
		{
			GameApp.Timer.Delay((Object)(object)((UIBaseView)this).NodeRect, length, (Action)delegate
			{
				onComplete?.Invoke();
			});
		}
		else
		{
			GameApp.Timer.Delay(length, (Action)delegate
			{
				onComplete?.Invoke();
			});
		}
	}

	public void SetHeadInfoVariant(int index)
	{
		UIUser_NameTab uIUser_NameTab = ResolveNameTab();
		if (!((Object)(object)uIUser_NameTab == (Object)null) && index == 1)
		{
			uIUser_NameTab.SetSettingUserButtonActive(active: false);
		}
	}

	public void Init(SettingHomeInit arg)
	{
		SetUserNameTab(arg.PlayerName, arg.HeadSprite, arg.HeadFrameSprite);
	}

	public void SetMusicToggleVisual(bool isOn)
	{
		if ((Object)(object)img_Music != (Object)null)
		{
			((Component)img_Music).gameObject.SetActive(isOn);
		}
	}

	public void SetSoundToggleVisual(bool isOn)
	{
		if ((Object)(object)img_Sound != (Object)null)
		{
			((Component)img_Sound).gameObject.SetActive(isOn);
		}
	}

	public void SetVibrationToggleVisual(bool isOn)
	{
		if ((Object)(object)img_Vibration != (Object)null)
		{
			((Component)img_Vibration).gameObject.SetActive(isOn);
		}
	}

	public void SetAccountBindEntriesVisible(bool isGooglePlayPlatform, bool isIosPlatform)
	{
		if ((Object)(object)gp_bind_account != (Object)null)
		{
			((Component)gp_bind_account).gameObject.SetActive(isGooglePlayPlatform);
		}
		if ((Object)(object)ios_bind_account != (Object)null)
		{
			((Component)ios_bind_account).gameObject.SetActive(isIosPlatform);
		}
	}

	public void SetAccountBindStatus(bool isGooglePlayEntry, bool isBound)
	{
		TMP_Text val = (isGooglePlayEntry ? gp_bind_txt : ios_bind_txt);
		if (!((Object)(object)val == (Object)null))
		{
			if (isBound)
			{
				val.text = GameApp.Lang.GetText("Account_SignOut", Array.Empty<object>());
			}
			else
			{
				val.text = (isGooglePlayEntry ? GameApp.Lang.GetText("Account_SignIn_Google", Array.Empty<object>()) : GameApp.Lang.GetText("Account_SignIn_Apple", Array.Empty<object>()));
			}
		}
	}

	public void RefreshName(string playerName)
	{
		UIUser_NameTab uIUser_NameTab = ResolveNameTab();
		if ((Object)(object)uIUser_NameTab != (Object)null)
		{
			uIUser_NameTab.RefreshName(playerName);
		}
	}

	public void SetUserNameTab(string playerName, Sprite headSprite, Sprite headFrameSprite)
	{
		UIUser_NameTab uIUser_NameTab = ResolveNameTab();
		if ((Object)(object)uIUser_NameTab != (Object)null)
		{
			uIUser_NameTab.Refresh(playerName, headSprite, headFrameSprite);
		}
	}

	public void RefreshNameHead(Sprite headSprite)
	{
		UIUser_NameTab?.RefreshHead(headSprite);
	}

	public void RefreshNameFrame(Sprite frameSprite)
	{
		UIUser_NameTab?.RefreshFrame(frameSprite);
	}

	public override void Dispose()
	{
		cachedNameTab = null;
		((UIBaseView)this).Dispose();
	}

	public override void Inject(UIBehaviour behaviour)
	{
		((UIBaseView)this).Inject(behaviour);
		BtnClose = behaviour.Get<Button>("BtnClose");
		UIAnimation = behaviour.Get<Animation>("UIAnimation");
		btnLanguage = behaviour.Get<Button>("btnLanguage");
		btnHowtoPlay = behaviour.Get<Button>("btnHowtoPlay");
		btnAboutUs = behaviour.Get<Button>("btnAboutUs");
		nameTab = behaviour.Get<UIUser_NameTab>("nameTab");
		btn_Music = behaviour.Get<Button>("btn_Music");
		img_Music = behaviour.Get<Image>("img_Music");
		txtm_Music = behaviour.Get<TMP_Text>("txtm_Music");
		btn_Sound = behaviour.Get<Button>("btn_Sound");
		img_Sound = behaviour.Get<Image>("img_Sound");
		txtm_Sound = behaviour.Get<TMP_Text>("txtm_Sound");
		btn_Vibration = behaviour.Get<Button>("btn_Vibration");
		img_Vibration = behaviour.Get<Image>("img_Vibration");
		txtm_Vibration = behaviour.Get<TMP_Text>("txtm_Vibration");
		BtnGM = behaviour.Get<Button>("BtnGM");
		InputField = behaviour.Get<TMP_InputField>("InputField");
		btnRandom = behaviour.Get<Button>("btnRandom");
		btnEdit = behaviour.Get<Button>("btnEdit");
		btnConvert = behaviour.Get<Button>("btnConvert");
		gp_bind_account = behaviour.Get<Button>("gp_bind_account");
		ios_bind_account = behaviour.Get<Button>("ios_bind_account");
		gp_bind_txt = behaviour.Get<TMP_Text>("gp_bind_txt");
		ios_bind_txt = behaviour.Get<TMP_Text>("ios_bind_txt");
	}
}
