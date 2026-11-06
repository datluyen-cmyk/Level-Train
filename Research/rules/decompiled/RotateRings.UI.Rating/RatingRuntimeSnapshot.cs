namespace RotateRings.UI.Rating;

public readonly struct RatingRuntimeSnapshot
{
	public bool ExperimentEnabled { get; init; }

	public int CompletedLevel { get; init; }

	public float ValidDurationSeconds { get; init; }

	public int ActiveDays { get; init; }

	public bool NetworkAvailable { get; init; }

	public int PopupCount { get; init; }

	public bool HasGivenHighStar { get; init; }

	public int CloseCount { get; init; }

	public int LowStarSubmitCount { get; init; }

	public int RequiredActiveDayInterval { get; init; }

	public int LastActionActiveDays { get; init; }
}
