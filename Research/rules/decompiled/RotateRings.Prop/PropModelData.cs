using System.Collections.Generic;
using MemoryPack;
using MemoryPack.Formatters;
using MemoryPack.Internal;
using XingYun.Save;

namespace RotateRings.Prop;

[MemoryPackable(/*Could not decode attribute arguments.*/)]
public class PropModelData : ISaveDefaultInitializer, IMemoryPackable<PropModelData>, IMemoryPackFormatterRegister
{
	[Preserve]
	private sealed class PropModelDataFormatter : MemoryPackFormatter<PropModelData>
	{
		[Preserve]
		public override void Serialize(ref MemoryPackWriter writer, ref PropModelData value)
		{
			PropModelData.Serialize(ref writer, ref value);
		}

		[Preserve]
		public override void Deserialize(ref MemoryPackReader reader, ref PropModelData value)
		{
			PropModelData.Deserialize(ref reader, ref value);
		}
	}

	public Dictionary<EPropType, int> PropDict;

	public List<int> UnlockPropList;

	public List<int> UsedPropList;

	public List<int> GuidedPropList;

	public List<int> GuideUserInteractedPropList;

	public Dictionary<EPropType, int> PendingFreePropDict;

	public Dictionary<EPropType, TrackPropUseReason> PendingFreePropReasonDict;

	public bool IsNewbieProp;

	public List<string> GrantedRewardTransactionIds;

	public bool IsPropCompatMigrated;

	public List<int> PendingUnlockIntroPropList;

	public void OnCreateDefault()
	{
		if (PropDict == null)
		{
			PropDict = new Dictionary<EPropType, int>();
		}
		if (UnlockPropList == null)
		{
			UnlockPropList = new List<int>();
		}
		if (UsedPropList == null)
		{
			UsedPropList = new List<int>();
		}
		if (GuidedPropList == null)
		{
			GuidedPropList = new List<int>();
		}
		if (GuideUserInteractedPropList == null)
		{
			GuideUserInteractedPropList = new List<int>();
		}
		if (PendingFreePropDict == null)
		{
			PendingFreePropDict = new Dictionary<EPropType, int>();
		}
		if (PendingFreePropReasonDict == null)
		{
			PendingFreePropReasonDict = new Dictionary<EPropType, TrackPropUseReason>();
		}
		if (GrantedRewardTransactionIds == null)
		{
			GrantedRewardTransactionIds = new List<string>();
		}
		if (PendingUnlockIntroPropList == null)
		{
			PendingUnlockIntroPropList = new List<int>();
		}
		IsPropCompatMigrated = false;
		IsNewbieProp = false;
	}

	static PropModelData()
	{
		RegisterFormatter();
	}

	[Preserve]
	public static void RegisterFormatter()
	{
		if (!MemoryPackFormatterProvider.IsRegistered<PropModelData>())
		{
			MemoryPackFormatterProvider.Register<PropModelData>((MemoryPackFormatter<PropModelData>)(object)new PropModelDataFormatter());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<PropModelData[]>())
		{
			MemoryPackFormatterProvider.Register<PropModelData[]>((MemoryPackFormatter<PropModelData[]>)(object)new ArrayFormatter<PropModelData>());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<Dictionary<EPropType, int>>())
		{
			MemoryPackFormatterProvider.Register<Dictionary<EPropType, int>>((MemoryPackFormatter<Dictionary<EPropType, int>>)(object)new DictionaryFormatter<EPropType, int>());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<EPropType>())
		{
			MemoryPackFormatterProvider.Register<EPropType>((MemoryPackFormatter<EPropType>)(object)new UnmanagedFormatter<EPropType>());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<List<int>>())
		{
			MemoryPackFormatterProvider.Register<List<int>>((MemoryPackFormatter<List<int>>)(object)new ListFormatter<int>());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<Dictionary<EPropType, TrackPropUseReason>>())
		{
			MemoryPackFormatterProvider.Register<Dictionary<EPropType, TrackPropUseReason>>((MemoryPackFormatter<Dictionary<EPropType, TrackPropUseReason>>)(object)new DictionaryFormatter<EPropType, TrackPropUseReason>());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<TrackPropUseReason>())
		{
			MemoryPackFormatterProvider.Register<TrackPropUseReason>((MemoryPackFormatter<TrackPropUseReason>)(object)new UnmanagedFormatter<TrackPropUseReason>());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<List<string>>())
		{
			MemoryPackFormatterProvider.Register<List<string>>((MemoryPackFormatter<List<string>>)(object)new ListFormatter<string>());
		}
	}

