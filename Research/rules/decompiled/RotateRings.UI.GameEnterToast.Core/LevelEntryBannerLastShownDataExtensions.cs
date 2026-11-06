namespace RotateRings.UI.GameEnterToast.Core;

internal static class LevelEntryBannerLastShownDataExtensions
{
	public static string KeyOf(this LevelEntryBannerLastShownData data, BannerDifficultyGroup group)
	{
		if (data == null)
		{
			return string.Empty;
		}
		return group switch
		{
			BannerDifficultyGroup.Simple => data.LastSimpleKey, 
			BannerDifficultyGroup.Hard => data.LastHardKey, 
			BannerDifficultyGroup.Shape => data.LastShapeKey, 
			_ => string.Empty, 
		};
	}

	public static void SetKeyOf(this LevelEntryBannerLastShownData data, BannerDifficultyGroup group, string key)
	{
		if (data != null)
		{
			string text = key ?? string.Empty;
			switch (group)
			{
			case BannerDifficultyGroup.Simple:
				data.LastSimpleKey = text;
				break;
			case BannerDifficultyGroup.Hard:
				data.LastHardKey = text;
				break;
			case BannerDifficultyGroup.Shape:
				data.LastShapeKey = text;
				break;
			}
		}
	}
}
