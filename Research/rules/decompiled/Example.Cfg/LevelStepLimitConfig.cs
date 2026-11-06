using MemoryPack;
using MemoryPack.Formatters;
using MemoryPack.Internal;

namespace Example.Cfg;

[MemoryPackable(/*Could not decode attribute arguments.*/)]
public class LevelStepLimitConfig : IMemoryPackable<LevelStepLimitConfig>, IMemoryPackFormatterRegister
{
	[Preserve]
	private sealed class LevelStepLimitConfigFormatter : MemoryPackFormatter<LevelStepLimitConfig>
	{
		[Preserve]
		public override void Serialize(ref MemoryPackWriter writer, ref LevelStepLimitConfig value)
		{
			LevelStepLimitConfig.Serialize(ref writer, ref value);
		}

		[Preserve]
		public override void Deserialize(ref MemoryPackReader reader, ref LevelStepLimitConfig value)
		{
			LevelStepLimitConfig.Deserialize(ref reader, ref value);
		}
	}

	[MemoryPackOrder(0)]
	public int Id { get; private set; }

	[MemoryPackOrder(1)]
	public int LooseStepCount { get; private set; }

	[MemoryPackOrder(2)]
	public int StrictStepCount { get; private set; }

	static LevelStepLimitConfig()
	{
		RegisterFormatter();
	}

	[Preserve]
	public static void RegisterFormatter()
	{
		if (!MemoryPackFormatterProvider.IsRegistered<LevelStepLimitConfig>())
		{
			MemoryPackFormatterProvider.Register<LevelStepLimitConfig>((MemoryPackFormatter<LevelStepLimitConfig>)(object)new LevelStepLimitConfigFormatter());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<LevelStepLimitConfig[]>())
		{
			MemoryPackFormatterProvider.Register<LevelStepLimitConfig[]>((MemoryPackFormatter<LevelStepLimitConfig[]>)(object)new ArrayFormatter<LevelStepLimitConfig>());
		}
	}

	[Preserve]
	public static void Serialize(ref MemoryPackWriter writer, ref LevelStepLimitConfig? value)
	{
		if (value == null)
		{
			((MemoryPackWriter)(ref writer)).WriteNullObjectHeader();
			return;
		}
		int id = value.Id;
		int looseStepCount = value.LooseStepCount;
		int strictStepCount = value.StrictStepCount;
		((MemoryPackWriter)(ref writer)).WriteUnmanagedWithObjectHeader<int, int, int>((byte)3, ref id, ref looseStepCount, ref strictStepCount);
	}

	[Preserve]
	public static void Deserialize(ref MemoryPackReader reader, ref LevelStepLimitConfig? value)
	{
		byte b = default(byte);
		if (!((MemoryPackReader)(ref reader)).TryReadObjectHeader(ref b))
		{
			value = null;
			return;
		}
		int id = default(int);
		int looseStepCount = default(int);
		int strictStepCount = default(int);
		if (b == 3)
		{
			if (value != null)
			{
				id = value.Id;
				looseStepCount = value.LooseStepCount;
				strictStepCount = value.StrictStepCount;
				((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref id);
				((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref looseStepCount);
				((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref strictStepCount);
				goto IL_00be;
			}
			((MemoryPackReader)(ref reader)).ReadUnmanaged<int, int, int>(ref id, ref looseStepCount, ref strictStepCount);
		}
		else
		{
			if (b > 3)
			{
				MemoryPackSerializationException.ThrowInvalidPropertyCount(typeof(LevelStepLimitConfig), (byte)3, b);
				return;
			}
			if (value == null)
			{
				id = 0;
				looseStepCount = 0;
				strictStepCount = 0;
			}
			else
			{
				id = value.Id;
				looseStepCount = value.LooseStepCount;
				strictStepCount = value.StrictStepCount;
			}
			if (b != 0)
			{
				((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref id);
				if (b != 1)
				{
					((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref looseStepCount);
					if (b != 2)
					{
						((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref strictStepCount);
						_ = 3;
					}
				}
			}
			if (value != null)
			{
				goto IL_00be;
			}
		}
		value = new LevelStepLimitConfig
		{
			Id = id,
			LooseStepCount = looseStepCount,
			StrictStepCount = strictStepCount
		};
		return;
		IL_00be:
		value.Id = id;
		value.LooseStepCount = looseStepCount;
		value.StrictStepCount = strictStepCount;
	}
}
