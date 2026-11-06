using System;
using System.Collections.Generic;
using System.Text;
using RotateRing.GamePlay;
using RotateRings.Common;
using RotateRings.Model;
using RotateRings.Prop;
using RotateRings.Shop;
using UniLog;
using UnityEngine;
using XingYun.Account;

namespace RotateRings.Services;

public static class CloudSaveDataHandler
{
	private const string Tag = "[CloudSaveData]";

	internal const int CurrentDataSchemaVersion = 3;

	public static CloudSaveDataSnapshot CaptureSnapshot()
	{
		PlayerModelProxy playerModelProxy = GameApp.Proxy.Get<PlayerModelProxy>();
		LevelModelProxy levelModelProxy = GameApp.Proxy.Get<LevelModelProxy>();
		PropModelProxy propModelProxy = GameApp.Proxy.Get<PropModelProxy>();
		ShopModelProxy shop = GameApp.Proxy.Get<ShopModelProxy>();
		CloudSaveCacheData data = CloudSaveCache.Data;
		return new CloudSaveDataSnapshot
		{
			DataSchemaVersion = 3,
			Version = Math.Max(0L, data.CloudSaveDataVersion),
			DeviceId = GetCurrentDeviceId(),
			PlayerName = (playerModelProxy?.GetName() ?? PlayerService.GetName()),
			HeadId = (playerModelProxy?.GetHeadId() ?? PlayerService.GetHeadId()),
			MainLevelId = (levelModelProxy?.MainLevel ?? 1),
			EndlessLevelId = GetCurrentEndlessLevelId(levelModelProxy),
			PropCounts = CapturePropCounts(propModelProxy),
			PurchaseCounts = CapturePurchasePropCounts(data.PendingCloudPurchasePropCounts),
			SyncedPurchaseCounts = CapturePurchasePropCounts(data.SyncedCloudPurchasePropCounts),
			GoodsBuyCounts = CaptureGoodsBuyCounts(shop),
			HasEverPurchased = data.HasEverPurchased,
			UnlockPropIds = CaptureUnlockPropIds(propModelProxy),
			UsedPropIds = CaptureInGamePropIds(propModelProxy?.Data.UsedPropList, propModelProxy),
			GuidedPropIds = CaptureInGamePropIds(propModelProxy?.Data.GuidedPropList, propModelProxy),
			GuideUserInteractedPropIds = CaptureInGamePropIds(propModelProxy?.Data.GuideUserInteractedPropList, propModelProxy),
			PendingFreeProps = CapturePendingFreeProps(propModelProxy),
			IsNewbieProp = (propModelProxy?.Data.IsNewbieProp ?? false),
			PendingUnlockIntroPropIds = CaptureInGamePropIds(propModelProxy?.Data.PendingUnlockIntroPropList, propModelProxy)
		};
	}

	public static byte[] SerializeCurrentSnapshot()
	{
		return SerializeSnapshot(CaptureSnapshot());
	}

	public static byte[] SerializeSnapshot(CloudSaveDataSnapshot snapshot)
	{
		if (snapshot == null)
		{
			throw new ArgumentNullException("snapshot");
		}
		return Encoding.UTF8.GetBytes(JsonUtility.ToJson((object)snapshot));
	}

	public static bool AreRawDataEqual(byte[] left, byte[] right)
	{
		if (left == right)
		{
			return true;
		}
		if (left == null || right == null || left.Length != right.Length)
		{
			return false;
		}
		for (int i = 0; i < left.Length; i++)
		{
			if (left[i] != right[i])
			{
				return false;
			}
		}
		return true;
	}

