using System;

namespace UnlockRing.Solver;

public interface ISolverClock
{
	long GetTimestamp();

	TimeSpan GetElapsedTime(long startTimestamp);
}
