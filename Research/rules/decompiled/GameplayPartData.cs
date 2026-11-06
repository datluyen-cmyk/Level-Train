using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public struct GameplayPartData
{
	[field: SerializeField]
	public string PhysicPartId { get; set; }

	[field: SerializeField]
	public int Id { get; set; }

	[field: SerializeField]
	public bool CanCollect { get; set; }

	[field: SerializeField]
	public int LockCount { get; set; }

	[field: SerializeField]
	public bool RandomBombCanSelect { get; set; }

	[field: SerializeField]
	public bool HasBomb { get; set; }

	[field: SerializeField]
	public float GapAngleDeg { get; set; }

	[field: SerializeField]
	public float InnerRadius { get; set; }

	[field: SerializeField]
	public float OuterRadius { get; set; }

	[field: SerializeField]
	public PartPaletteColor PaletteColor { get; set; }

	[field: SerializeField]
	public bool UseLevelSpecifiedColor { get; set; }

	[field: SerializeField]
	public PartLevelSpecifiedColor LevelSpecifiedColor { get; set; }

	[field: SerializeField]
	public bool IsPaletteLock { get; set; }

	[field: SerializeField]
	public bool IsPaletteLockKey { get; set; }

	[field: SerializeField]
	public List<PartLinkEntry> LockedBy { get; set; }

	[field: SerializeField]
	public List<PartLinkEntry> Locks { get; set; }

	public static GameplayPartData Create(string physicPartId, int id, bool canCollect = false, int lockCount = 0, float gapAngleDeg = 0f, float innerRadius = 0f, float outerRadius = 0f, bool randomBombCanSelect = true, bool hasBomb = false, PartPaletteColor paletteColor = PartPaletteColor.None, bool isPaletteLock = false, bool isPaletteLockKey = false)
	{
		return new GameplayPartData
		{
			PhysicPartId = physicPartId,
			Id = id,
			CanCollect = canCollect,
			LockCount = lockCount,
			GapAngleDeg = gapAngleDeg,
			InnerRadius = innerRadius,
			OuterRadius = outerRadius,
			RandomBombCanSelect = randomBombCanSelect,
			HasBomb = hasBomb,
			PaletteColor = paletteColor,
			IsPaletteLock = isPaletteLock,
			IsPaletteLockKey = isPaletteLockKey,
			LockedBy = new List<PartLinkEntry>(),
			Locks = new List<PartLinkEntry>()
		};
	}
}
