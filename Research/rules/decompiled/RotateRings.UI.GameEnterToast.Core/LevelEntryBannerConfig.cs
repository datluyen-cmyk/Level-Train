using System;
using RotateRings.GameWinIQ;
using XingYun.Experiment;

namespace RotateRings.UI.GameEnterToast.Core;

public static class LevelEntryBannerConfig
{
	public const int Style1 = 1;

	public const int Style2 = 2;

	public const int Style3 = 3;

	public const int Style4 = 4;

	public const int StyleHalloween = 5;

	public static string HalloweenBannerLangKey = "Ring_Halloween_Level_In";

	public const string HalloweenAudioVariantId = "variant_e7a38092";

	public const string ShapeBannerAudioVariantId = "variant_f0b830e4";

	public const int DefaultOpenLevel = 12;

	public const int DefaultStyleId = 1;

	public const int DefaultShapeStyleId = -1;

	public static Func<int, bool> IsShapeLevel = ResolveShapeLevelFromGameplayLevelConfig;

	public static int TempPlayerIq
	{
		get
		{
			int valueOrDefault = (GameApp.Proxy.Get<GameWinIQProxy>()?.Data?.LastDisplayScore).GetValueOrDefault();
			if (valueOrDefault <= 0)
			{
				return 120;
			}
			return valueOrDefault;
		}
	}

	public static bool ResolveOpenFlg()
	{
		return FunctionDataCenter.Get<LevelEntryBannerFunction>()?.OpenFlg ?? false;
	}

	public static int ResolveOpenLevel()
	{
		LevelEntryBannerFunction levelEntryBannerFunction = FunctionDataCenter.Get<LevelEntryBannerFunction>();
		if (levelEntryBannerFunction == null || levelEntryBannerFunction.OpenLevel <= 0)
		{
			return 12;
		}
		return levelEntryBannerFunction.OpenLevel;
	}

	public static int ResolveStyleId()
	{
		LevelEntryBannerFunction levelEntryBannerFunction = FunctionDataCenter.Get<LevelEntryBannerFunction>();
		if (levelEntryBannerFunction == null)
		{
			return 1;
		}
		int styleId = levelEntryBannerFunction.StyleId;
		if (styleId != 1 && styleId != 2 && styleId != 3 && styleId != 4)
		{
			return 1;
		}
		return levelEntryBannerFunction.StyleId;
	}

	public static int ResolveShapeStyleId()
	{
		LevelEntryBannerFunction levelEntryBannerFunction = FunctionDataCenter.Get<LevelEntryBannerFunction>();
		if (levelEntryBannerFunction == null)
		{
			return ResolveStyleId();
		}
		int shapeStyleId = levelEntryBannerFunction.ShapeStyleId;
		if (shapeStyleId != 1 && shapeStyleId != 2 && shapeStyleId != 3 && shapeStyleId != 4)
		{
			return ResolveStyleId();
		}
		return levelEntryBannerFunction.ShapeStyleId;
	}

	public static int ResolveStyleId(bool shapeBanner)
	{
		if (!shapeBanner)
		{
			return ResolveStyleId();
		}
		return ResolveShapeStyleId();
	}

	public static bool ResolveShapeBannerFlg()
	{
		return FunctionDataCenter.Get<LevelEntryBannerFunction>()?.ShapeBannerFlg ?? false;
	}

	public static string ResolveLocation(int styleId, bool shapeBanner)
	{
		if (styleId == 5)
		{
			return "UIGameEnterToastView_Halloween";
		}
		if (shapeBanner)
		{
			if (styleId != 3 && styleId != 4)
			{
				return "UIGameEnterToastViewShape1";
			}
			return "UIGameEnterToastViewShape2";
		}
		if (styleId != 3 && styleId != 4)
		{
			return "UIGameEnterToastView1";
		}
		return "UIGameEnterToastView2";
	}

	public static bool ResolveShowBgMask(int styleId)
	{
		if (styleId != 2)
		{
			return styleId == 4;
		}
		return true;
	}

	public static bool ShouldShowShapeBanner(int level)
	{
		if (ResolveShapeBannerFlg())
		{
			return IsShapeLevel(level);
		}
		return false;
	}

	private static bool ResolveShapeLevelFromGameplayLevelConfig(int level)
	{
		if (level <= 0 || !(Gameplay.Active is GameplayManager { LevelConfig: var levelConfig }))
		{
			return false;
		}
		if (levelConfig != null && levelConfig.LevelNumber == level)
		{
			return levelConfig.IsModelLevel;
		}
		return false;
	}
}
