using System;

namespace RotateRings.WeakGuide.EventHandlers;

public abstract class TimedWeakGuideHandlerBase : WeakGuideHandlerBase
{
	private readonly float guideShowDelay;

	private int guideTimerId = -1;

	protected WeakGuidePresentationState PresentationState { get; private set; }

	protected TimedWeakGuideHandlerBase(float guideShowDelay)
	{
		this.guideShowDelay = guideShowDelay;
	}

	protected abstract void HandleGuideTimerComplete();

	protected abstract void HideShownGuidePresentation();

	public override void OnSelected()
	{
		ResetRuntimeState();
		StartGuideTimer();
	}

	public override void OnDeselected()
	{
		ResetRuntimeState();
	}

	public override void OnPlayerAction()
	{
		if (base.IsActive)
		{
			StartGuideTimer();
			PresentationState = WeakGuidePresentationState.Suppressed;
		}
	}

	public override void OnInteractionStart()
	{
		if (base.IsActive)
		{
			StopGuideTimerAndPresentation();
			PresentationState = WeakGuidePresentationState.Suppressed;
		}
	}

	public override void OnInteractionEnd()
	{
		OnPlayerAction();
	}

	public override void OnEnterForeground()
	{
		if (base.IsActive)
		{
			StartGuideTimer();
		}
	}

	public override void OnEnterBackground()
	{
		if (base.IsActive)
		{
			StopGuideTimerAndPresentation();
		}
	}

	public override void OnLevelFinished()
	{
		ResetRuntimeState();
	}

	public override void Dispose()
	{
		ResetRuntimeState();
	}

	protected void MarkGuideShown()
	{
		PresentationState = WeakGuidePresentationState.Shown;
	}

	protected void StartGuideTimer()
	{
		StopGuideTimerAndPresentation();
		SetHandlerActive(active: true);
		guideTimerId = GameApp.Timer.Delay(guideShowDelay, (Action)OnGuideTimerCompleteInternal);
	}

	private void OnGuideTimerCompleteInternal()
	{
		guideTimerId = -1;
		if (base.IsActive)
		{
			HandleGuideTimerComplete();
		}
	}

	private void StopGuideTimerAndPresentation()
	{
		if (guideTimerId >= 0)
		{
			GameApp.Timer.Kill(guideTimerId);
			guideTimerId = -1;
		}
		if (PresentationState == WeakGuidePresentationState.Shown)
		{
			HideShownGuidePresentation();
		}
	}

	private void ResetRuntimeState()
	{
		StopGuideTimerAndPresentation();
		PresentationState = WeakGuidePresentationState.None;
		SetHandlerActive(active: false);
		OnStopHandler();
	}
}
