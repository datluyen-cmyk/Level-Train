using MemoryPack;
using MemoryPack.Formatters;
using MemoryPack.Internal;

namespace Example.Cfg;

[MemoryPackable(/*Could not decode attribute arguments.*/)]
public class RankListReward : IMemoryPackable<RankListReward>, IMemoryPackFormatterRegister
{
	[Preserve]
	private sealed class RankListRewardFormatter : MemoryPackFormatter<RankListReward>
	{
		[Preserve]
		public override void Serialize(ref MemoryPackWriter writer, ref RankListReward value)
		{
			RankListReward.Serialize(ref writer, ref value);
		}

		[Preserve]
		public override void Deserialize(ref MemoryPackReader reader, ref RankListReward value)
		{
			RankListReward.Deserialize(ref reader, ref value);
		}
	}

	[MemoryPackOrder(0)]
	public int Id { get; private set; }

	[MemoryPackOrder(1)]
	public int RewardGroup { get; private set; }

	[MemoryPackOrder(2)]
	public int Rank { get; private set; }

	[MemoryPackOrder(3)]
	public int Reward { get; private set; }

	static RankListReward()
	{
		RegisterFormatter();
	}

	[Preserve]
	public static void RegisterFormatter()
	{
		if (!MemoryPackFormatterProvider.IsRegistered<RankListReward>())
		{
			MemoryPackFormatterProvider.Register<RankListReward>((MemoryPackFormatter<RankListReward>)(object)new RankListRewardFormatter());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<RankListReward[]>())
		{
			MemoryPackFormatterProvider.Register<RankListReward[]>((MemoryPackFormatter<RankListReward[]>)(object)new ArrayFormatter<RankListReward>());
		}
	}

	[Preserve]
	public static void Serialize(ref MemoryPackWriter writer, ref RankListReward? value)
	{
		if (value == null)
		{
			((MemoryPackWriter)(ref writer)).WriteNullObjectHeader();
			return;
		}
		int id = value.Id;
		int rewardGroup = value.RewardGroup;
		int rank = value.Rank;
		int reward = value.Reward;
		((MemoryPackWriter)(ref writer)).WriteUnmanagedWithObjectHeader<int, int, int, int>((byte)4, ref id, ref rewardGroup, ref rank, ref reward);
	}

	[Preserve]
	public static void Deserialize(ref MemoryPackReader reader, ref RankListReward? value)
	{
		byte b = default(byte);
		if (!((MemoryPackReader)(ref reader)).TryReadObjectHeader(ref b))
		{
			value = null;
			return;
		}
		int id = default(int);
		int rewardGroup = default(int);
		int rank = default(int);
		int reward = default(int);
		if (b == 4)
		{
			if (value != null)
			{
				id = value.Id;
				rewardGroup = value.RewardGroup;
				rank = value.Rank;
				reward = value.Reward;
				((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref id);
				((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref rewardGroup);
				((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref rank);
				((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref reward);
				goto IL_00e9;
			}
			((MemoryPackReader)(ref reader)).ReadUnmanaged<int, int, int, int>(ref id, ref rewardGroup, ref rank, ref reward);
		}
		else
		{
			if (b > 4)
			{
				MemoryPackSerializationException.ThrowInvalidPropertyCount(typeof(RankListReward), (byte)4, b);
				return;
			}
			if (value == null)
			{
				id = 0;
				rewardGroup = 0;
				rank = 0;
				reward = 0;
			}
			else
			{
				id = value.Id;
				rewardGroup = value.RewardGroup;
				rank = value.Rank;
				reward = value.Reward;
			}
			if (b != 0)
			{
				((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref id);
				if (b != 1)
				{
					((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref rewardGroup);
					if (b != 2)
					{
						((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref rank);
						if (b != 3)
						{
							((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref reward);
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
		value = new RankListReward
		{
			Id = id,
			RewardGroup = rewardGroup,
			Rank = rank,
			Reward = reward
		};
		return;
		IL_00e9:
		value.Id = id;
		value.RewardGroup = rewardGroup;
		value.Rank = rank;
		value.Reward = reward;
	}
}
