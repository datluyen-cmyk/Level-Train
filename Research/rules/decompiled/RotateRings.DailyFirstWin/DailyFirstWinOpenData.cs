using System;
using System.Collections.Generic;

namespace RotateRings.DailyFirstWin;

public class DailyFirstWinOpenData
{
	public DailyFirstWinOpenMode Mode;

	public int CurrentDay;

	public int PeriodDays;

	public IReadOnlyList<int> MilestoneDays;

	public bool HasReward;

	public int PendingRewardDay;

	public bool IsMultiplierMotivated;

	public bool HasFirstWinToday;

	public int PuzzleId;

	public bool IsHalloweenPuzzleTheme;

	public bool PlayPieceUnlockAnim;

	public bool RequireNewPieceBeforeProgress;

	public Action OnClosed;
}
