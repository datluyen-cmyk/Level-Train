namespace RotateRings.Services;

public static class BgmExperimentDefaults
{
	public static bool IsDefaultMusicOnForStyle(int bgmStyleId)
	{
		return bgmStyleId != 0;
	}

	public static void ApplyStartupMusicDefaultIfNeeded()
	{
	}
}
