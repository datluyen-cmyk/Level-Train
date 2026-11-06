using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace UnlockRing.Solver;

public sealed class SolutionWitness
{
	public IReadOnlyList<SolutionStep> Steps { get; }

	public SolverCost Cost { get; }

	public SolutionStep? Hint
	{
		get
		{
			if (Steps.Count != 0)
			{
				return Steps[0];
			}
			return null;
		}
	}

	public SolutionWitness(IEnumerable<SolutionStep> steps, SolverCost cost)
	{
		Steps = new ReadOnlyCollection<SolutionStep>((steps ?? throw new ArgumentNullException("steps")).ToArray());
		Cost = cost;
	}
}
