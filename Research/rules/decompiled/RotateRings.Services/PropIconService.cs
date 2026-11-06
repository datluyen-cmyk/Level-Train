using Example.Cfg;
using RotateRings.Prop;
using UnityEngine;
using XingYun.Proxy;

namespace RotateRings.Services;

public static class PropIconService
{
	private enum PropIconSize
	{
		Normal,
		Large
	}

	private const string Tag = "PropIcon";

	public static string GetIconName(EPropType propType)
	{
		return ResolveIconName(PropIconConfig.Instance, propType, 1, PropIconSize.Normal);
	}

	public static string GetIconNameLarge(EPropType propType)
	{
		return ResolveIconName(PropIconConfig.Instance, propType, 1, PropIconSize.Large);
	}

	private static string ResolveIconName(PropIconConfig config, EPropType propType, int itemIconStyle, PropIconSize size)
	{
		if ((Object)(object)config != (Object)null)
		{
			PropIconEntry propIconEntry = config.FindEntry(propType);
			if (propIconEntry != null)
			{
				string text = FindStyleOverride(propIconEntry, itemIconStyle, size);
				if (!string.IsNullOrEmpty(text))
				{
					return text;
				}
			}
		}
		ProxyManager proxy = GameApp.Proxy;
		PropConfig propConfig = ((proxy != null) ? proxy.Get<PropModelProxy>() : null)?.GetPropConfig(propType);
		if (propConfig != null)
		{
			if (size == PropIconSize.Large)
			{
				if (!string.IsNullOrEmpty(propConfig.IconLarge))
				{
					return propConfig.IconLarge;
				}
				if (!string.IsNullOrEmpty(propConfig.Icon))
				{
					return propConfig.Icon;
				}
			}
			else if (!string.IsNullOrEmpty(propConfig.Icon))
			{
				return propConfig.Icon;
			}
		}
		if (propType != EPropType.None)
		{
			return "icon_" + propType.ToString().ToLowerInvariant();
		}
		return string.Empty;
	}

	private static string FindStyleOverride(PropIconEntry entry, int itemIconStyle, PropIconSize size)
	{
		if (entry.styleOverrides == null)
		{
			return null;
		}
		for (int i = 0; i < entry.styleOverrides.Count; i++)
		{
			PropIconStyleOverride propIconStyleOverride = entry.styleOverrides[i];
			if (propIconStyleOverride.styleId == itemIconStyle)
			{
				string text = ((size == PropIconSize.Large) ? propIconStyleOverride.iconNameLarge : propIconStyleOverride.iconName);
				if (!string.IsNullOrEmpty(text))
				{
					return text;
				}
			}
		}
		return null;
	}
}
