using System;
using System.Collections.Generic;
using Example.Cfg;
using Game.Common.Services;
using Game.DataTrack;
using Game.Input;
using Hot.Model;
using Lean.Touch;
using RotateRings.GamePlay.Events;
using RotateRings.GamePlay.Services;
using RotateRings.Model;
using RotateRings.Prop;
using RotateRings.UI.Game;
using RotateRings.UI.MultiTouchGuide;
using RotateRings.WeakGuide.Data;
using UniLog;
using UnityEngine;
using XingYun.Core.Singletons;
using XingYun.UI.Controller;
using XingYun.UI.Core;

namespace RotateRings.GamePlay.MultiTouchGuide;

public sealed class MultiTouchGuideService : GameplayServiceBase<GameplayManager>
{
	private const string Tag = "[MultiTouchGuide] ";

	private bool isRunning;

	private bool isPendingStart;

	private bool isWaitingFingerRelease;

	private MultiTouchGuideStep currentStep;

	private RotateCameraGestureController gestureController;

	private UIMultiTouchGuideController guideController;

	public MultiTouchGuideService(GameplayManager host)
		: base(host)
	{
		GameApp.Event.Listen<LevelStartPipelineFinishedEvent>((Action<LevelStartPipelineFinishedEvent>)OnLevelStartPipelineFinished);
		GameApp.Event.Listen<WeakGuideCompletedEvent>((Action<WeakGuideCompletedEvent>)OnWeakGuideCompleted);
		GameApp.Event.Listen<PropUnlockTutorialBlockingUiClosedEvent>((Action<PropUnlockTutorialBlockingUiClosedEvent>)OnPropUnlockTutorialBlockingUiClosed);
		GameApp.Event.Listen<MultiTouchGuideGestureSucceededEvent>((Action<MultiTouchGuideGestureSucceededEvent>)OnGuideGestureSucceeded);
	}

	public override void Dispose()
	{
		if (!base.IsDisposed)
		{
			AbortGuide();
			GameApp.Event.Unlisten<LevelStartPipelineFinishedEvent>((Action<LevelStartPipelineFinishedEvent>)OnLevelStartPipelineFinished);
			GameApp.Event.Unlisten<WeakGuideCompletedEvent>((Action<WeakGuideCompletedEvent>)OnWeakGuideCompleted);
			GameApp.Event.Unlisten<PropUnlockTutorialBlockingUiClosedEvent>((Action<PropUnlockTutorialBlockingUiClosedEvent>)OnPropUnlockTutorialBlockingUiClosed);
			GameApp.Event.Unlisten<MultiTouchGuideGestureSucceededEvent>((Action<MultiTouchGuideGestureSucceededEvent>)OnGuideGestureSucceeded);
			base.Dispose();
		}
	}

	public override void ClearLevel()
	{
		AbortGuide();
		isPendingStart = false;
		base.ClearLevel();
	}

	public void RequestTryStart()
	{
		int level = GameApp.Proxy.Get<LevelModelProxy>()?.Level ?? 0;
		if (!ShouldOfferGuide(level))
		{
			isPendingStart = false;
			return;
		}
		MultiTouchInputUtility.EnsureMultiTouchEnabled();
		isPendingStart = true;
		if (CanStartNow(level))
		{
			BeginGuide();
		}
	}

	private void OnLevelStartPipelineFinished(LevelStartPipelineFinishedEvent eventData)
	{
		if (eventData != null && eventData.Round == LevelStartPipeline.CurrentRound)
		{
			RequestTryStart();
		}
	}

	private void OnWeakGuideCompleted(WeakGuideCompletedEvent eventData)
	{
		if (isPendingStart)
		{
			RequestTryStart();
		}
	}

	private void OnPropUnlockTutorialBlockingUiClosed(PropUnlockTutorialBlockingUiClosedEvent eventData)
	{
		if (isPendingStart)
		{
			RequestTryStart();
		}
	}

	private void OnGuideGestureSucceeded(MultiTouchGuideGestureSucceededEvent eventData)
	{
		if (!isRunning || eventData == null)
		{
			return;
		}
		MultiTouchGuideStep step = (MultiTouchGuideStep)eventData.Step;
		if (step == MultiTouchGuideStep.Pan)
		{
			TryReportStepComplete(MultiTouchGuideStep.Pan);
			FinishGuide();
		}
		else
		{
			if (step != currentStep)
			{
				return;
			}
			TryReportStepComplete(currentStep);
			UIMultiTouchGuideController uIMultiTouchGuideController = ResolveGuideController();
			if (uIMultiTouchGuideController == null)
			{
				OnStepSuccessPresented();
				return;
			}
			if (IsZoomStep(currentStep))
			{
				uIMultiTouchGuideController.HideStep();
			}
			uIMultiTouchGuideController.PlayStepSuccess(OnStepSuccessPresented);
		}
	}

