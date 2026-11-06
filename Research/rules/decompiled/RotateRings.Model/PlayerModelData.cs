using System;
using System.Collections.Generic;
using MemoryPack;
using MemoryPack.Formatters;
using MemoryPack.Internal;
using XingYun.Save;

namespace RotateRings.Model;

[MemoryPackable(/*Could not decode attribute arguments.*/)]
public class PlayerModelData : ISaveDefaultInitializer, IMemoryPackable<PlayerModelData>, IMemoryPackFormatterRegister
{
	[Preserve]
	private sealed class PlayerModelDataFormatter : MemoryPackFormatter<PlayerModelData>
	{
		[Preserve]
		public override void Serialize(ref MemoryPackWriter writer, ref PlayerModelData value)
		{
			PlayerModelData.Serialize(ref writer, ref value);
		}

		[Preserve]
		public override void Deserialize(ref MemoryPackReader reader, ref PlayerModelData value)
		{
			PlayerModelData.Deserialize(ref reader, ref value);
		}
	}

	public string PlayerName;

	public string PlayerNameLanguage;

	public int HeadId;

	public int HeadFrameId;

	public bool IsNewUser;

	public bool IsPaidUser;

	public int GetAdRewardCount;

	public long LastInterAdTimestamp;

	public long LastLevelExitUtcSeconds;

	public int ColdLaunchCount;

	public List<int> RecentColdStartLevelCounts = new List<int>(3);

	public int RankOpenLevelLocked;

	public float AverageRecentColdStartLevels
	{
		get
		{
			if (RecentColdStartLevelCounts == null || RecentColdStartLevelCounts.Count == 0)
			{
				return 0f;
			}
			float num = 0f;
			for (int i = 0; i < RecentColdStartLevelCounts.Count; i++)
			{
				num += (float)RecentColdStartLevelCounts[i];
			}
			return num / (float)RecentColdStartLevelCounts.Count;
		}
	}

	public void PushColdStartSession()
	{
		if (RecentColdStartLevelCounts == null)
		{
			RecentColdStartLevelCounts = new List<int>(3);
		}
		RecentColdStartLevelCounts.Add(0);
		while (RecentColdStartLevelCounts.Count > 3)
		{
			RecentColdStartLevelCounts.RemoveAt(0);
		}
	}

	public void IncrementColdStartLevelWin()
	{
		EnsureRecentColdStartLevelHistoryMigrated();
		if (RecentColdStartLevelCounts != null && RecentColdStartLevelCounts.Count > 0)
		{
			List<int> recentColdStartLevelCounts = RecentColdStartLevelCounts;
			recentColdStartLevelCounts[recentColdStartLevelCounts.Count - 1]++;
		}
	}

	public void EnsureRecentColdStartLevelHistoryMigrated()
	{
		if (RecentColdStartLevelCounts == null)
		{
			RecentColdStartLevelCounts = new List<int>(3);
		}
		if (RecentColdStartLevelCounts.Count <= 0 && ColdLaunchCount > 0)
		{
			int num = Math.Min(ColdLaunchCount, 3);
			for (int i = 0; i < num; i++)
			{
				RecentColdStartLevelCounts.Add(1);
			}
		}
	}

	public void OnCreateDefault()
	{
		HeadId = 1;
		HeadFrameId = 1;
		IsPaidUser = false;
		IsNewUser = true;
	}

	static PlayerModelData()
	{
		RegisterFormatter();
	}

	[Preserve]
	public static void RegisterFormatter()
	{
		if (!MemoryPackFormatterProvider.IsRegistered<PlayerModelData>())
		{
			MemoryPackFormatterProvider.Register<PlayerModelData>((MemoryPackFormatter<PlayerModelData>)(object)new PlayerModelDataFormatter());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<PlayerModelData[]>())
		{
			MemoryPackFormatterProvider.Register<PlayerModelData[]>((MemoryPackFormatter<PlayerModelData[]>)(object)new ArrayFormatter<PlayerModelData>());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<List<int>>())
		{
			MemoryPackFormatterProvider.Register<List<int>>((MemoryPackFormatter<List<int>>)(object)new ListFormatter<int>());
		}
	}

