using System;
using Game.UI;
using RotateRings.Services;
using UniLog;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using XingYun.UI.Controller;
using XingYun.UI.Core;

namespace RotateRings.UI.Setting;

[UIPath("UILoginSelectAccount")]
public class UILoginSelectAccountController : GameBaseController<UILoginSelectAccountView>
{
	private const string LogTag = "[UI][UILoginSelectAccountController]";

	private UILoginSelectAccountParam openParam;

	private CloudSaveSelectionSource selectedSource;

	private bool isClosing;

	public UILoginSelectAccountController(UILoginSelectAccountView view)
		: base(view)
	{
	}

	public override void OnRegister(object param = null)
	{
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a6: Expected O, but got Unknown
		//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cd: Expected O, but got Unknown
		//IL_00ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f4: Expected O, but got Unknown
		//IL_0111: Unknown result type (might be due to invalid IL or missing references)
		//IL_011b: Expected O, but got Unknown
		openParam = param as UILoginSelectAccountParam;
		selectedSource = CloudSaveSelectionSource.Cloud;
		isClosing = false;
		if (openParam == null || openParam.CloudSnapshot == null || openParam.LocalSnapshot == null)
		{
			Log.LogError("[UI][UILoginSelectAccountController]", "存档选择参数无效，无法展示云端与本地存档");
			((BaseController<UILoginSelectAccountView>)this).CloseView(false);
			return;
		}
		((BaseController<UILoginSelectAccountView>)this).view.SetAccountPreviews(openParam.CloudSnapshot, openParam.LocalSnapshot);
		((BaseController<UILoginSelectAccountView>)this).view.SetCurrentAccountSelected();
		Button btn_Close = ((BaseController<UILoginSelectAccountView>)this).view.Btn_Close;
		if (btn_Close != null)
		{
			((UnityEvent)btn_Close.onClick).AddListener(new UnityAction(OnCloseClick));
		}
		Button continueBtn = ((BaseController<UILoginSelectAccountView>)this).view.continueBtn;
		if (continueBtn != null)
		{
			((UnityEvent)continueBtn.onClick).AddListener(new UnityAction(OnContinueClick));
		}
		Button bindBtn = ((BaseController<UILoginSelectAccountView>)this).view.bindBtn1;
		if (bindBtn != null)
		{
			((UnityEvent)bindBtn.onClick).AddListener(new UnityAction(OnBindBtn1Click));
		}
		Button bindBtn2 = ((BaseController<UILoginSelectAccountView>)this).view.bindBtn2;
		if (bindBtn2 != null)
		{
			((UnityEvent)bindBtn2.onClick).AddListener(new UnityAction(OnBindBtn2Click));
		}
		((BaseController<UILoginSelectAccountView>)this).view.PlayOpenAnimation();
	}

	private void OnCloseClick()
	{
		Action onClosed = openParam?.CancelAction;
		CloseViewWithAnimation(onClosed);
	}

	private void OnContinueClick()
	{
		if (openParam != null)
		{
			CloudSaveDataSnapshot previewSnapshot = ((selectedSource == CloudSaveSelectionSource.Cloud) ? openParam.CloudSnapshot : openParam.LocalSnapshot);
			GameApp.UI.Open("UILoginSaveAccount", (UILayer)2, (object)new UILoginSaveAccountParam(selectedSource, previewSnapshot, openParam.CloudSnapshot, openParam.CompleteAction), (Action)null);
		}
	}

	private void OnBindBtn1Click()
	{
		selectedSource = CloudSaveSelectionSource.Cloud;
		((BaseController<UILoginSelectAccountView>)this).view.SetAccountSelected(isFirstAccount: true);
	}

	private void OnBindBtn2Click()
	{
		selectedSource = CloudSaveSelectionSource.Local;
		((BaseController<UILoginSelectAccountView>)this).view.SetAccountSelected(isFirstAccount: false);
	}

	private void CloseViewWithAnimation(Action onClosed = null)
	{
		if (isClosing)
		{
			return;
		}
		if (((BaseController<UILoginSelectAccountView>)this).view == null || (Object)(object)((UIBaseView)((BaseController<UILoginSelectAccountView>)this).view).Behaviour == (Object)null)
		{
			onClosed?.Invoke();
			return;
		}
		isClosing = true;
		((BaseController<UILoginSelectAccountView>)this).view.PlayCloseAnimation(delegate
		{
			((BaseController<UILoginSelectAccountView>)this).CloseView(false);
			onClosed?.Invoke();
		});
	}

	public override void OnRemove()
	{
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Expected O, but got Unknown
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Expected O, but got Unknown
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Expected O, but got Unknown
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Expected O, but got Unknown
		Button btn_Close = ((BaseController<UILoginSelectAccountView>)this).view.Btn_Close;
		if (btn_Close != null)
		{
			((UnityEvent)btn_Close.onClick).RemoveListener(new UnityAction(OnCloseClick));
		}
		Button continueBtn = ((BaseController<UILoginSelectAccountView>)this).view.continueBtn;
		if (continueBtn != null)
		{
			((UnityEvent)continueBtn.onClick).RemoveListener(new UnityAction(OnContinueClick));
		}
		Button bindBtn = ((BaseController<UILoginSelectAccountView>)this).view.bindBtn1;
		if (bindBtn != null)
		{
			((UnityEvent)bindBtn.onClick).RemoveListener(new UnityAction(OnBindBtn1Click));
		}
		Button bindBtn2 = ((BaseController<UILoginSelectAccountView>)this).view.bindBtn2;
		if (bindBtn2 != null)
		{
			((UnityEvent)bindBtn2.onClick).RemoveListener(new UnityAction(OnBindBtn2Click));
		}
		openParam = null;
		base.OnRemove();
	}

	public override void OnEscape()
	{
		Action onClosed = openParam?.CancelAction;
		CloseViewWithAnimation(onClosed);
	}
}
