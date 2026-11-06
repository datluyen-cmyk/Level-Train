using System;

namespace RotateRings.WeakGuide.EventHandlers;

public abstract class WeakGuideHandlerBase : IWeakGuideHandler, IDisposable
{
	private bool isActive;

	public abstract string GuideId { get; }

	public bool IsActive => isActive;

	public abstract void OnSelected();

	public virtual void OnDeselected()
	{
		StopHandler();
	}

	public virtual void OnPlayerAction()
	{
	}

	public virtual void OnInteractionStart()
	{
	}

	public virtual void OnInteractionEnd()
	{
	}

	public virtual void OnEnterForeground()
	{
	}

	public virtual void OnEnterBackground()
	{
	}

	public virtual void OnLevelFinished()
	{
		StopHandler();
	}

	public virtual void Dispose()
	{
		StopHandler();
	}

	protected void SetHandlerActive(bool active)
	{
		isActive = active;
	}

	protected virtual void StopHandler()
	{
		isActive = false;
		OnStopHandler();
	}

	protected virtual void OnStopHandler()
	{
	}

	protected void LogInfo(string message)
	{
	}
}
