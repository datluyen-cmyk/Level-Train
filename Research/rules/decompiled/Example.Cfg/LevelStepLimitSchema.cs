using MemoryPack;
using MemoryPack.Formatters;
using MemoryPack.Internal;

namespace Example.Cfg;

[MemoryPackable(/*Could not decode attribute arguments.*/)]
public class LevelStepLimitSchema : IMemoryPackable<LevelStepLimitSchema>, IMemoryPackFormatterRegister
{
	[Preserve]
	private sealed class LevelStepLimitSchemaFormatter : MemoryPackFormatter<LevelStepLimitSchema>
	{
		[Preserve]
		public override void Serialize(ref MemoryPackWriter writer, ref LevelStepLimitSchema value)
		{
			LevelStepLimitSchema.Serialize(ref writer, ref value);
		}

		[Preserve]
		public override void Deserialize(ref MemoryPackReader reader, ref LevelStepLimitSchema value)
		{
			LevelStepLimitSchema.Deserialize(ref reader, ref value);
		}
	}

	[MemoryPackOrder(0)]
	public int Id { get; private set; }

	[MemoryPackOrder(1)]
	public int LevelType { get; private set; }

	[MemoryPackOrder(2)]
	public int StepLimitType { get; private set; }

	[MemoryPackOrder(3)]
	public int AddStepsPerAd { get; private set; }

	static LevelStepLimitSchema()
	{
		RegisterFormatter();
	}

	[Preserve]
	public static void RegisterFormatter()
	{
		if (!MemoryPackFormatterProvider.IsRegistered<LevelStepLimitSchema>())
		{
			MemoryPackFormatterProvider.Register<LevelStepLimitSchema>((MemoryPackFormatter<LevelStepLimitSchema>)(object)new LevelStepLimitSchemaFormatter());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<LevelStepLimitSchema[]>())
		{
			MemoryPackFormatterProvider.Register<LevelStepLimitSchema[]>((MemoryPackFormatter<LevelStepLimitSchema[]>)(object)new ArrayFormatter<LevelStepLimitSchema>());
		}
	}

	[Preserve]
	public static void Serialize(ref MemoryPackWriter writer, ref LevelStepLimitSchema? value)
	{
		if (value == null)
		{
			((MemoryPackWriter)(ref writer)).WriteNullObjectHeader();
			return;
		}
		int id = value.Id;
		int levelType = value.LevelType;
		int stepLimitType = value.StepLimitType;
		int addStepsPerAd = value.AddStepsPerAd;
		((MemoryPackWriter)(ref writer)).WriteUnmanagedWithObjectHeader<int, int, int, int>((byte)4, ref id, ref levelType, ref stepLimitType, ref addStepsPerAd);
	}

	[Preserve]
	public static void Deserialize(ref MemoryPackReader reader, ref LevelStepLimitSchema? value)
	{
		byte b = default(byte);
		if (!((MemoryPackReader)(ref reader)).TryReadObjectHeader(ref b))
		{
			value = null;
			return;
		}
		int id = default(int);
		int levelType = default(int);
		int stepLimitType = default(int);
		int addStepsPerAd = default(int);
		if (b == 4)
		{
			if (value != null)
			{
				id = value.Id;
				levelType = value.LevelType;
				stepLimitType = value.StepLimitType;
				addStepsPerAd = value.AddStepsPerAd;
				((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref id);
				((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref levelType);
				((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref stepLimitType);
				((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref addStepsPerAd);
				goto IL_00e9;
			}
			((MemoryPackReader)(ref reader)).ReadUnmanaged<int, int, int, int>(ref id, ref levelType, ref stepLimitType, ref addStepsPerAd);
		}
		else
		{
			if (b > 4)
			{
				MemoryPackSerializationException.ThrowInvalidPropertyCount(typeof(LevelStepLimitSchema), (byte)4, b);
				return;
			}
			if (value == null)
			{
				id = 0;
				levelType = 0;
				stepLimitType = 0;
				addStepsPerAd = 0;
			}
			else
			{
				id = value.Id;
				levelType = value.LevelType;
				stepLimitType = value.StepLimitType;
				addStepsPerAd = value.AddStepsPerAd;
			}
			if (b != 0)
			{
				((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref id);
				if (b != 1)
				{
					((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref levelType);
					if (b != 2)
					{
						((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref stepLimitType);
						if (b != 3)
						{
							((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref addStepsPerAd);
							_ = 4;
						}
					}
				}
			}
			if (value != null)
			{
				goto IL_00e9;
			}
		}
		value = new LevelStepLimitSchema
		{
			Id = id,
			LevelType = levelType,
			StepLimitType = stepLimitType,
			AddStepsPerAd = addStepsPerAd
		};
		return;
		IL_00e9:
		value.Id = id;
		value.LevelType = levelType;
		value.StepLimitType = stepLimitType;
		value.AddStepsPerAd = addStepsPerAd;
	}
}
