using System;
using System.Collections.Generic;
using Game.DataTrack;
using Hot.Model;
using RotateRings.Common;
using RotateRings.DailyFirstWin;
using RotateRings.GamePlay.Save;
using RotateRings.Model;
using RotateRings.UI.Level;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using XingYun.UI.Controller;
using XingYun.UI.Core;

namespace RotateRings.UI.DailyFirstWin;

[UIPath("UIDailyFirstWin")]
public class UIDailyFirstWinController : BaseController<UIDailyFirstWinView>
{
	private const string HomeDetailIncompleteKey = "FirstWin_OpenUI_Incomplete";

	private const string HomeDetailCompletedKey = "FirstWin_OpenUI_Completed";

	private const string SettlementCheerUpKeyPrefix = "FirstWin_PopUp_CheerUp_";

	private const int SettlementCheerUpCount = 10;

	private const string GotoButtonGuideAnimName = "Ani_UIHome_btn";

	private const float GotoButtonGuideDelay = 4f;

	private DailyFirstWinOpenData openData;

	private bool isClosing;

	private bool closedCallbackInvoked;

	private bool claimRewardOpened;

	private bool pendingGotoBtnEffect;

	private int gotoButtonGuideTimerId = -1;

	private int gotoButtonGuideAnimationTimerId = -1;

	private int periodCountdownTimerId = -1;

	private Animation gotoButtonGuideAnimation;

	public UIDailyFirstWinController(UIDailyFirstWinView view)
		: base(view)
	{
	}

	public override void OnRegister(object param = null)
	{
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Expected O, but got Unknown
		//IL_00cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d9: Expected O, but got Unknown
		RRDataTrack.Instance.ReportPageShow("每日首胜页");
		openData = (param as DailyFirstWinOpenData) ?? DailyFirstWinService.Instance.CreateOpenData(DailyFirstWinOpenMode.Home);
		isClosing = false;
		closedCallbackInvoked = false;
		claimRewardOpened = false;
		pendingGotoBtnEffect = false;
		if ((Object)(object)base.view.BtnClose != (Object)null)
		{
			((UnityEvent)base.view.BtnClose.onClick).AddListener(new UnityAction(OnCloseClick));
		}
		base.view.InitBtnOK(playIdleAnimation: true);
		if ((Object)(object)base.view.BtnOKView?.btnOK != (Object)null)
		{
			((UnityEvent)base.view.BtnOKView.btnOK.onClick).AddListener(new UnityAction(OnGotoClick));
		}
		bool flag = IsSettlementMode();
		IDailyFirstWinPresentationHost uIHost = DailyFirstWinModule.UIHost;
		if (flag)
		{
			uIHost.ReportBtnClick("每日首胜-首胜界面展现");
			Button btnClose = base.view.BtnClose;
			if (btnClose != null)
			{
				((Component)btnClose).gameObject.SetActive(false);
			}
		}
		else
		{
			Button btnClose2 = base.view.BtnClose;
			if (btnClose2 != null)
			{
				((Component)btnClose2).gameObject.SetActive(true);
			}
			if ((Object)(object)base.view.BtnClose != (Object)null)
			{
				((Selectable)base.view.BtnClose).interactable = true;
			}
		}
		int currentDay = openData?.CurrentDay ?? 0;
		bool hasFirstWinToday = openData?.HasFirstWinToday ?? false;
		RefreshView();
		base.view.SetRewardPreviewInteractable(interactable: false);
		base.view.PrepareOpenFirstFrame(flag, hasFirstWinToday);
		if (flag)
		{
			base.view.PrepareProgressCompletePresentation(currentDay, openData?.MilestoneDays);
		}
		base.view.PlayOpen(flag, hasFirstWinToday, flag ? ((Action)delegate
		{
			PlaySettlementProgressPresentation(currentDay);
		}) : null, delegate
		{
			base.view.SetRewardPreviewInteractable(interactable: true);
		}, !flag);
		RefreshGotoButtonGuideTimer();
		StartPeriodCountdownIfNeeded();
	}

