using MemoryPack;
using MemoryPack.Formatters;
using MemoryPack.Internal;

namespace Example.Cfg;

[MemoryPackable(/*Could not decode attribute arguments.*/)]
public class RoleNameConfig : IMemoryPackable<RoleNameConfig>, IMemoryPackFormatterRegister
{
	[Preserve]
	private sealed class RoleNameConfigFormatter : MemoryPackFormatter<RoleNameConfig>
	{
		[Preserve]
		public override void Serialize(ref MemoryPackWriter writer, ref RoleNameConfig value)
		{
			RoleNameConfig.Serialize(ref writer, ref value);
		}

		[Preserve]
		public override void Deserialize(ref MemoryPackReader reader, ref RoleNameConfig value)
		{
			RoleNameConfig.Deserialize(ref reader, ref value);
		}
	}

	[MemoryPackOrder(0)]
	public int ID { get; private set; }

	[MemoryPackOrder(1)]
	public string[] Country { get; private set; }

	[MemoryPackOrder(2)]
	public string RandomName { get; private set; }

	static RoleNameConfig()
	{
		RegisterFormatter();
	}

	[Preserve]
	public static void RegisterFormatter()
	{
		if (!MemoryPackFormatterProvider.IsRegistered<RoleNameConfig>())
		{
			MemoryPackFormatterProvider.Register<RoleNameConfig>((MemoryPackFormatter<RoleNameConfig>)(object)new RoleNameConfigFormatter());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<RoleNameConfig[]>())
		{
			MemoryPackFormatterProvider.Register<RoleNameConfig[]>((MemoryPackFormatter<RoleNameConfig[]>)(object)new ArrayFormatter<RoleNameConfig>());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<string[]>())
		{
			MemoryPackFormatterProvider.Register<string[]>((MemoryPackFormatter<string[]>)(object)new ArrayFormatter<string>());
		}
	}

	[Preserve]
	public static void Serialize(ref MemoryPackWriter writer, ref RoleNameConfig? value)
	{
		if (value == null)
		{
			((MemoryPackWriter)(ref writer)).WriteNullObjectHeader();
			return;
		}
		int iD = value.ID;
		((MemoryPackWriter)(ref writer)).WriteUnmanagedWithObjectHeader<int>((byte)3, ref iD);
		((MemoryPackWriter)(ref writer)).WriteArray<string>(value.Country);
		((MemoryPackWriter)(ref writer)).WriteString(value.RandomName);
	}

	[Preserve]
	public static void Deserialize(ref MemoryPackReader reader, ref RoleNameConfig? value)
	{
		byte b = default(byte);
		if (!((MemoryPackReader)(ref reader)).TryReadObjectHeader(ref b))
		{
			value = null;
			return;
		}
		int iD = default(int);
		string[] country;
		string randomName;
		if (b == 3)
		{
			if (value != null)
			{
				iD = value.ID;
				country = value.Country;
				randomName = value.RandomName;
				((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref iD);
				((MemoryPackReader)(ref reader)).ReadArray<string>(ref country);
				randomName = ((MemoryPackReader)(ref reader)).ReadString();
				goto IL_00c6;
			}
			((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref iD);
			country = ((MemoryPackReader)(ref reader)).ReadArray<string>();
			randomName = ((MemoryPackReader)(ref reader)).ReadString();
		}
		else
		{
			if (b > 3)
			{
				MemoryPackSerializationException.ThrowInvalidPropertyCount(typeof(RoleNameConfig), (byte)3, b);
				return;
			}
			if (value == null)
			{
				iD = 0;
				country = null;
				randomName = null;
			}
			else
			{
				iD = value.ID;
				country = value.Country;
				randomName = value.RandomName;
			}
			if (b != 0)
			{
				((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref iD);
				if (b != 1)
				{
					((MemoryPackReader)(ref reader)).ReadArray<string>(ref country);
					if (b != 2)
					{
						randomName = ((MemoryPackReader)(ref reader)).ReadString();
						_ = 3;
					}
				}
			}
			if (value != null)
			{
				goto IL_00c6;
			}
		}
		value = new RoleNameConfig
		{
			ID = iD,
			Country = country,
			RandomName = randomName
		};
		return;
		IL_00c6:
		value.ID = iD;
		value.Country = country;
		value.RandomName = randomName;
	}
}
