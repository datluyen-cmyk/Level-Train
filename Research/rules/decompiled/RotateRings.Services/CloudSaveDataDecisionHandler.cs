using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using RotateRing.GamePlay;
using RotateRings.Model;
using UniLog;
using XingYun.Account.CloudStorage;

namespace RotateRings.Services;

public sealed class CloudSaveDataDecisionHandler : ICloudStorageConflictResolver
{
	private const string Tag = "[CloudSaveDecision]";

	public CloudSaveDataDecisionResult LastResult { get; private set; }

	public void Reset()
	{
		LastResult = null;
	}

	public CloudSaveDataDecisionResult Decide(CloudSaveDataSnapshot localData, CloudSaveDataSnapshot cloudData)
	{
		if (localData == null && cloudData == null)
		{
			return CreateResult(CloudSaveMainLevelMergeResult.SyncFailed, null);
		}
		if (CloudSaveDataHandler.IsSchemaNewerThanClient(cloudData))
		{
			return CreateResult(CloudSaveMainLevelMergeResult.SyncFailed, null);
		}
		if (localData == null)
		{
			return CreateResult(CloudSaveMainLevelMergeResult.UseCloudNoUpload, null, cloudData, CloneSnapshot(cloudData));
		}
		if (cloudData == null)
		{
			return CreateLocalUploadResult(localData, cloudData);
		}
		CloudSaveDataHandler.UpgradeSchemaFromLocal(cloudData, localData);
		if (cloudData.Version == localData.Version)
		{
			return CreateLocalUploadResult(localData, cloudData);
		}
		CloudSaveDataSnapshot resultData;
		if (cloudData.Version > localData.Version)
		{
			return CreateResult(ResolveByLevelProgress(localData, cloudData, out resultData), localData, cloudData, resultData);
		}
		Log.LogError("[CloudSaveDecision]", "本地云存档业务版本高于云端，按云端版本更高的决策路径处理 localVersion={0} cloudVersion={1}", new object[2] { localData.Version, cloudData.Version });
		CloudSaveDataSnapshot resultData2;
		return CreateResult(ResolveByLevelProgress(localData, cloudData, out resultData2), localData, cloudData, resultData2);
	}

	public UniTask<IReadOnlyDictionary<string, ConflictAction>> ResolveAsync(IReadOnlyList<CloudStorageMergeContext> contexts, CancellationToken cancellationToken)
	{
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		cancellationToken.ThrowIfCancellationRequested();
		Dictionary<string, ConflictAction> dictionary = new Dictionary<string, ConflictAction>(1);
		LastResult = null;
		if (contexts == null || contexts.Count == 0)
		{
			return UniTask.FromResult<IReadOnlyDictionary<string, ConflictAction>>((IReadOnlyDictionary<string, ConflictAction>)dictionary);
		}
		_ = contexts.Count;
		_ = 1;
		CloudStorageMergeContext val = contexts[0];
		if (val == null || string.IsNullOrEmpty(val.SystemType))
		{
			return UniTask.FromResult<IReadOnlyDictionary<string, ConflictAction>>((IReadOnlyDictionary<string, ConflictAction>)dictionary);
		}
		CloudSaveDataDecisionResult cloudSaveDataDecisionResult = ResolveContext(val);
		Dictionary<string, ConflictAction> dictionary2 = dictionary;
		string systemType = val.SystemType;
		dictionary2[systemType] = (ConflictAction)(cloudSaveDataDecisionResult.MergeResult switch
		{
			CloudSaveMainLevelMergeResult.UseCloudNoUpload => 0, 
			CloudSaveMainLevelMergeResult.UseCloudNeedUpload => 1, 
			CloudSaveMainLevelMergeResult.UseLocal => 1, 
			_ => 3, 
		});
		LastResult = cloudSaveDataDecisionResult;
		return UniTask.FromResult<IReadOnlyDictionary<string, ConflictAction>>((IReadOnlyDictionary<string, ConflictAction>)dictionary);
	}

	private CloudSaveDataDecisionResult ResolveContext(CloudStorageMergeContext context)
	{
		CloudSaveDataSnapshot snapshot;
		bool isLocalDataValid = CloudSaveDataHandler.TryDeserializeSnapshot(context.LocalRawData, out snapshot);
		CloudSaveDataSnapshot snapshot2;
		bool isCloudDataValid = CloudSaveDataHandler.TryDeserializeSnapshot(context.CloudRawData, out snapshot2);
		if (HasInvalidRawData(context, isLocalDataValid, isCloudDataValid))
		{
			return CreateResult(CloudSaveMainLevelMergeResult.SyncFailed, null);
		}
		CloudSaveDataDecisionResult cloudSaveDataDecisionResult = Decide(snapshot, snapshot2);
		cloudSaveDataDecisionResult.CloudStorageVersion = Math.Max(0L, context.CloudVersion);
		return cloudSaveDataDecisionResult;
	}

