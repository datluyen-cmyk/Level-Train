using System;
using Rotaterings.Rank;

namespace RotateRings.RestartRetention;

public readonly struct RestartRetentionDecision
{
	public readonly int GroupId;

	public readonly int Streak;

	public readonly int Multiplier;

	public readonly int DisplayTier;

	public readonly RestartRetentionContent Content;

	public readonly bool CanRestart;

	public readonly bool UseBaseArt;

	public bool ShowPopup => Content != RestartRetentionContent.None;

	public bool ReturnToSettings { get; }

	public RestartRetentionDecision(RestartRetentionConfig config, int streak, RestartRetentionContent content, bool canRestart, bool useBaseArt = true)
	{
		GroupId = config.GroupId;
		Streak = Math.Max(0, streak);
		Multiplier = RankScoreService.GetMultiplier(Streak);
		DisplayTier = RankScoreService.GetTierIndex(Streak);
		Content = content;
		CanRestart = canRestart;
		UseBaseArt = useBaseArt;
		ReturnToSettings = config.CancelTarget == RestartRetentionCancelTarget.Settings;
	}
}
