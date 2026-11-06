using XingYun.Account.CloudStorage;

namespace RotateRings.Services;

public sealed class CloudSaveSnapshotPullResult
{
	public bool IsSuccess { get; }

	public CloudSaveDataSnapshot Snapshot { get; }

	public string FailureReason { get; }

	public CloudStorageErrorCode? ErrorCode { get; }

	public CloudSaveSnapshotPullResult(bool isSuccess, CloudSaveDataSnapshot snapshot, string failureReason = null, CloudStorageErrorCode? errorCode = null)
	{
		IsSuccess = isSuccess;
		Snapshot = snapshot;
		FailureReason = failureReason;
		ErrorCode = errorCode;
	}
}
