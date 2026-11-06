using System;

namespace RotateRings.UI.PropGuide;

public interface IPropGuideStrategy
{
	void OnRegister(UIPropGuideView view, PropGuideOpenData data, Action onDismiss);

	void OnEnterSecondPhase(UIPropGuideView view);

	void OnRemove(UIPropGuideView view);
}
