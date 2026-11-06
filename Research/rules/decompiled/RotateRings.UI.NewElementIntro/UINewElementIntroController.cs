using Game.UI;
using RotateRings.Common;
using RotateRings.Services;
using UniLog;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using XingYun.UI.Controller;
using XingYun.UI.Core;

namespace RotateRings.UI.NewElementIntro;

[UIPath("UINewElementIntro")]
public class UINewElementIntroController : GameBaseController<UINewElementIntroView>
{
	private const string Tag = "NewElementIntro";

	private NewElementIntroOpenData openData;

	private double closeAllowedAt;

	private bool isClosing;

	private bool isUiLocked;

	public UINewElementIntroController(UINewElementIntroView view)
		: base(view)
	{
	}

	public override void OnRegister(object param = null)
	{
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Expected O, but got Unknown
		openData = param as NewElementIntroOpenData;
		isClosing = false;
		closeAllowedAt = Time.realtimeSinceStartupAsDouble + (double)NewElementIntroRuntimeRules.MinimumDisplayTimeSeconds;
		if (openData == null)
		{
			Log.LogError("NewElementIntro", "打开参数无效，关闭新元素介绍");
			((BaseController<UINewElementIntroView>)this).CloseView(false);
			return;
		}
		if ((Object)(object)((BaseController<UINewElementIntroView>)this).view.btn_Close != (Object)null)
		{
			((Selectable)((BaseController<UINewElementIntroView>)this).view.btn_Close).interactable = true;
			((UnityEvent)((BaseController<UINewElementIntroView>)this).view.btn_Close.onClick).AddListener(new UnityAction(OnCloseClick));
		}
		LockUIService.Enable(float.MaxValue);
		isUiLocked = true;
		((BaseController<UINewElementIntroView>)this).view.Init(openData);
		AudioService.PlayUiSound(AudioKeys.NewItem.Value);
	}

	private void OnCloseClick()
	{
		if (!isClosing && openData != null && !(Time.realtimeSinceStartupAsDouble < closeAllowedAt))
		{
			isClosing = true;
			if ((Object)(object)((BaseController<UINewElementIntroView>)this).view.btn_Close != (Object)null)
			{
				((Selectable)((BaseController<UINewElementIntroView>)this).view.btn_Close).interactable = false;
			}
			((BaseController<UINewElementIntroView>)this).view.PlayCloseAnimation(CompleteClose);
		}
	}

	private void CompleteClose()
	{
		if (isClosing && openData != null)
		{
			NewElementIntroProxy newElementIntroProxy = GameApp.Proxy.Get<NewElementIntroProxy>();
			if (newElementIntroProxy != null && newElementIntroProxy.MarkIntroduced(openData.ElementId))
			{
				GameApp.Save.SaveAll();
			}
			((BaseController<UINewElementIntroView>)this).CloseView(false);
		}
	}

	public override void OnRemove()
	{
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Expected O, but got Unknown
		if ((Object)(object)((BaseController<UINewElementIntroView>)this).view.btn_Close != (Object)null)
		{
			((UnityEvent)((BaseController<UINewElementIntroView>)this).view.btn_Close.onClick).RemoveListener(new UnityAction(OnCloseClick));
		}
		if (isUiLocked)
		{
			isUiLocked = false;
			LockUIService.Disable();
		}
		NewElementIntroService.NotifyPopupRemoved();
		base.OnRemove();
	}

	public override void OnActive(bool isActive)
	{
		((BaseController<UINewElementIntroView>)this).OnActive(isActive);
	}

	public override void OnEscape()
	{
		OnCloseClick();
	}
}
