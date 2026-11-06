namespace RotateRings.Ad;

internal static class MainStageGameTimeGate
{
	public static bool IsMainLevelSessionTracking { get; private set; }

	public static bool IsValidDurationGameMode(string gameMode)
	{
		if (!(gameMode == 0.ToString()))
		{
			return gameMode == 2.ToString();
		}
		return true;
	}

	public static void SetMainLevelSessionTracking(bool countsTowardValidDuration, bool tracking)
	{
		IsMainLevelSessionTracking = countsTowardValidDuration && tracking;
	}
}
