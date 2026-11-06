using System;
using System.Collections.Generic;

namespace UnlockRing;

public sealed class GameplayOperationScoreTracker
{
	private const string TAG = "[UnlockRing][GameplayOperationScoreTracker] ";

	private const int RecentStepWindowSize = 5;

	private readonly Action<GameplayOperationScore> onScoreEvaluated;

	private readonly Queue<bool> recentStepReleases = new Queue<bool>(5);

	private int consecutiveReleaseCount;

	private int pendingIntervalSteps;

	private bool previousStepWasRelease;

	private bool isInitialized;

	public int RecentStepCount => recentStepReleases.Count;

	public GameplayOperationScoreTracker(Action<GameplayOperationScore> onScoreEvaluated = null)
	{
		this.onScoreEvaluated = onScoreEvaluated;
	}

	public void Initialize()
	{
		Reset();
		isInitialized = true;
	}

	public void Evaluate(GameplayStepRecord step)
	{
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		if (!isInitialized || step == null)
		{
			return;
		}
		bool flag = step.State == GameplayStepState.UnlockRelease && step.ReleasedParts.Count > 0;
		PushRecentStep(flag);
		if (!flag)
		{
			previousStepWasRelease = false;
			pendingIntervalSteps++;
			return;
		}
		int intervalSteps = pendingIntervalSteps;
		int recentReleaseSteps = CountRecentReleaseSteps();
		consecutiveReleaseCount = ((!previousStepWasRelease) ? 1 : (consecutiveReleaseCount + 1));
		previousStepWasRelease = true;
		pendingIntervalSteps = 0;
		int count = step.ReleasedParts.Count;
		for (int i = 0; i < step.ReleasedParts.Count; i++)
		{
			GameplayReleasedPart gameplayReleasedPart = step.ReleasedParts[i];
			GameplayOperationScore obj = GameplayOperationScoreCalculator.Calculate(gameplayReleasedPart.PartId, count, consecutiveReleaseCount, gameplayReleasedPart.ThreadingCount, intervalSteps, recentReleaseSteps);
			onScoreEvaluated?.Invoke(obj);
		}
	}

	private void PushRecentStep(bool isRelease)
	{
		if (recentStepReleases.Count == 5)
		{
			recentStepReleases.Dequeue();
		}
		recentStepReleases.Enqueue(isRelease);
	}

	public int CountRecentReleaseSteps()
	{
		int num = 0;
		foreach (bool recentStepRelease in recentStepReleases)
		{
			if (recentStepRelease)
			{
				num++;
			}
		}
		return num;
	}

	public void Reset()
	{
		consecutiveReleaseCount = 0;
		pendingIntervalSteps = 0;
		previousStepWasRelease = false;
		recentStepReleases.Clear();
		isInitialized = false;
	}
}
