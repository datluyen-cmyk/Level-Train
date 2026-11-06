using System;
using DG.Tweening;
using Game.DataTrack;
using RotateRings.Ad;
using RotateRings.Tool;
using RotateRings.UI.ClaimReward;
using UnityEngine;
using UnityEngine.Events;
using XingYun.UI.Controller;
using XingYun.UI.Core;

namespace RotateRings.UI.AdPop;

[UIPath("UILevelAdPop")]
public class UILevelAdPopController : BaseController<UILevelAdPopView>
{
	private const string LogTag = "AdPop";

	private AdPopPosition popPosition;

	private AdState adStateCurrent;

	private int netCheckTimerId;

	private int loadingAdTimerId;

	private int loadingAdDuration;

	private int loadingAdTimer;

	private Tween guaranteeDelayTween;

	private bool isDismissedByUser;

	private bool isClosing;

	private bool reportedCancel;

	private bool doOnce = true;

	private UIAdPopParam uiParam;

	public UILevelAdPopController(UILevelAdPopView view)
		: base(view)
	{
	}

	public override void OnRegister(object param = null)
	{
		isDismissedByUser = false;
		isClosing = false;
		reportedCancel = false;
		if (param != null)
		{
			uiParam = param as UIAdPopParam;
			if (uiParam != null)
			{
				popPosition = uiParam.PopPosition;
			}
		}
		base.view.Init();
		WireListeners();
		Initialize();
		base.view.PlayOpenAnimation();
		RRDataTrack.Instance.ADWindowOperation(RRDataTrack.ADWindowOperationType.Show);
		if (popPosition == AdPopPosition.RewardedLuckyDraw)
		{
			RRDataTrack.Instance.ReportAdRewardEnter(RRDataTrack.AdAction.Show, popPosition, fromWaitPopup: true);
		}
		HideClaimRewardMaskIfOpen();
	}

	public override void OnActive(bool isActive)
	{
		base.OnActive(isActive);
		base.view.OnUiStackActiveChanged(isActive);
		if (!isActive)
		{
			RestoreClaimRewardMaskIfOpen();
		}
	}

	private void WireListeners()
	{
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Expected O, but got Unknown
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Expected O, but got Unknown
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Expected O, but got Unknown
		AdPopHaveNetWidget haveNetWidget = base.view.HaveNetWidget;
		if ((Object)(object)haveNetWidget?.btnContinue != (Object)null)
		{
			((UnityEvent)haveNetWidget.btnContinue.onClick).AddListener(new UnityAction(OnContinueClick));
		}
		if ((Object)(object)haveNetWidget?.btnRestart != (Object)null)
		{
			((UnityEvent)haveNetWidget.btnRestart.onClick).AddListener(new UnityAction(OnRestartClick));
		}
		if ((Object)(object)haveNetWidget?.btnClose != (Object)null)
		{
			((UnityEvent)haveNetWidget.btnClose.onClick).AddListener(new UnityAction(OnRestartClick));
		}
	}

	private void UnwireListeners()
	{
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Expected O, but got Unknown
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Expected O, but got Unknown
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Expected O, but got Unknown
		AdPopHaveNetWidget haveNetWidget = base.view.HaveNetWidget;
		if ((Object)(object)haveNetWidget?.btnContinue != (Object)null)
		{
			((UnityEvent)haveNetWidget.btnContinue.onClick).RemoveListener(new UnityAction(OnContinueClick));
		}
		if ((Object)(object)haveNetWidget?.btnRestart != (Object)null)
		{
			((UnityEvent)haveNetWidget.btnRestart.onClick).RemoveListener(new UnityAction(OnRestartClick));
		}
		if ((Object)(object)haveNetWidget?.btnClose != (Object)null)
		{
			((UnityEvent)haveNetWidget.btnClose.onClick).RemoveListener(new UnityAction(OnRestartClick));
		}
	}

	private void Initialize()
	{
		loadingAdDuration = AdService.NoNetCountDown;
		netCheckTimerId = GameApp.Timer.Repeat(1f, (Action)CheckNetEverySecond);
		adStateCurrent = (NetUtil.IsHaveNet ? AdState.HaveNet : AdState.NoWifi);
		SetCurrentAdState(adStateCurrent);
	}

	private void SetCurrentAdState(AdState currentAdState)
	{
		adStateCurrent = currentAdState;
		base.view.SetBtnState(currentAdState);
	}

	private void CheckNetEverySecond()
	{
		if (isDismissedByUser || isClosing || adStateCurrent == AdState.LoadingAd)
		{
			return;
		}
		AdState adState = (NetUtil.IsHaveNet ? AdState.HaveNet : AdState.NoWifi);
		if (adState == adStateCurrent)
		{
			return;
		}
		switch (adState)
		{
		case AdState.NoWifi:
			if (adStateCurrent == AdState.HaveNet)
			{
			}
			break;
		case AdState.HaveNet:
			_ = adStateCurrent;
			_ = 3;
			break;
		}
		SetCurrentAdState(adState);
	}

	private void ShowRewardAd(Action<RewardAdResult> callback)
	{
		string rewardWaitAdPosition = AdPositionMap.GetRewardWaitAdPosition(popPosition);
		RRDataTrack.Instance.ReportAdVisit(RRDataTrack.AdVisitType.Reward, rewardWaitAdPosition);
		AdService.PlayRewardAdDirect(rewardWaitAdPosition, callback);
	}

