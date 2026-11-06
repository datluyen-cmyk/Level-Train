using MemoryPack;
using MemoryPack.Formatters;
using MemoryPack.Internal;
using XingYun.Save;

namespace RotateRings.RewardedLuckyDraw;

[MemoryPackable(/*Could not decode attribute arguments.*/)]
public class RewardedLuckyDrawData : ISaveDefaultInitializer, IMemoryPackable<RewardedLuckyDrawData>, IMemoryPackFormatterRegister
{
	[Preserve]
	private sealed class RewardedLuckyDrawDataFormatter : MemoryPackFormatter<RewardedLuckyDrawData>
	{
		[Preserve]
		public override void Serialize(ref MemoryPackWriter writer, ref RewardedLuckyDrawData value)
		{
			RewardedLuckyDrawData.Serialize(ref writer, ref value);
		}

		[Preserve]
		public override void Deserialize(ref MemoryPackReader reader, ref RewardedLuckyDrawData value)
		{
			RewardedLuckyDrawData.Deserialize(ref reader, ref value);
		}
	}

	public int TotalPropUses;

	public long VictoryCount;

	public long LastShownVictory = -1L;

	public bool FirstRewardClaimed;

	public long DrawSequence;

	public int DismissCount;

	public int BlockedThroughSession;

	public bool PopupUnresolved;

	public int PopupSession;

	public int LastInterruptedSession = -1;

	public string PendingTransactionId;

	public int PendingSlotId;

	public int PendingPropType;

	public int PendingCount;

	public bool AdCompleted;

	public bool ExemptNextFrontAd;

	public long ExemptVictory;

	public int LastShownLocalDate;

	public void OnCreateDefault()
	{
		LastShownVictory = -1L;
		LastInterruptedSession = -1;
	}

	static RewardedLuckyDrawData()
	{
		RegisterFormatter();
	}

	[Preserve]
	public static void RegisterFormatter()
	{
		if (!MemoryPackFormatterProvider.IsRegistered<RewardedLuckyDrawData>())
		{
			MemoryPackFormatterProvider.Register<RewardedLuckyDrawData>((MemoryPackFormatter<RewardedLuckyDrawData>)(object)new RewardedLuckyDrawDataFormatter());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<RewardedLuckyDrawData[]>())
		{
			MemoryPackFormatterProvider.Register<RewardedLuckyDrawData[]>((MemoryPackFormatter<RewardedLuckyDrawData[]>)(object)new ArrayFormatter<RewardedLuckyDrawData>());
		}
	}

	[Preserve]
	public static void Serialize(ref MemoryPackWriter writer, ref RewardedLuckyDrawData? value)
	{
		if (value == null)
		{
			((MemoryPackWriter)(ref writer)).WriteNullObjectHeader();
			return;
		}
		((MemoryPackWriter)(ref writer)).WriteUnmanagedWithObjectHeader<int, long, long, bool, long, int, int, bool, int, int>((byte)18, ref value.TotalPropUses, ref value.VictoryCount, ref value.LastShownVictory, ref value.FirstRewardClaimed, ref value.DrawSequence, ref value.DismissCount, ref value.BlockedThroughSession, ref value.PopupUnresolved, ref value.PopupSession, ref value.LastInterruptedSession);
		((MemoryPackWriter)(ref writer)).WriteString(value.PendingTransactionId);
		((MemoryPackWriter)(ref writer)).WriteUnmanaged<int, int, int, bool, bool, long, int>(ref value.PendingSlotId, ref value.PendingPropType, ref value.PendingCount, ref value.AdCompleted, ref value.ExemptNextFrontAd, ref value.ExemptVictory, ref value.LastShownLocalDate);
	}

