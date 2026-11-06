using System.Collections.Generic;
using MemoryPack;
using MemoryPack.Formatters;
using MemoryPack.Internal;

namespace Example.Cfg;

[MemoryPackable(/*Could not decode attribute arguments.*/)]
public class DailyFirstWinPuzzleConfig : IMemoryPackable<DailyFirstWinPuzzleConfig>, IMemoryPackFormatterRegister
{
	[Preserve]
	private sealed class DailyFirstWinPuzzleConfigFormatter : MemoryPackFormatter<DailyFirstWinPuzzleConfig>
	{
		[Preserve]
		public override void Serialize(ref MemoryPackWriter writer, ref DailyFirstWinPuzzleConfig value)
		{
			DailyFirstWinPuzzleConfig.Serialize(ref writer, ref value);
		}

		[Preserve]
		public override void Deserialize(ref MemoryPackReader reader, ref DailyFirstWinPuzzleConfig value)
		{
			DailyFirstWinPuzzleConfig.Deserialize(ref reader, ref value);
		}
	}

	[MemoryPackOrder(0)]
	public int Id { get; private set; }

	[MemoryPackOrder(1)]
	public string CompletedImage { get; private set; }

	[MemoryPackOrder(2)]
	public string OutlineImage { get; private set; }

	[MemoryPackOrder(3)]
	public List<string> Pieces { get; private set; }

	static DailyFirstWinPuzzleConfig()
	{
		RegisterFormatter();
	}

	[Preserve]
	public static void RegisterFormatter()
	{
		if (!MemoryPackFormatterProvider.IsRegistered<DailyFirstWinPuzzleConfig>())
		{
			MemoryPackFormatterProvider.Register<DailyFirstWinPuzzleConfig>((MemoryPackFormatter<DailyFirstWinPuzzleConfig>)(object)new DailyFirstWinPuzzleConfigFormatter());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<DailyFirstWinPuzzleConfig[]>())
		{
			MemoryPackFormatterProvider.Register<DailyFirstWinPuzzleConfig[]>((MemoryPackFormatter<DailyFirstWinPuzzleConfig[]>)(object)new ArrayFormatter<DailyFirstWinPuzzleConfig>());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<List<string>>())
		{
			MemoryPackFormatterProvider.Register<List<string>>((MemoryPackFormatter<List<string>>)(object)new ListFormatter<string>());
		}
	}

	[Preserve]
	public static void Serialize(ref MemoryPackWriter writer, ref DailyFirstWinPuzzleConfig? value)
	{
		if (value == null)
		{
			((MemoryPackWriter)(ref writer)).WriteNullObjectHeader();
			return;
		}
		int id = value.Id;
		((MemoryPackWriter)(ref writer)).WriteUnmanagedWithObjectHeader<int>((byte)4, ref id);
		((MemoryPackWriter)(ref writer)).WriteString(value.CompletedImage);
		((MemoryPackWriter)(ref writer)).WriteString(value.OutlineImage);
		List<string> pieces = value.Pieces;
		((MemoryPackWriter)(ref writer)).WriteValue<List<string>>(ref pieces);
	}

	[Preserve]
	public static void Deserialize(ref MemoryPackReader reader, ref DailyFirstWinPuzzleConfig? value)
	{
		byte b = default(byte);
		if (!((MemoryPackReader)(ref reader)).TryReadObjectHeader(ref b))
		{
			value = null;
			return;
		}
		int id = default(int);
		List<string> pieces;
		string completedImage;
		string outlineImage;
		if (b == 4)
		{
			if (value != null)
			{
				id = value.Id;
				completedImage = value.CompletedImage;
				outlineImage = value.OutlineImage;
				pieces = value.Pieces;
				((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref id);
				completedImage = ((MemoryPackReader)(ref reader)).ReadString();
				outlineImage = ((MemoryPackReader)(ref reader)).ReadString();
				((MemoryPackReader)(ref reader)).ReadValue<List<string>>(ref pieces);
				goto IL_00f5;
			}
			((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref id);
			completedImage = ((MemoryPackReader)(ref reader)).ReadString();
			outlineImage = ((MemoryPackReader)(ref reader)).ReadString();
			pieces = ((MemoryPackReader)(ref reader)).ReadValue<List<string>>();
		}
		else
		{
			if (b > 4)
			{
				MemoryPackSerializationException.ThrowInvalidPropertyCount(typeof(DailyFirstWinPuzzleConfig), (byte)4, b);
				return;
			}
			if (value == null)
			{
				id = 0;
				completedImage = null;
				outlineImage = null;
				pieces = null;
			}
			else
			{
				id = value.Id;
				completedImage = value.CompletedImage;
				outlineImage = value.OutlineImage;
				pieces = value.Pieces;
			}
			if (b != 0)
			{
				((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref id);
				if (b != 1)
				{
					completedImage = ((MemoryPackReader)(ref reader)).ReadString();
					if (b != 2)
					{
						outlineImage = ((MemoryPackReader)(ref reader)).ReadString();
						if (b != 3)
						{
							((MemoryPackReader)(ref reader)).ReadValue<List<string>>(ref pieces);
							_ = 4;
						}
					}
				}
			}
			if (value != null)
			{
				goto IL_00f5;
			}
		}
		value = new DailyFirstWinPuzzleConfig
		{
			Id = id,
			CompletedImage = completedImage,
			OutlineImage = outlineImage,
			Pieces = pieces
		};
		return;
		IL_00f5:
		value.Id = id;
		value.CompletedImage = completedImage;
		value.OutlineImage = outlineImage;
		value.Pieces = pieces;
	}
}
