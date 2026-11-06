namespace RotateRings.GameWinIQ;

public sealed class GameWinIQService
{
	private static readonly GameWinIQService instance = new GameWinIQService();

	private int lastCampaignLevel = -1;

	public static GameWinIQService Instance => instance;

	public void ApplyCrossDayRulesIfNeeded()
	{
		GetProxy()?.ApplyCrossDayRulesIfNeeded();
	}

	public void SyncAudioExperiment()
	{
		GetProxy()?.SyncGameWinIQAudioExperiment();
	}

	public void OnLevelLoaded(int level)
	{
		if (!IsEnabled())
		{
			return;
		}
		GameWinIQProxy proxy = GetProxy();
		if (proxy != null)
		{
			if (level != lastCampaignLevel)
			{
				lastCampaignLevel = level;
				proxy.OnEnterLevelCampaign(level);
			}
			else
			{
				proxy.OnReplayLevelLoaded();
			}
		}
	}

	public void RecordLevelRestart(int currentLevel)
	{
		GetProxy()?.RecordLevelRestart(currentLevel);
	}

	public void ClearLevelCampaignCache()
	{
		lastCampaignLevel = -1;
	}

	public GameWinIQSettlementSnapshot RecordOnLevelWin(int completedLevel, int restartCount)
	{
		GameWinIQProxy proxy = GetProxy();
		if (proxy == null)
		{
			return GameWinIQSettlementSnapshot.Empty;
		}
		int propUseCount = GameWinIQProxy.CountCurrentLevelPropUses();
		GameWinIQSettlementSnapshot result = proxy.RecordOnLevelWin(completedLevel, propUseCount, restartCount);
		ClearLevelCampaignCache();
		return result;
	}

	public GameWinIQSettlementSnapshot GetLastSettlementSnapshot()
	{
		return GetProxy()?.GetLastSettlementSnapshot() ?? GameWinIQSettlementSnapshot.Empty;
	}

	public string GetTrackBaseAndDisplayScore()
	{
		return GetProxy()?.GetTrackBaseAndDisplayScore() ?? string.Empty;
	}

	public static bool IsEnabled()
	{
		return GameWinIQProxy.IsExperimentEnabled();
	}

	private static GameWinIQProxy GetProxy()
	{
		return GameApp.Proxy.Get<GameWinIQProxy>();
	}
}
