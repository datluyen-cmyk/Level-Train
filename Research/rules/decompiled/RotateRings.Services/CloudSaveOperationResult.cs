using XingYun.Account.CloudStorage;

namespace RotateRings.Services;

public sealed class CloudSaveOperationResult
{
	public string OperationName { get; }

	public bool IsSuccess { get; }

	public bool IsInProgress { get; }

	public string Summary { get; }

	public CloudStorageErrorCode? ErrorCode { get; }

	public CloudSaveOperationResult(string operationName, bool isSuccess, bool isInProgress, string summary, CloudStorageErrorCode? errorCode = null)
	{
		OperationName = operationName;
		IsSuccess = isSuccess;
		IsInProgress = isInProgress;
		Summary = summary;
		ErrorCode = errorCode;
	}
}
