using System.Collections.Generic;
using MemoryPack;
using MemoryPack.Formatters;
using MemoryPack.Internal;

namespace Example.Cfg;

[MemoryPackable(/*Could not decode attribute arguments.*/)]
public class VibrationConfig_Android : IMemoryPackable<VibrationConfig_Android>, IMemoryPackFormatterRegister
{
	[Preserve]
	private sealed class VibrationConfig_AndroidFormatter : MemoryPackFormatter<VibrationConfig_Android>
	{
		[Preserve]
		public override void Serialize(ref MemoryPackWriter writer, ref VibrationConfig_Android value)
		{
			VibrationConfig_Android.Serialize(ref writer, ref value);
		}

		[Preserve]
		public override void Deserialize(ref MemoryPackReader reader, ref VibrationConfig_Android value)
		{
			VibrationConfig_Android.Deserialize(ref reader, ref value);
		}
	}

	[MemoryPackOrder(0)]
	public int ID { get; private set; }

	[MemoryPackOrder(1)]
	public List<int> NewAndroidTime { get; private set; }

	[MemoryPackOrder(2)]
	public List<int> AndroidStrong1 { get; private set; }

	[MemoryPackOrder(3)]
	public List<int> AndroidTime1 { get; private set; }

	[MemoryPackOrder(4)]
	public List<int> AndroidStrong2 { get; private set; }

	[MemoryPackOrder(5)]
	public List<int> AndroidTime2 { get; private set; }

	[MemoryPackOrder(6)]
	public List<int> AndroidStrong3 { get; private set; }

	[MemoryPackOrder(7)]
	public List<int> AndroidTime3 { get; private set; }

	[MemoryPackOrder(8)]
	public List<int> OldAndroidStrong { get; private set; }

	[MemoryPackOrder(9)]
	public List<int> OldAndroidTime { get; private set; }

	[MemoryPackOrder(10)]
	public int Priority { get; private set; }

	static VibrationConfig_Android()
	{
		RegisterFormatter();
	}

	[Preserve]
	public static void RegisterFormatter()
	{
		if (!MemoryPackFormatterProvider.IsRegistered<VibrationConfig_Android>())
		{
			MemoryPackFormatterProvider.Register<VibrationConfig_Android>((MemoryPackFormatter<VibrationConfig_Android>)(object)new VibrationConfig_AndroidFormatter());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<VibrationConfig_Android[]>())
		{
			MemoryPackFormatterProvider.Register<VibrationConfig_Android[]>((MemoryPackFormatter<VibrationConfig_Android[]>)(object)new ArrayFormatter<VibrationConfig_Android>());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<List<int>>())
		{
			MemoryPackFormatterProvider.Register<List<int>>((MemoryPackFormatter<List<int>>)(object)new ListFormatter<int>());
		}
	}

	[Preserve]
	public static void Serialize(ref MemoryPackWriter writer, ref VibrationConfig_Android? value)
	{
		if (value == null)
		{
			((MemoryPackWriter)(ref writer)).WriteNullObjectHeader();
			return;
		}
		int iD = value.ID;
		((MemoryPackWriter)(ref writer)).WriteUnmanagedWithObjectHeader<int>((byte)11, ref iD);
		List<int> newAndroidTime = value.NewAndroidTime;
		((MemoryPackWriter)(ref writer)).WriteValue<List<int>>(ref newAndroidTime);
		newAndroidTime = value.AndroidStrong1;
		((MemoryPackWriter)(ref writer)).WriteValue<List<int>>(ref newAndroidTime);
		newAndroidTime = value.AndroidTime1;
		((MemoryPackWriter)(ref writer)).WriteValue<List<int>>(ref newAndroidTime);
		newAndroidTime = value.AndroidStrong2;
		((MemoryPackWriter)(ref writer)).WriteValue<List<int>>(ref newAndroidTime);
		newAndroidTime = value.AndroidTime2;
		((MemoryPackWriter)(ref writer)).WriteValue<List<int>>(ref newAndroidTime);
		newAndroidTime = value.AndroidStrong3;
		((MemoryPackWriter)(ref writer)).WriteValue<List<int>>(ref newAndroidTime);
		newAndroidTime = value.AndroidTime3;
		((MemoryPackWriter)(ref writer)).WriteValue<List<int>>(ref newAndroidTime);
		newAndroidTime = value.OldAndroidStrong;
		((MemoryPackWriter)(ref writer)).WriteValue<List<int>>(ref newAndroidTime);
		newAndroidTime = value.OldAndroidTime;
		((MemoryPackWriter)(ref writer)).WriteValue<List<int>>(ref newAndroidTime);
		iD = value.Priority;
		((MemoryPackWriter)(ref writer)).WriteUnmanaged<int>(ref iD);
	}

