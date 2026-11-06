using MemoryPack;
using MemoryPack.Formatters;
using MemoryPack.Internal;

namespace Example.Cfg;

[MemoryPackable(/*Could not decode attribute arguments.*/)]
public class GameWinIQRange : IMemoryPackable<GameWinIQRange>, IMemoryPackFormatterRegister
{
	[Preserve]
	private sealed class GameWinIQRangeFormatter : MemoryPackFormatter<GameWinIQRange>
	{
		[Preserve]
		public override void Serialize(ref MemoryPackWriter writer, ref GameWinIQRange value)
		{
			GameWinIQRange.Serialize(ref writer, ref value);
		}

		[Preserve]
		public override void Deserialize(ref MemoryPackReader reader, ref GameWinIQRange value)
		{
			GameWinIQRange.Deserialize(ref reader, ref value);
		}
	}

	[MemoryPackOrder(0)]
	public int Id { get; private set; }

	[MemoryPackOrder(1)]
	public int Group { get; private set; }

	[MemoryPackOrder(2)]
	public string IQRange { get; private set; }

	[MemoryPackOrder(3)]
	public int ExpertLevel { get; private set; }

	[MemoryPackOrder(4)]
	public int HardLevel { get; private set; }

	[MemoryPackOrder(5)]
	public int NormalLevel { get; private set; }

	[MemoryPackOrder(6)]
	public int ItemCost { get; private set; }

	[MemoryPackOrder(7)]
	public int MaxItemCost { get; private set; }

	[MemoryPackOrder(8)]
	public int RestartCost { get; private set; }

	[MemoryPackOrder(9)]
	public int MaxRestartCost { get; private set; }

	[MemoryPackOrder(10)]
	public int UnlockedOC1 { get; private set; }

	[MemoryPackOrder(11)]
	public int UnlockedOC2 { get; private set; }

	[MemoryPackOrder(12)]
	public int UnlockedOC3 { get; private set; }

	[MemoryPackOrder(13)]
	public int MaxUnlockedOC { get; private set; }

	[MemoryPackOrder(14)]
	public int ErrorInvalidSteps { get; private set; }

	[MemoryPackOrder(15)]
	public int MaxErrorInvalidSteps { get; private set; }

	static GameWinIQRange()
	{
		RegisterFormatter();
	}

	[Preserve]
	public static void RegisterFormatter()
	{
		if (!MemoryPackFormatterProvider.IsRegistered<GameWinIQRange>())
		{
			MemoryPackFormatterProvider.Register<GameWinIQRange>((MemoryPackFormatter<GameWinIQRange>)(object)new GameWinIQRangeFormatter());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<GameWinIQRange[]>())
		{
			MemoryPackFormatterProvider.Register<GameWinIQRange[]>((MemoryPackFormatter<GameWinIQRange[]>)(object)new ArrayFormatter<GameWinIQRange>());
		}
	}

	[Preserve]
	public static void Serialize(ref MemoryPackWriter writer, ref GameWinIQRange? value)
	{
		if (value == null)
		{
			((MemoryPackWriter)(ref writer)).WriteNullObjectHeader();
			return;
		}
		int id = value.Id;
		int num = value.Group;
		((MemoryPackWriter)(ref writer)).WriteUnmanagedWithObjectHeader<int, int>((byte)16, ref id, ref num);
		((MemoryPackWriter)(ref writer)).WriteString(value.IQRange);
		id = value.ExpertLevel;
		num = value.HardLevel;
		int normalLevel = value.NormalLevel;
		int itemCost = value.ItemCost;
		int maxItemCost = value.MaxItemCost;
		int restartCost = value.RestartCost;
		int maxRestartCost = value.MaxRestartCost;
		int unlockedOC = value.UnlockedOC1;
		int unlockedOC2 = value.UnlockedOC2;
		int unlockedOC3 = value.UnlockedOC3;
		int maxUnlockedOC = value.MaxUnlockedOC;
		int errorInvalidSteps = value.ErrorInvalidSteps;
		int maxErrorInvalidSteps = value.MaxErrorInvalidSteps;
		((MemoryPackWriter)(ref writer)).WriteUnmanaged<int, int, int, int, int, int, int, int, int, int, int, int, int>(ref id, ref num, ref normalLevel, ref itemCost, ref maxItemCost, ref restartCost, ref maxRestartCost, ref unlockedOC, ref unlockedOC2, ref unlockedOC3, ref maxUnlockedOC, ref errorInvalidSteps, ref maxErrorInvalidSteps);
	}

