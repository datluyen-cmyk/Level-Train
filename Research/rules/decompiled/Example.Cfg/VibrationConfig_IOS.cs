using System.Collections.Generic;
using MemoryPack;
using MemoryPack.Formatters;
using MemoryPack.Internal;

namespace Example.Cfg;

[MemoryPackable(/*Could not decode attribute arguments.*/)]
public class VibrationConfig_IOS : IMemoryPackable<VibrationConfig_IOS>, IMemoryPackFormatterRegister
{
	[Preserve]
	private sealed class VibrationConfig_IOSFormatter : MemoryPackFormatter<VibrationConfig_IOS>
	{
		[Preserve]
		public override void Serialize(ref MemoryPackWriter writer, ref VibrationConfig_IOS value)
		{
			VibrationConfig_IOS.Serialize(ref writer, ref value);
		}

		[Preserve]
		public override void Deserialize(ref MemoryPackReader reader, ref VibrationConfig_IOS value)
		{
			VibrationConfig_IOS.Deserialize(ref reader, ref value);
		}
	}

	[MemoryPackOrder(0)]
	public int ID { get; private set; }

	[MemoryPackOrder(1)]
	public List<int> AppleMod { get; private set; }

	[MemoryPackOrder(2)]
	public List<int> AppleModGE13 { get; private set; }

	[MemoryPackOrder(3)]
	public int Priority { get; private set; }

	[MemoryPackOrder(4)]
	public List<int> VibrationContinued { get; private set; }

	static VibrationConfig_IOS()
	{
		RegisterFormatter();
	}

	[Preserve]
	public static void RegisterFormatter()
	{
		if (!MemoryPackFormatterProvider.IsRegistered<VibrationConfig_IOS>())
		{
			MemoryPackFormatterProvider.Register<VibrationConfig_IOS>((MemoryPackFormatter<VibrationConfig_IOS>)(object)new VibrationConfig_IOSFormatter());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<VibrationConfig_IOS[]>())
		{
			MemoryPackFormatterProvider.Register<VibrationConfig_IOS[]>((MemoryPackFormatter<VibrationConfig_IOS[]>)(object)new ArrayFormatter<VibrationConfig_IOS>());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<List<int>>())
		{
			MemoryPackFormatterProvider.Register<List<int>>((MemoryPackFormatter<List<int>>)(object)new ListFormatter<int>());
		}
	}

	[Preserve]
	public static void Serialize(ref MemoryPackWriter writer, ref VibrationConfig_IOS? value)
	{
		if (value == null)
		{
			((MemoryPackWriter)(ref writer)).WriteNullObjectHeader();
			return;
		}
		int iD = value.ID;
		((MemoryPackWriter)(ref writer)).WriteUnmanagedWithObjectHeader<int>((byte)5, ref iD);
		List<int> appleMod = value.AppleMod;
		((MemoryPackWriter)(ref writer)).WriteValue<List<int>>(ref appleMod);
		appleMod = value.AppleModGE13;
		((MemoryPackWriter)(ref writer)).WriteValue<List<int>>(ref appleMod);
		iD = value.Priority;
		((MemoryPackWriter)(ref writer)).WriteUnmanaged<int>(ref iD);
		appleMod = value.VibrationContinued;
		((MemoryPackWriter)(ref writer)).WriteValue<List<int>>(ref appleMod);
	}

	[Preserve]
	public static void Deserialize(ref MemoryPackReader reader, ref VibrationConfig_IOS? value)
	{
		byte b = default(byte);
		if (!((MemoryPackReader)(ref reader)).TryReadObjectHeader(ref b))
		{
			value = null;
			return;
		}
		int iD = default(int);
		List<int> appleMod;
		List<int> appleModGE;
		int priority = default(int);
		List<int> vibrationContinued;
		if (b == 5)
		{
			if (value != null)
			{
				iD = value.ID;
				appleMod = value.AppleMod;
				appleModGE = value.AppleModGE13;
				priority = value.Priority;
				vibrationContinued = value.VibrationContinued;
				((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref iD);
				((MemoryPackReader)(ref reader)).ReadValue<List<int>>(ref appleMod);
				((MemoryPackReader)(ref reader)).ReadValue<List<int>>(ref appleModGE);
				((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref priority);
				((MemoryPackReader)(ref reader)).ReadValue<List<int>>(ref vibrationContinued);
				goto IL_0130;
			}
			((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref iD);
			appleMod = ((MemoryPackReader)(ref reader)).ReadValue<List<int>>();
			appleModGE = ((MemoryPackReader)(ref reader)).ReadValue<List<int>>();
			((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref priority);
			vibrationContinued = ((MemoryPackReader)(ref reader)).ReadValue<List<int>>();
		}
		else
		{
			if (b > 5)
			{
				MemoryPackSerializationException.ThrowInvalidPropertyCount(typeof(VibrationConfig_IOS), (byte)5, b);
				return;
			}
			if (value == null)
			{
				iD = 0;
				appleMod = null;
				appleModGE = null;
				priority = 0;
				vibrationContinued = null;
			}
			else
			{
				iD = value.ID;
				appleMod = value.AppleMod;
				appleModGE = value.AppleModGE13;
				priority = value.Priority;
				vibrationContinued = value.VibrationContinued;
			}
			if (b != 0)
			{
				((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref iD);
				if (b != 1)
				{
					((MemoryPackReader)(ref reader)).ReadValue<List<int>>(ref appleMod);
					if (b != 2)
					{
						((MemoryPackReader)(ref reader)).ReadValue<List<int>>(ref appleModGE);
						if (b != 3)
						{
							((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref priority);
							if (b != 4)
							{
								((MemoryPackReader)(ref reader)).ReadValue<List<int>>(ref vibrationContinued);
								_ = 5;
							}
						}
					}
				}
			}
			if (value != null)
			{
				goto IL_0130;
			}
		}
		value = new VibrationConfig_IOS
		{
			ID = iD,
			AppleMod = appleMod,
			AppleModGE13 = appleModGE,
			Priority = priority,
			VibrationContinued = vibrationContinued
		};
		return;
		IL_0130:
		value.ID = iD;
		value.AppleMod = appleMod;
		value.AppleModGE13 = appleModGE;
		value.Priority = priority;
		value.VibrationContinued = vibrationContinued;
	}
}
