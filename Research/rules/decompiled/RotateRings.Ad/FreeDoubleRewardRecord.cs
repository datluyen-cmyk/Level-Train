using MemoryPack;
using MemoryPack.Formatters;
using MemoryPack.Internal;
using XingYun.Save;

namespace RotateRings.Ad;

[MemoryPackable(/*Could not decode attribute arguments.*/)]
public class FreeDoubleRewardRecord : ISaveDefaultInitializer, IMemoryPackable<FreeDoubleRewardRecord>, IMemoryPackFormatterRegister
{
	[Preserve]
	private sealed class FreeDoubleRewardRecordFormatter : MemoryPackFormatter<FreeDoubleRewardRecord>
	{
		[Preserve]
		public override void Serialize(ref MemoryPackWriter writer, ref FreeDoubleRewardRecord value)
		{
			FreeDoubleRewardRecord.Serialize(ref writer, ref value);
		}

		[Preserve]
		public override void Deserialize(ref MemoryPackReader reader, ref FreeDoubleRewardRecord value)
		{
			FreeDoubleRewardRecord.Deserialize(ref reader, ref value);
		}
	}

	public int UsedFreeCount;

	public void OnCreateDefault()
	{
		UsedFreeCount = 0;
	}

	static FreeDoubleRewardRecord()
	{
		RegisterFormatter();
	}

	[Preserve]
	public static void RegisterFormatter()
	{
		if (!MemoryPackFormatterProvider.IsRegistered<FreeDoubleRewardRecord>())
		{
			MemoryPackFormatterProvider.Register<FreeDoubleRewardRecord>((MemoryPackFormatter<FreeDoubleRewardRecord>)(object)new FreeDoubleRewardRecordFormatter());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<FreeDoubleRewardRecord[]>())
		{
			MemoryPackFormatterProvider.Register<FreeDoubleRewardRecord[]>((MemoryPackFormatter<FreeDoubleRewardRecord[]>)(object)new ArrayFormatter<FreeDoubleRewardRecord>());
		}
	}

	[Preserve]
	public static void Serialize(ref MemoryPackWriter writer, ref FreeDoubleRewardRecord? value)
	{
		if (value == null)
		{
			((MemoryPackWriter)(ref writer)).WriteNullObjectHeader();
		}
		else
		{
			((MemoryPackWriter)(ref writer)).WriteUnmanagedWithObjectHeader<int>((byte)1, ref value.UsedFreeCount);
		}
	}

	[Preserve]
	public static void Deserialize(ref MemoryPackReader reader, ref FreeDoubleRewardRecord? value)
	{
		byte b = default(byte);
		if (!((MemoryPackReader)(ref reader)).TryReadObjectHeader(ref b))
		{
			value = null;
			return;
		}
		int usedFreeCount = default(int);
		if (b == 1)
		{
			if (value != null)
			{
				usedFreeCount = value.UsedFreeCount;
				((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref usedFreeCount);
				goto IL_006b;
			}
			((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref usedFreeCount);
		}
		else
		{
			if (b > 1)
			{
				MemoryPackSerializationException.ThrowInvalidPropertyCount(typeof(FreeDoubleRewardRecord), (byte)1, b);
				return;
			}
			usedFreeCount = ((value != null) ? value.UsedFreeCount : 0);
			if (b != 0)
			{
				((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref usedFreeCount);
				_ = 1;
			}
			if (value != null)
			{
				goto IL_006b;
			}
		}
		value = new FreeDoubleRewardRecord
		{
			UsedFreeCount = usedFreeCount
		};
		return;
		IL_006b:
		value.UsedFreeCount = usedFreeCount;
	}
}
