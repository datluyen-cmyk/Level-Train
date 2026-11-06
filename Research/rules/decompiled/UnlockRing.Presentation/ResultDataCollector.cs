using System.Collections.Generic;

namespace UnlockRing.Presentation;

public sealed class ResultDataCollector
{
	private readonly List<IPresentationResult> results = new List<IPresentationResult>(8);

	public void Collect(IPresentationResult result)
	{
		if (result != null)
		{
			results.Add(result);
		}
	}

	public IReadOnlyList<IPresentationResult> CreateSnapshot()
	{
		return new List<IPresentationResult>(results);
	}

	public void Clear()
	{
		results.Clear();
	}
}
