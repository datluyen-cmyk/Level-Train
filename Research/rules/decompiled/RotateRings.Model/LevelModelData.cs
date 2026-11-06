using System;
using System.Collections.Generic;
using Hot.Model;
using MemoryPack;
using MemoryPack.Formatters;
using MemoryPack.Internal;
using RotateRings.Prop;
using RotateRings.SpecialLevel;
using XingYun.Save;

namespace RotateRings.Model;

[MemoryPackable(/*Could not decode attribute arguments.*/)]
public class LevelModelData : ISaveDefaultInitializer, IMemoryPackable<LevelModelData>, IMemoryPackFormatterRegister
{
	[Preserve]
	private sealed class LevelModelDataFormatter : MemoryPackFormatter<LevelModelData>
	{
		[Preserve]
		public override void Serialize(ref MemoryPackWriter writer, ref LevelModelData value)
		{
			LevelModelData.Serialize(ref writer, ref value);
		}

		[Preserve]
		public override void Deserialize(ref MemoryPackReader reader, ref LevelModelData value)
		{
			LevelModelData.Deserialize(ref reader, ref value);
		}
	}

	public Dictionary<GameMode, GameModeData> Game_mode_data_map;

	public int TrackAllSettledGameNum;

	public int TrackAllSettledWinNum;

	public int TrackAllMaxScore;

	public int ChestCurrentCycleIndex = -1;

	public int ChestCurrentLevelProgress;

	public int LastClaimedCycleIndex = -1;

	public GameModeData GameModeData => GetGameModeData(GameInfo.CurrentGameMode);

	public GameModeData GetGameModeData(GameMode gameMode)
	{
		if (Game_mode_data_map == null)
		{
			Game_mode_data_map = new Dictionary<GameMode, GameModeData>();
		}
		if (!Game_mode_data_map.TryGetValue(gameMode, out var value) || value == null)
		{
			value = new GameModeData
			{
				game_mode = (int)gameMode
			};
			Game_mode_data_map[gameMode] = value;
		}
		return value;
	}

	public int GetAllModeNum()
	{
		return SumModeField((GameModeData d) => d.mode_num);
	}

	public int GetAllModeWinnum()
	{
		return SumModeField((GameModeData d) => d.mode_winnum);
	}

	public void AddGameModelWinNum(GameMode gameMode)
	{
		if (TryGetModeData(gameMode, out var data))
		{
			data.mode_winnum++;
		}
	}

	public void AddGameModelPropUseData(GameMode gameMode, EPropType propType, TrackPropUseReason reasonId, int useCount)
	{
		if (TryGetModeData(gameMode, out var data))
		{
			GameModeData gameModeData = data;
			if (gameModeData.PropUseDat == null)
			{
				gameModeData.PropUseDat = new Dictionary<EPropType, Dictionary<int, int>>();
			}
			if (!data.PropUseDat.TryGetValue(propType, out var value) || value == null)
			{
				value = new Dictionary<int, int>();
				data.PropUseDat[propType] = value;
			}
			value.TryGetValue((int)reasonId, out var value2);
			value[(int)reasonId] = value2 + useCount;
		}
	}

	public void AddGameModelPropGainData(GameMode gameMode, EPropType propType, TrackPropGainReason reasonId, int gainCount)
	{
		if (TryGetModeData(gameMode, out var data))
		{
			GameModeData gameModeData = data;
			if (gameModeData.PropGainDat == null)
			{
				gameModeData.PropGainDat = new Dictionary<EPropType, Dictionary<int, int>>();
			}
			if (!data.PropGainDat.TryGetValue(propType, out var value) || value == null)
			{
				value = new Dictionary<int, int>();
				data.PropGainDat[propType] = value;
			}
			value.TryGetValue((int)reasonId, out var value2);
			value[(int)reasonId] = value2 + gainCount;
		}
	}

