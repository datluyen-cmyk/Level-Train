using System;
using Game.UI;
using UnityEngine;
using UnityEngine.Events;
using XingYun.UI.Controller;
using XingYun.UI.Core;

namespace RotateRings.UI.Setting;

[UIPath("UILoginSignInWindow")]
public class UILoginSignInController : GameBaseController<UILoginSignInView>
{
	private const string LogTag = "[UI][UILoginSignInController]";

	private const string SuccessTitleLangKey = "Account_Link_Success_Title";

	private const string SuccessDescriptionLangKey = "Account_Link_Success_Desc";

	private const string FailedTitleLangKey = "Account_Link_Failed_Title";

	private const string FailedDescriptionLangKey = "Account_Link_Failed_Desc";

	private const string SignOutSuccessTitleLangKey = "Account_SignOut_Success_Title";

	private const string SignOutSuccessDescriptionLangKey = "Account_SignOut_Success_Desc";

	private const string DeleteSuccessTitleLangKey = "Account_Delete_Success_Title";

	private const string DeleteSuccessDescriptionLangKey = "Account_Delete_Success_Desc";

	private const string ContinueButtonLangKey = "Button_Continue";

	private const string RetryButtonLangKey = "Common_Retry";

	private UILoginSignInParam openParam;

	private bool isClosing;

	public UILoginSignInController(UILoginSignInView view)
		: base(view)
	{
	}

	public override void OnRegister(object param = null)
	{
		//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d0: Expected O, but got Unknown
		//IL_00e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f1: Expected O, but got Unknown
		openParam = (param as UILoginSignInParam) ?? new UILoginSignInParam(UILoginSignInResultType.LinkSuccess);
		isClosing = false;
		switch (openParam.ResultType)
		{
		case UILoginSignInResultType.LinkFailed:
			((BaseController<UILoginSignInView>)this).view.SetLocalizedContent("Account_Link_Failed_Title", "Account_Link_Failed_Desc", "Common_Retry");
			break;
		case UILoginSignInResultType.SignOutSuccess:
			((BaseController<UILoginSignInView>)this).view.SetLocalizedContent("Account_SignOut_Success_Title", "Account_SignOut_Success_Desc", "Button_Continue");
			break;
		case UILoginSignInResultType.DeleteSuccess:
			((BaseController<UILoginSignInView>)this).view.SetLocalizedContent("Account_Delete_Success_Title", "Account_Delete_Success_Desc", "Button_Continue");
			break;
		default:
			((BaseController<UILoginSignInView>)this).view.SetLocalizedContent("Account_Link_Success_Title", "Account_Link_Success_Desc", "Button_Continue");
			break;
		}
		((UnityEvent)((BaseController<UILoginSignInView>)this).view.Btn_Close.onClick).AddListener(new UnityAction(OnCloseClick));
		((UnityEvent)((BaseController<UILoginSignInView>)this).view.Btn_Accept.onClick).AddListener(new UnityAction(OnAcceptClick));
		((BaseController<UILoginSignInView>)this).view.PlayOpenAnimation();
	}

	private void OnCloseClick()
	{
		UILoginSignInParam uILoginSignInParam = openParam;
		Action onClosed = ((uILoginSignInParam != null && uILoginSignInParam.ResultType == UILoginSignInResultType.LinkFailed) ? openParam.CancelAction : null);
		CloseViewWithAnimation(onClosed);
	}

	private void OnAcceptClick()
	{
		UILoginSignInParam uILoginSignInParam = openParam;
		if (uILoginSignInParam != null && uILoginSignInParam.ResultType == UILoginSignInResultType.LinkFailed && openParam.RetryAction != null)
		{
			Action retryAction = openParam.RetryAction;
			CloseViewWithAnimation(retryAction);
		}
		else
		{
			CloseViewWithAnimation();
		}
	}

	private void CloseViewWithAnimation(Action onClosed = null)
	{
		if (isClosing)
		{
			return;
		}
		if (((BaseController<UILoginSignInView>)this).view == null || (Object)(object)((UIBaseView)((BaseController<UILoginSignInView>)this).view).Behaviour == (Object)null)
		{
			onClosed?.Invoke();
			return;
		}
		isClosing = true;
		((BaseController<UILoginSignInView>)this).view.PlayCloseAnimation(delegate
		{
			((BaseController<UILoginSignInView>)this).CloseView(false);
			onClosed?.Invoke();
		});
	}

	public override void OnRemove()
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Expected O, but got Unknown
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Expected O, but got Unknown
		((UnityEvent)((BaseController<UILoginSignInView>)this).view.Btn_Close.onClick).RemoveListener(new UnityAction(OnCloseClick));
		((UnityEvent)((BaseController<UILoginSignInView>)this).view.Btn_Accept.onClick).RemoveListener(new UnityAction(OnAcceptClick));
		openParam = null;
		base.OnRemove();
	}

	public override void OnActive(bool isActive)
	{
		((BaseController<UILoginSignInView>)this).OnActive(isActive);
	}

	public override void OnEscape()
	{
		UILoginSignInParam uILoginSignInParam = openParam;
		Action onClosed = ((uILoginSignInParam != null && uILoginSignInParam.ResultType == UILoginSignInResultType.LinkFailed) ? openParam.CancelAction : null);
		CloseViewWithAnimation(onClosed);
	}
}
