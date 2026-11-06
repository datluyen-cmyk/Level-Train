using System;

namespace RotateRings.GamePlay.Services.Prop;

public interface IRotateRingPropHost
{
	bool IsLevelCompleted { get; }

	bool CanUseHammer();

	bool CanUseRocket();

	void BeginHammerRemoveMode(Action<HammerRemoveSelection> onSelected);

	void EndHammerRemoveMode();

	bool TryExecuteHammerRemove(IGameplayObject target);

	void ResetPropGestureCapture();
}
