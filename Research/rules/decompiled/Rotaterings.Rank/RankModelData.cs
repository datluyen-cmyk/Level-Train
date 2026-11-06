using System.Collections.Generic;
using MemoryPack;
using MemoryPack.Formatters;
using MemoryPack.Internal;
using XingYun.Save;
using XingYun.Utility;

namespace Rotaterings.Rank;

[MemoryPackable(/*Could not decode attribute arguments.*/)]
public class RankModelData : ISaveDefaultInitializer, IMemoryPackable<RankModelData>, IMemoryPackFormatterRegister
{
	[Preserve]
	private sealed class RankModelDataFormatter : MemoryPackFormatter<RankModelData>
	{
		[Preserve]
		public override void Serialize(ref MemoryPackWriter writer, ref RankModelData value)
		{
			RankModelData.Serialize(ref writer, ref value);
		}

		[Preserve]
		public override void Deserialize(ref MemoryPackReader reader, ref RankModelData value)
		{
			RankModelData.Deserialize(ref reader, ref value);
		}
	}

	public int Period;

	public long EndTimeStamp;

	public bool IsClaimReward;

	public long NextOpenTimeStamp;

	public List<RankRobotData> Robots = new List<RankRobotData>(50);

	public List<int> UsedRobotNameIdsThisActivity = new List<int>(64);

	public List<int> UsedRobotNameIdCooldownLastActivity = new List<int>(64);

	public int LastPeriodIndex = -1;

	public int PlayerRankScore;

	public int MultiplierTier;

	public int LastMultiplierTier;

	public int CurrentSettleScore;

	public int CurrentSettleRanking;

	public bool HasPlayerEnteredRank;

	public bool HasShownActivityOpenPopup;

	public int ClearLevels;

	public int ExpGames;

	public int ActivityActiveGamesTotal;

	public int ActivityActiveDays;

	public long TimeRefresh;

	public int StarCountOneRound;

	public int CurrentCollectCount;

	public int TotalCollectCards;

	public bool HasReportedCampaignEnd;

	public bool IsPeriodSettled;

	public bool HasEnteredRewardPage;

	public int WinsAfterPeriodSettled;

	public int OpenScoreAfterPeriodSettled;

	public long CooldownExitTimestamp;

	public bool IsHalloweenTheme;

	public int RobotNameConfig;

	public int RankRewardConfig;

	public bool HasActivityRecord
	{
		get
		{
			if (EndTimeStamp <= 0)
			{
				return Period > 0;
			}
			return true;
		}
	}

	public bool IsActivityEnd
	{
		get
		{
			if (HasActivityRecord)
			{
				if (EndTimeStamp > 0)
				{
					return Time.GetCurrentTimestamp() >= EndTimeStamp;
				}
				return true;
			}
			return false;
		}
	}

	public bool IsEndSettlementProcessed
	{
		get
		{
			if (!IsPeriodSettled)
			{
				return IsClaimReward;
			}
			return true;
		}
	}

	public bool IsPeriodSettlePending
	{
		get
		{
			if (IsActivityEnd)
			{
				return !IsEndSettlementProcessed;
			}
			return false;
		}
	}

	public void OnCreateDefault()
	{
		if (Robots == null)
		{
			Robots = new List<RankRobotData>(50);
		}
		if (UsedRobotNameIdsThisActivity == null)
		{
			UsedRobotNameIdsThisActivity = new List<int>(64);
		}
		if (UsedRobotNameIdCooldownLastActivity == null)
		{
			UsedRobotNameIdCooldownLastActivity = new List<int>(64);
		}
		RobotNameConfig = 0;
		ResetForNewPeriod();
	}

	public void ResetForNewPeriod()
	{
		IsClaimReward = false;
		PlayerRankScore = 0;
		MultiplierTier = 0;
		LastMultiplierTier = 0;
		CurrentSettleScore = 0;
		CurrentSettleRanking = 0;
		HasPlayerEnteredRank = false;
		HasShownActivityOpenPopup = false;
		ClearLevels = 0;
		ExpGames = 0;
		ActivityActiveGamesTotal = 0;
		ActivityActiveDays = 0;
		TimeRefresh = 0L;
		StarCountOneRound = 0;
		CurrentCollectCount = 0;
		TotalCollectCards = 0;
		HasReportedCampaignEnd = false;
		IsPeriodSettled = false;
		HasEnteredRewardPage = false;
		WinsAfterPeriodSettled = 0;
		OpenScoreAfterPeriodSettled = 0;
		CooldownExitTimestamp = 0L;
		IsHalloweenTheme = false;
		RankRewardConfig = 0;
	}

