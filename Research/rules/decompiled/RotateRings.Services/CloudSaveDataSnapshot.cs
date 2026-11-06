using System;
using System.Collections.Generic;

namespace RotateRings.Services;

[Serializable]
public class CloudSaveDataSnapshot
{
	public string DeviceId;

	public string PlayerName;

	public int HeadId;

	public int MainLevelId;

	public List<CloudSavePropCount> PropCounts;

	public List<CloudSavePurchaseCount> PurchaseCounts;

	public long Version;

	public List<int> UnlockPropIds;

	public List<int> UsedPropIds;

	public List<int> GuidedPropIds;

	public List<int> GuideUserInteractedPropIds;

	public List<CloudSavePendingFreeProp> PendingFreeProps;

	public bool IsNewbieProp;

	public List<int> PendingUnlockIntroPropIds;

	public bool HasEverPurchased;

	public List<CloudSaveGoodsBuyCount> GoodsBuyCounts;

	public int DataSchemaVersion;

	public int EndlessLevelId;

	public List<CloudSavePurchaseCount> SyncedPurchaseCounts;
}
