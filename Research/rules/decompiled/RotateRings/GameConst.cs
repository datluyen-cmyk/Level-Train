using UnityEngine;

namespace RotateRings;

public class GameConst
{
	public const string GameResPath = "Assets/02_Res";

	public static bool IsCreatePhysic = false;

	public const int WinCoins = 10;

	public const int JokerCardCoins = 20;

	public const int GameMode = 0;

	public const float PerFrameSec = 0.0166f;

	public static int InterAdCd = 70;

	public const int InterAdOpenLevel = 9;

	public const float InterAdFrontScreenDelay = 0.5f;

	public const int PassLevelExp = 10;

	public const int DayFirstWinExp = 60;

	public const int WinningStreakExp = 5;

	public const int WinningStreakMaxExp = 100;

	public const string CardResGroupTag = "CardResGroupTag";

	public const string CardBgResGroupTag = "CardBgResGroupTag";

	public const string ImageMaterialStyleResGroupTag = "ImageMaterialStyleResGroupTag";

	public const bool UseCoreLevel = false;

	public const string RingGameScene = "GameScene";

	public const int JigsawPuzzleStartLevelNumber = 70000;

	public const int SettlePopupOrderDailyFirstWin = 50;

	public const int SettlePopupOrderRank = 100;

	public const int PostSettleOrderCloudSaveApply = int.MaxValue;

	public const int DragAreaLayer = 6;

	public const int OverlapAreaLayer = 7;

	public const int HomePopupOrderCloudSaveLoginApply = 10;

	public const int HomePopupOrderUnclaimedReward = 40;

	public const int HomePopupOrderRankClaim = 50;

	public const int HomePopupOrderDailyFirstWinReward = 152;

	public const int HomePopupOrderRankActivityOpen = 200;

	public static LayerMask OverlapAreaLayerMask => LayerMask.op_Implicit(128);

	public static LayerMask DragAreaLayerMask => LayerMask.op_Implicit(64);

	public static LayerMask RingInputLayerMask => LayerMask.op_Implicit(LayerMask.op_Implicit(DragAreaLayerMask) | LayerMask.op_Implicit(OverlapAreaLayerMask));
}