	public override void OnRemove()
	{
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Expected O, but got Unknown
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Expected O, but got Unknown
		StopPeriodCountdown();
		KillGotoButtonGuideTimer();
		KillGotoButtonGuideAnimation();
		if ((Object)(object)base.view.BtnClose != (Object)null)
		{
			((UnityEvent)base.view.BtnClose.onClick).RemoveListener(new UnityAction(OnCloseClick));
		}
		if ((Object)(object)base.view.BtnOKView?.btnOK != (Object)null)
		{
			((UnityEvent)base.view.BtnOKView.btnOK.onClick).RemoveListener(new UnityAction(OnGotoClick));
		}
		InvokeClosedCallbackOnce();
		openData = null;
		isClosing = false;
	}

	public override void OnEscape()
	{
		if (!IsSettlementMode())
		{
			OnCloseClick();
		}
	}

	public override void OnActive(bool isActive)
	{
		base.OnActive(isActive);
		if (isActive && claimRewardOpened)
		{
			TryPlayPendingGotoBtnEffect();
		}
	}

	private void RefreshView()
	{
		bool flag = IsSettlementMode();
		int num = openData?.CurrentDay ?? 0;
		bool flag2 = openData?.HasFirstWinToday ?? false;
		base.view.SetTitleVisible(flag2);
		base.view.SetDetail(GetDetailText(flag));
		base.view.SetGotoInteractable(interactable: true);
		base.view.SetGotoVisible(!flag || !(openData?.HasReward ?? false));
		if (flag)
		{
			base.view.RefreshSettlementPlayButton();
		}
		else
		{
			int level = GameApp.Proxy.Get<LevelModelProxy>().Level;
			bool hasUnfinishedLevel = GameApp.Proxy.Get<RingLevelSaveProxy>().HasValidSave(level);
			GameMode currentGameMode = GameInfo.CurrentGameMode;
			ELevelDifficulty difficulty = LevelDifficultyUtil.GetDifficulty(currentGameMode, level);
			base.view.RefreshPlayButton(difficulty, hasUnfinishedLevel, currentGameMode, level);
		}
		base.view.SetDaySprite(num);
		base.view.RefreshProgress(num, openData?.PeriodDays ?? 7, flag2, openData?.MilestoneDays, openData?.PendingRewardDay ?? 0, OnRewardBoxClick);
	}

	private void StartPeriodCountdownIfNeeded()
	{
		StopPeriodCountdown();
		if (!isClosing && !IsSettlementMode() && DailyFirstWinService.Instance.IsEnabled())
		{
			OnPeriodSecondTick();
			periodCountdownTimerId = GameApp.Timer.Repeat(1f, (Action)OnPeriodSecondTick);
		}
	}

	private void StopPeriodCountdown()
	{
		if (periodCountdownTimerId >= 0)
		{
			GameApp.Timer.Kill(periodCountdownTimerId);
			periodCountdownTimerId = -1;
		}
	}

	private void OnPeriodSecondTick()
	{
		if (isClosing || IsSettlementMode() || openData == null)
		{
			StopPeriodCountdown();
			return;
		}
		if (!DailyFirstWinService.Instance.IsEnabled())
		{
			StopPeriodCountdown();
			return;
		}
		DailyFirstWinService.Instance.RefreshOnHome();
		DailyFirstWinProxy dailyFirstWinProxy = GameApp.Proxy.Get<DailyFirstWinProxy>();
		if (dailyFirstWinProxy != null)
		{
			int currentDayInCycle = dailyFirstWinProxy.GetCurrentDayInCycle();
			bool flag = dailyFirstWinProxy.HasFirstWinToday();
			int num = dailyFirstWinProxy.Data?.PendingRewardDay ?? 0;
			int currentPuzzleId = dailyFirstWinProxy.GetCurrentPuzzleId();
			if (currentDayInCycle != openData.CurrentDay || flag != openData.HasFirstWinToday || num != openData.PendingRewardDay || currentPuzzleId != openData.PuzzleId)
			{
				DailyFirstWinOpenMode mode = openData.Mode;
				Action onClosed = openData.OnClosed;
				openData = DailyFirstWinService.Instance.CreateOpenData(mode, onClosed);
				RefreshView();
			}
		}
	}

	private void PlaySettlementProgressPresentation(int currentDay)
	{
		base.view.PlayProgressCompletePresentation(currentDay, openData?.MilestoneDays, delegate
		{
			base.view.RefreshProgressState(currentDay, openData?.PeriodDays ?? 7, openData?.HasFirstWinToday ?? false, openData?.MilestoneDays, 0);
			DailyFirstWinOpenData dailyFirstWinOpenData = openData;
			if (dailyFirstWinOpenData != null && dailyFirstWinOpenData.HasReward)
			{
				OpenSettlementRewardClaimView();
			}
			else
			{
				base.view.SetGotoVisible(visible: true);
			}
		});
	}

