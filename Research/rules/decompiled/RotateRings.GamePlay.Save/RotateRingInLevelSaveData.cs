using System;
using MemoryPack;
using MemoryPack.Formatters;
using MemoryPack.Internal;
using XingYun.Save;

namespace RotateRings.GamePlay.Save;

[MemoryPackable(/*Could not decode attribute arguments.*/)]
public class RotateRingInLevelSaveData : ISaveDefaultInitializer, IMemoryPackable<RotateRingInLevelSaveData>, IMemoryPackFormatterRegister
{
	[Preserve]
	private sealed class RotateRingInLevelSaveDataFormatter : MemoryPackFormatter<RotateRingInLevelSaveData>
	{
		[Preserve]
		public override void Serialize(ref MemoryPackWriter writer, ref RotateRingInLevelSaveData value)
		{
			RotateRingInLevelSaveData.Serialize(ref writer, ref value);
		}

		[Preserve]
		public override void Deserialize(ref MemoryPackReader reader, ref RotateRingInLevelSaveData value)
		{
			RotateRingInLevelSaveData.Deserialize(ref reader, ref value);
		}
	}

	public const int CurrentVersion = 4;

	public int Version;

	public bool HasData;

	public int LevelNumber;

	public int LevelSlotId;

	public string CoreLevelJsonHash;

	public RotateRingPartSave[] Parts;

	public RotateRingBombSave[] Bombs;

	public string[] AcornPartIds;

	[Obsolete("已废弃，仅用于兼容旧版残局存档，请勿使用。")]
	public int RankPeriod;

	public int StepLimitRemainingSteps;

	public void OnCreateDefault()
	{
		Version = 4;
		HasData = false;
		LevelNumber = 0;
		LevelSlotId = 0;
		CoreLevelJsonHash = string.Empty;
		Parts = Array.Empty<RotateRingPartSave>();
		Bombs = Array.Empty<RotateRingBombSave>();
		AcornPartIds = Array.Empty<string>();
		StepLimitRemainingSteps = -1;
	}

	static RotateRingInLevelSaveData()
	{
		RegisterFormatter();
	}

	[Preserve]
	public static void RegisterFormatter()
	{
		if (!MemoryPackFormatterProvider.IsRegistered<RotateRingInLevelSaveData>())
		{
			MemoryPackFormatterProvider.Register<RotateRingInLevelSaveData>((MemoryPackFormatter<RotateRingInLevelSaveData>)(object)new RotateRingInLevelSaveDataFormatter());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<RotateRingInLevelSaveData[]>())
		{
			MemoryPackFormatterProvider.Register<RotateRingInLevelSaveData[]>((MemoryPackFormatter<RotateRingInLevelSaveData[]>)(object)new ArrayFormatter<RotateRingInLevelSaveData>());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<RotateRingPartSave[]>())
		{
			MemoryPackFormatterProvider.Register<RotateRingPartSave[]>((MemoryPackFormatter<RotateRingPartSave[]>)(object)new ArrayFormatter<RotateRingPartSave>());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<RotateRingBombSave[]>())
		{
			MemoryPackFormatterProvider.Register<RotateRingBombSave[]>((MemoryPackFormatter<RotateRingBombSave[]>)(object)new ArrayFormatter<RotateRingBombSave>());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<string[]>())
		{
			MemoryPackFormatterProvider.Register<string[]>((MemoryPackFormatter<string[]>)(object)new ArrayFormatter<string>());
		}
	}

	[Preserve]
	public static void Serialize(ref MemoryPackWriter writer, ref RotateRingInLevelSaveData? value)
	{
		if (value == null)
		{
			((MemoryPackWriter)(ref writer)).WriteNullObjectHeader();
			return;
		}
		((MemoryPackWriter)(ref writer)).WriteUnmanagedWithObjectHeader<int, bool, int, int>((byte)10, ref value.Version, ref value.HasData, ref value.LevelNumber, ref value.LevelSlotId);
		((MemoryPackWriter)(ref writer)).WriteString(value.CoreLevelJsonHash);
		((MemoryPackWriter)(ref writer)).WritePackableArray<RotateRingPartSave>(value.Parts);
		((MemoryPackWriter)(ref writer)).WritePackableArray<RotateRingBombSave>(value.Bombs);
		((MemoryPackWriter)(ref writer)).WriteArray<string>(value.AcornPartIds);
		((MemoryPackWriter)(ref writer)).WriteUnmanaged<int, int>(ref value.RankPeriod, ref value.StepLimitRemainingSteps);
	}

