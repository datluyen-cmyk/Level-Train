namespace RotateRings.GamePlay.Services;

public static class LevelEnterPipelinePriorities
{
	public const int LevelLoadingStarted = 5;

	public const int PrepareLevelLoad = 10;

	public const int ClearLevelAndWaitFrame = 20;

	public const int LoadLevelPrefab = 30;

	public const int ValidateLoadedLevelPrefab = 40;

	public const int ResetLevelStartPipelineState = 50;

	public const int InstantiateLevelRoot = 60;

	public const int CollectLevelGameplayObjects = 70;

	public const int ApplyLevelConfigAndStyle = 80;

	public const int ApplyLevelPartStyle = 81;

	public const int ApplyBoardFlip = 85;

	public const int PrepareLevelGameplayCamera = 90;

	public const int AdaptCoreLevelRender = 100;

	public const int ResetLevelSessionState = 110;

	public const int RegisterGameplaySolveHandlers = 120;

	public const int RegisterButterflySlotGuides = 130;

	public const int RegisterLevelInteractables = 140;

	public const int RebuildLevelManifestsAndScore = 141;

	public const int ResetDragCollisionOnLevel = 142;

	public const int AttachLevelBombEffects = 143;

	public const int AssignPaletteLockKeyColors = 144;

	public const int ApplyLevelSaveState = 145;

	public const int ApplyLevelPartData = 146;

	public const int SpawnPaletteLockGaps = 147;

	public const int NotifyLevelLoaded = 200;

	public const int EnterPhaseMin = 210;

	public const int PreEnterPipeline = 210;

	public const int LevelStepLimitInit = 25;

	public const int StartMainLevelStartPipeline = 220;

	public const int ShowLevelObject = 230;

	public const int CollectLevelEnterObjects = 235;

	public const int PlayLevelEnterAnimation = 240;

	public const int LevelEnterCompletedEvent = 250;

	public const int StartSceneEmbeddedWeakGuide = 255;

	public const int EnableLevelGameplayInput = 260;

	public const int ReportGameStartTrack = 270;

	public const int InitInterlockWhitelist = 280;

	public const int FinalizeLevelEnterPost = 290;

	public const int ReplayEntranceAnimation = 910;
}
