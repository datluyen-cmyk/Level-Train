using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace UnlockRing.Solver;

public sealed class ActionExpansion
{
	public IReadOnlyList<SolverAction> Actions { get; }

	public bool IsComplete { get; }

	public string Reason { get; }

	public ActionExpansion(IEnumerable<SolverAction> actions, bool isComplete = true, string reason = "")
	{
		Actions = new ReadOnlyCollection<SolverAction>((actions ?? throw new ArgumentNullException("actions")).OrderBy<SolverAction, string>((SolverAction x) => x.StableKey, StringComparer.Ordinal).ToArray());
		if (Actions.Select((SolverAction x) => x.StableKey).Distinct<string>(StringComparer.Ordinal).Count() != Actions.Count)
		{
			throw new ArgumentException("Action stable keys must be unique within an expansion.", "actions");
		}
		IsComplete = isComplete;
		Reason = reason ?? string.Empty;
	}
}
