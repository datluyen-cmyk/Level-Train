using MemoryPack;
using MemoryPack.Formatters;
using MemoryPack.Internal;
using XingYun.Save;

namespace RotateRings.Ad;

[MemoryPackable(/*Could not decode attribute arguments.*/)]
public class FirstEcpmTierRecord : ISaveDefaultInitializer, IMemoryPackable<FirstEcpmTierRecord>, IMemoryPackFormatterRegister
{
	[Preserve]
	private sealed class FirstEcpmTierRecordFormatter : MemoryPackFormatter<FirstEcpmTierRecord>
	{
		[Preserve]
		public override void Serialize(ref MemoryPackWriter writer, ref FirstEcpmTierRecord value)
		{
			FirstEcpmTierRecord.Serialize(ref writer, ref value);
		}

		[Preserve]
		public override void Deserialize(ref MemoryPackReader reader, ref FirstEcpmTierRecord value)
		{
			FirstEcpmTierRecord.Deserialize(ref reader, ref value);
		}
	}

	public bool Resolved;

	public int Tier;

	public void OnCreateDefault()
	{
		Resolved = false;
		Tier = 0;
	}

	static FirstEcpmTierRecord()
	{
		RegisterFormatter();
	}

	[Preserve]
	public static void RegisterFormatter()
	{
		if (!MemoryPackFormatterProvider.IsRegistered<FirstEcpmTierRecord>())
		{
			MemoryPackFormatterProvider.Register<FirstEcpmTierRecord>((MemoryPackFormatter<FirstEcpmTierRecord>)(object)new FirstEcpmTierRecordFormatter());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<FirstEcpmTierRecord[]>())
		{
			MemoryPackFormatterProvider.Register<FirstEcpmTierRecord[]>((MemoryPackFormatter<FirstEcpmTierRecord[]>)(object)new ArrayFormatter<FirstEcpmTierRecord>());
		}
	}

	[Preserve]
	public static void Serialize(ref MemoryPackWriter writer, ref FirstEcpmTierRecord? value)
	{
		if (value == null)
		{
			((MemoryPackWriter)(ref writer)).WriteNullObjectHeader();
		}
		else
		{
			((MemoryPackWriter)(ref writer)).WriteUnmanagedWithObjectHeader<bool, int>((byte)2, ref value.Resolved, ref value.Tier);
		}
	}

	[Preserve]
	public static void Deserialize(ref MemoryPackReader reader, ref FirstEcpmTierRecord? value)
	{
		byte b = default(byte);
		if (!((MemoryPackReader)(ref reader)).TryReadObjectHeader(ref b))
		{
			value = null;
			return;
		}
		bool resolved = default(bool);
		int tier = default(int);
		if (b == 2)
		{
			if (value != null)
			{
				resolved = value.Resolved;
				tier = value.Tier;
				((MemoryPackReader)(ref reader)).ReadUnmanaged<bool>(ref resolved);
				((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref tier);
				goto IL_0096;
			}
			((MemoryPackReader)(ref reader)).ReadUnmanaged<bool, int>(ref resolved, ref tier);
		}
		else
		{
			if (b > 2)
			{
				MemoryPackSerializationException.ThrowInvalidPropertyCount(typeof(FirstEcpmTierRecord), (byte)2, b);
				return;
			}
			if (value == null)
			{
				resolved = false;
				tier = 0;
			}
			else
			{
				resolved = value.Resolved;
				tier = value.Tier;
			}
			if (b != 0)
			{
				((MemoryPackReader)(ref reader)).ReadUnmanaged<bool>(ref resolved);
				if (b != 1)
				{
					((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref tier);
					_ = 2;
				}
			}
			if (value != null)
			{
				goto IL_0096;
			}
		}
		value = new FirstEcpmTierRecord
		{
			Resolved = resolved,
			Tier = tier
		};
		return;
		IL_0096:
		value.Resolved = resolved;
		value.Tier = tier;
	}
}
