using RotateRings.Model;

namespace RotateRings.Ad;

internal static class AdColdLaunchTracker
{
	private static bool countedThisSession;

	public static void Reset()
	{
		countedThisSession = false;
	}

	public static int GetCountForDebug()
	{
		return (GameApp.Proxy.Get<PlayerModelProxy>()?.Data?.ColdLaunchCount).GetValueOrDefault();
	}

	public static void RecordForSession()
	{
		if (!countedThisSession)
		{
			countedThisSession = true;
			PlayerModelProxy playerModelProxy = GameApp.Proxy.Get<PlayerModelProxy>();
			if (playerModelProxy?.Data != null)
			{
				playerModelProxy.Data.EnsureRecentColdStartLevelHistoryMigrated();
				playerModelProxy.Data.PushColdStartSession();
				playerModelProxy.Data.ColdLaunchCount++;
			}
		}
	}
}