	[Preserve]
	public static void Deserialize(ref MemoryPackReader reader, ref GameWinIQRange? value)
	{
		byte b = default(byte);
		if (!((MemoryPackReader)(ref reader)).TryReadObjectHeader(ref b))
		{
			value = null;
			return;
		}
		int id = default(int);
		int num = default(int);
		int expertLevel = default(int);
		int hardLevel = default(int);
		int normalLevel = default(int);
		int itemCost = default(int);
		int maxItemCost = default(int);
		int restartCost = default(int);
		int maxRestartCost = default(int);
		int unlockedOC = default(int);
		int unlockedOC2 = default(int);
		int unlockedOC3 = default(int);
		int maxUnlockedOC = default(int);
		int errorInvalidSteps = default(int);
		int maxErrorInvalidSteps = default(int);
		string iQRange;
		if (b == 16)
		{
			if (value != null)
			{
				id = value.Id;
				num = value.Group;
				iQRange = value.IQRange;
				expertLevel = value.ExpertLevel;
				hardLevel = value.HardLevel;
				normalLevel = value.NormalLevel;
				itemCost = value.ItemCost;
				maxItemCost = value.MaxItemCost;
				restartCost = value.RestartCost;
				maxRestartCost = value.MaxRestartCost;
				unlockedOC = value.UnlockedOC1;
				unlockedOC2 = value.UnlockedOC2;
				unlockedOC3 = value.UnlockedOC3;
				maxUnlockedOC = value.MaxUnlockedOC;
				errorInvalidSteps = value.ErrorInvalidSteps;
				maxErrorInvalidSteps = value.MaxErrorInvalidSteps;
				((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref id);
				((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref num);
				iQRange = ((MemoryPackReader)(ref reader)).ReadString();
				((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref expertLevel);
				((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref hardLevel);
				((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref normalLevel);
				((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref itemCost);
				((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref maxItemCost);
				((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref restartCost);
				((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref maxRestartCost);
				((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref unlockedOC);
				((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref unlockedOC2);
				((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref unlockedOC3);
				((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref maxUnlockedOC);
				((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref errorInvalidSteps);
				((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref maxErrorInvalidSteps);
				goto IL_0322;
			}
			((MemoryPackReader)(ref reader)).ReadUnmanaged<int, int>(ref id, ref num);
			iQRange = ((MemoryPackReader)(ref reader)).ReadString();
			((MemoryPackReader)(ref reader)).ReadUnmanaged<int, int, int, int, int, int, int, int, int, int, int, int, int>(ref expertLevel, ref hardLevel, ref normalLevel, ref itemCost, ref maxItemCost, ref restartCost, ref maxRestartCost, ref unlockedOC, ref unlockedOC2, ref unlockedOC3, ref maxUnlockedOC, ref errorInvalidSteps, ref maxErrorInvalidSteps);
		}
		else
		{
			if (b > 16)
			{
				MemoryPackSerializationException.ThrowInvalidPropertyCount(typeof(GameWinIQRange), (byte)16, b);
				return;
			}
			if (value == null)
			{
				id = 0;
				num = 0;
				iQRange = null;
				expertLevel = 0;
				hardLevel = 0;
				normalLevel = 0;
				itemCost = 0;
				maxItemCost = 0;
				restartCost = 0;
				maxRestartCost = 0;
				unlockedOC = 0;
				unlockedOC2 = 0;
				unlockedOC3 = 0;
				maxUnlockedOC = 0;
				errorInvalidSteps = 0;
				maxErrorInvalidSteps = 0;
			}
			else
			{
				id = value.Id;
				num = value.Group;
				iQRange = value.IQRange;
				expertLevel = value.ExpertLevel;
				hardLevel = value.HardLevel;
				normalLevel = value.NormalLevel;
				itemCost = value.ItemCost;
				maxItemCost = value.MaxItemCost;
				restartCost = value.RestartCost;
				maxRestartCost = value.MaxRestartCost;
				unlockedOC = value.UnlockedOC1;
				unlockedOC2 = value.UnlockedOC2;
				unlockedOC3 = value.UnlockedOC3;
				maxUnlockedOC = value.MaxUnlockedOC;
				errorInvalidSteps = value.ErrorInvalidSteps;
				maxErrorInvalidSteps = value.MaxErrorInvalidSteps;
			}
			if (b != 0)
			{
				((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref id);
				if (b != 1)
				{
					((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref num);
					if (b != 2)
					{
						iQRange = ((MemoryPackReader)(ref reader)).ReadString();
						if (b != 3)
						{
							((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref expertLevel);
							if (b != 4)
							{
								((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref hardLevel);
								if (b != 5)
								{
									((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref normalLevel);
									if (b != 6)
									{
										((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref itemCost);
										if (b != 7)
										{
											((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref maxItemCost);
											if (b != 8)
											{
												((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref restartCost);
												if (b != 9)
												{
													((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref maxRestartCost);
													if (b != 10)
													{
														((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref unlockedOC);
														if (b != 11)
														{
															((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref unlockedOC2);
															if (b != 12)
															{
																((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref unlockedOC3);
																if (b != 13)
																{
																	((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref maxUnlockedOC);
																	if (b != 14)
																	{
																		((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref errorInvalidSteps);
																		if (b != 15)
																		{
																			((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref maxErrorInvalidSteps);
																			_ = 16;
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
							}
						}
					}
				}
			}
			if (value != null)
			{
				goto IL_0322;
			}
		}
		value = new GameWinIQRange
		{
			Id = id,
			Group = num,
			IQRange = iQRange,
			ExpertLevel = expertLevel,
			HardLevel = hardLevel,
			NormalLevel = normalLevel,
			ItemCost = itemCost,
			MaxItemCost = maxItemCost,
			RestartCost = restartCost,
			MaxRestartCost = maxRestartCost,
			UnlockedOC1 = unlockedOC,
			UnlockedOC2 = unlockedOC2,
			UnlockedOC3 = unlockedOC3,
			MaxUnlockedOC = maxUnlockedOC,
			ErrorInvalidSteps = errorInvalidSteps,
			MaxErrorInvalidSteps = maxErrorInvalidSteps
		};
		return;
		IL_0322:
		value.Id = id;
		value.Group = num;
		value.IQRange = iQRange;
		value.ExpertLevel = expertLevel;
		value.HardLevel = hardLevel;
		value.NormalLevel = normalLevel;
		value.ItemCost = itemCost;
		value.MaxItemCost = maxItemCost;
		value.RestartCost = restartCost;
		value.MaxRestartCost = maxRestartCost;
		value.UnlockedOC1 = unlockedOC;
		value.UnlockedOC2 = unlockedOC2;
		value.UnlockedOC3 = unlockedOC3;
		value.MaxUnlockedOC = maxUnlockedOC;
		value.ErrorInvalidSteps = errorInvalidSteps;
		value.MaxErrorInvalidSteps = maxErrorInvalidSteps;
	}
}