	private void OnStepSuccessPresented()
	{
		if (isRunning)
		{
			if (IsZoomStep(currentStep) && HasPressedFinger())
			{
				BeginWaitFingerRelease();
			}
			else
			{
				AdvanceAfterStepSuccess();
			}
		}
	}

	private void BeginWaitFingerRelease()
	{
		if (!isWaitingFingerRelease)
		{
			isWaitingFingerRelease = true;
			LeanTouch.OnFingerUp += OnFingerUpWhileWaiting;
		}
	}

	private void StopWaitFingerRelease()
	{
		if (isWaitingFingerRelease)
		{
			isWaitingFingerRelease = false;
			LeanTouch.OnFingerUp -= OnFingerUpWhileWaiting;
		}
	}

	private void OnFingerUpWhileWaiting(LeanFinger finger)
	{
		if (!HasPressedFinger())
		{
			StopWaitFingerRelease();
			AdvanceAfterStepSuccess();
		}
	}

	private static bool HasPressedFinger()
	{
		List<LeanFinger> fingers = LeanTouch.Fingers;
		for (int i = 0; i < fingers.Count; i++)
		{
			LeanFinger val = fingers[i];
			if (val.Set && val.Index != -42)
			{
				return true;
			}
		}
		return false;
	}

	private static bool IsZoomStep(MultiTouchGuideStep step)
	{
		if (step != MultiTouchGuideStep.ZoomIn)
		{
			return step == MultiTouchGuideStep.ZoomOut;
		}
		return true;
	}

	private void AdvanceAfterStepSuccess()
	{
		switch (currentStep)
		{
		case MultiTouchGuideStep.ZoomIn:
			EnterStep(MultiTouchGuideStep.ZoomOut);
			break;
		case MultiTouchGuideStep.ZoomOut:
			EnterStep(MultiTouchGuideStep.Pan);
			break;
		case MultiTouchGuideStep.Pan:
			FinishGuide();
			break;
		}
	}

	private void BeginGuide()
	{
		isPendingStart = false;
		if (isRunning)
		{
			return;
		}
		gestureController = base.Host.GetGameService<RotateCameraGestureController>();
		if (gestureController == null)
		{
			return;
		}
		if (!GameApp.UI.IsOpen("UIMultiTouchGuide"))
		{
			GameApp.UI.Open("UIMultiTouchGuide", (UILayer)4, (object)null, (Action)null);
		}
		guideController = ResolveGuideController();
		if (guideController == null)
		{
			Log.LogError("[MultiTouchGuide] ", "打开多点触控引导 UI 失败");
			if (GameApp.UI.IsOpen("UIMultiTouchGuide"))
			{
				GameApp.UI.CloseView("UIMultiTouchGuide", (object)null, false);
			}
		}
		else
		{
			isRunning = true;
			currentStep = MultiTouchGuideStep.ZoomIn;
			Singleton<GameInputManager>.Instance.SetMultiTouchGuideSuppressingGameplay(suppress: true);
			SetUIGameInputBlocked(blocked: true);
			guideController.PlayOpen();
			EnterStep(currentStep);
		}
	}

	private void EnterStep(MultiTouchGuideStep step)
	{
		currentStep = step;
		TryReportStepOpen(step);
		guideController = ResolveGuideController();
		guideController?.EnterStep(step);
		gestureController?.SetGuideStepPolicy(MultiTouchGuideStepPolicy.ForStep(step));
	}

	private void FinishGuide()
	{
		MultiTouchGuideSave.MarkCompleted();
		UIMultiTouchGuideController uIMultiTouchGuideController = ResolveGuideController();
		if (uIMultiTouchGuideController != null)
		{
			uIMultiTouchGuideController.PlayClose(delegate
			{
				GameApp.UI.CloseView("UIMultiTouchGuide", (object)null, false);
			});
		}
		else
		{
			GameApp.UI.CloseView("UIMultiTouchGuide", (object)null, false);
		}
		EndGuideSession();
	}

	private void AbortGuide()
	{
		if (!isRunning && !isPendingStart)
		{
			gestureController?.ClearGuideStepPolicy();
			return;
		}
		if (GameApp.UI.IsOpen("UIMultiTouchGuide"))
		{
			GameApp.UI.CloseView("UIMultiTouchGuide", (object)null, false);
		}
		EndGuideSession();
		isPendingStart = false;
	}

	private void EndGuideSession()
	{
		isRunning = false;
		StopWaitFingerRelease();
		gestureController?.ClearGuideStepPolicy();
		gestureController = null;
		guideController = null;
		Singleton<GameInputManager>.Instance.SetMultiTouchGuideSuppressingGameplay(suppress: false);
		SetUIGameInputBlocked(blocked: false);
	}

