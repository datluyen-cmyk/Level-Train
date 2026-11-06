using System;
using System.Collections.Generic;
using Example.Cfg;
using UnlockRing.Core.Domain;
using XingYun.Config;
using XingYun.Core.Singletons;
using XingYun.Experiment;

namespace UnlockRing;

public sealed class GameplayOperationScore
{
	private const int DefaultEvaluationGroupId = 1;

	public PartId PartId { get; }

	public int ReleaseCount { get; }

	public int ConsecutiveReleaseCount { get; }

	public int ThreadingCount { get; }

	public int IntervalSteps { get; }

	public int RecentReleaseSteps { get; }

	public int ReleaseCountScore { get; }

	public int ConsecutiveReleaseScore { get; }

	public int ThreadingScore { get; }

	public int RecentReleaseStepScore { get; }

	public int BestStreakScore { get; }

	public int TotalScore { get; }

	public GameplayOperationEvaluation Evaluation { get; }

	public string Encouragement => GetEncouragement(Evaluation);

	public GameplayOperationScore(PartId partId, int releaseCount, int consecutiveReleaseCount, int threadingCount, int intervalSteps, int recentReleaseSteps, int releaseCountScore, int consecutiveReleaseScore, int threadingScore, int recentReleaseStepScore)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		PartId = partId;
		ReleaseCount = releaseCount;
		ConsecutiveReleaseCount = consecutiveReleaseCount;
		ThreadingCount = threadingCount;
		IntervalSteps = intervalSteps;
		RecentReleaseSteps = recentReleaseSteps;
		ReleaseCountScore = releaseCountScore;
		ConsecutiveReleaseScore = consecutiveReleaseScore;
		ThreadingScore = threadingScore;
		RecentReleaseStepScore = recentReleaseStepScore;
		BestStreakScore = Math.Max(consecutiveReleaseScore, recentReleaseStepScore);
		TotalScore = releaseCountScore + threadingScore + BestStreakScore;
		Evaluation = GetEvaluation(TotalScore);
	}

	private static GameplayOperationEvaluation GetEvaluation(int totalScore)
	{
		OperationEvaluationConfig operationEvaluationConfig = ResolveEvaluationConfig();
		if (operationEvaluationConfig == null)
		{
			return GameplayOperationEvaluation.None;
		}
		if (IsScoreInRange(totalScore, operationEvaluationConfig.Evaluation1))
		{
			return GameplayOperationEvaluation.Great;
		}
		if (IsScoreInRange(totalScore, operationEvaluationConfig.Evaluation2))
		{
			return GameplayOperationEvaluation.Excellent;
		}
		if (IsScoreInRange(totalScore, operationEvaluationConfig.Evaluation3))
		{
			return GameplayOperationEvaluation.Amazing;
		}
		if (IsScoreInRange(totalScore, operationEvaluationConfig.Evaluation4))
		{
			return GameplayOperationEvaluation.Perfect;
		}
		if (IsScoreInRange(totalScore, operationEvaluationConfig.Evaluation5))
		{
			return GameplayOperationEvaluation.Unbelievable;
		}
		return GameplayOperationEvaluation.None;
	}

	private static OperationEvaluationConfig ResolveEvaluationConfig()
	{
		int num = FunctionDataCenter.Get<OperationScoreFunction>()?.OperationEvaluationGroup ?? 1;
		try
		{
			TbOperationEvaluation tbOperationEvaluation = Singleton<ConfigManager>.Instance.GetTable<ExampleTables>()?.TbOperationEvaluation;
			if (tbOperationEvaluation == null)
			{
				return null;
			}
			OperationEvaluationConfig result = null;
			foreach (OperationEvaluationConfig data in tbOperationEvaluation.DataList)
			{
				if (data.GroupID == num)
				{
					return data;
				}
				if (data.GroupID == 1)
				{
					result = data;
				}
			}
			return result;
		}
		catch (Exception)
		{
			return null;
		}
	}

	private static bool IsScoreInRange(int totalScore, List<int> range)
	{
		if (range != null && range.Count >= 2 && totalScore >= range[0])
		{
			return totalScore <= range[1];
		}
		return false;
	}

	private static string GetEncouragement(GameplayOperationEvaluation evaluation)
	{
		return evaluation switch
		{
			GameplayOperationEvaluation.Great => "Great！", 
			GameplayOperationEvaluation.Excellent => "Excellent！", 
			GameplayOperationEvaluation.Amazing => "Amazing！", 
			GameplayOperationEvaluation.Perfect => "Perfect！", 
			GameplayOperationEvaluation.Unbelievable => "Unbelievable！", 
			_ => string.Empty, 
		};
	}
}
