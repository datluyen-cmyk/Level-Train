namespace RotateRings.Common;

public static class UIPopupPathDefine
{
	public const string UISettingHome = "UISettingHome";

	public const string UIAboutUsV2 = "UIAboutUs";

	public const string UIUserV2 = "UIUserV2";

	public const string UIConnectionLostV2 = "UIConnectionLostV2";

	public const string UIBuySuccess = "UIBuySuccess";

	public const string UICollection4JigsawPuzzleV2 = "UICollection4JigsawPuzzle";

	public const string UISettingHomeV1 = "UISettingHome";

	public const string UIAboutUsV1 = "UIAboutUs";

	public const string UIUserV1 = "UIUser";

	public const string UIConnectionLostV1 = "UIConnectionLost";

	public const string UIHowToPlayView = "UIHowToPlayView";

	public const string UICollection4JigsawPuzzleV1 = "UICollection4JigsawPuzzle";

	public const string UILevelAdPop = "UILevelAdPop";

	public const string UIPeakTrailGiveUp4NoTimeBack = "UIPeakTrailGiveUp4NoTimeBack";

	public const string UIPeakTrailGiveUp4Replay = "UIPeakTrailGiveUp4Replay";

	public static bool? IsV2GmOverride { get; set; }

	public static bool IsV2 => IsV2GmOverride ?? true;

	public static string UIAboutUs
	{
		get
		{
			if (!IsV2)
			{
				return "UIAboutUs";
			}
			return "UIAboutUs";
		}
	}

	public static string UIUser
	{
		get
		{
			if (!IsV2)
			{
				return "UIUser";
			}
			return "UIUserV2";
		}
	}

	public static string UIConnectionLost => "UIConnectionLost";

	public static string UICollection4JigsawPuzzle
	{
		get
		{
			if (!IsV2)
			{
				return "UICollection4JigsawPuzzle";
			}
			return "UICollection4JigsawPuzzle";
		}
	}
}
