using System.Collections.Generic;
using MemoryPack;
using MemoryPack.Formatters;
using MemoryPack.Internal;

namespace RotateRings.Services;

[MemoryPackable(/*Could not decode attribute arguments.*/)]
public class ReissueRewardTransaction : IMemoryPackable<ReissueRewardTransaction>, IMemoryPackFormatterRegister
{
	[Preserve]
	private sealed class ReissueRewardTransactionFormatter : MemoryPackFormatter<ReissueRewardTransaction>
	{
		[Preserve]
		public override void Serialize(ref MemoryPackWriter writer, ref ReissueRewardTransaction value)
		{
			ReissueRewardTransaction.Serialize(ref writer, ref value);
		}

		[Preserve]
		public override void Deserialize(ref MemoryPackReader reader, ref ReissueRewardTransaction value)
		{
			ReissueRewardTransaction.Deserialize(ref reader, ref value);
		}
	}

	public string TransactionId;

	public ReissueRewardSource Source;

	public string SourceKey;

	public int RewardId;

	public Dictionary<int, int> RewardItems;

	public int Multiplier;

	public int GainReason;

	public string RewardReasonId;

	public string RewardReasonDetail;

	public ReissueRewardStatus Status;

	public long ReservedAt;

	public long ConsumedAt;

	static ReissueRewardTransaction()
	{
		RegisterFormatter();
	}

	[Preserve]
	public static void RegisterFormatter()
	{
		if (!MemoryPackFormatterProvider.IsRegistered<ReissueRewardTransaction>())
		{
			MemoryPackFormatterProvider.Register<ReissueRewardTransaction>((MemoryPackFormatter<ReissueRewardTransaction>)(object)new ReissueRewardTransactionFormatter());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<ReissueRewardTransaction[]>())
		{
			MemoryPackFormatterProvider.Register<ReissueRewardTransaction[]>((MemoryPackFormatter<ReissueRewardTransaction[]>)(object)new ArrayFormatter<ReissueRewardTransaction>());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<ReissueRewardSource>())
		{
			MemoryPackFormatterProvider.Register<ReissueRewardSource>((MemoryPackFormatter<ReissueRewardSource>)(object)new UnmanagedFormatter<ReissueRewardSource>());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<Dictionary<int, int>>())
		{
			MemoryPackFormatterProvider.Register<Dictionary<int, int>>((MemoryPackFormatter<Dictionary<int, int>>)(object)new DictionaryFormatter<int, int>());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<ReissueRewardStatus>())
		{
			MemoryPackFormatterProvider.Register<ReissueRewardStatus>((MemoryPackFormatter<ReissueRewardStatus>)(object)new UnmanagedFormatter<ReissueRewardStatus>());
		}
	}

	[Preserve]
	public static void Serialize(ref MemoryPackWriter writer, ref ReissueRewardTransaction? value)
	{
		if (value == null)
		{
			((MemoryPackWriter)(ref writer)).WriteNullObjectHeader();
			return;
		}
		((MemoryPackWriter)(ref writer)).WriteObjectHeader((byte)12);
		((MemoryPackWriter)(ref writer)).WriteString(value.TransactionId);
		((MemoryPackWriter)(ref writer)).WriteUnmanaged<ReissueRewardSource>(ref value.Source);
		((MemoryPackWriter)(ref writer)).WriteString(value.SourceKey);
		((MemoryPackWriter)(ref writer)).WriteUnmanaged<int>(ref value.RewardId);
		((MemoryPackWriter)(ref writer)).WriteValue<Dictionary<int, int>>(ref value.RewardItems);
		((MemoryPackWriter)(ref writer)).WriteUnmanaged<int, int>(ref value.Multiplier, ref value.GainReason);
		((MemoryPackWriter)(ref writer)).WriteString(value.RewardReasonId);
		((MemoryPackWriter)(ref writer)).WriteString(value.RewardReasonDetail);
		((MemoryPackWriter)(ref writer)).WriteUnmanaged<ReissueRewardStatus, long, long>(ref value.Status, ref value.ReservedAt, ref value.ConsumedAt);
	}

