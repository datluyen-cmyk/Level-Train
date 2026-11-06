using MemoryPack;
using MemoryPack.Formatters;
using MemoryPack.Internal;

namespace RotateRings.GamePlay.Save;

[MemoryPackable(/*Could not decode attribute arguments.*/)]
public class RotateRingPartSave : IMemoryPackable<RotateRingPartSave>, IMemoryPackFormatterRegister
{
	public enum PartKindType : byte
	{
		Ring,
		Pull,
		Fixed
	}

	[Preserve]
	private sealed class RotateRingPartSaveFormatter : MemoryPackFormatter<RotateRingPartSave>
	{
		[Preserve]
		public override void Serialize(ref MemoryPackWriter writer, ref RotateRingPartSave value)
		{
			RotateRingPartSave.Serialize(ref writer, ref value);
		}

		[Preserve]
		public override void Deserialize(ref MemoryPackReader reader, ref RotateRingPartSave value)
		{
			RotateRingPartSave.Deserialize(ref reader, ref value);
		}
	}

	public string PartId;

	public PartKindType PartKind;

	public bool IsSolved;

	public float RotationZ;

	public float PosX;

	public float PosY;

	public int LockCount;

	static RotateRingPartSave()
	{
		RegisterFormatter();
	}

	[Preserve]
	public static void RegisterFormatter()
	{
		if (!MemoryPackFormatterProvider.IsRegistered<RotateRingPartSave>())
		{
			MemoryPackFormatterProvider.Register<RotateRingPartSave>((MemoryPackFormatter<RotateRingPartSave>)(object)new RotateRingPartSaveFormatter());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<RotateRingPartSave[]>())
		{
			MemoryPackFormatterProvider.Register<RotateRingPartSave[]>((MemoryPackFormatter<RotateRingPartSave[]>)(object)new ArrayFormatter<RotateRingPartSave>());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<PartKindType>())
		{
			MemoryPackFormatterProvider.Register<PartKindType>((MemoryPackFormatter<PartKindType>)(object)new UnmanagedFormatter<PartKindType>());
		}
	}

	[Preserve]
	public static void Serialize(ref MemoryPackWriter writer, ref RotateRingPartSave? value)
	{
		if (value == null)
		{
			((MemoryPackWriter)(ref writer)).WriteNullObjectHeader();
			return;
		}
		((MemoryPackWriter)(ref writer)).WriteObjectHeader((byte)7);
		((MemoryPackWriter)(ref writer)).WriteString(value.PartId);
		((MemoryPackWriter)(ref writer)).WriteUnmanaged<PartKindType, bool, float, float, float, int>(ref value.PartKind, ref value.IsSolved, ref value.RotationZ, ref value.PosX, ref value.PosY, ref value.LockCount);
	}

	[Preserve]
	public static void Deserialize(ref MemoryPackReader reader, ref RotateRingPartSave? value)
	{
		byte b = default(byte);
		if (!((MemoryPackReader)(ref reader)).TryReadObjectHeader(ref b))
		{
			value = null;
			return;
		}
		PartKindType partKind = default(PartKindType);
		bool isSolved = default(bool);
		float rotationZ = default(float);
		float posX = default(float);
		float posY = default(float);
		int lockCount = default(int);
		string partId;
		if (b == 7)
		{
			if (value != null)
			{
				partId = value.PartId;
				partKind = value.PartKind;
				isSolved = value.IsSolved;
				rotationZ = value.RotationZ;
				posX = value.PosX;
				posY = value.PosY;
				lockCount = value.LockCount;
				partId = ((MemoryPackReader)(ref reader)).ReadString();
				((MemoryPackReader)(ref reader)).ReadUnmanaged<PartKindType>(ref partKind);
				((MemoryPackReader)(ref reader)).ReadUnmanaged<bool>(ref isSolved);
				((MemoryPackReader)(ref reader)).ReadUnmanaged<float>(ref rotationZ);
				((MemoryPackReader)(ref reader)).ReadUnmanaged<float>(ref posX);
				((MemoryPackReader)(ref reader)).ReadUnmanaged<float>(ref posY);
				((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref lockCount);
				goto IL_017f;
			}
			partId = ((MemoryPackReader)(ref reader)).ReadString();
			((MemoryPackReader)(ref reader)).ReadUnmanaged<PartKindType, bool, float, float, float, int>(ref partKind, ref isSolved, ref rotationZ, ref posX, ref posY, ref lockCount);
		}
		else
		{
			if (b > 7)
			{
				MemoryPackSerializationException.ThrowInvalidPropertyCount(typeof(RotateRingPartSave), (byte)7, b);
				return;
			}
			if (value == null)
			{
				partId = null;
				partKind = PartKindType.Ring;
				isSolved = false;
				rotationZ = 0f;
				posX = 0f;
				posY = 0f;
				lockCount = 0;
			}
			else
			{
				partId = value.PartId;
				partKind = value.PartKind;
				isSolved = value.IsSolved;
				rotationZ = value.RotationZ;
				posX = value.PosX;
				posY = value.PosY;
				lockCount = value.LockCount;
			}
			if (b != 0)
			{
				partId = ((MemoryPackReader)(ref reader)).ReadString();
				if (b != 1)
				{
					((MemoryPackReader)(ref reader)).ReadUnmanaged<PartKindType>(ref partKind);
					if (b != 2)
					{
						((MemoryPackReader)(ref reader)).ReadUnmanaged<bool>(ref isSolved);
						if (b != 3)
						{
							((MemoryPackReader)(ref reader)).ReadUnmanaged<float>(ref rotationZ);
							if (b != 4)
							{
								((MemoryPackReader)(ref reader)).ReadUnmanaged<float>(ref posX);
								if (b != 5)
								{
									((MemoryPackReader)(ref reader)).ReadUnmanaged<float>(ref posY);
									if (b != 6)
									{
										((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref lockCount);
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
				goto IL_017f;
			}
		}
		value = new RotateRingPartSave
		{
			PartId = partId,
			PartKind = partKind,
			IsSolved = isSolved,
			RotationZ = rotationZ,
			PosX = posX,
			PosY = posY,
			LockCount = lockCount
		};
		return;
		IL_017f:
		value.PartId = partId;
		value.PartKind = partKind;
		value.IsSolved = isSolved;
		value.RotationZ = rotationZ;
		value.PosX = posX;
		value.PosY = posY;
		value.LockCount = lockCount;
	}
}
