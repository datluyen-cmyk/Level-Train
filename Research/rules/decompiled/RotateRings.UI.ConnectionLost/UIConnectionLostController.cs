using System;
using Game.UI;
using UnityEngine.Events;
using UnityEngine.UI;
using XingYun.IAP.Core;
using XingYun.UI.Controller;
using XingYun.UI.Core;

namespace RotateRings.UI.ConnectionLost;

[UIPath("UIConnectionLost")]
public class UIConnectionLostController : GameBaseController<UIConnectionLostView>
{
	private const string Tag = "[UIConnectionLostController]";

	private XingYunIAPErrorCode errorCode;

	private Action onConfirm;

	private Action onCancel;

	private bool callbackHandled;

	private bool isClosing;

	public UIConnectionLostController(UIConnectionLostView view)
		: base(view)
	{
	}

	public override void OnRegister(object param = null)
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Expected O, but got Unknown
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Expected O, but got Unknown
		callbackHandled = false;
		isClosing = false;
		errorCode = (XingYunIAPErrorCode)9999;
		onConfirm = null;
		onCancel = null;
		((UnityEvent)((BaseController<UIConnectionLostView>)this).view.btnContinue.onClick).AddListener(new UnityAction(OnContinueClick));
		Button btnClose = ((BaseController<UIConnectionLostView>)this).view.btnClose;
		if (btnClose != null)
		{
			((UnityEvent)btnClose.onClick).AddListener(new UnityAction(OnCloseClick));
		}
		((BaseController<UIConnectionLostView>)this).view.Init();
		((BaseController<UIConnectionLostView>)this).view.PlayOpenAnimation();
		ParseParam(param);
		UpdateContent();
	}

	public override void OnRemove()
	{
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Expected O, but got Unknown
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Expected O, but got Unknown
		TriggerCancelOnce();
		((UnityEvent)((BaseController<UIConnectionLostView>)this).view.btnContinue.onClick).RemoveListener(new UnityAction(OnContinueClick));
		Button btnClose = ((BaseController<UIConnectionLostView>)this).view.btnClose;
		if (btnClose != null)
		{
			((UnityEvent)btnClose.onClick).RemoveListener(new UnityAction(OnCloseClick));
		}
		onConfirm = null;
		onCancel = null;
		isClosing = false;
		base.OnRemove();
	}

	public override void OnEscape()
	{
		OnContinueClick();
	}

	private void ParseParam(object param)
	{
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		if (param == null)
		{
			return;
		}
		if (!(param is ConnectionLostParam connectionLostParam))
		{
			if (param is XingYunIAPErrorCode val)
			{
				errorCode = val;
			}
		}
		else
		{
			errorCode = connectionLostParam.ErrorCode;
			onConfirm = connectionLostParam.OnConfirm;
			onCancel = connectionLostParam.OnCancel;
		}
	}

	private void UpdateContent()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Invalid comparison between Unknown and I4
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Invalid comparison between Unknown and I4
		XingYunIAPErrorCode val = errorCode;
		string titleKey;
		string descKey;
		string iconName;
		string fallbackIconName;
		if ((int)val != 4)
		{
			if (val - 2001 <= 1)
			{
				titleKey = "Shop_Purchase_NoInternet_Title";
				descKey = "Shop_Purchase_NoInternet_Desc";
				iconName = "icon_no_network";
				fallbackIconName = "icon_no_network";
			}
			else
			{
				titleKey = "Shop_Purchase_Failed_Title";
				descKey = "Shop_Purchase_Failed_Desc";
				iconName = "icon_payment_failed";
				fallbackIconName = "icon_payment_failed";
			}
		}
		else
		{
			titleKey = "Shop_Purchase_Canceled_Title";
			descKey = "Shop_Purchase_Canceled_Desc";
			iconName = "icon_cancel_order";
			fallbackIconName = "icon_cancel_order";
		}
		((BaseController<UIConnectionLostView>)this).view.SetPurchaseResultContent(titleKey, descKey, iconName, fallbackIconName);
	}

	private void OnContinueClick()
	{
		TriggerConfirmOnce();
		CloseViewHandler();
	}

	private void OnCloseClick()
	{
		TriggerCancelOnce();
		CloseViewHandler();
	}

	private void CloseViewHandler()
	{
		if (!isClosing)
		{
			isClosing = true;
			((BaseController<UIConnectionLostView>)this).view.PlayCloseAnimation(delegate
			{
				((BaseController<UIConnectionLostView>)this).CloseView(false);
			});
		}
	}

	private void TriggerConfirmOnce()
	{
		if (!callbackHandled)
		{
			callbackHandled = true;
			onConfirm?.Invoke();
		}
	}

	private void TriggerCancelOnce()
	{
		if (!callbackHandled)
		{
			callbackHandled = true;
			onCancel?.Invoke();
		}
	}
}
