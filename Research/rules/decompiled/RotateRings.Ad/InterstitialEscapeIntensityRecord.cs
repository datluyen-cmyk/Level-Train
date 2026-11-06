using System.Collections.Generic;
using MemoryPack;
using MemoryPack.Formatters;
using MemoryPack.Internal;
using XingYun.Save;

namespace RotateRings.Ad;

[MemoryPackable(/*Could not decode attribute arguments.*/)]
public class InterstitialEscapeIntensityRecord : ISaveDefaultInitializer, IMemoryPackable<InterstitialEscapeIntensityRecord>, IMemoryPackFormatterRegister
{
	[Preserve]
	private sealed class InterstitialEscapeIntensityRecordFormatter : MemoryPackFormatter<InterstitialEscapeIntensityRecord>
	{
		[Preserve]
		public override void Serialize(ref MemoryPackWriter writer, ref InterstitialEscapeIntensityRecord value)
		{
			InterstitialEscapeIntensityRecord.Serialize(ref writer, ref value);
		}

		[Preserve]
		public override void Deserialize(ref MemoryPackReader reader, ref InterstitialEscapeIntensityRecord value)
		{
			InterstitialEscapeIntensityRecord.Deserialize(ref reader, ref value);
		}
	}

	public const int WindowCapacity = 3;

	public List<float> EscapeIntensityWindow = new List<float>(3);

	public int RewardBonusSeconds;

	public float RewardRequiredElapsedSeconds;

	public void OnCreateDefault()
	{
		EscapeIntensityWindow = new List<float>(3);
		RewardBonusSeconds = 0;
		RewardRequiredElapsedSeconds = 0f;
	}

	static InterstitialEscapeIntensityRecord()
	{
		RegisterFormatter();
	}

	[Preserve]
	public static void RegisterFormatter()
	{
		if (!MemoryPackFormatterProvider.IsRegistered<InterstitialEscapeIntensityRecord>())
		{
			MemoryPackFormatterProvider.Register<InterstitialEscapeIntensityRecord>((MemoryPackFormatter<InterstitialEscapeIntensityRecord>)(object)new InterstitialEscapeIntensityRecordFormatter());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<InterstitialEscapeIntensityRecord[]>())
		{
			MemoryPackFormatterProvider.Register<InterstitialEscapeIntensityRecord[]>((MemoryPackFormatter<InterstitialEscapeIntensityRecord[]>)(object)new ArrayFormatter<InterstitialEscapeIntensityRecord>());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<List<float>>())
		{
			MemoryPackFormatterProvider.Register<List<float>>((MemoryPackFormatter<List<float>>)(object)new ListFormatter<float>());
		}
	}

	[Preserve]
	public static void Serialize(ref MemoryPackWriter writer, ref InterstitialEscapeIntensityRecord? value)
	{
		if (value == null)
		{
			((MemoryPackWriter)(ref writer)).WriteNullObjectHeader();
			return;
		}
		((MemoryPackWriter)(ref writer)).WriteObjectHeader((byte)3);
		((MemoryPackWriter)(ref writer)).WriteValue<List<float>>(ref value.EscapeIntensityWindow);
		((MemoryPackWriter)(ref writer)).WriteUnmanaged<int, float>(ref value.RewardBonusSeconds, ref value.RewardRequiredElapsedSeconds);
	}

	[Preserve]
	public static void Deserialize(ref MemoryPackReader reader, ref InterstitialEscapeIntensityRecord? value)
	{
		byte b = default(byte);
		if (!((MemoryPackReader)(ref reader)).TryReadObjectHeader(ref b))
		{
			value = null;
			return;
		}
		List<float> escapeIntensityWindow;
		int rewardBonusSeconds = default(int);
		float rewardRequiredElapsedSeconds = default(float);
		if (b == 3)
		{
			if (value != null)
			{
				escapeIntensityWindow = value.EscapeIntensityWindow;
				rewardBonusSeconds = value.RewardBonusSeconds;
				rewardRequiredElapsedSeconds = value.RewardRequiredElapsedSeconds;
				((MemoryPackReader)(ref reader)).ReadValue<List<float>>(ref escapeIntensityWindow);
				((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref rewardBonusSeconds);
				((MemoryPackReader)(ref reader)).ReadUnmanaged<float>(ref rewardRequiredElapsedSeconds);
				goto IL_00c7;
			}
			escapeIntensityWindow = ((MemoryPackReader)(ref reader)).ReadValue<List<float>>();
			((MemoryPackReader)(ref reader)).ReadUnmanaged<int, float>(ref rewardBonusSeconds, ref rewardRequiredElapsedSeconds);
		}
		else
		{
			if (b > 3)
			{
				MemoryPackSerializationException.ThrowInvalidPropertyCount(typeof(InterstitialEscapeIntensityRecord), (byte)3, b);
				return;
			}
			if (value == null)
			{
				escapeIntensityWindow = null;
				rewardBonusSeconds = 0;
				rewardRequiredElapsedSeconds = 0f;
			}
			else
			{
				escapeIntensityWindow = value.EscapeIntensityWindow;
				rewardBonusSeconds = value.RewardBonusSeconds;
				rewardRequiredElapsedSeconds = value.RewardRequiredElapsedSeconds;
			}
			if (b != 0)
			{
				((MemoryPackReader)(ref reader)).ReadValue<List<float>>(ref escapeIntensityWindow);
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
		value = new InterstitialEscapeIntensityRecord
		{
			EscapeIntensityWindow = escapeIntensityWindow,
			RewardBonusSeconds = rewardBonusSeconds,
			RewardRequiredElapsedSeconds = rewardRequiredElapsedSeconds
		};
		return;
		IL_00c7:
		value.EscapeIntensityWindow = escapeIntensityWindow;
		value.RewardBonusSeconds = rewardBonusSeconds;
		value.RewardRequiredElapsedSeconds = rewardRequiredElapsedSeconds;
	}
}