	[Preserve]
	public static void Serialize(ref MemoryPackWriter writer, ref PlayerModelData? value)
	{
		if (value == null)
		{
			((MemoryPackWriter)(ref writer)).WriteNullObjectHeader();
			return;
		}
		((MemoryPackWriter)(ref writer)).WriteObjectHeader((byte)13);
		((MemoryPackWriter)(ref writer)).WriteString(value.PlayerName);
		((MemoryPackWriter)(ref writer)).WriteString(value.PlayerNameLanguage);
		((MemoryPackWriter)(ref writer)).WriteUnmanaged<int, int, bool, bool, int, long, long, int>(ref value.HeadId, ref value.HeadFrameId, ref value.IsNewUser, ref value.IsPaidUser, ref value.GetAdRewardCount, ref value.LastInterAdTimestamp, ref value.LastLevelExitUtcSeconds, ref value.ColdLaunchCount);
		((MemoryPackWriter)(ref writer)).WriteValue<List<int>>(ref value.RecentColdStartLevelCounts);
		ref int rankOpenLevelLocked = ref value.RankOpenLevelLocked;
		float averageRecentColdStartLevels = value.AverageRecentColdStartLevels;
		((MemoryPackWriter)(ref writer)).WriteUnmanaged<int, float>(ref rankOpenLevelLocked, ref averageRecentColdStartLevels);
	}

