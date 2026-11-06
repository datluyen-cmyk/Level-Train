using System;
using Example.Cfg;
using Game.DataTrack;
using Game.UI;
using RotateRings.Ad;
using RotateRings.Common;
using RotateRings.Services;
using UnityEngine;
using UnityEngine.Events;
using XingYun.UI.Controller;

namespace RotateRings.UI.RewardedLuckyDraw;

public abstract class UIRewardedLuckyDrawController<TView> : GameBaseController<TView> where TView : GameBaseView, IRewardedLuckyDrawView
{
	private const string LuckyWheelAudioVariantId = "variant_507793e1";

	private RewardedLuckyDrawRequest request;

	private bool removed;

	private bool busy;

	private bool closing;

	private Action closeCallbacks;

	protected UIRewardedLuckyDrawController(TView view)
		: base(view)
	{
	}

	public override void OnRegister(object param = null)
	{
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Expected O, but got Unknown
		//IL_00c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ce: Expected O, but got Unknown
		request = param as RewardedLuckyDrawRequest;
		removed = false;
		busy = true;
		closing = false;
		closeCallbacks = null;
		if (request == null || !request.Valid)
		{
			((BaseController<TView>)this).CloseView(false);
			return;
		}
		((BaseController<TView>)this).view.InitializePresentation(request.Slots, GetPropIconPaths(request.Slots));
		((BaseController<TView>)this).view.SetButtonsInteractable(interactable: false);
		((UnityEvent)((BaseController<TView>)this).view.CloseButton.onClick).AddListener(new UnityAction(OnCloseClick));
		((UnityEvent)((BaseController<TView>)this).view.AdButton.onClick).AddListener(new UnityAction(OnAdClick));
		request.Controller = (IController)(object)this;
		if (!RewardedLuckyDrawService.RegisterPopup(request))
		{
			((BaseController<TView>)this).CloseView(false);
			return;
		}
		PlayOpenPresentation(delegate
		{
			if (!removed && !closing)
			{
				busy = false;
				((BaseController<TView>)this).view.SetButtonsInteractable(interactable: true);
			}
		});
	}

	protected virtual void PlayOpenPresentation(Action onCompleted)
	{
		onCompleted?.Invoke();
	}

	protected virtual void PlayClosePresentation(Action onCompleted)
	{
		onCompleted?.Invoke();
	}

	protected virtual void PlayClaimExitPresentation(Action onCompleted)
	{
		onCompleted?.Invoke();
	}

	public void CloseWithAnimation(Action onCompleted)
	{
		if (removed)
		{
			onCompleted?.Invoke();
			return;
		}
		closeCallbacks = (Action)Delegate.Combine(closeCallbacks, onCompleted);
		if (!closing)
		{
			closing = true;
			busy = true;
			((BaseController<TView>)this).view.SetButtonsInteractable(interactable: false);
			PlayClosePresentation(delegate
			{
				Action action = closeCallbacks;
				closeCallbacks = null;
				action?.Invoke();
			});
		}
	}

	protected virtual string[] GetPropIconPaths(RewardedLuckyDrawProbability[] slots)
	{
		return null;
	}

	public override void OnActive(bool isActive)
	{
		((BaseController<TView>)this).OnActive(isActive);
		RewardedLuckyDrawRequest rewardedLuckyDrawRequest = request;
		if (rewardedLuckyDrawRequest == null || !rewardedLuckyDrawRequest.OpeningClaim || isActive)
		{
			((BaseController<TView>)this).view.NotifyPopupMaskStackActive(isActive);
		}
	}

	private void OnAdClick()
	{
		if (busy || removed)
		{
			return;
		}
		RRDataTrack.Instance.ReportAdRewardEnter(RRDataTrack.AdAction.Click, AdPopPosition.RewardedLuckyDraw);
		busy = true;
		((BaseController<TView>)this).view.SetButtonsInteractable(interactable: false);
		RewardedLuckyDrawService.WatchAd(request, delegate(bool success, int slotId)
		{
			if (!removed)
			{
				if (!success)
				{
					busy = false;
					((BaseController<TView>)this).view.SetButtonsInteractable(interactable: true);
				}
				else
				{
					if (request.PresentationType == 0)
					{
						AudioService.ActivateExperiment("variant_507793e1");
						AudioService.PlayUiSound(AudioKeys.LuckyWheel.Value);
					}
					((BaseController<TView>)this).view.PlayResult(slotId, RewardedLuckyDrawService.WheelAnimationSettings, delegate
					{
						//IL_0037: Unknown result type (might be due to invalid IL or missing references)
						//IL_003c: Unknown result type (might be due to invalid IL or missing references)
						//IL_0041: Unknown result type (might be due to invalid IL or missing references)
						//IL_0048: Unknown result type (might be due to invalid IL or missing references)
						if (!removed)
						{
							RewardedLuckyDrawRequest claimRequest = request;
							Vector2 propFlyTargetPos = Vector2.op_Implicit(((Component)((BaseController<TView>)this).view.AdButton).transform.position);
							RewardedLuckyDrawService.OpenClaimReward(claimRequest, propFlyTargetPos, ((BaseController<TView>)this).view.HideContentForClaim, delegate
							{
								if (!removed && request == claimRequest && claimRequest.Valid && !claimRequest.Completed)
								{
									PlayClaimExitPresentation(delegate
									{
										if (!removed && request == claimRequest && claimRequest.Valid && !claimRequest.Completed)
										{
											((BaseController<TView>)this).view.HideContentForClaim();
										}
									});
								}
							});
						}
					});
				}
			}
		});
	}

	private void OnCloseClick()
	{
		if (!busy && !removed)
		{
			RewardedLuckyDrawService.DismissPopup(request);
		}
	}

	public override void OnEscape()
	{
		OnCloseClick();
	}

	public override void OnRemove()
	{
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Expected O, but got Unknown
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Expected O, but got Unknown
		removed = true;
		closeCallbacks = null;
		((UnityEvent)((BaseController<TView>)this).view.CloseButton.onClick).RemoveListener(new UnityAction(OnCloseClick));
		((UnityEvent)((BaseController<TView>)this).view.AdButton.onClick).RemoveListener(new UnityAction(OnAdClick));
		((BaseController<TView>)this).view.StopPresentation();
		if (request != null && (object)request.Controller == this)
		{
			request.Controller = null;
		}
		RewardedLuckyDrawService.OnPopupRemoved(request);
		base.OnRemove();
	}
}
