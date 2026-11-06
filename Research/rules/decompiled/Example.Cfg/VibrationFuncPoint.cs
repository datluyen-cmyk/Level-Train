using MemoryPack;
using MemoryPack.Formatters;
using MemoryPack.Internal;

namespace Example.Cfg;

[MemoryPackable(/*Could not decode attribute arguments.*/)]
public class VibrationFuncPoint : IMemoryPackable<VibrationFuncPoint>, IMemoryPackFormatterRegister
{
	[Preserve]
	private sealed class VibrationFuncPointFormatter : MemoryPackFormatter<VibrationFuncPoint>
	{
		[Preserve]
		public override void Serialize(ref MemoryPackWriter writer, ref VibrationFuncPoint value)
		{
			VibrationFuncPoint.Serialize(ref writer, ref value);
		}

		[Preserve]
		public override void Deserialize(ref MemoryPackReader reader, ref VibrationFuncPoint value)
		{
			VibrationFuncPoint.Deserialize(ref reader, ref value);
		}
	}

	[MemoryPackOrder(0)]
	public int ID { get; private set; }

	static VibrationFuncPoint()
	{
		RegisterFormatter();
	}

	[Preserve]
	public static void RegisterFormatter()
	{
		if (!MemoryPackFormatterProvider.IsRegistered<VibrationFuncPoint>())
		{
			MemoryPackFormatterProvider.Register<VibrationFuncPoint>((MemoryPackFormatter<VibrationFuncPoint>)(object)new VibrationFuncPointFormatter());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<VibrationFuncPoint[]>())
		{
			MemoryPackFormatterProvider.Register<VibrationFuncPoint[]>((MemoryPackFormatter<VibrationFuncPoint[]>)(object)new ArrayFormatter<VibrationFuncPoint>());
		}
	}

	[Preserve]
	public static void Serialize(ref MemoryPackWriter writer, ref VibrationFuncPoint? value)
	{
		if (value == null)
		{
			((MemoryPackWriter)(ref writer)).WriteNullObjectHeader();
			return;
		}
		int iD = value.ID;
		((MemoryPackWriter)(ref writer)).WriteUnmanagedWithObjectHeader<int>((byte)1, ref iD);
	}

	[Preserve]
	public static void Deserialize(ref MemoryPackReader reader, ref VibrationFuncPoint? value)
	{
		byte b = default(byte);
		if (!((MemoryPackReader)(ref reader)).TryReadObjectHeader(ref b))
		{
			value = null;
			return;
		}
		int iD = default(int);
		if (b == 1)
		{
			if (value != null)
			{
				iD = value.ID;
				((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref iD);
				goto IL_006b;
			}
			((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref iD);
		}
		else
		{
			if (b > 1)
			{
				MemoryPackSerializationException.ThrowInvalidPropertyCount(typeof(VibrationFuncPoint), (byte)1, b);
				return;
			}
			iD = ((value != null) ? value.ID : 0);
			if (b != 0)
			{
				((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref iD);
				_ = 1;
			}
			if (value != null)
			{
				goto IL_006b;
			}
		}
		value = new VibrationFuncPoint
		{
			ID = iD
		};
		return;
		IL_006b:
		value.ID = iD;
	}
}
