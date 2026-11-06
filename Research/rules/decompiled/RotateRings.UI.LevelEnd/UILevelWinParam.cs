using System;
using RotateRings.GameWinIQ;

namespace RotateRings.UI.LevelEnd;

public class UILevelWinParam
{
	public int Score;

	public int StarCount;

	public int CompletedLevel;

	public bool IsVictorySnapshot;

	public bool IsMainToEndlessTransition;

	public Action OnNext;

	public GameWinIQSettlementSnapshot GameWinIqSnapshot;
}
