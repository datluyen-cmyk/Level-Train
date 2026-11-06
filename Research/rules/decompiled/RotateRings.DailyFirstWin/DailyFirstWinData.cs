using System.Collections.Generic;
using MemoryPack;
using MemoryPack.Formatters;
using MemoryPack.Internal;
using XingYun.Save;

namespace RotateRings.DailyFirstWin;

[MemoryPackable(/*Could not decode attribute arguments.*/)]
public class DailyFirstWinData : ISaveDefaultInitializer, IMemoryPackable<DailyFirstWinData>, IMemoryPackFormatterRegister
{
	[Preserve]
	private sealed class DailyFirstWinDataFormatter : MemoryPackFormatter<DailyFirstWinData>
	{
		[Preserve]
		public override void Serialize(ref MemoryPackWriter writer, ref DailyFirstWinData value)
		{
			DailyFirstWinData.Serialize(ref writer, ref value);
		}

		[Preserve]
		public override void Deserialize(ref MemoryPackReader reader, ref DailyFirstWinData value)
		{
			DailyFirstWinData.Deserialize(ref reader, ref value);
		}
	}

	public long LastWinTimestamp;

	public int CurrentDayInCycle;

	public int PendingRewardDay;

	public bool IsRewardClaimFlowActive;

	public int CurrentPuzzleId;

	public bool DeferPuzzleEntry;

	public bool EntryPlayModeMigrationApplied;

	public List<int> PuzzleRandomBag;

	public string PuzzleRotationStateKey;

	public DailyFirstWinPuzzleTheme CurrentPuzzleTheme;

	public int LastHalloweenPuzzleId;

	public bool HasCompletedHalloweenPuzzle;

	public bool HasPreselectedPuzzleForPendingCycle;

	public void OnCreateDefault()
	{
		LastWinTimestamp = 0L;
		CurrentDayInCycle = 0;
		PendingRewardDay = 0;
		IsRewardClaimFlowActive = false;
		CurrentPuzzleId = 0;
		DeferPuzzleEntry = false;
		EntryPlayModeMigrationApplied = false;
		PuzzleRandomBag = null;
		PuzzleRotationStateKey = string.Empty;
		CurrentPuzzleTheme = DailyFirstWinPuzzleTheme.Normal;
		LastHalloweenPuzzleId = 0;
		HasCompletedHalloweenPuzzle = false;
		HasPreselectedPuzzleForPendingCycle = false;
	}

	static DailyFirstWinData()
	{
		RegisterFormatter();
	}

	[Preserve]
	public static void RegisterFormatter()
	{
		if (!MemoryPackFormatterProvider.IsRegistered<DailyFirstWinData>())
		{
			MemoryPackFormatterProvider.Register<DailyFirstWinData>((MemoryPackFormatter<DailyFirstWinData>)(object)new DailyFirstWinDataFormatter());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<DailyFirstWinData[]>())
		{
			MemoryPackFormatterProvider.Register<DailyFirstWinData[]>((MemoryPackFormatter<DailyFirstWinData[]>)(object)new ArrayFormatter<DailyFirstWinData>());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<List<int>>())
		{
			MemoryPackFormatterProvider.Register<List<int>>((MemoryPackFormatter<List<int>>)(object)new ListFormatter<int>());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<DailyFirstWinPuzzleTheme>())
		{
			MemoryPackFormatterProvider.Register<DailyFirstWinPuzzleTheme>((MemoryPackFormatter<DailyFirstWinPuzzleTheme>)(object)new UnmanagedFormatter<DailyFirstWinPuzzleTheme>());
		}
	}

	[Preserve]
	public static void Serialize(ref MemoryPackWriter writer, ref DailyFirstWinData? value)
	{
		if (value == null)
		{
			((MemoryPackWriter)(ref writer)).WriteNullObjectHeader();
			return;
		}
		((MemoryPackWriter)(ref writer)).WriteUnmanagedWithObjectHeader<long, int, int, bool, int, bool, bool>((byte)13, ref value.LastWinTimestamp, ref value.CurrentDayInCycle, ref value.PendingRewardDay, ref value.IsRewardClaimFlowActive, ref value.CurrentPuzzleId, ref value.DeferPuzzleEntry, ref value.EntryPlayModeMigrationApplied);
		((MemoryPackWriter)(ref writer)).WriteValue<List<int>>(ref value.PuzzleRandomBag);
		((MemoryPackWriter)(ref writer)).WriteString(value.PuzzleRotationStateKey);
		((MemoryPackWriter)(ref writer)).WriteUnmanaged<DailyFirstWinPuzzleTheme, int, bool, bool>(ref value.CurrentPuzzleTheme, ref value.LastHalloweenPuzzleId, ref value.HasCompletedHalloweenPuzzle, ref value.HasPreselectedPuzzleForPendingCycle);
	}

