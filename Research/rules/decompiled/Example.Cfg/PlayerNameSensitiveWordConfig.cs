using MemoryPack;
using MemoryPack.Formatters;
using MemoryPack.Internal;

namespace Example.Cfg;

[MemoryPackable(/*Could not decode attribute arguments.*/)]
public class PlayerNameSensitiveWordConfig : IMemoryPackable<PlayerNameSensitiveWordConfig>, IMemoryPackFormatterRegister
{
	[Preserve]
	private sealed class PlayerNameSensitiveWordConfigFormatter : MemoryPackFormatter<PlayerNameSensitiveWordConfig>
	{
		[Preserve]
		public override void Serialize(ref MemoryPackWriter writer, ref PlayerNameSensitiveWordConfig value)
		{
			PlayerNameSensitiveWordConfig.Serialize(ref writer, ref value);
		}

		[Preserve]
		public override void Deserialize(ref MemoryPackReader reader, ref PlayerNameSensitiveWordConfig value)
		{
			PlayerNameSensitiveWordConfig.Deserialize(ref reader, ref value);
		}
	}

	[MemoryPackOrder(0)]
	public int ID { get; private set; }

	[MemoryPackOrder(1)]
	public string Word { get; private set; }

	static PlayerNameSensitiveWordConfig()
	{
		RegisterFormatter();
	}

	[Preserve]
	public static void RegisterFormatter()
	{
		if (!MemoryPackFormatterProvider.IsRegistered<PlayerNameSensitiveWordConfig>())
		{
			MemoryPackFormatterProvider.Register<PlayerNameSensitiveWordConfig>((MemoryPackFormatter<PlayerNameSensitiveWordConfig>)(object)new PlayerNameSensitiveWordConfigFormatter());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<PlayerNameSensitiveWordConfig[]>())
		{
			MemoryPackFormatterProvider.Register<PlayerNameSensitiveWordConfig[]>((MemoryPackFormatter<PlayerNameSensitiveWordConfig[]>)(object)new ArrayFormatter<PlayerNameSensitiveWordConfig>());
		}
	}

	[Preserve]
	public static void Serialize(ref MemoryPackWriter writer, ref PlayerNameSensitiveWordConfig? value)
	{
		if (value == null)
		{
			((MemoryPackWriter)(ref writer)).WriteNullObjectHeader();
			return;
		}
		int iD = value.ID;
		((MemoryPackWriter)(ref writer)).WriteUnmanagedWithObjectHeader<int>((byte)2, ref iD);
		((MemoryPackWriter)(ref writer)).WriteString(value.Word);
	}

	[Preserve]
	public static void Deserialize(ref MemoryPackReader reader, ref PlayerNameSensitiveWordConfig? value)
	{
		byte b = default(byte);
		if (!((MemoryPackReader)(ref reader)).TryReadObjectHeader(ref b))
		{
			value = null;
			return;
		}
		int iD = default(int);
		string word;
		if (b == 2)
		{
			if (value != null)
			{
				iD = value.ID;
				word = value.Word;
				((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref iD);
				word = ((MemoryPackReader)(ref reader)).ReadString();
				goto IL_0099;
			}
			((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref iD);
			word = ((MemoryPackReader)(ref reader)).ReadString();
		}
		else
		{
			if (b > 2)
			{
				MemoryPackSerializationException.ThrowInvalidPropertyCount(typeof(PlayerNameSensitiveWordConfig), (byte)2, b);
				return;
			}
			if (value == null)
			{
				iD = 0;
				word = null;
			}
			else
			{
				iD = value.ID;
				word = value.Word;
			}
			if (b != 0)
			{
				((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref iD);
				if (b != 1)
				{
					word = ((MemoryPackReader)(ref reader)).ReadString();
					_ = 2;
				}
			}
			if (value != null)
			{
				goto IL_0099;
			}
		}
		value = new PlayerNameSensitiveWordConfig
		{
			ID = iD,
			Word = word
		};
		return;
		IL_0099:
		value.ID = iD;
		value.Word = word;
	}
}
