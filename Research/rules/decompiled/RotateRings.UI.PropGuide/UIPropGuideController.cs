using System;
using Game.DataTrack;
using Game.UI;
using RotateRings.GamePlay.Events;
using RotateRings.Prop;
using RotateRings.UI.Game;
using RotateRings.UI.PropItem;
using UnityEngine;
using UnityEngine.Events;
using XingYun.Experiment;
using XingYun.UI.Controller;
using XingYun.UI.Core;

namespace RotateRings.UI.PropGuide;

[UIPath("UIPropGuide")]
public class UIPropGuideController : GameBaseController<UIPropGuideView>
{
	private const string Tag = "UI";

	private const float IdlePromptDelaySeconds = 5f;

	private PropGuideOpenData openData;

	private bool closedCallbackInvoked;

	private bool isClosing;

	private int closeTimerId = -1;

	private bool idleSessionActive;

	private int idleTimerId = -1;

	private IPropGuideStrategy strategy;

	public UIPropGuideController(UIPropGuideView view)
		: base(view)
	{
	}

	public override void OnRegister(object param = null)
	{
		ResetSessionState();
		openData = param as PropGuideOpenData;
		if (!TryInitializeView(openData))
		{
			((BaseController<UIPropGuideView>)this).CloseView(false);
			return;
		}
		if (openData != null)
		{
			GameApp.Proxy.Get<PropModelProxy>()?.MarkPropGuided(openData.PropType);
		}
		GameApp.Proxy.Get<PropModelProxy>()?.ClearGuideUserInteracted(openData.PropType);
		strategy = PropGuideStrategyFactory.Create(openData.PropType);
		BindEvents();
		((BaseController<UIPropGuideView>)this).view.OnTopContentShown = OnTopContentShown;
		((BaseController<UIPropGuideView>)this).view.OnItemTipPhaseEnded = OnGuideItemConfirmed;
		SetUIGameInputBlocked(blocked: true);
		strategy?.OnRegister(((BaseController<UIPropGuideView>)this).view, openData, HandleUserDismiss);
		if (!openData.IsFirstGuide)
		{
			strategy?.OnEnterSecondPhase(((BaseController<UIPropGuideView>)this).view);
		}
		((BaseController<UIPropGuideView>)this).view.PlayOpenAnimation(openData.IsFromUnlockView);
		if (openData.IsFirstGuide)
		{
			if (GetPropUseGuideTapAni() == 1)
			{
				((BaseController<UIPropGuideView>)this).view.ShowGuideHandButtonEffect();
			}
			else
			{
				TryStartIdlePromptSession();
			}
		}
	}

	public override void OnRemove()
	{
		bool num = openData != null && openData.EntrySource == PropUseEntrySource.UnlockIntro;
		if (closeTimerId > 0)
		{
			GameApp.Timer.Kill(closeTimerId);
			closeTimerId = -1;
		}
		StopIdlePromptSession();
		SetUIGameInputBlocked(blocked: false);
		strategy?.OnRemove(((BaseController<UIPropGuideView>)this).view);
		if (((BaseController<UIPropGuideView>)this).view != null)
		{
			((BaseController<UIPropGuideView>)this).view.OnTopContentShown = null;
			((BaseController<UIPropGuideView>)this).view.OnItemTipPhaseEnded = null;
		}
		UnbindEvents();
		InvokeClosedCallback();
		if (num)
		{
			GameApp.Event.Send<PropUnlockTutorialBlockingUiClosedEvent>(new PropUnlockTutorialBlockingUiClosedEvent());
		}
		base.OnRemove();
	}

	public override void OnEscape()
	{
		HandleUserDismiss();
	}

	private void HandleUserDismiss()
	{
		MarkUserInteracted();
		ReportGuideDismissTrack();
		ClosePopup();
	}

	private void BindEvents()
	{
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Expected O, but got Unknown
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Expected O, but got Unknown
		if ((Object)(object)((BaseController<UIPropGuideView>)this).view.Btnclose != (Object)null)
		{
			((UnityEvent)((BaseController<UIPropGuideView>)this).view.Btnclose.onClick).AddListener(new UnityAction(OnCloseClick));
		}
		if ((Object)(object)((BaseController<UIPropGuideView>)this).view.Mask != (Object)null)
		{
			((UnityEvent)((BaseController<UIPropGuideView>)this).view.Mask.onClick).AddListener(new UnityAction(OnMaskClick));
		}
		GameApp.Event.Listen<PropChangeEvent>((Action<PropChangeEvent>)OnPropChanged);
	}

	private void UnbindEvents()
	{
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Expected O, but got Unknown
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Expected O, but got Unknown
		if ((Object)(object)((BaseController<UIPropGuideView>)this).view.Btnclose != (Object)null)
		{
			((UnityEvent)((BaseController<UIPropGuideView>)this).view.Btnclose.onClick).RemoveListener(new UnityAction(OnCloseClick));
		}
		if ((Object)(object)((BaseController<UIPropGuideView>)this).view.Mask != (Object)null)
		{
			((UnityEvent)((BaseController<UIPropGuideView>)this).view.Mask.onClick).RemoveListener(new UnityAction(OnMaskClick));
		}
		GameApp.Event.Unlisten<PropChangeEvent>((Action<PropChangeEvent>)OnPropChanged);
	}

	private void OnCloseClick()
	{
		HandleUserDismiss();
	}

	private void OnMaskClick()
	{
		HandleUserDismiss();
	}

	private void OnGuideItemConfirmed()
	{
		MarkUserInteracted();
		StopIdlePromptSession();
	}

