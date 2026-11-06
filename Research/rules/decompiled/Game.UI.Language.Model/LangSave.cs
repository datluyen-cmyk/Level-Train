using MemoryPack;
using MemoryPack.Formatters;
using MemoryPack.Internal;

namespace Game.UI.Language.Model;

[MemoryPackable(/*Could not decode attribute arguments.*/)]
public class LangSave : IMemoryPackable<LangSave>, IMemoryPackFormatterRegister
{
	[Preserve]
	private sealed class LangSaveFormatter : MemoryPackFormatter<LangSave>
	{
		[Preserve]
		public override void Serialize(ref MemoryPackWriter writer, ref LangSave value)
		{
			LangSave.Serialize(ref writer, ref value);
		}

		[Preserve]
		public override void Deserialize(ref MemoryPackReader reader, ref LangSave value)
		{
			LangSave.Deserialize(ref reader, ref value);
		}
	}

	public string language;

	static LangSave()
	{
		RegisterFormatter();
	}

	[Preserve]
	public static void RegisterFormatter()
	{
		if (!MemoryPackFormatterProvider.IsRegistered<LangSave>())
		{
			MemoryPackFormatterProvider.Register<LangSave>((MemoryPackFormatter<LangSave>)(object)new LangSaveFormatter());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<LangSave[]>())
		{
			MemoryPackFormatterProvider.Register<LangSave[]>((MemoryPackFormatter<LangSave[]>)(object)new ArrayFormatter<LangSave>());
		}
	}

	[Preserve]
	public static void Serialize(ref MemoryPackWriter writer, ref LangSave? value)
	{
		if (value == null)
		{
			((MemoryPackWriter)(ref writer)).WriteNullObjectHeader();
			return;
		}
		((MemoryPackWriter)(ref writer)).WriteObjectHeader((byte)1);
		((MemoryPackWriter)(ref writer)).WriteString(value.language);
	}

	[Preserve]
	public static void Deserialize(ref MemoryPackReader reader, ref LangSave? value)
	{
		byte b = default(byte);
		if (!((MemoryPackReader)(ref reader)).TryReadObjectHeader(ref b))
		{
			value = null;
			return;
		}
		string text;
		if (b == 1)
		{
			if (value != null)
			{
				text = value.language;
				text = ((MemoryPackReader)(ref reader)).ReadString();
				goto IL_0068;
			}
			text = ((MemoryPackReader)(ref reader)).ReadString();
		}
		else
		{
			if (b > 1)
			{
				MemoryPackSerializationException.ThrowInvalidPropertyCount(typeof(LangSave), (byte)1, b);
				return;
			}
			text = ((value != null) ? value.language : null);
			if (b != 0)
			{
				text = ((MemoryPackReader)(ref reader)).ReadString();
				_ = 1;
			}
			if (value != null)
			{
				goto IL_0068;
			}
		}
		value = new LangSave
		{
			language = text
		};
		return;
		IL_0068:
		value.language = text;
	}
}
