using Game.DataTrack;

namespace RotateRings.Services;

public sealed class CloudSaveSyncTrackData
{
	public string Result { get; }

	public int DurationMs { get; }

	public CloudSaveDataSnapshot LocalSnapshot { get; }

	public CloudSaveDataSnapshot CloudSnapshot { get; }

	public CloudSaveDataSnapshot FinalSnapshot { get; }

	public string SelectedSaveSource { get; }

	public string FailReason { get; }

	public bool IsConflict { get; }

	public CloudSaveSyncTrackData(string result, int durationMs, CloudSaveDataSnapshot localSnapshot, CloudSaveDataSnapshot cloudSnapshot, CloudSaveDataSnapshot finalSnapshot, string selectedSaveSource, string failReason, bool isConflict)
	{
		Result = result;
		DurationMs = durationMs;
		LocalSnapshot = localSnapshot;
		CloudSnapshot = cloudSnapshot;
		FinalSnapshot = finalSnapshot;
		SelectedSaveSource = selectedSaveSource;
		FailReason = failReason;
		IsConflict = isConflict;
	}

	public void Report(string triggerNode)
	{
		RRDataTrack.Instance.ReportSavedataSyncRequestResult(triggerNode, Result, DurationMs, LocalSnapshot, CloudSnapshot, FinalSnapshot, SelectedSaveSource, FailReason, IsConflict);
	}
}
