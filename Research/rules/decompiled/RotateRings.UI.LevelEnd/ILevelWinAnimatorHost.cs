using System;
using RotateRings.GameWinIQ;

namespace RotateRings.UI.LevelEnd;

public interface ILevelWinAnimatorHost
{
	UILevelWinView View { get; }

	int SettlementScore { get; }

	GameWinIQSettlementSnapshot IqSnapshot { get; }

	void OnScorePhaseStarted();

	void PlayChestProgress(float duration, Action onComplete);

	bool OnContentAppearStarted();

	void OnContentAppearCompleted();
}
