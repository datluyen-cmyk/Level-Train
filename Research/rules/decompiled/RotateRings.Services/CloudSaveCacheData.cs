using System.Collections.Generic;
using MemoryPack;
using MemoryPack.Formatters;
using MemoryPack.Internal;
using XingYun.Save;

namespace RotateRings.Services;

[MemoryPackable(/*Could not decode attribute arguments.*/)]
public class CloudSaveCacheData : ISaveDefaultInitializer, IMemoryPackable<CloudSaveCacheData>, IMemoryPackFormatterRegister
{
	[Preserve]
	private sealed class CloudSaveCacheDataFormatter : MemoryPackFormatter<CloudSaveCacheData>
	{
		[Preserve]
		public override void Serialize(ref MemoryPackWriter writer, ref CloudSaveCacheData value)
		{
			CloudSaveCacheData.Serialize(ref writer, ref value);
		}

		[Preserve]
		public override void Deserialize(ref MemoryPackReader reader, ref CloudSaveCacheData value)
		{
			CloudSaveCacheData.Deserialize(ref reader, ref value);
		}
	}

	public Dictionary<int, int> PendingCloudPurchasePropCounts;

	public long CloudSaveDataVersion;

	public bool HasEverPurchased;

	public bool HasBoundThirdPartyOnce;

	public bool PendingForceCloudUpload;

	public bool PendingBindSelection;

	public Dictionary<int, int> SyncedCloudPurchasePropCounts;

	public void OnCreateDefault()
	{
		PendingCloudPurchasePropCounts = new Dictionary<int, int>();
		SyncedCloudPurchasePropCounts = new Dictionary<int, int>();
		CloudSaveDataVersion = 0L;
		HasEverPurchased = false;
		HasBoundThirdPartyOnce = false;
		PendingForceCloudUpload = false;
		PendingBindSelection = false;
	}

	static CloudSaveCacheData()
	{
		RegisterFormatter();
	}

	[Preserve]
	public static void RegisterFormatter()
	{
		if (!MemoryPackFormatterProvider.IsRegistered<CloudSaveCacheData>())
		{
			MemoryPackFormatterProvider.Register<CloudSaveCacheData>((MemoryPackFormatter<CloudSaveCacheData>)(object)new CloudSaveCacheDataFormatter());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<CloudSaveCacheData[]>())
		{
			MemoryPackFormatterProvider.Register<CloudSaveCacheData[]>((MemoryPackFormatter<CloudSaveCacheData[]>)(object)new ArrayFormatter<CloudSaveCacheData>());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<Dictionary<int, int>>())
		{
			MemoryPackFormatterProvider.Register<Dictionary<int, int>>((MemoryPackFormatter<Dictionary<int, int>>)(object)new DictionaryFormatter<int, int>());
		}
	}

	[Preserve]
	public static void Serialize(ref MemoryPackWriter writer, ref CloudSaveCacheData? value)
	{
		if (value == null)
		{
			((MemoryPackWriter)(ref writer)).WriteNullObjectHeader();
			return;
		}
		((MemoryPackWriter)(ref writer)).WriteObjectHeader((byte)7);
		((MemoryPackWriter)(ref writer)).WriteValue<Dictionary<int, int>>(ref value.PendingCloudPurchasePropCounts);
		((MemoryPackWriter)(ref writer)).WriteUnmanaged<long, bool, bool, bool, bool>(ref value.CloudSaveDataVersion, ref value.HasEverPurchased, ref value.HasBoundThirdPartyOnce, ref value.PendingForceCloudUpload, ref value.PendingBindSelection);
		((MemoryPackWriter)(ref writer)).WriteValue<Dictionary<int, int>>(ref value.SyncedCloudPurchasePropCounts);
	}

