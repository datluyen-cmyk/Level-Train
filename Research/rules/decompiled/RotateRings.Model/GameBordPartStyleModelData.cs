using MemoryPack;
using MemoryPack.Formatters;
using MemoryPack.Internal;
using XingYun.Save;

namespace RotateRings.Model;

[MemoryPackable(/*Could not decode attribute arguments.*/)]
public class GameBordPartStyleModelData : ISaveDefaultInitializer, IMemoryPackable<GameBordPartStyleModelData>, IMemoryPackFormatterRegister
{
	[Preserve]
	private sealed class GameBordPartStyleModelDataFormatter : MemoryPackFormatter<GameBordPartStyleModelData>
	{
		[Preserve]
		public override void Serialize(ref MemoryPackWriter writer, ref GameBordPartStyleModelData value)
		{
			GameBordPartStyleModelData.Serialize(ref writer, ref value);
		}

		[Preserve]
		public override void Deserialize(ref MemoryPackReader reader, ref GameBordPartStyleModelData value)
		{
			GameBordPartStyleModelData.Deserialize(ref reader, ref value);
		}
	}

	public int Exp5LastRandom = -1;

	public int Exp6LastRestartStyle = -1;

	public int Exp6SessionStyle = -1;

	public int Exp6BoundLaunchId;

	public bool Exp6PendingRoll;

	public int Exp7InstallDay;

	public int Exp7DayStyle = -1;

	public int Exp7PrevDayStyle = -1;

	public int ActiveResumeGameMode = -1;

	public int ActiveResumeLevel = -1;

	public int ActiveResumePartStyle = -1;

	public int Exp14LockedLevel = -1;

	public int Exp14LockedPartStyle = -1;

	public int Exp15LockedGlobalLevel = -1;

	public int Exp15LockedPartStyle = -1;

	public int Exp15LockedDayYmd;

	public int Exp15LastLocalCalendarYmd;

	public int Exp15LocalNaturalDayIndex;

	public void OnCreateDefault()
	{
		Exp5LastRandom = -1;
		Exp6LastRestartStyle = -1;
		Exp6SessionStyle = -1;
		Exp6BoundLaunchId = 0;
		Exp6PendingRoll = false;
		Exp7InstallDay = 0;
		Exp7DayStyle = -1;
		Exp7PrevDayStyle = -1;
		ActiveResumeGameMode = -1;
		ActiveResumeLevel = -1;
		ActiveResumePartStyle = -1;
		Exp14LockedLevel = -1;
		Exp14LockedPartStyle = -1;
		Exp15LockedGlobalLevel = -1;
		Exp15LockedPartStyle = -1;
		Exp15LockedDayYmd = 0;
		Exp15LastLocalCalendarYmd = 0;
		Exp15LocalNaturalDayIndex = 0;
	}

	static GameBordPartStyleModelData()
	{
		RegisterFormatter();
	}

	[Preserve]
	public static void RegisterFormatter()
	{
		if (!MemoryPackFormatterProvider.IsRegistered<GameBordPartStyleModelData>())
		{
			MemoryPackFormatterProvider.Register<GameBordPartStyleModelData>((MemoryPackFormatter<GameBordPartStyleModelData>)(object)new GameBordPartStyleModelDataFormatter());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<GameBordPartStyleModelData[]>())
		{
			MemoryPackFormatterProvider.Register<GameBordPartStyleModelData[]>((MemoryPackFormatter<GameBordPartStyleModelData[]>)(object)new ArrayFormatter<GameBordPartStyleModelData>());
		}
	}

	[Preserve]
	public static void Serialize(ref MemoryPackWriter writer, ref GameBordPartStyleModelData? value)
	{
		if (value == null)
		{
			((MemoryPackWriter)(ref writer)).WriteNullObjectHeader();
			return;
		}
		((MemoryPackWriter)(ref writer)).WriteUnmanagedWithObjectHeader<int, int, int, int, bool, int, int, int, int, int, int, int, int, int, int>((byte)18, ref value.Exp5LastRandom, ref value.Exp6LastRestartStyle, ref value.Exp6SessionStyle, ref value.Exp6BoundLaunchId, ref value.Exp6PendingRoll, ref value.Exp7InstallDay, ref value.Exp7DayStyle, ref value.Exp7PrevDayStyle, ref value.ActiveResumeGameMode, ref value.ActiveResumeLevel, ref value.ActiveResumePartStyle, ref value.Exp14LockedLevel, ref value.Exp14LockedPartStyle, ref value.Exp15LockedGlobalLevel, ref value.Exp15LockedPartStyle);
		((MemoryPackWriter)(ref writer)).WriteUnmanaged<int, int, int>(ref value.Exp15LockedDayYmd, ref value.Exp15LastLocalCalendarYmd, ref value.Exp15LocalNaturalDayIndex);
	}

