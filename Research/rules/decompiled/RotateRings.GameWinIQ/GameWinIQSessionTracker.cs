using System.Collections.Generic;
using UnlockRing;

namespace RotateRings.GameWinIQ;

public sealed class GameWinIQSessionTracker
{
	private readonly List<int> unlockReleaseCounts = new List<int>();

	private int maxUnlockReleaseCount;

	private int invalidAndWrongStepCount;

	private int baseScoreAtLevelEnter;

	private bool isActive;

	public IReadOnlyList<int> UnlockReleaseCounts => unlockReleaseCounts;

	public int MaxUnlockReleaseCount => maxUnlockReleaseCount;

	public int InvalidAndWrongStepCount => invalidAndWrongStepCount;

	public int BaseScoreAtLevelEnter => baseScoreAtLevelEnter;

	public void BeginSession()
	{
		isActive = true;
	}

	public void SetBaseScoreAtLevelEnter(int baseScore)
	{
		baseScoreAtLevelEnter = baseScore;
	}

	public void ResetPerformanceStats()
	{
		unlockReleaseCounts.Clear();
		maxUnlockReleaseCount = 0;
		invalidAndWrongStepCount = 0;
	}

	public void EndSession()
	{
		isActive = false;
	}

	public void Reset()
	{
		unlockReleaseCounts.Clear();
		maxUnlockReleaseCount = 0;
		invalidAndWrongStepCount = 0;
		baseScoreAtLevelEnter = 0;
		isActive = false;
	}

	public void RecordStep(GameplayStepRecord step)
	{
		if (!isActive || step == null)
		{
			return;
		}
		if (step.State == GameplayStepState.UnlockRelease && step.ReleasedParts.Count > 0)
		{
			int count = step.ReleasedParts.Count;
			unlockReleaseCounts.Add(count);
			if (count > maxUnlockReleaseCount)
			{
				maxUnlockReleaseCount = count;
			}
		}
		else if (step.State == GameplayStepState.Error || step.State == GameplayStepState.NoEffect)
		{
			invalidAndWrongStepCount++;
		}
	}
}