	private void OnCloseClick()
	{
		if (!IsSettlementMode())
		{
			KillGotoButtonGuideAnimation();
			DailyFirstWinModule.UIHost.ReportBtnClick("每日首胜-返回主页");
			RRDataTrack.Instance.ReportPageShow("每日首胜进主页");
			RequestClose(null);
		}
	}

	private void OnGotoClick()
	{
		KillGotoButtonGuideAnimation();
		if (IsSettlementMode())
		{
			DailyFirstWinModule.UIHost.ReportBtnClick("每日首胜-继续按钮");
			RequestClose(null);
		}
		else
		{
			DailyFirstWinModule.UIHost.ReportBtnClick("每日首胜-开始游戏");
			EnterLevelCommand.PrepareOverlayEnterLevel();
			RequestClose(DailyFirstWinModule.UIHost.EnterMainLevel);
		}
	}

	private void OnRewardBoxClick(int milestoneDay, RectTransform target)
	{
		KillGotoButtonGuideAnimation();
		RefreshGotoButtonGuideTimer();
		DailyFirstWinModule.UIHost.ReportBtnClick("每日首胜-宝箱预览");
		DailyFirstWinRewardInfo dailyFirstWinRewardInfo = GameApp.Proxy.Get<DailyFirstWinProxy>()?.GetRewardInfoByMilestoneDay(milestoneDay) ?? default(DailyFirstWinRewardInfo);
		if (dailyFirstWinRewardInfo.RewardConfigId > 0)
		{
			List<RewardItemSnapshot> rewards = DailyFirstWinModule.Host.BuildRewardItems(dailyFirstWinRewardInfo.RewardConfigId);
			DailyFirstWinModule.UIHost.ShowChestRewards(target, rewards);
		}
	}

	private void OpenSettlementRewardClaimView()
	{
		DailyFirstWinModule.UIHost.ReportBtnClick("每日首胜-首胜宝箱点击");
		DailyFirstWinModule.UIHost.CloseTips();
		RectTransform collectTarget = (RectTransform)(((Object)(object)base.view.BtnOKView?.btnOK != (Object)null) ? /*isinst with value type is only supported in some contexts*/: null);
		int day = openData?.PendingRewardDay ?? 0;
		RectTransform dayItemRectTransform = base.view.GetDayItemRectTransform(day);
		claimRewardOpened = true;
		if (!DailyFirstWinService.Instance.OpenPendingRewardClaimView(ShowSettlementContinueButton, collectTarget, dayItemRectTransform, OnRewardFlyStart, OnRewardFirstPropArrive))
		{
			claimRewardOpened = false;
			base.view.SetGotoVisible(visible: true);
			base.view.SetGotoInteractable(interactable: true);
		}
	}

	private void OnRewardFlyStart()
	{
		base.view.EnsureGotoVisibleForFly();
	}

	private void OnRewardFirstPropArrive()
	{
		pendingGotoBtnEffect = true;
	}

	private void TryPlayPendingGotoBtnEffect()
	{
		if (pendingGotoBtnEffect)
		{
			pendingGotoBtnEffect = false;
			base.view.PlayGotoBtnEffect();
		}
	}

	private void ShowSettlementContinueButton()
	{
		base.view.PlayGotoFadeIn(0.2f, delegate
		{
			base.view.SetGotoInteractable(interactable: true);
		});
	}

	private void RequestClose(Action afterClosed)
	{
		if (!isClosing)
		{
			isClosing = true;
			StopPeriodCountdown();
			KillGotoButtonGuideTimer();
			KillGotoButtonGuideAnimation();
			DailyFirstWinModule.UIHost.CloseTips();
			base.view.PlayClose(delegate
			{
				InvokeClosedCallbackOnce();
				base.CloseView(false);
				afterClosed?.Invoke();
			});
		}
	}

	private void InvokeClosedCallbackOnce()
	{
		if (!closedCallbackInvoked)
		{
			closedCallbackInvoked = true;
			openData?.OnClosed?.Invoke();
		}
	}

