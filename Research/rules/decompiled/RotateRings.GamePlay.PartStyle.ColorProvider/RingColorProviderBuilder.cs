using System.Collections.Generic;
using Example.Cfg;

namespace RotateRings.GamePlay.PartStyle.ColorProvider;

public static class RingColorProviderBuilder
{
	private const string TAG = "[PartStyle][RingColorProviderBuilder]";

	public const int BaseboardSchemeId = 1000;

	public static IColorProvider Build(int schemeId, PartStyleType styleType)
	{
		RingColorConfig ringColorConfig = FindConfig(schemeId, styleType);
		if (ringColorConfig == null)
		{
			return null;
		}
		if (HasAnyEmptyColor(ringColorConfig))
		{
			return null;
		}
		return new ConfigRingColorProvider(ringColorConfig);
	}

	private static RingColorConfig FindConfig(int schemeId, PartStyleType styleType)
	{
		ExampleTables table = GameApp.Config.GetTable<ExampleTables>();
		if (table == null)
		{
			return null;
		}
		List<RingColorConfig> list = table.TbRingColorConfig?.DataList;
		if (list == null || list.Count == 0)
		{
			return null;
		}
		for (int i = 0; i < list.Count; i++)
		{
			RingColorConfig ringColorConfig = list[i];
			if (ringColorConfig.SchemeId == schemeId && ringColorConfig.StyleId == styleType)
			{
				return ringColorConfig;
			}
		}
		return null;
	}

	private static bool HasAnyEmptyColor(RingColorConfig cfg)
	{
		if (!string.IsNullOrWhiteSpace(cfg.Color1) && !string.IsNullOrWhiteSpace(cfg.Color2) && !string.IsNullOrWhiteSpace(cfg.Color3) && !string.IsNullOrWhiteSpace(cfg.Color4) && !string.IsNullOrWhiteSpace(cfg.Color5) && !string.IsNullOrWhiteSpace(cfg.Color6))
		{
			return string.IsNullOrWhiteSpace(cfg.Color7);
		}
		return true;
	}
}
