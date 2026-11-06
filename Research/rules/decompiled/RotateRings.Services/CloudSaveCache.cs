using System;
using System.Collections.Generic;

namespace RotateRings.Services;

public static class CloudSaveCache
{
	public static CloudSaveCacheData Data => Load();

	public static void AddPendingPurchasePropCounts(IReadOnlyDictionary<int, int> propCounts)
	{
		if (propCounts == null)
		{
			return;
		}
		CloudSaveCacheData data = Data;
		foreach (KeyValuePair<int, int> propCount in propCounts)
		{
			if (propCount.Key >= 0 && propCount.Value > 0)
			{
				data.PendingCloudPurchasePropCounts.TryGetValue(propCount.Key, out var value);
				data.PendingCloudPurchasePropCounts[propCount.Key] = value + propCount.Value;
			}
		}
	}

	public static void CommitPurchaseSync(List<CloudSavePurchaseCount> syncedCounts, List<CloudSavePurchaseCount> mergedPendingCounts)
	{
		CloudSaveCacheData data = Data;
		data.SyncedCloudPurchasePropCounts.Clear();
		if (syncedCounts != null)
		{
			for (int i = 0; i < syncedCounts.Count; i++)
			{
				CloudSavePurchaseCount cloudSavePurchaseCount = syncedCounts[i];
				if (cloudSavePurchaseCount != null && cloudSavePurchaseCount.PropId >= 0 && cloudSavePurchaseCount.Count > 0)
				{
					data.SyncedCloudPurchasePropCounts[cloudSavePurchaseCount.PropId] = cloudSavePurchaseCount.Count;
				}
			}
		}
		if (mergedPendingCounts == null)
		{
			data.PendingCloudPurchasePropCounts.Clear();
			return;
		}
		for (int j = 0; j < mergedPendingCounts.Count; j++)
		{
			CloudSavePurchaseCount cloudSavePurchaseCount2 = mergedPendingCounts[j];
			if (cloudSavePurchaseCount2 != null && data.PendingCloudPurchasePropCounts.TryGetValue(cloudSavePurchaseCount2.PropId, out var value))
			{
				int num = value - cloudSavePurchaseCount2.Count;
				if (num > 0)
				{
					data.PendingCloudPurchasePropCounts[cloudSavePurchaseCount2.PropId] = num;
				}
				else
				{
					data.PendingCloudPurchasePropCounts.Remove(cloudSavePurchaseCount2.PropId);
				}
			}
		}
	}

	public static void SetPendingForceCloudUpload(bool value)
	{
		Data.PendingForceCloudUpload = value;
	}

	public static void SetPendingBindSelection(bool value)
	{
		Data.PendingBindSelection = value;
	}

	private static CloudSaveCacheData Load()
	{
		CloudSaveCacheData orCreate = GameApp.Save.GetOrCreate<CloudSaveCacheData>();
		CloudSaveCacheData cloudSaveCacheData = orCreate;
		if (cloudSaveCacheData.PendingCloudPurchasePropCounts == null)
		{
			cloudSaveCacheData.PendingCloudPurchasePropCounts = new Dictionary<int, int>();
		}
		cloudSaveCacheData = orCreate;
		if (cloudSaveCacheData.SyncedCloudPurchasePropCounts == null)
		{
			cloudSaveCacheData.SyncedCloudPurchasePropCounts = new Dictionary<int, int>();
		}
		orCreate.CloudSaveDataVersion = Math.Max(0L, orCreate.CloudSaveDataVersion);
		return orCreate;
	}
}
