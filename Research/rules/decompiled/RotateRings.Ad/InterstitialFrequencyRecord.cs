using MemoryPack;
using MemoryPack.Formatters;
using MemoryPack.Internal;
using XingYun.Save;

namespace RotateRings.Ad;

[MemoryPackable(/*Could not decode attribute arguments.*/)]
public class InterstitialFrequencyRecord : ISaveDefaultInitializer, IMemoryPackable<InterstitialFrequencyRecord>, IMemoryPackFormatterRegister
{
	[Preserve]
	private sealed class InterstitialFrequencyRecordFormatter : MemoryPackFormatter<InterstitialFrequencyRecord>
	{
		[Preserve]
		public override void Serialize(ref MemoryPackWriter writer, ref InterstitialFrequencyRecord value)
		{
			InterstitialFrequencyRecord.Serialize(ref writer, ref value);
		}

		[Preserve]
		public override void Deserialize(ref MemoryPackReader reader, ref InterstitialFrequencyRecord value)
		{
			InterstitialFrequencyRecord.Deserialize(ref reader, ref value);
		}
	}

	public double PreviousAverageInterval;

	public void OnCreateDefault()
	{
		PreviousAverageInterval = -1.0;
	}

	static InterstitialFrequencyRecord()
	{
		RegisterFormatter();
	}

	[Preserve]
	public static void RegisterFormatter()
	{
		if (!MemoryPackFormatterProvider.IsRegistered<InterstitialFrequencyRecord>())
		{
			MemoryPackFormatterProvider.Register<InterstitialFrequencyRecord>((MemoryPackFormatter<InterstitialFrequencyRecord>)(object)new InterstitialFrequencyRecordFormatter());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<InterstitialFrequencyRecord[]>())
		{
			MemoryPackFormatterProvider.Register<InterstitialFrequencyRecord[]>((MemoryPackFormatter<InterstitialFrequencyRecord[]>)(object)new ArrayFormatter<InterstitialFrequencyRecord>());
		}
	}

	[Preserve]
	public static void Serialize(ref MemoryPackWriter writer, ref InterstitialFrequencyRecord? value)
	{
		if (value == null)
		{
			((MemoryPackWriter)(ref writer)).WriteNullObjectHeader();
		}
		else
		{
			((MemoryPackWriter)(ref writer)).WriteUnmanagedWithObjectHeader<double>((byte)1, ref value.PreviousAverageInterval);
		}
	}

	[Preserve]
	public static void Deserialize(ref MemoryPackReader reader, ref InterstitialFrequencyRecord? value)
	{
		byte b = default(byte);
		if (!((MemoryPackReader)(ref reader)).TryReadObjectHeader(ref b))
		{
			value = null;
			return;
		}
		double previousAverageInterval = default(double);
		if (b == 1)
		{
			if (value != null)
			{
				previousAverageInterval = value.PreviousAverageInterval;
				((MemoryPackReader)(ref reader)).ReadUnmanaged<double>(ref previousAverageInterval);
				goto IL_0073;
			}
			((MemoryPackReader)(ref reader)).ReadUnmanaged<double>(ref previousAverageInterval);
		}
		else
		{
			if (b > 1)
			{
				MemoryPackSerializationException.ThrowInvalidPropertyCount(typeof(InterstitialFrequencyRecord), (byte)1, b);
				return;
			}
			previousAverageInterval = ((value != null) ? value.PreviousAverageInterval : 0.0);
			if (b != 0)
			{
				((MemoryPackReader)(ref reader)).ReadUnmanaged<double>(ref previousAverageInterval);
				_ = 1;
			}
			if (value != null)
			{
				goto IL_0073;
			}
		}
		value = new InterstitialFrequencyRecord
		{
			PreviousAverageInterval = previousAverageInterval
		};
		return;
		IL_0073:
		value.PreviousAverageInterval = previousAverageInterval;
	}
}
