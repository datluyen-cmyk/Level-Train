using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace UnlockRing.Solver;

public sealed class SolveResult
{
	public SolveStatus Status { get; }

	public SolutionWitness? Solution { get; }

	public string Reason { get; }

	public long ExpandedStates { get; }

	public long GeneratedTransitions { get; }

	public TimeSpan Elapsed { get; }

	public IReadOnlyList<ReliableConflictSet> LearnedNogoods { get; }

	public SolutionStep? Hint => Solution?.Hint;

	internal SolveResult(SolveStatus status, SolutionWitness? solution, string reason, long expandedStates, long generatedTransitions, TimeSpan elapsed, IEnumerable<ReliableConflictSet> learnedNogoods)
	{
		Status = status;
		Solution = solution;
		Reason = reason;
		ExpandedStates = expandedStates;
		GeneratedTransitions = generatedTransitions;
		Elapsed = elapsed;
		LearnedNogoods = new ReadOnlyCollection<ReliableConflictSet>(learnedNogoods.OrderBy<ReliableConflictSet, string>((ReliableConflictSet x) => x.StableKey, StringComparer.Ordinal).ToArray());
	}
}
