using System;
using Game.UI;
using RotateRings.Common;
using RotateRings.Services;
using UnityEngine.Events;
using XingYun.UI.Controller;
using XingYun.UI.Core;

namespace RotateRings.UI.RestartRetention;

[UIPath("UIRestartRetention")]
public class UIRestartRetentionController : GameBaseController<UIRestartRetentionView>
{
	private RestartRetentionRequest request;

	private bool clicked;

	private bool isClosing;

	private bool isRemoved;

	public UIRestartRetentionController(UIRestartRetentionView view)
		: base(view)
	{
	}

	public override void OnRegister(object param = null)
	{
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Expected O, but got Unknown
		//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b6: Expected O, but got Unknown
		request = (param as RestartRetentionRequest) ?? RestartRetentionFlow.CreateRequest(null, preview: true);
		request.Registered = false;
		clicked = false;
		isClosing = false;
		isRemoved = false;
		((BaseController<UIRestartRetentionView>)this).view.InitializePresentation();
		((BaseController<UIRestartRetentionView>)this).view.Show(RestartRetentionPresentation.Build(request.Decision, ((BaseController<UIRestartRetentionView>)this).view.HighlightColor, ((BaseController<UIRestartRetentionView>)this).view.LeaderboardAvailable));
		((UnityEvent)((BaseController<UIRestartRetentionView>)this).view.btn_Continue.onClick).AddListener(new UnityAction(OnContinueClick));
		((UnityEvent)((BaseController<UIRestartRetentionView>)this).view.btn_Restart.onClick).AddListener(new UnityAction(OnRestartClick));
		((BaseController<UIRestartRetentionView>)this).view.PlayOpenAnimation();
		request.Registered = true;
	}

	public bool OwnsRequest(RestartRetentionRequest expectedRequest)
	{
		if (expectedRequest != null && !isRemoved)
		{
			return request == expectedRequest;
		}
		return false;
	}

	private void OnContinueClick()
	{
		if (!clicked)
		{
			clicked = true;
			((BaseController<UIRestartRetentionView>)this).view.SetButtonsInteractable(interactable: false);
			AudioService.PlayUiSound(AudioKeys.BtnGroupClick.Value);
			RestartRetentionFlow.Cancel(request);
		}
	}

	public override void OnActive(bool isActive)
	{
		((BaseController<UIRestartRetentionView>)this).OnActive(isActive);
		((BaseController<UIRestartRetentionView>)this).view.OnUiStackActiveChanged(isActive);
	}

	private void OnRestartClick()
	{
		if (!clicked)
		{
			clicked = true;
			((BaseController<UIRestartRetentionView>)this).view.SetButtonsInteractable(interactable: false);
			AudioService.PlayUiSound(AudioKeys.BtnGroupClick.Value);
			RestartRetentionFlow.Confirm(request);
		}
	}

	public void PlayCloseAnimation(RestartRetentionRequest expectedRequest, Action onComplete)
	{
		if (isRemoved || request != expectedRequest)
		{
			RestartRetentionFlow.Release(expectedRequest);
		}
		else
		{
			if (isClosing)
			{
				return;
			}
			isClosing = true;
			clicked = true;
			((BaseController<UIRestartRetentionView>)this).view.SetButtonsInteractable(interactable: false);
			((BaseController<UIRestartRetentionView>)this).view.PlayCloseAnimation(delegate
			{
				if (!isRemoved && !expectedRequest.Handled)
				{
					onComplete?.Invoke();
				}
			});
		}
	}

	public override void OnRemove()
	{
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Expected O, but got Unknown
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Expected O, but got Unknown
		isRemoved = true;
		((UnityEvent)((BaseController<UIRestartRetentionView>)this).view.btn_Continue.onClick).RemoveListener(new UnityAction(OnContinueClick));
		((UnityEvent)((BaseController<UIRestartRetentionView>)this).view.btn_Restart.onClick).RemoveListener(new UnityAction(OnRestartClick));
		if (!request.Handled)
		{
			RestartRetentionFlow.Release(request);
		}
		base.OnRemove();
	}

	public override void OnEscape()
	{
		OnContinueClick();
	}
}
