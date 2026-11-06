using System;
using System.Collections.Generic;
using Example.Cfg;

namespace Rotaterings.Rank;

public static class RankRobotNameConfigProvider
{
	private const string Tag = "Rank";

	private const int DefaultConfig = 0;

	private const int NicknamePool1Config = 1;

	private const int NicknamePool2Config = 2;

	private const int NicknamePool3Config = 3;

	public static int ResolveAvailableConfig(ExampleTables tables, int configuredValue)
	{
		if (configuredValue == 0 || IsNicknamePoolAvailable(tables, configuredValue))
		{
			return configuredValue;
		}
		return 0;
	}

	public static RankRobotProfileSource CreateProfileSource(ExampleTables tables, int appliedConfig, HashSet<int> excludedNameIds = null)
	{
		return new RankRobotProfileSource(tables, excludedNameIds, BuildNicknameMap(tables, appliedConfig));
	}

	public static HashSet<int> GetNameIdsMatching(ExampleTables tables, int appliedConfig, string playerName)
	{
		if (string.IsNullOrEmpty(playerName))
		{
			return null;
		}
		Dictionary<int, string> dictionary = BuildNicknameMap(tables, appliedConfig);
		HashSet<int> hashSet = null;
		foreach (KeyValuePair<int, string> item in dictionary)
		{
			if (string.Equals(item.Value, playerName, StringComparison.Ordinal))
			{
				if (hashSet == null)
				{
					hashSet = new HashSet<int>();
				}
				hashSet.Add(item.Key);
			}
		}
		return hashSet;
	}

	private static bool IsNicknamePoolAvailable(ExampleTables tables, int config)
	{
		return config switch
		{
			1 => tables != null && tables.TbRankRobotNicknamePool1?.DataList?.Count > 0, 
			2 => tables != null && tables.TbRankRobotNicknamePool2?.DataList?.Count > 0, 
			3 => tables != null && tables.TbRankRobotNicknamePool3?.DataList?.Count > 0, 
			_ => false, 
		};
	}

	private static Dictionary<int, string> BuildNicknameMap(ExampleTables tables, int appliedConfig)
	{
		return appliedConfig switch
		{
			1 => BuildNicknameMap(tables?.TbRankRobotNicknamePool1?.DataList), 
			2 => BuildNicknameMap(tables?.TbRankRobotNicknamePool2?.DataList), 
			3 => BuildNicknameMap(tables?.TbRankRobotNicknamePool3?.DataList), 
			_ => BuildDefaultNameMap(tables?.TbRobotNameConfig?.DataList), 
		};
	}

	private static Dictionary<int, string> BuildNicknameMap<T>(List<T> nameList) where T : class
	{
		Dictionary<int, string> dictionary = new Dictionary<int, string>(nameList?.Count ?? 0);
		if (nameList == null)
		{
			return dictionary;
		}
		for (int i = 0; i < nameList.Count; i++)
		{
			T val = nameList[i];
			if (!(val is RankRobotNicknamePool1 rankRobotNicknamePool))
			{
				if (!(val is RankRobotNicknamePool2 rankRobotNicknamePool2))
				{
					if (val is RankRobotNicknamePool3 rankRobotNicknamePool3 && !string.IsNullOrWhiteSpace(rankRobotNicknamePool3.Name))
					{
						dictionary[rankRobotNicknamePool3.ID] = rankRobotNicknamePool3.Name;
					}
				}
				else if (!string.IsNullOrWhiteSpace(rankRobotNicknamePool2.Name))
				{
					dictionary[rankRobotNicknamePool2.ID] = rankRobotNicknamePool2.Name;
				}
			}
			else if (!string.IsNullOrWhiteSpace(rankRobotNicknamePool.Name))
			{
				dictionary[rankRobotNicknamePool.ID] = rankRobotNicknamePool.Name;
			}
		}
		return dictionary;
	}

	private static Dictionary<int, string> BuildDefaultNameMap(List<RobotNameConfig> nameList)
	{
		Dictionary<int, string> dictionary = new Dictionary<int, string>(nameList?.Count ?? 0);
		if (nameList == null)
		{
			return dictionary;
		}
		for (int i = 0; i < nameList.Count; i++)
		{
			RobotNameConfig robotNameConfig = nameList[i];
			string displayName = RankRobotProfileSource.GetDisplayName(robotNameConfig);
			if (robotNameConfig != null && !string.IsNullOrWhiteSpace(displayName))
			{
				dictionary[robotNameConfig.ID] = displayName;
			}
		}
		return dictionary;
	}
}
