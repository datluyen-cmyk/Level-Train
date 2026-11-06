using System.Collections.Generic;
using MemoryPack;
using MemoryPack.Formatters;
using MemoryPack.Internal;
using RotateRings.Prop;

namespace RotateRings.Model;

[MemoryPackable(/*Could not decode attribute arguments.*/)]
public class GameModeData : IMemoryPackable<GameModeData>, IMemoryPackFormatterRegister
{
	[Preserve]
	private sealed class GameModeDataFormatter : MemoryPackFormatter<GameModeData>
	{
		[Preserve]
		public override void Serialize(ref MemoryPackWriter writer, ref GameModeData value)
		{
			GameModeData.Serialize(ref writer, ref value);
		}

		[Preserve]
		public override void Deserialize(ref MemoryPackReader reader, ref GameModeData value)
		{
			GameModeData.Deserialize(ref reader, ref value);
		}
	}

	public int level = 1;

	public int TryLevelNum;

	public int LevelSlotId;

	public int game_mode;

	public int GlobalLevelCount;

	public int EndlessSeason;

	public int game_score;

	public int LevelState;

	public int AllStepNum;

	public int steps;

	public int game_time;

	public int game_time_real;

	public int DeadNum;

	public GameplayState GameplayState;

	public int mode_num;

	public int mode_winnum;

	public Dictionary<EPropType, Dictionary<int, int>> PropUseDat;

	public Dictionary<EPropType, Dictionary<int, int>> PropGainDat;

	public int RewardCoinNum;

	public int UseCoinNum;

	public int WinningStreak;

	public int TrackSettledGameNum;

	public int TrackSettledWinNum;

	public List<bool> TrackRecentSettledWins;

	public List<int> TrackRecentSettledScores;

	public int TrackModeMaxScore;

	public string TrackWinStreakDayKey;

	public int TrackModeWinStreakDay;

	public int TrackModeWinStreak;

	public int CurrentLevelLose;

	public bool LosingStreakCounted;

	public int CurrentLevelLoseForLevel = -1;

	public bool IsFirstEnterLevel;

	public int TotalLevelLose;

	public bool LastLevelLoseFlag;

	public int CurrentReleaseCount;

	public int CumulativeReleaseTriggeredCount;

	public int RandomPickLevel;

	public int RandomPickCoreLevelPath;

	public int RandomPickParts;

	public int RandomPickBonus;

	public int RandomPickSpecified;

	public string RandomPickCsv;

	public int RandomPickTeachElement;

	public int StepLimitInitialSteps;

	public int StepLimitUsedSteps;

	public int[] StepLimitAdAddSteps;

	public void ResetForNewSeason()
	{
		level = 1;
		TryLevelNum = 0;
		LevelSlotId = 0;
		GlobalLevelCount = 0;
		mode_num = 0;
		mode_winnum = 0;
		WinningStreak = 0;
		TrackSettledGameNum = 0;
		TrackSettledWinNum = 0;
		TrackRecentSettledWins?.Clear();
		TrackRecentSettledScores?.Clear();
		TrackModeMaxScore = 0;
		TrackWinStreakDayKey = null;
		TrackModeWinStreakDay = 0;
		TrackModeWinStreak = 0;
		CurrentLevelLose = 0;
		LosingStreakCounted = false;
		CurrentLevelLoseForLevel = -1;
		IsFirstEnterLevel = false;
		TotalLevelLose = 0;
		LastLevelLoseFlag = false;
		CurrentReleaseCount = 0;
		CumulativeReleaseTriggeredCount = 0;
		StepLimitInitialSteps = 0;
		StepLimitUsedSteps = 0;
		StepLimitAdAddSteps = null;
		ClearData();
	}

	public void ClearData()
	{
		PropUseDat?.Clear();
		PropGainDat?.Clear();
		game_score = 0;
		CurrentReleaseCount = 0;
		CumulativeReleaseTriggeredCount = 0;
		LevelState = 0;
		AllStepNum = 0;
		steps = 0;
		game_time = 0;
		game_time_real = 0;
		DeadNum = 0;
		GameplayState = GameplayState.Initialized;
		RewardCoinNum = (UseCoinNum = 0);
		StepLimitInitialSteps = 0;
		StepLimitUsedSteps = 0;
		StepLimitAdAddSteps = null;
	}

