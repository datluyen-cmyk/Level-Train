using System;
using System.Collections.Generic;
using System.Linq;

namespace UnlockRing.Solver;

public sealed class NogoodStore
{
	private readonly SortedDictionary<string, ReliableConflictSet> _sets = new SortedDictionary<string, ReliableConflictSet>(StringComparer.Ordinal);

	public int Count => _sets.Count;

	public IReadOnlyCollection<ReliableConflictSet> Learned => _sets.Values;

	public bool Learn(ReliableConflictSet conflict)
	{
		Compatibility.ThrowIfNull(conflict, "conflict");
		return _sets.TryAdd(conflict.StableKey, conflict);
	}

	public ReliableConflictSet? FindTriggered(IEnumerable<DecisionLiteral> decisions)
	{
		HashSet<DecisionLiteral> set = decisions.ToHashSet();
		return _sets.Values.FirstOrDefault((ReliableConflictSet x) => x.IsTriggeredBy(set));
	}
}
