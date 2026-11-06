using System;

namespace UnlockRing.Solver;

public sealed record SolveBudget
{
	public TimeSpan TimeLimit { get; }

	public long MaxExpandedStates { get; }

	public long MaxGeneratedTransitions { get; }

	public SolveBudget(TimeSpan timeLimit, long maxExpandedStates = long.MaxValue, long maxGeneratedTransitions = long.MaxValue)
	{
		if (timeLimit <= TimeSpan.Zero)
		{
			throw new ArgumentOutOfRangeException("timeLimit");
		}
		if (maxExpandedStates <= 0)
		{
			throw new ArgumentOutOfRangeException("maxExpandedStates");
		}
		if (maxGeneratedTransitions <= 0)
		{
			throw new ArgumentOutOfRangeException("maxGeneratedTransitions");
		}
		TimeLimit = timeLimit;
		MaxExpandedStates = maxExpandedStates;
		MaxGeneratedTransitions = maxGeneratedTransitions;
	}

	public static SolveBudget For(SolveBudgetProfile profile)
	{
		return profile switch
		{
			SolveBudgetProfile.Interactive => new SolveBudget(TimeSpan.FromSeconds(1.0)), 
			SolveBudgetProfile.Standard => new SolveBudget(TimeSpan.FromSeconds(30.0)), 
			SolveBudgetProfile.Authoring => new SolveBudget(TimeSpan.FromMinutes(30.0)), 
			_ => throw new ArgumentOutOfRangeException("profile"), 
		};
	}
}
