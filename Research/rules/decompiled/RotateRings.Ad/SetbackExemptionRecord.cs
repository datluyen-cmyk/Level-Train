using MemoryPack;
using MemoryPack.Formatters;
using MemoryPack.Internal;
using XingYun.Save;

namespace RotateRings.Ad;

[MemoryPackable(/*Could not decode attribute arguments.*/)]
public class SetbackExemptionRecord : ISaveDefaultInitializer, IMemoryPackable<SetbackExemptionRecord>, IMemoryPackFormatterRegister
{
	[Preserve]
	private sealed class SetbackExemptionRecordFormatter : MemoryPackFormatter<SetbackExemptionRecord>
	{
		[Preserve]
		public override void Serialize(ref MemoryPackWriter writer, ref SetbackExemptionRecord value)
		{
			SetbackExemptionRecord.Serialize(ref writer, ref value);
		}

		[Preserve]
		public override void Deserialize(ref MemoryPackReader reader, ref SetbackExemptionRecord value)
		{
			SetbackExemptionRecord.Deserialize(ref reader, ref value);
		}
	}

	public string ActiveConfigRaw;

	public int LevelKey;

	public int RestartCount;

	public int ExitCount;

	public int ExemptTimes;

	public int ExemptBatchTotal;

	public int QuotaExpireLevelKey;

	public int CooldownUntilLevelKey;

	public void OnCreateDefault()
	{
		ActiveConfigRaw = string.Empty;
		LevelKey = 0;
		RestartCount = 0;
		ExitCount = 0;
		ExemptTimes = 0;
		ExemptBatchTotal = 0;
		QuotaExpireLevelKey = 0;
		CooldownUntilLevelKey = 0;
	}

	static SetbackExemptionRecord()
	{
		RegisterFormatter();
	}

	[Preserve]
	public static void RegisterFormatter()
	{
		if (!MemoryPackFormatterProvider.IsRegistered<SetbackExemptionRecord>())
		{
			MemoryPackFormatterProvider.Register<SetbackExemptionRecord>((MemoryPackFormatter<SetbackExemptionRecord>)(object)new SetbackExemptionRecordFormatter());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<SetbackExemptionRecord[]>())
		{
			MemoryPackFormatterProvider.Register<SetbackExemptionRecord[]>((MemoryPackFormatter<SetbackExemptionRecord[]>)(object)new ArrayFormatter<SetbackExemptionRecord>());
		}
	}

	[Preserve]
	public static void Serialize(ref MemoryPackWriter writer, ref SetbackExemptionRecord? value)
	{
		if (value == null)
		{
			((MemoryPackWriter)(ref writer)).WriteNullObjectHeader();
			return;
		}
		((MemoryPackWriter)(ref writer)).WriteObjectHeader((byte)8);
		((MemoryPackWriter)(ref writer)).WriteString(value.ActiveConfigRaw);
		((MemoryPackWriter)(ref writer)).WriteUnmanaged<int, int, int, int, int, int, int>(ref value.LevelKey, ref value.RestartCount, ref value.ExitCount, ref value.ExemptTimes, ref value.ExemptBatchTotal, ref value.QuotaExpireLevelKey, ref value.CooldownUntilLevelKey);
	}

	[Preserve]
	public static void Deserialize(ref MemoryPackReader reader, ref SetbackExemptionRecord? value)
	{
		byte b = default(byte);
		if (!((MemoryPackReader)(ref reader)).TryReadObjectHeader(ref b))
		{
			value = null;
			return;
		}
		int levelKey = default(int);
		int restartCount = default(int);
		int exitCount = default(int);
		int exemptTimes = default(int);
		int exemptBatchTotal = default(int);
		int quotaExpireLevelKey = default(int);
		int cooldownUntilLevelKey = default(int);
		string activeConfigRaw;
		if (b == 8)
		{
			if (value != null)
			{
				activeConfigRaw = value.ActiveConfigRaw;
				levelKey = value.LevelKey;
				restartCount = value.RestartCount;
				exitCount = value.ExitCount;
				exemptTimes = value.ExemptTimes;
				exemptBatchTotal = value.ExemptBatchTotal;
				quotaExpireLevelKey = value.QuotaExpireLevelKey;
				cooldownUntilLevelKey = value.CooldownUntilLevelKey;
				activeConfigRaw = ((MemoryPackReader)(ref reader)).ReadString();
				((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref levelKey);
				((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref restartCount);
				((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref exitCount);
				((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref exemptTimes);
				((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref exemptBatchTotal);
				((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref quotaExpireLevelKey);
				((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref cooldownUntilLevelKey);
				goto IL_019e;
			}
			activeConfigRaw = ((MemoryPackReader)(ref reader)).ReadString();
			((MemoryPackReader)(ref reader)).ReadUnmanaged<int, int, int, int, int, int, int>(ref levelKey, ref restartCount, ref exitCount, ref exemptTimes, ref exemptBatchTotal, ref quotaExpireLevelKey, ref cooldownUntilLevelKey);
		}
		else
		{
			if (b > 8)
			{
				MemoryPackSerializationException.ThrowInvalidPropertyCount(typeof(SetbackExemptionRecord), (byte)8, b);
				return;
			}
			if (value == null)
			{
				activeConfigRaw = null;
				levelKey = 0;
				restartCount = 0;
				exitCount = 0;
				exemptTimes = 0;
				exemptBatchTotal = 0;
				quotaExpireLevelKey = 0;
				cooldownUntilLevelKey = 0;
			}
			else
			{
				activeConfigRaw = value.ActiveConfigRaw;
				levelKey = value.LevelKey;
				restartCount = value.RestartCount;
				exitCount = value.ExitCount;
				exemptTimes = value.ExemptTimes;
				exemptBatchTotal = value.ExemptBatchTotal;
				quotaExpireLevelKey = value.QuotaExpireLevelKey;
				cooldownUntilLevelKey = value.CooldownUntilLevelKey;
			}
			if (b != 0)
			{
				activeConfigRaw = ((MemoryPackReader)(ref reader)).ReadString();
				if (b != 1)
				{
					((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref levelKey);
					if (b != 2)
					{
						((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref restartCount);
						if (b != 3)
						{
							((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref exitCount);
							if (b != 4)
							{
								((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref exemptTimes);
								if (b != 5)
								{
									((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref exemptBatchTotal);
									if (b != 6)
									{
										((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref quotaExpireLevelKey);
										if (b != 7)
										{
											((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref cooldownUntilLevelKey);
											_ = 8;
										}
									}
								}
							}
						}
					}
				}
			}
			if (value != null)
			{
				goto IL_019e;
			}
		}
		value = new SetbackExemptionRecord
		{
			ActiveConfigRaw = activeConfigRaw,
			LevelKey = levelKey,
			RestartCount = restartCount,
			ExitCount = exitCount,
			ExemptTimes = exemptTimes,
			ExemptBatchTotal = exemptBatchTotal,
			QuotaExpireLevelKey = quotaExpireLevelKey,
			CooldownUntilLevelKey = cooldownUntilLevelKey
		};
		return;
		IL_019e:
		value.ActiveConfigRaw = activeConfigRaw;
		value.LevelKey = levelKey;
		value.RestartCount = restartCount;
		value.ExitCount = exitCount;
		value.ExemptTimes = exemptTimes;
		value.ExemptBatchTotal = exemptBatchTotal;
		value.QuotaExpireLevelKey = quotaExpireLevelKey;
		value.CooldownUntilLevelKey = cooldownUntilLevelKey;
	}
}