	[Preserve]
	public static void Deserialize(ref MemoryPackReader reader, ref ReissueRewardTransaction? value)
	{
		byte b = default(byte);
		if (!((MemoryPackReader)(ref reader)).TryReadObjectHeader(ref b))
		{
			value = null;
			return;
		}
		ReissueRewardSource source = default(ReissueRewardSource);
		int rewardId = default(int);
		Dictionary<int, int> rewardItems;
		int multiplier = default(int);
		int gainReason = default(int);
		ReissueRewardStatus status = default(ReissueRewardStatus);
		long reservedAt = default(long);
		long consumedAt = default(long);
		string transactionId;
		string sourceKey;
		string rewardReasonId;
		string rewardReasonDetail;
		if (b == 12)
		{
			if (value != null)
			{
				transactionId = value.TransactionId;
				source = value.Source;
				sourceKey = value.SourceKey;
				rewardId = value.RewardId;
				rewardItems = value.RewardItems;
				multiplier = value.Multiplier;
				gainReason = value.GainReason;
				rewardReasonId = value.RewardReasonId;
				rewardReasonDetail = value.RewardReasonDetail;
				status = value.Status;
				reservedAt = value.ReservedAt;
				consumedAt = value.ConsumedAt;
				transactionId = ((MemoryPackReader)(ref reader)).ReadString();
				((MemoryPackReader)(ref reader)).ReadUnmanaged<ReissueRewardSource>(ref source);
				sourceKey = ((MemoryPackReader)(ref reader)).ReadString();
				((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref rewardId);
				((MemoryPackReader)(ref reader)).ReadValue<Dictionary<int, int>>(ref rewardItems);
				((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref multiplier);
				((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref gainReason);
				rewardReasonId = ((MemoryPackReader)(ref reader)).ReadString();
				rewardReasonDetail = ((MemoryPackReader)(ref reader)).ReadString();
				((MemoryPackReader)(ref reader)).ReadUnmanaged<ReissueRewardStatus>(ref status);
				((MemoryPackReader)(ref reader)).ReadUnmanaged<long>(ref reservedAt);
				((MemoryPackReader)(ref reader)).ReadUnmanaged<long>(ref consumedAt);
				goto IL_0280;
			}
			transactionId = ((MemoryPackReader)(ref reader)).ReadString();
			((MemoryPackReader)(ref reader)).ReadUnmanaged<ReissueRewardSource>(ref source);
			sourceKey = ((MemoryPackReader)(ref reader)).ReadString();
			((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref rewardId);
			rewardItems = ((MemoryPackReader)(ref reader)).ReadValue<Dictionary<int, int>>();
			((MemoryPackReader)(ref reader)).ReadUnmanaged<int, int>(ref multiplier, ref gainReason);
			rewardReasonId = ((MemoryPackReader)(ref reader)).ReadString();
			rewardReasonDetail = ((MemoryPackReader)(ref reader)).ReadString();
			((MemoryPackReader)(ref reader)).ReadUnmanaged<ReissueRewardStatus, long, long>(ref status, ref reservedAt, ref consumedAt);
		}
		else
		{
			if (b > 12)
			{
				MemoryPackSerializationException.ThrowInvalidPropertyCount(typeof(ReissueRewardTransaction), (byte)12, b);
				return;
			}
			if (value == null)
			{
				transactionId = null;
				source = (ReissueRewardSource)0;
				sourceKey = null;
				rewardId = 0;
				rewardItems = null;
				multiplier = 0;
				gainReason = 0;
				rewardReasonId = null;
				rewardReasonDetail = null;
				status = (ReissueRewardStatus)0;
				reservedAt = 0L;
				consumedAt = 0L;
			}
			else
			{
				transactionId = value.TransactionId;
				source = value.Source;
				sourceKey = value.SourceKey;
				rewardId = value.RewardId;
				rewardItems = value.RewardItems;
				multiplier = value.Multiplier;
				gainReason = value.GainReason;
				rewardReasonId = value.RewardReasonId;
				rewardReasonDetail = value.RewardReasonDetail;
				status = value.Status;
				reservedAt = value.ReservedAt;
				consumedAt = value.ConsumedAt;
			}
			if (b != 0)
			{
				transactionId = ((MemoryPackReader)(ref reader)).ReadString();
				if (b != 1)
				{
					((MemoryPackReader)(ref reader)).ReadUnmanaged<ReissueRewardSource>(ref source);
					if (b != 2)
					{
						sourceKey = ((MemoryPackReader)(ref reader)).ReadString();
						if (b != 3)
						{
							((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref rewardId);
							if (b != 4)
							{
								((MemoryPackReader)(ref reader)).ReadValue<Dictionary<int, int>>(ref rewardItems);
								if (b != 5)
								{
									((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref multiplier);
									if (b != 6)
									{
										((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref gainReason);
										if (b != 7)
										{
											rewardReasonId = ((MemoryPackReader)(ref reader)).ReadString();
											if (b != 8)
											{
												rewardReasonDetail = ((MemoryPackReader)(ref reader)).ReadString();
												if (b != 9)
												{
													((MemoryPackReader)(ref reader)).ReadUnmanaged<ReissueRewardStatus>(ref status);
													if (b != 10)
													{
														((MemoryPackReader)(ref reader)).ReadUnmanaged<long>(ref reservedAt);
														if (b != 11)
														{
															((MemoryPackReader)(ref reader)).ReadUnmanaged<long>(ref consumedAt);
															_ = 12;
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
				goto IL_0280;
			}
		}
		value = new ReissueRewardTransaction
		{
			TransactionId = transactionId,
			Source = source,
			SourceKey = sourceKey,
			RewardId = rewardId,
			RewardItems = rewardItems,
			Multiplier = multiplier,
			GainReason = gainReason,
			RewardReasonId = rewardReasonId,
			RewardReasonDetail = rewardReasonDetail,
			Status = status,
			ReservedAt = reservedAt,
			ConsumedAt = consumedAt
		};
		return;
		IL_0280:
		value.TransactionId = transactionId;
		value.Source = source;
		value.SourceKey = sourceKey;
		value.RewardId = rewardId;
		value.RewardItems = rewardItems;
		value.Multiplier = multiplier;
		value.GainReason = gainReason;
		value.RewardReasonId = rewardReasonId;
		value.RewardReasonDetail = rewardReasonDetail;
		value.Status = status;
		value.ReservedAt = reservedAt;
		value.ConsumedAt = consumedAt;
	}
}
