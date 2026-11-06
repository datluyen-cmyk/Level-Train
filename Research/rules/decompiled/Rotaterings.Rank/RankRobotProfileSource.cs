using System;
using System.Collections.Generic;
using Example.Cfg;
using XingYun.RankRobot;

namespace Rotaterings.Rank;

public sealed class RankRobotProfileSource : IRobotProfileSource
{
	private readonly ExampleTables tables;

	private readonly HashSet<int> excludedNameIds;

	private readonly IReadOnlyDictionary<int, string> nicknameMap;

	public RankRobotProfileSource(ExampleTables tables, HashSet<int> excludedNameIds = null, IReadOnlyDictionary<int, string> nicknameMap = null)
	{
		this.tables = tables;
		this.excludedNameIds = excludedNameIds;
		this.nicknameMap = nicknameMap;
	}

	public List<int> GetAllNameIds()
	{
		if (nicknameMap != null)
		{
			return GetNicknameMapIds();
		}
		List<RobotNameConfig> list = tables?.TbRobotNameConfig?.DataList;
		if (list == null || list.Count == 0)
		{
			return BuildFallbackNameIds();
		}
		List<int> list2 = new List<int>(list.Count);
		for (int i = 0; i < list.Count; i++)
		{
			RobotNameConfig robotNameConfig = list[i];
			if (robotNameConfig != null && (excludedNameIds == null || !excludedNameIds.Contains(robotNameConfig.ID)))
			{
				list2.Add(robotNameConfig.ID);
			}
		}
		if (list2.Count <= 0)
		{
			return BuildFallbackNameIds();
		}
		return list2;
	}

	private List<int> GetNicknameMapIds()
	{
		List<int> list = new List<int>(nicknameMap.Count);
		foreach (KeyValuePair<int, string> item in nicknameMap)
		{
			if (excludedNameIds == null || !excludedNameIds.Contains(item.Key))
			{
				list.Add(item.Key);
			}
		}
		if (list.Count <= 0)
		{
			return BuildFallbackNameIds();
		}
		return list;
	}

	public RobotProfile GetRandomProfile(int nameId, Random rng)
	{
		int headId = PickRandomHeadId(rng);
		int headFrameId = PickRandomHeadFrameId(rng);
		return new RobotProfile
		{
			HeadId = headId,
			HeadFrameId = headFrameId
		};
	}

	public bool ContainsNameId(int nameId)
	{
		string value;
		if (nicknameMap != null)
		{
			return nicknameMap.TryGetValue(nameId, out value);
		}
		return !string.IsNullOrWhiteSpace(GetDisplayName(tables?.TbRobotNameConfig?.GetOrDefault(nameId)));
	}

	public string GetName(int nameId)
	{
		if (nicknameMap != null && nicknameMap.TryGetValue(nameId, out var value) && !string.IsNullOrEmpty(value))
		{
			return value;
		}
		string displayName = GetDisplayName(tables?.TbRobotNameConfig?.GetOrDefault(nameId));
		if (!string.IsNullOrEmpty(displayName))
		{
			return displayName;
		}
		return $"Player{nameId}";
	}

	public bool TryPickRandomNameId(Random rng, IReadOnlyCollection<int> reservedNameIds, out int nameId, out string displayName)
	{
		nameId = 0;
		displayName = null;
		if (rng == null)
		{
			return false;
		}
		List<int> allNameIds = GetAllNameIds();
		if (allNameIds == null || allNameIds.Count == 0)
		{
			return false;
		}
		int num = rng.Next(0, allNameIds.Count);
		for (int i = 0; i < allNameIds.Count; i++)
		{
			int num2 = allNameIds[(num + i) % allNameIds.Count];
			if (reservedNameIds == null || !ContainsId(reservedNameIds, num2))
			{
				nameId = num2;
				displayName = GetName(num2);
				return true;
			}
		}
		return false;
	}

	private static bool ContainsId(IReadOnlyCollection<int> ids, int value)
	{
		foreach (int id in ids)
		{
			if (id == value)
			{
				return true;
			}
		}
		return false;
	}

	public static string GetDisplayName(RobotNameConfig cfg)
	{
		if (cfg == null)
		{
			return null;
		}
		if (!UsesHappyName(cfg))
		{
			return cfg.Rolename;
		}
		return cfg.Happy;
	}

	private static bool UsesHappyName(RobotNameConfig cfg)
	{
		if (cfg.Country == null || cfg.Country.Length == 0)
		{
			return false;
		}
		for (int i = 0; i < cfg.Country.Length; i++)
		{
			string text = cfg.Country[i];
			if (text != "EN_US" && text != "EN_GB")
			{
				return true;
			}
		}
		return false;
	}

	private int PickRandomHeadId(Random rng)
	{
		List<HeadConfig> list = tables?.TbHeadConfig?.DataList;
		if (list == null || list.Count == 0)
		{
			return 0;
		}
		return list[rng.Next(0, list.Count)]?.ID ?? 0;
	}

	private int PickRandomHeadFrameId(Random rng)
	{
		return 1;
	}

	private static List<int> BuildFallbackNameIds()
	{
		List<int> list = new List<int>(RankConst.MaxRankBot);
		for (int i = 1; i <= RankConst.MaxRankBot; i++)
		{
			list.Add(i);
		}
		return list;
	}
}
