using MemoryPack;
using MemoryPack.Formatters;
using MemoryPack.Internal;
using XingYun.Save;

namespace RotateRings.Ad;

[MemoryPackable(/*Could not decode attribute arguments.*/)]
public class ARRRecord : ISaveDefaultInitializer, IMemoryPackable<ARRRecord>, IMemoryPackFormatterRegister
{
	[Preserve]
	private sealed class ARRRecordFormatter : MemoryPackFormatter<ARRRecord>
	{
		[Preserve]
		public override void Serialize(ref MemoryPackWriter writer, ref ARRRecord value)
		{
			ARRRecord.Serialize(ref writer, ref value);
		}

		[Preserve]
		public override void Deserialize(ref MemoryPackReader reader, ref ARRRecord value)
		{
			ARRRecord.Deserialize(ref reader, ref value);
		}
	}

	public int RewardWatchedLevelKey;

	public long RewardExemptExpireTimestamp;

	public void OnCreateDefault()
	{
		RewardWatchedLevelKey = 0;
		RewardExemptExpireTimestamp = 0L;
	}

	static ARRRecord()
	{
		RegisterFormatter();
	}

	[Preserve]
	public static void RegisterFormatter()
	{
		if (!MemoryPackFormatterProvider.IsRegistered<ARRRecord>())
		{
			MemoryPackFormatterProvider.Register<ARRRecord>((MemoryPackFormatter<ARRRecord>)(object)new ARRRecordFormatter());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<ARRRecord[]>())
		{
			MemoryPackFormatterProvider.Register<ARRRecord[]>((MemoryPackFormatter<ARRRecord[]>)(object)new ArrayFormatter<ARRRecord>());
		}
	}

	[Preserve]
	public static void Serialize(ref MemoryPackWriter writer, ref ARRRecord? value)
	{
		if (value == null)
		{
			((MemoryPackWriter)(ref writer)).WriteNullObjectHeader();
		}
		else
		{
			((MemoryPackWriter)(ref writer)).WriteUnmanagedWithObjectHeader<int, long>((byte)2, ref value.RewardWatchedLevelKey, ref value.RewardExemptExpireTimestamp);
		}
	}

	[Preserve]
	public static void Deserialize(ref MemoryPackReader reader, ref ARRRecord? value)
	{
		byte b = default(byte);
		if (!((MemoryPackReader)(ref reader)).TryReadObjectHeader(ref b))
		{
			value = null;
			return;
		}
		int rewardWatchedLevelKey = default(int);
		long rewardExemptExpireTimestamp = default(long);
		if (b == 2)
		{
			if (value != null)
			{
				rewardWatchedLevelKey = value.RewardWatchedLevelKey;
				rewardExemptExpireTimestamp = value.RewardExemptExpireTimestamp;
				((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref rewardWatchedLevelKey);
				((MemoryPackReader)(ref reader)).ReadUnmanaged<long>(ref rewardExemptExpireTimestamp);
				goto IL_0097;
			}
			((MemoryPackReader)(ref reader)).ReadUnmanaged<int, long>(ref rewardWatchedLevelKey, ref rewardExemptExpireTimestamp);
		}
		else
		{
			if (b > 2)
			{
				MemoryPackSerializationException.ThrowInvalidPropertyCount(typeof(ARRRecord), (byte)2, b);
				return;
			}
			if (value == null)
			{
				rewardWatchedLevelKey = 0;
				rewardExemptExpireTimestamp = 0L;
			}
			else
			{
				rewardWatchedLevelKey = value.RewardWatchedLevelKey;
				rewardExemptExpireTimestamp = value.RewardExemptExpireTimestamp;
			}
			if (b != 0)
			{
				((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref rewardWatchedLevelKey);
				if (b != 1)
				{
					((MemoryPackReader)(ref reader)).ReadUnmanaged<long>(ref rewardExemptExpireTimestamp);
					_ = 2;
				}
			}
			if (value != null)
			{
				goto IL_0097;
			}
		}
		value = new ARRRecord
		{
			RewardWatchedLevelKey = rewardWatchedLevelKey,
			RewardExemptExpireTimestamp = rewardExemptExpireTimestamp
		};
		return;
		IL_0097:
		value.RewardWatchedLevelKey = rewardWatchedLevelKey;
		value.RewardExemptExpireTimestamp = rewardExemptExpireTimestamp;
	}
}
