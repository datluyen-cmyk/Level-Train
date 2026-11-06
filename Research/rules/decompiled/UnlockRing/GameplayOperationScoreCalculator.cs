using System;
using System.Collections.Generic;
using Example.Cfg;
using UnlockRing.Core.Domain;
using XingYun.Config;
using XingYun.Core.Singletons;
using XingYun.Experiment;

namespace UnlockRing;

public static class GameplayOperationScoreCalculator
{
	private const string TAG = "[UnlockRing][GameplayOperationScoreCalculator] ";

	private const int DefaultGroupId = 1;

	private static readonly int[] ReleaseCountScoreFallback = new int[3] { 30, 60, 100 };

	private static readonly int[] ConsecutiveReleaseScoreFallback = new int[5] { 0, 10, 20, 40, 60 };

	private static readonly int[] RecentReleaseStepScoreFallback = new int[4] { 0, 10, 20, 30 };

	public static GameplayOperationScore Calculate(PartId partId, int releaseCount, int consecutiveReleaseCount, int threadingCount, int intervalSteps, int recentReleaseSteps)
	{
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		ValidateArguments(releaseCount, consecutiveReleaseCount, threadingCount, intervalSteps, recentReleaseSteps);
		OperationScoreConfig config = ResolveConfig();
		int releaseCountScore = CalculateReleaseCountScore(releaseCount, config);
		int consecutiveReleaseScore = CalculateConsecutiveReleaseScore(consecutiveReleaseCount, config);
		int threadingScore = CalculateThreadingScore(threadingCount);
		int recentReleaseStepScore = CalculateRecentReleaseStepScore(recentReleaseSteps, config);
		return new GameplayOperationScore(partId, releaseCount, consecutiveReleaseCount, threadingCount, intervalSteps, recentReleaseSteps, releaseCountScore, consecutiveReleaseScore, threadingScore, recentReleaseStepScore);
	}

	private static OperationScoreConfig ResolveConfig()
	{
		int num = 1;
		OperationScoreFunction operationScoreFunction = FunctionDataCenter.Get<OperationScoreFunction>();
		if (operationScoreFunction != null)
		{
			num = operationScoreFunction.OperationScoreGroup;
		}
		TbOperationScoreConfig tbOperationScoreConfig = null;
		try
		{
			tbOperationScoreConfig = Singleton<ConfigManager>.Instance.GetTable<ExampleTables>()?.TbOperationScoreConfig;
		}
		catch (Exception)
		{
			return null;
		}
		if (tbOperationScoreConfig == null)
		{
			return null;
		}
		OperationScoreConfig operationScoreConfig = tbOperationScoreConfig.Get(num);
		if (operationScoreConfig == null && num != 1)
		{
			operationScoreConfig = tbOperationScoreConfig.Get(1);
		}
		return operationScoreConfig;
	}

	private static int GetSteppedScore(List<int> keys, List<int> scores, int value, int[] fallback)
	{
		if (keys == null || scores == null || keys.Count == 0 || keys.Count != scores.Count)
		{
			return GetFallbackScore(fallback, value);
		}
		int result = scores[0];
		bool flag = false;
		for (int i = 0; i < keys.Count && value >= keys[i]; i++)
		{
			result = scores[i];
			flag = true;
		}
		if (!flag)
		{
			return 0;
		}
		return result;
	}

	private static int GetFallbackScore(int[] fallback, int value)
	{
		int num = ((value > 1) ? (value - 1) : 0);
		if (num >= fallback.Length)
		{
			num = fallback.Length - 1;
		}
		return fallback[num];
	}

	private static int CalculateReleaseCountScore(int releaseCount, OperationScoreConfig config)
	{
		return GetSteppedScore(config?.UnlockNum, config?.UnlockNumScore, releaseCount, ReleaseCountScoreFallback);
	}

	private static int CalculateConsecutiveReleaseScore(int consecutiveReleaseCount, OperationScoreConfig config)
	{
		return GetSteppedScore(config?.StepNum, config?.StepNumScore, consecutiveReleaseCount, ConsecutiveReleaseScoreFallback);
	}

	private static int CalculateThreadingScore(int threadingCount)
	{
		return 0;
	}

	private static int CalculateRecentReleaseStepScore(int recentReleaseSteps, OperationScoreConfig config)
	{
		List<int> list = config?.ContinuousTopScore;
		if (list == null || list.Count == 0)
		{
			return GetFallbackScore(RecentReleaseStepScoreFallback, recentReleaseSteps);
		}
		int num = ((recentReleaseSteps > 1) ? (recentReleaseSteps - 1) : 0);
		if (num >= list.Count)
		{
			num = list.Count - 1;
		}
		return list[num];
	}

	private static void ValidateArguments(int releaseCount, int consecutiveReleaseCount, int threadingCount, int intervalSteps, int recentReleaseSteps)
	{
		if (releaseCount <= 0)
		{
			throw new ArgumentOutOfRangeException("releaseCount");
		}
		if (consecutiveReleaseCount <= 0)
		{
			throw new ArgumentOutOfRangeException("consecutiveReleaseCount");
		}
		if (threadingCount < 0)
		{
			throw new ArgumentOutOfRangeException("threadingCount");
		}
		if (intervalSteps < 0)
		{
			throw new ArgumentOutOfRangeException("intervalSteps");
		}
		if (recentReleaseSteps <= 0)
		{
			throw new ArgumentOutOfRangeException("recentReleaseSteps");
		}
	}
}
