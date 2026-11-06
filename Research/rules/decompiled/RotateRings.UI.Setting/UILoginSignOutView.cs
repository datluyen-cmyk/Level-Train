using System;
using Game.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using XingYun.Extensions;
using XingYun.UI.Core;

namespace RotateRings.UI.Setting;

public class UILoginSignOutView : GameBaseView
{
	private Animation uiAnimation;

	public Button Btn_Close;

	public TMP_Text TxtM_Text;

	public Button Btn_SignOut;

	public Button Btn_Cancel;

	public TMP_Text titleTxt;

	public void SetLocalizedContent(string titleKey, string descriptionKey)
	{
		TMP_Text obj = titleTxt;
		if (obj != null)
		{
			TMPTextExtensions.SetLocalizedText(obj, titleKey, Array.Empty<object>());
		}
		TMP_Text txtM_Text = TxtM_Text;
		if (txtM_Text != null)
		{
			TMPTextExtensions.SetLocalizedText(txtM_Text, descriptionKey, Array.Empty<object>());
		}
	}

	public void PlayOpenAnimation()
	{
		UILoginPopupAnimationHelper.PlayOpen(ResolveUIAnimation());
	}

	public void PlayCloseAnimation(Action onComplete = null)
	{
		UILoginPopupAnimationHelper.PlayClose(ResolveUIAnimation(), ((UIBaseView)this).NodeRect, onComplete);
	}

	private Animation ResolveUIAnimation()
	{
		if ((Object)(object)uiAnimation != (Object)null)
		{
			return uiAnimation;
		}
		uiAnimation = UILoginPopupAnimationHelper.ResolveAnimation(((UIBaseView)this).Behaviour);
		return uiAnimation;
	}

	public override void Inject(UIBehaviour behaviour)
	{
		((UIBaseView)this).Inject(behaviour);
		Btn_Close = behaviour.Get<Button>("Btn_Close");
		TxtM_Text = behaviour.Get<TMP_Text>("TxtM_Text");
		Btn_SignOut = behaviour.Get<Button>("Btn_SignOut");
		Btn_Cancel = behaviour.Get<Button>("Btn_Cancel");
		titleTxt = behaviour.Get<TMP_Text>("titleTxt");
	}
}
