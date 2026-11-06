namespace RotateRings.GameWinIQ;

public readonly struct GameWinIQSettlementDetail
{
	public int CompletedLevel { get; }

	public int LevelTailDigit { get; }

	public int DifficultyIndex { get; }

	public int LevelClearBonus { get; }

	public int PropUseCount { get; }

	public int PropBonusApplied { get; }

	public int PropBonusCap { get; }

	public int RestartCount { get; }

	public int RestartPenaltyApplied { get; }

	public int RestartPenaltyFloorCap { get; }

	public int MaxUnlockReleaseCount { get; }

	public int UnlockSessionBonus { get; }

	public int UnlockSessionBonusCap { get; }

	public int InvalidStepCount { get; }

	public int InvalidSessionPenalty { get; }

	public int InvalidSessionPenaltyCap { get; }

	public float HighBaseRandomCoeff { get; }

	public int MappedDisplayFromBase { get; }

	public GameWinIQSettlementDetail(int completedLevel, int levelTailDigit, int difficultyIndex, int levelClearBonus, int propUseCount, int propBonusApplied, int propBonusCap, int restartCount, int restartPenaltyApplied, int restartPenaltyFloorCap, int maxUnlockReleaseCount, int unlockSessionBonus, int unlockSessionBonusCap, int invalidStepCount, int invalidSessionPenalty, int invalidSessionPenaltyCap, float highBaseRandomCoeff, int mappedDisplayFromBase)
	{
		CompletedLevel = completedLevel;
		LevelTailDigit = levelTailDigit;
		DifficultyIndex = difficultyIndex;
		LevelClearBonus = levelClearBonus;
		PropUseCount = propUseCount;
		PropBonusApplied = propBonusApplied;
		PropBonusCap = propBonusCap;
		RestartCount = restartCount;
		RestartPenaltyApplied = restartPenaltyApplied;
		RestartPenaltyFloorCap = restartPenaltyFloorCap;
		MaxUnlockReleaseCount = maxUnlockReleaseCount;
		UnlockSessionBonus = unlockSessionBonus;
		UnlockSessionBonusCap = unlockSessionBonusCap;
		InvalidStepCount = invalidStepCount;
		InvalidSessionPenalty = invalidSessionPenalty;
		InvalidSessionPenaltyCap = invalidSessionPenaltyCap;
		HighBaseRandomCoeff = highBaseRandomCoeff;
		MappedDisplayFromBase = mappedDisplayFromBase;
	}
}
