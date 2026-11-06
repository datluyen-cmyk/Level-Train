using System;
using XingYun.UI.Core;

namespace RotateRings.WeakGuide.EventHandlers;

internal sealed class RingHintWeakGuideHandler : WeakGuideHandlerBase
{
	private bool isGuideVisible;

	private bool isSuppressed;

	public override string GuideId => "ring_hint";

	public override void OnSelected()
	{
		isSuppressed = false;
		SetHandlerActive(active: true);
		ShowGuide();
	}

	public override void OnInteractionStart()
	{
		if (base.IsActive)
		{
			isSuppressed = true;
			HideGuide();
		}
	}

	public override void OnPlayerAction()
	{
		if (base.IsActive)
		{
			isSuppressed = true;
			HideGuide();
		}
	}

	public override void OnEnterForeground()
	{
		if (base.IsActive && !isSuppressed)
		{
			ShowGuide();
		}
	}

	public override void OnEnterBackground()
	{
		if (base.IsActive)
		{
			HideGuide();
		}
	}

	protected override void OnStopHandler()
	{
		HideGuide();
		isSuppressed = false;
	}

	private void ShowGuide()
	{
		if (!isGuideVisible)
		{
			GameApp.UI.Open("WeakGuideView", (UILayer)2, (object)null, (Action)null);
			isGuideVisible = GameApp.UI.IsOpen("WeakGuideView");
		}
	}

	private void HideGuide()
	{
		if (isGuideVisible)
		{
			if (GameApp.UI.IsOpen("WeakGuideView"))
			{
				GameApp.UI.CloseView("WeakGuideView", (object)null, false);
			}
			isGuideVisible = false;
		}
	}
}
