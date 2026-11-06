using MemoryPack;
using MemoryPack.Formatters;
using MemoryPack.Internal;

namespace Example.Cfg;

[MemoryPackable(/*Could not decode attribute arguments.*/)]
public class VibrationLevel_Android : IMemoryPackable<VibrationLevel_Android>, IMemoryPackFormatterRegister
{
	[Preserve]
	private sealed class VibrationLevel_AndroidFormatter : MemoryPackFormatter<VibrationLevel_Android>
	{
		[Preserve]
		public override void Serialize(ref MemoryPackWriter writer, ref VibrationLevel_Android value)
		{
			VibrationLevel_Android.Serialize(ref writer, ref value);
		}

		[Preserve]
		public override void Deserialize(ref MemoryPackReader reader, ref VibrationLevel_Android value)
		{
			VibrationLevel_Android.Deserialize(ref reader, ref value);
		}
	}

	[MemoryPackOrder(0)]
	public string ID { get; private set; }

	[MemoryPackOrder(1)]
	public int Level { get; private set; }

	static VibrationLevel_Android()
	{
		RegisterFormatter();
	}

	[Preserve]
	public static void RegisterFormatter()
	{
		if (!MemoryPackFormatterProvider.IsRegistered<VibrationLevel_Android>())
		{
			MemoryPackFormatterProvider.Register<VibrationLevel_Android>((MemoryPackFormatter<VibrationLevel_Android>)(object)new VibrationLevel_AndroidFormatter());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<VibrationLevel_Android[]>())
		{
			MemoryPackFormatterProvider.Register<VibrationLevel_Android[]>((MemoryPackFormatter<VibrationLevel_Android[]>)(object)new ArrayFormatter<VibrationLevel_Android>());
		}
	}

	[Preserve]
	public static void Serialize(ref MemoryPackWriter writer, ref VibrationLevel_Android? value)
	{
		if (value == null)
		{
			((MemoryPackWriter)(ref writer)).WriteNullObjectHeader();
			return;
		}
		((MemoryPackWriter)(ref writer)).WriteObjectHeader((byte)2);
		((MemoryPackWriter)(ref writer)).WriteString(value.ID);
		int level = value.Level;
		((MemoryPackWriter)(ref writer)).WriteUnmanaged<int>(ref level);
	}

	[Preserve]
	public static void Deserialize(ref MemoryPackReader reader, ref VibrationLevel_Android? value)
	{
		byte b = default(byte);
		if (!((MemoryPackReader)(ref reader)).TryReadObjectHeader(ref b))
		{
			value = null;
			return;
		}
		int level = default(int);
		string iD;
		if (b == 2)
		{
			if (value != null)
			{
				iD = value.ID;
				level = value.Level;
				iD = ((MemoryPackReader)(ref reader)).ReadString();
				((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref level);
				goto IL_0099;
			}
			iD = ((MemoryPackReader)(ref reader)).ReadString();
			((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref level);
		}
		else
		{
			if (b > 2)
			{
				MemoryPackSerializationException.ThrowInvalidPropertyCount(typeof(VibrationLevel_Android), (byte)2, b);
				return;
			}
			if (value == null)
			{
				iD = null;
				level = 0;
			}
			else
			{
				iD = value.ID;
				level = value.Level;
			}
			if (b != 0)
			{
				iD = ((MemoryPackReader)(ref reader)).ReadString();
				if (b != 1)
				{
					((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref level);
					_ = 2;
				}
			}
			if (value != null)
			{
				goto IL_0099;
			}
		}
		value = new VibrationLevel_Android
		{
			ID = iD,
			Level = level
		};
		return;
		IL_0099:
		value.ID = iD;
		value.Level = level;
	}
}
