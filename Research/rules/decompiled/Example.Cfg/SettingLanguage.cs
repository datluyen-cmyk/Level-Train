using MemoryPack;
using MemoryPack.Formatters;
using MemoryPack.Internal;

namespace Example.Cfg;

[MemoryPackable(/*Could not decode attribute arguments.*/)]
public class SettingLanguage : IMemoryPackable<SettingLanguage>, IMemoryPackFormatterRegister
{
	[Preserve]
	private sealed class SettingLanguageFormatter : MemoryPackFormatter<SettingLanguage>
	{
		[Preserve]
		public override void Serialize(ref MemoryPackWriter writer, ref SettingLanguage value)
		{
			SettingLanguage.Serialize(ref writer, ref value);
		}

		[Preserve]
		public override void Deserialize(ref MemoryPackReader reader, ref SettingLanguage value)
		{
			SettingLanguage.Deserialize(ref reader, ref value);
		}
	}

	[MemoryPackOrder(0)]
	public string Key { get; private set; }

	[MemoryPackOrder(1)]
	public string TitleId { get; private set; }

	[MemoryPackOrder(2)]
	public string[] AllLanguageCode { get; private set; }

	[MemoryPackOrder(3)]
	public int SortId { get; private set; }

	[MemoryPackOrder(4)]
	public int IsShow { get; private set; }

	static SettingLanguage()
	{
		RegisterFormatter();
	}

	[Preserve]
	public static void RegisterFormatter()
	{
		if (!MemoryPackFormatterProvider.IsRegistered<SettingLanguage>())
		{
			MemoryPackFormatterProvider.Register<SettingLanguage>((MemoryPackFormatter<SettingLanguage>)(object)new SettingLanguageFormatter());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<SettingLanguage[]>())
		{
			MemoryPackFormatterProvider.Register<SettingLanguage[]>((MemoryPackFormatter<SettingLanguage[]>)(object)new ArrayFormatter<SettingLanguage>());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<string[]>())
		{
			MemoryPackFormatterProvider.Register<string[]>((MemoryPackFormatter<string[]>)(object)new ArrayFormatter<string>());
		}
	}

	[Preserve]
	public static void Serialize(ref MemoryPackWriter writer, ref SettingLanguage? value)
	{
		if (value == null)
		{
			((MemoryPackWriter)(ref writer)).WriteNullObjectHeader();
			return;
		}
		((MemoryPackWriter)(ref writer)).WriteObjectHeader((byte)5);
		((MemoryPackWriter)(ref writer)).WriteString(value.Key);
		((MemoryPackWriter)(ref writer)).WriteString(value.TitleId);
		((MemoryPackWriter)(ref writer)).WriteArray<string>(value.AllLanguageCode);
		int sortId = value.SortId;
		int isShow = value.IsShow;
		((MemoryPackWriter)(ref writer)).WriteUnmanaged<int, int>(ref sortId, ref isShow);
	}

	[Preserve]
	public static void Deserialize(ref MemoryPackReader reader, ref SettingLanguage? value)
	{
		byte b = default(byte);
		if (!((MemoryPackReader)(ref reader)).TryReadObjectHeader(ref b))
		{
			value = null;
			return;
		}
		string[] allLanguageCode;
		int sortId = default(int);
		int isShow = default(int);
		string key;
		string titleId;
		if (b == 5)
		{
			if (value != null)
			{
				key = value.Key;
				titleId = value.TitleId;
				allLanguageCode = value.AllLanguageCode;
				sortId = value.SortId;
				isShow = value.IsShow;
				key = ((MemoryPackReader)(ref reader)).ReadString();
				titleId = ((MemoryPackReader)(ref reader)).ReadString();
				((MemoryPackReader)(ref reader)).ReadArray<string>(ref allLanguageCode);
				((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref sortId);
				((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref isShow);
				goto IL_0122;
			}
			key = ((MemoryPackReader)(ref reader)).ReadString();
			titleId = ((MemoryPackReader)(ref reader)).ReadString();
			allLanguageCode = ((MemoryPackReader)(ref reader)).ReadArray<string>();
			((MemoryPackReader)(ref reader)).ReadUnmanaged<int, int>(ref sortId, ref isShow);
		}
		else
		{
			if (b > 5)
			{
				MemoryPackSerializationException.ThrowInvalidPropertyCount(typeof(SettingLanguage), (byte)5, b);
				return;
			}
			if (value == null)
			{
				key = null;
				titleId = null;
				allLanguageCode = null;
				sortId = 0;
				isShow = 0;
			}
			else
			{
				key = value.Key;
				titleId = value.TitleId;
				allLanguageCode = value.AllLanguageCode;
				sortId = value.SortId;
				isShow = value.IsShow;
			}
			if (b != 0)
			{
				key = ((MemoryPackReader)(ref reader)).ReadString();
				if (b != 1)
				{
					titleId = ((MemoryPackReader)(ref reader)).ReadString();
					if (b != 2)
					{
						((MemoryPackReader)(ref reader)).ReadArray<string>(ref allLanguageCode);
						if (b != 3)
						{
							((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref sortId);
							if (b != 4)
							{
								((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref isShow);
								_ = 5;
							}
						}
					}
				}
			}
			if (value != null)
			{
				goto IL_0122;
			}
		}
		value = new SettingLanguage
		{
			Key = key,
			TitleId = titleId,
			AllLanguageCode = allLanguageCode,
			SortId = sortId,
			IsShow = isShow
		};
		return;
		IL_0122:
		value.Key = key;
		value.TitleId = titleId;
		value.AllLanguageCode = allLanguageCode;
		value.SortId = sortId;
		value.IsShow = isShow;
	}
}