	[Preserve]
	public static void Deserialize(ref MemoryPackReader reader, ref RewardedLuckyDrawData? value)
	{
		byte b = default(byte);
		if (!((MemoryPackReader)(ref reader)).TryReadObjectHeader(ref b))
		{
			value = null;
			return;
		}
		int totalPropUses = default(int);
		long victoryCount = default(long);
		long lastShownVictory = default(long);
		bool firstRewardClaimed = default(bool);
		long drawSequence = default(long);
		int dismissCount = default(int);
		int blockedThroughSession = default(int);
		bool popupUnresolved = default(bool);
		int popupSession = default(int);
		int lastInterruptedSession = default(int);
		int pendingSlotId = default(int);
		int pendingPropType = default(int);
		int pendingCount = default(int);
		bool adCompleted = default(bool);
		bool exemptNextFrontAd = default(bool);
		long exemptVictory = default(long);
		int lastShownLocalDate = default(int);
		string pendingTransactionId;
		if (b == 18)
		{
			if (value != null)
			{
				totalPropUses = value.TotalPropUses;
				victoryCount = value.VictoryCount;
				lastShownVictory = value.LastShownVictory;
				firstRewardClaimed = value.FirstRewardClaimed;
				drawSequence = value.DrawSequence;
				dismissCount = value.DismissCount;
				blockedThroughSession = value.BlockedThroughSession;
				popupUnresolved = value.PopupUnresolved;
				popupSession = value.PopupSession;
				lastInterruptedSession = value.LastInterruptedSession;
				pendingTransactionId = value.PendingTransactionId;
				pendingSlotId = value.PendingSlotId;
				pendingPropType = value.PendingPropType;
				pendingCount = value.PendingCount;
				adCompleted = value.AdCompleted;
				exemptNextFrontAd = value.ExemptNextFrontAd;
				exemptVictory = value.ExemptVictory;
				lastShownLocalDate = value.LastShownLocalDate;
				((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref totalPropUses);
				((MemoryPackReader)(ref reader)).ReadUnmanaged<long>(ref victoryCount);
				((MemoryPackReader)(ref reader)).ReadUnmanaged<long>(ref lastShownVictory);
				((MemoryPackReader)(ref reader)).ReadUnmanaged<bool>(ref firstRewardClaimed);
				((MemoryPackReader)(ref reader)).ReadUnmanaged<long>(ref drawSequence);
				((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref dismissCount);
				((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref blockedThroughSession);
				((MemoryPackReader)(ref reader)).ReadUnmanaged<bool>(ref popupUnresolved);
				((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref popupSession);
				((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref lastInterruptedSession);
				pendingTransactionId = ((MemoryPackReader)(ref reader)).ReadString();
				((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref pendingSlotId);
				((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref pendingPropType);
				((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref pendingCount);
				((MemoryPackReader)(ref reader)).ReadUnmanaged<bool>(ref adCompleted);
				((MemoryPackReader)(ref reader)).ReadUnmanaged<bool>(ref exemptNextFrontAd);
				((MemoryPackReader)(ref reader)).ReadUnmanaged<long>(ref exemptVictory);
				((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref lastShownLocalDate);
				goto IL_0387;
			}
			((MemoryPackReader)(ref reader)).ReadUnmanaged<int, long, long, bool, long, int, int, bool, int, int>(ref totalPropUses, ref victoryCount, ref lastShownVictory, ref firstRewardClaimed, ref drawSequence, ref dismissCount, ref blockedThroughSession, ref popupUnresolved, ref popupSession, ref lastInterruptedSession);
			pendingTransactionId = ((MemoryPackReader)(ref reader)).ReadString();
			((MemoryPackReader)(ref reader)).ReadUnmanaged<int, int, int, bool, bool, long, int>(ref pendingSlotId, ref pendingPropType, ref pendingCount, ref adCompleted, ref exemptNextFrontAd, ref exemptVictory, ref lastShownLocalDate);
		}
		else
		{
			if (b > 18)
			{
				MemoryPackSerializationException.ThrowInvalidPropertyCount(typeof(RewardedLuckyDrawData), (byte)18, b);
				return;
			}
			if (value == null)
			{
				totalPropUses = 0;
				victoryCount = 0L;
				lastShownVictory = 0L;
				firstRewardClaimed = false;
				drawSequence = 0L;
				dismissCount = 0;
				blockedThroughSession = 0;
				popupUnresolved = false;
				popupSession = 0;
				lastInterruptedSession = 0;
				pendingTransactionId = null;
				pendingSlotId = 0;
				pendingPropType = 0;
				pendingCount = 0;
				adCompleted = false;
				exemptNextFrontAd = false;
				exemptVictory = 0L;
				lastShownLocalDate = 0;
			}
			else
			{
				totalPropUses = value.TotalPropUses;
				victoryCount = value.VictoryCount;
				lastShownVictory = value.LastShownVictory;
				firstRewardClaimed = value.FirstRewardClaimed;
				drawSequence = value.DrawSequence;
				dismissCount = value.DismissCount;
				blockedThroughSession = value.BlockedThroughSession;
				popupUnresolved = value.PopupUnresolved;
				popupSession = value.PopupSession;
				lastInterruptedSession = value.LastInterruptedSession;
				pendingTransactionId = value.PendingTransactionId;
				pendingSlotId = value.PendingSlotId;
				pendingPropType = value.PendingPropType;
				pendingCount = value.PendingCount;
				adCompleted = value.AdCompleted;
				exemptNextFrontAd = value.ExemptNextFrontAd;
				exemptVictory = value.ExemptVictory;
				lastShownLocalDate = value.LastShownLocalDate;
			}
			if (b != 0)
			{
				((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref totalPropUses);
				if (b != 1)
				{
					((MemoryPackReader)(ref reader)).ReadUnmanaged<long>(ref victoryCount);
					if (b != 2)
					{
						((MemoryPackReader)(ref reader)).ReadUnmanaged<long>(ref lastShownVictory);
						if (b != 3)
						{
							((MemoryPackReader)(ref reader)).ReadUnmanaged<bool>(ref firstRewardClaimed);
							if (b != 4)
							{
								((MemoryPackReader)(ref reader)).ReadUnmanaged<long>(ref drawSequence);
								if (b != 5)
								{
									((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref dismissCount);
									if (b != 6)
									{
										((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref blockedThroughSession);
										if (b != 7)
										{
											((MemoryPackReader)(ref reader)).ReadUnmanaged<bool>(ref popupUnresolved);
											if (b != 8)
											{
												((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref popupSession);
												if (b != 9)
												{
													((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref lastInterruptedSession);
													if (b != 10)
													{
														pendingTransactionId = ((MemoryPackReader)(ref reader)).ReadString();
														if (b != 11)
														{
															((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref pendingSlotId);
															if (b != 12)
															{
																((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref pendingPropType);
																if (b != 13)
																{
																	((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref pendingCount);
																	if (b != 14)
																	{
																		((MemoryPackReader)(ref reader)).ReadUnmanaged<bool>(ref adCompleted);
																		if (b != 15)
																		{
																			((MemoryPackReader)(ref reader)).ReadUnmanaged<bool>(ref exemptNextFrontAd);
																			if (b != 16)
																			{
																				((MemoryPackReader)(ref reader)).ReadUnmanaged<long>(ref exemptVictory);
																				if (b != 17)
																				{
																					((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref lastShownLocalDate);
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
				goto IL_0387;
			}
		}
		value = new RewardedLuckyDrawData
		{
			TotalPropUses = totalPropUses,
			VictoryCount = victoryCount,
			LastShownVictory = lastShownVictory,
			FirstRewardClaimed = firstRewardClaimed,
			DrawSequence = drawSequence,
			DismissCount = dismissCount,
			BlockedThroughSession = blockedThroughSession,
			PopupUnresolved = popupUnresolved,
			PopupSession = popupSession,
			LastInterruptedSession = lastInterruptedSession,
			PendingTransactionId = pendingTransactionId,
			PendingSlotId = pendingSlotId,
			PendingPropType = pendingPropType,
			PendingCount = pendingCount,
			AdCompleted = adCompleted,
			ExemptNextFrontAd = exemptNextFrontAd,
			ExemptVictory = exemptVictory,
			LastShownLocalDate = lastShownLocalDate
		};
		return;
		IL_0387:
		value.TotalPropUses = totalPropUses;
		value.VictoryCount = victoryCount;
		value.LastShownVictory = lastShownVictory;
		value.FirstRewardClaimed = firstRewardClaimed;
		value.DrawSequence = drawSequence;
		value.DismissCount = dismissCount;
		value.BlockedThroughSession = blockedThroughSession;
		value.PopupUnresolved = popupUnresolved;
		value.PopupSession = popupSession;
		value.LastInterruptedSession = lastInterruptedSession;
		value.PendingTransactionId = pendingTransactionId;
		value.PendingSlotId = pendingSlotId;
		value.PendingPropType = pendingPropType;
		value.PendingCount = pendingCount;
		value.AdCompleted = adCompleted;
		value.ExemptNextFrontAd = exemptNextFrontAd;
		value.ExemptVictory = exemptVictory;
		value.LastShownLocalDate = lastShownLocalDate;
	}
}