	public void ResetToInitial()
	{
		ResetForNewPeriod();
		Period = 0;
		EndTimeStamp = 0L;
		NextOpenTimeStamp = 0L;
		LastPeriodIndex = -1;
		RobotNameConfig = 0;
		if (Robots == null)
		{
			Robots = new List<RankRobotData>(50);
		}
		if (UsedRobotNameIdsThisActivity == null)
		{
			UsedRobotNameIdsThisActivity = new List<int>(64);
		}
		if (UsedRobotNameIdCooldownLastActivity == null)
		{
			UsedRobotNameIdCooldownLastActivity = new List<int>(64);
		}
		Robots.Clear();
		UsedRobotNameIdsThisActivity.Clear();
		UsedRobotNameIdCooldownLastActivity.Clear();
	}

	static RankModelData()
	{
		RegisterFormatter();
	}

	[Preserve]
	public static void RegisterFormatter()
	{
		if (!MemoryPackFormatterProvider.IsRegistered<RankModelData>())
		{
			MemoryPackFormatterProvider.Register<RankModelData>((MemoryPackFormatter<RankModelData>)(object)new RankModelDataFormatter());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<RankModelData[]>())
		{
			MemoryPackFormatterProvider.Register<RankModelData[]>((MemoryPackFormatter<RankModelData[]>)(object)new ArrayFormatter<RankModelData>());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<List<RankRobotData>>())
		{
			MemoryPackFormatterProvider.Register<List<RankRobotData>>((MemoryPackFormatter<List<RankRobotData>>)(object)new ListFormatter<RankRobotData>());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<List<int>>())
		{
			MemoryPackFormatterProvider.Register<List<int>>((MemoryPackFormatter<List<int>>)(object)new ListFormatter<int>());
		}
	}

	[Preserve]
	public static void Serialize(ref MemoryPackWriter writer, ref RankModelData? value)
	{
		if (value == null)
		{
			((MemoryPackWriter)(ref writer)).WriteNullObjectHeader();
			return;
		}
		((MemoryPackWriter)(ref writer)).WriteUnmanagedWithObjectHeader<int, long, bool, long>((byte)36, ref value.Period, ref value.EndTimeStamp, ref value.IsClaimReward, ref value.NextOpenTimeStamp);
		((MemoryPackWriter)(ref writer)).WriteValue<List<RankRobotData>>(ref value.Robots);
		((MemoryPackWriter)(ref writer)).WriteValue<List<int>>(ref value.UsedRobotNameIdsThisActivity);
		((MemoryPackWriter)(ref writer)).WriteValue<List<int>>(ref value.UsedRobotNameIdCooldownLastActivity);
		((MemoryPackWriter)(ref writer)).WriteUnmanaged<int, int, int, int, int, int, bool, bool, int, int, int, int, long, int, int>(ref value.LastPeriodIndex, ref value.PlayerRankScore, ref value.MultiplierTier, ref value.LastMultiplierTier, ref value.CurrentSettleScore, ref value.CurrentSettleRanking, ref value.HasPlayerEnteredRank, ref value.HasShownActivityOpenPopup, ref value.ClearLevels, ref value.ExpGames, ref value.ActivityActiveGamesTotal, ref value.ActivityActiveDays, ref value.TimeRefresh, ref value.StarCountOneRound, ref value.CurrentCollectCount);
		ref int totalCollectCards = ref value.TotalCollectCards;
		ref bool hasReportedCampaignEnd = ref value.HasReportedCampaignEnd;
		ref bool isPeriodSettled = ref value.IsPeriodSettled;
		ref bool hasEnteredRewardPage = ref value.HasEnteredRewardPage;
		ref int winsAfterPeriodSettled = ref value.WinsAfterPeriodSettled;
		ref int openScoreAfterPeriodSettled = ref value.OpenScoreAfterPeriodSettled;
		ref long cooldownExitTimestamp = ref value.CooldownExitTimestamp;
		ref bool isHalloweenTheme = ref value.IsHalloweenTheme;
		ref int robotNameConfig = ref value.RobotNameConfig;
		ref int rankRewardConfig = ref value.RankRewardConfig;
		bool hasActivityRecord = value.HasActivityRecord;
		bool isActivityEnd = value.IsActivityEnd;
		bool isEndSettlementProcessed = value.IsEndSettlementProcessed;
		bool isPeriodSettlePending = value.IsPeriodSettlePending;
		((MemoryPackWriter)(ref writer)).WriteUnmanaged<int, bool, bool, bool, int, int, long, bool, int, int, bool, bool, bool, bool>(ref totalCollectCards, ref hasReportedCampaignEnd, ref isPeriodSettled, ref hasEnteredRewardPage, ref winsAfterPeriodSettled, ref openScoreAfterPeriodSettled, ref cooldownExitTimestamp, ref isHalloweenTheme, ref robotNameConfig, ref rankRewardConfig, ref hasActivityRecord, ref isActivityEnd, ref isEndSettlementProcessed, ref isPeriodSettlePending);
	}