	public static bool AreSnapshotsEqual(CloudSaveDataSnapshot left, CloudSaveDataSnapshot right)
	{
		if (left == right)
		{
			return true;
		}
		if (left == null || right == null || left.Version != right.Version || left.DeviceId != right.DeviceId || left.PlayerName != right.PlayerName || left.HeadId != right.HeadId || left.MainLevelId != right.MainLevelId || left.EndlessLevelId != right.EndlessLevelId || left.HasEverPurchased != right.HasEverPurchased || left.PropCounts == null || right.PropCounts == null || left.PurchaseCounts == null || right.PurchaseCounts == null || left.UnlockPropIds == null || right.UnlockPropIds == null || left.PropCounts.Count != right.PropCounts.Count || left.PurchaseCounts.Count != right.PurchaseCounts.Count || left.UnlockPropIds.Count != right.UnlockPropIds.Count)
		{
			return false;
		}
		for (int i = 0; i < left.PropCounts.Count; i++)
		{
			CloudSavePropCount cloudSavePropCount = left.PropCounts[i];
			CloudSavePropCount cloudSavePropCount2 = right.PropCounts[i];
			if (cloudSavePropCount == null || cloudSavePropCount2 == null || cloudSavePropCount.PropId != cloudSavePropCount2.PropId || cloudSavePropCount.Count != cloudSavePropCount2.Count)
			{
				return false;
			}
		}
		for (int j = 0; j < left.PurchaseCounts.Count; j++)
		{
			CloudSavePurchaseCount cloudSavePurchaseCount = left.PurchaseCounts[j];
			CloudSavePurchaseCount cloudSavePurchaseCount2 = right.PurchaseCounts[j];
			if (cloudSavePurchaseCount == null || cloudSavePurchaseCount2 == null || cloudSavePurchaseCount.PropId != cloudSavePurchaseCount2.PropId || cloudSavePurchaseCount.Count != cloudSavePurchaseCount2.Count)
			{
				return false;
			}
		}
		for (int k = 0; k < left.UnlockPropIds.Count; k++)
		{
			if (left.UnlockPropIds[k] != right.UnlockPropIds[k])
			{
				return false;
			}
		}
		if (left.IsNewbieProp == right.IsNewbieProp && AreIntListsEqual(left.UsedPropIds, right.UsedPropIds) && AreIntListsEqual(left.GuidedPropIds, right.GuidedPropIds) && AreIntListsEqual(left.GuideUserInteractedPropIds, right.GuideUserInteractedPropIds) && ArePendingFreePropsEqual(left.PendingFreeProps, right.PendingFreeProps) && AreIntListsEqual(left.PendingUnlockIntroPropIds, right.PendingUnlockIntroPropIds))
		{
			return ArePurchaseCountsEqual(left.SyncedPurchaseCounts, right.SyncedPurchaseCounts);
		}
		return false;
	}

	public static bool TryDeserializeSnapshot(byte[] rawData, out CloudSaveDataSnapshot snapshot)
	{
		snapshot = null;
		if (rawData == null || rawData.Length == 0)
		{
			return false;
		}
		try
		{
			string text = Encoding.UTF8.GetString(rawData);
			snapshot = JsonUtility.FromJson<CloudSaveDataSnapshot>(text);
		}
		catch (Exception ex)
		{
			Log.LogError("[CloudSaveData]", "云存档 JSON 数据反序列化异常: {0}", new object[1] { ex });
			return false;
		}
		if (snapshot == null)
		{
			return false;
		}
		InitializeMissingPropState(snapshot);
		if (!IsSnapshotValid(snapshot))
		{
			snapshot = null;
			return false;
		}
		return true;
	}

	public static bool IsSchemaNewerThanClient(CloudSaveDataSnapshot snapshot)
	{
		if (snapshot != null)
		{
			return snapshot.DataSchemaVersion > 3;
		}
		return false;
	}

	public static void UpgradeSchemaFromLocal(CloudSaveDataSnapshot snapshot, CloudSaveDataSnapshot localData)
	{
		if (snapshot != null && localData != null && snapshot.DataSchemaVersion < 3)
		{
			if (snapshot.DataSchemaVersion < 2)
			{
				snapshot.EndlessLevelId = localData.EndlessLevelId;
			}
			snapshot.DataSchemaVersion = 3;
		}
	}