	private static UIMultiTouchGuideController ResolveGuideController()
	{
		IController val = default(IController);
		if (GameApp.UI.IsOpen("UIMultiTouchGuide", ref val) && val is UIMultiTouchGuideController result)
		{
			return result;
		}
		return null;
	}

	private static bool ShouldOfferGuide(int level)
	{
		if (Gameplay.Mode != GameplayMode.RotateRing)
		{
			return false;
		}
		if (level <= MultiTouchGuideConfig.MinLevelExclusive)
		{
			return false;
		}
		if (!RotateCameraGestureController.ZoomEnabled || !RotateCameraGestureController.PanEnabled)
		{
			return false;
		}
		if (MultiTouchGuideSave.HasCompleted())
		{
			return false;
		}
		if (HasGameplayOrPropTutorialOnLevel(level))
		{
			return false;
		}
		return true;
	}

	private static bool HasGameplayOrPropTutorialOnLevel(int level)
	{
		if (WeakGuideStepsLocator.Default.FindEntry(level) != null)
		{
			return true;
		}
		return HasPropTutorialOnLevel(level);
	}

	private static bool HasPropTutorialOnLevel(int level)
	{
		PropModelProxy propModelProxy = GameApp.Proxy.Get<PropModelProxy>();
		if (propModelProxy == null)
		{
			return false;
		}
		List<PropConfig> list = propModelProxy.CollectUnlockableProps(level);
		if (list != null && list.Count > 0)
		{
			return true;
		}
		if (GameInfo.CurrentGameMode != GameMode.Main)
		{
			return false;
		}
		List<EPropType> list2 = propModelProxy.CollectReplayGuideProps(level);
		for (int i = 0; i < list2.Count; i++)
		{
			EPropType propType = list2[i];
			if (!propModelProxy.IsPropGuided(propType) || !propModelProxy.IsGuideUserInteracted(propType))
			{
				return true;
			}
		}
		return false;
	}

	private bool CanStartNow(int level)
	{
		if (!ShouldOfferGuide(level))
		{
			return false;
		}
		if ((Object)(object)base.Host != (Object)null && base.Host.IsLevelEnterPlaying)
		{
			return false;
		}
		if (!IsWeakGuideIdle())
		{
			return false;
		}
		if (IsBlockingGuideUiOpen())
		{
			return false;
		}
		RotateCameraAdaptation rotateCameraAdaptation = base.Host?.GetGameService<RotateCameraAdaptation>();
		if (rotateCameraAdaptation == null || rotateCameraAdaptation.CachedOrthographicSize <= 0f)
		{
			return false;
		}
		return true;
	}

	private bool IsWeakGuideIdle()
	{
		if ((Object)(object)base.Host?.CurrentLevelObject == (Object)null)
		{
			return true;
		}
		GameLevelInfo component = base.Host.CurrentLevelObject.GetComponent<GameLevelInfo>();
		WeakGuideManager weakGuideManager = (((Object)(object)component != (Object)null) ? component.weakGuideManager : null);
		if ((Object)(object)weakGuideManager == (Object)null)
		{
			return true;
		}
		return weakGuideManager.IsCompleted;
	}

	private static bool IsBlockingGuideUiOpen()
	{
		if (!GameApp.UI.IsOpen("UIPropGuide") && !GameApp.UI.IsOpen("UIPropUnlock"))
		{
			return GameApp.UI.IsOpen("WeakGuideView");
		}
		return true;
	}

	private static void SetUIGameInputBlocked(bool blocked)
	{
		IController val = default(IController);
		if (GameApp.UI.IsOpen("UIGame", ref val) && val is UIGameController uIGameController)
		{
			uIGameController.View?.SetInputBlocked(blocked);
		}
	}

	private static void TryReportStepOpen(MultiTouchGuideStep step)
	{
		if (step != MultiTouchGuideStep.Done && MultiTouchGuideSave.TryMarkStepOpenReported(step))
		{
			ReportTutorialStep(step, "打开");
		}
	}

	private static void TryReportStepComplete(MultiTouchGuideStep step)
	{
		if (step != MultiTouchGuideStep.Done && MultiTouchGuideSave.TryMarkStepCompleteReported(step))
		{
			ReportTutorialStep(step, "完成");
		}
	}

	private static void ReportTutorialStep(MultiTouchGuideStep step, string action)
	{
		if (step != MultiTouchGuideStep.Done)
		{
			int num = (int)(step + 1);
			string stageName = string.Format("{0}-步骤{1}-{2}", "新手引导", num, action);
			RRDataTrack.Instance.ReportTutorial(num, 1, stageName);
		}
	}
}
