using MemoryPack;
using MemoryPack.Formatters;
using MemoryPack.Internal;
using XingYun.Save;

namespace RotateRings.Ad;

[MemoryPackable(/*Could not decode attribute arguments.*/)]
public class InterstitialBackgroundEscapeRecord : ISaveDefaultInitializer, IMemoryPackable<InterstitialBackgroundEscapeRecord>, IMemoryPackFormatterRegister
{
	[Preserve]
	private sealed class InterstitialBackgroundEscapeRecordFormatter : MemoryPackFormatter<InterstitialBackgroundEscapeRecord>
	{
		[Preserve]
		public override void Serialize(ref MemoryPackWriter writer, ref InterstitialBackgroundEscapeRecord value)
		{
			InterstitialBackgroundEscapeRecord.Serialize(ref writer, ref value);
		}

		[Preserve]
		public override void Deserialize(ref MemoryPackReader reader, ref InterstitialBackgroundEscapeRecord value)
		{
			InterstitialBackgroundEscapeRecord.Deserialize(ref reader, ref value);
		}
	}

	public bool ClosePending;

	public float LastEscapeDurationSeconds;

	public void OnCreateDefault()
	{
		ClosePending = false;
		LastEscapeDurationSeconds = 0f;
	}

	static InterstitialBackgroundEscapeRecord()
	{
		RegisterFormatter();
	}

	[Preserve]
	public static void RegisterFormatter()
	{
		if (!MemoryPackFormatterProvider.IsRegistered<InterstitialBackgroundEscapeRecord>())
		{
			MemoryPackFormatterProvider.Register<InterstitialBackgroundEscapeRecord>((MemoryPackFormatter<InterstitialBackgroundEscapeRecord>)(object)new InterstitialBackgroundEscapeRecordFormatter());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<InterstitialBackgroundEscapeRecord[]>())
		{
			MemoryPackFormatterProvider.Register<InterstitialBackgroundEscapeRecord[]>((MemoryPackFormatter<InterstitialBackgroundEscapeRecord[]>)(object)new ArrayFormatter<InterstitialBackgroundEscapeRecord>());
		}
	}

	[Preserve]
	public static void Serialize(ref MemoryPackWriter writer, ref InterstitialBackgroundEscapeRecord? value)
	{
		if (value == null)
		{
			((MemoryPackWriter)(ref writer)).WriteNullObjectHeader();
		}
		else
		{
			((MemoryPackWriter)(ref writer)).WriteUnmanagedWithObjectHeader<bool, float>((byte)2, ref value.ClosePending, ref value.LastEscapeDurationSeconds);
		}
	}

	[Preserve]
	public static void Deserialize(ref MemoryPackReader reader, ref InterstitialBackgroundEscapeRecord? value)
	{
		byte b = default(byte);
		if (!((MemoryPackReader)(ref reader)).TryReadObjectHeader(ref b))
		{
			value = null;
			return;
		}
		bool closePending = default(bool);
		float lastEscapeDurationSeconds = default(float);
		if (b == 2)
		{
			if (value != null)
			{
				closePending = value.ClosePending;
				lastEscapeDurationSeconds = value.LastEscapeDurationSeconds;
				((MemoryPackReader)(ref reader)).ReadUnmanaged<bool>(ref closePending);
				((MemoryPackReader)(ref reader)).ReadUnmanaged<float>(ref lastEscapeDurationSeconds);
				goto IL_009a;
			}
			((MemoryPackReader)(ref reader)).ReadUnmanaged<bool, float>(ref closePending, ref lastEscapeDurationSeconds);
		}
		else
		{
			if (b > 2)
			{
				MemoryPackSerializationException.ThrowInvalidPropertyCount(typeof(InterstitialBackgroundEscapeRecord), (byte)2, b);
				return;
			}
			if (value == null)
			{
				closePending = false;
				lastEscapeDurationSeconds = 0f;
			}
			else
			{
				closePending = value.ClosePending;
				lastEscapeDurationSeconds = value.LastEscapeDurationSeconds;
			}
			if (b != 0)
			{
				((MemoryPackReader)(ref reader)).ReadUnmanaged<bool>(ref closePending);
				if (b != 1)
				{
					((MemoryPackReader)(ref reader)).ReadUnmanaged<float>(ref lastEscapeDurationSeconds);
					_ = 2;
				}
			}
			if (value != null)
			{
				goto IL_009a;
			}
		}
		value = new InterstitialBackgroundEscapeRecord
		{
			ClosePending = closePending,
			LastEscapeDurationSeconds = lastEscapeDurationSeconds
		};
		return;
		IL_009a:
		value.ClosePending = closePending;
		value.LastEscapeDurationSeconds = lastEscapeDurationSeconds;
	}
}