	public static bool TryApplySnapshot(CloudSaveDataSnapshot snapshot, bool commitCloudSyncState, List<CloudSavePurchaseCount> mergedPendingPurchaseCounts)
	{
		if (!IsSnapshotValid(snapshot))
		{
			return false;
		}
		PlayerModelProxy playerModelProxy = GameApp.Proxy.Get<PlayerModelProxy>();
		LevelModelProxy levelModelProxy = GameApp.Proxy.Get<LevelModelProxy>();
		PropModelProxy propModelProxy = GameApp.Proxy.Get<PropModelProxy>();
		ShopModelProxy shopModelProxy = GameApp.Proxy.Get<ShopModelProxy>();
		if (playerModelProxy == null || levelModelProxy == null || propModelProxy == null || shopModelProxy == null)
		{
			return false;
		}
		playerModelProxy.SetName(snapshot.PlayerName);
		playerModelProxy.SetHeadId(snapshot.HeadId);
		PlayerService.SetName(snapshot.PlayerName);
		PlayerService.SetHeadId(snapshot.HeadId);
		levelModelProxy.Data.GetGameModeData(GameMode.Main).level = snapshot.MainLevelId;
		levelModelProxy.Data.GetGameModeData(GameMode.Endless).level = snapshot.EndlessLevelId;
		levelModelProxy.Data.RefreshCurrentGameMode(LevelConfigService.MainLevelMax, LevelConfigService.CurrentEndlessSeason);
		levelModelProxy.SyncGameInfo();
		List<CloudSavePropCount> list = snapshot.PropCounts;
		if (commitCloudSyncState)
		{
			CloudSaveCache.CommitPurchaseSync(snapshot.SyncedPurchaseCounts, mergedPendingPurchaseCounts);
			CloudSaveCache.Data.CloudSaveDataVersion = Math.Max(0L, snapshot.Version);
			list = AddPurchasePropCounts(list, CloudSaveCache.Data.PendingCloudPurchasePropCounts);
		}
		ApplyPropCounts(propModelProxy, list);
		ApplyGoodsBuyCounts(shopModelProxy, snapshot.GoodsBuyCounts);
		ApplyUnlockPropIds(propModelProxy, snapshot.UnlockPropIds);
		ApplyPropState(propModelProxy, snapshot);
		CloudSaveCache.Data.HasEverPurchased |= snapshot.HasEverPurchased;
		GameApp.Event.Send<CloudSaveDataAppliedEvent>(new CloudSaveDataAppliedEvent());
		return true;
	}

	private static int GetCurrentEndlessLevelId(LevelModelProxy level)
	{
		return level?.Data.GetGameModeData(GameMode.Endless).level ?? 1;
	}

	private static string GetCurrentDeviceId()
	{
		string text = AccountHelper.TryGetDeviceId();
		if (!string.IsNullOrEmpty(text))
		{
			return text;
		}
		return "unknown";
	}

	private static List<CloudSavePropCount> CapturePropCounts(PropModelProxy prop)
	{
		List<CloudSavePropCount> list = new List<CloudSavePropCount>((prop?.Data.PropDict?.Count).GetValueOrDefault());
		if (prop?.Data.PropDict == null)
		{
			return list;
		}
		foreach (KeyValuePair<EPropType, int> item in prop.Data.PropDict)
		{
			list.Add(new CloudSavePropCount
			{
				PropId = (int)item.Key,
				Count = Math.Max(0, item.Value)
			});
		}
		list.Sort((CloudSavePropCount left, CloudSavePropCount right) => left.PropId.CompareTo(right.PropId));
		return list;
	}

	private static List<CloudSaveGoodsBuyCount> CaptureGoodsBuyCounts(ShopModelProxy shop)
	{
		List<CloudSaveGoodsBuyCount> list = new List<CloudSaveGoodsBuyCount>((shop?.Data.GoodsBuyCount?.Count).GetValueOrDefault());
		if (shop?.Data.GoodsBuyCount == null)
		{
			return list;
		}
		foreach (KeyValuePair<int, int> item in shop.Data.GoodsBuyCount)
		{
			if (item.Key > 0 && item.Value != 0)
			{
				list.Add(new CloudSaveGoodsBuyCount
				{
					GoodsId = item.Key,
					Count = item.Value
				});
			}
		}
		list.Sort((CloudSaveGoodsBuyCount left, CloudSaveGoodsBuyCount right) => left.GoodsId.CompareTo(right.GoodsId));
		return list;
	}

	private static List<int> CaptureUnlockPropIds(PropModelProxy prop)
	{
		List<int> list = new List<int>((prop?.Data.UnlockPropList?.Count).GetValueOrDefault());
		if (prop?.Data.UnlockPropList == null)
		{
			return list;
		}
		for (int i = 0; i < prop.Data.UnlockPropList.Count; i++)
		{
			int num = prop.Data.UnlockPropList[i];
			if (prop.IsInGameProp(num) && !list.Contains(num))
			{
				list.Add(num);
			}
		}
		list.Sort();
		return list;
	}

