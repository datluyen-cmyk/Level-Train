using System.Collections.Generic;
using MemoryPack;
using MemoryPack.Formatters;
using MemoryPack.Internal;
using XingYun.Save;

namespace RotateRings.Ad;

[MemoryPackable(/*Could not decode attribute arguments.*/)]
public class InterstitialEscapeWindowRecord : ISaveDefaultInitializer, IMemoryPackable<InterstitialEscapeWindowRecord>, IMemoryPackFormatterRegister
{
	[Preserve]
	private sealed class InterstitialEscapeWindowRecordFormatter : MemoryPackFormatter<InterstitialEscapeWindowRecord>
	{
		[Preserve]
		public override void Serialize(ref MemoryPackWriter writer, ref InterstitialEscapeWindowRecord value)
		{
			InterstitialEscapeWindowRecord.Serialize(ref writer, ref value);
		}

		[Preserve]
		public override void Deserialize(ref MemoryPackReader reader, ref InterstitialEscapeWindowRecord value)
		{
			InterstitialEscapeWindowRecord.Deserialize(ref reader, ref value);
		}
	}

	public const int WindowCapacity = 3;

	public List<bool> EscapeWindow = new List<bool>(3);

	public int RewardBonusSeconds;

	public float RewardRequiredElapsedSeconds;

	public void OnCreateDefault()
	{
		EscapeWindow = new List<bool>(3);
		RewardBonusSeconds = 0;
		RewardRequiredElapsedSeconds = 0f;
	}

	static InterstitialEscapeWindowRecord()
	{
		RegisterFormatter();
	}

	[Preserve]
	public static void RegisterFormatter()
	{
		if (!MemoryPackFormatterProvider.IsRegistered<InterstitialEscapeWindowRecord>())
		{
			MemoryPackFormatterProvider.Register<InterstitialEscapeWindowRecord>((MemoryPackFormatter<InterstitialEscapeWindowRecord>)(object)new InterstitialEscapeWindowRecordFormatter());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<InterstitialEscapeWindowRecord[]>())
		{
			MemoryPackFormatterProvider.Register<InterstitialEscapeWindowRecord[]>((MemoryPackFormatter<InterstitialEscapeWindowRecord[]>)(object)new ArrayFormatter<InterstitialEscapeWindowRecord>());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<List<bool>>())
		{
			MemoryPackFormatterProvider.Register<List<bool>>((MemoryPackFormatter<List<bool>>)(object)new ListFormatter<bool>());
		}
	}

	[Preserve]
	public static void Serialize(ref MemoryPackWriter writer, ref InterstitialEscapeWindowRecord? value)
	{
		if (value == null)
		{
			((MemoryPackWriter)(ref writer)).WriteNullObjectHeader();
			return;
		}
		((MemoryPackWriter)(ref writer)).WriteObjectHeader((byte)3);
		((MemoryPackWriter)(ref writer)).WriteValue<List<bool>>(ref value.EscapeWindow);
		((MemoryPackWriter)(ref writer)).WriteUnmanaged<int, float>(ref value.RewardBonusSeconds, ref value.RewardRequiredElapsedSeconds);
	}

	[Preserve]
	public static void Deserialize(ref MemoryPackReader reader, ref InterstitialEscapeWindowRecord? value)
	{
		byte b = default(byte);
		if (!((MemoryPackReader)(ref reader)).TryReadObjectHeader(ref b))
		{
			value = null;
			return;
		}
		List<bool> escapeWindow;
		int rewardBonusSeconds = default(int);
		float rewardRequiredElapsedSeconds = default(float);
		if (b == 3)
		{
			if (value != null)
			{
				escapeWindow = value.EscapeWindow;
				rewardBonusSeconds = value.RewardBonusSeconds;
				rewardRequiredElapsedSeconds = value.RewardRequiredElapsedSeconds;
				((MemoryPackReader)(ref reader)).ReadValue<List<bool>>(ref escapeWindow);
				((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref rewardBonusSeconds);
				((MemoryPackReader)(ref reader)).ReadUnmanaged<float>(ref rewardRequiredElapsedSeconds);
				goto IL_00c7;
			}
			escapeWindow = ((MemoryPackReader)(ref reader)).ReadValue<List<bool>>();
			((MemoryPackReader)(ref reader)).ReadUnmanaged<int, float>(ref rewardBonusSeconds, ref rewardRequiredElapsedSeconds);
		}
		else
		{
			if (b > 3)
			{
				MemoryPackSerializationException.ThrowInvalidPropertyCount(typeof(InterstitialEscapeWindowRecord), (byte)3, b);
				return;
			}
			if (value == null)
			{
				escapeWindow = null;
				rewardBonusSeconds = 0;
				rewardRequiredElapsedSeconds = 0f;
			}
			else
			{
				escapeWindow = value.EscapeWindow;
				rewardBonusSeconds = value.RewardBonusSeconds;
				rewardRequiredElapsedSeconds = value.RewardRequiredElapsedSeconds;
			}
			if (b != 0)
			{
				((MemoryPackReader)(ref reader)).ReadValue<List<bool>>(ref escapeWindow);
				if (b != 1)
				{
					((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref rewardBonusSeconds);
					if (b != 2)
					{
						((MemoryPackReader)(ref reader)).ReadUnmanaged<float>(ref rewardRequiredElapsedSeconds);
						_ = 3;
					}
				}
			}
			if (value != null)
			{
				goto IL_00c7;
			}
		}
		value = new InterstitialEscapeWindowRecord
		{
			EscapeWindow = escapeWindow,
			RewardBonusSeconds = rewardBonusSeconds,
			RewardRequiredElapsedSeconds = rewardRequiredElapsedSeconds
		};
		return;
		IL_00c7:
		value.EscapeWindow = escapeWindow;
		value.RewardBonusSeconds = rewardBonusSeconds;
		value.RewardRequiredElapsedSeconds = rewardRequiredElapsedSeconds;
	}
}
