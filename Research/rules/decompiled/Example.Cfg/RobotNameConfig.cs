using MemoryPack;
using MemoryPack.Formatters;
using MemoryPack.Internal;

namespace Example.Cfg;

[MemoryPackable(/*Could not decode attribute arguments.*/)]
public class RobotNameConfig : IMemoryPackable<RobotNameConfig>, IMemoryPackFormatterRegister
{
	[Preserve]
	private sealed class RobotNameConfigFormatter : MemoryPackFormatter<RobotNameConfig>
	{
		[Preserve]
		public override void Serialize(ref MemoryPackWriter writer, ref RobotNameConfig value)
		{
			RobotNameConfig.Serialize(ref writer, ref value);
		}

		[Preserve]
		public override void Deserialize(ref MemoryPackReader reader, ref RobotNameConfig value)
		{
			RobotNameConfig.Deserialize(ref reader, ref value);
		}
	}

	[MemoryPackOrder(0)]
	public int ID { get; private set; }

	[MemoryPackOrder(1)]
	public string[] Country { get; private set; }

	[MemoryPackOrder(2)]
	public string Rolename { get; private set; }

	[MemoryPackOrder(3)]
	public string Gender { get; private set; }

	[MemoryPackOrder(4)]
	public string AvatarId { get; private set; }

	[MemoryPackOrder(5)]
	public string Happy { get; private set; }

	static RobotNameConfig()
	{
		RegisterFormatter();
	}

	[Preserve]
	public static void RegisterFormatter()
	{
		if (!MemoryPackFormatterProvider.IsRegistered<RobotNameConfig>())
		{
			MemoryPackFormatterProvider.Register<RobotNameConfig>((MemoryPackFormatter<RobotNameConfig>)(object)new RobotNameConfigFormatter());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<RobotNameConfig[]>())
		{
			MemoryPackFormatterProvider.Register<RobotNameConfig[]>((MemoryPackFormatter<RobotNameConfig[]>)(object)new ArrayFormatter<RobotNameConfig>());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<string[]>())
		{
			MemoryPackFormatterProvider.Register<string[]>((MemoryPackFormatter<string[]>)(object)new ArrayFormatter<string>());
		}
	}

	[Preserve]
	public static void Serialize(ref MemoryPackWriter writer, ref RobotNameConfig? value)
	{
		if (value == null)
		{
			((MemoryPackWriter)(ref writer)).WriteNullObjectHeader();
			return;
		}
		int iD = value.ID;
		((MemoryPackWriter)(ref writer)).WriteUnmanagedWithObjectHeader<int>((byte)6, ref iD);
		((MemoryPackWriter)(ref writer)).WriteArray<string>(value.Country);
		((MemoryPackWriter)(ref writer)).WriteString(value.Rolename);
		((MemoryPackWriter)(ref writer)).WriteString(value.Gender);
		((MemoryPackWriter)(ref writer)).WriteString(value.AvatarId);
		((MemoryPackWriter)(ref writer)).WriteString(value.Happy);
	}

	[Preserve]
	public static void Deserialize(ref MemoryPackReader reader, ref RobotNameConfig? value)
	{
		byte b = default(byte);
		if (!((MemoryPackReader)(ref reader)).TryReadObjectHeader(ref b))
		{
			value = null;
			return;
		}
		int iD = default(int);
		string[] country;
		string rolename;
		string gender;
		string avatarId;
		string happy;
		if (b == 6)
		{
			if (value != null)
			{
				iD = value.ID;
				country = value.Country;
				rolename = value.Rolename;
				gender = value.Gender;
				avatarId = value.AvatarId;
				happy = value.Happy;
				((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref iD);
				((MemoryPackReader)(ref reader)).ReadArray<string>(ref country);
				rolename = ((MemoryPackReader)(ref reader)).ReadString();
				gender = ((MemoryPackReader)(ref reader)).ReadString();
				avatarId = ((MemoryPackReader)(ref reader)).ReadString();
				happy = ((MemoryPackReader)(ref reader)).ReadString();
				goto IL_015f;
			}
			((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref iD);
			country = ((MemoryPackReader)(ref reader)).ReadArray<string>();
			rolename = ((MemoryPackReader)(ref reader)).ReadString();
			gender = ((MemoryPackReader)(ref reader)).ReadString();
			avatarId = ((MemoryPackReader)(ref reader)).ReadString();
			happy = ((MemoryPackReader)(ref reader)).ReadString();
		}
		else
		{
			if (b > 6)
			{
				MemoryPackSerializationException.ThrowInvalidPropertyCount(typeof(RobotNameConfig), (byte)6, b);
				return;
			}
			if (value == null)
			{
				iD = 0;
				country = null;
				rolename = null;
				gender = null;
				avatarId = null;
				happy = null;
			}
			else
			{
				iD = value.ID;
				country = value.Country;
				rolename = value.Rolename;
				gender = value.Gender;
				avatarId = value.AvatarId;
				happy = value.Happy;
			}
			if (b != 0)
			{
				((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref iD);
				if (b != 1)
				{
					((MemoryPackReader)(ref reader)).ReadArray<string>(ref country);
					if (b != 2)
					{
						rolename = ((MemoryPackReader)(ref reader)).ReadString();
						if (b != 3)
						{
							gender = ((MemoryPackReader)(ref reader)).ReadString();
							if (b != 4)
							{
								avatarId = ((MemoryPackReader)(ref reader)).ReadString();
								if (b != 5)
								{
									happy = ((MemoryPackReader)(ref reader)).ReadString();
									_ = 6;
								}
							}
						}
					}
				}
			}
			if (value != null)
			{
				goto IL_015f;
			}
		}
		value = new RobotNameConfig
		{
			ID = iD,
			Country = country,
			Rolename = rolename,
			Gender = gender,
			AvatarId = avatarId,
			Happy = happy
		};
		return;
		IL_015f:
		value.ID = iD;
		value.Country = country;
		value.Rolename = rolename;
		value.Gender = gender;
		value.AvatarId = avatarId;
		value.Happy = happy;
	}
}