	private static List<int> CaptureInGamePropIds(List<int> propIds, PropModelProxy prop)
	{
		List<int> list = new List<int>(propIds?.Count ?? 0);
		if (propIds == null || prop == null)
		{
			return list;
		}
		for (int i = 0; i < propIds.Count; i++)
		{
			int num = propIds[i];
			if (prop.IsInGameProp(num) && !list.Contains(num))
			{
				list.Add(num);
			}
		}
		list.Sort();
		return list;
	}

	private static List<CloudSavePendingFreeProp> CapturePendingFreeProps(PropModelProxy prop)
	{
		List<CloudSavePendingFreeProp> list = new List<CloudSavePendingFreeProp>((prop?.Data.PendingFreePropDict?.Count).GetValueOrDefault());
		if (prop?.Data.PendingFreePropDict == null)
		{
			return list;
		}
		foreach (KeyValuePair<EPropType, int> item in prop.Data.PendingFreePropDict)
		{
			int key = (int)item.Key;
			if (prop.IsInGameProp(key) && item.Value > 0)
			{
				TrackPropUseReason value = TrackPropUseReason.Free;
				if (prop.Data.PendingFreePropReasonDict != null)
				{
					prop.Data.PendingFreePropReasonDict.TryGetValue(item.Key, out value);
				}
				list.Add(new CloudSavePendingFreeProp
				{
					PropId = key,
					Count = item.Value,
					Reason = value
				});
			}
		}
		list.Sort((CloudSavePendingFreeProp left, CloudSavePendingFreeProp right) => left.PropId.CompareTo(right.PropId));
		return list;
	}

	private static List<CloudSavePurchaseCount> CapturePurchasePropCounts(Dictionary<int, int> source)
	{
		List<CloudSavePurchaseCount> list = new List<CloudSavePurchaseCount>(source.Count);
		foreach (KeyValuePair<int, int> item in source)
		{
			if (item.Key >= 0 && item.Value > 0)
			{
				list.Add(new CloudSavePurchaseCount
				{
					PropId = item.Key,
					Count = item.Value
				});
			}
		}
		list.Sort((CloudSavePurchaseCount left, CloudSavePurchaseCount right) => left.PropId.CompareTo(right.PropId));
		return list;
	}

	public static bool TryApplyUploadedPropIncrease(CloudSaveDataSnapshot localData, CloudSaveDataSnapshot resultData)
	{
		Dictionary<int, int> dictionary = new Dictionary<int, int>(localData.PropCounts.Count);
		for (int i = 0; i < localData.PropCounts.Count; i++)
		{
			CloudSavePropCount cloudSavePropCount = localData.PropCounts[i];
			if (cloudSavePropCount != null && cloudSavePropCount.PropId >= 0)
			{
				dictionary[cloudSavePropCount.PropId] = Math.Max(0, cloudSavePropCount.Count);
			}
		}
		PropModelProxy propModelProxy = null;
		for (int j = 0; j < resultData.PropCounts.Count; j++)
		{
			CloudSavePropCount cloudSavePropCount2 = resultData.PropCounts[j];
			if (cloudSavePropCount2 == null || cloudSavePropCount2.PropId < 0)
			{
				continue;
			}
			dictionary.TryGetValue(cloudSavePropCount2.PropId, out var value);
			int num = cloudSavePropCount2.Count - value;
			if (num > 0)
			{
				if (propModelProxy == null)
				{
					propModelProxy = GameApp.Proxy.Get<PropModelProxy>();
				}
				if (propModelProxy == null)
				{
					Log.LogError("[CloudSaveData]", "本地道具存档未就绪，最终快照中补回的道具未写入本机");
					return false;
				}
				EPropType propId = (EPropType)cloudSavePropCount2.PropId;
				PropModelData data = propModelProxy.Data;
				Dictionary<EPropType, int> obj = data.PropDict ?? (data.PropDict = new Dictionary<EPropType, int>());
				obj.TryGetValue(propId, out var value2);
				obj[propId] = Math.Max(0, value2) + num;
				GameApp.Event.Send<PropChangeEvent>(new PropChangeEvent
				{
					PropType = propId,
					DeltaCount = num
				});
			}
		}
		return true;
	}

