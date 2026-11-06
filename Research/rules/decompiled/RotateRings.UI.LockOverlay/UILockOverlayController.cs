using System;
using Game.Common.Services;
using Hot.Model;
using RotateRings.Common;
using RotateRings.UI.Shop;
using UnityEngine;
using XingYun.UI.Controller;
using XingYun.UI.Core;

namespace RotateRings.UI.LockOverlay;

[UIPath("UILockOverlay")]
public class UILockOverlayController : BaseController<UILockOverlayView>
{
	private const float TimerIntervalUpdate = 0.5f;

	private const float PurchaseLoadingGuaranteedCloseSeconds = 5f;

	private const string Tag = "[UILockOverlay] ";

	private int timerId;

	private int purchaseLoadingGuaranteedTimerId;

	private bool needGuaranteedClosePurchaseLoading;

	private bool isLockUI;

	private int curLockUICount;

	private float curLockRemainTime = float.MinValue;

	public void ForceReset(string reason)
	{
		ResetLock();
	}

	public UILockOverlayController(UILockOverlayView view)
		: base(view)
	{
	}

	public override void OnRegister(object param = null)
	{
		base.ListenEvent<EventLockUI>((Action<EventLockUI>)LockUIHandler);
		base.ListenEvent<PurchaseLoadingEvent>((Action<PurchaseLoadingEvent>)PurchaseLoadingHandler);
		base.view.Init();
		if (PurchaseService.IsShowing)
		{
			ShowPurchaseLoadingWithGuaranteedClose();
		}
		timerId = GameApp.Timer.Repeat((Object)(object)((UIBaseView)base.view).Behaviour, 0.5f, (Action)SimulateUpdate);
	}

	public override void OnRemove()
	{
		base.UnlistenEvent<EventLockUI>((Action<EventLockUI>)LockUIHandler);
		base.UnlistenEvent<PurchaseLoadingEvent>((Action<PurchaseLoadingEvent>)PurchaseLoadingHandler);
		GameApp.Timer.Kill(timerId);
		timerId = 0;
		HidePurchaseLoading(killTimer: true);
		ResetLock();
	}

	private void PurchaseLoadingHandler(PurchaseLoadingEvent obj)
	{
		if (obj != null)
		{
			if (obj.IsShowing)
			{
				ShowPurchaseLoadingWithGuaranteedClose();
			}
			else
			{
				HidePurchaseLoading(killTimer: true);
			}
		}
	}

	private void ShowPurchaseLoadingWithGuaranteedClose()
	{
		KillPurchaseLoadingGuaranteedTimer();
		needGuaranteedClosePurchaseLoading = true;
		base.view.ShowPurchaseLoading(isShow: true);
		purchaseLoadingGuaranteedTimerId = GameApp.Timer.Delay(5f, (Action)delegate
		{
			if (needGuaranteedClosePurchaseLoading)
			{
				purchaseLoadingGuaranteedTimerId = 0;
				HidePurchaseLoading(killTimer: false);
				PurchaseService.ForceClear();
			}
		});
	}

	private void HidePurchaseLoading(bool killTimer)
	{
		needGuaranteedClosePurchaseLoading = false;
		if (killTimer)
		{
			KillPurchaseLoadingGuaranteedTimer();
		}
		else
		{
			purchaseLoadingGuaranteedTimerId = 0;
		}
		base.view.ShowPurchaseLoading(isShow: false);
	}

	private void KillPurchaseLoadingGuaranteedTimer()
	{
		if (purchaseLoadingGuaranteedTimerId != 0)
		{
			GameApp.Timer.Kill(purchaseLoadingGuaranteedTimerId);
			purchaseLoadingGuaranteedTimerId = 0;
		}
	}

	private void LockUIHandler(EventLockUI obj)
	{
		if (obj == null)
		{
			return;
		}
		if (obj.lockUI)
		{
			curLockUICount++;
			if (!(curLockRemainTime > obj.durationTime))
			{
				isLockUI = true;
				GameInfo.IsGameUILock = true;
				curLockRemainTime = obj.durationTime;
				base.view.ShowLock(isShow: true);
			}
		}
		else
		{
			curLockUICount--;
			if (curLockUICount <= 0)
			{
				ResetLock();
			}
		}
	}

	private void ResetLock()
	{
		isLockUI = false;
		GameInfo.IsGameUILock = false;
		curLockUICount = 0;
		curLockRemainTime = float.MinValue;
		base.view.ShowLock(isShow: false);
	}

	private void SimulateUpdate()
	{
		if (isLockUI)
		{
			float num = curLockRemainTime;
			curLockRemainTime -= 0.5f;
			bool flag = curLockRemainTime == num;
			if (curLockRemainTime <= 0f)
			{
				ResetLock();
			}
			else if (flag)
			{
				LockUIHealthCheck.RecheckStuckMask("OverlayTick");
			}
		}
	}
}
