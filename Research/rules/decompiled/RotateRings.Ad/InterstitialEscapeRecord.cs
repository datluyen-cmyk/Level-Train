using System.Collections.Generic;
using MemoryPack;
using MemoryPack.Formatters;
using MemoryPack.Internal;
using XingYun.Save;

namespace RotateRings.Ad;

[MemoryPackable(/*Could not decode attribute arguments.*/)]
public class InterstitialEscapeRecord : ISaveDefaultInitializer, IMemoryPackable<InterstitialEscapeRecord>, IMemoryPackFormatterRegister
{
	[Preserve]
	private sealed class InterstitialEscapeRecordFormatter : MemoryPackFormatter<InterstitialEscapeRecord>
	{
		[Preserve]
		public override void Serialize(ref MemoryPackWriter writer, ref InterstitialEscapeRecord value)
		{
			InterstitialEscapeRecord.Serialize(ref writer, ref value);
		}

		[Preserve]
		public override void Deserialize(ref MemoryPackReader reader, ref InterstitialEscapeRecord value)
		{
			InterstitialEscapeRecord.Deserialize(ref reader, ref value);
		}
	}

	public List<bool> EscapeList = new List<bool>(3);

	public int TotalCallCount;

	public int EscapeSkipCount;

	public int EscapeSkipBatchTotal;

	public bool IsInEscapeSkipPeriod;

	public long EscapeSkipExpireTimestamp;

	public int FirstInterstitialLevelAfterEscape;

	public bool InterstitialClosePending;

	public bool PendingInterstitialIsAfterShow;

	public int ShowAdSuccessfulCount;

	public int ShowAdPostSuccessfulCount;

	public void OnCreateDefault()
	{
		EscapeList = new List<bool>(3);
		TotalCallCount = 0;
		EscapeSkipCount = 0;
		EscapeSkipBatchTotal = 0;
		IsInEscapeSkipPeriod = false;
		EscapeSkipExpireTimestamp = 0L;
		FirstInterstitialLevelAfterEscape = 0;
		InterstitialClosePending = false;
		PendingInterstitialIsAfterShow = false;
		ShowAdSuccessfulCount = 0;
		ShowAdPostSuccessfulCount = 0;
	}

	static InterstitialEscapeRecord()
	{
		RegisterFormatter();
	}

	[Preserve]
	public static void RegisterFormatter()
	{
		if (!MemoryPackFormatterProvider.IsRegistered<InterstitialEscapeRecord>())
		{
			MemoryPackFormatterProvider.Register<InterstitialEscapeRecord>((MemoryPackFormatter<InterstitialEscapeRecord>)(object)new InterstitialEscapeRecordFormatter());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<InterstitialEscapeRecord[]>())
		{
			MemoryPackFormatterProvider.Register<InterstitialEscapeRecord[]>((MemoryPackFormatter<InterstitialEscapeRecord[]>)(object)new ArrayFormatter<InterstitialEscapeRecord>());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<List<bool>>())
		{
			MemoryPackFormatterProvider.Register<List<bool>>((MemoryPackFormatter<List<bool>>)(object)new ListFormatter<bool>());
		}
	}

	[Preserve]
	public static void Serialize(ref MemoryPackWriter writer, ref InterstitialEscapeRecord? value)
	{
		if (value == null)
		{
			((MemoryPackWriter)(ref writer)).WriteNullObjectHeader();
			return;
		}
		((MemoryPackWriter)(ref writer)).WriteObjectHeader((byte)11);
		((MemoryPackWriter)(ref writer)).WriteValue<List<bool>>(ref value.EscapeList);
		((MemoryPackWriter)(ref writer)).WriteUnmanaged<int, int, int, bool, long, int, bool, bool, int, int>(ref value.TotalCallCount, ref value.EscapeSkipCount, ref value.EscapeSkipBatchTotal, ref value.IsInEscapeSkipPeriod, ref value.EscapeSkipExpireTimestamp, ref value.FirstInterstitialLevelAfterEscape, ref value.InterstitialClosePending, ref value.PendingInterstitialIsAfterShow, ref value.ShowAdSuccessfulCount, ref value.ShowAdPostSuccessfulCount);
	}

