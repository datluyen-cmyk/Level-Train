using MemoryPack;
using MemoryPack.Formatters;
using MemoryPack.Internal;

namespace Example.Cfg;

[MemoryPackable(/*Could not decode attribute arguments.*/)]
public class RankRobotNicknamePool1 : IMemoryPackable<RankRobotNicknamePool1>, IMemoryPackFormatterRegister
{
	[Preserve]
	private sealed class RankRobotNicknamePool1Formatter : MemoryPackFormatter<RankRobotNicknamePool1>
	{
		[Preserve]
		public override void Serialize(ref MemoryPackWriter writer, ref RankRobotNicknamePool1 value)
		{
			RankRobotNicknamePool1.Serialize(ref writer, ref value);
		}

		[Preserve]
		public override void Deserialize(ref MemoryPackReader reader, ref RankRobotNicknamePool1 value)
		{
			RankRobotNicknamePool1.Deserialize(ref reader, ref value);
		}
	}

	[MemoryPackOrder(0)]
	public int ID { get; private set; }

	[MemoryPackOrder(1)]
	public string Name { get; private set; }

	static RankRobotNicknamePool1()
	{
		RegisterFormatter();
	}

	[Preserve]
	public static void RegisterFormatter()
	{
		if (!MemoryPackFormatterProvider.IsRegistered<RankRobotNicknamePool1>())
		{
			MemoryPackFormatterProvider.Register<RankRobotNicknamePool1>((MemoryPackFormatter<RankRobotNicknamePool1>)(object)new RankRobotNicknamePool1Formatter());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<RankRobotNicknamePool1[]>())
		{
			MemoryPackFormatterProvider.Register<RankRobotNicknamePool1[]>((MemoryPackFormatter<RankRobotNicknamePool1[]>)(object)new ArrayFormatter<RankRobotNicknamePool1>());
		}
	}

	[Preserve]
	public static void Serialize(ref MemoryPackWriter writer, ref RankRobotNicknamePool1? value)
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
	public static void Deserialize(ref MemoryPackReader reader, ref RankRobotNicknamePool1? value)
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
				MemoryPackSerializationException.ThrowInvalidPropertyCount(typeof(RankRobotNicknamePool1), (byte)2, b);
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
		value = new RankRobotNicknamePool1
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
