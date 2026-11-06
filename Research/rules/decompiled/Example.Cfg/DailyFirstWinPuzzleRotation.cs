using System.Collections.Generic;
using MemoryPack;
using MemoryPack.Formatters;
using MemoryPack.Internal;

namespace Example.Cfg;

[MemoryPackable(/*Could not decode attribute arguments.*/)]
public class DailyFirstWinPuzzleRotation : IMemoryPackable<DailyFirstWinPuzzleRotation>, IMemoryPackFormatterRegister
{
	[Preserve]
	private sealed class DailyFirstWinPuzzleRotationFormatter : MemoryPackFormatter<DailyFirstWinPuzzleRotation>
	{
		[Preserve]
		public override void Serialize(ref MemoryPackWriter writer, ref DailyFirstWinPuzzleRotation value)
		{
			DailyFirstWinPuzzleRotation.Serialize(ref writer, ref value);
		}

		[Preserve]
		public override void Deserialize(ref MemoryPackReader reader, ref DailyFirstWinPuzzleRotation value)
		{
			DailyFirstWinPuzzleRotation.Deserialize(ref reader, ref value);
		}
	}

	[MemoryPackOrder(0)]
	public int Id { get; private set; }

	[MemoryPackOrder(1)]
	public string Theme { get; private set; }

	[MemoryPackOrder(2)]
	public string Mode { get; private set; }

	[MemoryPackOrder(3)]
	public int FixedPuzzleId { get; private set; }

	[MemoryPackOrder(4)]
	public List<int> PoolIds { get; private set; }

	[MemoryPackOrder(5)]
	public int PuzzlesPerCycle { get; private set; }

	static DailyFirstWinPuzzleRotation()
	{
		RegisterFormatter();
	}

	[Preserve]
	public static void RegisterFormatter()
	{
		if (!MemoryPackFormatterProvider.IsRegistered<DailyFirstWinPuzzleRotation>())
		{
			MemoryPackFormatterProvider.Register<DailyFirstWinPuzzleRotation>((MemoryPackFormatter<DailyFirstWinPuzzleRotation>)(object)new DailyFirstWinPuzzleRotationFormatter());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<DailyFirstWinPuzzleRotation[]>())
		{
			MemoryPackFormatterProvider.Register<DailyFirstWinPuzzleRotation[]>((MemoryPackFormatter<DailyFirstWinPuzzleRotation[]>)(object)new ArrayFormatter<DailyFirstWinPuzzleRotation>());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<List<int>>())
		{
			MemoryPackFormatterProvider.Register<List<int>>((MemoryPackFormatter<List<int>>)(object)new ListFormatter<int>());
		}
	}

	[Preserve]
	public static void Serialize(ref MemoryPackWriter writer, ref DailyFirstWinPuzzleRotation? value)
	{
		if (value == null)
		{
			((MemoryPackWriter)(ref writer)).WriteNullObjectHeader();
			return;
		}
		int id = value.Id;
		((MemoryPackWriter)(ref writer)).WriteUnmanagedWithObjectHeader<int>((byte)6, ref id);
		((MemoryPackWriter)(ref writer)).WriteString(value.Theme);
		((MemoryPackWriter)(ref writer)).WriteString(value.Mode);
		id = value.FixedPuzzleId;
		((MemoryPackWriter)(ref writer)).WriteUnmanaged<int>(ref id);
		List<int> poolIds = value.PoolIds;
		((MemoryPackWriter)(ref writer)).WriteValue<List<int>>(ref poolIds);
		id = value.PuzzlesPerCycle;
		((MemoryPackWriter)(ref writer)).WriteUnmanaged<int>(ref id);
	}

	[Preserve]
	public static void Deserialize(ref MemoryPackReader reader, ref DailyFirstWinPuzzleRotation? value)
	{
		byte b = default(byte);
		if (!((MemoryPackReader)(ref reader)).TryReadObjectHeader(ref b))
		{
			value = null;
			return;
		}
		int id = default(int);
		int fixedPuzzleId = default(int);
		List<int> poolIds;
		int puzzlesPerCycle = default(int);
		string theme;
		string mode;
		if (b == 6)
		{
			if (value != null)
			{
				id = value.Id;
				theme = value.Theme;
				mode = value.Mode;
				fixedPuzzleId = value.FixedPuzzleId;
				poolIds = value.PoolIds;
				puzzlesPerCycle = value.PuzzlesPerCycle;
				((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref id);
				theme = ((MemoryPackReader)(ref reader)).ReadString();
				mode = ((MemoryPackReader)(ref reader)).ReadString();
				((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref fixedPuzzleId);
				((MemoryPackReader)(ref reader)).ReadValue<List<int>>(ref poolIds);
				((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref puzzlesPerCycle);
				goto IL_015d;
			}
			((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref id);
			theme = ((MemoryPackReader)(ref reader)).ReadString();
			mode = ((MemoryPackReader)(ref reader)).ReadString();
			((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref fixedPuzzleId);
			poolIds = ((MemoryPackReader)(ref reader)).ReadValue<List<int>>();
			((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref puzzlesPerCycle);
		}
		else
		{
			if (b > 6)
			{
				MemoryPackSerializationException.ThrowInvalidPropertyCount(typeof(DailyFirstWinPuzzleRotation), (byte)6, b);
				return;
			}
			if (value == null)
			{
				id = 0;
				theme = null;
				mode = null;
				fixedPuzzleId = 0;
				poolIds = null;
				puzzlesPerCycle = 0;
			}
			else
			{
				id = value.Id;
				theme = value.Theme;
				mode = value.Mode;
				fixedPuzzleId = value.FixedPuzzleId;
				poolIds = value.PoolIds;
				puzzlesPerCycle = value.PuzzlesPerCycle;
			}
			if (b != 0)
			{
				((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref id);
				if (b != 1)
				{
					theme = ((MemoryPackReader)(ref reader)).ReadString();
					if (b != 2)
					{
						mode = ((MemoryPackReader)(ref reader)).ReadString();
						if (b != 3)
						{
							((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref fixedPuzzleId);
							if (b != 4)
							{
								((MemoryPackReader)(ref reader)).ReadValue<List<int>>(ref poolIds);
								if (b != 5)
								{
									((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref puzzlesPerCycle);
									_ = 6;
								}
							}
						}
					}
				}
			}
			if (value != null)
			{
				goto IL_015d;
			}
		}
		value = new DailyFirstWinPuzzleRotation
		{
			Id = id,
			Theme = theme,
			Mode = mode,
			FixedPuzzleId = fixedPuzzleId,
			PoolIds = poolIds,
			PuzzlesPerCycle = puzzlesPerCycle
		};
		return;
		IL_015d:
		value.Id = id;
		value.Theme = theme;
		value.Mode = mode;
		value.FixedPuzzleId = fixedPuzzleId;
		value.PoolIds = poolIds;
		value.PuzzlesPerCycle = puzzlesPerCycle;
	}
}