	private static bool HasInvalidRawData(CloudStorageMergeContext context, bool isLocalDataValid, bool isCloudDataValid)
	{
		bool num = context.LocalRawData != null && context.LocalRawData.Length != 0;
		bool flag = context.CloudRawData != null && context.CloudRawData.Length != 0;
		if (!num || isLocalDataValid)
		{
			if (flag)
			{
				return !isCloudDataValid;
			}
			return false;
		}
		return true;
	}

	private static CloudSaveMainLevelMergeResult ResolveByLevelProgress(CloudSaveDataSnapshot localData, CloudSaveDataSnapshot cloudData, out CloudSaveDataSnapshot resultData)
	{
		if (localData.MainLevelId > cloudData.MainLevelId || (localData.MainLevelId == cloudData.MainLevelId && localData.EndlessLevelId > cloudData.EndlessLevelId))
		{
			resultData = CloneSnapshot(localData);
			MergeHasEverPurchased(resultData, cloudData);
			MergeCloudPurchaseIntoLocal(resultData, localData, cloudData);
			PrepareUploadSnapshot(resultData, localData, cloudData);
			return CloudSaveMainLevelMergeResult.UseLocal;
		}
		LogIfLocalSyncedPurchaseAhead(localData, cloudData);
		resultData = CloneSnapshot(cloudData);
		bool num = AddPendingPurchaseProps(resultData, localData.PurchaseCounts);
		if (num)
		{
			resultData.SyncedPurchaseCounts = CloudSaveDataHandler.SumPurchaseCounts(cloudData.SyncedPurchaseCounts, localData.PurchaseCounts);
		}
		bool flag = MergeHasEverPurchased(resultData, localData);
		if (num || flag)
		{
			PrepareUploadSnapshot(resultData, localData, cloudData);
			return CloudSaveMainLevelMergeResult.UseCloudNeedUpload;
		}
		resultData.Version = cloudData.Version;
		return CloudSaveMainLevelMergeResult.UseCloudNoUpload;
	}

	private static CloudSaveDataDecisionResult CreateLocalUploadResult(CloudSaveDataSnapshot localData, CloudSaveDataSnapshot cloudData)
	{
		CloudSaveDataSnapshot cloudSaveDataSnapshot = CloneSnapshot(localData);
		MergeHasEverPurchased(cloudSaveDataSnapshot, cloudData);
		MergeCloudPurchaseIntoLocal(cloudSaveDataSnapshot, localData, cloudData);
		PrepareUploadSnapshot(cloudSaveDataSnapshot, localData, cloudData);
		return CreateResult(CloudSaveMainLevelMergeResult.UseLocal, localData, cloudData, cloudSaveDataSnapshot);
	}

	private static void PrepareUploadSnapshot(CloudSaveDataSnapshot resultData, CloudSaveDataSnapshot localData, CloudSaveDataSnapshot cloudData)
	{
		resultData.Version = GetNextVersion(localData, cloudData);
		ClearPendingPurchaseCounts(resultData);
	}

	private static bool HasMainLevelConfig(int mainLevelId)
	{
		if (mainLevelId > 0)
		{
			if (mainLevelId != LevelConfigService.MainLevelMax + 1)
			{
				return LevelConfigService.GetLevelConfig(GameMode.Main, mainLevelId) != null;
			}
			return true;
		}
		return false;
	}

	private static bool AddPendingPurchaseProps(CloudSaveDataSnapshot target, List<CloudSavePurchaseCount> pendingPurchaseCounts)
	{
		if (pendingPurchaseCounts == null || pendingPurchaseCounts.Count == 0)
		{
			return false;
		}
		Dictionary<int, int> dictionary = new Dictionary<int, int>(target.PropCounts?.Count ?? 0);
		if (target.PropCounts != null)
		{
			for (int i = 0; i < target.PropCounts.Count; i++)
			{
				CloudSavePropCount cloudSavePropCount = target.PropCounts[i];
				if (cloudSavePropCount != null && cloudSavePropCount.PropId >= 0)
				{
					dictionary[cloudSavePropCount.PropId] = Math.Max(0, cloudSavePropCount.Count);
				}
			}
		}
		bool flag = false;
		for (int j = 0; j < pendingPurchaseCounts.Count; j++)
		{
			CloudSavePurchaseCount cloudSavePurchaseCount = pendingPurchaseCounts[j];
			if (cloudSavePurchaseCount != null && cloudSavePurchaseCount.PropId >= 0 && cloudSavePurchaseCount.Count > 0)
			{
				dictionary.TryGetValue(cloudSavePurchaseCount.PropId, out var value);
				dictionary[cloudSavePurchaseCount.PropId] = value + cloudSavePurchaseCount.Count;
				flag = true;
			}
		}
		if (!flag)
		{
			return false;
		}
		target.PropCounts = new List<CloudSavePropCount>(dictionary.Count);
		foreach (KeyValuePair<int, int> item in dictionary)
		{
			target.PropCounts.Add(new CloudSavePropCount
			{
				PropId = item.Key,
				Count = item.Value
			});
		}
		target.PropCounts.Sort((CloudSavePropCount left, CloudSavePropCount right) => left.PropId.CompareTo(right.PropId));
		return true;
	}

