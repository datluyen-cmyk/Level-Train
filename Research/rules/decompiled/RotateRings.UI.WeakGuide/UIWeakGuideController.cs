using System;
using Game.UI;
using UnityEngine;
using UnityEngine.UI;
using XingYun.Extensions;
using XingYun.UI.Controller;
using XingYun.UI.Core;

namespace RotateRings.UI.WeakGuide;

[UIPath("WeakGuideView")]
public class UIWeakGuideController : GameBaseController<UIWeakGuideView>
{
	public UIWeakGuideController(UIWeakGuideView view)
		: base(view)
	{
	}

	public override void OnRegister(object param = null)
	{
	}

	public override void OnActive(bool isActive)
	{
		((BaseController<UIWeakGuideView>)this).OnActive(isActive);
		if (isActive && ((BaseController<UIWeakGuideView>)this).view != null && (Object)(object)((BaseController<UIWeakGuideView>)this).view.layout != (Object)null && ((Component)((BaseController<UIWeakGuideView>)this).view.layout).gameObject.activeInHierarchy)
		{
			((BaseController<UIWeakGuideView>)this).view.PlayOpenAnimation();
		}
	}

	public override void OnRemove()
	{
		base.OnRemove();
	}

	public override void OnEscape()
	{
	}

	public void PlayOpenAnimation()
	{
		((BaseController<UIWeakGuideView>)this).view?.PlayOpenAnimation();
	}

	public void PlayCloseAnimation()
	{
		((BaseController<UIWeakGuideView>)this).view?.PlayCloseAnimation();
	}

	public float GetCloseAnimationDuration()
	{
		if (((BaseController<UIWeakGuideView>)this).view == null)
		{
			return 0f;
		}
		return ((BaseController<UIWeakGuideView>)this).view.GetClipDuration("anim_SingeToast_close");
	}

	public void SetGuideText(string key)
	{
		if ((Object)(object)((BaseController<UIWeakGuideView>)this).view?.txtGuide == (Object)null || string.IsNullOrEmpty(key))
		{
			return;
		}
		TMPTextExtensions.SetLocalizedText(((BaseController<UIWeakGuideView>)this).view.txtGuide, key, Array.Empty<object>());
		if (!string.IsNullOrEmpty(GameApp.Lang.GetText(key, Array.Empty<object>())))
		{
			if ((Object)(object)((BaseController<UIWeakGuideView>)this).view.layout != (Object)null && !((Component)((BaseController<UIWeakGuideView>)this).view.layout).gameObject.activeSelf)
			{
				((Component)((BaseController<UIWeakGuideView>)this).view.layout).gameObject.SetActive(true);
				((BaseController<UIWeakGuideView>)this).view.PlayOpenAnimation();
			}
			ForceRefreshLayout();
		}
	}

	private void ForceRefreshLayout()
	{
		if ((Object)(object)((BaseController<UIWeakGuideView>)this).view?.layout != (Object)null)
		{
			Transform transform = ((Component)((BaseController<UIWeakGuideView>)this).view.layout).transform;
			RectTransform val = (RectTransform)(object)((transform is RectTransform) ? transform : null);
			if ((Object)(object)val != (Object)null)
			{
				LayoutRebuilder.ForceRebuildLayoutImmediate(val);
			}
		}
	}
}
