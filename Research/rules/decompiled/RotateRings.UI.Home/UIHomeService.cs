using UnityEngine;
using UnityEngine.UI;
using XingYun.UI.Controller;

namespace RotateRings.UI.Home;

public class UIHomeService
{
	public static RectTransform GetButtonLevelRT()
	{
		IController val = default(IController);
		if (GameApp.UI.IsOpen("UIHome", ref val) && val is UIHomeController uIHomeController)
		{
			UIHomeBtnOKView btnOKView = uIHomeController.View.BtnOKView;
			object obj;
			if (btnOKView == null)
			{
				obj = null;
			}
			else
			{
				Button btnOK = btnOKView.btnOK;
				obj = ((btnOK != null) ? ((Component)btnOK).transform : null);
			}
			return (RectTransform)((obj is RectTransform) ? obj : null);
		}
		return null;
	}

	public static bool LoadLevelButtonEffect()
	{
		IController val = default(IController);
		if (GameApp.UI.IsOpen("UIHome", ref val) && val is UIHomeController uIHomeController)
		{
			return uIHomeController.View.PlayLevelButtonEffect();
		}
		return false;
	}
}
