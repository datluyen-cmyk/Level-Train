using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace UnlockRing.Solver;

public sealed record BombAttribution
{
	public string BombId { get; }

	public IReadOnlyList<string> RemovedPartIds { get; }

	public string Reason { get; }

	public BombAttribution(string bombId, IEnumerable<string> removedPartIds, string reason)
	{
		BombId = bombId ?? throw new ArgumentNullException("bombId");
		RemovedPartIds = NormalizeRemovedParts(bombId, removedPartIds);
		Reason = reason ?? string.Empty;
	}

	private static IReadOnlyList<string> NormalizeRemovedParts(string bombId, IEnumerable<string> removedPartIds)
	{
		Compatibility.ThrowIfNullOrWhiteSpace(bombId, "bombId");
		Compatibility.ThrowIfNull(removedPartIds, "removedPartIds");
		string[] source = removedPartIds.ToArray();
		if (source.Any(string.IsNullOrWhiteSpace))
		{
			throw new ArgumentException("Removed part ids must not be empty.", "removedPartIds");
		}
		string[] array = source.Distinct<string>(StringComparer.Ordinal).OrderBy<string, string>((string x) => x, StringComparer.Ordinal).ToArray();
		if (!array.Contains<string>(bombId, StringComparer.Ordinal))
		{
			throw new ArgumentException("Bomb attribution must include removal of its source host.", "removedPartIds");
		}
		return new ReadOnlyCollection<string>(array);
	}
}
