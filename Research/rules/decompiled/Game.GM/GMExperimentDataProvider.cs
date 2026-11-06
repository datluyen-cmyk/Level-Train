using System.Collections.Generic;
using System.Linq;
using Example.Cfg;
using XingYun.Cfg;
using XingYun.GM;

namespace Game.GM;

public class GMExperimentDataProvider : IGMExperimentDataProvider
{
	private readonly Dictionary<string, List<ExperimentConfig>> cache = new Dictionary<string, List<ExperimentConfig>>();

	private ExampleTables tables;

	private ExampleTables Tables
	{
		get
		{
			if (tables == null)
			{
				tables = GameApp.Config.GetTable<ExampleTables>();
			}
			return tables;
		}
	}

	public List<ExperimentConfig> GetExperimentData(string platform)
	{
		if (string.IsNullOrEmpty(platform))
		{
			return new List<ExperimentConfig>();
		}
		string text = platform.ToLower();
		if (cache.TryGetValue(text, out var value))
		{
			return value;
		}
		if (Tables == null)
		{
			return new List<ExperimentConfig>();
		}
		List<ExperimentConfig> list = ((!(text == "ios")) ? (Tables.TbExptConfig_Android?.DataList?.ToList() ?? new List<ExperimentConfig>()) : (Tables.TbExptConfig_IOS?.DataList?.ToList() ?? new List<ExperimentConfig>()));
		cache[text] = list;
		return list;
	}

	public List<ExperimentConfig> FilterByPeriod(string platform, int period)
	{
		return (from e in GetExperimentData(platform)
			where e.Period == period
			select e).ToList();
	}

	public List<int> GetAvailablePeriods(string platform)
	{
		return (from p in (from e in GetExperimentData(platform)
				select e.Period).Distinct()
			orderby p descending
			select p).ToList();
	}

	public void ClearCache()
	{
		cache.Clear();
	}

	public void ClearCache(string platform)
	{
		if (!string.IsNullOrEmpty(platform))
		{
			cache.Remove(platform.ToLower());
		}
	}
}
