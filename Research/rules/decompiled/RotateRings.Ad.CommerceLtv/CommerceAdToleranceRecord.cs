using System.Collections.Generic;
using MemoryPack;
using MemoryPack.Formatters;
using MemoryPack.Internal;
using XingYun.Save;

namespace RotateRings.Ad.CommerceLtv;

[MemoryPackable(/*Could not decode attribute arguments.*/)]
public class CommerceAdToleranceRecord : ISaveDefaultInitializer, IMemoryPackable<CommerceAdToleranceRecord>, IMemoryPackFormatterRegister
{
	[Preserve]
	private sealed class CommerceAdToleranceRecordFormatter : MemoryPackFormatter<CommerceAdToleranceRecord>
	{
		[Preserve]
		public override void Serialize(ref MemoryPackWriter writer, ref CommerceAdToleranceRecord value)
		{
			CommerceAdToleranceRecord.Serialize(ref writer, ref value);
		}

		[Preserve]
		public override void Deserialize(ref MemoryPackReader reader, ref CommerceAdToleranceRecord value)
		{
			CommerceAdToleranceRecord.Deserialize(ref reader, ref value);
		}
	}

	public const int MaxGameWindow = 7;

	public List<int> RewardShowsPerGame = new List<int>(7);

	public int CurrentGameRewardShows;

	public void OnCreateDefault()
	{
		RewardShowsPerGame = new List<int>(7);
		CurrentGameRewardShows = 0;
	}

	static CommerceAdToleranceRecord()
	{
		RegisterFormatter();
	}

	[Preserve]
	public static void RegisterFormatter()
	{
		if (!MemoryPackFormatterProvider.IsRegistered<CommerceAdToleranceRecord>())
		{
			MemoryPackFormatterProvider.Register<CommerceAdToleranceRecord>((MemoryPackFormatter<CommerceAdToleranceRecord>)(object)new CommerceAdToleranceRecordFormatter());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<CommerceAdToleranceRecord[]>())
		{
			MemoryPackFormatterProvider.Register<CommerceAdToleranceRecord[]>((MemoryPackFormatter<CommerceAdToleranceRecord[]>)(object)new ArrayFormatter<CommerceAdToleranceRecord>());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<List<int>>())
		{
			MemoryPackFormatterProvider.Register<List<int>>((MemoryPackFormatter<List<int>>)(object)new ListFormatter<int>());
		}
	}

	[Preserve]
	public static void Serialize(ref MemoryPackWriter writer, ref CommerceAdToleranceRecord? value)
	{
		if (value == null)
		{
			((MemoryPackWriter)(ref writer)).WriteNullObjectHeader();
			return;
		}
		((MemoryPackWriter)(ref writer)).WriteObjectHeader((byte)2);
		((MemoryPackWriter)(ref writer)).WriteValue<List<int>>(ref value.RewardShowsPerGame);
		((MemoryPackWriter)(ref writer)).WriteUnmanaged<int>(ref value.CurrentGameRewardShows);
	}

	[Preserve]
	public static void Deserialize(ref MemoryPackReader reader, ref CommerceAdToleranceRecord? value)
	{
		byte b = default(byte);
		if (!((MemoryPackReader)(ref reader)).TryReadObjectHeader(ref b))
		{
			value = null;
			return;
		}
		List<int> rewardShowsPerGame;
		int currentGameRewardShows = default(int);
		if (b == 2)
		{
			if (value != null)
			{
				rewardShowsPerGame = value.RewardShowsPerGame;
				currentGameRewardShows = value.CurrentGameRewardShows;
				((MemoryPackReader)(ref reader)).ReadValue<List<int>>(ref rewardShowsPerGame);
				((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref currentGameRewardShows);
				goto IL_009b;
			}
			rewardShowsPerGame = ((MemoryPackReader)(ref reader)).ReadValue<List<int>>();
			((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref currentGameRewardShows);
		}
		else
		{
			if (b > 2)
			{
				MemoryPackSerializationException.ThrowInvalidPropertyCount(typeof(CommerceAdToleranceRecord), (byte)2, b);
				return;
			}
			if (value == null)
			{
				rewardShowsPerGame = null;
				currentGameRewardShows = 0;
			}
			else
			{
				rewardShowsPerGame = value.RewardShowsPerGame;
				currentGameRewardShows = value.CurrentGameRewardShows;
			}
			if (b != 0)
			{
				((MemoryPackReader)(ref reader)).ReadValue<List<int>>(ref rewardShowsPerGame);
				if (b != 1)
				{
					((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref currentGameRewardShows);
					_ = 2;
				}
			}
			if (value != null)
			{
				goto IL_009b;
			}
		}
		value = new CommerceAdToleranceRecord
		{
			RewardShowsPerGame = rewardShowsPerGame,
			CurrentGameRewardShows = currentGameRewardShows
		};
		return;
		IL_009b:
		value.RewardShowsPerGame = rewardShowsPerGame;
		value.CurrentGameRewardShows = currentGameRewardShows;
	}
}