	[Preserve]
	public static void Serialize(ref MemoryPackWriter writer, ref PropModelData? value)
	{
		if (value == null)
		{
			((MemoryPackWriter)(ref writer)).WriteNullObjectHeader();
			return;
		}
		((MemoryPackWriter)(ref writer)).WriteObjectHeader((byte)11);
		((MemoryPackWriter)(ref writer)).WriteValue<Dictionary<EPropType, int>>(ref value.PropDict);
		((MemoryPackWriter)(ref writer)).WriteValue<List<int>>(ref value.UnlockPropList);
		((MemoryPackWriter)(ref writer)).WriteValue<List<int>>(ref value.UsedPropList);
		((MemoryPackWriter)(ref writer)).WriteValue<List<int>>(ref value.GuidedPropList);
		((MemoryPackWriter)(ref writer)).WriteValue<List<int>>(ref value.GuideUserInteractedPropList);
		((MemoryPackWriter)(ref writer)).WriteValue<Dictionary<EPropType, int>>(ref value.PendingFreePropDict);
		((MemoryPackWriter)(ref writer)).WriteValue<Dictionary<EPropType, TrackPropUseReason>>(ref value.PendingFreePropReasonDict);
		((MemoryPackWriter)(ref writer)).WriteUnmanaged<bool>(ref value.IsNewbieProp);
		((MemoryPackWriter)(ref writer)).WriteValue<List<string>>(ref value.GrantedRewardTransactionIds);
		((MemoryPackWriter)(ref writer)).WriteUnmanaged<bool>(ref value.IsPropCompatMigrated);
		((MemoryPackWriter)(ref writer)).WriteValue<List<int>>(ref value.PendingUnlockIntroPropList);
	}

