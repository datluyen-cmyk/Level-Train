using System;

namespace RotateRings.WeakGuide.EventHandlers;

public interface IWeakGuideHandler : IDisposable
{
	string GuideId { get; }

	bool IsActive { get; }

	void OnSelected();

	void OnDeselected();

	void OnPlayerAction();

	void OnInteractionStart();

	void OnInteractionEnd();

	void OnEnterForeground();

	void OnEnterBackground();

	void OnLevelFinished();
}
