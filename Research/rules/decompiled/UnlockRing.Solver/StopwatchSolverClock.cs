using System;
using System.Diagnostics;

namespace UnlockRing.Solver;

public sealed class StopwatchSolverClock : ISolverClock
{
	public static StopwatchSolverClock Instance { get; } = new StopwatchSolverClock();

	private StopwatchSolverClock()
	{
	}

	public long GetTimestamp()
	{
		return Stopwatch.GetTimestamp();
	}

	public TimeSpan GetElapsedTime(long startTimestamp)
	{
		return TimeSpan.FromSeconds((double)(Stopwatch.GetTimestamp() - startTimestamp) / (double)Stopwatch.Frequency);
	}
}
