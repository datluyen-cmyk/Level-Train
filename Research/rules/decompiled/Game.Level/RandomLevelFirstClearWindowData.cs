using System.Collections.Generic;
using MemoryPack;
using MemoryPack.Formatters;
using MemoryPack.Internal;

namespace Game.Level;

[MemoryPackable(/*Could not decode attribute arguments.*/)]
public class RandomLevelFirstClearWindowData : IMemoryPackable<RandomLevelFirstClearWindowData>, IMemoryPackFormatterRegister
{
	[Preserve]
	private sealed class RandomLevelFirstClearWindowDataFormatter : MemoryPackFormatter<RandomLevelFirstClearWindowData>
	{
		[Preserve]
		public override void Serialize(ref MemoryPackWriter writer, ref RandomLevelFirstClearWindowData value)
		{
			RandomLevelFirstClearWindowData.Serialize(ref writer, ref value);
		}

		[Preserve]
		public override void Deserialize(ref MemoryPackReader reader, ref RandomLevelFirstClearWindowData value)
		{
			RandomLevelFirstClearWindowData.Deserialize(ref reader, ref value);
		}
	}

	public List<int> Window = new List<int>();

	public int CurrentLevel = -1;

	public bool CurrentLevelRestarted;

	public bool CurrentLevelReported;

	public bool CurrentLevelSpecified;

	public bool CurrentRunUsedProp;

	public List<int> GameWindow = new List<int>();

	public bool CurrentRoundSettled;

	static RandomLevelFirstClearWindowData()
	{
		RegisterFormatter();
	}

	[Preserve]
	public static void RegisterFormatter()
	{
		if (!MemoryPackFormatterProvider.IsRegistered<RandomLevelFirstClearWindowData>())
		{
			MemoryPackFormatterProvider.Register<RandomLevelFirstClearWindowData>((MemoryPackFormatter<RandomLevelFirstClearWindowData>)(object)new RandomLevelFirstClearWindowDataFormatter());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<RandomLevelFirstClearWindowData[]>())
		{
			MemoryPackFormatterProvider.Register<RandomLevelFirstClearWindowData[]>((MemoryPackFormatter<RandomLevelFirstClearWindowData[]>)(object)new ArrayFormatter<RandomLevelFirstClearWindowData>());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<List<int>>())
		{
			MemoryPackFormatterProvider.Register<List<int>>((MemoryPackFormatter<List<int>>)(object)new ListFormatter<int>());
		}
	}

	[Preserve]
	public static void Serialize(ref MemoryPackWriter writer, ref RandomLevelFirstClearWindowData? value)
	{
		if (value == null)
		{
			((MemoryPackWriter)(ref writer)).WriteNullObjectHeader();
			return;
		}
		((MemoryPackWriter)(ref writer)).WriteObjectHeader((byte)8);
		((MemoryPackWriter)(ref writer)).WriteValue<List<int>>(ref value.Window);
		((MemoryPackWriter)(ref writer)).WriteUnmanaged<int, bool, bool, bool, bool>(ref value.CurrentLevel, ref value.CurrentLevelRestarted, ref value.CurrentLevelReported, ref value.CurrentLevelSpecified, ref value.CurrentRunUsedProp);
		((MemoryPackWriter)(ref writer)).WriteValue<List<int>>(ref value.GameWindow);
		((MemoryPackWriter)(ref writer)).WriteUnmanaged<bool>(ref value.CurrentRoundSettled);
	}