	private void OnTopContentShown()
	{
		strategy?.OnEnterSecondPhase(((BaseController<UIPropGuideView>)this).view);
	}

	private void ReportGuideDismissTrack()
	{
		EPropType ePropType = openData?.PropType ?? EPropType.None;
		if (ePropType != EPropType.None)
		{
			RRDataTrack.Instance.ReportCancelUsePropClick(ePropType);
		}
	}

	private void MarkUserInteracted()
	{
		if (openData != null)
		{
			GameApp.Proxy.Get<PropModelProxy>()?.MarkGuideUserInteracted(openData.PropType);
		}
	}

	private void OnPropChanged(PropChangeEvent eventData)
	{
		if (ShouldRefreshPropItem(eventData))
		{
			((BaseController<UIPropGuideView>)this).view.RefreshPropItem(eventData.PropType);
		}
	}

	internal void ClosePopup(bool isSuccessUse = false)
	{
		if (isClosing)
		{
			return;
		}
		isClosing = true;
		if (isSuccessUse)
		{
			MarkUserInteracted();
		}
		if (!isSuccessUse)
		{
			InvokeClosedCallback();
		}
		StopIdlePromptSession();
		strategy?.OnRemove(((BaseController<UIPropGuideView>)this).view);
		strategy = null;
		float num = ((BaseController<UIPropGuideView>)this).view.PlayCloseAnimation();
		if (num > 0f)
		{
			closeTimerId = GameApp.Timer.Delay((Object)(object)((UIBaseView)((BaseController<UIPropGuideView>)this).view).Behaviour, num, (Action)delegate
			{
				((BaseController<UIPropGuideView>)this).CloseView(false);
			});
		}
		else
		{
			((BaseController<UIPropGuideView>)this).CloseView(false);
		}
	}

	private bool TryInitializeView(PropGuideOpenData data)
	{
		if (data == null)
		{
			return false;
		}
		if (data.PropType == EPropType.None)
		{
			return false;
		}
		if (!((BaseController<UIPropGuideView>)this).view.Init(data))
		{
			return false;
		}
		return true;
	}

	private void ResetSessionState()
	{
		openData = null;
		closedCallbackInvoked = false;
		isClosing = false;
		closeTimerId = -1;
		idleSessionActive = false;
		idleTimerId = -1;
		strategy = null;
	}

	private static int GetPropUseGuideTapAni()
	{
		PropFunction propFunction = FunctionDataCenter.Get<PropFunction>();
		if (propFunction != null)
		{
			return Mathf.Max(0, propFunction.PropUseGuideTapAni);
		}
		return 0;
	}

	private void TryStartIdlePromptSession()
	{
		if (((BaseController<UIPropGuideView>)this).view != null)
		{
			idleSessionActive = true;
			ResetIdlePromptTimer();
		}
	}

	private void ResetIdlePromptTimer()
	{
		if (idleSessionActive && ((BaseController<UIPropGuideView>)this).view != null)
		{
			KillIdleTimers();
			MonoBehaviour val = TryGetIdleTimerHost();
			if (!((Object)(object)val == (Object)null))
			{
				idleTimerId = GameApp.Timer.Repeat((Object)(object)val, 5f, (Action)OnIdlePromptElapsed);
			}
		}
	}

	private void StopIdlePromptSession()
	{
		idleSessionActive = false;
		KillIdleTimers();
		((BaseController<UIPropGuideView>)this).view?.GetGuidePropItemView()?.StopPropItemIdleAni();
		((BaseController<UIPropGuideView>)this).view?.HideGuideHandButtonEffect();
	}

	private void OnIdlePromptElapsed()
	{
		if (idleSessionActive && ((BaseController<UIPropGuideView>)this).view != null)
		{
			((BaseController<UIPropGuideView>)this).view.GetGuidePropItemView()?.PlayPropItemIdleAni();
		}
	}

	private void KillIdleTimers()
	{
		if (idleTimerId >= 0)
		{
			GameApp.Timer.Kill(idleTimerId);
			idleTimerId = -1;
		}
	}

	private MonoBehaviour TryGetIdleTimerHost()
	{
		UIPropItemView uIPropItemView = ((BaseController<UIPropGuideView>)this).view?.GetGuidePropItemView();
		if ((Object)(object)((uIPropItemView != null) ? ((UIBaseView)uIPropItemView).Behaviour : null) != (Object)null)
		{
			return (MonoBehaviour)(object)((UIBaseView)uIPropItemView).Behaviour;
		}
		UIPropGuideView view = ((BaseController<UIPropGuideView>)this).view;
		if (view == null)
		{
			return null;
		}
		return (MonoBehaviour)(object)((UIBaseView)view).Behaviour;
	}

	private void InvokeClosedCallback()
	{
		if (!closedCallbackInvoked)
		{
			closedCallbackInvoked = true;
			openData?.OnClosed?.Invoke();
		}
	}

	private static void SetUIGameInputBlocked(bool blocked)
	{
		if (TryGetUIGameController(out var gameController))
		{
			gameController.View.SetInputBlocked(blocked);
		}
	}

	private static bool TryGetUIGameController(out UIGameController gameController)
	{
		gameController = null;
		IController val = default(IController);
		if (!GameApp.UI.IsOpen("UIGame", ref val) || !(val is UIGameController uIGameController))
		{
			return false;
		}
		gameController = uIGameController;
		return true;
	}

	private bool ShouldRefreshPropItem(PropChangeEvent eventData)
	{
		if (eventData == null || openData == null)
		{
			return false;
		}
		if (eventData.PropType == EPropType.None)
		{
			return false;
		}
		return eventData.PropType == openData.PropType;
	}
}