	public Dictionary<int, int> GetGameModelPropUseData(GameMode gameMode, EPropType propType)
	{
		if (TryGetModeData(gameMode, out var data) && data.PropUseDat != null && data.PropUseDat.TryGetValue(propType, out var value))
		{
			return value;
		}
		return null;
	}

	public int GetGameModelPropUseCount(GameMode gameMode, EPropType propType, TrackPropUseReason reason)
	{
		Dictionary<int, int> gameModelPropUseData = GetGameModelPropUseData(gameMode, propType);
		if (gameModelPropUseData == null || !gameModelPropUseData.TryGetValue((int)reason, out var value))
		{
			return 0;
		}
		return value;
	}

	public int GetGameModelPropUseCount(GameMode gameMode, EPropType propType)
	{
		Dictionary<int, int> gameModelPropUseData = GetGameModelPropUseData(gameMode, propType);
		if (gameModelPropUseData == null)
		{
			return 0;
		}
		int num = 0;
		foreach (KeyValuePair<int, int> item in gameModelPropUseData)
		{
			num += item.Value;
		}
		return num;
	}

	public Dictionary<int, int> GetGameModelPropGainData(GameMode gameMode, EPropType propType)
	{
		if (TryGetModeData(gameMode, out var data) && data.PropGainDat != null && data.PropGainDat.TryGetValue(propType, out var value))
		{
			return value;
		}
		return null;
	}

	public int GetGameModelPropGainCount(GameMode gameMode, EPropType propType, TrackPropGainReason reason)
	{
		Dictionary<int, int> gameModelPropGainData = GetGameModelPropGainData(gameMode, propType);
		if (gameModelPropGainData == null || !gameModelPropGainData.TryGetValue((int)reason, out var value))
		{
			return 0;
		}
		return value;
	}

	public int GetGameModelPropGainCount(GameMode gameMode, EPropType propType)
	{
		Dictionary<int, int> gameModelPropGainData = GetGameModelPropGainData(gameMode, propType);
		if (gameModelPropGainData == null)
		{
			return 0;
		}
		int num = 0;
		foreach (KeyValuePair<int, int> item in gameModelPropGainData)
		{
			num += item.Value;
		}
		return num;
	}

	public void ClearGameModelPropUseData(GameMode gameMode)
	{
		if (TryGetModeData(gameMode, out var data))
		{
			data.PropUseDat?.Clear();
		}
	}

	public void RefreshCurrentGameMode(int mainLevelMax, int currentEndlessSeason)
	{
		GameInfo.CurrentGameMode = ((GetGameModeData(GameMode.Main).level > mainLevelMax) ? GameMode.Endless : GameMode.Main);
		if (GameInfo.CurrentGameMode == GameMode.Endless)
		{
			EnsureEndlessSeason(currentEndlessSeason);
		}
		SpecialLevelModule.ApplyActiveUncompletedSessionGameMode();
	}

	private void EnsureEndlessSeason(int currentEndlessSeason)
	{
		GameModeData gameModeData = GetGameModeData(GameMode.Endless);
		if (gameModeData.EndlessSeason != currentEndlessSeason)
		{
			gameModeData.ResetForNewSeason();
			gameModeData.EndlessSeason = currentEndlessSeason;
		}
	}

	public void OnCreateDefault()
	{
		if (Game_mode_data_map == null)
		{
			Game_mode_data_map = new Dictionary<GameMode, GameModeData>();
		}
		GameMode gameMode = GameMode.Main;
		if (!Game_mode_data_map.TryGetValue(gameMode, out var value) || value == null)
		{
			Game_mode_data_map[gameMode] = new GameModeData
			{
				game_mode = (int)gameMode,
				level = 1
			};
		}
	}

	private bool TryGetModeData(GameMode gameMode, out GameModeData data)
	{
		data = null;
		if (Game_mode_data_map != null && Game_mode_data_map.TryGetValue(gameMode, out data))
		{
			return data != null;
		}
		return false;
	}

