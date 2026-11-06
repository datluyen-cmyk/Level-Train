using System.Collections.Generic;
using MemoryPack;
using MemoryPack.Formatters;
using MemoryPack.Internal;

namespace Example.Cfg;

[MemoryPackable(/*Could not decode attribute arguments.*/)]
public class StageReward : IMemoryPackable<StageReward>, IMemoryPackFormatterRegister
{
	[Preserve]
	private sealed class StageRewardFormatter : MemoryPackFormatter<StageReward>
	{
		[Preserve]
		public override void Serialize(ref MemoryPackWriter writer, ref StageReward value)
		{
			StageReward.Serialize(ref writer, ref value);
		}

		[Preserve]
		public override void Deserialize(ref MemoryPackReader reader, ref StageReward value)
		{
			StageReward.Deserialize(ref reader, ref value);
		}
	}

	[MemoryPackOrder(0)]
	public int id { get; private set; }

	[MemoryPackOrder(1)]
	public int group { get; private set; }

	[MemoryPackOrder(2)]
	public int looptype { get; private set; }

	[MemoryPackOrder(3)]
	public List<int> Progress { get; private set; }

	[MemoryPackOrder(4)]
	public List<int> Reward { get; private set; }

	static StageReward()
	{
		RegisterFormatter();
	}

	[Preserve]
	public static void RegisterFormatter()
	{
		if (!MemoryPackFormatterProvider.IsRegistered<StageReward>())
		{
			MemoryPackFormatterProvider.Register<StageReward>((MemoryPackFormatter<StageReward>)(object)new StageRewardFormatter());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<StageReward[]>())
		{
			MemoryPackFormatterProvider.Register<StageReward[]>((MemoryPackFormatter<StageReward[]>)(object)new ArrayFormatter<StageReward>());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<List<int>>())
		{
			MemoryPackFormatterProvider.Register<List<int>>((MemoryPackFormatter<List<int>>)(object)new ListFormatter<int>());
		}
	}

	[Preserve]
	public static void Serialize(ref MemoryPackWriter writer, ref StageReward? value)
	{
		if (value == null)
		{
			((MemoryPackWriter)(ref writer)).WriteNullObjectHeader();
			return;
		}
		int num = value.id;
		int num2 = value.group;
		int num3 = value.looptype;
		((MemoryPackWriter)(ref writer)).WriteUnmanagedWithObjectHeader<int, int, int>((byte)5, ref num, ref num2, ref num3);
		List<int> progress = value.Progress;
		((MemoryPackWriter)(ref writer)).WriteValue<List<int>>(ref progress);
		progress = value.Reward;
		((MemoryPackWriter)(ref writer)).WriteValue<List<int>>(ref progress);
	}

	[Preserve]
	public static void Deserialize(ref MemoryPackReader reader, ref StageReward? value)
	{
		byte b = default(byte);
		if (!((MemoryPackReader)(ref reader)).TryReadObjectHeader(ref b))
		{
			value = null;
			return;
		}
		int num = default(int);
		int num2 = default(int);
		int num3 = default(int);
		List<int> progress;
		List<int> reward;
		if (b == 5)
		{
			if (value != null)
			{
				num = value.id;
				num2 = value.group;
				num3 = value.looptype;
				progress = value.Progress;
				reward = value.Reward;
				((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref num);
				((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref num2);
				((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref num3);
				((MemoryPackReader)(ref reader)).ReadValue<List<int>>(ref progress);
				((MemoryPackReader)(ref reader)).ReadValue<List<int>>(ref reward);
				goto IL_0123;
			}
			((MemoryPackReader)(ref reader)).ReadUnmanaged<int, int, int>(ref num, ref num2, ref num3);
			progress = ((MemoryPackReader)(ref reader)).ReadValue<List<int>>();
			reward = ((MemoryPackReader)(ref reader)).ReadValue<List<int>>();
		}
		else
		{
			if (b > 5)
			{
				MemoryPackSerializationException.ThrowInvalidPropertyCount(typeof(StageReward), (byte)5, b);
				return;
			}
			if (value == null)
			{
				num = 0;
				num2 = 0;
				num3 = 0;
				progress = null;
				reward = null;
			}
			else
			{
				num = value.id;
				num2 = value.group;
				num3 = value.looptype;
				progress = value.Progress;
				reward = value.Reward;
			}
			if (b != 0)
			{
				((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref num);
				if (b != 1)
				{
					((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref num2);
					if (b != 2)
					{
						((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref num3);
						if (b != 3)
						{
							((MemoryPackReader)(ref reader)).ReadValue<List<int>>(ref progress);
							if (b != 4)
							{
								((MemoryPackReader)(ref reader)).ReadValue<List<int>>(ref reward);
								_ = 5;
							}
						}
					}
				}
			}
			if (value != null)
			{
				goto IL_0123;
			}
		}
		value = new StageReward
		{
			id = num,
			group = num2,
			looptype = num3,
			Progress = progress,
			Reward = reward
		};
		return;
		IL_0123:
		value.id = num;
		value.group = num2;
		value.looptype = num3;
		value.Progress = progress;
		value.Reward = reward;
	}
}
