using MemoryPack;
using MemoryPack.Formatters;
using MemoryPack.Internal;

namespace Example.Cfg;

[MemoryPackable(/*Could not decode attribute arguments.*/)]
public class AudioConfig : IMemoryPackable<AudioConfig>, IMemoryPackFormatterRegister
{
	[Preserve]
	private sealed class AudioConfigFormatter : MemoryPackFormatter<AudioConfig>
	{
		[Preserve]
		public override void Serialize(ref MemoryPackWriter writer, ref AudioConfig value)
		{
			AudioConfig.Serialize(ref writer, ref value);
		}

		[Preserve]
		public override void Deserialize(ref MemoryPackReader reader, ref AudioConfig value)
		{
			AudioConfig.Deserialize(ref reader, ref value);
		}
	}

	[MemoryPackOrder(0)]
	public string ID { get; private set; }

	[MemoryPackOrder(1)]
	public float Volume { get; private set; }

	static AudioConfig()
	{
		RegisterFormatter();
	}

	[Preserve]
	public static void RegisterFormatter()
	{
		if (!MemoryPackFormatterProvider.IsRegistered<AudioConfig>())
		{
			MemoryPackFormatterProvider.Register<AudioConfig>((MemoryPackFormatter<AudioConfig>)(object)new AudioConfigFormatter());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<AudioConfig[]>())
		{
			MemoryPackFormatterProvider.Register<AudioConfig[]>((MemoryPackFormatter<AudioConfig[]>)(object)new ArrayFormatter<AudioConfig>());
		}
	}

	[Preserve]
	public static void Serialize(ref MemoryPackWriter writer, ref AudioConfig? value)
	{
		if (value == null)
		{
			((MemoryPackWriter)(ref writer)).WriteNullObjectHeader();
			return;
		}
		((MemoryPackWriter)(ref writer)).WriteObjectHeader((byte)2);
		((MemoryPackWriter)(ref writer)).WriteString(value.ID);
		float volume = value.Volume;
		((MemoryPackWriter)(ref writer)).WriteUnmanaged<float>(ref volume);
	}

	[Preserve]
	public static void Deserialize(ref MemoryPackReader reader, ref AudioConfig? value)
	{
		byte b = default(byte);
		if (!((MemoryPackReader)(ref reader)).TryReadObjectHeader(ref b))
		{
			value = null;
			return;
		}
		float volume = default(float);
		string iD;
		if (b == 2)
		{
			if (value != null)
			{
				iD = value.ID;
				volume = value.Volume;
				iD = ((MemoryPackReader)(ref reader)).ReadString();
				((MemoryPackReader)(ref reader)).ReadUnmanaged<float>(ref volume);
				goto IL_009d;
			}
			iD = ((MemoryPackReader)(ref reader)).ReadString();
			((MemoryPackReader)(ref reader)).ReadUnmanaged<float>(ref volume);
		}
		else
		{
			if (b > 2)
			{
				MemoryPackSerializationException.ThrowInvalidPropertyCount(typeof(AudioConfig), (byte)2, b);
				return;
			}
			if (value == null)
			{
				iD = null;
				volume = 0f;
			}
			else
			{
				iD = value.ID;
				volume = value.Volume;
			}
			if (b != 0)
			{
				iD = ((MemoryPackReader)(ref reader)).ReadString();
				if (b != 1)
				{
					((MemoryPackReader)(ref reader)).ReadUnmanaged<float>(ref volume);
					_ = 2;
				}
			}
			if (value != null)
			{
				goto IL_009d;
			}
		}
		value = new AudioConfig
		{
			ID = iD,
			Volume = volume
		};
		return;
		IL_009d:
		value.ID = iD;
		value.Volume = volume;
	}
}
