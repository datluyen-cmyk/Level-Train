using MemoryPack;
using MemoryPack.Formatters;
using MemoryPack.Internal;

namespace Example.Cfg;

[MemoryPackable(/*Could not decode attribute arguments.*/)]
public class GameWinIQCalcRules : IMemoryPackable<GameWinIQCalcRules>, IMemoryPackFormatterRegister
{
	[Preserve]
	private sealed class GameWinIQCalcRulesFormatter : MemoryPackFormatter<GameWinIQCalcRules>
	{
		[Preserve]
		public override void Serialize(ref MemoryPackWriter writer, ref GameWinIQCalcRules value)
		{
			GameWinIQCalcRules.Serialize(ref writer, ref value);
		}

		[Preserve]
		public override void Deserialize(ref MemoryPackReader reader, ref GameWinIQCalcRules value)
		{
			GameWinIQCalcRules.Deserialize(ref reader, ref value);
		}
	}

	[MemoryPackOrder(0)]
	public int Id { get; private set; }

	[MemoryPackOrder(1)]
	public int Group { get; private set; }

	[MemoryPackOrder(2)]
	public string IQCalcRule1 { get; private set; }

	[MemoryPackOrder(3)]
	public string IQCalcRule2 { get; private set; }

	[MemoryPackOrder(4)]
	public int InvalidStepThreshold { get; private set; }

	static GameWinIQCalcRules()
	{
		RegisterFormatter();
	}

	[Preserve]
	public static void RegisterFormatter()
	{
		if (!MemoryPackFormatterProvider.IsRegistered<GameWinIQCalcRules>())
		{
			MemoryPackFormatterProvider.Register<GameWinIQCalcRules>((MemoryPackFormatter<GameWinIQCalcRules>)(object)new GameWinIQCalcRulesFormatter());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<GameWinIQCalcRules[]>())
		{
			MemoryPackFormatterProvider.Register<GameWinIQCalcRules[]>((MemoryPackFormatter<GameWinIQCalcRules[]>)(object)new ArrayFormatter<GameWinIQCalcRules>());
		}
	}

	[Preserve]
	public static void Serialize(ref MemoryPackWriter writer, ref GameWinIQCalcRules? value)
	{
		if (value == null)
		{
			((MemoryPackWriter)(ref writer)).WriteNullObjectHeader();
			return;
		}
		int id = value.Id;
		int num = value.Group;
		((MemoryPackWriter)(ref writer)).WriteUnmanagedWithObjectHeader<int, int>((byte)5, ref id, ref num);
		((MemoryPackWriter)(ref writer)).WriteString(value.IQCalcRule1);
		((MemoryPackWriter)(ref writer)).WriteString(value.IQCalcRule2);
		id = value.InvalidStepThreshold;
		((MemoryPackWriter)(ref writer)).WriteUnmanaged<int>(ref id);
	}

	[Preserve]
	public static void Deserialize(ref MemoryPackReader reader, ref GameWinIQCalcRules? value)
	{
		byte b = default(byte);
		if (!((MemoryPackReader)(ref reader)).TryReadObjectHeader(ref b))
		{
			value = null;
			return;
		}
		int id = default(int);
		int num = default(int);
		int invalidStepThreshold = default(int);
		string iQCalcRule;
		string iQCalcRule2;
		if (b == 5)
		{
			if (value != null)
			{
				id = value.Id;
				num = value.Group;
				iQCalcRule = value.IQCalcRule1;
				iQCalcRule2 = value.IQCalcRule2;
				invalidStepThreshold = value.InvalidStepThreshold;
				((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref id);
				((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref num);
				iQCalcRule = ((MemoryPackReader)(ref reader)).ReadString();
				iQCalcRule2 = ((MemoryPackReader)(ref reader)).ReadString();
				((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref invalidStepThreshold);
				goto IL_0129;
			}
			((MemoryPackReader)(ref reader)).ReadUnmanaged<int, int>(ref id, ref num);
			iQCalcRule = ((MemoryPackReader)(ref reader)).ReadString();
			iQCalcRule2 = ((MemoryPackReader)(ref reader)).ReadString();
			((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref invalidStepThreshold);
		}
		else
		{
			if (b > 5)
			{
				MemoryPackSerializationException.ThrowInvalidPropertyCount(typeof(GameWinIQCalcRules), (byte)5, b);
				return;
			}
			if (value == null)
			{
				id = 0;
				num = 0;
				iQCalcRule = null;
				iQCalcRule2 = null;
				invalidStepThreshold = 0;
			}
			else
			{
				id = value.Id;
				num = value.Group;
				iQCalcRule = value.IQCalcRule1;
				iQCalcRule2 = value.IQCalcRule2;
				invalidStepThreshold = value.InvalidStepThreshold;
			}
			if (b != 0)
			{
				((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref id);
				if (b != 1)
				{
					((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref num);
					if (b != 2)
					{
						iQCalcRule = ((MemoryPackReader)(ref reader)).ReadString();
						if (b != 3)
						{
							iQCalcRule2 = ((MemoryPackReader)(ref reader)).ReadString();
							if (b != 4)
							{
								((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref invalidStepThreshold);
								_ = 5;
							}
						}
					}
				}
			}
			if (value != null)
			{
				goto IL_0129;
			}
		}
		value = new GameWinIQCalcRules
		{
			Id = id,
			Group = num,
			IQCalcRule1 = iQCalcRule,
			IQCalcRule2 = iQCalcRule2,
			InvalidStepThreshold = invalidStepThreshold
		};
		return;
		IL_0129:
		value.Id = id;
		value.Group = num;
		value.IQCalcRule1 = iQCalcRule;
		value.IQCalcRule2 = iQCalcRule2;
		value.InvalidStepThreshold = invalidStepThreshold;
	}
}
