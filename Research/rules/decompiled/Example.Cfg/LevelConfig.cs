using MemoryPack;
using MemoryPack.Formatters;
using MemoryPack.Internal;

namespace Example.Cfg;

[MemoryPackable(/*Could not decode attribute arguments.*/)]
public class LevelConfig : IMemoryPackable<LevelConfig>, IMemoryPackFormatterRegister
{
	[Preserve]
	private sealed class LevelConfigFormatter : MemoryPackFormatter<LevelConfig>
	{
		[Preserve]
		public override void Serialize(ref MemoryPackWriter writer, ref LevelConfig value)
		{
			LevelConfig.Serialize(ref writer, ref value);
		}

		[Preserve]
		public override void Deserialize(ref MemoryPackReader reader, ref LevelConfig value)
		{
			LevelConfig.Deserialize(ref reader, ref value);
		}
	}

	[MemoryPackOrder(0)]
	public int LevelID { get; private set; }

	[MemoryPackOrder(1)]
	public string PhysicsLevelPath { get; private set; }

	[MemoryPackOrder(2)]
	public string CoreLevelPath { get; private set; }

	static LevelConfig()
	{
		RegisterFormatter();
	}

	[Preserve]
	public static void RegisterFormatter()
	{
		if (!MemoryPackFormatterProvider.IsRegistered<LevelConfig>())
		{
			MemoryPackFormatterProvider.Register<LevelConfig>((MemoryPackFormatter<LevelConfig>)(object)new LevelConfigFormatter());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<LevelConfig[]>())
		{
			MemoryPackFormatterProvider.Register<LevelConfig[]>((MemoryPackFormatter<LevelConfig[]>)(object)new ArrayFormatter<LevelConfig>());
		}
	}

	[Preserve]
	public static void Serialize(ref MemoryPackWriter writer, ref LevelConfig? value)
	{
		if (value == null)
		{
			((MemoryPackWriter)(ref writer)).WriteNullObjectHeader();
			return;
		}
		int levelID = value.LevelID;
		((MemoryPackWriter)(ref writer)).WriteUnmanagedWithObjectHeader<int>((byte)3, ref levelID);
		((MemoryPackWriter)(ref writer)).WriteString(value.PhysicsLevelPath);
		((MemoryPackWriter)(ref writer)).WriteString(value.CoreLevelPath);
	}

	[Preserve]
	public static void Deserialize(ref MemoryPackReader reader, ref LevelConfig? value)
	{
		byte b = default(byte);
		if (!((MemoryPackReader)(ref reader)).TryReadObjectHeader(ref b))
		{
			value = null;
			return;
		}
		int levelID = default(int);
		string physicsLevelPath;
		string coreLevelPath;
		if (b == 3)
		{
			if (value != null)
			{
				levelID = value.LevelID;
				physicsLevelPath = value.PhysicsLevelPath;
				coreLevelPath = value.CoreLevelPath;
				((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref levelID);
				physicsLevelPath = ((MemoryPackReader)(ref reader)).ReadString();
				coreLevelPath = ((MemoryPackReader)(ref reader)).ReadString();
				goto IL_00c4;
			}
			((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref levelID);
			physicsLevelPath = ((MemoryPackReader)(ref reader)).ReadString();
			coreLevelPath = ((MemoryPackReader)(ref reader)).ReadString();
		}
		else
		{
			if (b > 3)
			{
				MemoryPackSerializationException.ThrowInvalidPropertyCount(typeof(LevelConfig), (byte)3, b);
				return;
			}
			if (value == null)
			{
				levelID = 0;
				physicsLevelPath = null;
				coreLevelPath = null;
			}
			else
			{
				levelID = value.LevelID;
				physicsLevelPath = value.PhysicsLevelPath;
				coreLevelPath = value.CoreLevelPath;
			}
			if (b != 0)
			{
				((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref levelID);
				if (b != 1)
				{
					physicsLevelPath = ((MemoryPackReader)(ref reader)).ReadString();
					if (b != 2)
					{
						coreLevelPath = ((MemoryPackReader)(ref reader)).ReadString();
						_ = 3;
					}
				}
			}
			if (value != null)
			{
				goto IL_00c4;
			}
		}
		value = new LevelConfig
		{
			LevelID = levelID,
			PhysicsLevelPath = physicsLevelPath,
			CoreLevelPath = coreLevelPath
		};
		return;
		IL_00c4:
		value.LevelID = levelID;
		value.PhysicsLevelPath = physicsLevelPath;
		value.CoreLevelPath = coreLevelPath;
	}
}