	static GameModeData()
	{
		RegisterFormatter();
	}

	[Preserve]
	public static void RegisterFormatter()
	{
		if (!MemoryPackFormatterProvider.IsRegistered<GameModeData>())
		{
			MemoryPackFormatterProvider.Register<GameModeData>((MemoryPackFormatter<GameModeData>)(object)new GameModeDataFormatter());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<GameModeData[]>())
		{
			MemoryPackFormatterProvider.Register<GameModeData[]>((MemoryPackFormatter<GameModeData[]>)(object)new ArrayFormatter<GameModeData>());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<GameplayState>())
		{
			MemoryPackFormatterProvider.Register<GameplayState>((MemoryPackFormatter<GameplayState>)(object)new UnmanagedFormatter<GameplayState>());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<Dictionary<EPropType, Dictionary<int, int>>>())
		{
			MemoryPackFormatterProvider.Register<Dictionary<EPropType, Dictionary<int, int>>>((MemoryPackFormatter<Dictionary<EPropType, Dictionary<int, int>>>)(object)new DictionaryFormatter<EPropType, Dictionary<int, int>>());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<EPropType>())
		{
			MemoryPackFormatterProvider.Register<EPropType>((MemoryPackFormatter<EPropType>)(object)new UnmanagedFormatter<EPropType>());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<Dictionary<int, int>>())
		{
			MemoryPackFormatterProvider.Register<Dictionary<int, int>>((MemoryPackFormatter<Dictionary<int, int>>)(object)new DictionaryFormatter<int, int>());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<List<bool>>())
		{
			MemoryPackFormatterProvider.Register<List<bool>>((MemoryPackFormatter<List<bool>>)(object)new ListFormatter<bool>());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<List<int>>())
		{
			MemoryPackFormatterProvider.Register<List<int>>((MemoryPackFormatter<List<int>>)(object)new ListFormatter<int>());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<int[]>())
		{
			MemoryPackFormatterProvider.Register<int[]>((MemoryPackFormatter<int[]>)(object)new ArrayFormatter<int>());
		}
	}

	[Preserve]
	public static void Serialize(ref MemoryPackWriter writer, ref GameModeData? value)
	{
		if (value == null)
		{
			((MemoryPackWriter)(ref writer)).WriteNullObjectHeader();
			return;
		}
		((MemoryPackWriter)(ref writer)).WriteUnmanagedWithObjectHeader<int, int, int, int, int, int, int, int, int, int, int, int, int, GameplayState, int>((byte)47, ref value.level, ref value.TryLevelNum, ref value.LevelSlotId, ref value.game_mode, ref value.GlobalLevelCount, ref value.EndlessSeason, ref value.game_score, ref value.LevelState, ref value.AllStepNum, ref value.steps, ref value.game_time, ref value.game_time_real, ref value.DeadNum, ref value.GameplayState, ref value.mode_num);
		((MemoryPackWriter)(ref writer)).WriteUnmanaged<int>(ref value.mode_winnum);
		((MemoryPackWriter)(ref writer)).WriteValue<Dictionary<EPropType, Dictionary<int, int>>>(ref value.PropUseDat);
		((MemoryPackWriter)(ref writer)).WriteValue<Dictionary<EPropType, Dictionary<int, int>>>(ref value.PropGainDat);
		((MemoryPackWriter)(ref writer)).WriteUnmanaged<int, int, int, int, int>(ref value.RewardCoinNum, ref value.UseCoinNum, ref value.WinningStreak, ref value.TrackSettledGameNum, ref value.TrackSettledWinNum);
		((MemoryPackWriter)(ref writer)).WriteValue<List<bool>>(ref value.TrackRecentSettledWins);
		((MemoryPackWriter)(ref writer)).WriteValue<List<int>>(ref value.TrackRecentSettledScores);
		((MemoryPackWriter)(ref writer)).WriteUnmanaged<int>(ref value.TrackModeMaxScore);
		((MemoryPackWriter)(ref writer)).WriteString(value.TrackWinStreakDayKey);
		((MemoryPackWriter)(ref writer)).WriteUnmanaged<int, int, int, bool, int, bool, int, bool, int, int, int, int, int, int, int>(ref value.TrackModeWinStreakDay, ref value.TrackModeWinStreak, ref value.CurrentLevelLose, ref value.LosingStreakCounted, ref value.CurrentLevelLoseForLevel, ref value.IsFirstEnterLevel, ref value.TotalLevelLose, ref value.LastLevelLoseFlag, ref value.CurrentReleaseCount, ref value.CumulativeReleaseTriggeredCount, ref value.RandomPickLevel, ref value.RandomPickCoreLevelPath, ref value.RandomPickParts, ref value.RandomPickBonus, ref value.RandomPickSpecified);
		((MemoryPackWriter)(ref writer)).WriteString(value.RandomPickCsv);
		((MemoryPackWriter)(ref writer)).WriteUnmanaged<int, int, int>(ref value.RandomPickTeachElement, ref value.StepLimitInitialSteps, ref value.StepLimitUsedSteps);
		((MemoryPackWriter)(ref writer)).WriteUnmanagedArray<int>(value.StepLimitAdAddSteps);
	}