	[Preserve]
	public static void Deserialize(ref MemoryPackReader reader, ref PlayerModelData? value)
	{
		byte b = default(byte);
		if (!((MemoryPackReader)(ref reader)).TryReadObjectHeader(ref b))
		{
			value = null;
			return;
		}
		int headId = default(int);
		int headFrameId = default(int);
		bool isNewUser = default(bool);
		bool isPaidUser = default(bool);
		int getAdRewardCount = default(int);
		long lastInterAdTimestamp = default(long);
		long lastLevelExitUtcSeconds = default(long);
		int coldLaunchCount = default(int);
		List<int> recentColdStartLevelCounts;
		int rankOpenLevelLocked = default(int);
		string playerName;
		string playerNameLanguage;
		if (b == 13)
		{
			float averageRecentColdStartLevels = default(float);
			if (value != null)
			{
				playerName = value.PlayerName;
				playerNameLanguage = value.PlayerNameLanguage;
				headId = value.HeadId;
				headFrameId = value.HeadFrameId;
				isNewUser = value.IsNewUser;
				isPaidUser = value.IsPaidUser;
				getAdRewardCount = value.GetAdRewardCount;
				lastInterAdTimestamp = value.LastInterAdTimestamp;
				lastLevelExitUtcSeconds = value.LastLevelExitUtcSeconds;
				coldLaunchCount = value.ColdLaunchCount;
				recentColdStartLevelCounts = value.RecentColdStartLevelCounts;
				rankOpenLevelLocked = value.RankOpenLevelLocked;
				averageRecentColdStartLevels = value.AverageRecentColdStartLevels;
				playerName = ((MemoryPackReader)(ref reader)).ReadString();
				playerNameLanguage = ((MemoryPackReader)(ref reader)).ReadString();
				((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref headId);
				((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref headFrameId);
				((MemoryPackReader)(ref reader)).ReadUnmanaged<bool>(ref isNewUser);
				((MemoryPackReader)(ref reader)).ReadUnmanaged<bool>(ref isPaidUser);
				((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref getAdRewardCount);
				((MemoryPackReader)(ref reader)).ReadUnmanaged<long>(ref lastInterAdTimestamp);
				((MemoryPackReader)(ref reader)).ReadUnmanaged<long>(ref lastLevelExitUtcSeconds);
				((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref coldLaunchCount);
				((MemoryPackReader)(ref reader)).ReadValue<List<int>>(ref recentColdStartLevelCounts);
				((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref rankOpenLevelLocked);
				((MemoryPackReader)(ref reader)).ReadUnmanaged<float>(ref averageRecentColdStartLevels);
				goto IL_029b;
			}
			playerName = ((MemoryPackReader)(ref reader)).ReadString();
			playerNameLanguage = ((MemoryPackReader)(ref reader)).ReadString();
			((MemoryPackReader)(ref reader)).ReadUnmanaged<int, int, bool, bool, int, long, long, int>(ref headId, ref headFrameId, ref isNewUser, ref isPaidUser, ref getAdRewardCount, ref lastInterAdTimestamp, ref lastLevelExitUtcSeconds, ref coldLaunchCount);
			recentColdStartLevelCounts = ((MemoryPackReader)(ref reader)).ReadValue<List<int>>();
			((MemoryPackReader)(ref reader)).ReadUnmanaged<int, float>(ref rankOpenLevelLocked, ref averageRecentColdStartLevels);
		}
		else
		{
			if (b > 13)
			{
				MemoryPackSerializationException.ThrowInvalidPropertyCount(typeof(PlayerModelData), (byte)13, b);
				return;
			}
			float averageRecentColdStartLevels;
			if (value == null)
			{
				playerName = null;
				playerNameLanguage = null;
				headId = 0;
				headFrameId = 0;
				isNewUser = false;
				isPaidUser = false;
				getAdRewardCount = 0;
				lastInterAdTimestamp = 0L;
				lastLevelExitUtcSeconds = 0L;
				coldLaunchCount = 0;
				recentColdStartLevelCounts = null;
				rankOpenLevelLocked = 0;
				averageRecentColdStartLevels = 0f;
			}
			else
			{
				playerName = value.PlayerName;
				playerNameLanguage = value.PlayerNameLanguage;
				headId = value.HeadId;
				headFrameId = value.HeadFrameId;
				isNewUser = value.IsNewUser;
				isPaidUser = value.IsPaidUser;
				getAdRewardCount = value.GetAdRewardCount;
				lastInterAdTimestamp = value.LastInterAdTimestamp;
				lastLevelExitUtcSeconds = value.LastLevelExitUtcSeconds;
				coldLaunchCount = value.ColdLaunchCount;
				recentColdStartLevelCounts = value.RecentColdStartLevelCounts;
				rankOpenLevelLocked = value.RankOpenLevelLocked;
				averageRecentColdStartLevels = value.AverageRecentColdStartLevels;
			}
			if (b != 0)
			{
				playerName = ((MemoryPackReader)(ref reader)).ReadString();
				if (b != 1)
				{
					playerNameLanguage = ((MemoryPackReader)(ref reader)).ReadString();
					if (b != 2)
					{
						((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref headId);
						if (b != 3)
						{
							((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref headFrameId);
							if (b != 4)
							{
								((MemoryPackReader)(ref reader)).ReadUnmanaged<bool>(ref isNewUser);
								if (b != 5)
								{
									((MemoryPackReader)(ref reader)).ReadUnmanaged<bool>(ref isPaidUser);
									if (b != 6)
									{
										((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref getAdRewardCount);
										if (b != 7)
										{
											((MemoryPackReader)(ref reader)).ReadUnmanaged<long>(ref lastInterAdTimestamp);
											if (b != 8)
											{
												((MemoryPackReader)(ref reader)).ReadUnmanaged<long>(ref lastLevelExitUtcSeconds);
												if (b != 9)
												{
													((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref coldLaunchCount);
													if (b != 10)
													{
														((MemoryPackReader)(ref reader)).ReadValue<List<int>>(ref recentColdStartLevelCounts);
														if (b != 11)
														{
															((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref rankOpenLevelLocked);
															if (b != 12)
															{
																((MemoryPackReader)(ref reader)).ReadUnmanaged<float>(ref averageRecentColdStartLevels);
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
				goto IL_029b;
			}
		}
		value = new PlayerModelData
		{
			PlayerName = playerName,
			PlayerNameLanguage = playerNameLanguage,
			HeadId = headId,
			HeadFrameId = headFrameId,
			IsNewUser = isNewUser,
			IsPaidUser = isPaidUser,
			GetAdRewardCount = getAdRewardCount,
			LastInterAdTimestamp = lastInterAdTimestamp,
			LastLevelExitUtcSeconds = lastLevelExitUtcSeconds,
			ColdLaunchCount = coldLaunchCount,
			RecentColdStartLevelCounts = recentColdStartLevelCounts,
			RankOpenLevelLocked = rankOpenLevelLocked
		};
		return;
		IL_029b:
		value.PlayerName = playerName;
		value.PlayerNameLanguage = playerNameLanguage;
		value.HeadId = headId;
		value.HeadFrameId = headFrameId;
		value.IsNewUser = isNewUser;
		value.IsPaidUser = isPaidUser;
		value.GetAdRewardCount = getAdRewardCount;
		value.LastInterAdTimestamp = lastInterAdTimestamp;
		value.LastLevelExitUtcSeconds = lastLevelExitUtcSeconds;
		value.ColdLaunchCount = coldLaunchCount;
		value.RecentColdStartLevelCounts = recentColdStartLevelCounts;
		value.RankOpenLevelLocked = rankOpenLevelLocked;
	}
}
