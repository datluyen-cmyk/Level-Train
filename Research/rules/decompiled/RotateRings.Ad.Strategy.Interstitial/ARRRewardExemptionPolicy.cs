using System;

namespace RotateRings.Ad.Strategy.Interstitial;

public sealed class ARRRewardExemptionPolicy : InterstitialPolicyBase
{
	private const string Tag = "<Ad>.AccelerateRevenueRecovery";

	public override string Name => "AccelerateRewardExemption";

	public override void OnRewardAdFinished(RewardAdFinishedContext context)
	{
		if (!context.IsSuccess || !context.IsFinished)
		{
			return;
		}
		int num = ARRExperiment.GetGroup();
		if (!ARRExperiment.IsTakeOverGroup(num))
		{
			return;
		}
		ARRRecord record = GetRecord();
		switch (num)
		{
		case 1:
		{
			int level = context.Level;
			if (level > 0)
			{
				record.RewardWatchedLevelKey = level;
			}
			break;
		}
		case 4:
		{
			int rewardExemptSeconds = ARRExperiment.GetRewardExemptSeconds(AdBridgeHelper.GetInstallDays());
			record.RewardExemptExpireTimestamp = GetUtcTimestamp() + rewardExemptSeconds;
			break;
		}
		}
	}

	public override InterAdCheckResult Evaluate(InterstitialPolicyContext context)
	{
		int num = ARRExperiment.GetGroup();
		if (!ARRExperiment.IsTakeOverGroup(num) || context.IsFrontAd)
		{
			return InterAdCheckResult.Allow;
		}
		ARRRecord record = GetRecord();
		switch (num)
		{
		case 1:
		{
			int num2 = context.Level - 1;
			if (record.RewardWatchedLevelKey > 0 && record.RewardWatchedLevelKey == num2)
			{
				record.RewardWatchedLevelKey = 0;
				return InterAdCheckResult.Block("ARR策略激励后免后插屏");
			}
			return InterAdCheckResult.Allow;
		}
		case 4:
			if (record.RewardExemptExpireTimestamp - GetUtcTimestamp() > 0)
			{
				return InterAdCheckResult.Block("ARR策略激励后免后插屏");
			}
			return InterAdCheckResult.Allow;
		default:
			return InterAdCheckResult.Allow;
		}
	}

	public override void OnRestartClicked(int level)
	{
		ARRRecord record = GetRecord();
		if (record.RewardWatchedLevelKey > 0)
		{
			record.RewardWatchedLevelKey = 0;
		}
	}

	public override void ResetForDebug()
	{
		ResetStateForDebug();
	}

	public static void ResetStateForDebug()
	{
		ARRRecord record = GetRecord();
		record.RewardWatchedLevelKey = 0;
		record.RewardExemptExpireTimestamp = 0L;
	}

	public override string BuildDebugState()
	{
		return BuildStateSnapshot();
	}

	public static string BuildStateSnapshot()
	{
		ARRRecord record = GetRecord();
		long num = record.RewardExemptExpireTimestamp - GetUtcTimestamp();
		return $"组={ARRExperiment.GetGroup()} 本关激励关卡={record.RewardWatchedLevelKey} " + $"时间窗剩余={((num > 0) ? num : 0)}s";
	}

	private static ARRRecord GetRecord()
	{
		return GameApp.Save.GetOrCreate<ARRRecord>();
	}

	private static long GetUtcTimestamp()
	{
		return DateTimeOffset.UtcNow.ToUnixTimeSeconds();
	}
}
