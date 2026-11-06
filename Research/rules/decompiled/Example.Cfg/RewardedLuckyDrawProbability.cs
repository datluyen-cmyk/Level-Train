using MemoryPack;
using MemoryPack.Formatters;
using MemoryPack.Internal;

namespace Example.Cfg;

[MemoryPackable(/*Could not decode attribute arguments.*/)]
public class RewardedLuckyDrawProbability : IMemoryPackable<RewardedLuckyDrawProbability>, IMemoryPackFormatterRegister
{
	[Preserve]
	private sealed class RewardedLuckyDrawProbabilityFormatter : MemoryPackFormatter<RewardedLuckyDrawProbability>
	{
		[Preserve]
		public override void Serialize(ref MemoryPackWriter writer, ref RewardedLuckyDrawProbability value)
		{
			RewardedLuckyDrawProbability.Serialize(ref writer, ref value);
		}

		[Preserve]
		public override void Deserialize(ref MemoryPackReader reader, ref RewardedLuckyDrawProbability value)
		{
			RewardedLuckyDrawProbability.Deserialize(ref reader, ref value);
		}
	}

	[MemoryPackOrder(0)]
	public int Id { get; private set; }

	[MemoryPackOrder(1)]
	public int GroupId { get; private set; }

	[MemoryPackOrder(2)]
	public int SlotId { get; private set; }

	[MemoryPackOrder(3)]
	public int PropType { get; private set; }

	[MemoryPackOrder(4)]
	public int Count { get; private set; }

	[MemoryPackOrder(5)]
	public int Weight { get; private set; }

	static RewardedLuckyDrawProbability()
	{
		RegisterFormatter();
	}

	[Preserve]
	public static void RegisterFormatter()
	{
		if (!MemoryPackFormatterProvider.IsRegistered<RewardedLuckyDrawProbability>())
		{
			MemoryPackFormatterProvider.Register<RewardedLuckyDrawProbability>((MemoryPackFormatter<RewardedLuckyDrawProbability>)(object)new RewardedLuckyDrawProbabilityFormatter());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<RewardedLuckyDrawProbability[]>())
		{
			MemoryPackFormatterProvider.Register<RewardedLuckyDrawProbability[]>((MemoryPackFormatter<RewardedLuckyDrawProbability[]>)(object)new ArrayFormatter<RewardedLuckyDrawProbability>());
		}
	}

	[Preserve]
	public static void Serialize(ref MemoryPackWriter writer, ref RewardedLuckyDrawProbability? value)
	{
		if (value == null)
		{
			((MemoryPackWriter)(ref writer)).WriteNullObjectHeader();
			return;
		}
		int id = value.Id;
		int groupId = value.GroupId;
		int slotId = value.SlotId;
		int propType = value.PropType;
		int count = value.Count;
		int weight = value.Weight;
		((MemoryPackWriter)(ref writer)).WriteUnmanagedWithObjectHeader<int, int, int, int, int, int>((byte)6, ref id, ref groupId, ref slotId, ref propType, ref count, ref weight);
	}

	[Preserve]
	public static void Deserialize(ref MemoryPackReader reader, ref RewardedLuckyDrawProbability? value)
	{
		byte b = default(byte);
		if (!((MemoryPackReader)(ref reader)).TryReadObjectHeader(ref b))
		{
			value = null;
			return;
		}
		int id = default(int);
		int groupId = default(int);
		int slotId = default(int);
		int propType = default(int);
		int count = default(int);
		int weight = default(int);
		if (b == 6)
		{
			if (value != null)
			{
				id = value.Id;
				groupId = value.GroupId;
				slotId = value.SlotId;
				propType = value.PropType;
				count = value.Count;
				weight = value.Weight;
				((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref id);
				((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref groupId);
				((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref slotId);
				((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref propType);
				((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref count);
				((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref weight);
				goto IL_0145;
			}
			((MemoryPackReader)(ref reader)).ReadUnmanaged<int, int, int, int, int, int>(ref id, ref groupId, ref slotId, ref propType, ref count, ref weight);
		}
		else
		{
			if (b > 6)
			{
				MemoryPackSerializationException.ThrowInvalidPropertyCount(typeof(RewardedLuckyDrawProbability), (byte)6, b);
				return;
			}
			if (value == null)
			{
				id = 0;
				groupId = 0;
				slotId = 0;
				propType = 0;
				count = 0;
				weight = 0;
			}
			else
			{
				id = value.Id;
				groupId = value.GroupId;
				slotId = value.SlotId;
				propType = value.PropType;
				count = value.Count;
				weight = value.Weight;
			}
			if (b != 0)
			{
				((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref id);
				if (b != 1)
				{
					((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref groupId);
					if (b != 2)
					{
						((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref slotId);
						if (b != 3)
						{
							((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref propType);
							if (b != 4)
							{
								((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref count);
								if (b != 5)
								{
									((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref weight);
									_ = 6;
								}
							}
						}
					}
				}
			}
			if (value != null)
			{
				goto IL_0145;
			}
		}
		value = new RewardedLuckyDrawProbability
		{
			Id = id,
			GroupId = groupId,
			SlotId = slotId,
			PropType = propType,
			Count = count,
			Weight = weight
		};
		return;
		IL_0145:
		value.Id = id;
		value.GroupId = groupId;
		value.SlotId = slotId;
		value.PropType = propType;
		value.Count = count;
		value.Weight = weight;
	}
}
