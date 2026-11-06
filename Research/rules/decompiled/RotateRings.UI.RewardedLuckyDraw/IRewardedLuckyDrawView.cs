using System;
using Example.Cfg;
using UnityEngine.UI;

namespace RotateRings.UI.RewardedLuckyDraw;

public interface IRewardedLuckyDrawView
{
	Button CloseButton { get; }

	Button AdButton { get; }

	void InitializePresentation(RewardedLuckyDrawProbability[] slots, string[] iconPaths);

	void SetButtonsInteractable(bool interactable);

	void HideContentForClaim();

	void NotifyPopupMaskStackActive(bool isActive);

	void PlayResult(int slotId, RewardedLuckyDrawWheelAnimationSettings animation, Action onCompleted);

	void StopPresentation();
}