	[Preserve]
	public static void Deserialize(ref MemoryPackReader reader, ref VibrationConfig_Android? value)
	{
		byte b = default(byte);
		if (!((MemoryPackReader)(ref reader)).TryReadObjectHeader(ref b))
		{
			value = null;
			return;
		}
		int iD = default(int);
		List<int> newAndroidTime;
		List<int> androidStrong;
		List<int> androidTime;
		List<int> androidStrong2;
		List<int> androidTime2;
		List<int> androidStrong3;
		List<int> androidTime3;
		List<int> oldAndroidStrong;
		List<int> oldAndroidTime;
		int priority = default(int);
		if (b == 11)
		{
			if (value != null)
			{
				iD = value.ID;
				newAndroidTime = value.NewAndroidTime;
				androidStrong = value.AndroidStrong1;
				androidTime = value.AndroidTime1;
				androidStrong2 = value.AndroidStrong2;
				androidTime2 = value.AndroidTime2;
				androidStrong3 = value.AndroidStrong3;
				androidTime3 = value.AndroidTime3;
				oldAndroidStrong = value.OldAndroidStrong;
				oldAndroidTime = value.OldAndroidTime;
				priority = value.Priority;
				((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref iD);
				((MemoryPackReader)(ref reader)).ReadValue<List<int>>(ref newAndroidTime);
				((MemoryPackReader)(ref reader)).ReadValue<List<int>>(ref androidStrong);
				((MemoryPackReader)(ref reader)).ReadValue<List<int>>(ref androidTime);
				((MemoryPackReader)(ref reader)).ReadValue<List<int>>(ref androidStrong2);
				((MemoryPackReader)(ref reader)).ReadValue<List<int>>(ref androidTime2);
				((MemoryPackReader)(ref reader)).ReadValue<List<int>>(ref androidStrong3);
				((MemoryPackReader)(ref reader)).ReadValue<List<int>>(ref androidTime3);
				((MemoryPackReader)(ref reader)).ReadValue<List<int>>(ref oldAndroidStrong);
				((MemoryPackReader)(ref reader)).ReadValue<List<int>>(ref oldAndroidTime);
				((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref priority);
				goto IL_025f;
			}
			((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref iD);
			newAndroidTime = ((MemoryPackReader)(ref reader)).ReadValue<List<int>>();
			androidStrong = ((MemoryPackReader)(ref reader)).ReadValue<List<int>>();
			androidTime = ((MemoryPackReader)(ref reader)).ReadValue<List<int>>();
			androidStrong2 = ((MemoryPackReader)(ref reader)).ReadValue<List<int>>();
			androidTime2 = ((MemoryPackReader)(ref reader)).ReadValue<List<int>>();
			androidStrong3 = ((MemoryPackReader)(ref reader)).ReadValue<List<int>>();
			androidTime3 = ((MemoryPackReader)(ref reader)).ReadValue<List<int>>();
			oldAndroidStrong = ((MemoryPackReader)(ref reader)).ReadValue<List<int>>();
			oldAndroidTime = ((MemoryPackReader)(ref reader)).ReadValue<List<int>>();
			((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref priority);
		}
		else
		{
			if (b > 11)
			{
				MemoryPackSerializationException.ThrowInvalidPropertyCount(typeof(VibrationConfig_Android), (byte)11, b);
				return;
			}
			if (value == null)
			{
				iD = 0;
				newAndroidTime = null;
				androidStrong = null;
				androidTime = null;
				androidStrong2 = null;
				androidTime2 = null;
				androidStrong3 = null;
				androidTime3 = null;
				oldAndroidStrong = null;
				oldAndroidTime = null;
				priority = 0;
			}
			else
			{
				iD = value.ID;
				newAndroidTime = value.NewAndroidTime;
				androidStrong = value.AndroidStrong1;
				androidTime = value.AndroidTime1;
				androidStrong2 = value.AndroidStrong2;
				androidTime2 = value.AndroidTime2;
				androidStrong3 = value.AndroidStrong3;
				androidTime3 = value.AndroidTime3;
				oldAndroidStrong = value.OldAndroidStrong;
				oldAndroidTime = value.OldAndroidTime;
				priority = value.Priority;
			}
			if (b != 0)
			{
				((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref iD);
				if (b != 1)
				{
					((MemoryPackReader)(ref reader)).ReadValue<List<int>>(ref newAndroidTime);
					if (b != 2)
					{
						((MemoryPackReader)(ref reader)).ReadValue<List<int>>(ref androidStrong);
						if (b != 3)
						{
							((MemoryPackReader)(ref reader)).ReadValue<List<int>>(ref androidTime);
							if (b != 4)
							{
								((MemoryPackReader)(ref reader)).ReadValue<List<int>>(ref androidStrong2);
								if (b != 5)
								{
									((MemoryPackReader)(ref reader)).ReadValue<List<int>>(ref androidTime2);
									if (b != 6)
									{
										((MemoryPackReader)(ref reader)).ReadValue<List<int>>(ref androidStrong3);
										if (b != 7)
										{
											((MemoryPackReader)(ref reader)).ReadValue<List<int>>(ref androidTime3);
											if (b != 8)
											{
												((MemoryPackReader)(ref reader)).ReadValue<List<int>>(ref oldAndroidStrong);
												if (b != 9)
												{
													((MemoryPackReader)(ref reader)).ReadValue<List<int>>(ref oldAndroidTime);
													if (b != 10)
													{
														((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref priority);
														_ = 11;
													}
												}
											}
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
				goto IL_025f;
			}
		}
		value = new VibrationConfig_Android
		{
			ID = iD,
			NewAndroidTime = newAndroidTime,
			AndroidStrong1 = androidStrong,
			AndroidTime1 = androidTime,
			AndroidStrong2 = androidStrong2,
			AndroidTime2 = androidTime2,
			AndroidStrong3 = androidStrong3,
			AndroidTime3 = androidTime3,
			OldAndroidStrong = oldAndroidStrong,
			OldAndroidTime = oldAndroidTime,
			Priority = priority
		};
		return;
		IL_025f:
		value.ID = iD;
		value.NewAndroidTime = newAndroidTime;
		value.AndroidStrong1 = androidStrong;
		value.AndroidTime1 = androidTime;
		value.AndroidStrong2 = androidStrong2;
		value.AndroidTime2 = androidTime2;
		value.AndroidStrong3 = androidStrong3;
		value.AndroidTime3 = androidTime3;
		value.OldAndroidStrong = oldAndroidStrong;
		value.OldAndroidTime = oldAndroidTime;
		value.Priority = priority;
	}
}
