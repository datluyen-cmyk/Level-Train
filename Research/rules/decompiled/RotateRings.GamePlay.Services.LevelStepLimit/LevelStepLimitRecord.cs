using MemoryPack;
using MemoryPack.Formatters;
using MemoryPack.Internal;
using XingYun.Save;

namespace RotateRings.GamePlay.Services.LevelStepLimit;

[MemoryPackable(/*Could not decode attribute arguments.*/)]
public class LevelStepLimitRecord : ISaveDefaultInitializer, IMemoryPackable<LevelStepLimitRecord>, IMemoryPackFormatterRegister
{
	[Preserve]
	private sealed class LevelStepLimitRecordFormatter : MemoryPackFormatter<LevelStepLimitRecord>
	{
		[Preserve]
		public override void Serialize(ref MemoryPackWriter writer, ref LevelStepLimitRecord value)
		{
			LevelStepLimitRecord.Serialize(ref writer, ref value);
		}

		[Preserve]
		public override void Deserialize(ref MemoryPackReader reader, ref LevelStepLimitRecord value)
		{
			LevelStepLimitRecord.Deserialize(ref reader, ref value);
		}
	}

	public bool FreeGiftClaimed;

	public bool TipsShown;

	public void OnCreateDefault()
	{
		FreeGiftClaimed = false;
		TipsShown = false;
	}

	static LevelStepLimitRecord()
	{
		RegisterFormatter();
	}

	[Preserve]
	public static void RegisterFormatter()
	{
		if (!MemoryPackFormatterProvider.IsRegistered<LevelStepLimitRecord>())
		{
			MemoryPackFormatterProvider.Register<LevelStepLimitRecord>((MemoryPackFormatter<LevelStepLimitRecord>)(object)new LevelStepLimitRecordFormatter());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<LevelStepLimitRecord[]>())
		{
			MemoryPackFormatterProvider.Register<LevelStepLimitRecord[]>((MemoryPackFormatter<LevelStepLimitRecord[]>)(object)new ArrayFormatter<LevelStepLimitRecord>());
		}
	}

	[Preserve]
	public static void Serialize(ref MemoryPackWriter writer, ref LevelStepLimitRecord? value)
	{
		if (value == null)
		{
			((MemoryPackWriter)(ref writer)).WriteNullObjectHeader();
		}
		else
		{
			((MemoryPackWriter)(ref writer)).WriteUnmanagedWithObjectHeader<bool, bool>((byte)2, ref value.FreeGiftClaimed, ref value.TipsShown);
		}
	}

	[Preserve]
	public static void Deserialize(ref MemoryPackReader reader, ref LevelStepLimitRecord? value)
	{
		byte b = default(byte);
		if (!((MemoryPackReader)(ref reader)).TryReadObjectHeader(ref b))
		{
			value = null;
			return;
		}
		bool freeGiftClaimed = default(bool);
		bool tipsShown = default(bool);
		if (b == 2)
		{
			if (value != null)
			{
				freeGiftClaimed = value.FreeGiftClaimed;
				tipsShown = value.TipsShown;
				((MemoryPackReader)(ref reader)).ReadUnmanaged<bool>(ref freeGiftClaimed);
				((MemoryPackReader)(ref reader)).ReadUnmanaged<bool>(ref tipsShown);
				goto IL_0096;
			}
			((MemoryPackReader)(ref reader)).ReadUnmanaged<bool, bool>(ref freeGiftClaimed, ref tipsShown);
		}
		else
		{
			if (b > 2)
			{
				MemoryPackSerializationException.ThrowInvalidPropertyCount(typeof(LevelStepLimitRecord), (byte)2, b);
				return;
			}
			if (value == null)
			{
				freeGiftClaimed = false;
				tipsShown = false;
			}
			else
			{
				freeGiftClaimed = value.FreeGiftClaimed;
				tipsShown = value.TipsShown;
			}
			if (b != 0)
			{
				((MemoryPackReader)(ref reader)).ReadUnmanaged<bool>(ref freeGiftClaimed);
				if (b != 1)
				{
					((MemoryPackReader)(ref reader)).ReadUnmanaged<bool>(ref tipsShown);
					_ = 2;
				}
			}
			if (value != null)
			{
				goto IL_0096;
			}
		}
		value = new LevelStepLimitRecord
		{
			FreeGiftClaimed = freeGiftClaimed,
			TipsShown = tipsShown
		};
		return;
		IL_0096:
		value.FreeGiftClaimed = freeGiftClaimed;
		value.TipsShown = tipsShown;
	}
}
