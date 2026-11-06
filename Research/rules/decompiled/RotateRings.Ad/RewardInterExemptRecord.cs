using MemoryPack;
using MemoryPack.Formatters;
using MemoryPack.Internal;
using XingYun.Save;

namespace RotateRings.Ad;

[MemoryPackable(/*Could not decode attribute arguments.*/)]
public class RewardInterExemptRecord : ISaveDefaultInitializer, IMemoryPackable<RewardInterExemptRecord>, IMemoryPackFormatterRegister
{
	[Preserve]
	private sealed class RewardInterExemptRecordFormatter : MemoryPackFormatter<RewardInterExemptRecord>
	{
		[Preserve]
		public override void Serialize(ref MemoryPackWriter writer, ref RewardInterExemptRecord value)
		{
			RewardInterExemptRecord.Serialize(ref writer, ref value);
		}

		[Preserve]
		public override void Deserialize(ref MemoryPackReader reader, ref RewardInterExemptRecord value)
		{
			RewardInterExemptRecord.Deserialize(ref reader, ref value);
		}
	}

	public int PendingMidRewardGroup;

	public int SkipNextFrontAdGroup;

	public long ExemptionExpireTimestamp;

	public void OnCreateDefault()
	{
		PendingMidRewardGroup = 0;
		SkipNextFrontAdGroup = 0;
		ExemptionExpireTimestamp = 0L;
	}

	static RewardInterExemptRecord()
	{
		RegisterFormatter();
	}

	[Preserve]
	public static void RegisterFormatter()
	{
		if (!MemoryPackFormatterProvider.IsRegistered<RewardInterExemptRecord>())
		{
			MemoryPackFormatterProvider.Register<RewardInterExemptRecord>((MemoryPackFormatter<RewardInterExemptRecord>)(object)new RewardInterExemptRecordFormatter());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<RewardInterExemptRecord[]>())
		{
			MemoryPackFormatterProvider.Register<RewardInterExemptRecord[]>((MemoryPackFormatter<RewardInterExemptRecord[]>)(object)new ArrayFormatter<RewardInterExemptRecord>());
		}
	}

	[Preserve]
	public static void Serialize(ref MemoryPackWriter writer, ref RewardInterExemptRecord? value)
	{
		if (value == null)
		{
			((MemoryPackWriter)(ref writer)).WriteNullObjectHeader();
		}
		else
		{
			((MemoryPackWriter)(ref writer)).WriteUnmanagedWithObjectHeader<int, int, long>((byte)3, ref value.PendingMidRewardGroup, ref value.SkipNextFrontAdGroup, ref value.ExemptionExpireTimestamp);
		}
	}

	[Preserve]
	public static void Deserialize(ref MemoryPackReader reader, ref RewardInterExemptRecord? value)
	{
		byte b = default(byte);
		if (!((MemoryPackReader)(ref reader)).TryReadObjectHeader(ref b))
		{
			value = null;
			return;
		}
		int pendingMidRewardGroup = default(int);
		int skipNextFrontAdGroup = default(int);
		long exemptionExpireTimestamp = default(long);
		if (b == 3)
		{
			if (value != null)
			{
				pendingMidRewardGroup = value.PendingMidRewardGroup;
				skipNextFrontAdGroup = value.SkipNextFrontAdGroup;
				exemptionExpireTimestamp = value.ExemptionExpireTimestamp;
				((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref pendingMidRewardGroup);
				((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref skipNextFrontAdGroup);
				((MemoryPackReader)(ref reader)).ReadUnmanaged<long>(ref exemptionExpireTimestamp);
				goto IL_00bf;
			}
			((MemoryPackReader)(ref reader)).ReadUnmanaged<int, int, long>(ref pendingMidRewardGroup, ref skipNextFrontAdGroup, ref exemptionExpireTimestamp);
		}
		else
		{
			if (b > 3)
			{
				MemoryPackSerializationException.ThrowInvalidPropertyCount(typeof(RewardInterExemptRecord), (byte)3, b);
				return;
			}
			if (value == null)
			{
				pendingMidRewardGroup = 0;
				skipNextFrontAdGroup = 0;
				exemptionExpireTimestamp = 0L;
			}
			else
			{
				pendingMidRewardGroup = value.PendingMidRewardGroup;
				skipNextFrontAdGroup = value.SkipNextFrontAdGroup;
				exemptionExpireTimestamp = value.ExemptionExpireTimestamp;
			}
			if (b != 0)
			{
				((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref pendingMidRewardGroup);
				if (b != 1)
				{
					((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref skipNextFrontAdGroup);
					if (b != 2)
					{
						((MemoryPackReader)(ref reader)).ReadUnmanaged<long>(ref exemptionExpireTimestamp);
						_ = 3;
					}
				}
			}
			if (value != null)
			{
				goto IL_00bf;
			}
		}
		value = new RewardInterExemptRecord
		{
			PendingMidRewardGroup = pendingMidRewardGroup,
			SkipNextFrontAdGroup = skipNextFrontAdGroup,
			ExemptionExpireTimestamp = exemptionExpireTimestamp
		};
		return;
		IL_00bf:
		value.PendingMidRewardGroup = pendingMidRewardGroup;
		value.SkipNextFrontAdGroup = skipNextFrontAdGroup;
		value.ExemptionExpireTimestamp = exemptionExpireTimestamp;
	}
}
