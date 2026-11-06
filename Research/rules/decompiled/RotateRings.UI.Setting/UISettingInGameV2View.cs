using System;
using RotateRings.Common;
using Rotaterings.Tools;
using TMPro;
using UniLog;
using UnityEngine;
using UnityEngine.UI;
using XingYun.UI.Core;

namespace RotateRings.UI.Setting;

public class UISettingInGameV2View : UIBaseView
{
	private const string OpenAnimName = "anim_popup_open";

	private const string CloseAnimName = "anim_popup_close";

	public Animation UIAnimation;

	public Button BtnClose;

	public CanvasGroup BGMaskCanvasGroup;

	public CanvasGroup UIPanelCanvasGroup;

	public Button BtnGM;

	public Button bgMask;

	public Button ButtonHome;

	public Button ButtonReplay;

	public Button btn_Music;

	public Image img_Music;

	public TextMeshProUGUI txtm_Music;

	public Button btn_Sound;

	public Image img_Sound;

	public TextMeshProUGUI txtm_Sound;

	public Button btn_Vibration;

	public Image img_Vibration;

	public TextMeshProUGUI txtm_Vibration;

	public Button btn_HowtoPlay;

	public Button btn_Language;

	public void NotifyPopupMaskStackActive(bool isActive)
	{
		UIBehaviour behaviour = ((UIBaseView)this).Behaviour;
		if (behaviour != null)
		{
			((Component)behaviour).GetComponent<UIPopupMaskFader>()?.OnUiStackActiveChanged(isActive);
		}
	}

	public void SetCanvasGroupZero()
	{
		if ((Object)(object)BGMaskCanvasGroup != (Object)null)
		{
			BGMaskCanvasGroup.alpha = 0f;
		}
		if ((Object)(object)UIPanelCanvasGroup != (Object)null)
		{
			UIPanelCanvasGroup.alpha = 0f;
		}
	}

	public void PlayOpenAnimation()
	{
		UIAnimUtil.PlayUIOpenAnimation(UIAnimation, "anim_popup_open");
	}

	public void PlayCloseAnimation(Action onComplete = null)
	{
		if ((Object)(object)UIAnimation == (Object)null || (Object)(object)UIAnimation.GetClip("anim_popup_close") == (Object)null)
		{
			if ((Object)(object)UIAnimation != (Object)null)
			{
				Log.LogError("UI", "[UISettingInGameV2View] 动画 anim_popup_close 不存在");
			}
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

	public override void Dispose()
	{
		((UIBaseView)this).Dispose();
	}

	public override void Inject(UIBehaviour behaviour)
	{
		((UIBaseView)this).Inject(behaviour);
		UIAnimation = behaviour.Get<Animation>("UIAnimation");
		BtnClose = behaviour.Get<Button>("BtnClose");
		BGMaskCanvasGroup = behaviour.Get<CanvasGroup>("BGMaskCanvasGroup");
		UIPanelCanvasGroup = behaviour.Get<CanvasGroup>("UIPanelCanvasGroup");
		BtnGM = behaviour.Get<Button>("BtnGM");
		bgMask = behaviour.Get<Button>("bgMask");
		ButtonHome = behaviour.Get<Button>("ButtonHome");
		ButtonReplay = behaviour.Get<Button>("ButtonReplay");
		btn_Music = behaviour.Get<Button>("btn_Music");
		img_Music = behaviour.Get<Image>("img_Music");
		txtm_Music = behaviour.Get<TextMeshProUGUI>("txtm_Music");
		btn_Sound = behaviour.Get<Button>("btn_Sound");
		img_Sound = behaviour.Get<Image>("img_Sound");
		txtm_Sound = behaviour.Get<TextMeshProUGUI>("txtm_Sound");
		btn_Vibration = behaviour.Get<Button>("btn_Vibration");
		img_Vibration = behaviour.Get<Image>("img_Vibration");
		txtm_Vibration = behaviour.Get<TextMeshProUGUI>("txtm_Vibration");
		btn_HowtoPlay = behaviour.Get<Button>("btn_HowtoPlay");
		btn_Language = behaviour.Get<Button>("btn_Language");
	}
}
