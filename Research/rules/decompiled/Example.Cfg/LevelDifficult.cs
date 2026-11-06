using System.Collections.Generic;
using MemoryPack;
using MemoryPack.Formatters;
using MemoryPack.Internal;

namespace Example.Cfg;

[MemoryPackable(/*Could not decode attribute arguments.*/)]
public class LevelDifficult : IMemoryPackable<LevelDifficult>, IMemoryPackFormatterRegister
{
	[Preserve]
	private sealed class LevelDifficultFormatter : MemoryPackFormatter<LevelDifficult>
	{
		[Preserve]
		public override void Serialize(ref MemoryPackWriter writer, ref LevelDifficult value)
		{
			LevelDifficult.Serialize(ref writer, ref value);
		}

		[Preserve]
		public override void Deserialize(ref MemoryPackReader reader, ref LevelDifficult value)
		{
			LevelDifficult.Deserialize(ref reader, ref value);
		}
	}

	[MemoryPackOrder(0)]
	public int Id { get; private set; }

	[MemoryPackOrder(1)]
	public int GameMode { get; private set; }

	[MemoryPackOrder(2)]
	public int Begin { get; private set; }

	[MemoryPackOrder(3)]
	public int End { get; private set; }

	[MemoryPackOrder(4)]
	public int Divisor { get; private set; }

	[MemoryPackOrder(5)]
	public List<int> Remainder { get; private set; }

	[MemoryPackOrder(6)]
	public int DifficultMode { get; private set; }

	static LevelDifficult()
	{
		RegisterFormatter();
	}

	[Preserve]
	public static void RegisterFormatter()
	{
		if (!MemoryPackFormatterProvider.IsRegistered<LevelDifficult>())
		{
			MemoryPackFormatterProvider.Register<LevelDifficult>((MemoryPackFormatter<LevelDifficult>)(object)new LevelDifficultFormatter());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<LevelDifficult[]>())
		{
			MemoryPackFormatterProvider.Register<LevelDifficult[]>((MemoryPackFormatter<LevelDifficult[]>)(object)new ArrayFormatter<LevelDifficult>());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<List<int>>())
		{
			MemoryPackFormatterProvider.Register<List<int>>((MemoryPackFormatter<List<int>>)(object)new ListFormatter<int>());
		}
	}

	[Preserve]
	public static void Serialize(ref MemoryPackWriter writer, ref LevelDifficult? value)
	{
		if (value == null)
		{
			((MemoryPackWriter)(ref writer)).WriteNullObjectHeader();
			return;
		}
		int id = value.Id;
		int gameMode = value.GameMode;
		int begin = value.Begin;
		int end = value.End;
		int divisor = value.Divisor;
		((MemoryPackWriter)(ref writer)).WriteUnmanagedWithObjectHeader<int, int, int, int, int>((byte)7, ref id, ref gameMode, ref begin, ref end, ref divisor);
		List<int> remainder = value.Remainder;
		((MemoryPackWriter)(ref writer)).WriteValue<List<int>>(ref remainder);
		id = value.DifficultMode;
		((MemoryPackWriter)(ref writer)).WriteUnmanaged<int>(ref id);
	}

	[Preserve]
	public static void Deserialize(ref MemoryPackReader reader, ref LevelDifficult? value)
	{
		byte b = default(byte);
		if (!((MemoryPackReader)(ref reader)).TryReadObjectHeader(ref b))
		{
			value = null;
			return;
		}
		int id = default(int);
		int gameMode = default(int);
		int begin = default(int);
		int end = default(int);
		int divisor = default(int);
		List<int> remainder;
		int difficultMode = default(int);
		if (b == 7)
		{
			if (value != null)
			{
				id = value.Id;
				gameMode = value.GameMode;
				begin = value.Begin;
				end = value.End;
				divisor = value.Divisor;
				remainder = value.Remainder;
				difficultMode = value.DifficultMode;
				((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref id);
				((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref gameMode);
				((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref begin);
				((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref end);
				((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref divisor);
				((MemoryPackReader)(ref reader)).ReadValue<List<int>>(ref remainder);
				((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref difficultMode);
				goto IL_017c;
			}
			((MemoryPackReader)(ref reader)).ReadUnmanaged<int, int, int, int, int>(ref id, ref gameMode, ref begin, ref end, ref divisor);
			remainder = ((MemoryPackReader)(ref reader)).ReadValue<List<int>>();
			((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref difficultMode);
		}
		else
		{
			if (b > 7)
			{
				MemoryPackSerializationException.ThrowInvalidPropertyCount(typeof(LevelDifficult), (byte)7, b);
				return;
			}
			if (value == null)
			{
				id = 0;
				gameMode = 0;
				begin = 0;
				end = 0;
				divisor = 0;
				remainder = null;
				difficultMode = 0;
			}
			else
			{
				id = value.Id;
				gameMode = value.GameMode;
				begin = value.Begin;
				end = value.End;
				divisor = value.Divisor;
				remainder = value.Remainder;
				difficultMode = value.DifficultMode;
			}
			if (b != 0)
			{
				((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref id);
				if (b != 1)
				{
					((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref gameMode);
					if (b != 2)
					{
						((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref begin);
						if (b != 3)
						{
							((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref end);
							if (b != 4)
							{
								((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref divisor);
								if (b != 5)
								{
									((MemoryPackReader)(ref reader)).ReadValue<List<int>>(ref remainder);
									if (b != 6)
									{
										((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref difficultMode);
										_ = 7;
									}
								}
							}
						}
					}
				}
			}
			if (value != null)
			{
				goto IL_017c;
			}
		}
		value = new LevelDifficult
		{
			Id = id,
			GameMode = gameMode,
			Begin = begin,
			End = end,
			Divisor = divisor,
			Remainder = remainder,
			DifficultMode = difficultMode
		};
		return;
		IL_017c:
		value.Id = id;
		value.GameMode = gameMode;
		value.Begin = begin;
		value.End = end;
		value.Divisor = divisor;
		value.Remainder = remainder;
		value.DifficultMode = difficultMode;
	}
}
