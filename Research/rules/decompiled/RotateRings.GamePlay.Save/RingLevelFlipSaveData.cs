using MemoryPack;
using MemoryPack.Formatters;
using MemoryPack.Internal;
using XingYun.Save;

namespace RotateRings.GamePlay.Save;

[MemoryPackable(/*Could not decode attribute arguments.*/)]
public class RingLevelFlipSaveData : ISaveDefaultInitializer, IMemoryPackable<RingLevelFlipSaveData>, IMemoryPackFormatterRegister
{
	[Preserve]
	private sealed class RingLevelFlipSaveDataFormatter : MemoryPackFormatter<RingLevelFlipSaveData>
	{
		[Preserve]
		public override void Serialize(ref MemoryPackWriter writer, ref RingLevelFlipSaveData value)
		{
			RingLevelFlipSaveData.Serialize(ref writer, ref value);
		}

		[Preserve]
		public override void Deserialize(ref MemoryPackReader reader, ref RingLevelFlipSaveData value)
		{
			RingLevelFlipSaveData.Deserialize(ref reader, ref value);
		}
	}

	public const int Undecided = -1;

	public int LevelNumber;

	public int FlipMode;

	public void OnCreateDefault()
	{
		LevelNumber = 0;
		FlipMode = -1;
	}

	static RingLevelFlipSaveData()
	{
		RegisterFormatter();
	}

	[Preserve]
	public static void RegisterFormatter()
	{
		if (!MemoryPackFormatterProvider.IsRegistered<RingLevelFlipSaveData>())
		{
			MemoryPackFormatterProvider.Register<RingLevelFlipSaveData>((MemoryPackFormatter<RingLevelFlipSaveData>)(object)new RingLevelFlipSaveDataFormatter());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<RingLevelFlipSaveData[]>())
		{
			MemoryPackFormatterProvider.Register<RingLevelFlipSaveData[]>((MemoryPackFormatter<RingLevelFlipSaveData[]>)(object)new ArrayFormatter<RingLevelFlipSaveData>());
		}
	}

	[Preserve]
	public static void Serialize(ref MemoryPackWriter writer, ref RingLevelFlipSaveData? value)
	{
		if (value == null)
		{
			((MemoryPackWriter)(ref writer)).WriteNullObjectHeader();
		}
		else
		{
			((MemoryPackWriter)(ref writer)).WriteUnmanagedWithObjectHeader<int, int>((byte)2, ref value.LevelNumber, ref value.FlipMode);
		}
	}

	[Preserve]
	public static void Deserialize(ref MemoryPackReader reader, ref RingLevelFlipSaveData? value)
	{
		byte b = default(byte);
		if (!((MemoryPackReader)(ref reader)).TryReadObjectHeader(ref b))
		{
			value = null;
			return;
		}
		int levelNumber = default(int);
		int flipMode = default(int);
		if (b == 2)
		{
			if (value != null)
			{
				levelNumber = value.LevelNumber;
				flipMode = value.FlipMode;
				((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref levelNumber);
				((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref flipMode);
				goto IL_0096;
			}
			((MemoryPackReader)(ref reader)).ReadUnmanaged<int, int>(ref levelNumber, ref flipMode);
		}
		else
		{
			if (b > 2)
			{
				MemoryPackSerializationException.ThrowInvalidPropertyCount(typeof(RingLevelFlipSaveData), (byte)2, b);
				return;
			}
			if (value == null)
			{
				levelNumber = 0;
				flipMode = 0;
			}
			else
			{
				levelNumber = value.LevelNumber;
				flipMode = value.FlipMode;
			}
			if (b != 0)
			{
				((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref levelNumber);
				if (b != 1)
				{
					((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref flipMode);
					_ = 2;
				}
			}
			if (value != null)
			{
				goto IL_0096;
			}
		}
		value = new RingLevelFlipSaveData
		{
			LevelNumber = levelNumber,
			FlipMode = flipMode
		};
		return;
		IL_0096:
		value.LevelNumber = levelNumber;
		value.FlipMode = flipMode;
	}
}
