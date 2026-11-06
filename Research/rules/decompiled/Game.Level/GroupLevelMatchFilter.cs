using System.Collections.Generic;
using Example.Cfg;

namespace Game.Level;

public class GroupLevelMatchFilter : ILevelMatchFilter
{
	private readonly HashSet<int> groupIds;

	public GroupLevelMatchFilter(int groupId)
	{
		groupIds = new HashSet<int> { groupId };
	}

	public GroupLevelMatchFilter(IEnumerable<int> groupIds)
	{
		this.groupIds = ((groupIds == null) ? new HashSet<int>() : new HashSet<int>(groupIds));
	}

	public List<LevelMatchConfig> Filter(List<LevelMatchConfig> configs)
	{
		List<LevelMatchConfig> list = new List<LevelMatchConfig>();
		if (configs == null)
		{
			return list;
		}
		foreach (LevelMatchConfig config in configs)
		{
			if (config != null && groupIds.Contains(config.GroupId))
			{
				list.Add(config);
			}
		}
		return list;
	}

	public string GetMatchFilterInfo()
	{
		if (groupIds.Count == 0)
		{
			return " GroupIds=[] ";
		}
		return " GroupIds=[" + string.Join(",", groupIds) + "] ";
	}
}
