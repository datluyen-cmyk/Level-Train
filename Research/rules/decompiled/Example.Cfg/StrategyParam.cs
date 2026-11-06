using MemoryPack;
using MemoryPack.Formatters;
using MemoryPack.Internal;

namespace Example.Cfg;

[MemoryPackable(/*Could not decode attribute arguments.*/)]
public class StrategyParam : IMemoryPackable<StrategyParam>, IMemoryPackFormatterRegister
{
	[Preserve]
	private sealed class StrategyParamFormatter : MemoryPackFormatter<StrategyParam>
	{
		[Preserve]
		public override void Serialize(ref MemoryPackWriter writer, ref StrategyParam value)
		{
			StrategyParam.Serialize(ref writer, ref value);
		}

		[Preserve]
		public override void Deserialize(ref MemoryPackReader reader, ref StrategyParam value)
		{
			StrategyParam.Deserialize(ref reader, ref value);
		}
	}

	[MemoryPackOrder(0)]
	public string Arg1 { get; private set; }

	[MemoryPackOrder(1)]
	public string Arg2 { get; private set; }

	[MemoryPackOrder(2)]
	public string Arg3 { get; private set; }

	[MemoryPackOrder(3)]
	public string Arg4 { get; private set; }

	[MemoryPackOrder(4)]
	public string Arg5 { get; private set; }

	static StrategyParam()
	{
		RegisterFormatter();
	}

	[Preserve]
	public static void RegisterFormatter()
	{
		if (!MemoryPackFormatterProvider.IsRegistered<StrategyParam>())
		{
			MemoryPackFormatterProvider.Register<StrategyParam>((MemoryPackFormatter<StrategyParam>)(object)new StrategyParamFormatter());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<StrategyParam[]>())
		{
			MemoryPackFormatterProvider.Register<StrategyParam[]>((MemoryPackFormatter<StrategyParam[]>)(object)new ArrayFormatter<StrategyParam>());
		}
	}

	[Preserve]
	public static void Serialize(ref MemoryPackWriter writer, ref StrategyParam? value)
	{
		if (value == null)
		{
			((MemoryPackWriter)(ref writer)).WriteNullObjectHeader();
			return;
		}
		((MemoryPackWriter)(ref writer)).WriteObjectHeader((byte)5);
		((MemoryPackWriter)(ref writer)).WriteString(value.Arg1);
		((MemoryPackWriter)(ref writer)).WriteString(value.Arg2);
		((MemoryPackWriter)(ref writer)).WriteString(value.Arg3);
		((MemoryPackWriter)(ref writer)).WriteString(value.Arg4);
		((MemoryPackWriter)(ref writer)).WriteString(value.Arg5);
	}

	[Preserve]
	public static void Deserialize(ref MemoryPackReader reader, ref StrategyParam? value)
	{
		byte b = default(byte);
		if (!((MemoryPackReader)(ref reader)).TryReadObjectHeader(ref b))
		{
			value = null;
			return;
		}
		string arg;
		string arg2;
		string arg3;
		string arg4;
		string arg5;
		if (b == 5)
		{
			if (value != null)
			{
				arg = value.Arg1;
				arg2 = value.Arg2;
				arg3 = value.Arg3;
				arg4 = value.Arg4;
				arg5 = value.Arg5;
				arg = ((MemoryPackReader)(ref reader)).ReadString();
				arg2 = ((MemoryPackReader)(ref reader)).ReadString();
				arg3 = ((MemoryPackReader)(ref reader)).ReadString();
				arg4 = ((MemoryPackReader)(ref reader)).ReadString();
				arg5 = ((MemoryPackReader)(ref reader)).ReadString();
				goto IL_0129;
			}
			arg = ((MemoryPackReader)(ref reader)).ReadString();
			arg2 = ((MemoryPackReader)(ref reader)).ReadString();
			arg3 = ((MemoryPackReader)(ref reader)).ReadString();
			arg4 = ((MemoryPackReader)(ref reader)).ReadString();
			arg5 = ((MemoryPackReader)(ref reader)).ReadString();
		}
		else
		{
			if (b > 5)
			{
				MemoryPackSerializationException.ThrowInvalidPropertyCount(typeof(StrategyParam), (byte)5, b);
				return;
			}
			if (value == null)
			{
				arg = null;
				arg2 = null;
				arg3 = null;
				arg4 = null;
				arg5 = null;
			}
			else
			{
				arg = value.Arg1;
				arg2 = value.Arg2;
				arg3 = value.Arg3;
				arg4 = value.Arg4;
				arg5 = value.Arg5;
			}
			if (b != 0)
			{
				arg = ((MemoryPackReader)(ref reader)).ReadString();
				if (b != 1)
				{
					arg2 = ((MemoryPackReader)(ref reader)).ReadString();
					if (b != 2)
					{
						arg3 = ((MemoryPackReader)(ref reader)).ReadString();
						if (b != 3)
						{
							arg4 = ((MemoryPackReader)(ref reader)).ReadString();
							if (b != 4)
							{
								arg5 = ((MemoryPackReader)(ref reader)).ReadString();
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
		value = new StrategyParam
		{
			Arg1 = arg,
			Arg2 = arg2,
			Arg3 = arg3,
			Arg4 = arg4,
			Arg5 = arg5
		};
		return;
		IL_0129:
		value.Arg1 = arg;
		value.Arg2 = arg2;
		value.Arg3 = arg3;
		value.Arg4 = arg4;
		value.Arg5 = arg5;
	}
}
