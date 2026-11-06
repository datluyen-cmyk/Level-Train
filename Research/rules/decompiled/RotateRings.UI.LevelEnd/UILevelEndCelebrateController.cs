using System;
using Game.UI;
using UnityEngine;
using XingYun.UI.Controller;
using XingYun.UI.Core;

namespace RotateRings.UI.LevelEnd;

[UIPath("UILevelEndCelebrate")]
public class UILevelEndCelebrateController : GameBaseController<UILevelEndCelebrateView>
{
	public sealed class OpenParam
	{
		public int WinCelebrateType { get; }

		public Action OnCompleted { get; }

		public OpenParam(int winCelebrateType, Action onCompleted)
		{
			WinCelebrateType = winCelebrateType;
			OnCompleted = onCompleted;
		}
	}

	private const string Tag = "[UILevelEndCelebrateController] ";

	private const float AutoCloseEntranceTimeout = 5f;

	private Action pendingCompletion;

	private bool isRemoved;

	private bool isCloseQueued;

	private bool isDeferredCloseInProgress;

	private int winCelebrateType;

	private int autoCloseEntranceTimeoutTimerId = -1;

	public UILevelEndCelebrateController(UILevelEndCelebrateView view)
		: base(view)
	{
	}

	public override void OnRegister(object param = null)
	{
		InitializePresentation(param);
		if (IsDeferredCloseType())
		{
			GameApp.Event.Listen<LevelEndCelebrateCloseRequestedEvent>((Action<LevelEndCelebrateCloseRequestedEvent>)OnCloseRequested);
		}
		((BaseController<UILevelEndCelebrateView>)this).view.ConfigureLogo(winCelebrateType);
		((BaseController<UILevelEndCelebrateView>)this).view.PlayCelebrateEffects(winCelebrateType);
		PlayEntranceAnimation();
	}

	private void InitializePresentation(object param)
	{
		isRemoved = false;
		isCloseQueued = false;
		isDeferredCloseInProgress = false;
		OpenParam openParam = param as OpenParam;
		winCelebrateType = openParam?.WinCelebrateType ?? ((param is int num) ? num : 0);
		pendingCompletion = openParam?.OnCompleted;
	}

	private void PlayEntranceAnimation()
	{
		if (IsDeferredCloseType())
		{
			((BaseController<UILevelEndCelebrateView>)this).view.PlayLogoOpen(OnEntranceAnimationCompleted, CloseForUnavailableSpineResource);
			return;
		}
		StartAutoCloseEntranceTimeout();
		((BaseController<UILevelEndCelebrateView>)this).view.PlayLogoOpen(PlayAutoCloseAnimation, CloseForUnavailableSpineResource);
	}

	private bool IsDeferredCloseType()
	{
		int num = winCelebrateType;
		return num == 5 || num == 6;
	}

	private void OnCloseRequested(LevelEndCelebrateCloseRequestedEvent eventData)
	{
		if (IsDeferredCloseType() && !isRemoved && !isDeferredCloseInProgress)
		{
			isDeferredCloseInProgress = true;
			PlayCloseAnimation();
		}
	}

	private void PlayAutoCloseAnimation()
	{
		StopAutoCloseEntranceTimeout();
		if (!isRemoved)
		{
			PlayCloseAnimation();
		}
	}

	private void PlayCloseAnimation()
	{
		if (isRemoved)
		{
			OnCloseAnimationCompleted();
			return;
		}
		((BaseController<UILevelEndCelebrateView>)this).view.PlayLogoClose(delegate
		{
			if (!isRemoved)
			{
				((BaseController<UILevelEndCelebrateView>)this).CloseView(false);
				OnCloseAnimationCompleted();
			}
		}, CloseForUnavailableSpineResource);
	}

	private void StartAutoCloseEntranceTimeout()
	{
		StopAutoCloseEntranceTimeout();
		autoCloseEntranceTimeoutTimerId = GameApp.Timer.Delay((Object)(object)((UIBaseView)((BaseController<UILevelEndCelebrateView>)this).view).Behaviour, 5f, (Action)OnAutoCloseEntranceTimedOut);
	}

	private void StopAutoCloseEntranceTimeout()
	{
		if (autoCloseEntranceTimeoutTimerId != -1)
		{
			GameApp.Timer.Kill(autoCloseEntranceTimeoutTimerId);
			autoCloseEntranceTimeoutTimerId = -1;
		}
	}

	private void OnAutoCloseEntranceTimedOut()
	{
		autoCloseEntranceTimeoutTimerId = -1;
		if (!isRemoved)
		{
			((BaseController<UILevelEndCelebrateView>)this).CloseView(false);
		}
	}

	private void CloseForUnavailableSpineResource()
	{
		StopAutoCloseEntranceTimeout();
		if (!isRemoved && !isCloseQueued)
		{
			isCloseQueued = true;
			GameApp.Timer.NextFrame((Object)(object)((UIBaseView)((BaseController<UILevelEndCelebrateView>)this).view).Behaviour, (Action)delegate
			{
				((BaseController<UILevelEndCelebrateView>)this).CloseView(false);
			});
		}
	}

	private void OnEntranceAnimationCompleted()
	{
		InvokePendingCompletion();
	}

	private void OnCloseAnimationCompleted()
	{
		if (isDeferredCloseInProgress)
		{
			isDeferredCloseInProgress = false;
			GameApp.Event.Send<LevelEndCelebrateCloseCompletedEvent>(new LevelEndCelebrateCloseCompletedEvent());
		}
	}

	private void InvokePendingCompletion()
	{
		Action action = pendingCompletion;
		pendingCompletion = null;
		action?.Invoke();
	}

	public override void OnRemove()
	{
		isRemoved = true;
		GameApp.Event.Unlisten<LevelEndCelebrateCloseRequestedEvent>((Action<LevelEndCelebrateCloseRequestedEvent>)OnCloseRequested);
		StopAutoCloseEntranceTimeout();
		base.OnRemove();
		if (isDeferredCloseInProgress)
		{
			OnCloseAnimationCompleted();
		}
		else
		{
			InvokePendingCompletion();
		}
	}

	public override void OnActive(bool isActive)
	{
		((BaseController<UILevelEndCelebrateView>)this).OnActive(isActive);
	}

	public override void OnEscape()
	{
		((BaseController<UILevelEndCelebrateView>)this).CloseView(false);
	}
}