	[Preserve]
	public static void Deserialize(ref MemoryPackReader reader, ref CloudSaveCacheData? value)
	{
		byte b = default(byte);
		if (!((MemoryPackReader)(ref reader)).TryReadObjectHeader(ref b))
		{
			value = null;
			return;
		}
		Dictionary<int, int> pendingCloudPurchasePropCounts;
		long cloudSaveDataVersion = default(long);
		bool hasEverPurchased = default(bool);
		bool hasBoundThirdPartyOnce = default(bool);
		bool pendingForceCloudUpload = default(bool);
		bool pendingBindSelection = default(bool);
		Dictionary<int, int> syncedCloudPurchasePropCounts;
		if (b == 7)
		{
			if (value != null)
			{
				pendingCloudPurchasePropCounts = value.PendingCloudPurchasePropCounts;
				cloudSaveDataVersion = value.CloudSaveDataVersion;
				hasEverPurchased = value.HasEverPurchased;
				hasBoundThirdPartyOnce = value.HasBoundThirdPartyOnce;
				pendingForceCloudUpload = value.PendingForceCloudUpload;
				pendingBindSelection = value.PendingBindSelection;
				syncedCloudPurchasePropCounts = value.SyncedCloudPurchasePropCounts;
				((MemoryPackReader)(ref reader)).ReadValue<Dictionary<int, int>>(ref pendingCloudPurchasePropCounts);
				((MemoryPackReader)(ref reader)).ReadUnmanaged<long>(ref cloudSaveDataVersion);
				((MemoryPackReader)(ref reader)).ReadUnmanaged<bool>(ref hasEverPurchased);
				((MemoryPackReader)(ref reader)).ReadUnmanaged<bool>(ref hasBoundThirdPartyOnce);
				((MemoryPackReader)(ref reader)).ReadUnmanaged<bool>(ref pendingForceCloudUpload);
				((MemoryPackReader)(ref reader)).ReadUnmanaged<bool>(ref pendingBindSelection);
				((MemoryPackReader)(ref reader)).ReadValue<Dictionary<int, int>>(ref syncedCloudPurchasePropCounts);
				goto IL_017c;
			}
			pendingCloudPurchasePropCounts = ((MemoryPackReader)(ref reader)).ReadValue<Dictionary<int, int>>();
			((MemoryPackReader)(ref reader)).ReadUnmanaged<long, bool, bool, bool, bool>(ref cloudSaveDataVersion, ref hasEverPurchased, ref hasBoundThirdPartyOnce, ref pendingForceCloudUpload, ref pendingBindSelection);
			syncedCloudPurchasePropCounts = ((MemoryPackReader)(ref reader)).ReadValue<Dictionary<int, int>>();
		}
		else
		{
			if (b > 7)
			{
				MemoryPackSerializationException.ThrowInvalidPropertyCount(typeof(CloudSaveCacheData), (byte)7, b);
				return;
			}
			if (value == null)
			{
				pendingCloudPurchasePropCounts = null;
				cloudSaveDataVersion = 0L;
				hasEverPurchased = false;
				hasBoundThirdPartyOnce = false;
				pendingForceCloudUpload = false;
				pendingBindSelection = false;
				syncedCloudPurchasePropCounts = null;
			}
			else
			{
				pendingCloudPurchasePropCounts = value.PendingCloudPurchasePropCounts;
				cloudSaveDataVersion = value.CloudSaveDataVersion;
				hasEverPurchased = value.HasEverPurchased;
				hasBoundThirdPartyOnce = value.HasBoundThirdPartyOnce;
				pendingForceCloudUpload = value.PendingForceCloudUpload;
				pendingBindSelection = value.PendingBindSelection;
				syncedCloudPurchasePropCounts = value.SyncedCloudPurchasePropCounts;
			}
			if (b != 0)
			{
				((MemoryPackReader)(ref reader)).ReadValue<Dictionary<int, int>>(ref pendingCloudPurchasePropCounts);
				if (b != 1)
				{
					((MemoryPackReader)(ref reader)).ReadUnmanaged<long>(ref cloudSaveDataVersion);
					if (b != 2)
					{
						((MemoryPackReader)(ref reader)).ReadUnmanaged<bool>(ref hasEverPurchased);
						if (b != 3)
						{
							((MemoryPackReader)(ref reader)).ReadUnmanaged<bool>(ref hasBoundThirdPartyOnce);
							if (b != 4)
							{
								((MemoryPackReader)(ref reader)).ReadUnmanaged<bool>(ref pendingForceCloudUpload);
								if (b != 5)
								{
									((MemoryPackReader)(ref reader)).ReadUnmanaged<bool>(ref pendingBindSelection);
									if (b != 6)
									{
										((MemoryPackReader)(ref reader)).ReadValue<Dictionary<int, int>>(ref syncedCloudPurchasePropCounts);
										_ = 7;
									}
								}
							}
						}
					}
				}
			}
			if (value != null)
			{
				goto IL_017c;
			}
		}
		value = new CloudSaveCacheData
		{
			PendingCloudPurchasePropCounts = pendingCloudPurchasePropCounts,
			CloudSaveDataVersion = cloudSaveDataVersion,
			HasEverPurchased = hasEverPurchased,
			HasBoundThirdPartyOnce = hasBoundThirdPartyOnce,
			PendingForceCloudUpload = pendingForceCloudUpload,
			PendingBindSelection = pendingBindSelection,
			SyncedCloudPurchasePropCounts = syncedCloudPurchasePropCounts
		};
		return;
		IL_017c:
		value.PendingCloudPurchasePropCounts = pendingCloudPurchasePropCounts;
		value.CloudSaveDataVersion = cloudSaveDataVersion;
		value.HasEverPurchased = hasEverPurchased;
		value.HasBoundThirdPartyOnce = hasBoundThirdPartyOnce;
		value.PendingForceCloudUpload = pendingForceCloudUpload;
		value.PendingBindSelection = pendingBindSelection;
		value.SyncedCloudPurchasePropCounts = syncedCloudPurchasePropCounts;
	}
}
