using MemoryPack;
using MemoryPack.Formatters;
using MemoryPack.Internal;
using XingYun.Save;

namespace RotateRings.Ad;

[MemoryPackable(/*Could not decode attribute arguments.*/)]
public class PropWinInterExemptRecord : ISaveDefaultInitializer, IMemoryPackable<PropWinInterExemptRecord>, IMemoryPackFormatterRegister
{
	[Preserve]
	private sealed class PropWinInterExemptRecordFormatter : MemoryPackFormatter<PropWinInterExemptRecord>
	{
		[Preserve]
		public override void Serialize(ref MemoryPackWriter writer, ref PropWinInterExemptRecord value)
		{
			PropWinInterExemptRecord.Serialize(ref writer, ref value);
		}

		[Preserve]
		public override void Deserialize(ref MemoryPackReader reader, ref PropWinInterExemptRecord value)
		{
			PropWinInterExemptRecord.Deserialize(ref reader, ref value);
		}
	}

	public int PendingPropWinGroup;

	public int SkipNextFrontAdGroup;

	public int BoundLevel;

	public int BoundModeNum;

	public void OnCreateDefault()
	{
		PendingPropWinGroup = 0;
		SkipNextFrontAdGroup = 0;
		BoundLevel = 0;
		BoundModeNum = 0;
	}

	static PropWinInterExemptRecord()
	{
		RegisterFormatter();
	}

	[Preserve]
	public static void RegisterFormatter()
	{
		if (!MemoryPackFormatterProvider.IsRegistered<PropWinInterExemptRecord>())
		{
			MemoryPackFormatterProvider.Register<PropWinInterExemptRecord>((MemoryPackFormatter<PropWinInterExemptRecord>)(object)new PropWinInterExemptRecordFormatter());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<PropWinInterExemptRecord[]>())
		{
			MemoryPackFormatterProvider.Register<PropWinInterExemptRecord[]>((MemoryPackFormatter<PropWinInterExemptRecord[]>)(object)new ArrayFormatter<PropWinInterExemptRecord>());
		}
	}

	[Preserve]
	public static void Serialize(ref MemoryPackWriter writer, ref PropWinInterExemptRecord? value)
	{
		if (value == null)
		{
			((MemoryPackWriter)(ref writer)).WriteNullObjectHeader();
		}
		else
		{
			((MemoryPackWriter)(ref writer)).WriteUnmanagedWithObjectHeader<int, int, int, int>((byte)4, ref value.PendingPropWinGroup, ref value.SkipNextFrontAdGroup, ref value.BoundLevel, ref value.BoundModeNum);
		}
	}

	[Preserve]
	public static void Deserialize(ref MemoryPackReader reader, ref PropWinInterExemptRecord? value)
	{
		byte b = default(byte);
		if (!((MemoryPackReader)(ref reader)).TryReadObjectHeader(ref b))
		{
			value = null;
			return;
		}
		int pendingPropWinGroup = default(int);
		int skipNextFrontAdGroup = default(int);
		int boundLevel = default(int);
		int boundModeNum = default(int);
		if (b == 4)
		{
			if (value != null)
			{
				pendingPropWinGroup = value.PendingPropWinGroup;
				skipNextFrontAdGroup = value.SkipNextFrontAdGroup;
				boundLevel = value.BoundLevel;
				boundModeNum = value.BoundModeNum;
				((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref pendingPropWinGroup);
				((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref skipNextFrontAdGroup);
				((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref boundLevel);
				((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref boundModeNum);
				goto IL_00e9;
			}
			((MemoryPackReader)(ref reader)).ReadUnmanaged<int, int, int, int>(ref pendingPropWinGroup, ref skipNextFrontAdGroup, ref boundLevel, ref boundModeNum);
		}
		else
		{
			if (b > 4)
			{
				MemoryPackSerializationException.ThrowInvalidPropertyCount(typeof(PropWinInterExemptRecord), (byte)4, b);
				return;
			}
			if (value == null)
			{
				pendingPropWinGroup = 0;
				skipNextFrontAdGroup = 0;
				boundLevel = 0;
				boundModeNum = 0;
			}
			else
			{
				pendingPropWinGroup = value.PendingPropWinGroup;
				skipNextFrontAdGroup = value.SkipNextFrontAdGroup;
				boundLevel = value.BoundLevel;
				boundModeNum = value.BoundModeNum;
			}
			if (b != 0)
			{
				((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref pendingPropWinGroup);
				if (b != 1)
				{
					((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref skipNextFrontAdGroup);
					if (b != 2)
					{
						((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref boundLevel);
						if (b != 3)
						{
							((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref boundModeNum);
							_ = 4;
						}
					}
				}
			}
			if (value != null)
			{
				goto IL_00e9;
			}
		}
		value = new PropWinInterExemptRecord
		{
			PendingPropWinGroup = pendingPropWinGroup,
			SkipNextFrontAdGroup = skipNextFrontAdGroup,
			BoundLevel = boundLevel,
			BoundModeNum = boundModeNum
		};
		return;
		IL_00e9:
		value.PendingPropWinGroup = pendingPropWinGroup;
		value.SkipNextFrontAdGroup = skipNextFrontAdGroup;
		value.BoundLevel = boundLevel;
		value.BoundModeNum = boundModeNum;
	}
}
