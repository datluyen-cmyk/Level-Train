using System;
using Game.DataTrack;
using Game.UI;
using RotateRings.GamePlay.Events;
using RotateRings.GamePlay.MultiTouchGuide;
using UnityEngine;
using UnityEngine.Events;
using XingYun.UI.Controller;
using XingYun.UI.Core;

namespace RotateRings.UI.MultiTouchGuide;

[UIPath("UIMultiTouchGuide")]
public class UIMultiTouchGuideController : GameBaseController<UIMultiTouchGuideView>
{
	private const string Tag = "UI";

	private int showStepButtonTimerId = -1;

	public UIMultiTouchGuideController(UIMultiTouchGuideView view)
		: base(view)
	{
	}

	public override void OnRegister(object param = null)
	{
		((BaseController<UIMultiTouchGuideView>)this).view?.HideStepGestureNodesForOpen();
		RegisterButtonEvents();
		HideAllStepButtons();
	}

	public override void OnRemove()
	{
		CancelShowStepButtonTimer();
		UnregisterButtonEvents();
	}

	private void RegisterButtonEvents()
	{
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Expected O, but got Unknown
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Expected O, but got Unknown
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Expected O, but got Unknown
		if (((BaseController<UIMultiTouchGuideView>)this).view != null)
		{
			if ((Object)(object)((BaseController<UIMultiTouchGuideView>)this).view.ZoomInBtn != (Object)null)
			{
				((UnityEvent)((BaseController<UIMultiTouchGuideView>)this).view.ZoomInBtn.onClick).AddListener(new UnityAction(OnStepButtonClick));
			}
			if ((Object)(object)((BaseController<UIMultiTouchGuideView>)this).view.ZoomOutBtn != (Object)null)
			{
				((UnityEvent)((BaseController<UIMultiTouchGuideView>)this).view.ZoomOutBtn.onClick).AddListener(new UnityAction(OnStepButtonClick));
			}
			if ((Object)(object)((BaseController<UIMultiTouchGuideView>)this).view.MoveBtn != (Object)null)
			{
				((UnityEvent)((BaseController<UIMultiTouchGuideView>)this).view.MoveBtn.onClick).AddListener(new UnityAction(OnStepButtonClick));
			}
		}
	}

	private void UnregisterButtonEvents()
	{
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Expected O, but got Unknown
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Expected O, but got Unknown
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Expected O, but got Unknown
		if (((BaseController<UIMultiTouchGuideView>)this).view != null)
		{
			if ((Object)(object)((BaseController<UIMultiTouchGuideView>)this).view.ZoomInBtn != (Object)null)
			{
				((UnityEvent)((BaseController<UIMultiTouchGuideView>)this).view.ZoomInBtn.onClick).RemoveListener(new UnityAction(OnStepButtonClick));
			}
			if ((Object)(object)((BaseController<UIMultiTouchGuideView>)this).view.ZoomOutBtn != (Object)null)
			{
				((UnityEvent)((BaseController<UIMultiTouchGuideView>)this).view.ZoomOutBtn.onClick).RemoveListener(new UnityAction(OnStepButtonClick));
			}
			if ((Object)(object)((BaseController<UIMultiTouchGuideView>)this).view.MoveBtn != (Object)null)
			{
				((UnityEvent)((BaseController<UIMultiTouchGuideView>)this).view.MoveBtn.onClick).RemoveListener(new UnityAction(OnStepButtonClick));
			}
		}
	}

	private void HideAllStepButtons()
	{
		if (((BaseController<UIMultiTouchGuideView>)this).view != null)
		{
			if ((Object)(object)((BaseController<UIMultiTouchGuideView>)this).view.ZoomInBtn != (Object)null)
			{
				((Component)((BaseController<UIMultiTouchGuideView>)this).view.ZoomInBtn).gameObject.SetActive(false);
			}
			if ((Object)(object)((BaseController<UIMultiTouchGuideView>)this).view.ZoomOutBtn != (Object)null)
			{
				((Component)((BaseController<UIMultiTouchGuideView>)this).view.ZoomOutBtn).gameObject.SetActive(false);
			}
			if ((Object)(object)((BaseController<UIMultiTouchGuideView>)this).view.MoveBtn != (Object)null)
			{
				((Component)((BaseController<UIMultiTouchGuideView>)this).view.MoveBtn).gameObject.SetActive(false);
			}
		}
	}