	[Preserve]
	public static void Deserialize(ref MemoryPackReader reader, ref RankModelData? value)
	{
		byte b = default(byte);
		if (!((MemoryPackReader)(ref reader)).TryReadObjectHeader(ref b))
		{
			value = null;
			return;
		}
		int period = default(int);
		long endTimeStamp = default(long);
		bool isClaimReward = default(bool);
		long nextOpenTimeStamp = default(long);
		List<RankRobotData> robots;
		List<int> usedRobotNameIdsThisActivity;
		List<int> usedRobotNameIdCooldownLastActivity;
		int lastPeriodIndex = default(int);
		int playerRankScore = default(int);
		int multiplierTier = default(int);
		int lastMultiplierTier = default(int);
		int currentSettleScore = default(int);
		int currentSettleRanking = default(int);
		bool hasPlayerEnteredRank = default(bool);
		bool hasShownActivityOpenPopup = default(bool);
		int clearLevels = default(int);
		int expGames = default(int);
		int activityActiveGamesTotal = default(int);
		int activityActiveDays = default(int);
		long timeRefresh = default(long);
		int starCountOneRound = default(int);
		int currentCollectCount = default(int);
		int totalCollectCards = default(int);
		bool hasReportedCampaignEnd = default(bool);
		bool isPeriodSettled = default(bool);
		bool hasEnteredRewardPage = default(bool);
		int winsAfterPeriodSettled = default(int);
		int openScoreAfterPeriodSettled = default(int);
		long cooldownExitTimestamp = default(long);
		bool isHalloweenTheme = default(bool);
		int robotNameConfig = default(int);
		int rankRewardConfig = default(int);
		if (b == 36)
		{
			bool hasActivityRecord = default(bool);
			bool isActivityEnd = default(bool);
			bool isEndSettlementProcessed = default(bool);
			bool isPeriodSettlePending = default(bool);
			if (value != null)
			{
				period = value.Period;
				endTimeStamp = value.EndTimeStamp;
				isClaimReward = value.IsClaimReward;
				nextOpenTimeStamp = value.NextOpenTimeStamp;
				robots = value.Robots;
				usedRobotNameIdsThisActivity = value.UsedRobotNameIdsThisActivity;
				usedRobotNameIdCooldownLastActivity = value.UsedRobotNameIdCooldownLastActivity;
				lastPeriodIndex = value.LastPeriodIndex;
				playerRankScore = value.PlayerRankScore;
				multiplierTier = value.MultiplierTier;
				lastMultiplierTier = value.LastMultiplierTier;
				currentSettleScore = value.CurrentSettleScore;
				currentSettleRanking = value.CurrentSettleRanking;
				hasPlayerEnteredRank = value.HasPlayerEnteredRank;
				hasShownActivityOpenPopup = value.HasShownActivityOpenPopup;
				clearLevels = value.ClearLevels;
				expGames = value.ExpGames;
				activityActiveGamesTotal = value.ActivityActiveGamesTotal;
				activityActiveDays = value.ActivityActiveDays;
				timeRefresh = value.TimeRefresh;
				starCountOneRound = value.StarCountOneRound;
				currentCollectCount = value.CurrentCollectCount;
				totalCollectCards = value.TotalCollectCards;
				hasReportedCampaignEnd = value.HasReportedCampaignEnd;
				isPeriodSettled = value.IsPeriodSettled;
				hasEnteredRewardPage = value.HasEnteredRewardPage;
				winsAfterPeriodSettled = value.WinsAfterPeriodSettled;
				openScoreAfterPeriodSettled = value.OpenScoreAfterPeriodSettled;
				cooldownExitTimestamp = value.CooldownExitTimestamp;
				isHalloweenTheme = value.IsHalloweenTheme;
				robotNameConfig = value.RobotNameConfig;
				rankRewardConfig = value.RankRewardConfig;
				hasActivityRecord = value.HasActivityRecord;
				isActivityEnd = value.IsActivityEnd;
				isEndSettlementProcessed = value.IsEndSettlementProcessed;
				isPeriodSettlePending = value.IsPeriodSettlePending;
				((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref period);
				((MemoryPackReader)(ref reader)).ReadUnmanaged<long>(ref endTimeStamp);
				((MemoryPackReader)(ref reader)).ReadUnmanaged<bool>(ref isClaimReward);
				((MemoryPackReader)(ref reader)).ReadUnmanaged<long>(ref nextOpenTimeStamp);
				((MemoryPackReader)(ref reader)).ReadValue<List<RankRobotData>>(ref robots);
				((MemoryPackReader)(ref reader)).ReadValue<List<int>>(ref usedRobotNameIdsThisActivity);
				((MemoryPackReader)(ref reader)).ReadValue<List<int>>(ref usedRobotNameIdCooldownLastActivity);
				((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref lastPeriodIndex);
				((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref playerRankScore);
				((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref multiplierTier);
				((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref lastMultiplierTier);
				((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref currentSettleScore);
				((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref currentSettleRanking);
				((MemoryPackReader)(ref reader)).ReadUnmanaged<bool>(ref hasPlayerEnteredRank);
				((MemoryPackReader)(ref reader)).ReadUnmanaged<bool>(ref hasShownActivityOpenPopup);
				((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref clearLevels);
				((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref expGames);
				((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref activityActiveGamesTotal);
				((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref activityActiveDays);
				((MemoryPackReader)(ref reader)).ReadUnmanaged<long>(ref timeRefresh);
				((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref starCountOneRound);
				((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref currentCollectCount);
				((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref totalCollectCards);
				((MemoryPackReader)(ref reader)).ReadUnmanaged<bool>(ref hasReportedCampaignEnd);
				((MemoryPackReader)(ref reader)).ReadUnmanaged<bool>(ref isPeriodSettled);
				((MemoryPackReader)(ref reader)).ReadUnmanaged<bool>(ref hasEnteredRewardPage);
				((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref winsAfterPeriodSettled);
				((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref openScoreAfterPeriodSettled);
				((MemoryPackReader)(ref reader)).ReadUnmanaged<long>(ref cooldownExitTimestamp);
				((MemoryPackReader)(ref reader)).ReadUnmanaged<bool>(ref isHalloweenTheme);
				((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref robotNameConfig);
				((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref rankRewardConfig);
				((MemoryPackReader)(ref reader)).ReadUnmanaged<bool>(ref hasActivityRecord);
				((MemoryPackReader)(ref reader)).ReadUnmanaged<bool>(ref isActivityEnd);
				((MemoryPackReader)(ref reader)).ReadUnmanaged<bool>(ref isEndSettlementProcessed);
				((MemoryPackReader)(ref reader)).ReadUnmanaged<bool>(ref isPeriodSettlePending);
				goto IL_06e7;
			}
			((MemoryPackReader)(ref reader)).ReadUnmanaged<int, long, bool, long>(ref period, ref endTimeStamp, ref isClaimReward, ref nextOpenTimeStamp);
			robots = ((MemoryPackReader)(ref reader)).ReadValue<List<RankRobotData>>();
			usedRobotNameIdsThisActivity = ((MemoryPackReader)(ref reader)).ReadValue<List<int>>();
			usedRobotNameIdCooldownLastActivity = ((MemoryPackReader)(ref reader)).ReadValue<List<int>>();
			((MemoryPackReader)(ref reader)).ReadUnmanaged<int, int, int, int, int, int, bool, bool, int, int, int, int, long, int, int>(ref lastPeriodIndex, ref playerRankScore, ref multiplierTier, ref lastMultiplierTier, ref currentSettleScore, ref currentSettleRanking, ref hasPlayerEnteredRank, ref hasShownActivityOpenPopup, ref clearLevels, ref expGames, ref activityActiveGamesTotal, ref activityActiveDays, ref timeRefresh, ref starCountOneRound, ref currentCollectCount);
			((MemoryPackReader)(ref reader)).ReadUnmanaged<int, bool, bool, bool, int, int, long, bool, int, int, bool, bool, bool, bool>(ref totalCollectCards, ref hasReportedCampaignEnd, ref isPeriodSettled, ref hasEnteredRewardPage, ref winsAfterPeriodSettled, ref openScoreAfterPeriodSettled, ref cooldownExitTimestamp, ref isHalloweenTheme, ref robotNameConfig, ref rankRewardConfig, ref hasActivityRecord, ref isActivityEnd, ref isEndSettlementProcessed, ref isPeriodSettlePending);
		}
		else
		{
			if (b > 36)
			{
				MemoryPackSerializationException.ThrowInvalidPropertyCount(typeof(RankModelData), (byte)36, b);
				return;
			}
			bool hasActivityRecord;
			bool isActivityEnd;
			bool isEndSettlementProcessed;
			bool isPeriodSettlePending;
			if (value == null)
			{
				period = 0;
				endTimeStamp = 0L;
				isClaimReward = false;
				nextOpenTimeStamp = 0L;
				robots = null;
				usedRobotNameIdsThisActivity = null;
				usedRobotNameIdCooldownLastActivity = null;
				lastPeriodIndex = 0;
				playerRankScore = 0;
				multiplierTier = 0;
				lastMultiplierTier = 0;
				currentSettleScore = 0;
				currentSettleRanking = 0;
				hasPlayerEnteredRank = false;
				hasShownActivityOpenPopup = false;
				clearLevels = 0;
				expGames = 0;
				activityActiveGamesTotal = 0;
				activityActiveDays = 0;
				timeRefresh = 0L;
				starCountOneRound = 0;
				currentCollectCount = 0;
				totalCollectCards = 0;
				hasReportedCampaignEnd = false;
				isPeriodSettled = false;
				hasEnteredRewardPage = false;
				winsAfterPeriodSettled = 0;
				openScoreAfterPeriodSettled = 0;
				cooldownExitTimestamp = 0L;
				isHalloweenTheme = false;
				robotNameConfig = 0;
				rankRewardConfig = 0;
				hasActivityRecord = false;
				isActivityEnd = false;
				isEndSettlementProcessed = false;
				isPeriodSettlePending = false;
			}
			else
			{
				period = value.Period;
				endTimeStamp = value.EndTimeStamp;
				isClaimReward = value.IsClaimReward;
				nextOpenTimeStamp = value.NextOpenTimeStamp;
				robots = value.Robots;
				usedRobotNameIdsThisActivity = value.UsedRobotNameIdsThisActivity;
				usedRobotNameIdCooldownLastActivity = value.UsedRobotNameIdCooldownLastActivity;
				lastPeriodIndex = value.LastPeriodIndex;
				playerRankScore = value.PlayerRankScore;
				multiplierTier = value.MultiplierTier;
				lastMultiplierTier = value.LastMultiplierTier;
				currentSettleScore = value.CurrentSettleScore;
				currentSettleRanking = value.CurrentSettleRanking;
				hasPlayerEnteredRank = value.HasPlayerEnteredRank;
				hasShownActivityOpenPopup = value.HasShownActivityOpenPopup;
				clearLevels = value.ClearLevels;
				expGames = value.ExpGames;
				activityActiveGamesTotal = value.ActivityActiveGamesTotal;
				activityActiveDays = value.ActivityActiveDays;
				timeRefresh = value.TimeRefresh;
				starCountOneRound = value.StarCountOneRound;
				currentCollectCount = value.CurrentCollectCount;
				totalCollectCards = value.TotalCollectCards;
				hasReportedCampaignEnd = value.HasReportedCampaignEnd;
				isPeriodSettled = value.IsPeriodSettled;
				hasEnteredRewardPage = value.HasEnteredRewardPage;
				winsAfterPeriodSettled = value.WinsAfterPeriodSettled;
				openScoreAfterPeriodSettled = value.OpenScoreAfterPeriodSettled;
				cooldownExitTimestamp = value.CooldownExitTimestamp;
				isHalloweenTheme = value.IsHalloweenTheme;
				robotNameConfig = value.RobotNameConfig;
				rankRewardConfig = value.RankRewardConfig;
				hasActivityRecord = value.HasActivityRecord;
				isActivityEnd = value.IsActivityEnd;
				isEndSettlementProcessed = value.IsEndSettlementProcessed;
				isPeriodSettlePending = value.IsPeriodSettlePending;
			}
			if (b != 0)
			{
				((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref period);
				if (b != 1)
				{
					((MemoryPackReader)(ref reader)).ReadUnmanaged<long>(ref endTimeStamp);
					if (b != 2)
					{
						((MemoryPackReader)(ref reader)).ReadUnmanaged<bool>(ref isClaimReward);
						if (b != 3)
						{
							((MemoryPackReader)(ref reader)).ReadUnmanaged<long>(ref nextOpenTimeStamp);
							if (b != 4)
							{
								((MemoryPackReader)(ref reader)).ReadValue<List<RankRobotData>>(ref robots);
								if (b != 5)
								{
									((MemoryPackReader)(ref reader)).ReadValue<List<int>>(ref usedRobotNameIdsThisActivity);
									if (b != 6)
									{
										((MemoryPackReader)(ref reader)).ReadValue<List<int>>(ref usedRobotNameIdCooldownLastActivity);
										if (b != 7)
										{
											((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref lastPeriodIndex);
											if (b != 8)
											{
												((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref playerRankScore);
												if (b != 9)
												{
													((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref multiplierTier);
													if (b != 10)
													{
														((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref lastMultiplierTier);
														if (b != 11)
														{
															((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref currentSettleScore);
															if (b != 12)
															{
																((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref currentSettleRanking);
																if (b != 13)
																{
																	((MemoryPackReader)(ref reader)).ReadUnmanaged<bool>(ref hasPlayerEnteredRank);
																	if (b != 14)
																	{
																		((MemoryPackReader)(ref reader)).ReadUnmanaged<bool>(ref hasShownActivityOpenPopup);
																		if (b != 15)
																		{
																			((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref clearLevels);
																			if (b != 16)
																			{
																				((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref expGames);
																				if (b != 17)
																				{
																					((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref activityActiveGamesTotal);
																					if (b != 18)
																					{
																						((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref activityActiveDays);
																						if (b != 19)
																						{
																							((MemoryPackReader)(ref reader)).ReadUnmanaged<long>(ref timeRefresh);
																							if (b != 20)
																							{
																								((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref starCountOneRound);
																								if (b != 21)
																								{
																									((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref currentCollectCount);
																									if (b != 22)
																									{
																										((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref totalCollectCards);
																										if (b != 23)
																										{
																											((MemoryPackReader)(ref reader)).ReadUnmanaged<bool>(ref hasReportedCampaignEnd);
																											if (b != 24)
																											{
																												((MemoryPackReader)(ref reader)).ReadUnmanaged<bool>(ref isPeriodSettled);
																												if (b != 25)
																												{
																													((MemoryPackReader)(ref reader)).ReadUnmanaged<bool>(ref hasEnteredRewardPage);
																													if (b != 26)
																													{
																														((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref winsAfterPeriodSettled);
																														if (b != 27)
																														{
																															((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref openScoreAfterPeriodSettled);
																															if (b != 28)
																															{
																																((MemoryPackReader)(ref reader)).ReadUnmanaged<long>(ref cooldownExitTimestamp);
																																if (b != 29)
																																{
																																	((MemoryPackReader)(ref reader)).ReadUnmanaged<bool>(ref isHalloweenTheme);
																																	if (b != 30)
																																	{
																																		((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref robotNameConfig);
																																		if (b != 31)
																																		{
																																			((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref rankRewardConfig);
																																			if (b != 32)
																																			{
																																				((MemoryPackReader)(ref reader)).ReadUnmanaged<bool>(ref hasActivityRecord);
																																				if (b != 33)
																																				{
																																					((MemoryPackReader)(ref reader)).ReadUnmanaged<bool>(ref isActivityEnd);
																																					if (b != 34)
																																					{
																																						((MemoryPackReader)(ref reader)).ReadUnmanaged<bool>(ref isEndSettlementProcessed);
																																						if (b != 35)
																																						{
																																							((MemoryPackReader)(ref reader)).ReadUnmanaged<bool>(ref isPeriodSettlePending);
																																							_ = 36;
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
				goto IL_06e7;
			}
		}
		value = new RankModelData
		{
			Period = period,
			EndTimeStamp = endTimeStamp,
			IsClaimReward = isClaimReward,
			NextOpenTimeStamp = nextOpenTimeStamp,
			Robots = robots,
			UsedRobotNameIdsThisActivity = usedRobotNameIdsThisActivity,
			UsedRobotNameIdCooldownLastActivity = usedRobotNameIdCooldownLastActivity,
			LastPeriodIndex = lastPeriodIndex,
			PlayerRankScore = playerRankScore,
			MultiplierTier = multiplierTier,
			LastMultiplierTier = lastMultiplierTier,
			CurrentSettleScore = currentSettleScore,
			CurrentSettleRanking = currentSettleRanking,
			HasPlayerEnteredRank = hasPlayerEnteredRank,
			HasShownActivityOpenPopup = hasShownActivityOpenPopup,
			ClearLevels = clearLevels,
			ExpGames = expGames,
			ActivityActiveGamesTotal = activityActiveGamesTotal,
			ActivityActiveDays = activityActiveDays,
			TimeRefresh = timeRefresh,
			StarCountOneRound = starCountOneRound,
			CurrentCollectCount = currentCollectCount,
			TotalCollectCards = totalCollectCards,
			HasReportedCampaignEnd = hasReportedCampaignEnd,
			IsPeriodSettled = isPeriodSettled,
			HasEnteredRewardPage = hasEnteredRewardPage,
			WinsAfterPeriodSettled = winsAfterPeriodSettled,
			OpenScoreAfterPeriodSettled = openScoreAfterPeriodSettled,
			CooldownExitTimestamp = cooldownExitTimestamp,
			IsHalloweenTheme = isHalloweenTheme,
			RobotNameConfig = robotNameConfig,
			RankRewardConfig = rankRewardConfig
		};
		return;
		IL_06e7:
		value.Period = period;
		value.EndTimeStamp = endTimeStamp;
		value.IsClaimReward = isClaimReward;
		value.NextOpenTimeStamp = nextOpenTimeStamp;
		value.Robots = robots;
		value.UsedRobotNameIdsThisActivity = usedRobotNameIdsThisActivity;
		value.UsedRobotNameIdCooldownLastActivity = usedRobotNameIdCooldownLastActivity;
		value.LastPeriodIndex = lastPeriodIndex;
		value.PlayerRankScore = playerRankScore;
		value.MultiplierTier = multiplierTier;
		value.LastMultiplierTier = lastMultiplierTier;
		value.CurrentSettleScore = currentSettleScore;
		value.CurrentSettleRanking = currentSettleRanking;
		value.HasPlayerEnteredRank = hasPlayerEnteredRank;
		value.HasShownActivityOpenPopup = hasShownActivityOpenPopup;
		value.ClearLevels = clearLevels;
		value.ExpGames = expGames;
		value.ActivityActiveGamesTotal = activityActiveGamesTotal;
		value.ActivityActiveDays = activityActiveDays;
		value.TimeRefresh = timeRefresh;
		value.StarCountOneRound = starCountOneRound;
		value.CurrentCollectCount = currentCollectCount;
		value.TotalCollectCards = totalCollectCards;
		value.HasReportedCampaignEnd = hasReportedCampaignEnd;
		value.IsPeriodSettled = isPeriodSettled;
		value.HasEnteredRewardPage = hasEnteredRewardPage;
		value.WinsAfterPeriodSettled = winsAfterPeriodSettled;
		value.OpenScoreAfterPeriodSettled = openScoreAfterPeriodSettled;
		value.CooldownExitTimestamp = cooldownExitTimestamp;
		value.IsHalloweenTheme = isHalloweenTheme;
		value.RobotNameConfig = robotNameConfig;
		value.RankRewardConfig = rankRewardConfig;
	}
}
