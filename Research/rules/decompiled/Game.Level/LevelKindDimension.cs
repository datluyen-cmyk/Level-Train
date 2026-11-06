using System.Collections.Generic;
using System.Linq;
using XingYun.Level;

namespace Game.Level;

public class LevelKindDimension : DimensionStrategyBase
{
	public const string KIND_KEY = "Kind";

	public override string Key => "Kind";

	public override List<string> FilterFiles(LevelContext ctx, LevelRepository repo, List<string> candidates)
	{
		if (candidates == null || candidates.Count == 0)
		{
			return candidates;
		}
		List<string> availableValuesWithStrategy = ((DimensionStrategyBase)this).GetAvailableValuesWithStrategy(ctx, repo);
		if (availableValuesWithStrategy == null || availableValuesWithStrategy.Count == 0)
		{
			return candidates;
		}
		HashSet<string> hashSet = new HashSet<string>();
		foreach (string item in availableValuesWithStrategy)
		{
			foreach (string item2 in repo.GetFilesWithDimensionValue(((DimensionStrategyBase)this).Key, item))
			{
				hashSet.Add(item2);
			}
		}
		if (hashSet.Count == 0)
		{
			return candidates;
		}
		List<string> list = candidates.Where(hashSet.Contains).ToList();
		if (list.Count <= 0)
		{
			return candidates;
		}
		return list;
	}
}
