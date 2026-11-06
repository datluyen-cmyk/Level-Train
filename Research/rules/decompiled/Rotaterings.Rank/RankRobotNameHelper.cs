using System;
using System.Collections.Generic;
using Example.Cfg;

namespace Rotaterings.Rank;

public static class RankRobotNameHelper
{
	private static bool RequiresRepair(RankRobotData bot, RankRobotProfileSource source)
	{
		if (bot != null)
		{
			if (!string.IsNullOrWhiteSpace(bot.Name))
			{
				return !source.ContainsNameId(bot.RobotNameId);
			}
			return true;
		}
		return false;
	}

	private static bool TryRepairRobot(RankRobotData bot, RankRobotProfileSource source, Random rng, HashSet<int> reservedNameIds)
	{
		if (source == null || !RequiresRepair(bot, source))
		{
			return false;
		}
		if (source.ContainsNameId(bot.RobotNameId))
		{
			string name = source.GetName(bot.RobotNameId);
			if (bot.Name == name)
			{
				return false;
			}
			bot.Name = name;
			return true;
		}
		if (!source.TryPickRandomNameId(rng, reservedNameIds, out var nameId, out var displayName))
		{
			string name2 = source.GetName(bot.RobotNameId);
			if (bot.Name == name2)
			{
				return false;
			}
			bot.Name = name2;
			return true;
		}
		bool result = bot.RobotNameId != nameId || bot.Name != displayName;
		bot.RobotNameId = nameId;
		bot.Name = displayName;
		return result;
	}

	private static void BuildReservedNameIds(IList<RankRobotData> robots, HashSet<int> reservedNameIds)
	{
		for (int i = 0; i < robots.Count; i++)
		{
			RankRobotData rankRobotData = robots[i];
			if (rankRobotData != null)
			{
				reservedNameIds.Add(rankRobotData.RobotNameId);
			}
		}
	}

	public static bool RepairRobotsIfNeeded(IList<RankRobotData> robots, ExampleTables tables, int nameConfig, Random rng)
	{
		if (robots == null || robots.Count == 0 || tables == null || rng == null)
		{
			return false;
		}
		RankRobotProfileSource source = RankRobotNameConfigProvider.CreateProfileSource(tables, nameConfig);
		bool flag = false;
		for (int i = 0; i < robots.Count; i++)
		{
			if (RequiresRepair(robots[i], source))
			{
				flag = true;
				break;
			}
		}
		if (!flag)
		{
			return false;
		}
		HashSet<int> hashSet = new HashSet<int>();
		BuildReservedNameIds(robots, hashSet);
		bool result = false;
		for (int j = 0; j < robots.Count; j++)
		{
			RankRobotData rankRobotData = robots[j];
			if (RequiresRepair(rankRobotData, source))
			{
				if (TryRepairRobot(rankRobotData, source, rng, hashSet))
				{
					result = true;
				}
				hashSet.Add(rankRobotData.RobotNameId);
			}
		}
		return result;
	}
}