	private static void MergeCloudPurchaseIntoLocal(CloudSaveDataSnapshot resultData, CloudSaveDataSnapshot localData, CloudSaveDataSnapshot cloudData)
	{
		LogIfLocalSyncedPurchaseAhead(localData, cloudData);
		Dictionary<int, int> dictionary = ToPurchaseCountDict(localData.SyncedPurchaseCounts);
		List<CloudSavePurchaseCount> list = new List<CloudSavePurchaseCount>();
		List<CloudSavePurchaseCount> list2 = new List<CloudSavePurchaseCount>(dictionary.Count);
		foreach (KeyValuePair<int, int> item in ToPurchaseCountDict(cloudData?.SyncedPurchaseCounts))
		{
			dictionary.TryGetValue(item.Key, out var value);
			if (item.Value > value)
			{
				list.Add(new CloudSavePurchaseCount
				{
					PropId = item.Key,
					Count = item.Value - value
				});
				dictionary[item.Key] = item.Value;
			}
		}
		foreach (KeyValuePair<int, int> item2 in dictionary)
		{
			list2.Add(new CloudSavePurchaseCount
			{
				PropId = item2.Key,
				Count = item2.Value
			});
		}
		AddPendingPurchaseProps(resultData, list);
		resultData.SyncedPurchaseCounts = CloudSaveDataHandler.SumPurchaseCounts(list2, localData.PurchaseCounts);
	}

	private static void LogIfLocalSyncedPurchaseAhead(CloudSaveDataSnapshot localData, CloudSaveDataSnapshot cloudData)
	{
		if (cloudData == null)
		{
			return;
		}
		Dictionary<int, int> dictionary = ToPurchaseCountDict(cloudData.SyncedPurchaseCounts);
		foreach (KeyValuePair<int, int> item in ToPurchaseCountDict(localData.SyncedPurchaseCounts))
		{
			dictionary.TryGetValue(item.Key, out var value);
			if (item.Value > value)
			{
				Log.LogError("[CloudSaveDecision]", "本地已同步购买量高于云端 propId={0} local={1} cloud={2}", new object[3] { item.Key, item.Value, value });
			}
		}
	}

	private static Dictionary<int, int> ToPurchaseCountDict(List<CloudSavePurchaseCount> counts)
	{
		Dictionary<int, int> dictionary = new Dictionary<int, int>(counts?.Count ?? 0);
		if (counts == null)
		{
			return dictionary;
		}
		for (int i = 0; i < counts.Count; i++)
		{
			CloudSavePurchaseCount cloudSavePurchaseCount = counts[i];
			if (cloudSavePurchaseCount != null && cloudSavePurchaseCount.PropId >= 0 && cloudSavePurchaseCount.Count > 0)
			{
				dictionary[cloudSavePurchaseCount.PropId] = cloudSavePurchaseCount.Count;
			}
		}
		return dictionary;
	}

	private static bool MergeHasEverPurchased(CloudSaveDataSnapshot target, CloudSaveDataSnapshot source)
	{
		bool flag = target?.HasEverPurchased ?? false;
		bool flag2 = source?.HasEverPurchased ?? false;
		target.HasEverPurchased = flag || flag2;
		if (flag2)
		{
			return !flag;
		}
		return false;
	}

	private static void ClearPendingPurchaseCounts(CloudSaveDataSnapshot snapshot)
	{
		snapshot.PurchaseCounts = new List<CloudSavePurchaseCount>();
	}

	private static long GetNextVersion(CloudSaveDataSnapshot localData, CloudSaveDataSnapshot cloudData)
	{
		return Math.Max(localData?.Version ?? 0, cloudData?.Version ?? 0) + 1;
	}

	private static CloudSaveDataDecisionResult CreateResult(CloudSaveMainLevelMergeResult mergeResult, CloudSaveDataSnapshot data)
	{
		return new CloudSaveDataDecisionResult(mergeResult, null, null, data);
	}

	private static CloudSaveDataDecisionResult CreateResult(CloudSaveMainLevelMergeResult mergeResult, CloudSaveDataSnapshot localData, CloudSaveDataSnapshot cloudData, CloudSaveDataSnapshot data)
	{
		return new CloudSaveDataDecisionResult(mergeResult, localData, cloudData, data);
	}

	private static CloudSaveDataSnapshot CloneSnapshot(CloudSaveDataSnapshot source)
	{
		if (source == null)
		{
			return null;
		}
		CloudSaveDataHandler.TryDeserializeSnapshot(CloudSaveDataHandler.SerializeSnapshot(source), out var snapshot);
		return snapshot;
	}
}