	[Preserve]
	public static void Deserialize(ref MemoryPackReader reader, ref RotateRingInLevelSaveData? value)
	{
		byte b = default(byte);
		if (!((MemoryPackReader)(ref reader)).TryReadObjectHeader(ref b))
		{
			value = null;
			return;
		}
		int version = default(int);
		bool hasData = default(bool);
		int levelNumber = default(int);
		int levelSlotId = default(int);
		RotateRingPartSave[] parts;
		RotateRingBombSave[] bombs;
		string[] acornPartIds;
		int rankPeriod = default(int);
		int stepLimitRemainingSteps = default(int);
		string coreLevelJsonHash;
		if (b == 10)
		{
			if (value != null)
			{
				version = value.Version;
				hasData = value.HasData;
				levelNumber = value.LevelNumber;
				levelSlotId = value.LevelSlotId;
				coreLevelJsonHash = value.CoreLevelJsonHash;
				parts = value.Parts;
				bombs = value.Bombs;
				acornPartIds = value.AcornPartIds;
				rankPeriod = value.RankPeriod;
				stepLimitRemainingSteps = value.StepLimitRemainingSteps;
				((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref version);
				((MemoryPackReader)(ref reader)).ReadUnmanaged<bool>(ref hasData);
				((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref levelNumber);
				((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref levelSlotId);
				coreLevelJsonHash = ((MemoryPackReader)(ref reader)).ReadString();
				((MemoryPackReader)(ref reader)).ReadPackableArray<RotateRingPartSave>(ref parts);
				((MemoryPackReader)(ref reader)).ReadPackableArray<RotateRingBombSave>(ref bombs);
				((MemoryPackReader)(ref reader)).ReadArray<string>(ref acornPartIds);
				((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref rankPeriod);
				((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref stepLimitRemainingSteps);
				goto IL_0214;
			}
			((MemoryPackReader)(ref reader)).ReadUnmanaged<int, bool, int, int>(ref version, ref hasData, ref levelNumber, ref levelSlotId);
			coreLevelJsonHash = ((MemoryPackReader)(ref reader)).ReadString();
			parts = ((MemoryPackReader)(ref reader)).ReadPackableArray<RotateRingPartSave>();
			bombs = ((MemoryPackReader)(ref reader)).ReadPackableArray<RotateRingBombSave>();
			acornPartIds = ((MemoryPackReader)(ref reader)).ReadArray<string>();
			((MemoryPackReader)(ref reader)).ReadUnmanaged<int, int>(ref rankPeriod, ref stepLimitRemainingSteps);
		}
		else
		{
			if (b > 10)
			{
				MemoryPackSerializationException.ThrowInvalidPropertyCount(typeof(RotateRingInLevelSaveData), (byte)10, b);
				return;
			}
			if (value == null)
			{
				version = 0;
				hasData = false;
				levelNumber = 0;
				levelSlotId = 0;
				coreLevelJsonHash = null;
				parts = null;
				bombs = null;
				acornPartIds = null;
				rankPeriod = 0;
				stepLimitRemainingSteps = 0;
			}
			else
			{
				version = value.Version;
				hasData = value.HasData;
				levelNumber = value.LevelNumber;
				levelSlotId = value.LevelSlotId;
				coreLevelJsonHash = value.CoreLevelJsonHash;
				parts = value.Parts;
				bombs = value.Bombs;
				acornPartIds = value.AcornPartIds;
				rankPeriod = value.RankPeriod;
				stepLimitRemainingSteps = value.StepLimitRemainingSteps;
			}
			if (b != 0)
			{
				((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref version);
				if (b != 1)
				{
					((MemoryPackReader)(ref reader)).ReadUnmanaged<bool>(ref hasData);
					if (b != 2)
					{
						((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref levelNumber);
						if (b != 3)
						{
							((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref levelSlotId);
							if (b != 4)
							{
								coreLevelJsonHash = ((MemoryPackReader)(ref reader)).ReadString();
								if (b != 5)
								{
									((MemoryPackReader)(ref reader)).ReadPackableArray<RotateRingPartSave>(ref parts);
									if (b != 6)
									{
										((MemoryPackReader)(ref reader)).ReadPackableArray<RotateRingBombSave>(ref bombs);
										if (b != 7)
										{
											((MemoryPackReader)(ref reader)).ReadArray<string>(ref acornPartIds);
											if (b != 8)
											{
												((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref rankPeriod);
												if (b != 9)
												{
													((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref stepLimitRemainingSteps);
													_ = 10;
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
				goto IL_0214;
			}
		}
		value = new RotateRingInLevelSaveData
		{
			Version = version,
			HasData = hasData,
			LevelNumber = levelNumber,
			LevelSlotId = levelSlotId,
			CoreLevelJsonHash = coreLevelJsonHash,
			Parts = parts,
			Bombs = bombs,
			AcornPartIds = acornPartIds,
			RankPeriod = rankPeriod,
			StepLimitRemainingSteps = stepLimitRemainingSteps
		};
		return;
		IL_0214:
		value.Version = version;
		value.HasData = hasData;
		value.LevelNumber = levelNumber;
		value.LevelSlotId = levelSlotId;
		value.CoreLevelJsonHash = coreLevelJsonHash;
		value.Parts = parts;
		value.Bombs = bombs;
		value.AcornPartIds = acornPartIds;
		value.RankPeriod = rankPeriod;
		value.StepLimitRemainingSteps = stepLimitRemainingSteps;
	}
}