	private void OnStepButtonClick()
	{
		RRDataTrack.Instance.ReportBtnClick("盘面-手势新手引导跳过");
		GameApp.Event.Send<MultiTouchGuideGestureSucceededEvent>(new MultiTouchGuideGestureSucceededEvent
		{
			Step = 2
		});
	}

	public void EnterStep(MultiTouchGuideStep step)
	{
		CancelShowStepButtonTimer();
		HideAllStepButtons();
		((BaseController<UIMultiTouchGuideView>)this).view?.ShowStep(step, delegate
		{
			ScheduleShowStepButton(step);
		});
	}

	public void HideStep()
	{
		CancelShowStepButtonTimer();
		((BaseController<UIMultiTouchGuideView>)this).view?.HideAllSteps();
		HideAllStepButtons();
	}

	private void ScheduleShowStepButton(MultiTouchGuideStep step)
	{
		GameObject val = ((((BaseController<UIMultiTouchGuideView>)this).view != null && (Object)(object)((UIBaseView)((BaseController<UIMultiTouchGuideView>)this).view).Behaviour != (Object)null) ? ((Component)((UIBaseView)((BaseController<UIMultiTouchGuideView>)this).view).Behaviour).gameObject : null);
		float stepButtonShowDelay = MultiTouchGuideConfig.StepButtonShowDelay;
		showStepButtonTimerId = (((Object)(object)val != (Object)null) ? GameApp.Timer.Delay((Object)(object)val, stepButtonShowDelay, (Action)delegate
		{
			ShowStepButton(step);
		}) : GameApp.Timer.Delay(stepButtonShowDelay, (Action)delegate
		{
			ShowStepButton(step);
		}));
	}

	private void CancelShowStepButtonTimer()
	{
		if (showStepButtonTimerId >= 0)
		{
			GameApp.Timer.Kill(showStepButtonTimerId);
			showStepButtonTimerId = -1;
		}
	}

	public void PlayStepSuccess(Action onComplete)
	{
		((BaseController<UIMultiTouchGuideView>)this).view?.PlayStepSuccess(onComplete);
	}

	public void PlayOpen()
	{
		((BaseController<UIMultiTouchGuideView>)this).view?.PlayOpen();
	}

	public void PlayClose(Action onComplete)
	{
		((BaseController<UIMultiTouchGuideView>)this).view?.PlayClose(onComplete);
	}

	private void ShowStepButton(MultiTouchGuideStep step)
	{
		if (((BaseController<UIMultiTouchGuideView>)this).view != null)
		{
			bool active = step == MultiTouchGuideStep.ZoomIn;
			bool active2 = step == MultiTouchGuideStep.ZoomOut;
			bool active3 = step == MultiTouchGuideStep.Pan;
			if ((Object)(object)((BaseController<UIMultiTouchGuideView>)this).view.ZoomInBtn != (Object)null)
			{
				((Component)((BaseController<UIMultiTouchGuideView>)this).view.ZoomInBtn).gameObject.SetActive(active);
			}
			if ((Object)(object)((BaseController<UIMultiTouchGuideView>)this).view.ZoomOutBtn != (Object)null)
			{
				((Component)((BaseController<UIMultiTouchGuideView>)this).view.ZoomOutBtn).gameObject.SetActive(active2);
			}
			if ((Object)(object)((BaseController<UIMultiTouchGuideView>)this).view.MoveBtn != (Object)null)
			{
				((Component)((BaseController<UIMultiTouchGuideView>)this).view.MoveBtn).gameObject.SetActive(active3);
			}
		}
	}
}
