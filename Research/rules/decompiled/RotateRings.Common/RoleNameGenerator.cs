using System.Collections.Generic;
using Example.Cfg;
using UnityEngine;
using XingYun.Aot.Lang;
using XingYun.Config;

namespace RotateRings.Common;

public static class RoleNameGenerator
{
	private static readonly HashSet<string> EnglishFallbackCodes = new HashSet<string> { "EN_US", "EN_GB" };

	private static int lastRandomNameId = -1;

	private static string lastPickedLanguageCode;

	public static bool TryConsumeLanguageForName(string playerName, out string languageCode)
	{
		languageCode = null;
		if (lastRandomNameId < 0 || string.IsNullOrEmpty(playerName))
		{
			return false;
		}
		ConfigManager config = GameApp.Config;
		RoleNameConfig roleNameConfig = ((config != null) ? config.GetTable<ExampleTables>() : null)?.TbRoleNameConfig?.GetOrDefault(lastRandomNameId);
		if (roleNameConfig == null || roleNameConfig.RandomName != playerName)
		{
			ClearLastPickedEntry();
			return false;
		}
		languageCode = ((!string.IsNullOrEmpty(lastPickedLanguageCode)) ? lastPickedLanguageCode : RoleNameLanguageResolver.ResolveFromRoleNameId(lastRandomNameId));
		ClearLastPickedEntry();
		return !string.IsNullOrEmpty(languageCode);
	}

	public static string GenerateRandomEnglishName()
	{
		List<RoleNameConfig> list = LoadTable();
		List<RoleNameConfig> list2 = ((list == null || list.Count == 0) ? new List<RoleNameConfig>(0) : FilterByAnyLanguage(list, EnglishFallbackCodes));
		if (list2.Count == 0)
		{
			lastRandomNameId = -1;
			lastPickedLanguageCode = null;
			return "Player";
		}
		RoleNameConfig roleNameConfig = PickExcluding(list2, lastRandomNameId);
		CachePickedEntry(roleNameConfig);
		return roleNameConfig.RandomName;
	}

	public static string GenerateRandomName()
	{
		List<RoleNameConfig> list = CollectCandidates(LanguageContext.CurrentLanguage);
		if (list.Count == 0)
		{
			lastRandomNameId = -1;
			lastPickedLanguageCode = null;
			return "Player";
		}
		RoleNameConfig roleNameConfig = PickExcluding(list, lastRandomNameId);
		CachePickedEntry(roleNameConfig);
		return roleNameConfig.RandomName;
	}

	public static string GenerateDefaultName()
	{
		List<RoleNameConfig> list = CollectCandidates(LanguageContext.CurrentLanguage);
		if (list.Count == 0)
		{
			lastRandomNameId = -1;
			lastPickedLanguageCode = null;
			return "Player";
		}
		RoleNameConfig roleNameConfig = list[Random.Range(0, list.Count)];
		CachePickedEntry(roleNameConfig);
		return roleNameConfig.RandomName;
	}

	private static void CachePickedEntry(RoleNameConfig picked)
	{
		lastRandomNameId = picked.ID;
		lastPickedLanguageCode = RoleNameLanguageResolver.PickLanguageFromCountry(picked.Country);
	}

	private static void ClearLastPickedEntry()
	{
		lastRandomNameId = -1;
		lastPickedLanguageCode = null;
	}

	private static List<RoleNameConfig> CollectCandidates(string languageCode)
	{
		List<RoleNameConfig> list = LoadTable();
		if (list == null || list.Count == 0)
		{
			return new List<RoleNameConfig>(0);
		}
		List<RoleNameConfig> list2 = FilterByLanguage(list, languageCode);
		if (list2.Count > 0)
		{
			return list2;
		}
		return FilterByAnyLanguage(list, EnglishFallbackCodes);
	}

	private static List<RoleNameConfig> FilterByLanguage(List<RoleNameConfig> source, string languageCode)
	{
		List<RoleNameConfig> list = new List<RoleNameConfig>();
		if (string.IsNullOrEmpty(languageCode))
		{
			return list;
		}
		foreach (RoleNameConfig item in source)
		{
			if (item.Country == null)
			{
				continue;
			}
			string[] country = item.Country;
			for (int i = 0; i < country.Length; i++)
			{
				if (country[i] == languageCode)
				{
					list.Add(item);
					break;
				}
			}
		}
		return list;
	}

	private static List<RoleNameConfig> FilterByAnyLanguage(List<RoleNameConfig> source, HashSet<string> codes)
	{
		List<RoleNameConfig> list = new List<RoleNameConfig>();
		if (codes == null || codes.Count == 0)
		{
			return list;
		}
		foreach (RoleNameConfig item2 in source)
		{
			if (item2.Country == null)
			{
				continue;
			}
			string[] country = item2.Country;
			foreach (string item in country)
			{
				if (codes.Contains(item))
				{
					list.Add(item2);
					break;
				}
			}
		}
		return list;
	}

	private static RoleNameConfig PickExcluding(List<RoleNameConfig> candidates, int excludeId)
	{
		if (candidates.Count <= 1)
		{
			return candidates[0];
		}
		if (excludeId < 0)
		{
			return candidates[Random.Range(0, candidates.Count)];
		}
		List<RoleNameConfig> list = new List<RoleNameConfig>(candidates.Count - 1);
		foreach (RoleNameConfig candidate in candidates)
		{
			if (candidate.ID != excludeId)
			{
				list.Add(candidate);
			}
		}
		if (list.Count <= 0)
		{
			return candidates[0];
		}
		return list[Random.Range(0, list.Count)];
	}

	private static List<RoleNameConfig> LoadTable()
	{
		ConfigManager config = GameApp.Config;
		return ((config != null) ? config.GetTable<ExampleTables>() : null)?.TbRoleNameConfig?.DataList;
	}
}
