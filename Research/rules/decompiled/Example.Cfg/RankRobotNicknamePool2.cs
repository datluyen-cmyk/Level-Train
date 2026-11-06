using MemoryPack;
using MemoryPack.Formatters;
using MemoryPack.Internal;

namespace Example.Cfg;

[MemoryPackable(/*Could not decode attribute arguments.*/)]
public class RankRobotNicknamePool2 : IMemoryPackable<RankRobotNicknamePool2>, IMemoryPackFormatterRegister
{
	[Preserve]
	private sealed class RankRobotNicknamePool2Formatter : MemoryPackFormatter<RankRobotNicknamePool2>
	{
		[Preserve]
		public override void Serialize(ref MemoryPackWriter writer, ref RankRobotNicknamePool2 value)
		{
			RankRobotNicknamePool2.Serialize(ref writer, ref value);
		}

		[Preserve]
		public override void Deserialize(ref MemoryPackReader reader, ref RankRobotNicknamePool2 value)
		{
			RankRobotNicknamePool2.Deserialize(ref reader, ref value);
		}
	}

	[MemoryPackOrder(0)]
	public int ID { get; private set; }

	[MemoryPackOrder(1)]
	public string Name { get; private set; }

	static RankRobotNicknamePool2()
	{
		RegisterFormatter();
	}

	[Preserve]
	public static void RegisterFormatter()
	{
		if (!MemoryPackFormatterProvider.IsRegistered<RankRobotNicknamePool2>())
		{
			MemoryPackFormatterProvider.Register<RankRobotNicknamePool2>((MemoryPackFormatter<RankRobotNicknamePool2>)(object)new RankRobotNicknamePool2Formatter());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<RankRobotNicknamePool2[]>())
		{
			MemoryPackFormatterProvider.Register<RankRobotNicknamePool2[]>((MemoryPackFormatter<RankRobotNicknamePool2[]>)(object)new ArrayFormatter<RankRobotNicknamePool2>());
		}
	}

	[Preserve]
	public static void Serialize(ref MemoryPackWriter writer, ref RankRobotNicknamePool2? value)
	{
		if (value == null)
		{
			((MemoryPackWriter)(ref writer)).WriteNullObjectHeader();
			return;
		}
		int iD = value.ID;
		((MemoryPackWriter)(ref writer)).WriteUnmanagedWithObjectHeader<int>((byte)2, ref iD);
		((MemoryPackWriter)(ref writer)).WriteString(value.Name);
	}

	[Preserve]
	public static void Deserialize(ref MemoryPackReader reader, ref RankRobotNicknamePool2? value)
	{
		byte b = default(byte);
		if (!((MemoryPackReader)(ref reader)).TryReadObjectHeader(ref b))
		{
			value = null;
			return;
		}
		int iD = default(int);
		string name;
		if (b == 2)
		{
			if (value != null)
			{
				iD = value.ID;
				name = value.Name;
				((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref iD);
				name = ((MemoryPackReader)(ref reader)).ReadString();
				goto IL_0099;
			}
			((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref iD);
			name = ((MemoryPackReader)(ref reader)).ReadString();
		}
		else
		{
			if (b > 2)
			{
				MemoryPackSerializationException.ThrowInvalidPropertyCount(typeof(RankRobotNicknamePool2), (byte)2, b);
				return;
			}
			if (value == null)
			{
				iD = 0;
				name = null;
			}
			else
			{
				iD = value.ID;
				name = value.Name;
			}
			if (b != 0)
			{
				((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref iD);
				if (b != 1)
				{
					name = ((MemoryPackReader)(ref reader)).ReadString();
					_ = 2;
				}
			}
			if (value != null)
			{
				goto IL_0099;
			}
		}
		value = new RankRobotNicknamePool2
		{
			ID = iD,
			Name = name
		};
		return;
		IL_0099:
		value.ID = iD;
		value.Name = name;
	}
}
