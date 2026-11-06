using System;
using XingYun.UI.Controller;
using XingYun.UI.Core;

namespace RotateRings.UI.Toast;

public static class ToastService
{
	public static void Show(string content, EToastType toastType = EToastType.Single)
	{
		Open(new UIToastOpenParam
		{
			Content = content,
			ToastType = toastType
		});
	}

	public static void Show(string content, ToastVerticalAlign verticalAlign, float offset, EToastType toastType = EToastType.Single)
	{
		Open(new UIToastOpenParam
		{
			Content = content,
			ToastType = toastType,
			VerticalAlign = verticalAlign,
			Offset = offset
		});
	}

	public static void Open(UIToastOpenParam param)
	{
		if (param != null)
		{
			IController val = default(IController);
			if (GameApp.UI.IsOpen("UIToast", ref val) && val is UIToastController uIToastController)
			{
				uIToastController.ShowInstance(param);
			}
			else
			{
				GameApp.UI.Open("UIToast", (UILayer)4, (object)param, (Action)null);
			}
		}
	}
}
