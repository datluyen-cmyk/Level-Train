using UnityEngine;

namespace RotateRings.GamePlay.Services;

public class RingLevelConfigService : GameplayServiceBase<GameplayManager>
{
	public int LevelNumber { get; private set; }

	public string CoreLevelJsonHash { get; private set; } = string.Empty;

	public bool IsUseCoreLevel { get; private set; }

	public bool IsModelLevel { get; private set; }

	public bool SpecifyColor { get; private set; }

	public RingLevelConfigService(GameplayManager host)
		: base(host)
	{
	}

	public void ApplyFromLevelInfo(GameLevelInfo levelInfo, int level)
	{
		LevelNumber = level;
		if ((Object)(object)levelInfo == (Object)null)
		{
			CoreLevelJsonHash = string.Empty;
			IsUseCoreLevel = false;
			IsModelLevel = false;
			SpecifyColor = false;
		}
		else
		{
			CoreLevelJsonHash = levelInfo.CoreLevelJsonHash;
			IsUseCoreLevel = levelInfo.isUseCoreLevel;
			IsModelLevel = levelInfo.isModelLevel;
			SpecifyColor = levelInfo.specifyColor;
		}
	}

	public override void ClearLevel()
	{
		LevelNumber = 0;
		CoreLevelJsonHash = string.Empty;
		IsUseCoreLevel = false;
		IsModelLevel = false;
		SpecifyColor = false;
		base.ClearLevel();
	}

	public string GetCoreLevelJsonHash(int level)
	{
		if (level <= 0 || LevelNumber != level)
		{
			return string.Empty;
		}
		return CoreLevelJsonHash;
	}

	public bool IsNewLevelWithoutContentHash(int level)
	{
		if (level > 0 && LevelNumber == level)
		{
			return string.IsNullOrEmpty(CoreLevelJsonHash);
		}
		return false;
	}
}
