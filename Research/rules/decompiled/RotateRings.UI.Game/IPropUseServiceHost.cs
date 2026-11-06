using System;
using RotateRings.GamePlay.Services.Prop;
using RotateRings.Prop;
using RotateRings.UI.PropGuide;

namespace RotateRings.UI.Game;

public interface IPropUseServiceHost
{
	bool IsUsingByAd { get; set; }

	UIGameView View { get; }

	SkyrocketPropBadgeProvider SkyrocketBadgeProvider { get; }

	IRocketPropUseCallback Callback { get; }

	bool CanStartProp(EPropType propType);

	void BeginUseProp(PropUseEvent eventData, TrackPropUseReason useReason);

	void UsePropByAd(PropUseEvent eventData);

	void UsePropByAd(PropUseEvent eventData, Action onGrantOnlyComplete);

	bool TryDirectUseSkyrocketAfterAd(PropUseEvent eventData);

	void SendPropUsingState(EPropType propType, bool isUsing);

	void DismissPropGuideSilently();

	bool TryOpenPropGuide(PropGuideOpenData data);

	void ClosePropGuide(bool isSuccessUse);

	void OnFastFireGuideClosed(PropUseEntrySource entrySource);

	void NotifyUnlockIntroCompleteIfNeeded(PropUseEntrySource entrySource);
}
