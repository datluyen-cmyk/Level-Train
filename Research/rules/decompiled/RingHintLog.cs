using System.Collections.Generic;
using System.Diagnostics;
using UnlockRing.Core.Domain;
using UnlockRing.Solver;

public static class RingHintLog
{
	private const string FinderTag = "[UnlockRing][HintFinder] ";

	private const string PresenterTag = "[UnlockRing][HintPresenter] ";

	private const string LogPrefix = "[UnlockRingHint] ";

	private static string Describe(in RingHintResult result)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Invalid comparison between Unknown and I4
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		if ((int)result.MotionKind == 0)
		{
			return $"Rotate {result.MotionAmount:0.##}°";
		}
		if ((int)result.MotionKind == 1)
		{
			Vector2D translateAxis = result.TranslateAxis;
			return $"Translate {result.MotionAmount:0.##} axis=({((Vector2D)(ref translateAxis)).X:0.###},{((Vector2D)(ref translateAxis)).Y:0.###})";
		}
		return $"{result.MotionKind} {result.MotionAmount:0.##}";
	}

	private static string Describe(MotionRequest motion)
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		if (motion == null)
		{
			return "none";
		}
		if ((int)motion.Kind == 0)
		{
			return $"Rotate {motion.RequestedAmount:0.##}°";
		}
		if (motion.Direction.HasValue)
		{
			Vector2D value = motion.Direction.Value;
			return $"Translate {motion.RequestedAmount:0.##} axis=({((Vector2D)(ref value)).X:0.###},{((Vector2D)(ref value)).Y:0.###})";
		}
		return $"{motion.Kind} {motion.RequestedAmount:0.##}";
	}

	[Conditional("SRDEBUG")]
	public static void LogQueryStart(string levelId, long revision)
	{
	}

	[Conditional("SRDEBUG")]
	public static void LogPhaseTiming(double releaseMs, double movableMs, double solverMs, double totalMs)
	{
	}

	[Conditional("SRDEBUG")]
	public static void LogReleaseScanStats(int scanCount, int skipCount)
	{
	}

	[Conditional("SRDEBUG")]
	public static void LogControllerCacheReused(int totalCount, int applicableCount)
	{
	}

	[Conditional("SRDEBUG")]
	public static void LogQueryAvailableOperationsFallback(int count)
	{
	}

	[Conditional("SRDEBUG")]
	public static void LogSolverSkippedHasRelease()
	{
	}

	[Conditional("SRDEBUG")]
	public static void LogSolverSkippedHasOperable()
	{
	}

	[Conditional("SRDEBUG")]
	public static void LogSolverNoFirstStep(SolveStatus status, string reason)
	{
	}

	[Conditional("SRDEBUG")]
	public static void LogSolverFirstStep(MotionRequest motion)
	{
	}

	[Conditional("SRDEBUG")]
	public static void LogSolverMatched(string partId, int beforeWeight, int afterWeight)
	{
	}

	[Conditional("SRDEBUG")]
	public static void LogSolverAdded(string partId, int weight)
	{
	}

	[Conditional("SRDEBUG")]
	public static void LogSolverFailed(string message)
	{
	}

	[Conditional("SRDEBUG")]
	public static void LogReleaseProgressBonus(string partId, int progressScore, int afterWeight)
	{
	}

	[Conditional("SRDEBUG")]
	public static void LogAllResults(IReadOnlyList<RingHintResult> ranked)
	{
		if (ranked.Count != 0)
		{
			for (int i = 0; i < ranked.Count; i++)
			{
				_ = ranked[i];
			}
			_ = ranked[0];
		}
	}

	[Conditional("SRDEBUG")]
	public static void LogShowHintFailedNotReady()
	{
	}

	[Conditional("SRDEBUG")]
	public static void LogConfirmedOnPart(string partId)
	{
	}

	[Conditional("SRDEBUG")]
	public static void LogWrongPartAttempt(string attemptedPartId, string targetPartId)
	{
	}

	[Conditional("SRDEBUG")]
	public static void LogQueryNoCandidates()
	{
	}

	[Conditional("SRDEBUG")]
	public static void LogShowTop1(int totalCount, in RingHintResult best)
	{
	}

	[Conditional("SRDEBUG")]
	public static void LogQueryFailed(string message)
	{
	}
}
