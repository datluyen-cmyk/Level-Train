using MemoryPack;
using MemoryPack.Formatters;
using MemoryPack.Internal;

namespace Example.Cfg;

[MemoryPackable(/*Could not decode attribute arguments.*/)]
public class HeadConfig : IMemoryPackable<HeadConfig>, IMemoryPackFormatterRegister
{
	[Preserve]
	private sealed class HeadConfigFormatter : MemoryPackFormatter<HeadConfig>
	{
		[Preserve]
		public override void Serialize(ref MemoryPackWriter writer, ref HeadConfig value)
		{
			HeadConfig.Serialize(ref writer, ref value);
		}

		[Preserve]
		public override void Deserialize(ref MemoryPackReader reader, ref HeadConfig value)
		{
			HeadConfig.Deserialize(ref reader, ref value);
		}
	}

	[MemoryPackOrder(0)]
	public int ID { get; private set; }

	[MemoryPackOrder(1)]
	public string Icon { get; private set; }

	[MemoryPackOrder(2)]
	public string Frame { get; private set; }

	static HeadConfig()
	{
		RegisterFormatter();
	}

	[Preserve]
	public static void RegisterFormatter()
	{
		if (!MemoryPackFormatterProvider.IsRegistered<HeadConfig>())
		{
			MemoryPackFormatterProvider.Register<HeadConfig>((MemoryPackFormatter<HeadConfig>)(object)new HeadConfigFormatter());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<HeadConfig[]>())
		{
			MemoryPackFormatterProvider.Register<HeadConfig[]>((MemoryPackFormatter<HeadConfig[]>)(object)new ArrayFormatter<HeadConfig>());
		}
	}

	[Preserve]
	public static void Serialize(ref MemoryPackWriter writer, ref HeadConfig? value)
	{
		if (value == null)
		{
			((MemoryPackWriter)(ref writer)).WriteNullObjectHeader();
			return;
		}
		int iD = value.ID;
		((MemoryPackWriter)(ref writer)).WriteUnmanagedWithObjectHeader<int>((byte)3, ref iD);
		((MemoryPackWriter)(ref writer)).WriteString(value.Icon);
		((MemoryPackWriter)(ref writer)).WriteString(value.Frame);
	}

	[Preserve]
	public static void Deserialize(ref MemoryPackReader reader, ref HeadConfig? value)
	{
		byte b = default(byte);
		if (!((MemoryPackReader)(ref reader)).TryReadObjectHeader(ref b))
		{
			value = null;
			return;
		}
		int iD = default(int);
		string icon;
		string frame;
		if (b == 3)
		{
			if (value != null)
			{
				iD = value.ID;
				icon = value.Icon;
				frame = value.Frame;
				((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref iD);
				icon = ((MemoryPackReader)(ref reader)).ReadString();
				frame = ((MemoryPackReader)(ref reader)).ReadString();
				goto IL_00c4;
			}
			((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref iD);
			icon = ((MemoryPackReader)(ref reader)).ReadString();
			frame = ((MemoryPackReader)(ref reader)).ReadString();
		}
		else
		{
			if (b > 3)
			{
				MemoryPackSerializationException.ThrowInvalidPropertyCount(typeof(HeadConfig), (byte)3, b);
				return;
			}
			if (value == null)
			{
				iD = 0;
				icon = null;
				frame = null;
			}
			else
			{
				iD = value.ID;
				icon = value.Icon;
				frame = value.Frame;
			}
			if (b != 0)
			{
				((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref iD);
				if (b != 1)
				{
					icon = ((MemoryPackReader)(ref reader)).ReadString();
					if (b != 2)
					{
						frame = ((MemoryPackReader)(ref reader)).ReadString();
						_ = 3;
					}
				}
			}
			if (value != null)
			{
				goto IL_00c4;
			}
		}
		value = new HeadConfig
		{
			ID = iD,
			Icon = icon,
			Frame = frame
		};
		return;
		IL_00c4:
		value.ID = iD;
		value.Icon = icon;
		value.Frame = frame;
	}
}
