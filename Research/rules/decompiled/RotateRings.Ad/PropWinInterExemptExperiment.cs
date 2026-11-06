using RotateRings.Common;
using RotateRings.Model;

namespace RotateRings.Ad;

public static class PropWinInterExemptExperiment
{
	private const string Tag = "<Ad>[PropWinInterExempt] ";

	private const int SkipPostGroup = 1;

	private const int SkipPostAndNextFrontGroup = 2;

	public static void OnPropUsed()
	{
		int num = GetGroup();
		if (IsValidGroup(num))
		{
			LevelModelProxy levelModelProxy = GameApp.Proxy.Get<LevelModelProxy>();
			if (levelModelProxy != null)
			{
				PropWinInterExemptRecord record = GetRecord();
				ClearInactiveState(record, num);
				record.PendingPropWinGroup = num;
				record.BoundLevel = levelModelProxy.Level;
				record.BoundModeNum = levelModelProxy.ModeSessionNum;
			}
		}
	}

	public static void OnLevelCompleted()
	{
		int num = GetGroup();
		if (num == 2)
		{
			PropWinInterExemptRecord record = GetRecord();
			ClearInactiveState(record, num);
			if (IsPendingSessionValid(record) && record.PendingPropWinGroup == num)
			{
				record.SkipNextFrontAdGroup = num;
			}
		}
	}

	public static bool TryConsumeInterstitialExemption(bool isFrontAd, out string blockReason)
	{
		blockReason = null;
		int num = GetGroup();
		if (!IsValidGroup(num))
		{
			return false;
		}
		PropWinInterExemptRecord record = GetRecord();
		ClearInactiveState(record, num);
		if (!isFrontAd && record.PendingPropWinGroup == num)
		{
			if (!IsPendingSessionValid(record))
			{
				ClearPendingPostState(record);
				return false;
			}
			record.PendingPropWinGroup = 0;
			record.BoundLevel = 0;
			record.BoundModeNum = 0;
			if (num == 2)
			{
				record.SkipNextFrontAdGroup = num;
			}
			blockReason = "使用道具通关后免后插屏";
			return true;
		}
		if (isFrontAd && num == 2 && record.SkipNextFrontAdGroup == num)
		{
			record.SkipNextFrontAdGroup = 0;
			blockReason = "使用道具通关后免下一局前插屏";
			return true;
		}
		return false;
	}

	public static string BuildStateSummary()
	{
		PropWinInterExemptRecord record = GetRecord();
		LevelModelProxy levelModelProxy = GameApp.Proxy.Get<LevelModelProxy>();
		return $"propWinGroup={GetGroup()} pending={record.PendingPropWinGroup} skipNextFront={record.SkipNextFrontAdGroup} bound=({record.BoundLevel},{record.BoundModeNum}) current=({levelModelProxy?.Level ?? (-1)},{levelModelProxy?.ModeSessionNum ?? (-1)})";
	}

	private static int GetGroup()
	{
		return FeatureConfig.InterstitialFunction?.PropWinInterExemptGroup ?? 0;
	}

	private static PropWinInterExemptRecord GetRecord()
	{
		return GameApp.Save.GetOrCreate<PropWinInterExemptRecord>();
	}

	private static bool IsValidGroup(int group)
	{
		if (group != 1)
		{
			return group == 2;
		}
		return true;
	}

	private static void ClearInactiveState(PropWinInterExemptRecord record, int group)
	{
		if (!IsValidGroup(group))
		{
			ClearAllState(record);
			return;
		}
		if (record.PendingPropWinGroup != 0 && record.PendingPropWinGroup != group)
		{
			ClearPendingPostState(record);
		}
		if (record.SkipNextFrontAdGroup != 0 && record.SkipNextFrontAdGroup != group)
		{
			record.SkipNextFrontAdGroup = 0;
		}
	}

	private static bool IsPendingSessionValid(PropWinInterExemptRecord record)
	{
		LevelModelProxy levelModelProxy = GameApp.Proxy.Get<LevelModelProxy>();
		if (levelModelProxy == null || record.PendingPropWinGroup == 0)
		{
			return false;
		}
		if (record.BoundModeNum != levelModelProxy.ModeSessionNum)
		{
			return false;
		}
		int level = levelModelProxy.Level;
		if (record.BoundLevel != level)
		{
			return record.BoundLevel == level - 1;
		}
		return true;
	}

	private static void ClearPendingPostState(PropWinInterExemptRecord record)
	{
		record.PendingPropWinGroup = 0;
		record.BoundLevel = 0;
		record.BoundModeNum = 0;
	}

	private static void ClearAllState(PropWinInterExemptRecord record)
	{
		ClearPendingPostState(record);
		record.SkipNextFrontAdGroup = 0;
	}
}
