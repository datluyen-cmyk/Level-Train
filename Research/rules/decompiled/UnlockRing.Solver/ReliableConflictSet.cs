using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace UnlockRing.Solver;

public sealed class ReliableConflictSet
{
	public IReadOnlyList<DecisionLiteral> Literals { get; }

	public string Reason { get; }

	public string StableKey { get; }

	public ReliableConflictSet(IEnumerable<DecisionLiteral> literals, string reason = "")
	{
		Compatibility.ThrowIfNull(literals, "literals");
		DecisionLiteral[] array = literals.Distinct().OrderBy<DecisionLiteral, string>((DecisionLiteral x) => x.StableKey, StringComparer.Ordinal).ToArray();
		if (array.Length == 0)
		{
			throw new ArgumentException("A conflict set cannot be empty.", "literals");
		}
		Literals = new ReadOnlyCollection<DecisionLiteral>(array);
		Reason = reason ?? string.Empty;
		StableKey = string.Join("|", array.Select((DecisionLiteral x) => x.StableKey));
	}

	internal bool IsTriggeredBy(IReadOnlyCollection<DecisionLiteral> decisions)
	{
		return Literals.All(((IEnumerable<DecisionLiteral>)decisions).Contains<DecisionLiteral>);
	}
}