	internal static List<CloudSavePurchaseCount> SumPurchaseCounts(List<CloudSavePurchaseCount> left, List<CloudSavePurchaseCount> right)
	{
		Dictionary<int, int> dictionary = new Dictionary<int, int>();
		AccumulatePurchaseCounts(dictionary, left);
		AccumulatePurchaseCounts(dictionary, right);
		return CapturePurchasePropCounts(dictionary);
	}

	private static void AccumulatePurchaseCounts(Dictionary<int, int> target, List<CloudSavePurchaseCount> source)
	{
		if (source == null)
		{
			return;
		}
		for (int i = 0; i < source.Count; i++)
		{
			CloudSavePurchaseCount cloudSavePurchaseCount = source[i];
			if (cloudSavePurchaseCount != null && cloudSavePurchaseCount.PropId >= 0 && cloudSavePurchaseCount.Count > 0)
			{
				target.TryGetValue(cloudSavePurchaseCount.PropId, out var value);
				target[cloudSavePurchaseCount.PropId] = value + cloudSavePurchaseCount.Count;
			}
		}
	}

	private static List<CloudSavePropCount> AddPurchasePropCounts(List<CloudSavePropCount> propCounts, Dictionary<int, int> pendingCounts)
	{
		if (pendingCounts.Count == 0)
		{
			return propCounts;
		}
		Dictionary<int, int> dictionary = new Dictionary<int, int>(propCounts.Count + pendingCounts.Count);
		for (int i = 0; i < propCounts.Count; i++)
		{
			CloudSavePropCount cloudSavePropCount = propCounts[i];
			if (cloudSavePropCount != null && cloudSavePropCount.PropId >= 0)
			{
				dictionary[cloudSavePropCount.PropId] = Math.Max(0, cloudSavePropCount.Count);
			}
		}
		foreach (KeyValuePair<int, int> pendingCount in pendingCounts)
		{
			if (pendingCount.Key >= 0 && pendingCount.Value > 0)
			{
				dictionary.TryGetValue(pendingCount.Key, out var value);
				dictionary[pendingCount.Key] = value + pendingCount.Value;
			}
		}
		List<CloudSavePropCount> list = new List<CloudSavePropCount>(dictionary.Count);
		foreach (KeyValuePair<int, int> item in dictionary)
		{
			list.Add(new CloudSavePropCount
			{
				PropId = item.Key,
				Count = item.Value
			});
		}
		list.Sort((CloudSavePropCount left, CloudSavePropCount right) => left.PropId.CompareTo(right.PropId));
		return list;
	}

	private static void ApplyPropCounts(PropModelProxy prop, List<CloudSavePropCount> cloudPropCounts)
	{
		PropModelData data = prop.Data;
		Dictionary<EPropType, int> dictionary = data.PropDict ?? (data.PropDict = new Dictionary<EPropType, int>());
		Dictionary<EPropType, int> dictionary2 = new Dictionary<EPropType, int>(dictionary);
		dictionary.Clear();
		for (int i = 0; i < cloudPropCounts.Count; i++)
		{
			CloudSavePropCount cloudSavePropCount = cloudPropCounts[i];
			if (cloudSavePropCount != null && cloudSavePropCount.PropId >= 0)
			{
				EPropType propId = (EPropType)cloudSavePropCount.PropId;
				int num = Math.Max(0, cloudSavePropCount.Count);
				dictionary2.TryGetValue(propId, out var value);
				dictionary[propId] = num;
				dictionary2.Remove(propId);
				GameApp.Event.Send<PropChangeEvent>(new PropChangeEvent
				{
					PropType = propId,
					DeltaCount = num - value
				});
			}
		}
		foreach (KeyValuePair<EPropType, int> item in dictionary2)
		{
			GameApp.Event.Send<PropChangeEvent>(new PropChangeEvent
			{
				PropType = item.Key,
				DeltaCount = -item.Value
			});
		}
	}

	private static void ApplyGoodsBuyCounts(ShopModelProxy shop, List<CloudSaveGoodsBuyCount> cloudGoodsBuyCounts)
	{
		ShopModelData data = shop.Data;
		Dictionary<int, int> dictionary = data.GoodsBuyCount ?? (data.GoodsBuyCount = new Dictionary<int, int>());
		dictionary.Clear();
		for (int i = 0; i < cloudGoodsBuyCounts.Count; i++)
		{
			CloudSaveGoodsBuyCount cloudSaveGoodsBuyCount = cloudGoodsBuyCounts[i];
			if (cloudSaveGoodsBuyCount != null && cloudSaveGoodsBuyCount.GoodsId > 0 && cloudSaveGoodsBuyCount.Count != 0)
			{
				dictionary[cloudSaveGoodsBuyCount.GoodsId] = cloudSaveGoodsBuyCount.Count;
			}
		}
	}