	private int SumModeField(Func<GameModeData, int> selector)
	{
		if (Game_mode_data_map == null)
		{
			return 0;
		}
		int num = 0;
		foreach (KeyValuePair<GameMode, GameModeData> item in Game_mode_data_map)
		{
			int num2 = selector(item.Value);
			if (num2 > 0)
			{
				num += num2;
			}
		}
		return num;
	}

	static LevelModelData()
	{
		RegisterFormatter();
	}

	[Preserve]
	public static void RegisterFormatter()
	{
		if (!MemoryPackFormatterProvider.IsRegistered<LevelModelData>())
		{
			MemoryPackFormatterProvider.Register<LevelModelData>((MemoryPackFormatter<LevelModelData>)(object)new LevelModelDataFormatter());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<LevelModelData[]>())
		{
			MemoryPackFormatterProvider.Register<LevelModelData[]>((MemoryPackFormatter<LevelModelData[]>)(object)new ArrayFormatter<LevelModelData>());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<Dictionary<GameMode, GameModeData>>())
		{
			MemoryPackFormatterProvider.Register<Dictionary<GameMode, GameModeData>>((MemoryPackFormatter<Dictionary<GameMode, GameModeData>>)(object)new DictionaryFormatter<GameMode, GameModeData>());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<GameMode>())
		{
			MemoryPackFormatterProvider.Register<GameMode>((MemoryPackFormatter<GameMode>)(object)new UnmanagedFormatter<GameMode>());
		}
	}

	[Preserve]
	public static void Serialize(ref MemoryPackWriter writer, ref LevelModelData? value)
	{
		if (value == null)
		{
			((MemoryPackWriter)(ref writer)).WriteNullObjectHeader();
			return;
		}
		((MemoryPackWriter)(ref writer)).WriteObjectHeader((byte)8);
		((MemoryPackWriter)(ref writer)).WriteValue<Dictionary<GameMode, GameModeData>>(ref value.Game_mode_data_map);
		((MemoryPackWriter)(ref writer)).WriteUnmanaged<int, int, int, int, int, int>(ref value.TrackAllSettledGameNum, ref value.TrackAllSettledWinNum, ref value.TrackAllMaxScore, ref value.ChestCurrentCycleIndex, ref value.ChestCurrentLevelProgress, ref value.LastClaimedCycleIndex);
		GameModeData gameModeData = value.GameModeData;
		((MemoryPackWriter)(ref writer)).WritePackable<GameModeData>(ref gameModeData);
	}

