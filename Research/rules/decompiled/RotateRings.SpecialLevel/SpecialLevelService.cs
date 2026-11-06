using Hot.Model;
using RotateRings.GamePlay.Save;
using RotateRings.Model;

namespace RotateRings.SpecialLevel;

public abstract class SpecialLevelService
{
	protected const int SpecialLevelNumber = 1;

	private bool hasPendingInsert;

	private int pendingActivityDayIndex;

	private int pendingBaseGameMode;

	private int preSwitchedBaseGameMode = -1;

	protected abstract GameMode SpecialGameMode { get; }

	protected abstract string LogTag { get; }

	public abstract int BannerStyleId { get; }

	public abstract string DisplayName { get; }

	public abstract string BannerLangKey { get; }

	protected virtual bool IgnoreInsertLimits => false;

	public bool IsModeMatched(GameMode gameMode)
	{
		return gameMode == SpecialGameMode;
	}

	protected virtual bool IsSupportedBaseMode(GameMode gameMode)
	{
		return gameMode == GameMode.Main;
	}

	public abstract bool IsActivityAvailable();

	public abstract int ResolveActivityDayIndex();

	public abstract int ResolveLevelResourceId(int activityDayIndex);

	public int ResolveEnterLevel(int requestedLevel)
	{
		int result = requestedLevel;
		hasPendingInsert = false;
		SpecialLevelProxy proxy = GetProxy();
		if (proxy == null)
		{
			return result;
		}
		bool flag = TryReconcileStaleSession(proxy);
		if (flag)
		{
			result = GameApp.Proxy.Get<LevelModelProxy>()?.Level ?? requestedLevel;
		}
		if ((!IgnoreInsertLimits || IsSpecialLevelMode()) && proxy.HasActiveSession && !proxy.Data.IsLevelCompleted)
		{
			EnterSpecialMode();
			return 1;
		}
		if (IgnoreInsertLimits && proxy.HasActiveSession)
		{
			proxy.EndSession();
		}
		bool flag2 = !proxy.HasActiveSession && IsSpecialLevelMode();
		if (!IsInsertAvailable(proxy) || (!flag2 && !CanInsertAsNewGame(requestedLevel, flag)))
		{
			return result;
		}
		ClearSpecialLevelResidualSave();
		pendingActivityDayIndex = ResolveActivityDayIndex();
		pendingBaseGameMode = ResolvePendingBaseGameMode(proxy);
		hasPendingInsert = true;
		EnterSpecialMode();
		return 1;
	}

	private int ResolvePendingBaseGameMode(SpecialLevelProxy proxy)
	{
		GameMode currentGameMode = GameInfo.CurrentGameMode;
		if (!SpecialLevelModule.IsSpecialLevelModeFor(currentGameMode))
		{
			return (int)currentGameMode;
		}
		if (preSwitchedBaseGameMode >= 0)
		{
			return preSwitchedBaseGameMode;
		}
		GameMode baseGameMode = (GameMode)proxy.Data.BaseGameMode;
		if (!SpecialLevelModule.IsSpecialLevelModeFor(baseGameMode))
		{
			return (int)baseGameMode;
		}
		return 0;
	}

	public void CommitInsertOnLevelLoaded()
	{
		if (hasPendingInsert)
		{
			hasPendingInsert = false;
			preSwitchedBaseGameMode = -1;
			GetProxy()?.BeginSession(pendingActivityDayIndex, pendingBaseGameMode);
			OnLevelInsertCommitted(pendingActivityDayIndex);
		}
	}

	protected virtual void OnLevelInsertCommitted(int activityDayIndex)
	{
	}

	public bool IsSpecialLevelMode()
	{
		return GameInfo.CurrentGameMode == SpecialGameMode;
	}

	public bool TryApplyActiveSessionGameMode(SpecialLevelProxy proxy)
	{
		if (proxy == null || !proxy.HasActiveSession || proxy.Data.IsLevelCompleted || !ShouldRestoreActiveSession(proxy))
		{
			return false;
		}
		GameInfo.CurrentGameMode = SpecialGameMode;
		return true;
	}

	protected virtual bool ShouldRestoreActiveSession(SpecialLevelProxy proxy)
	{
		return true;
	}

