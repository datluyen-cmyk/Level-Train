using System.Collections.Generic;
using MemoryPack;
using MemoryPack.Formatters;
using MemoryPack.Internal;

namespace Example.Cfg;

[MemoryPackable(/*Could not decode attribute arguments.*/)]
public class TextConfig : IMemoryPackable<TextConfig>, IMemoryPackFormatterRegister
{
	[Preserve]
	private sealed class TextConfigFormatter : MemoryPackFormatter<TextConfig>
	{
		[Preserve]
		public override void Serialize(ref MemoryPackWriter writer, ref TextConfig value)
		{
			TextConfig.Serialize(ref writer, ref value);
		}

		[Preserve]
		public override void Deserialize(ref MemoryPackReader reader, ref TextConfig value)
		{
			TextConfig.Deserialize(ref reader, ref value);
		}
	}

	[MemoryPackOrder(0)]
	public string Key { get; private set; }

	[MemoryPackOrder(1)]
	public Dictionary<string, string> Text { get; private set; }

	static TextConfig()
	{
		RegisterFormatter();
	}

	[Preserve]
	public static void RegisterFormatter()
	{
		if (!MemoryPackFormatterProvider.IsRegistered<TextConfig>())
		{
			MemoryPackFormatterProvider.Register<TextConfig>((MemoryPackFormatter<TextConfig>)(object)new TextConfigFormatter());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<TextConfig[]>())
		{
			MemoryPackFormatterProvider.Register<TextConfig[]>((MemoryPackFormatter<TextConfig[]>)(object)new ArrayFormatter<TextConfig>());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<Dictionary<string, string>>())
		{
			MemoryPackFormatterProvider.Register<Dictionary<string, string>>((MemoryPackFormatter<Dictionary<string, string>>)(object)new DictionaryFormatter<string, string>());
		}
	}

	[Preserve]
	public static void Serialize(ref MemoryPackWriter writer, ref TextConfig? value)
	{
		if (value == null)
		{
			((MemoryPackWriter)(ref writer)).WriteNullObjectHeader();
			return;
		}
		((MemoryPackWriter)(ref writer)).WriteObjectHeader((byte)2);
		((MemoryPackWriter)(ref writer)).WriteString(value.Key);
		Dictionary<string, string> text = value.Text;
		((MemoryPackWriter)(ref writer)).WriteValue<Dictionary<string, string>>(ref text);
	}

	[Preserve]
	public static void Deserialize(ref MemoryPackReader reader, ref TextConfig? value)
	{
		byte b = default(byte);
		if (!((MemoryPackReader)(ref reader)).TryReadObjectHeader(ref b))
		{
			value = null;
			return;
		}
		Dictionary<string, string> text;
		string key;
		if (b == 2)
		{
			if (value != null)
			{
				key = value.Key;
				text = value.Text;
				key = ((MemoryPackReader)(ref reader)).ReadString();
				((MemoryPackReader)(ref reader)).ReadValue<Dictionary<string, string>>(ref text);
				goto IL_0098;
			}
			key = ((MemoryPackReader)(ref reader)).ReadString();
			text = ((MemoryPackReader)(ref reader)).ReadValue<Dictionary<string, string>>();
		}
		else
		{
			if (b > 2)
			{
				MemoryPackSerializationException.ThrowInvalidPropertyCount(typeof(TextConfig), (byte)2, b);
				return;
			}
			if (value == null)
			{
				key = null;
				text = null;
			}
			else
			{
				key = value.Key;
				text = value.Text;
			}
			if (b != 0)
			{
				key = ((MemoryPackReader)(ref reader)).ReadString();
				if (b != 1)
				{
					((MemoryPackReader)(ref reader)).ReadValue<Dictionary<string, string>>(ref text);
					_ = 2;
				}
			}
			if (value != null)
			{
				goto IL_0098;
			}
		}
		value = new TextConfig
		{
			Key = key,
			Text = text
		};
		return;
		IL_0098:
		value.Key = key;
		value.Text = text;
	}
}
