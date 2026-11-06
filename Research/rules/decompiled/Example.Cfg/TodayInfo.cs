using MemoryPack;
using MemoryPack.Formatters;
using MemoryPack.Internal;

namespace Example.Cfg;

[MemoryPackable(/*Could not decode attribute arguments.*/)]
public class TodayInfo : IMemoryPackable<TodayInfo>, IMemoryPackFormatterRegister
{
	[Preserve]
	private sealed class TodayInfoFormatter : MemoryPackFormatter<TodayInfo>
	{
		[Preserve]
		public override void Serialize(ref MemoryPackWriter writer, ref TodayInfo value)
		{
			TodayInfo.Serialize(ref writer, ref value);
		}

		[Preserve]
		public override void Deserialize(ref MemoryPackReader reader, ref TodayInfo value)
		{
			TodayInfo.Deserialize(ref reader, ref value);
		}
	}

	[MemoryPackOrder(0)]
	public bool IsStart { get; private set; }

	[MemoryPackOrder(1)]
	public long Day { get; private set; }

	[MemoryPackOrder(2)]
	public int TodayIndex { get; private set; }

	static TodayInfo()
	{
		RegisterFormatter();
	}

	[Preserve]
	public static void RegisterFormatter()
	{
		if (!MemoryPackFormatterProvider.IsRegistered<TodayInfo>())
		{
			MemoryPackFormatterProvider.Register<TodayInfo>((MemoryPackFormatter<TodayInfo>)(object)new TodayInfoFormatter());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<TodayInfo[]>())
		{
			MemoryPackFormatterProvider.Register<TodayInfo[]>((MemoryPackFormatter<TodayInfo[]>)(object)new ArrayFormatter<TodayInfo>());
		}
	}

	[Preserve]
	public static void Serialize(ref MemoryPackWriter writer, ref TodayInfo? value)
	{
		if (value == null)
		{
			((MemoryPackWriter)(ref writer)).WriteNullObjectHeader();
			return;
		}
		bool isStart = value.IsStart;
		long day = value.Day;
		int todayIndex = value.TodayIndex;
		((MemoryPackWriter)(ref writer)).WriteUnmanagedWithObjectHeader<bool, long, int>((byte)3, ref isStart, ref day, ref todayIndex);
	}

	[Preserve]
	public static void Deserialize(ref MemoryPackReader reader, ref TodayInfo? value)
	{
		byte b = default(byte);
		if (!((MemoryPackReader)(ref reader)).TryReadObjectHeader(ref b))
		{
			value = null;
			return;
		}
		bool isStart = default(bool);
		long day = default(long);
		int todayIndex = default(int);
		if (b == 3)
		{
			if (value != null)
			{
				isStart = value.IsStart;
				day = value.Day;
				todayIndex = value.TodayIndex;
				((MemoryPackReader)(ref reader)).ReadUnmanaged<bool>(ref isStart);
				((MemoryPackReader)(ref reader)).ReadUnmanaged<long>(ref day);
				((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref todayIndex);
				goto IL_00bf;
			}
			((MemoryPackReader)(ref reader)).ReadUnmanaged<bool, long, int>(ref isStart, ref day, ref todayIndex);
		}
		else
		{
			if (b > 3)
			{
				MemoryPackSerializationException.ThrowInvalidPropertyCount(typeof(TodayInfo), (byte)3, b);
				return;
			}
			if (value == null)
			{
				isStart = false;
				day = 0L;
				todayIndex = 0;
			}
			else
			{
				isStart = value.IsStart;
				day = value.Day;
				todayIndex = value.TodayIndex;
			}
			if (b != 0)
			{
				((MemoryPackReader)(ref reader)).ReadUnmanaged<bool>(ref isStart);
				if (b != 1)
				{
					((MemoryPackReader)(ref reader)).ReadUnmanaged<long>(ref day);
					if (b != 2)
					{
						((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref todayIndex);
						_ = 3;
					}
				}
			}
			if (value != null)
			{
				goto IL_00bf;
			}
		}
		value = new TodayInfo
		{
			IsStart = isStart,
			Day = day,
			TodayIndex = todayIndex
		};
		return;
		IL_00bf:
		value.IsStart = isStart;
		value.Day = day;
		value.TodayIndex = todayIndex;
	}
}
