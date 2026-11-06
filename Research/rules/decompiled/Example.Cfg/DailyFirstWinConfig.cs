using MemoryPack;
using MemoryPack.Formatters;
using MemoryPack.Internal;

namespace Example.Cfg;

[MemoryPackable(/*Could not decode attribute arguments.*/)]
public class DailyFirstWinConfig : IMemoryPackable<DailyFirstWinConfig>, IMemoryPackFormatterRegister
{
	[Preserve]
	private sealed class DailyFirstWinConfigFormatter : MemoryPackFormatter<DailyFirstWinConfig>
	{
		[Preserve]
		public override void Serialize(ref MemoryPackWriter writer, ref DailyFirstWinConfig value)
		{
			DailyFirstWinConfig.Serialize(ref writer, ref value);
		}

		[Preserve]
		public override void Deserialize(ref MemoryPackReader reader, ref DailyFirstWinConfig value)
		{
			DailyFirstWinConfig.Deserialize(ref reader, ref value);
		}
	}

	[MemoryPackOrder(0)]
	public int Id { get; private set; }

	[MemoryPackOrder(1)]
	public int OpenLevel { get; private set; }

	[MemoryPackOrder(2)]
	public string TrigleCheckTime { get; private set; }

	[MemoryPackOrder(3)]
	public int Period { get; private set; }

	[MemoryPackOrder(4)]
	public string Days { get; private set; }

	[MemoryPackOrder(5)]
	public string Rewards { get; private set; }

	static DailyFirstWinConfig()
	{
		RegisterFormatter();
	}

	[Preserve]
	public static void RegisterFormatter()
	{
		if (!MemoryPackFormatterProvider.IsRegistered<DailyFirstWinConfig>())
		{
			MemoryPackFormatterProvider.Register<DailyFirstWinConfig>((MemoryPackFormatter<DailyFirstWinConfig>)(object)new DailyFirstWinConfigFormatter());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<DailyFirstWinConfig[]>())
		{
			MemoryPackFormatterProvider.Register<DailyFirstWinConfig[]>((MemoryPackFormatter<DailyFirstWinConfig[]>)(object)new ArrayFormatter<DailyFirstWinConfig>());
		}
	}

	[Preserve]
	public static void Serialize(ref MemoryPackWriter writer, ref DailyFirstWinConfig? value)
	{
		if (value == null)
		{
			((MemoryPackWriter)(ref writer)).WriteNullObjectHeader();
			return;
		}
		int id = value.Id;
		int openLevel = value.OpenLevel;
		((MemoryPackWriter)(ref writer)).WriteUnmanagedWithObjectHeader<int, int>((byte)6, ref id, ref openLevel);
		((MemoryPackWriter)(ref writer)).WriteString(value.TrigleCheckTime);
		id = value.Period;
		((MemoryPackWriter)(ref writer)).WriteUnmanaged<int>(ref id);
		((MemoryPackWriter)(ref writer)).WriteString(value.Days);
		((MemoryPackWriter)(ref writer)).WriteString(value.Rewards);
	}

	[Preserve]
	public static void Deserialize(ref MemoryPackReader reader, ref DailyFirstWinConfig? value)
	{
		byte b = default(byte);
		if (!((MemoryPackReader)(ref reader)).TryReadObjectHeader(ref b))
		{
			value = null;
			return;
		}
		int id = default(int);
		int openLevel = default(int);
		int period = default(int);
		string trigleCheckTime;
		string days;
		string rewards;
		if (b == 6)
		{
			if (value != null)
			{
				id = value.Id;
				openLevel = value.OpenLevel;
				trigleCheckTime = value.TrigleCheckTime;
				period = value.Period;
				days = value.Days;
				rewards = value.Rewards;
				((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref id);
				((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref openLevel);
				trigleCheckTime = ((MemoryPackReader)(ref reader)).ReadString();
				((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref period);
				days = ((MemoryPackReader)(ref reader)).ReadString();
				rewards = ((MemoryPackReader)(ref reader)).ReadString();
				goto IL_015a;
			}
			((MemoryPackReader)(ref reader)).ReadUnmanaged<int, int>(ref id, ref openLevel);
			trigleCheckTime = ((MemoryPackReader)(ref reader)).ReadString();
			((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref period);
			days = ((MemoryPackReader)(ref reader)).ReadString();
			rewards = ((MemoryPackReader)(ref reader)).ReadString();
		}
		else
		{
			if (b > 6)
			{
				MemoryPackSerializationException.ThrowInvalidPropertyCount(typeof(DailyFirstWinConfig), (byte)6, b);
				return;
			}
			if (value == null)
			{
				id = 0;
				openLevel = 0;
				trigleCheckTime = null;
				period = 0;
				days = null;
				rewards = null;
			}
			else
			{
				id = value.Id;
				openLevel = value.OpenLevel;
				trigleCheckTime = value.TrigleCheckTime;
				period = value.Period;
				days = value.Days;
				rewards = value.Rewards;
			}
			if (b != 0)
			{
				((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref id);
				if (b != 1)
				{
					((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref openLevel);
					if (b != 2)
					{
						trigleCheckTime = ((MemoryPackReader)(ref reader)).ReadString();
						if (b != 3)
						{
							((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref period);
							if (b != 4)
							{
								days = ((MemoryPackReader)(ref reader)).ReadString();
								if (b != 5)
								{
									rewards = ((MemoryPackReader)(ref reader)).ReadString();
									_ = 6;
								}
							}
						}
					}
				}
			}
			if (value != null)
			{
				goto IL_015a;
			}
		}
		value = new DailyFirstWinConfig
		{
			Id = id,
			OpenLevel = openLevel,
			TrigleCheckTime = trigleCheckTime,
			Period = period,
			Days = days,
			Rewards = rewards
		};
		return;
		IL_015a:
		value.Id = id;
		value.OpenLevel = openLevel;
		value.TrigleCheckTime = trigleCheckTime;
		value.Period = period;
		value.Days = days;
		value.Rewards = rewards;
	}
}
