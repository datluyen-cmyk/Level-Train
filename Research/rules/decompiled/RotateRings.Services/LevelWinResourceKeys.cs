using RotateRings.Halloween;

namespace RotateRings.Services;

public static class LevelWinResourceKeys
{
	public const string LevelWinPrefab = "UILevelWin";

	public const string AnimalResGroup = "UILevelWin_Animal";

	public const string EffectResGroup = "UILevelWin_Effect";

	public const string SpinePrefab = "eff_spine_jieusuan_songshu_01";

	public const string HalloweenSpinePrefab = "eff_spine_fullscreen_songshu_gp4_5";

	public const string ProgressPrefabL = "PreStageRewardProgressL";

	public const string ProgressPrefabM = "PreStageRewardProgressM";

	public const string ProgressPrefabR = "PreStageRewardProgressR";

	public const string ProgressTrailPrefab = "eff_uiwin_trail_01";

	public const string SfxLevelCompleteClip = "rr_sfx_level_complete_2";

	public static readonly string[] VoCompleteSquirrelClips = new string[5] { "rr_vo_complete_Squirrel2", "rr_vo_complete_Squirrel", "rr_vo_complete_squirrel4", "rr_vo_complete_squirrel5", "rr_vo_complete_squirrel6" };

	public static string GetSpinePrefab()
	{
		if (!HalloweenFeatureService.IsSettlementEnabled())
		{
			return "eff_spine_jieusuan_songshu_01";
		}
		return "eff_spine_fullscreen_songshu_gp4_5";
	}
}
