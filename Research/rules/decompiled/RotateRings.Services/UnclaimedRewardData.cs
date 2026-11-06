using System.Collections.Generic;
using System.Runtime.CompilerServices;
using MemoryPack;
using MemoryPack.Formatters;
using MemoryPack.Internal;
using XingYun.Save;

namespace RotateRings.Services;

[MemoryPackable(/*Could not decode attribute arguments.*/)]
public class UnclaimedRewardData : ISaveDefaultInitializer, IMemoryPackable<UnclaimedRewardData>, IMemoryPackFormatterRegister
{
	[Preserve]
	private sealed class UnclaimedRewardDataFormatter : MemoryPackFormatter<UnclaimedRewardData>
	{
		[Preserve]
		public override void Serialize(ref MemoryPackWriter writer, ref UnclaimedRewardData value)
		{
			UnclaimedRewardData.Serialize(ref writer, ref value);
		}

		[Preserve]
		public override void Deserialize(ref MemoryPackReader reader, ref UnclaimedRewardData value)
		{
			UnclaimedRewardData.Deserialize(ref reader, ref value);
		}
	}

	public List<ReissueRewardTransaction> Transactions;

	public void OnCreateDefault()
	{
		if (Transactions == null)
		{
			Transactions = new List<ReissueRewardTransaction>();
		}
	}

	static UnclaimedRewardData()
	{
		RegisterFormatter();
	}

	[Preserve]
	public static void RegisterFormatter()
	{
		if (!MemoryPackFormatterProvider.IsRegistered<UnclaimedRewardData>())
		{
			MemoryPackFormatterProvider.Register<UnclaimedRewardData>((MemoryPackFormatter<UnclaimedRewardData>)(object)new UnclaimedRewardDataFormatter());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<UnclaimedRewardData[]>())
		{
			MemoryPackFormatterProvider.Register<UnclaimedRewardData[]>((MemoryPackFormatter<UnclaimedRewardData[]>)(object)new ArrayFormatter<UnclaimedRewardData>());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<List<ReissueRewardTransaction>>())
		{
			MemoryPackFormatterProvider.Register<List<ReissueRewardTransaction>>((MemoryPackFormatter<List<ReissueRewardTransaction>>)(object)new ListFormatter<ReissueRewardTransaction>());
		}
	}

	[Preserve]
	public static void Serialize(ref MemoryPackWriter writer, ref UnclaimedRewardData? value)
	{
		if (value == null)
		{
			((MemoryPackWriter)(ref writer)).WriteNullObjectHeader();
			return;
		}
		((MemoryPackWriter)(ref writer)).WriteObjectHeader((byte)1);
		ListFormatter.SerializePackable<ReissueRewardTransaction>(ref writer, ref Unsafe.AsRef(in value.Transactions));
	}

	[Preserve]
	public static void Deserialize(ref MemoryPackReader reader, ref UnclaimedRewardData? value)
	{
		byte b = default(byte);
		if (!((MemoryPackReader)(ref reader)).TryReadObjectHeader(ref b))
		{
			value = null;
			return;
		}
		List<ReissueRewardTransaction> transactions;
		if (b == 1)
		{
			if (value != null)
			{
				transactions = value.Transactions;
				ListFormatter.DeserializePackable<ReissueRewardTransaction>(ref reader, ref transactions);
				goto IL_006a;
			}
			transactions = ListFormatter.DeserializePackable<ReissueRewardTransaction>(ref reader);
		}
		else
		{
			if (b > 1)
			{
				MemoryPackSerializationException.ThrowInvalidPropertyCount(typeof(UnclaimedRewardData), (byte)1, b);
				return;
			}
			transactions = ((value != null) ? value.Transactions : null);
			if (b != 0)
			{
				ListFormatter.DeserializePackable<ReissueRewardTransaction>(ref reader, ref transactions);
				_ = 1;
			}
			if (value != null)
			{
				goto IL_006a;
			}
		}
		value = new UnclaimedRewardData
		{
			Transactions = transactions
		};
		return;
		IL_006a:
		value.Transactions = transactions;
	}
}