	[Preserve]
	public static void Deserialize(ref MemoryPackReader reader, ref InterstitialEscapeRecord? value)
	{
		byte b = default(byte);
		if (!((MemoryPackReader)(ref reader)).TryReadObjectHeader(ref b))
		{
			value = null;
			return;
		}
		List<bool> escapeList;
		int totalCallCount = default(int);
		int escapeSkipCount = default(int);
		int escapeSkipBatchTotal = default(int);
		bool isInEscapeSkipPeriod = default(bool);
		long escapeSkipExpireTimestamp = default(long);
		int firstInterstitialLevelAfterEscape = default(int);
		bool interstitialClosePending = default(bool);
		bool pendingInterstitialIsAfterShow = default(bool);
		int showAdSuccessfulCount = default(int);
		int showAdPostSuccessfulCount = default(int);
		if (b == 11)
		{
			if (value != null)
			{
				escapeList = value.EscapeList;
				totalCallCount = value.TotalCallCount;
				escapeSkipCount = value.EscapeSkipCount;
				escapeSkipBatchTotal = value.EscapeSkipBatchTotal;
				isInEscapeSkipPeriod = value.IsInEscapeSkipPeriod;
				escapeSkipExpireTimestamp = value.EscapeSkipExpireTimestamp;
				firstInterstitialLevelAfterEscape = value.FirstInterstitialLevelAfterEscape;
				interstitialClosePending = value.InterstitialClosePending;
				pendingInterstitialIsAfterShow = value.PendingInterstitialIsAfterShow;
				showAdSuccessfulCount = value.ShowAdSuccessfulCount;
				showAdPostSuccessfulCount = value.ShowAdPostSuccessfulCount;
				((MemoryPackReader)(ref reader)).ReadValue<List<bool>>(ref escapeList);
				((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref totalCallCount);
				((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref escapeSkipCount);
				((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref escapeSkipBatchTotal);
				((MemoryPackReader)(ref reader)).ReadUnmanaged<bool>(ref isInEscapeSkipPeriod);
				((MemoryPackReader)(ref reader)).ReadUnmanaged<long>(ref escapeSkipExpireTimestamp);
				((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref firstInterstitialLevelAfterEscape);
				((MemoryPackReader)(ref reader)).ReadUnmanaged<bool>(ref interstitialClosePending);
				((MemoryPackReader)(ref reader)).ReadUnmanaged<bool>(ref pendingInterstitialIsAfterShow);
				((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref showAdSuccessfulCount);
				((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref showAdPostSuccessfulCount);
				goto IL_022b;
			}
			escapeList = ((MemoryPackReader)(ref reader)).ReadValue<List<bool>>();
			((MemoryPackReader)(ref reader)).ReadUnmanaged<int, int, int, bool, long, int, bool, bool, int, int>(ref totalCallCount, ref escapeSkipCount, ref escapeSkipBatchTotal, ref isInEscapeSkipPeriod, ref escapeSkipExpireTimestamp, ref firstInterstitialLevelAfterEscape, ref interstitialClosePending, ref pendingInterstitialIsAfterShow, ref showAdSuccessfulCount, ref showAdPostSuccessfulCount);
		}
		else
		{
			if (b > 11)
			{
				MemoryPackSerializationException.ThrowInvalidPropertyCount(typeof(InterstitialEscapeRecord), (byte)11, b);
				return;
			}
			if (value == null)
			{
				escapeList = null;
				totalCallCount = 0;
				escapeSkipCount = 0;
				escapeSkipBatchTotal = 0;
				isInEscapeSkipPeriod = false;
				escapeSkipExpireTimestamp = 0L;
				firstInterstitialLevelAfterEscape = 0;
				interstitialClosePending = false;
				pendingInterstitialIsAfterShow = false;
				showAdSuccessfulCount = 0;
				showAdPostSuccessfulCount = 0;
			}
			else
			{
				escapeList = value.EscapeList;
				totalCallCount = value.TotalCallCount;
				escapeSkipCount = value.EscapeSkipCount;
				escapeSkipBatchTotal = value.EscapeSkipBatchTotal;
				isInEscapeSkipPeriod = value.IsInEscapeSkipPeriod;
				escapeSkipExpireTimestamp = value.EscapeSkipExpireTimestamp;
				firstInterstitialLevelAfterEscape = value.FirstInterstitialLevelAfterEscape;
				interstitialClosePending = value.InterstitialClosePending;
				pendingInterstitialIsAfterShow = value.PendingInterstitialIsAfterShow;
				showAdSuccessfulCount = value.ShowAdSuccessfulCount;
				showAdPostSuccessfulCount = value.ShowAdPostSuccessfulCount;
			}
			if (b != 0)
			{
				((MemoryPackReader)(ref reader)).ReadValue<List<bool>>(ref escapeList);
				if (b != 1)
				{
					((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref totalCallCount);
					if (b != 2)
					{
						((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref escapeSkipCount);
						if (b != 3)
						{
							((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref escapeSkipBatchTotal);
							if (b != 4)
							{
								((MemoryPackReader)(ref reader)).ReadUnmanaged<bool>(ref isInEscapeSkipPeriod);
								if (b != 5)
								{
									((MemoryPackReader)(ref reader)).ReadUnmanaged<long>(ref escapeSkipExpireTimestamp);
									if (b != 6)
									{
										((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref firstInterstitialLevelAfterEscape);
										if (b != 7)
										{
											((MemoryPackReader)(ref reader)).ReadUnmanaged<bool>(ref interstitialClosePending);
											if (b != 8)
											{
												((MemoryPackReader)(ref reader)).ReadUnmanaged<bool>(ref pendingInterstitialIsAfterShow);
												if (b != 9)
												{
													((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref showAdSuccessfulCount);
													if (b != 10)
													{
														((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref showAdPostSuccessfulCount);
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
				goto IL_022b;
			}
		}
		value = new InterstitialEscapeRecord
		{
			EscapeList = escapeList,
			TotalCallCount = totalCallCount,
			EscapeSkipCount = escapeSkipCount,
			EscapeSkipBatchTotal = escapeSkipBatchTotal,
			IsInEscapeSkipPeriod = isInEscapeSkipPeriod,
			EscapeSkipExpireTimestamp = escapeSkipExpireTimestamp,
			FirstInterstitialLevelAfterEscape = firstInterstitialLevelAfterEscape,
			InterstitialClosePending = interstitialClosePending,
			PendingInterstitialIsAfterShow = pendingInterstitialIsAfterShow,
			ShowAdSuccessfulCount = showAdSuccessfulCount,
			ShowAdPostSuccessfulCount = showAdPostSuccessfulCount
		};
		return;
		IL_022b:
		value.EscapeList = escapeList;
		value.TotalCallCount = totalCallCount;
		value.EscapeSkipCount = escapeSkipCount;
		value.EscapeSkipBatchTotal = escapeSkipBatchTotal;
		value.IsInEscapeSkipPeriod = isInEscapeSkipPeriod;
		value.EscapeSkipExpireTimestamp = escapeSkipExpireTimestamp;
		value.FirstInterstitialLevelAfterEscape = firstInterstitialLevelAfterEscape;
		value.InterstitialClosePending = interstitialClosePending;
		value.PendingInterstitialIsAfterShow = pendingInterstitialIsAfterShow;
		value.ShowAdSuccessfulCount = showAdSuccessfulCount;
		value.ShowAdPostSuccessfulCount = showAdPostSuccessfulCount;
	}
}