	[Preserve]
	public static void Deserialize(ref MemoryPackReader reader, ref GameModeData? value)
	{
		byte b = default(byte);
		if (!((MemoryPackReader)(ref reader)).TryReadObjectHeader(ref b))
		{
			value = null;
			return;
		}
		int num = default(int);
		int tryLevelNum = default(int);
		int levelSlotId = default(int);
		int num2 = default(int);
		int globalLevelCount = default(int);
		int endlessSeason = default(int);
		int num3 = default(int);
		int levelState = default(int);
		int allStepNum = default(int);
		int num4 = default(int);
		int num5 = default(int);
		int num6 = default(int);
		int deadNum = default(int);
		GameplayState gameplayState = default(GameplayState);
		int num7 = default(int);
		int num8 = default(int);
		Dictionary<EPropType, Dictionary<int, int>> propUseDat;
		Dictionary<EPropType, Dictionary<int, int>> propGainDat;
		int rewardCoinNum = default(int);
		int useCoinNum = default(int);
		int winningStreak = default(int);
		int trackSettledGameNum = default(int);
		int trackSettledWinNum = default(int);
		List<bool> trackRecentSettledWins;
		List<int> trackRecentSettledScores;
		int trackModeMaxScore = default(int);
		int trackModeWinStreakDay = default(int);
		int trackModeWinStreak = default(int);
		int currentLevelLose = default(int);
		bool losingStreakCounted = default(bool);
		int currentLevelLoseForLevel = default(int);
		bool isFirstEnterLevel = default(bool);
		int totalLevelLose = default(int);
		bool lastLevelLoseFlag = default(bool);
		int currentReleaseCount = default(int);
		int cumulativeReleaseTriggeredCount = default(int);
		int randomPickLevel = default(int);
		int randomPickCoreLevelPath = default(int);
		int randomPickParts = default(int);
		int randomPickBonus = default(int);
		int randomPickSpecified = default(int);
		int randomPickTeachElement = default(int);
		int stepLimitInitialSteps = default(int);
		int stepLimitUsedSteps = default(int);
		int[] stepLimitAdAddSteps;
		string trackWinStreakDayKey;
		string randomPickCsv;
		if (b == 47)
		{
			if (value != null)
			{
				num = value.level;
				tryLevelNum = value.TryLevelNum;
				levelSlotId = value.LevelSlotId;
				num2 = value.game_mode;
				globalLevelCount = value.GlobalLevelCount;
				endlessSeason = value.EndlessSeason;
				num3 = value.game_score;
				levelState = value.LevelState;
				allStepNum = value.AllStepNum;
				num4 = value.steps;
				num5 = value.game_time;
				num6 = value.game_time_real;
				deadNum = value.DeadNum;
				gameplayState = value.GameplayState;
				num7 = value.mode_num;
				num8 = value.mode_winnum;
				propUseDat = value.PropUseDat;
				propGainDat = value.PropGainDat;
				rewardCoinNum = value.RewardCoinNum;
				useCoinNum = value.UseCoinNum;
				winningStreak = value.WinningStreak;
				trackSettledGameNum = value.TrackSettledGameNum;
				trackSettledWinNum = value.TrackSettledWinNum;
				trackRecentSettledWins = value.TrackRecentSettledWins;
				trackRecentSettledScores = value.TrackRecentSettledScores;
				trackModeMaxScore = value.TrackModeMaxScore;
				trackWinStreakDayKey = value.TrackWinStreakDayKey;
				trackModeWinStreakDay = value.TrackModeWinStreakDay;
				trackModeWinStreak = value.TrackModeWinStreak;
				currentLevelLose = value.CurrentLevelLose;
				losingStreakCounted = value.LosingStreakCounted;
				currentLevelLoseForLevel = value.CurrentLevelLoseForLevel;
				isFirstEnterLevel = value.IsFirstEnterLevel;
				totalLevelLose = value.TotalLevelLose;
				lastLevelLoseFlag = value.LastLevelLoseFlag;
				currentReleaseCount = value.CurrentReleaseCount;
				cumulativeReleaseTriggeredCount = value.CumulativeReleaseTriggeredCount;
				randomPickLevel = value.RandomPickLevel;
				randomPickCoreLevelPath = value.RandomPickCoreLevelPath;
				randomPickParts = value.RandomPickParts;
				randomPickBonus = value.RandomPickBonus;
				randomPickSpecified = value.RandomPickSpecified;
				randomPickCsv = value.RandomPickCsv;
				randomPickTeachElement = value.RandomPickTeachElement;
				stepLimitInitialSteps = value.StepLimitInitialSteps;
				stepLimitUsedSteps = value.StepLimitUsedSteps;
				stepLimitAdAddSteps = value.StepLimitAdAddSteps;
				((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref num);
				((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref tryLevelNum);
				((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref levelSlotId);
				((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref num2);
				((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref globalLevelCount);
				((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref endlessSeason);
				((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref num3);
				((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref levelState);
				((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref allStepNum);
				((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref num4);
				((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref num5);
				((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref num6);
				((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref deadNum);
				((MemoryPackReader)(ref reader)).ReadUnmanaged<GameplayState>(ref gameplayState);
				((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref num7);
				((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref num8);
				((MemoryPackReader)(ref reader)).ReadValue<Dictionary<EPropType, Dictionary<int, int>>>(ref propUseDat);
				((MemoryPackReader)(ref reader)).ReadValue<Dictionary<EPropType, Dictionary<int, int>>>(ref propGainDat);
				((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref rewardCoinNum);
				((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref useCoinNum);
				((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref winningStreak);
				((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref trackSettledGameNum);
				((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref trackSettledWinNum);
				((MemoryPackReader)(ref reader)).ReadValue<List<bool>>(ref trackRecentSettledWins);
				((MemoryPackReader)(ref reader)).ReadValue<List<int>>(ref trackRecentSettledScores);
				((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref trackModeMaxScore);
				trackWinStreakDayKey = ((MemoryPackReader)(ref reader)).ReadString();
				((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref trackModeWinStreakDay);
				((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref trackModeWinStreak);
				((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref currentLevelLose);
				((MemoryPackReader)(ref reader)).ReadUnmanaged<bool>(ref losingStreakCounted);
				((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref currentLevelLoseForLevel);
				((MemoryPackReader)(ref reader)).ReadUnmanaged<bool>(ref isFirstEnterLevel);
				((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref totalLevelLose);
				((MemoryPackReader)(ref reader)).ReadUnmanaged<bool>(ref lastLevelLoseFlag);
				((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref currentReleaseCount);
				((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref cumulativeReleaseTriggeredCount);
				((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref randomPickLevel);
				((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref randomPickCoreLevelPath);
				((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref randomPickParts);
				((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref randomPickBonus);
				((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref randomPickSpecified);
				randomPickCsv = ((MemoryPackReader)(ref reader)).ReadString();
				((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref randomPickTeachElement);
				((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref stepLimitInitialSteps);
				((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref stepLimitUsedSteps);
				((MemoryPackReader)(ref reader)).ReadUnmanagedArray<int>(ref stepLimitAdAddSteps);
				goto IL_0918;
			}
			((MemoryPackReader)(ref reader)).ReadUnmanaged<int, int, int, int, int, int, int, int, int, int, int, int, int, GameplayState, int>(ref num, ref tryLevelNum, ref levelSlotId, ref num2, ref globalLevelCount, ref endlessSeason, ref num3, ref levelState, ref allStepNum, ref num4, ref num5, ref num6, ref deadNum, ref gameplayState, ref num7);
			((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref num8);
			propUseDat = ((MemoryPackReader)(ref reader)).ReadValue<Dictionary<EPropType, Dictionary<int, int>>>();
			propGainDat = ((MemoryPackReader)(ref reader)).ReadValue<Dictionary<EPropType, Dictionary<int, int>>>();
			((MemoryPackReader)(ref reader)).ReadUnmanaged<int, int, int, int, int>(ref rewardCoinNum, ref useCoinNum, ref winningStreak, ref trackSettledGameNum, ref trackSettledWinNum);
			trackRecentSettledWins = ((MemoryPackReader)(ref reader)).ReadValue<List<bool>>();
			trackRecentSettledScores = ((MemoryPackReader)(ref reader)).ReadValue<List<int>>();
			((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref trackModeMaxScore);
			trackWinStreakDayKey = ((MemoryPackReader)(ref reader)).ReadString();
			((MemoryPackReader)(ref reader)).ReadUnmanaged<int, int, int, bool, int, bool, int, bool, int, int, int, int, int, int, int>(ref trackModeWinStreakDay, ref trackModeWinStreak, ref currentLevelLose, ref losingStreakCounted, ref currentLevelLoseForLevel, ref isFirstEnterLevel, ref totalLevelLose, ref lastLevelLoseFlag, ref currentReleaseCount, ref cumulativeReleaseTriggeredCount, ref randomPickLevel, ref randomPickCoreLevelPath, ref randomPickParts, ref randomPickBonus, ref randomPickSpecified);
			randomPickCsv = ((MemoryPackReader)(ref reader)).ReadString();
			((MemoryPackReader)(ref reader)).ReadUnmanaged<int, int, int>(ref randomPickTeachElement, ref stepLimitInitialSteps, ref stepLimitUsedSteps);
			stepLimitAdAddSteps = ((MemoryPackReader)(ref reader)).ReadUnmanagedArray<int>();
		}
		else
		{
			if (b > 47)
			{
				MemoryPackSerializationException.ThrowInvalidPropertyCount(typeof(GameModeData), (byte)47, b);
				return;
			}
			if (value == null)
			{
				num = 0;
				tryLevelNum = 0;
				levelSlotId = 0;
				num2 = 0;
				globalLevelCount = 0;
				endlessSeason = 0;
				num3 = 0;
				levelState = 0;
				allStepNum = 0;
				num4 = 0;
				num5 = 0;
				num6 = 0;
				deadNum = 0;
				gameplayState = GameplayState.Initialized;
				num7 = 0;
				num8 = 0;
				propUseDat = null;
				propGainDat = null;
				rewardCoinNum = 0;
				useCoinNum = 0;
				winningStreak = 0;
				trackSettledGameNum = 0;
				trackSettledWinNum = 0;
				trackRecentSettledWins = null;
				trackRecentSettledScores = null;
				trackModeMaxScore = 0;
				trackWinStreakDayKey = null;
				trackModeWinStreakDay = 0;
				trackModeWinStreak = 0;
				currentLevelLose = 0;
				losingStreakCounted = false;
				currentLevelLoseForLevel = 0;
				isFirstEnterLevel = false;
				totalLevelLose = 0;
				lastLevelLoseFlag = false;
				currentReleaseCount = 0;
				cumulativeReleaseTriggeredCount = 0;
				randomPickLevel = 0;
				randomPickCoreLevelPath = 0;
				randomPickParts = 0;
				randomPickBonus = 0;
				randomPickSpecified = 0;
				randomPickCsv = null;
				randomPickTeachElement = 0;
				stepLimitInitialSteps = 0;
				stepLimitUsedSteps = 0;
				stepLimitAdAddSteps = null;
			}
			else
			{
				num = value.level;
				tryLevelNum = value.TryLevelNum;
				levelSlotId = value.LevelSlotId;
				num2 = value.game_mode;
				globalLevelCount = value.GlobalLevelCount;
				endlessSeason = value.EndlessSeason;
				num3 = value.game_score;
				levelState = value.LevelState;
				allStepNum = value.AllStepNum;
				num4 = value.steps;
				num5 = value.game_time;
				num6 = value.game_time_real;
				deadNum = value.DeadNum;
				gameplayState = value.GameplayState;
				num7 = value.mode_num;
				num8 = value.mode_winnum;
				propUseDat = value.PropUseDat;
				propGainDat = value.PropGainDat;
				rewardCoinNum = value.RewardCoinNum;
				useCoinNum = value.UseCoinNum;
				winningStreak = value.WinningStreak;
				trackSettledGameNum = value.TrackSettledGameNum;
				trackSettledWinNum = value.TrackSettledWinNum;
				trackRecentSettledWins = value.TrackRecentSettledWins;
				trackRecentSettledScores = value.TrackRecentSettledScores;
				trackModeMaxScore = value.TrackModeMaxScore;
				trackWinStreakDayKey = value.TrackWinStreakDayKey;
				trackModeWinStreakDay = value.TrackModeWinStreakDay;
				trackModeWinStreak = value.TrackModeWinStreak;
				currentLevelLose = value.CurrentLevelLose;
				losingStreakCounted = value.LosingStreakCounted;
				currentLevelLoseForLevel = value.CurrentLevelLoseForLevel;
				isFirstEnterLevel = value.IsFirstEnterLevel;
				totalLevelLose = value.TotalLevelLose;
				lastLevelLoseFlag = value.LastLevelLoseFlag;
				currentReleaseCount = value.CurrentReleaseCount;
				cumulativeReleaseTriggeredCount = value.CumulativeReleaseTriggeredCount;
				randomPickLevel = value.RandomPickLevel;
				randomPickCoreLevelPath = value.RandomPickCoreLevelPath;
				randomPickParts = value.RandomPickParts;
				randomPickBonus = value.RandomPickBonus;
				randomPickSpecified = value.RandomPickSpecified;
				randomPickCsv = value.RandomPickCsv;
				randomPickTeachElement = value.RandomPickTeachElement;
				stepLimitInitialSteps = value.StepLimitInitialSteps;
				stepLimitUsedSteps = value.StepLimitUsedSteps;
				stepLimitAdAddSteps = value.StepLimitAdAddSteps;
			}
			if (b != 0)
			{
				((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref num);
				if (b != 1)
				{
					((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref tryLevelNum);
					if (b != 2)
					{
						((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref levelSlotId);
						if (b != 3)
						{
							((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref num2);
							if (b != 4)
							{
								((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref globalLevelCount);
								if (b != 5)
								{
									((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref endlessSeason);
									if (b != 6)
									{
										((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref num3);
										if (b != 7)
										{
											((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref levelState);
											if (b != 8)
											{
												((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref allStepNum);
												if (b != 9)
												{
													((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref num4);
													if (b != 10)
													{
														((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref num5);
														if (b != 11)
														{
															((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref num6);
															if (b != 12)
															{
																((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref deadNum);
																if (b != 13)
																{
																	((MemoryPackReader)(ref reader)).ReadUnmanaged<GameplayState>(ref gameplayState);
																	if (b != 14)
																	{
																		((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref num7);
																		if (b != 15)
																		{
																			((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref num8);
																			if (b != 16)
																			{
																				((MemoryPackReader)(ref reader)).ReadValue<Dictionary<EPropType, Dictionary<int, int>>>(ref propUseDat);
																				if (b != 17)
																				{
																					((MemoryPackReader)(ref reader)).ReadValue<Dictionary<EPropType, Dictionary<int, int>>>(ref propGainDat);
																					if (b != 18)
																					{
																						((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref rewardCoinNum);
																						if (b != 19)
																						{
																							((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref useCoinNum);
																							if (b != 20)
																							{
																								((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref winningStreak);
																								if (b != 21)
																								{
																									((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref trackSettledGameNum);
																									if (b != 22)
																									{
																										((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref trackSettledWinNum);
																										if (b != 23)
																										{
																											((MemoryPackReader)(ref reader)).ReadValue<List<bool>>(ref trackRecentSettledWins);
																											if (b != 24)
																											{
																												((MemoryPackReader)(ref reader)).ReadValue<List<int>>(ref trackRecentSettledScores);
																												if (b != 25)
																												{
																													((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref trackModeMaxScore);
																													if (b != 26)
																													{
																														trackWinStreakDayKey = ((MemoryPackReader)(ref reader)).ReadString();
																														if (b != 27)
																														{
																															((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref trackModeWinStreakDay);
																															if (b != 28)
																															{
																																((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref trackModeWinStreak);
																																if (b != 29)
																																{
																																	((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref currentLevelLose);
																																	if (b != 30)
																																	{
																																		((MemoryPackReader)(ref reader)).ReadUnmanaged<bool>(ref losingStreakCounted);
																																		if (b != 31)
																																		{
																																			((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref currentLevelLoseForLevel);
																																			if (b != 32)
																																			{
																																				((MemoryPackReader)(ref reader)).ReadUnmanaged<bool>(ref isFirstEnterLevel);
																																				if (b != 33)
																																				{
																																					((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref totalLevelLose);
																																					if (b != 34)
																																					{
																																						((MemoryPackReader)(ref reader)).ReadUnmanaged<bool>(ref lastLevelLoseFlag);
																																						if (b != 35)
																																						{
																																							((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref currentReleaseCount);
																																							if (b != 36)
																																							{
																																								((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref cumulativeReleaseTriggeredCount);
																																								if (b != 37)
																																								{
																																									((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref randomPickLevel);
																																									if (b != 38)
																																									{
																																										((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref randomPickCoreLevelPath);
																																										if (b != 39)
																																										{
																																											((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref randomPickParts);
																																											if (b != 40)
																																											{
																																												((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref randomPickBonus);
																																												if (b != 41)
																																												{
																																													((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref randomPickSpecified);
																																													if (b != 42)
																																													{
																																														randomPickCsv = ((MemoryPackReader)(ref reader)).ReadString();
																																														if (b != 43)
																																														{
																																															((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref randomPickTeachElement);
																																															if (b != 44)
																																															{
																																																((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref stepLimitInitialSteps);
																																																if (b != 45)
																																																{
																																																	((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref stepLimitUsedSteps);
																																																	if (b != 46)
																																																	{
																																																		((MemoryPackReader)(ref reader)).ReadUnmanagedArray<int>(ref stepLimitAdAddSteps);
																																																		_ = 47;
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
				goto IL_0918;
			}
		}
		value = new GameModeData
		{
			level = num,
			TryLevelNum = tryLevelNum,
			LevelSlotId = levelSlotId,
			game_mode = num2,
			GlobalLevelCount = globalLevelCount,
			EndlessSeason = endlessSeason,
			game_score = num3,
			LevelState = levelState,
			AllStepNum = allStepNum,
			steps = num4,
			game_time = num5,
			game_time_real = num6,
			DeadNum = deadNum,
			GameplayState = gameplayState,
			mode_num = num7,
			mode_winnum = num8,
			PropUseDat = propUseDat,
			PropGainDat = propGainDat,
			RewardCoinNum = rewardCoinNum,
			UseCoinNum = useCoinNum,
			WinningStreak = winningStreak,
			TrackSettledGameNum = trackSettledGameNum,
			TrackSettledWinNum = trackSettledWinNum,
			TrackRecentSettledWins = trackRecentSettledWins,
			TrackRecentSettledScores = trackRecentSettledScores,
			TrackModeMaxScore = trackModeMaxScore,
			TrackWinStreakDayKey = trackWinStreakDayKey,
			TrackModeWinStreakDay = trackModeWinStreakDay,
			TrackModeWinStreak = trackModeWinStreak,
			CurrentLevelLose = currentLevelLose,
			LosingStreakCounted = losingStreakCounted,
			CurrentLevelLoseForLevel = currentLevelLoseForLevel,
			IsFirstEnterLevel = isFirstEnterLevel,
			TotalLevelLose = totalLevelLose,
			LastLevelLoseFlag = lastLevelLoseFlag,
			CurrentReleaseCount = currentReleaseCount,
			CumulativeReleaseTriggeredCount = cumulativeReleaseTriggeredCount,
			RandomPickLevel = randomPickLevel,
			RandomPickCoreLevelPath = randomPickCoreLevelPath,
			RandomPickParts = randomPickParts,
			RandomPickBonus = randomPickBonus,
			RandomPickSpecified = randomPickSpecified,
			RandomPickCsv = randomPickCsv,
			RandomPickTeachElement = randomPickTeachElement,
			StepLimitInitialSteps = stepLimitInitialSteps,
			StepLimitUsedSteps = stepLimitUsedSteps,
			StepLimitAdAddSteps = stepLimitAdAddSteps
		};
		return;
		IL_0918:
		value.level = num;
		value.TryLevelNum = tryLevelNum;
		value.LevelSlotId = levelSlotId;
		value.game_mode = num2;
		value.GlobalLevelCount = globalLevelCount;
		value.EndlessSeason = endlessSeason;
		value.game_score = num3;
		value.LevelState = levelState;
		value.AllStepNum = allStepNum;
		value.steps = num4;
		value.game_time = num5;
		value.game_time_real = num6;
		value.DeadNum = deadNum;
		value.GameplayState = gameplayState;
		value.mode_num = num7;
		value.mode_winnum = num8;
		value.PropUseDat = propUseDat;
		value.PropGainDat = propGainDat;
		value.RewardCoinNum = rewardCoinNum;
		value.UseCoinNum = useCoinNum;
		value.WinningStreak = winningStreak;
		value.TrackSettledGameNum = trackSettledGameNum;
		value.TrackSettledWinNum = trackSettledWinNum;
		value.TrackRecentSettledWins = trackRecentSettledWins;
		value.TrackRecentSettledScores = trackRecentSettledScores;
		value.TrackModeMaxScore = trackModeMaxScore;
		value.TrackWinStreakDayKey = trackWinStreakDayKey;
		value.TrackModeWinStreakDay = trackModeWinStreakDay;
		value.TrackModeWinStreak = trackModeWinStreak;
		value.CurrentLevelLose = currentLevelLose;
		value.LosingStreakCounted = losingStreakCounted;
		value.CurrentLevelLoseForLevel = currentLevelLoseForLevel;
		value.IsFirstEnterLevel = isFirstEnterLevel;
		value.TotalLevelLose = totalLevelLose;
		value.LastLevelLoseFlag = lastLevelLoseFlag;
		value.CurrentReleaseCount = currentReleaseCount;
		value.CumulativeReleaseTriggeredCount = cumulativeReleaseTriggeredCount;
		value.RandomPickLevel = randomPickLevel;
		value.RandomPickCoreLevelPath = randomPickCoreLevelPath;
		value.RandomPickParts = randomPickParts;
		value.RandomPickBonus = randomPickBonus;
		value.RandomPickSpecified = randomPickSpecified;
		value.RandomPickCsv = randomPickCsv;
		value.RandomPickTeachElement = randomPickTeachElement;
		value.StepLimitInitialSteps = stepLimitInitialSteps;
		value.StepLimitUsedSteps = stepLimitUsedSteps;
		value.StepLimitAdAddSteps = stepLimitAdAddSteps;
	}
}
