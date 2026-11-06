namespace RotateRings.UI.GameEnterToast.Core;

public static class LevelEntryBannerLastShownStore
{
	private const string SlotKey = "LevelEntryBanner_LastShown";

	public static string GetLastShownKey(BannerDifficultyGroup group)
	{
		return Load().KeyOf(group);
	}

	public static void SetLastShownKey(BannerDifficultyGroup group, string key)
	{
		Load().SetKeyOf(group, key);
	}

	private static LevelEntryBannerLastShownData Load()
	{
		return GameApp.Save.GetOrCreate<LevelEntryBannerLastShownData>("LevelEntryBanner_LastShown");
	}
}
