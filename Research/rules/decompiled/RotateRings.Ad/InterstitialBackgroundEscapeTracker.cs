using RotateRings.Model;

namespace RotateRings.Ad;

public static class InterstitialBackgroundEscapeTracker
{
	private static InterstitialBackgroundEscapeRecord GetBackgroundRecord()
	{
		return GameApp.Save.GetOrCreate<InterstitialBackgroundEscapeRecord>();
	}

	private static InterstitialEscapeRecord GetEscapeRecord()
	{
		return GameApp.Save.GetOrCreate<InterstitialEscapeRecord>();
	}

	public static void NotifyAdStarted()
	{
		GetBackgroundRecord().ClosePending = true;
	}

	public static void NotifyAdClosed()
	{
		GetBackgroundRecord().ClosePending = false;
	}

	public static void CheckFirstInterstitialLevelAfterEscape()
	{
		if (AdService.IsBackgroundEscapeLevelSkipEnabled())
		{
			InterstitialBackgroundEscapeRecord backgroundRecord = GetBackgroundRecord();
			if (backgroundRecord.ClosePending)
			{
				InterstitialEscapeRecord escapeRecord = GetEscapeRecord();
				int num = GameApp.Proxy.Get<LevelModelProxy>()?.Level ?? 1;
				escapeRecord.FirstInterstitialLevelAfterEscape = num + 3;
				backgroundRecord.ClosePending = false;
				backgroundRecord.LastEscapeDurationSeconds = 0f;
			}
		}
	}

	public static void ClearState()
	{
		InterstitialBackgroundEscapeRecord backgroundRecord = GetBackgroundRecord();
		backgroundRecord.ClosePending = false;
		backgroundRecord.LastEscapeDurationSeconds = 0f;
	}
}
