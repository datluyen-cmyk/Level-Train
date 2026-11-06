using RotateRings.Common;

namespace RotateRings.Ad.CommerceLtv;

public static class CommerceLtvExperiment
{
	public const int MaxGroup = 3;

	public static int GetGroup()
	{
		return FeatureConfig.InterstitialFunction?.CommerceLTVRatio ?? 0;
	}

	public static bool IsActive()
	{
		return IsValidGroup(GetGroup());
	}

	public static bool IsValidGroup(int group)
	{
		if (group >= 1)
		{
			return group <= 3;
		}
		return false;
	}
}