	private static void ApplyUnlockPropIds(PropModelProxy prop, List<int> cloudUnlockPropIds)
	{
		PropModelData data = prop.Data;
		List<int> list = data.UnlockPropList ?? (data.UnlockPropList = new List<int>());
		list.Clear();
		for (int i = 0; i < cloudUnlockPropIds.Count; i++)
		{
			int num = cloudUnlockPropIds[i];
			if (prop.IsInGameProp(num) && !list.Contains(num))
			{
				list.Add(num);
			}
		}
		list.Sort();
	}

	private static void ApplyPropState(PropModelProxy prop, CloudSaveDataSnapshot snapshot)
	{
		PropModelData data = prop.Data;
		ApplyInGamePropIds(data.UsedPropList ?? (data.UsedPropList = new List<int>()), snapshot.UsedPropIds, prop);
		data = prop.Data;
		ApplyInGamePropIds(data.GuidedPropList ?? (data.GuidedPropList = new List<int>()), snapshot.GuidedPropIds, prop);
		data = prop.Data;
		ApplyInGamePropIds(data.GuideUserInteractedPropList ?? (data.GuideUserInteractedPropList = new List<int>()), snapshot.GuideUserInteractedPropIds, prop);
		ApplyPendingFreeProps(prop, snapshot.PendingFreeProps);
		data = prop.Data;
		ApplyInGamePropIds(data.PendingUnlockIntroPropList ?? (data.PendingUnlockIntroPropList = new List<int>()), snapshot.PendingUnlockIntroPropIds, prop);
		prop.Data.IsNewbieProp = snapshot.IsNewbieProp;
	}

	private static void ApplyInGamePropIds(List<int> target, List<int> source, PropModelProxy prop)
	{
		target.Clear();
		for (int i = 0; i < source.Count; i++)
		{
			int num = source[i];
			if (prop.IsInGameProp(num) && !target.Contains(num))
			{
				target.Add(num);
			}
		}
		target.Sort();
	}

	private static void ApplyPendingFreeProps(PropModelProxy prop, List<CloudSavePendingFreeProp> source)
	{
		PropModelData data = prop.Data;
		Dictionary<EPropType, int> dictionary = data.PendingFreePropDict ?? (data.PendingFreePropDict = new Dictionary<EPropType, int>());
		data = prop.Data;
		Dictionary<EPropType, TrackPropUseReason> dictionary2 = data.PendingFreePropReasonDict ?? (data.PendingFreePropReasonDict = new Dictionary<EPropType, TrackPropUseReason>());
		dictionary.Clear();
		dictionary2.Clear();
		for (int i = 0; i < source.Count; i++)
		{
			CloudSavePendingFreeProp cloudSavePendingFreeProp = source[i];
			if (cloudSavePendingFreeProp != null && prop.IsInGameProp(cloudSavePendingFreeProp.PropId) && cloudSavePendingFreeProp.Count > 0)
			{
				EPropType propId = (EPropType)cloudSavePendingFreeProp.PropId;
				dictionary[propId] = cloudSavePendingFreeProp.Count;
				dictionary2[propId] = cloudSavePendingFreeProp.Reason;
			}
		}
	}