	private bool IsSettlementMode()
	{
		if (openData != null)
		{
			return openData.Mode == DailyFirstWinOpenMode.Settlement;
		}
		return false;
	}

	private string GetDetailText(bool isSettlement)
	{
		IDailyFirstWinPresentationHost uIHost = DailyFirstWinModule.UIHost;
		if (isSettlement)
		{
			int num = Random.Range(0, 10);
			return GetText(uIHost, string.Format("{0}{1}", "FirstWin_PopUp_CheerUp_", num), "Today's first win is complete. Keep the streak going!");
		}
		bool flag = openData?.HasFirstWinToday ?? false;
		return GetText(uIHost, flag ? "FirstWin_OpenUI_Completed" : "FirstWin_OpenUI_Incomplete", flag ? "Today's first win is complete. Keep the streak going!" : "Win one main level each day to light up the progress.");
	}

	private static string GetText(IDailyFirstWinPresentationHost uiHost, string key, string fallback)
	{
		string text = uiHost.GetText(key);
		if (string.IsNullOrEmpty(text) || string.Equals(text, key, StringComparison.Ordinal))
		{
			return fallback;
		}
		return text;
	}

	private void RefreshGotoButtonGuideTimer()
	{
		KillGotoButtonGuideTimer();
		if (ShouldPlayGotoButtonGuide())
		{
			gotoButtonGuideTimerId = GameApp.Timer.Delay((Object)(object)((UIBaseView)base.view).NodeRect, 4f, (Action)PlayGotoButtonGuide);
		}
	}

	private bool ShouldPlayGotoButtonGuide()
	{
		if (isClosing || (Object)(object)base.view?.BtnContent == (Object)null || (Object)(object)base.view?.BtnOKView?.btnOK == (Object)null || IsSettlementMode())
		{
			return false;
		}
		if (!DailyFirstWinModule.UIHost.IsHomeButtonEffectEnabled())
		{
			return false;
		}
		if (base.view.BtnContent.activeInHierarchy)
		{
			return ((Selectable)base.view.BtnOKView.btnOK).interactable;
		}
		return false;
	}

	private void PlayGotoButtonGuide()
	{
		gotoButtonGuideTimerId = -1;
		if (ShouldPlayGotoButtonGuide())
		{
			Animation component = base.view.BtnContent.GetComponent<Animation>();
			AnimationClip val = (((Object)(object)component != (Object)null) ? component.GetClip("Ani_UIHome_btn") : null);
			if ((Object)(object)component == (Object)null || (Object)(object)val == (Object)null)
			{
				RefreshGotoButtonGuideTimer();
				return;
			}
			KillGotoButtonGuideAnimation();
			gotoButtonGuideAnimation = component;
			component.Stop();
			component.Rewind("Ani_UIHome_btn");
			component.Play("Ani_UIHome_btn");
			gotoButtonGuideAnimationTimerId = GameApp.Timer.Delay((Object)(object)((UIBaseView)base.view).NodeRect, val.length, (Action)OnGotoButtonGuideAnimationComplete);
		}
	}

	private void OnGotoButtonGuideAnimationComplete()
	{
		gotoButtonGuideAnimationTimerId = -1;
		if ((Object)(object)gotoButtonGuideAnimation != (Object)null)
		{
			gotoButtonGuideAnimation.Stop();
			gotoButtonGuideAnimation.Rewind("Ani_UIHome_btn");
			gotoButtonGuideAnimation.Sample();
			gotoButtonGuideAnimation = null;
		}
		RefreshGotoButtonGuideTimer();
	}

	private void KillGotoButtonGuideTimer()
	{
		if (gotoButtonGuideTimerId >= 0)
		{
			GameApp.Timer.Kill(gotoButtonGuideTimerId);
			gotoButtonGuideTimerId = -1;
		}
	}

	private void KillGotoButtonGuideAnimation()
	{
		if (gotoButtonGuideAnimationTimerId >= 0)
		{
			GameApp.Timer.Kill(gotoButtonGuideAnimationTimerId);
			gotoButtonGuideAnimationTimerId = -1;
		}
		if (!((Object)(object)gotoButtonGuideAnimation == (Object)null))
		{
			gotoButtonGuideAnimation.Stop();
			gotoButtonGuideAnimation.Rewind("Ani_UIHome_btn");
			gotoButtonGuideAnimation.Sample();
			gotoButtonGuideAnimation = null;
		}
	}
}
