using System.Collections.Generic;
using Example.Cfg;
using XingYun.Aot.Lang;
using XingYun.Config;

namespace RotateRings.Common;

public static class RoleNameLanguageResolver
{
	public static string ResolveFromConfig(string name)
	{
		if (string.IsNullOrEmpty(name))
		{
			return null;
		}
		ConfigManager config = GameApp.Config;
		List<RoleNameConfig> list = ((config != null) ? config.GetTable<ExampleTables>() : null)?.TbRoleNameConfig?.DataList;
		if (list == null || list.Count == 0)
		{
			return null;
		}
		string text = null;
		string currentLanguage = LanguageContext.CurrentLanguage;
		foreach (RoleNameConfig item in list)
		{
			if (item.RandomName != name)
			{
				continue;
			}
			if (text == null)
			{
				text = PickLanguageFromCountry(item.Country);
			}
			if (string.IsNullOrEmpty(currentLanguage) || item.Country == null)
			{
				continue;
			}
			for (int i = 0; i < item.Country.Length; i++)
			{
				if (item.Country[i] == currentLanguage)
				{
					return PickLanguageFromCountry(item.Country);
				}
			}
		}
		return text;
	}

	public static string ResolveFromRoleNameId(int id)
	{
		if (id < 0)
		{
			return null;
		}
		ConfigManager config = GameApp.Config;
		RoleNameConfig roleNameConfig = ((config != null) ? config.GetTable<ExampleTables>() : null)?.TbRoleNameConfig?.GetOrDefault(id);
		if (roleNameConfig == null)
		{
			return null;
		}
		return PickLanguageFromCountry(roleNameConfig.Country);
	}

	public static string PickLanguageFromCountry(string[] countryCodes)
	{
		if (countryCodes == null || countryCodes.Length == 0)
		{
			return null;
		}
		string currentLanguage = LanguageContext.CurrentLanguage;
		if (!string.IsNullOrEmpty(currentLanguage))
		{
			for (int i = 0; i < countryCodes.Length; i++)
			{
				if (countryCodes[i] == currentLanguage)
				{
					return currentLanguage;
				}
			}
		}
		return countryCodes[0];
	}
}
