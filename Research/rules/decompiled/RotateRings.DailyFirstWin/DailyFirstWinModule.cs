namespace RotateRings.DailyFirstWin;

public static class DailyFirstWinModule
{
	private const string TAG = "[DailyFirstWin] ";

	public static IDailyFirstWinHost Host { get; private set; }

	public static IDailyFirstWinPresentationHost UIHost { get; private set; }

	public static bool IsInitialized
	{
		get
		{
			if (Host != null)
			{
				return UIHost != null;
			}
			return false;
		}
	}

	public static void Initialize(IDailyFirstWinHost host, IDailyFirstWinPresentationHost uiHost)
	{
		Host = host;
		UIHost = uiHost;
	}

	public static void CompleteStartup(DailyFirstWinProxy proxy)
	{
		proxy?.TryApplyEntryPlayModeMigrationOnce();
		proxy?.PreparePendingCyclePuzzleOnStartup();
		proxy?.SyncPuzzleAudioExperiment();
	}

	public static void Shutdown()
	{
		Host = null;
		UIHost = null;
	}
}