	[Preserve]
	public static void Deserialize(ref MemoryPackReader reader, ref GameBordPartStyleModelData? value)
	{
		byte b = default(byte);
		if (!((MemoryPackReader)(ref reader)).TryReadObjectHeader(ref b))
		{
			value = null;
			return;
		}
		int exp5LastRandom = default(int);
		int exp6LastRestartStyle = default(int);
		int exp6SessionStyle = default(int);
		int exp6BoundLaunchId = default(int);
		bool exp6PendingRoll = default(bool);
		int exp7InstallDay = default(int);
		int exp7DayStyle = default(int);
		int exp7PrevDayStyle = default(int);
		int activeResumeGameMode = default(int);
		int activeResumeLevel = default(int);
		int activeResumePartStyle = default(int);
		int exp14LockedLevel = default(int);
		int exp14LockedPartStyle = default(int);
		int exp15LockedGlobalLevel = default(int);
		int exp15LockedPartStyle = default(int);
		int exp15LockedDayYmd = default(int);
		int exp15LastLocalCalendarYmd = default(int);
		int exp15LocalNaturalDayIndex = default(int);
		if (b == 18)
		{
			if (value != null)
			{
				exp5LastRandom = value.Exp5LastRandom;
				exp6LastRestartStyle = value.Exp6LastRestartStyle;
				exp6SessionStyle = value.Exp6SessionStyle;
				exp6BoundLaunchId = value.Exp6BoundLaunchId;
				exp6PendingRoll = value.Exp6PendingRoll;
				exp7InstallDay = value.Exp7InstallDay;
				exp7DayStyle = value.Exp7DayStyle;
				exp7PrevDayStyle = value.Exp7PrevDayStyle;
				activeResumeGameMode = value.ActiveResumeGameMode;
				activeResumeLevel = value.ActiveResumeLevel;
				activeResumePartStyle = value.ActiveResumePartStyle;
				exp14LockedLevel = value.Exp14LockedLevel;
				exp14LockedPartStyle = value.Exp14LockedPartStyle;
				exp15LockedGlobalLevel = value.Exp15LockedGlobalLevel;
				exp15LockedPartStyle = value.Exp15LockedPartStyle;
				exp15LockedDayYmd = value.Exp15LockedDayYmd;
				exp15LastLocalCalendarYmd = value.Exp15LastLocalCalendarYmd;
				exp15LocalNaturalDayIndex = value.Exp15LocalNaturalDayIndex;
				((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref exp5LastRandom);
				((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref exp6LastRestartStyle);
				((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref exp6SessionStyle);
				((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref exp6BoundLaunchId);
				((MemoryPackReader)(ref reader)).ReadUnmanaged<bool>(ref exp6PendingRoll);
				((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref exp7InstallDay);
				((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref exp7DayStyle);
				((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref exp7PrevDayStyle);
				((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref activeResumeGameMode);
				((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref activeResumeLevel);
				((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref activeResumePartStyle);
				((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref exp14LockedLevel);
				((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref exp14LockedPartStyle);
				((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref exp15LockedGlobalLevel);
				((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref exp15LockedPartStyle);
				((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref exp15LockedDayYmd);
				((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref exp15LastLocalCalendarYmd);
				((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref exp15LocalNaturalDayIndex);
				goto IL_037d;
			}
			((MemoryPackReader)(ref reader)).ReadUnmanaged<int, int, int, int, bool, int, int, int, int, int, int, int, int, int, int>(ref exp5LastRandom, ref exp6LastRestartStyle, ref exp6SessionStyle, ref exp6BoundLaunchId, ref exp6PendingRoll, ref exp7InstallDay, ref exp7DayStyle, ref exp7PrevDayStyle, ref activeResumeGameMode, ref activeResumeLevel, ref activeResumePartStyle, ref exp14LockedLevel, ref exp14LockedPartStyle, ref exp15LockedGlobalLevel, ref exp15LockedPartStyle);
			((MemoryPackReader)(ref reader)).ReadUnmanaged<int, int, int>(ref exp15LockedDayYmd, ref exp15LastLocalCalendarYmd, ref exp15LocalNaturalDayIndex);
		}
		else
		{
			if (b > 18)
			{
				MemoryPackSerializationException.ThrowInvalidPropertyCount(typeof(GameBordPartStyleModelData), (byte)18, b);
				return;
			}
			if (value == null)
			{
				exp5LastRandom = 0;
				exp6LastRestartStyle = 0;
				exp6SessionStyle = 0;
				exp6BoundLaunchId = 0;
				exp6PendingRoll = false;
				exp7InstallDay = 0;
				exp7DayStyle = 0;
				exp7PrevDayStyle = 0;
				activeResumeGameMode = 0;
				activeResumeLevel = 0;
				activeResumePartStyle = 0;
				exp14LockedLevel = 0;
				exp14LockedPartStyle = 0;
				exp15LockedGlobalLevel = 0;
				exp15LockedPartStyle = 0;
				exp15LockedDayYmd = 0;
				exp15LastLocalCalendarYmd = 0;
				exp15LocalNaturalDayIndex = 0;
			}
			else
			{
				exp5LastRandom = value.Exp5LastRandom;
				exp6LastRestartStyle = value.Exp6LastRestartStyle;
				exp6SessionStyle = value.Exp6SessionStyle;
				exp6BoundLaunchId = value.Exp6BoundLaunchId;
				exp6PendingRoll = value.Exp6PendingRoll;
				exp7InstallDay = value.Exp7InstallDay;
				exp7DayStyle = value.Exp7DayStyle;
				exp7PrevDayStyle = value.Exp7PrevDayStyle;
				activeResumeGameMode = value.ActiveResumeGameMode;
				activeResumeLevel = value.ActiveResumeLevel;
				activeResumePartStyle = value.ActiveResumePartStyle;
				exp14LockedLevel = value.Exp14LockedLevel;
				exp14LockedPartStyle = value.Exp14LockedPartStyle;
				exp15LockedGlobalLevel = value.Exp15LockedGlobalLevel;
				exp15LockedPartStyle = value.Exp15LockedPartStyle;
				exp15LockedDayYmd = value.Exp15LockedDayYmd;
				exp15LastLocalCalendarYmd = value.Exp15LastLocalCalendarYmd;
				exp15LocalNaturalDayIndex = value.Exp15LocalNaturalDayIndex;
			}
			if (b != 0)
			{
				((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref exp5LastRandom);
				if (b != 1)
				{
					((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref exp6LastRestartStyle);
					if (b != 2)
					{
						((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref exp6SessionStyle);
						if (b != 3)
						{
							((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref exp6BoundLaunchId);
							if (b != 4)
							{
								((MemoryPackReader)(ref reader)).ReadUnmanaged<bool>(ref exp6PendingRoll);
								if (b != 5)
								{
									((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref exp7InstallDay);
									if (b != 6)
									{
										((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref exp7DayStyle);
										if (b != 7)
										{
											((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref exp7PrevDayStyle);
											if (b != 8)
											{
												((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref activeResumeGameMode);
												if (b != 9)
												{
													((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref activeResumeLevel);
													if (b != 10)
													{
														((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref activeResumePartStyle);
														if (b != 11)
														{
															((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref exp14LockedLevel);
															if (b != 12)
															{
																((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref exp14LockedPartStyle);
																if (b != 13)
																{
																	((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref exp15LockedGlobalLevel);
																	if (b != 14)
																	{
																		((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref exp15LockedPartStyle);
																		if (b != 15)
																		{
																			((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref exp15LockedDayYmd);
																			if (b != 16)
																			{
																				((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref exp15LastLocalCalendarYmd);
																				if (b != 17)
																				{
																					((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref exp15LocalNaturalDayIndex);
																					_ = 18;
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
				}
			}
			if (value != null)
			{
				goto IL_037d;
			}
		}
		value = new GameBordPartStyleModelData
		{
			Exp5LastRandom = exp5LastRandom,
			Exp6LastRestartStyle = exp6LastRestartStyle,
			Exp6SessionStyle = exp6SessionStyle,
			Exp6BoundLaunchId = exp6BoundLaunchId,
			Exp6PendingRoll = exp6PendingRoll,
			Exp7InstallDay = exp7InstallDay,
			Exp7DayStyle = exp7DayStyle,
			Exp7PrevDayStyle = exp7PrevDayStyle,
			ActiveResumeGameMode = activeResumeGameMode,
			ActiveResumeLevel = activeResumeLevel,
			ActiveResumePartStyle = activeResumePartStyle,
			Exp14LockedLevel = exp14LockedLevel,
			Exp14LockedPartStyle = exp14LockedPartStyle,
			Exp15LockedGlobalLevel = exp15LockedGlobalLevel,
			Exp15LockedPartStyle = exp15LockedPartStyle,
			Exp15LockedDayYmd = exp15LockedDayYmd,
			Exp15LastLocalCalendarYmd = exp15LastLocalCalendarYmd,
			Exp15LocalNaturalDayIndex = exp15LocalNaturalDayIndex
		};
		return;
		IL_037d:
		value.Exp5LastRandom = exp5LastRandom;
		value.Exp6LastRestartStyle = exp6LastRestartStyle;
		value.Exp6SessionStyle = exp6SessionStyle;
		value.Exp6BoundLaunchId = exp6BoundLaunchId;
		value.Exp6PendingRoll = exp6PendingRoll;
		value.Exp7InstallDay = exp7InstallDay;
		value.Exp7DayStyle = exp7DayStyle;
		value.Exp7PrevDayStyle = exp7PrevDayStyle;
		value.ActiveResumeGameMode = activeResumeGameMode;
		value.ActiveResumeLevel = activeResumeLevel;
		value.ActiveResumePartStyle = activeResumePartStyle;
		value.Exp14LockedLevel = exp14LockedLevel;
		value.Exp14LockedPartStyle = exp14LockedPartStyle;
		value.Exp15LockedGlobalLevel = exp15LockedGlobalLevel;
		value.Exp15LockedPartStyle = exp15LockedPartStyle;
		value.Exp15LockedDayYmd = exp15LockedDayYmd;
		value.Exp15LastLocalCalendarYmd = exp15LastLocalCalendarYmd;
		value.Exp15LocalNaturalDayIndex = exp15LocalNaturalDayIndex;
	}
}