	[Preserve]
	public static void Deserialize(ref MemoryPackReader reader, ref PropModelData? value)
	{
		byte b = default(byte);
		if (!((MemoryPackReader)(ref reader)).TryReadObjectHeader(ref b))
		{
			value = null;
			return;
		}
		Dictionary<EPropType, int> propDict;
		List<int> unlockPropList;
		List<int> usedPropList;
		List<int> guidedPropList;
		List<int> guideUserInteractedPropList;
		Dictionary<EPropType, int> pendingFreePropDict;
		Dictionary<EPropType, TrackPropUseReason> pendingFreePropReasonDict;
		bool isNewbieProp = default(bool);
		List<string> grantedRewardTransactionIds;
		bool isPropCompatMigrated = default(bool);
		List<int> pendingUnlockIntroPropList;
		if (b == 11)
		{
			if (value != null)
			{
				propDict = value.PropDict;
				unlockPropList = value.UnlockPropList;
				usedPropList = value.UsedPropList;
				guidedPropList = value.GuidedPropList;
				guideUserInteractedPropList = value.GuideUserInteractedPropList;
				pendingFreePropDict = value.PendingFreePropDict;
				pendingFreePropReasonDict = value.PendingFreePropReasonDict;
				isNewbieProp = value.IsNewbieProp;
				grantedRewardTransactionIds = value.GrantedRewardTransactionIds;
				isPropCompatMigrated = value.IsPropCompatMigrated;
				pendingUnlockIntroPropList = value.PendingUnlockIntroPropList;
				((MemoryPackReader)(ref reader)).ReadValue<Dictionary<EPropType, int>>(ref propDict);
				((MemoryPackReader)(ref reader)).ReadValue<List<int>>(ref unlockPropList);
				((MemoryPackReader)(ref reader)).ReadValue<List<int>>(ref usedPropList);
				((MemoryPackReader)(ref reader)).ReadValue<List<int>>(ref guidedPropList);
				((MemoryPackReader)(ref reader)).ReadValue<List<int>>(ref guideUserInteractedPropList);
				((MemoryPackReader)(ref reader)).ReadValue<Dictionary<EPropType, int>>(ref pendingFreePropDict);
				((MemoryPackReader)(ref reader)).ReadValue<Dictionary<EPropType, TrackPropUseReason>>(ref pendingFreePropReasonDict);
				((MemoryPackReader)(ref reader)).ReadUnmanaged<bool>(ref isNewbieProp);
				((MemoryPackReader)(ref reader)).ReadValue<List<string>>(ref grantedRewardTransactionIds);
				((MemoryPackReader)(ref reader)).ReadUnmanaged<bool>(ref isPropCompatMigrated);
				((MemoryPackReader)(ref reader)).ReadValue<List<int>>(ref pendingUnlockIntroPropList);
				goto IL_025e;
			}
			propDict = ((MemoryPackReader)(ref reader)).ReadValue<Dictionary<EPropType, int>>();
			unlockPropList = ((MemoryPackReader)(ref reader)).ReadValue<List<int>>();
			usedPropList = ((MemoryPackReader)(ref reader)).ReadValue<List<int>>();
			guidedPropList = ((MemoryPackReader)(ref reader)).ReadValue<List<int>>();
			guideUserInteractedPropList = ((MemoryPackReader)(ref reader)).ReadValue<List<int>>();
			pendingFreePropDict = ((MemoryPackReader)(ref reader)).ReadValue<Dictionary<EPropType, int>>();
			pendingFreePropReasonDict = ((MemoryPackReader)(ref reader)).ReadValue<Dictionary<EPropType, TrackPropUseReason>>();
			((MemoryPackReader)(ref reader)).ReadUnmanaged<bool>(ref isNewbieProp);
			grantedRewardTransactionIds = ((MemoryPackReader)(ref reader)).ReadValue<List<string>>();
			((MemoryPackReader)(ref reader)).ReadUnmanaged<bool>(ref isPropCompatMigrated);
			pendingUnlockIntroPropList = ((MemoryPackReader)(ref reader)).ReadValue<List<int>>();
		}
		else
		{
			if (b > 11)
			{
				MemoryPackSerializationException.ThrowInvalidPropertyCount(typeof(PropModelData), (byte)11, b);
				return;
			}
			if (value == null)
			{
				propDict = null;
				unlockPropList = null;
				usedPropList = null;
				guidedPropList = null;
				guideUserInteractedPropList = null;
				pendingFreePropDict = null;
				pendingFreePropReasonDict = null;
				isNewbieProp = false;
				grantedRewardTransactionIds = null;
				isPropCompatMigrated = false;
				pendingUnlockIntroPropList = null;
			}
			else
			{
				propDict = value.PropDict;
				unlockPropList = value.UnlockPropList;
				usedPropList = value.UsedPropList;
				guidedPropList = value.GuidedPropList;
				guideUserInteractedPropList = value.GuideUserInteractedPropList;
				pendingFreePropDict = value.PendingFreePropDict;
				pendingFreePropReasonDict = value.PendingFreePropReasonDict;
				isNewbieProp = value.IsNewbieProp;
				grantedRewardTransactionIds = value.GrantedRewardTransactionIds;
				isPropCompatMigrated = value.IsPropCompatMigrated;
				pendingUnlockIntroPropList = value.PendingUnlockIntroPropList;
			}
			if (b != 0)
			{
				((MemoryPackReader)(ref reader)).ReadValue<Dictionary<EPropType, int>>(ref propDict);
				if (b != 1)
				{
					((MemoryPackReader)(ref reader)).ReadValue<List<int>>(ref unlockPropList);
					if (b != 2)
					{
						((MemoryPackReader)(ref reader)).ReadValue<List<int>>(ref usedPropList);
						if (b != 3)
						{
							((MemoryPackReader)(ref reader)).ReadValue<List<int>>(ref guidedPropList);
							if (b != 4)
							{
								((MemoryPackReader)(ref reader)).ReadValue<List<int>>(ref guideUserInteractedPropList);
								if (b != 5)
								{
									((MemoryPackReader)(ref reader)).ReadValue<Dictionary<EPropType, int>>(ref pendingFreePropDict);
									if (b != 6)
									{
										((MemoryPackReader)(ref reader)).ReadValue<Dictionary<EPropType, TrackPropUseReason>>(ref pendingFreePropReasonDict);
										if (b != 7)
										{
											((MemoryPackReader)(ref reader)).ReadUnmanaged<bool>(ref isNewbieProp);
											if (b != 8)
											{
												((MemoryPackReader)(ref reader)).ReadValue<List<string>>(ref grantedRewardTransactionIds);
												if (b != 9)
												{
													((MemoryPackReader)(ref reader)).ReadUnmanaged<bool>(ref isPropCompatMigrated);
													if (b != 10)
													{
														((MemoryPackReader)(ref reader)).ReadValue<List<int>>(ref pendingUnlockIntroPropList);
														_ = 11;
													}
												}
											}
										}
									}
								}
							}
						}
					}
				}
			}
			if (value != null)
			{
				goto IL_025e;
			}
		}
		value = new PropModelData
		{
			PropDict = propDict,
			UnlockPropList = unlockPropList,
			UsedPropList = usedPropList,
			GuidedPropList = guidedPropList,
			GuideUserInteractedPropList = guideUserInteractedPropList,
			PendingFreePropDict = pendingFreePropDict,
			PendingFreePropReasonDict = pendingFreePropReasonDict,
			IsNewbieProp = isNewbieProp,
			GrantedRewardTransactionIds = grantedRewardTransactionIds,
			IsPropCompatMigrated = isPropCompatMigrated,
			PendingUnlockIntroPropList = pendingUnlockIntroPropList
		};
		return;
		IL_025e:
		value.PropDict = propDict;
		value.UnlockPropList = unlockPropList;
		value.UsedPropList = usedPropList;
		value.GuidedPropList = guidedPropList;
		value.GuideUserInteractedPropList = guideUserInteractedPropList;
		value.PendingFreePropDict = pendingFreePropDict;
		value.PendingFreePropReasonDict = pendingFreePropReasonDict;
		value.IsNewbieProp = isNewbieProp;
		value.GrantedRewardTransactionIds = grantedRewardTransactionIds;
		value.IsPropCompatMigrated = isPropCompatMigrated;
		value.PendingUnlockIntroPropList = pendingUnlockIntroPropList;
	}
}
