namespace RotateRings.Services;

public sealed class CloudSaveDataDecisionResult
{
	public CloudSaveMainLevelMergeResult MergeResult { get; }

	public CloudSaveDataSnapshot LocalData { get; }

	public CloudSaveDataSnapshot CloudData { get; }

	public long CloudStorageVersion { get; internal set; }

	public CloudSaveDataSnapshot Data { get; }

	public CloudSaveDataDecisionResult(CloudSaveMainLevelMergeResult mergeResult, CloudSaveDataSnapshot localData, CloudSaveDataSnapshot cloudData, CloudSaveDataSnapshot data)
	{
		MergeResult = mergeResult;
		LocalData = localData;
		CloudData = cloudData;
		Data = data;
	}
}
