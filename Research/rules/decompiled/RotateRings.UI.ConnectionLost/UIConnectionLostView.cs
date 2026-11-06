using System;
using Game.UI;
using RotateRings.Common;
using Script.Tools;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using XingYun.Extensions;
using XingYun.UI.Core;

namespace RotateRings.UI.ConnectionLost;

public class UIConnectionLostView : GameBaseView
{
	private const string OpenAnimName = "anim_popup_open";

	private const string CloseAnimName = "anim_popup_close";

	public RectTransform Root;

	public Animation UIAnimation;

	public Button btnContinue;

	public TMP_Text txtTitle;

	public Button btnClose;

	public Image imgIcon;

	public TMP_Text txtContent;

	public void Init()
	{
		if ((Object)(object)Root != (Object)null)
		{
			((Component)Root).gameObject.SetActive(true);
		}
	}

	public void PlayOpenAnimation()
	{
		UIAnimUtil.PlayUIOpenAnimation(UIAnimation, "anim_popup_open");
	}

	public void PlayCloseAnimation(Action onComplete)
	{
		if ((Object)(object)UIAnimation == (Object)null || (Object)(object)UIAnimation.GetClip("anim_popup_close") == (Object)null)
		{
			onComplete?.Invoke();
			return;
		}
		UIAnimation.Stop();
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

	public void SetPurchaseResultContent(string titleKey, string descKey, string iconName, string fallbackIconName)
	{
		TMP_Text obj = txtTitle;
		if (obj != null)
		{
			TMPTextExtensions.SetLocalizedText(obj, titleKey, Array.Empty<object>());
		}
		TMP_Text obj2 = txtContent;
		if (obj2 != null)
		{
			TMPTextExtensions.SetLocalizedText(obj2, descKey, Array.Empty<object>());
		}
		if ((Object)(object)imgIcon != (Object)null && !string.IsNullOrEmpty(iconName))
		{
			imgIcon.SetAtlasSprite("Shop", iconName);
			((Graphic)imgIcon).SetNativeSize();
		}
	}

	public override void Dispose()
	{
		((UIBaseView)this).Dispose();
	}

	public override void Inject(UIBehaviour behaviour)
	{
		((UIBaseView)this).Inject(behaviour);
		Root = behaviour.Get<RectTransform>("Root");
		UIAnimation = behaviour.Get<Animation>("UIAnimation");
		btnContinue = behaviour.Get<Button>("btnContinue");
		txtTitle = behaviour.Get<TMP_Text>("txtTitle");
		btnClose = behaviour.Get<Button>("btnClose");
		imgIcon = behaviour.Get<Image>("imgIcon");
		txtContent = behaviour.Get<TMP_Text>("txtContent");
	}
}