	public void RestoreBaseMode(bool endSession)
	{
		hasPendingInsert = false;
		SpecialLevelProxy proxy = GetProxy();
		if (proxy != null)
		{
			int baseGameMode = proxy.Data.BaseGameMode;
			if (endSession)
			{
				proxy.EndSession();
				ClearSpecialLevelResidualSave();
			}
			GameInfo.CurrentGameMode = (GameMode)baseGameMode;
			GameApp.Proxy.Get<LevelModelProxy>()?.SyncGameInfo();
		}
	}

	public void MarkCompleted()
	{
		GetProxy()?.MarkSessionCompleted();
	}

	public void DropExpiredSessionOnStartup()
	{
		SpecialLevelProxy proxy = GetProxy();
		if (proxy != null)
		{
			bool num = proxy.Data.ActivityDayIndex > 0;
			proxy.DropExpiredSession();
			if (num && proxy.Data.ActivityDayIndex == 0)
			{
				ClearSpecialLevelResidualSave();
			}
		}
	}

	protected bool CanInsertAsNewGame(int requestedLevel, bool isCrossDayReset)
	{
		if (isCrossDayReset)
		{
			return true;
		}
		if (IgnoreInsertLimits)
		{
			return true;
		}
		if (HasLevelSave(requestedLevel))
		{
			return false;
		}
		LevelModelProxy levelModelProxy = GameApp.Proxy.Get<LevelModelProxy>();
		if (levelModelProxy != null)
		{
			return levelModelProxy.TryLevelNum <= 0;
		}
		return false;
	}

	protected bool HasLevelSave(int level)
	{
		return GameApp.Proxy.Get<RingLevelSaveProxy>()?.HasValidSave(level) ?? false;
	}

	private bool IsInsertAvailable(SpecialLevelProxy proxy)
	{
		if (!IgnoreInsertLimits && proxy.HasInsertedToday)
		{
			return false;
		}
		GameMode currentGameMode = GameInfo.CurrentGameMode;
		if (!IsSupportedBaseMode(currentGameMode) && !IsSpecialLevelMode() && !IgnoreInsertLimits)
		{
			return false;
		}
		if (!IsActivityAvailable())
		{
			return false;
		}
		int activityDayIndex = ResolveActivityDayIndex();
		if (ResolveLevelResourceId(activityDayIndex) <= 0)
		{
			if (IgnoreInsertLimits)
			{
				return true;
			}
			return false;
		}
		return true;
	}

	private bool TryReconcileStaleSession(SpecialLevelProxy proxy)
	{
		if (proxy.Data.ActivityDayIndex <= 0 || proxy.HasActiveSession)
		{
			return false;
		}
		proxy.DropExpiredSession();
		ClearSpecialLevelResidualSave();
		GameInfo.CurrentGameMode = (GameMode)proxy.Data.BaseGameMode;
		return true;
	}

	public bool TryApplyEligibleInsertGameMode(int requestedBaseLevel)
	{
		if (IsSpecialLevelMode())
		{
			return false;
		}
		SpecialLevelProxy proxy = GetProxy();
		if (proxy == null)
		{
			return false;
		}
		if (!IgnoreInsertLimits && proxy.HasActiveSession && !proxy.Data.IsLevelCompleted)
		{
			return false;
		}
		if (!IsInsertAvailable(proxy) || !CanInsertAsNewGame(requestedBaseLevel, isCrossDayReset: false))
		{
			return false;
		}
		preSwitchedBaseGameMode = (int)GameInfo.CurrentGameMode;
		EnterSpecialMode();
		return true;
	}

	private void EnterSpecialMode()
	{
		GameInfo.CurrentGameMode = SpecialGameMode;
	}

	private void ClearSpecialLevelResidualSave()
	{
		RingLevelSaveProxy ringLevelSaveProxy = GameApp.Proxy.Get<RingLevelSaveProxy>();
		if (ringLevelSaveProxy?.Data != null && (!ringLevelSaveProxy.Data.HasData || ringLevelSaveProxy.Data.LevelNumber == 1))
		{
			ringLevelSaveProxy.ClearSave(writeToDisk: false);
		}
	}

	protected SpecialLevelProxy GetProxy()
	{
		return GameApp.Proxy.Get<SpecialLevelProxy>();
	}
}
