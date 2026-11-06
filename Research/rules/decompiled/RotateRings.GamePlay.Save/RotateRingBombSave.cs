using MemoryPack;
using MemoryPack.Formatters;
using MemoryPack.Internal;

namespace RotateRings.GamePlay.Save;

[MemoryPackable(/*Could not decode attribute arguments.*/)]
public class RotateRingBombSave : IMemoryPackable<RotateRingBombSave>, IMemoryPackFormatterRegister
{
	[Preserve]
	private sealed class RotateRingBombSaveFormatter : MemoryPackFormatter<RotateRingBombSave>
	{
		[Preserve]
		public override void Serialize(ref MemoryPackWriter writer, ref RotateRingBombSave value)
		{
			RotateRingBombSave.Serialize(ref writer, ref value);
		}

		[Preserve]
		public override void Deserialize(ref MemoryPackReader reader, ref RotateRingBombSave value)
		{
			RotateRingBombSave.Deserialize(ref reader, ref value);
		}
	}

	public string BombId;

	public bool IsSolved;

	static RotateRingBombSave()
	{
		RegisterFormatter();
	}

	[Preserve]
	public static void RegisterFormatter()
	{
		if (!MemoryPackFormatterProvider.IsRegistered<RotateRingBombSave>())
		{
			MemoryPackFormatterProvider.Register<RotateRingBombSave>((MemoryPackFormatter<RotateRingBombSave>)(object)new RotateRingBombSaveFormatter());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<RotateRingBombSave[]>())
		{
			MemoryPackFormatterProvider.Register<RotateRingBombSave[]>((MemoryPackFormatter<RotateRingBombSave[]>)(object)new ArrayFormatter<RotateRingBombSave>());
		}
	}

	[Preserve]
	public static void Serialize(ref MemoryPackWriter writer, ref RotateRingBombSave? value)
	{
		if (value == null)
		{
			((MemoryPackWriter)(ref writer)).WriteNullObjectHeader();
			return;
		}
		((MemoryPackWriter)(ref writer)).WriteObjectHeader((byte)2);
		((MemoryPackWriter)(ref writer)).WriteString(value.BombId);
		((MemoryPackWriter)(ref writer)).WriteUnmanaged<bool>(ref value.IsSolved);
	}

	[Preserve]
	public static void Deserialize(ref MemoryPackReader reader, ref RotateRingBombSave? value)
	{
		byte b = default(byte);
		if (!((MemoryPackReader)(ref reader)).TryReadObjectHeader(ref b))
		{
			value = null;
			return;
		}
		bool isSolved = default(bool);
		string bombId;
		if (b == 2)
		{
			if (value != null)
			{
				bombId = value.BombId;
				isSolved = value.IsSolved;
				bombId = ((MemoryPackReader)(ref reader)).ReadString();
				((MemoryPackReader)(ref reader)).ReadUnmanaged<bool>(ref isSolved);
				goto IL_0099;
			}
			bombId = ((MemoryPackReader)(ref reader)).ReadString();
			((MemoryPackReader)(ref reader)).ReadUnmanaged<bool>(ref isSolved);
		}
		else
		{
			if (b > 2)
			{
				MemoryPackSerializationException.ThrowInvalidPropertyCount(typeof(RotateRingBombSave), (byte)2, b);
				return;
			}
			if (value == null)
			{
				bombId = null;
				isSolved = false;
			}
			else
			{
				bombId = value.BombId;
				isSolved = value.IsSolved;
			}
			if (b != 0)
			{
				bombId = ((MemoryPackReader)(ref reader)).ReadString();
				if (b != 1)
				{
					((MemoryPackReader)(ref reader)).ReadUnmanaged<bool>(ref isSolved);
					_ = 2;
				}
			}
			if (value != null)
			{
				goto IL_0099;
			}
		}
		value = new RotateRingBombSave
		{
			BombId = bombId,
			IsSolved = isSolved
		};
		return;
		IL_0099:
		value.BombId = bombId;
		value.IsSolved = isSolved;
	}
}
