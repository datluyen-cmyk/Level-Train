using MemoryPack;
using MemoryPack.Formatters;
using MemoryPack.Internal;

namespace Example.Cfg;

[MemoryPackable(/*Could not decode attribute arguments.*/)]
public class CameraConfig : IMemoryPackable<CameraConfig>, IMemoryPackFormatterRegister
{
	[Preserve]
	private sealed class CameraConfigFormatter : MemoryPackFormatter<CameraConfig>
	{
		[Preserve]
		public override void Serialize(ref MemoryPackWriter writer, ref CameraConfig value)
		{
			CameraConfig.Serialize(ref writer, ref value);
		}

		[Preserve]
		public override void Deserialize(ref MemoryPackReader reader, ref CameraConfig value)
		{
			CameraConfig.Deserialize(ref reader, ref value);
		}
	}

	[MemoryPackOrder(0)]
	public string Id { get; private set; }

	[MemoryPackOrder(1)]
	public float ScaleValue { get; private set; }

	static CameraConfig()
	{
		RegisterFormatter();
	}

	[Preserve]
	public static void RegisterFormatter()
	{
		if (!MemoryPackFormatterProvider.IsRegistered<CameraConfig>())
		{
			MemoryPackFormatterProvider.Register<CameraConfig>((MemoryPackFormatter<CameraConfig>)(object)new CameraConfigFormatter());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<CameraConfig[]>())
		{
			MemoryPackFormatterProvider.Register<CameraConfig[]>((MemoryPackFormatter<CameraConfig[]>)(object)new ArrayFormatter<CameraConfig>());
		}
	}

	[Preserve]
	public static void Serialize(ref MemoryPackWriter writer, ref CameraConfig? value)
	{
		if (value == null)
		{
			((MemoryPackWriter)(ref writer)).WriteNullObjectHeader();
			return;
		}
		((MemoryPackWriter)(ref writer)).WriteObjectHeader((byte)2);
		((MemoryPackWriter)(ref writer)).WriteString(value.Id);
		float scaleValue = value.ScaleValue;
		((MemoryPackWriter)(ref writer)).WriteUnmanaged<float>(ref scaleValue);
	}

	[Preserve]
	public static void Deserialize(ref MemoryPackReader reader, ref CameraConfig? value)
	{
		byte b = default(byte);
		if (!((MemoryPackReader)(ref reader)).TryReadObjectHeader(ref b))
		{
			value = null;
			return;
		}
		float scaleValue = default(float);
		string id;
		if (b == 2)
		{
			if (value != null)
			{
				id = value.Id;
				scaleValue = value.ScaleValue;
				id = ((MemoryPackReader)(ref reader)).ReadString();
				((MemoryPackReader)(ref reader)).ReadUnmanaged<float>(ref scaleValue);
				goto IL_009d;
			}
			id = ((MemoryPackReader)(ref reader)).ReadString();
			((MemoryPackReader)(ref reader)).ReadUnmanaged<float>(ref scaleValue);
		}
		else
		{
			if (b > 2)
			{
				MemoryPackSerializationException.ThrowInvalidPropertyCount(typeof(CameraConfig), (byte)2, b);
				return;
			}
			if (value == null)
			{
				id = null;
				scaleValue = 0f;
			}
			else
			{
				id = value.Id;
				scaleValue = value.ScaleValue;
			}
			if (b != 0)
			{
				id = ((MemoryPackReader)(ref reader)).ReadString();
				if (b != 1)
				{
					((MemoryPackReader)(ref reader)).ReadUnmanaged<float>(ref scaleValue);
					_ = 2;
				}
			}
			if (value != null)
			{
				goto IL_009d;
			}
		}
		value = new CameraConfig
		{
			Id = id,
			ScaleValue = scaleValue
		};
		return;
		IL_009d:
		value.Id = id;
		value.ScaleValue = scaleValue;
	}
}
