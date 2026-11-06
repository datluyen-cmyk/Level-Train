using System.Collections.Generic;
using MemoryPack;
using MemoryPack.Formatters;
using MemoryPack.Internal;

namespace Example.Cfg;

[MemoryPackable(/*Could not decode attribute arguments.*/)]
public class RewardConfig : IMemoryPackable<RewardConfig>, IMemoryPackFormatterRegister
{
	[Preserve]
	private sealed class RewardConfigFormatter : MemoryPackFormatter<RewardConfig>
	{
		[Preserve]
		public override void Serialize(ref MemoryPackWriter writer, ref RewardConfig value)
		{
			RewardConfig.Serialize(ref writer, ref value);
		}

		[Preserve]
		public override void Deserialize(ref MemoryPackReader reader, ref RewardConfig value)
		{
			RewardConfig.Deserialize(ref reader, ref value);
		}
	}

	[MemoryPackOrder(0)]
	public int Id { get; private set; }

	[MemoryPackOrder(1)]
	public Dictionary<int, int> get { get; private set; }

	static RewardConfig()
	{
		RegisterFormatter();
	}

	[Preserve]
	public static void RegisterFormatter()
	{
		if (!MemoryPackFormatterProvider.IsRegistered<RewardConfig>())
		{
			MemoryPackFormatterProvider.Register<RewardConfig>((MemoryPackFormatter<RewardConfig>)(object)new RewardConfigFormatter());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<RewardConfig[]>())
		{
			MemoryPackFormatterProvider.Register<RewardConfig[]>((MemoryPackFormatter<RewardConfig[]>)(object)new ArrayFormatter<RewardConfig>());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<Dictionary<int, int>>())
		{
			MemoryPackFormatterProvider.Register<Dictionary<int, int>>((MemoryPackFormatter<Dictionary<int, int>>)(object)new DictionaryFormatter<int, int>());
		}
	}

	[Preserve]
	public static void Serialize(ref MemoryPackWriter writer, ref RewardConfig? value)
	{
		if (value == null)
		{
			((MemoryPackWriter)(ref writer)).WriteNullObjectHeader();
			return;
		}
		int id = value.Id;
		((MemoryPackWriter)(ref writer)).WriteUnmanagedWithObjectHeader<int>((byte)2, ref id);
		Dictionary<int, int> dictionary = value.get;
		((MemoryPackWriter)(ref writer)).WriteValue<Dictionary<int, int>>(ref dictionary);
	}

	[Preserve]
	public static void Deserialize(ref MemoryPackReader reader, ref RewardConfig? value)
	{
		byte b = default(byte);
		if (!((MemoryPackReader)(ref reader)).TryReadObjectHeader(ref b))
		{
			value = null;
			return;
		}
		int id = default(int);
		Dictionary<int, int> dictionary;
		if (b == 2)
		{
			if (value != null)
			{
				id = value.Id;
				dictionary = value.get;
				((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref id);
				((MemoryPackReader)(ref reader)).ReadValue<Dictionary<int, int>>(ref dictionary);
				goto IL_009b;
			}
			((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref id);
			dictionary = ((MemoryPackReader)(ref reader)).ReadValue<Dictionary<int, int>>();
		}
		else
		{
			if (b > 2)
			{
				MemoryPackSerializationException.ThrowInvalidPropertyCount(typeof(RewardConfig), (byte)2, b);
				return;
			}
			if (value == null)
			{
				id = 0;
				dictionary = null;
			}
			else
			{
				id = value.Id;
				dictionary = value.get;
			}
			if (b != 0)
			{
				((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref id);
				if (b != 1)
				{
					((MemoryPackReader)(ref reader)).ReadValue<Dictionary<int, int>>(ref dictionary);
					_ = 2;
				}
			}
			if (value != null)
			{
				goto IL_009b;
			}
		}
		value = new RewardConfig
		{
			Id = id,
			get = dictionary
		};
		return;
		IL_009b:
		value.Id = id;
		value.get = dictionary;
	}
}
