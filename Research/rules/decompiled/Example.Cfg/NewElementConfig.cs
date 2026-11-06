using MemoryPack;
using MemoryPack.Formatters;
using MemoryPack.Internal;

namespace Example.Cfg;

[MemoryPackable(/*Could not decode attribute arguments.*/)]
public class NewElementConfig : IMemoryPackable<NewElementConfig>, IMemoryPackFormatterRegister
{
	[Preserve]
	private sealed class NewElementConfigFormatter : MemoryPackFormatter<NewElementConfig>
	{
		[Preserve]
		public override void Serialize(ref MemoryPackWriter writer, ref NewElementConfig value)
		{
			NewElementConfig.Serialize(ref writer, ref value);
		}

		[Preserve]
		public override void Deserialize(ref MemoryPackReader reader, ref NewElementConfig value)
		{
			NewElementConfig.Deserialize(ref reader, ref value);
		}
	}

	[MemoryPackOrder(0)]
	public int id { get; private set; }

	[MemoryPackOrder(1)]
	public int UnlockLv { get; private set; }

	[MemoryPackOrder(2)]
	public string Name { get; private set; }

	[MemoryPackOrder(3)]
	public string Desc { get; private set; }

	[MemoryPackOrder(4)]
	public string Icon { get; private set; }

	static NewElementConfig()
	{
		RegisterFormatter();
	}

	[Preserve]
	public static void RegisterFormatter()
	{
		if (!MemoryPackFormatterProvider.IsRegistered<NewElementConfig>())
		{
			MemoryPackFormatterProvider.Register<NewElementConfig>((MemoryPackFormatter<NewElementConfig>)(object)new NewElementConfigFormatter());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<NewElementConfig[]>())
		{
			MemoryPackFormatterProvider.Register<NewElementConfig[]>((MemoryPackFormatter<NewElementConfig[]>)(object)new ArrayFormatter<NewElementConfig>());
		}
	}

	[Preserve]
	public static void Serialize(ref MemoryPackWriter writer, ref NewElementConfig? value)
	{
		if (value == null)
		{
			((MemoryPackWriter)(ref writer)).WriteNullObjectHeader();
			return;
		}
		int num = value.id;
		int unlockLv = value.UnlockLv;
		((MemoryPackWriter)(ref writer)).WriteUnmanagedWithObjectHeader<int, int>((byte)5, ref num, ref unlockLv);
		((MemoryPackWriter)(ref writer)).WriteString(value.Name);
		((MemoryPackWriter)(ref writer)).WriteString(value.Desc);
		((MemoryPackWriter)(ref writer)).WriteString(value.Icon);
	}

	[Preserve]
	public static void Deserialize(ref MemoryPackReader reader, ref NewElementConfig? value)
	{
		byte b = default(byte);
		if (!((MemoryPackReader)(ref reader)).TryReadObjectHeader(ref b))
		{
			value = null;
			return;
		}
		int num = default(int);
		int unlockLv = default(int);
		string name;
		string desc;
		string icon;
		if (b == 5)
		{
			if (value != null)
			{
				num = value.id;
				unlockLv = value.UnlockLv;
				name = value.Name;
				desc = value.Desc;
				icon = value.Icon;
				((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref num);
				((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref unlockLv);
				name = ((MemoryPackReader)(ref reader)).ReadString();
				desc = ((MemoryPackReader)(ref reader)).ReadString();
				icon = ((MemoryPackReader)(ref reader)).ReadString();
				goto IL_0129;
			}
			((MemoryPackReader)(ref reader)).ReadUnmanaged<int, int>(ref num, ref unlockLv);
			name = ((MemoryPackReader)(ref reader)).ReadString();
			desc = ((MemoryPackReader)(ref reader)).ReadString();
			icon = ((MemoryPackReader)(ref reader)).ReadString();
		}
		else
		{
			if (b > 5)
			{
				MemoryPackSerializationException.ThrowInvalidPropertyCount(typeof(NewElementConfig), (byte)5, b);
				return;
			}
			if (value == null)
			{
				num = 0;
				unlockLv = 0;
				name = null;
				desc = null;
				icon = null;
			}
			else
			{
				num = value.id;
				unlockLv = value.UnlockLv;
				name = value.Name;
				desc = value.Desc;
				icon = value.Icon;
			}
			if (b != 0)
			{
				((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref num);
				if (b != 1)
				{
					((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref unlockLv);
					if (b != 2)
					{
						name = ((MemoryPackReader)(ref reader)).ReadString();
						if (b != 3)
						{
							desc = ((MemoryPackReader)(ref reader)).ReadString();
							if (b != 4)
							{
								icon = ((MemoryPackReader)(ref reader)).ReadString();
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
		value = new NewElementConfig
		{
			id = num,
			UnlockLv = unlockLv,
			Name = name,
			Desc = desc,
			Icon = icon
		};
		return;
		IL_0129:
		value.id = num;
		value.UnlockLv = unlockLv;
		value.Name = name;
		value.Desc = desc;
		value.Icon = icon;
	}
}
