using MemoryPack;
using MemoryPack.Formatters;
using MemoryPack.Internal;

namespace Example.Cfg;

[MemoryPackable(/*Could not decode attribute arguments.*/)]
public class GameWinIQMaxDecay : IMemoryPackable<GameWinIQMaxDecay>, IMemoryPackFormatterRegister
{
	[Preserve]
	private sealed class GameWinIQMaxDecayFormatter : MemoryPackFormatter<GameWinIQMaxDecay>
	{
		[Preserve]
		public override void Serialize(ref MemoryPackWriter writer, ref GameWinIQMaxDecay value)
		{
			GameWinIQMaxDecay.Serialize(ref writer, ref value);
		}

		[Preserve]
		public override void Deserialize(ref MemoryPackReader reader, ref GameWinIQMaxDecay value)
		{
			GameWinIQMaxDecay.Deserialize(ref reader, ref value);
		}
	}

	[MemoryPackOrder(0)]
	public int Id { get; private set; }

	[MemoryPackOrder(1)]
	public int Group { get; private set; }

	[MemoryPackOrder(2)]
	public int Arg1 { get; private set; }

	[MemoryPackOrder(3)]
	public float Arg2 { get; private set; }

	static GameWinIQMaxDecay()
	{
		RegisterFormatter();
	}

	[Preserve]
	public static void RegisterFormatter()
	{
		if (!MemoryPackFormatterProvider.IsRegistered<GameWinIQMaxDecay>())
		{
			MemoryPackFormatterProvider.Register<GameWinIQMaxDecay>((MemoryPackFormatter<GameWinIQMaxDecay>)(object)new GameWinIQMaxDecayFormatter());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<GameWinIQMaxDecay[]>())
		{
			MemoryPackFormatterProvider.Register<GameWinIQMaxDecay[]>((MemoryPackFormatter<GameWinIQMaxDecay[]>)(object)new ArrayFormatter<GameWinIQMaxDecay>());
		}
	}

	[Preserve]
	public static void Serialize(ref MemoryPackWriter writer, ref GameWinIQMaxDecay? value)
	{
		if (value == null)
		{
			((MemoryPackWriter)(ref writer)).WriteNullObjectHeader();
			return;
		}
		int id = value.Id;
		int num = value.Group;
		int arg = value.Arg1;
		float arg2 = value.Arg2;
		((MemoryPackWriter)(ref writer)).WriteUnmanagedWithObjectHeader<int, int, int, float>((byte)4, ref id, ref num, ref arg, ref arg2);
	}

	[Preserve]
	public static void Deserialize(ref MemoryPackReader reader, ref GameWinIQMaxDecay? value)
	{
		byte b = default(byte);
		if (!((MemoryPackReader)(ref reader)).TryReadObjectHeader(ref b))
		{
			value = null;
			return;
		}
		int id = default(int);
		int num = default(int);
		int arg = default(int);
		float arg2 = default(float);
		if (b == 4)
		{
			if (value != null)
			{
				id = value.Id;
				num = value.Group;
				arg = value.Arg1;
				arg2 = value.Arg2;
				((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref id);
				((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref num);
				((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref arg);
				((MemoryPackReader)(ref reader)).ReadUnmanaged<float>(ref arg2);
				goto IL_00f0;
			}
			((MemoryPackReader)(ref reader)).ReadUnmanaged<int, int, int, float>(ref id, ref num, ref arg, ref arg2);
		}
		else
		{
			if (b > 4)
			{
				MemoryPackSerializationException.ThrowInvalidPropertyCount(typeof(GameWinIQMaxDecay), (byte)4, b);
				return;
			}
			if (value == null)
			{
				id = 0;
				num = 0;
				arg = 0;
				arg2 = 0f;
			}
			else
			{
				id = value.Id;
				num = value.Group;
				arg = value.Arg1;
				arg2 = value.Arg2;
			}
			if (b != 0)
			{
				((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref id);
				if (b != 1)
				{
					((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref num);
					if (b != 2)
					{
						((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref arg);
						if (b != 3)
						{
							((MemoryPackReader)(ref reader)).ReadUnmanaged<float>(ref arg2);
							_ = 4;
						}
					}
				}
			}
			if (value != null)
			{
				goto IL_00f0;
			}
		}
		value = new GameWinIQMaxDecay
		{
			Id = id,
			Group = num,
			Arg1 = arg,
			Arg2 = arg2
		};
		return;
		IL_00f0:
		value.Id = id;
		value.Group = num;
		value.Arg1 = arg;
		value.Arg2 = arg2;
	}
}