	private void OnContinueClick()
	{
		if (popPosition == AdPopPosition.RewardedLuckyDraw)
		{
			RRDataTrack.Instance.ReportAdRewardEnter(RRDataTrack.AdAction.Click, popPosition, fromWaitPopup: true);
		}
		if (doOnce && !AdService.IsRewardReady)
		{
			ShowRewardAd(null);
			doOnce = false;
		}
		if (adStateCurrent == AdState.HaveNet)
		{
			StartLoadingCountdown();
		}
		RRDataTrack.Instance.ADWindowOperation(RRDataTrack.ADWindowOperationType.Retry);
	}

	private void StartLoadingCountdown()
	{
		loadingAdTimer = loadingAdDuration;
		GameApp.Timer.Kill(loadingAdTimerId);
		loadingAdTimerId = GameApp.Timer.RepeatWithDelay(1f, 1f, (Action)CheckRewardAdLoading);
		SetCurrentAdState(AdState.LoadingAd);
		base.view.SetCountDown(loadingAdTimer);
		if (popPosition != AdPopPosition.RewardedLuckyDraw)
		{
			RRDataTrack.Instance.ReportAdRewardEnter(RRDataTrack.AdAction.Click, popPosition);
		}
	}

	private void CheckRewardAdLoading()
	{
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Expected O, but got Unknown
		if (isDismissedByUser || isClosing)
		{
			return;
		}
		if (!NetUtil.IsHaveNet)
		{
			SetCurrentAdState(AdState.NoWifi);
			GameApp.Timer.Kill(loadingAdTimerId);
		}
		else
		{
			if (adStateCurrent != AdState.LoadingAd)
			{
				return;
			}
			loadingAdTimer--;
			if (loadingAdTimer <= 0)
			{
				base.view.SetCountDown(0);
				GameApp.Timer.Kill(loadingAdTimerId);
				guaranteeDelayTween = DOVirtual.DelayedCall(0.2f, (TweenCallback)delegate
				{
					if (!isDismissedByUser && !isClosing)
					{
						AdState currentAdState = (NetUtil.IsHaveNet ? AdState.HaveNet : AdState.NoWifi);
						SetCurrentAdState(currentAdState);
						OnAdResult(RewardAdResult.Success(isFinishedAd: false));
					}
				}, true);
			}
			else
			{
				base.view.SetCountDown(loadingAdTimer);
				if (AdService.IsRewardReady)
				{
					ShowRewardAd(OnAdResult);
					GameApp.Timer.Kill(loadingAdTimerId);
				}
			}
		}
	}

	private void OnAdResult(RewardAdResult result)
	{
		if (!isDismissedByUser && !isClosing)
		{
			CancelPendingWork();
			uiParam?.OnAdResult?.Invoke(result);
			CloseWithAnimation();
		}
	}

	private void OnRestartClick()
	{
		if (!isDismissedByUser && !isClosing)
		{
			isDismissedByUser = true;
			reportedCancel = true;
			CancelPendingWork();
			RRDataTrack.Instance.ADWindowOperation(RRDataTrack.ADWindowOperationType.Cancel);
			uiParam?.OnPopClosed?.Invoke();
			CloseWithAnimation();
		}
	}

	private void CloseWithAnimation()
	{
		if (isClosing)
		{
			return;
		}
		isClosing = true;
		float num = base.view.PlayCloseAnimation();
		if (num > 0f)
		{
			GameApp.Timer.Delay((Object)(object)((UIBaseView)base.view).Behaviour, num, (Action)delegate
			{
				base.CloseView(false);
			});
		}
		else
		{
			base.CloseView(false);
		}
	}

	private void CancelPendingWork()
	{
		GameApp.Timer.Kill(netCheckTimerId);
		GameApp.Timer.Kill(loadingAdTimerId);
		Tween obj = guaranteeDelayTween;
		if (obj != null)
		{
			TweenExtensions.Kill(obj, false);
		}
		guaranteeDelayTween = null;
		loadingAdTimer = 0;
	}

	public override void OnRemove()
	{
		UnwireListeners();
		CancelPendingWork();
		if (!reportedCancel)
		{
			RRDataTrack.Instance.ADWindowOperation(RRDataTrack.ADWindowOperationType.Close);
		}
	}

	private void ReportAdVisitClose()
	{
		string adPosition = AdPositionMap.GetAdPosition(popPosition);
		RRDataTrack.Instance.ReportAdVisit(RRDataTrack.AdVisitType.Reward, adPosition, "close");
	}

	public override void OnEscape()
	{
	}

	private static void HideClaimRewardMaskIfOpen()
	{
		IController val = default(IController);
		if (GameApp.UI.IsOpen("UIClaimReward", ref val) && val is UIClaimRewardController uIClaimRewardController)
		{
			uIClaimRewardController.View.NotifyPopupMaskStackActive(isActive: false);
		}
	}

	private static void RestoreClaimRewardMaskIfOpen()
	{
		IController val = default(IController);
		if (GameApp.UI.IsOpen("UIClaimReward", ref val) && val is UIClaimRewardController uIClaimRewardController)
		{
			uIClaimRewardController.View.NotifyPopupMaskStackActive(isActive: true);
		}
	}
}