	private static void InitializeMissingPropState(CloudSaveDataSnapshot snapshot)
	{
		CloudSaveDataSnapshot cloudSaveDataSnapshot = snapshot;
		if (cloudSaveDataSnapshot.PropCounts == null)
		{
			cloudSaveDataSnapshot.PropCounts = new List<CloudSavePropCount>();
		}
		cloudSaveDataSnapshot = snapshot;
		if (cloudSaveDataSnapshot.PurchaseCounts == null)
		{
			cloudSaveDataSnapshot.PurchaseCounts = new List<CloudSavePurchaseCount>();
		}
		cloudSaveDataSnapshot = snapshot;
		if (cloudSaveDataSnapshot.SyncedPurchaseCounts == null)
		{
			cloudSaveDataSnapshot.SyncedPurchaseCounts = new List<CloudSavePurchaseCount>();
		}
		cloudSaveDataSnapshot = snapshot;
		if (cloudSaveDataSnapshot.GoodsBuyCounts == null)
		{
			cloudSaveDataSnapshot.GoodsBuyCounts = new List<CloudSaveGoodsBuyCount>();
		}
		cloudSaveDataSnapshot = snapshot;
		if (cloudSaveDataSnapshot.UnlockPropIds == null)
		{
			cloudSaveDataSnapshot.UnlockPropIds = new List<int>();
		}
		cloudSaveDataSnapshot = snapshot;
		if (cloudSaveDataSnapshot.UsedPropIds == null)
		{
			cloudSaveDataSnapshot.UsedPropIds = new List<int>();
		}
		cloudSaveDataSnapshot = snapshot;
		if (cloudSaveDataSnapshot.GuidedPropIds == null)
		{
			cloudSaveDataSnapshot.GuidedPropIds = new List<int>();
		}
		cloudSaveDataSnapshot = snapshot;
		if (cloudSaveDataSnapshot.GuideUserInteractedPropIds == null)
		{
			cloudSaveDataSnapshot.GuideUserInteractedPropIds = new List<int>();
		}
		cloudSaveDataSnapshot = snapshot;
		if (cloudSaveDataSnapshot.PendingFreeProps == null)
		{
			cloudSaveDataSnapshot.PendingFreeProps = new List<CloudSavePendingFreeProp>();
		}
		cloudSaveDataSnapshot = snapshot;
		if (cloudSaveDataSnapshot.PendingUnlockIntroPropIds == null)
		{
			cloudSaveDataSnapshot.PendingUnlockIntroPropIds = new List<int>();
		}
		if (snapshot.EndlessLevelId <= 0)
		{
			snapshot.EndlessLevelId = 1;
		}
	}

	private static bool AreIntListsEqual(List<int> left, List<int> right)
	{
		if (left == right)
		{
			return true;
		}
		if (left == null || right == null || left.Count != right.Count)
		{
			return false;
		}
		for (int i = 0; i < left.Count; i++)
		{
			if (left[i] != right[i])
			{
				return false;
			}
		}
		return true;
	}

	private static bool ArePurchaseCountsEqual(List<CloudSavePurchaseCount> left, List<CloudSavePurchaseCount> right)
	{
		if (left == right)
		{
			return true;
		}
		if (left == null || right == null || left.Count != right.Count)
		{
			return false;
		}
		for (int i = 0; i < left.Count; i++)
		{
			if (left[i] == null || right[i] == null || left[i].PropId != right[i].PropId || left[i].Count != right[i].Count)
			{
				return false;
			}
		}
		return true;
	}

	private static bool ArePendingFreePropsEqual(List<CloudSavePendingFreeProp> left, List<CloudSavePendingFreeProp> right)
	{
		if (left == right)
		{
			return true;
		}
		if (left == null || right == null || left.Count != right.Count)
		{
			return false;
		}
		for (int i = 0; i < left.Count; i++)
		{
			CloudSavePendingFreeProp cloudSavePendingFreeProp = left[i];
			CloudSavePendingFreeProp cloudSavePendingFreeProp2 = right[i];
			if (cloudSavePendingFreeProp == null || cloudSavePendingFreeProp2 == null || cloudSavePendingFreeProp.PropId != cloudSavePendingFreeProp2.PropId || cloudSavePendingFreeProp.Count != cloudSavePendingFreeProp2.Count || cloudSavePendingFreeProp.Reason != cloudSavePendingFreeProp2.Reason)
			{
				return false;
			}
		}
		return true;
	}

	private static bool IsSnapshotValid(CloudSaveDataSnapshot snapshot)
	{
		if (snapshot != null && !string.IsNullOrEmpty(snapshot.DeviceId) && !string.IsNullOrEmpty(snapshot.PlayerName) && snapshot.HeadId > 0 && snapshot.MainLevelId > 0 && snapshot.EndlessLevelId > 0 && snapshot.PropCounts != null && snapshot.PurchaseCounts != null && snapshot.GoodsBuyCounts != null)
		{
			return snapshot.UnlockPropIds != null;
		}
		return false;
	}
}
