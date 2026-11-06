using System;
using RotateRings.Common;

namespace RotateRings.Ad;

public static class RewardInterExemptExperiment
{
	private const string Tag = "<Ad>[RewardInterExempt] ";

	private const string MidRewardPositionPrefix = "局中激励";

	private const int MidRewardSkipPostGroup = 1;

	private const int MidRewardSkipPostAndNextFrontGroup = 2;

	private const int AnyRewardSkip70SecondsGroup = 3;

	private const int AnyRewardSkip100SecondsGroup = 4;

	public static void OnRewardAdFinished(string adPosition)
	{
		int num = GetGroup();
		if (IsValidGroup(num))
		{
			RewardInterExemptRecord record = GetRecord();
			ClearInactiveState(record, num);
			if (num == 3 || num == 4)
			{
				int durationSeconds = ((num == 3) ? 70 : 100);
				RestartTimedExemptionWindow(record, durationSeconds, num, adPosition);
			}
			else if (IsMidRewardPosition(adPosition))
			{
				record.PendingMidRewardGroup = num;
			}
		}
	}

	public static void OnLevelCompleted()
	{
		int num = GetGroup();
		if (num == 2)
		{
			RewardInterExemptRecord record = GetRecord();
			ClearInactiveState(record, num);
			if (record.PendingMidRewardGroup == num)
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
		RewardInterExemptRecord record = GetRecord();
		ClearInactiveState(record, num);
		if (num == 3 || num == 4)
		{
			if (record.ExemptionExpireTimestamp - GetUtcTimestamp() <= 0)
			{
				record.ExemptionExpireTimestamp = 0L;
				return false;
			}
			blockReason = "激励广告后免插屏时间窗生效";
			return true;
		}
		if (!isFrontAd && record.PendingMidRewardGroup == num)
		{
			record.PendingMidRewardGroup = 0;
			if (num == 2)
			{
				record.SkipNextFrontAdGroup = num;
			}
			blockReason = "局内激励通关后免后插屏";
			return true;
		}
		if (isFrontAd && num == 2 && record.SkipNextFrontAdGroup == num)
		{
			record.SkipNextFrontAdGroup = 0;
			blockReason = "局内激励通关后免下一局前插屏";
			return true;
		}
		return false;
	}

	private static int GetGroup()
	{
		return FeatureConfig.InterstitialFunction?.RewardInterExemptGroup ?? 0;
	}

	private static RewardInterExemptRecord GetRecord()
	{
		return GameApp.Save.GetOrCreate<RewardInterExemptRecord>();
	}

	private static bool IsValidGroup(int group)
	{
		if (group >= 1)
		{
			return group <= 4;
		}
		return false;
	}

	private static void ClearInactiveState(RewardInterExemptRecord record, int group)
	{
		if (group == 1 || group == 2)
		{
			record.ExemptionExpireTimestamp = 0L;
			if (record.PendingMidRewardGroup != group)
			{
				record.PendingMidRewardGroup = 0;
			}
			if (record.SkipNextFrontAdGroup != group)
			{
				record.SkipNextFrontAdGroup = 0;
			}
		}
		else
		{
			record.PendingMidRewardGroup = 0;
			record.SkipNextFrontAdGroup = 0;
		}
	}

	private static bool IsMidRewardPosition(string adPosition)
	{
		if (!string.IsNullOrEmpty(adPosition))
		{
			return adPosition.StartsWith("局中激励", StringComparison.Ordinal);
		}
		return false;
	}

	private static void RestartTimedExemptionWindow(RewardInterExemptRecord record, int durationSeconds, int group, string adPosition)
	{
		long utcTimestamp = GetUtcTimestamp();
		long exemptionExpireTimestamp = record.ExemptionExpireTimestamp;
		record.ExemptionExpireTimestamp = utcTimestamp + durationSeconds;
	}

	private static long GetUtcTimestamp()
	{
		return DateTimeOffset.UtcNow.ToUnixTimeSeconds();
	}
}
