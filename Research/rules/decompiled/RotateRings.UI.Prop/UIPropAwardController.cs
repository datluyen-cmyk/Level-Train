using System;
using Game.UI;
using RotateRings.Common;
using RotateRings.Services;
using UnityEngine.Events;
using XingYun.UI.Controller;
using XingYun.UI.Core;

namespace RotateRings.UI.Prop;

[UIPath("UIPropAward")]
public class UIPropAwardController : GameBaseController<UIPropAwardView>
{
	private const string Tag = "UIPropAward";

	private UIPropAwardParam openParam;

	private bool handledClose;

	public UIPropAwardController(UIPropAwardView view)
		: base(view)
	{
	}

	public override void OnRegister(object param = null)
	{
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Expected O, but got Unknown
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Expected O, but got Unknown
		openParam = param as UIPropAwardParam;
		handledClose = false;
		if (openParam == null)
		{
			((BaseController<UIPropAwardView>)this).CloseView(false);
			return;
		}
		((BaseController<UIPropAwardView>)this).view.BindPresentation(openParam.PropType, openParam.GrantCount);
		((BaseController<UIPropAwardView>)this).view.SetButtonsInteractable(interactable: true);
		((UnityEvent)((BaseController<UIPropAwardView>)this).view.Btn_Close.onClick).AddListener(new UnityAction(OnCloseClick));
		((UnityEvent)((BaseController<UIPropAwardView>)this).view.Btn_Accept.onClick).AddListener(new UnityAction(OnAcceptClick));
	}

	private void OnCloseClick()
	{
		if (!handledClose)
		{
			handledClose = true;
			AudioService.PlayUiSound(AudioKeys.BtnGroupClick.Value);
			openParam?.OnClose?.Invoke();
			((BaseController<UIPropAwardView>)this).CloseView(false);
		}
	}

	private void OnAcceptClick()
	{
		if (!handledClose)
		{
			handledClose = true;
			((BaseController<UIPropAwardView>)this).view.SetButtonsInteractable(interactable: false);
			AudioService.PlayUiSound(AudioKeys.BtnGroupClick.Value);
			Action obj = openParam?.OnAccept;
			((BaseController<UIPropAwardView>)this).CloseView(false);
			obj?.Invoke();
		}
	}

	public override void OnActive(bool isActive)
	{
		((BaseController<UIPropAwardView>)this).OnActive(isActive);
		((BaseController<UIPropAwardView>)this).view.OnUiStackActiveChanged(isActive);
	}

	public override void OnRemove()
	{
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Expected O, but got Unknown
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Expected O, but got Unknown
		if (!handledClose)
		{
			handledClose = true;
			openParam?.OnClose?.Invoke();
		}
		((UnityEvent)((BaseController<UIPropAwardView>)this).view.Btn_Close.onClick).RemoveListener(new UnityAction(OnCloseClick));
		((UnityEvent)((BaseController<UIPropAwardView>)this).view.Btn_Accept.onClick).RemoveListener(new UnityAction(OnAcceptClick));
		base.OnRemove();
	}

	public override void OnEscape()
	{
		OnCloseClick();
	}
}