	[Preserve]
	public static void Deserialize(ref MemoryPackReader reader, ref LevelModelData? value)
	{
		byte b = default(byte);
		if (!((MemoryPackReader)(ref reader)).TryReadObjectHeader(ref b))
		{
			value = null;
			return;
		}
		Dictionary<GameMode, GameModeData> game_mode_data_map;
		int trackAllSettledGameNum = default(int);
		int trackAllSettledWinNum = default(int);
		int trackAllMaxScore = default(int);
		int chestCurrentCycleIndex = default(int);
		int chestCurrentLevelProgress = default(int);
		int lastClaimedCycleIndex = default(int);
		if (b == 8)
		{
			GameModeData gameModeData;
			if (value != null)
			{
				game_mode_data_map = value.Game_mode_data_map;
				trackAllSettledGameNum = value.TrackAllSettledGameNum;
				trackAllSettledWinNum = value.TrackAllSettledWinNum;
				trackAllMaxScore = value.TrackAllMaxScore;
				chestCurrentCycleIndex = value.ChestCurrentCycleIndex;
				chestCurrentLevelProgress = value.ChestCurrentLevelProgress;
				lastClaimedCycleIndex = value.LastClaimedCycleIndex;
				gameModeData = value.GameModeData;
				((MemoryPackReader)(ref reader)).ReadValue<Dictionary<GameMode, GameModeData>>(ref game_mode_data_map);
				((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref trackAllSettledGameNum);
				((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref trackAllSettledWinNum);
				((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref trackAllMaxScore);
				((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref chestCurrentCycleIndex);
				((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref chestCurrentLevelProgress);
				((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref lastClaimedCycleIndex);
				((MemoryPackReader)(ref reader)).ReadPackable<GameModeData>(ref gameModeData);
				goto IL_01a6;
			}
			game_mode_data_map = ((MemoryPackReader)(ref reader)).ReadValue<Dictionary<GameMode, GameModeData>>();
			((MemoryPackReader)(ref reader)).ReadUnmanaged<int, int, int, int, int, int>(ref trackAllSettledGameNum, ref trackAllSettledWinNum, ref trackAllMaxScore, ref chestCurrentCycleIndex, ref chestCurrentLevelProgress, ref lastClaimedCycleIndex);
			gameModeData = ((MemoryPackReader)(ref reader)).ReadPackable<GameModeData>();
		}
		else
		{
			if (b > 8)
			{
				MemoryPackSerializationException.ThrowInvalidPropertyCount(typeof(LevelModelData), (byte)8, b);
				return;
			}
			GameModeData gameModeData;
			if (value == null)
			{
				game_mode_data_map = null;
				trackAllSettledGameNum = 0;
				trackAllSettledWinNum = 0;
				trackAllMaxScore = 0;
				chestCurrentCycleIndex = 0;
				chestCurrentLevelProgress = 0;
				lastClaimedCycleIndex = 0;
				gameModeData = null;
			}
			else
			{
				game_mode_data_map = value.Game_mode_data_map;
				trackAllSettledGameNum = value.TrackAllSettledGameNum;
				trackAllSettledWinNum = value.TrackAllSettledWinNum;
				trackAllMaxScore = value.TrackAllMaxScore;
				chestCurrentCycleIndex = value.ChestCurrentCycleIndex;
				chestCurrentLevelProgress = value.ChestCurrentLevelProgress;
				lastClaimedCycleIndex = value.LastClaimedCycleIndex;
				gameModeData = value.GameModeData;
			}
			if (b != 0)
			{
				((MemoryPackReader)(ref reader)).ReadValue<Dictionary<GameMode, GameModeData>>(ref game_mode_data_map);
				if (b != 1)
				{
					((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref trackAllSettledGameNum);
					if (b != 2)
					{
						((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref trackAllSettledWinNum);
						if (b != 3)
						{
							((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref trackAllMaxScore);
							if (b != 4)
							{
								((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref chestCurrentCycleIndex);
								if (b != 5)
								{
									((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref chestCurrentLevelProgress);
									if (b != 6)
									{
										((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref lastClaimedCycleIndex);
										if (b != 7)
										{
											((MemoryPackReader)(ref reader)).ReadPackable<GameModeData>(ref gameModeData);
											_ = 8;
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
				goto IL_01a6;
			}
		}
		value = new LevelModelData
		{
			Game_mode_data_map = game_mode_data_map,
			TrackAllSettledGameNum = trackAllSettledGameNum,
			TrackAllSettledWinNum = trackAllSettledWinNum,
			TrackAllMaxScore = trackAllMaxScore,
			ChestCurrentCycleIndex = chestCurrentCycleIndex,
			ChestCurrentLevelProgress = chestCurrentLevelProgress,
			LastClaimedCycleIndex = lastClaimedCycleIndex
		};
		return;
		IL_01a6:
		value.Game_mode_data_map = game_mode_data_map;
		value.TrackAllSettledGameNum = trackAllSettledGameNum;
		value.TrackAllSettledWinNum = trackAllSettledWinNum;
		value.TrackAllMaxScore = trackAllMaxScore;
		value.ChestCurrentCycleIndex = chestCurrentCycleIndex;
		value.ChestCurrentLevelProgress = chestCurrentLevelProgress;
		value.LastClaimedCycleIndex = lastClaimedCycleIndex;
	}
}