	[Preserve]
	public static void Deserialize(ref MemoryPackReader reader, ref DailyFirstWinData? value)
	{
		byte b = default(byte);
		if (!((MemoryPackReader)(ref reader)).TryReadObjectHeader(ref b))
		{
			value = null;
			return;
		}
		long lastWinTimestamp = default(long);
		int currentDayInCycle = default(int);
		int pendingRewardDay = default(int);
		bool isRewardClaimFlowActive = default(bool);
		int currentPuzzleId = default(int);
		bool deferPuzzleEntry = default(bool);
		bool entryPlayModeMigrationApplied = default(bool);
		List<int> puzzleRandomBag;
		DailyFirstWinPuzzleTheme currentPuzzleTheme = default(DailyFirstWinPuzzleTheme);
		int lastHalloweenPuzzleId = default(int);
		bool hasCompletedHalloweenPuzzle = default(bool);
		bool hasPreselectedPuzzleForPendingCycle = default(bool);
		string puzzleRotationStateKey;
		if (b == 13)
		{
			if (value != null)
			{
				lastWinTimestamp = value.LastWinTimestamp;
				currentDayInCycle = value.CurrentDayInCycle;
				pendingRewardDay = value.PendingRewardDay;
				isRewardClaimFlowActive = value.IsRewardClaimFlowActive;
				currentPuzzleId = value.CurrentPuzzleId;
				deferPuzzleEntry = value.DeferPuzzleEntry;
				entryPlayModeMigrationApplied = value.EntryPlayModeMigrationApplied;
				puzzleRandomBag = value.PuzzleRandomBag;
				puzzleRotationStateKey = value.PuzzleRotationStateKey;
				currentPuzzleTheme = value.CurrentPuzzleTheme;
				lastHalloweenPuzzleId = value.LastHalloweenPuzzleId;
				hasCompletedHalloweenPuzzle = value.HasCompletedHalloweenPuzzle;
				hasPreselectedPuzzleForPendingCycle = value.HasPreselectedPuzzleForPendingCycle;
				((MemoryPackReader)(ref reader)).ReadUnmanaged<long>(ref lastWinTimestamp);
				((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref currentDayInCycle);
				((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref pendingRewardDay);
				((MemoryPackReader)(ref reader)).ReadUnmanaged<bool>(ref isRewardClaimFlowActive);
				((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref currentPuzzleId);
				((MemoryPackReader)(ref reader)).ReadUnmanaged<bool>(ref deferPuzzleEntry);
				((MemoryPackReader)(ref reader)).ReadUnmanaged<bool>(ref entryPlayModeMigrationApplied);
				((MemoryPackReader)(ref reader)).ReadValue<List<int>>(ref puzzleRandomBag);
				puzzleRotationStateKey = ((MemoryPackReader)(ref reader)).ReadString();
				((MemoryPackReader)(ref reader)).ReadUnmanaged<DailyFirstWinPuzzleTheme>(ref currentPuzzleTheme);
				((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref lastHalloweenPuzzleId);
				((MemoryPackReader)(ref reader)).ReadUnmanaged<bool>(ref hasCompletedHalloweenPuzzle);
				((MemoryPackReader)(ref reader)).ReadUnmanaged<bool>(ref hasPreselectedPuzzleForPendingCycle);
				goto IL_0296;
			}
			((MemoryPackReader)(ref reader)).ReadUnmanaged<long, int, int, bool, int, bool, bool>(ref lastWinTimestamp, ref currentDayInCycle, ref pendingRewardDay, ref isRewardClaimFlowActive, ref currentPuzzleId, ref deferPuzzleEntry, ref entryPlayModeMigrationApplied);
			puzzleRandomBag = ((MemoryPackReader)(ref reader)).ReadValue<List<int>>();
			puzzleRotationStateKey = ((MemoryPackReader)(ref reader)).ReadString();
			((MemoryPackReader)(ref reader)).ReadUnmanaged<DailyFirstWinPuzzleTheme, int, bool, bool>(ref currentPuzzleTheme, ref lastHalloweenPuzzleId, ref hasCompletedHalloweenPuzzle, ref hasPreselectedPuzzleForPendingCycle);
		}
		else
		{
			if (b > 13)
			{
				MemoryPackSerializationException.ThrowInvalidPropertyCount(typeof(DailyFirstWinData), (byte)13, b);
				return;
			}
			if (value == null)
			{
				lastWinTimestamp = 0L;
				currentDayInCycle = 0;
				pendingRewardDay = 0;
				isRewardClaimFlowActive = false;
				currentPuzzleId = 0;
				deferPuzzleEntry = false;
				entryPlayModeMigrationApplied = false;
				puzzleRandomBag = null;
				puzzleRotationStateKey = null;
				currentPuzzleTheme = DailyFirstWinPuzzleTheme.Normal;
				lastHalloweenPuzzleId = 0;
				hasCompletedHalloweenPuzzle = false;
				hasPreselectedPuzzleForPendingCycle = false;
			}
			else
			{
				lastWinTimestamp = value.LastWinTimestamp;
				currentDayInCycle = value.CurrentDayInCycle;
				pendingRewardDay = value.PendingRewardDay;
				isRewardClaimFlowActive = value.IsRewardClaimFlowActive;
				currentPuzzleId = value.CurrentPuzzleId;
				deferPuzzleEntry = value.DeferPuzzleEntry;
				entryPlayModeMigrationApplied = value.EntryPlayModeMigrationApplied;
				puzzleRandomBag = value.PuzzleRandomBag;
				puzzleRotationStateKey = value.PuzzleRotationStateKey;
				currentPuzzleTheme = value.CurrentPuzzleTheme;
				lastHalloweenPuzzleId = value.LastHalloweenPuzzleId;
				hasCompletedHalloweenPuzzle = value.HasCompletedHalloweenPuzzle;
				hasPreselectedPuzzleForPendingCycle = value.HasPreselectedPuzzleForPendingCycle;
			}
			if (b != 0)
			{
				((MemoryPackReader)(ref reader)).ReadUnmanaged<long>(ref lastWinTimestamp);
				if (b != 1)
				{
					((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref currentDayInCycle);
					if (b != 2)
					{
						((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref pendingRewardDay);
						if (b != 3)
						{
							((MemoryPackReader)(ref reader)).ReadUnmanaged<bool>(ref isRewardClaimFlowActive);
							if (b != 4)
							{
								((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref currentPuzzleId);
								if (b != 5)
								{
									((MemoryPackReader)(ref reader)).ReadUnmanaged<bool>(ref deferPuzzleEntry);
									if (b != 6)
									{
										((MemoryPackReader)(ref reader)).ReadUnmanaged<bool>(ref entryPlayModeMigrationApplied);
										if (b != 7)
										{
											((MemoryPackReader)(ref reader)).ReadValue<List<int>>(ref puzzleRandomBag);
											if (b != 8)
											{
												puzzleRotationStateKey = ((MemoryPackReader)(ref reader)).ReadString();
												if (b != 9)
												{
													((MemoryPackReader)(ref reader)).ReadUnmanaged<DailyFirstWinPuzzleTheme>(ref currentPuzzleTheme);
													if (b != 10)
													{
														((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref lastHalloweenPuzzleId);
														if (b != 11)
														{
															((MemoryPackReader)(ref reader)).ReadUnmanaged<bool>(ref hasCompletedHalloweenPuzzle);
															if (b != 12)
															{
																((MemoryPackReader)(ref reader)).ReadUnmanaged<bool>(ref hasPreselectedPuzzleForPendingCycle);
																_ = 13;
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
				goto IL_0296;
			}
		}
		value = new DailyFirstWinData
		{
			LastWinTimestamp = lastWinTimestamp,
			CurrentDayInCycle = currentDayInCycle,
			PendingRewardDay = pendingRewardDay,
			IsRewardClaimFlowActive = isRewardClaimFlowActive,
			CurrentPuzzleId = currentPuzzleId,
			DeferPuzzleEntry = deferPuzzleEntry,
			EntryPlayModeMigrationApplied = entryPlayModeMigrationApplied,
			PuzzleRandomBag = puzzleRandomBag,
			PuzzleRotationStateKey = puzzleRotationStateKey,
			CurrentPuzzleTheme = currentPuzzleTheme,
			LastHalloweenPuzzleId = lastHalloweenPuzzleId,
			HasCompletedHalloweenPuzzle = hasCompletedHalloweenPuzzle,
			HasPreselectedPuzzleForPendingCycle = hasPreselectedPuzzleForPendingCycle
		};
		return;
		IL_0296:
		value.LastWinTimestamp = lastWinTimestamp;
		value.CurrentDayInCycle = currentDayInCycle;
		value.PendingRewardDay = pendingRewardDay;
		value.IsRewardClaimFlowActive = isRewardClaimFlowActive;
		value.CurrentPuzzleId = currentPuzzleId;
		value.DeferPuzzleEntry = deferPuzzleEntry;
		value.EntryPlayModeMigrationApplied = entryPlayModeMigrationApplied;
		value.PuzzleRandomBag = puzzleRandomBag;
		value.PuzzleRotationStateKey = puzzleRotationStateKey;
		value.CurrentPuzzleTheme = currentPuzzleTheme;
		value.LastHalloweenPuzzleId = lastHalloweenPuzzleId;
		value.HasCompletedHalloweenPuzzle = hasCompletedHalloweenPuzzle;
		value.HasPreselectedPuzzleForPendingCycle = hasPreselectedPuzzleForPendingCycle;
	}
}