	[Preserve]
	public static void Deserialize(ref MemoryPackReader reader, ref RandomLevelFirstClearWindowData? value)
	{
		byte b = default(byte);
		if (!((MemoryPackReader)(ref reader)).TryReadObjectHeader(ref b))
		{
			value = null;
			return;
		}
		List<int> window;
		int currentLevel = default(int);
		bool currentLevelRestarted = default(bool);
		bool currentLevelReported = default(bool);
		bool currentLevelSpecified = default(bool);
		bool currentRunUsedProp = default(bool);
		List<int> gameWindow;
		bool currentRoundSettled = default(bool);
		if (b == 8)
		{
			if (value != null)
			{
				window = value.Window;
				currentLevel = value.CurrentLevel;
				currentLevelRestarted = value.CurrentLevelRestarted;
				currentLevelReported = value.CurrentLevelReported;
				currentLevelSpecified = value.CurrentLevelSpecified;
				currentRunUsedProp = value.CurrentRunUsedProp;
				gameWindow = value.GameWindow;
				currentRoundSettled = value.CurrentRoundSettled;
				((MemoryPackReader)(ref reader)).ReadValue<List<int>>(ref window);
				((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref currentLevel);
				((MemoryPackReader)(ref reader)).ReadUnmanaged<bool>(ref currentLevelRestarted);
				((MemoryPackReader)(ref reader)).ReadUnmanaged<bool>(ref currentLevelReported);
				((MemoryPackReader)(ref reader)).ReadUnmanaged<bool>(ref currentLevelSpecified);
				((MemoryPackReader)(ref reader)).ReadUnmanaged<bool>(ref currentRunUsedProp);
				((MemoryPackReader)(ref reader)).ReadValue<List<int>>(ref gameWindow);
				((MemoryPackReader)(ref reader)).ReadUnmanaged<bool>(ref currentRoundSettled);
				goto IL_01ac;
			}
			window = ((MemoryPackReader)(ref reader)).ReadValue<List<int>>();
			((MemoryPackReader)(ref reader)).ReadUnmanaged<int, bool, bool, bool, bool>(ref currentLevel, ref currentLevelRestarted, ref currentLevelReported, ref currentLevelSpecified, ref currentRunUsedProp);
			gameWindow = ((MemoryPackReader)(ref reader)).ReadValue<List<int>>();
			((MemoryPackReader)(ref reader)).ReadUnmanaged<bool>(ref currentRoundSettled);
		}
		else
		{
			if (b > 8)
			{
				MemoryPackSerializationException.ThrowInvalidPropertyCount(typeof(RandomLevelFirstClearWindowData), (byte)8, b);
				return;
			}
			if (value == null)
			{
				window = null;
				currentLevel = 0;
				currentLevelRestarted = false;
				currentLevelReported = false;
				currentLevelSpecified = false;
				currentRunUsedProp = false;
				gameWindow = null;
				currentRoundSettled = false;
			}
			else
			{
				window = value.Window;
				currentLevel = value.CurrentLevel;
				currentLevelRestarted = value.CurrentLevelRestarted;
				currentLevelReported = value.CurrentLevelReported;
				currentLevelSpecified = value.CurrentLevelSpecified;
				currentRunUsedProp = value.CurrentRunUsedProp;
				gameWindow = value.GameWindow;
				currentRoundSettled = value.CurrentRoundSettled;
			}
			if (b != 0)
			{
				((MemoryPackReader)(ref reader)).ReadValue<List<int>>(ref window);
				if (b != 1)
				{
					((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref currentLevel);
					if (b != 2)
					{
						((MemoryPackReader)(ref reader)).ReadUnmanaged<bool>(ref currentLevelRestarted);
						if (b != 3)
						{
							((MemoryPackReader)(ref reader)).ReadUnmanaged<bool>(ref currentLevelReported);
							if (b != 4)
							{
								((MemoryPackReader)(ref reader)).ReadUnmanaged<bool>(ref currentLevelSpecified);
								if (b != 5)
								{
									((MemoryPackReader)(ref reader)).ReadUnmanaged<bool>(ref currentRunUsedProp);
									if (b != 6)
									{
										((MemoryPackReader)(ref reader)).ReadValue<List<int>>(ref gameWindow);
										if (b != 7)
										{
											((MemoryPackReader)(ref reader)).ReadUnmanaged<bool>(ref currentRoundSettled);
											_ = 8;
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
				goto IL_01ac;
			}
		}
		value = new RandomLevelFirstClearWindowData
		{
			Window = window,
			CurrentLevel = currentLevel,
			CurrentLevelRestarted = currentLevelRestarted,
			CurrentLevelReported = currentLevelReported,
			CurrentLevelSpecified = currentLevelSpecified,
			CurrentRunUsedProp = currentRunUsedProp,
			GameWindow = gameWindow,
			CurrentRoundSettled = currentRoundSettled
		};
		return;
		IL_01ac:
		value.Window = window;
		value.CurrentLevel = currentLevel;
		value.CurrentLevelRestarted = currentLevelRestarted;
		value.CurrentLevelReported = currentLevelReported;
		value.CurrentLevelSpecified = currentLevelSpecified;
		value.CurrentRunUsedProp = currentRunUsedProp;
		value.GameWindow = gameWindow;
		value.CurrentRoundSettled = currentRoundSettled;
	}
}
