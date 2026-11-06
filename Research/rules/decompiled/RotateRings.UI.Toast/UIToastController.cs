using System;
using Game.UI;
using XingYun.UI.Controller;
using XingYun.UI.Core;

namespace RotateRings.UI.Toast;

[UIPath("UIToast")]
public class UIToastController : GameBaseController<UIToastView>
{
	private bool isClosing;

	private UIToastOpenParam currentParam;

	public UIToastController(UIToastView view)
		: base(view)
	{
	}

	public override void OnRegister(object param = null)
	{
		isClosing = false;
		currentParam = null;
		if (!(param is UIToastOpenParam param2))
		{
			CloseToast();
		}
		else
		{
			ShowInstance(param2);
		}
	}

	public void ShowInstance(UIToastOpenParam param)
	{
		if (param != null && !isClosing)
		{
			currentParam = param;
			((BaseController<UIToastView>)this).view.ShowToast(param, CloseToast);
		}
	}

	public void RefreshVisibleText()
	{
		((BaseController<UIToastView>)this).view.RefreshVisibleText();
	}

	public override void OnRemove()
	{
		isClosing = true;
		currentParam = null;
		((BaseController<UIToastView>)this).view.ForceCleanup();
		base.OnRemove();
	}

	public override void OnActive(bool isActive)
	{
		((BaseController<UIToastView>)this).OnActive(isActive);
		if (!isActive)
		{
			CloseToast();
		}
	}

	public override void OnEscape()
	{
		Action action = currentParam?.OnEscape;
		if (action != null)
		{
			action();
		}
		else
		{
			CloseToast();
		}
	}

	private void CloseToast()
	{
		if (!isClosing)
		{
			isClosing = true;
			Action obj = currentParam?.OnDismissed;
			currentParam = null;
			obj?.Invoke();
			((BaseController<UIToastView>)this).CloseView(false);
		}
	}
}
