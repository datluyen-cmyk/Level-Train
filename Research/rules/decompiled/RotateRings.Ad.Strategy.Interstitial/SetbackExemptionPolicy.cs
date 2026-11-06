using XingYun.Experiment;

namespace RotateRings.Ad.Strategy.Interstitial;

public sealed class SetbackExemptionPolicy : InterstitialPolicyBase
{
	private const string Tag = "<Ad>.SetbackExemption";

	private const int QuotaExpireLevelSpan = 3;

	private const int CooldownLevelSpan = 3;

	private static string lastWarnedRaw;

	public override string Name => "SetbackExemption";

	public override void OnInit()
	{
		SetbackExemptionConfig.TryParse(ReadConfiguredValue(), out var _);
	}

	public static bool TryGetActiveConfig(out SetbackExemptionConfig config)
	{
		string text = ReadConfiguredValue();
		if (SetbackExemptionConfig.TryParse(text, out config))
		{
			lastWarnedRaw = null;
			return true;
		}
		string text2 = text?.Trim();
		if (!string.IsNullOrEmpty(text2) && text2 != SetbackExemptionConfig.Disabled.ToString() && lastWarnedRaw != text2)
		{
			lastWarnedRaw = text2;
		}
		return false;
	}

	private static string ReadConfiguredValue()
	{
		return FunctionDataCenter.Get<InterstitialFunction>()?.SetbackExemption ?? string.Empty;
	}

	public override void OnRestartClicked(int level)
	{
		if (!TryGetActiveConfig(out var config))
		{
			ClearState("实验未开启或配置非法");
			return;
		}
		SetbackExemptionRecord record = GetRecord();
		SyncConfig(record, config);
		SyncLevel(record, level);
		record.RestartCount++;
		TryGrantQuota(record, config, level, "主动重开");
	}

	public override void OnMidGameExit(int level)
	{
		if (!TryGetActiveConfig(out var config))
		{
			ClearState("实验未开启或配置非法");
		}
		else if (config.TrackExit)
		{
			SetbackExemptionRecord record = GetRecord();
			SyncConfig(record, config);
			SyncLevel(record, level);
			record.ExitCount++;
			TryGrantQuota(record, config, level, "中途退出");
		}
	}

	public override InterAdCheckResult Evaluate(InterstitialPolicyContext context)
	{
		if (!TryGetActiveConfig(out var config))
		{
			ClearState("实验未开启或配置非法");
			return InterAdCheckResult.Allow;
		}
		SetbackExemptionRecord record = GetRecord();
		SyncConfig(record, config);
		SyncLevel(record, context.Level);
		if (record.ExemptTimes <= 0)
		{
			return InterAdCheckResult.Allow;
		}
		if (record.ExemptBatchTotal > 0)
		{
			_ = record.ExemptBatchTotal;
		}
		else
			_ = record.ExemptTimes;
		_ = record.ExemptTimes;
		record.ExemptTimes--;
		if (record.ExemptTimes <= 0)
		{
			record.ExemptTimes = 0;
			record.ExemptBatchTotal = 0;
			record.QuotaExpireLevelKey = 0;
		}
		return InterAdCheckResult.Block("受挫豁免：本关连续重开/中途退出免插屏");
	}

	public override void ResetForDebug()
	{
		ResetStateForDebug();
	}

	public static void ResetStateForDebug()
	{
		SetbackExemptionRecord record = GetRecord();
		ClearCountersAndQuota(record);
		record.ActiveConfigRaw = string.Empty;
		record.LevelKey = 0;
	}

	public override string BuildDebugState()
	{
		return BuildStateSnapshot();
	}

	public static string BuildStateSnapshot()
	{
		SetbackExemptionConfig config;
		bool flag = SetbackExemptionConfig.TryParse(ReadConfiguredValue(), out config);
		return BuildStateSummary(GetRecord(), flag ? config : SetbackExemptionConfig.Disabled);
	}

	private static void TryGrantQuota(SetbackExemptionRecord record, SetbackExemptionConfig config, int level, string reason)
	{
		if ((record.CooldownUntilLevelKey <= 0 || level > record.CooldownUntilLevelKey) && record.ExemptTimes <= 0 && (record.RestartCount >= config.RestartThreshold || (config.TrackExit && record.ExitCount >= config.ExitThreshold)))
		{
			record.ExemptTimes = config.ExemptCount;
			record.ExemptBatchTotal = config.ExemptCount;
			record.QuotaExpireLevelKey = level + 3;
			record.CooldownUntilLevelKey = level + 3;
		}
	}

	private static void SyncConfig(SetbackExemptionRecord record, SetbackExemptionConfig config)
	{
		if (!(record.ActiveConfigRaw == config.Raw))
		{
			_ = record.ActiveConfigRaw;
			record.ActiveConfigRaw = config.Raw;
			ClearCountersAndQuota(record);
		}
	}

	private static void SyncLevel(SetbackExemptionRecord record, int level)
	{
		if (level > 0 && record.LevelKey != level)
		{
			_ = record.LevelKey;
			record.LevelKey = level;
			record.RestartCount = 0;
			record.ExitCount = 0;
			if (record.ExemptTimes > 0 && record.QuotaExpireLevelKey > 0 && level > record.QuotaExpireLevelKey)
			{
				record.ExemptTimes = 0;
				record.ExemptBatchTotal = 0;
				record.QuotaExpireLevelKey = 0;
			}
			if (record.CooldownUntilLevelKey > 0 && level > record.CooldownUntilLevelKey)
			{
				record.CooldownUntilLevelKey = 0;
			}
		}
	}

	private static void ClearState(string reason)
	{
		SetbackExemptionRecord record = GetRecord();
		if (record.ExemptTimes > 0 || record.RestartCount > 0 || record.ExitCount > 0 || record.CooldownUntilLevelKey > 0 || !string.IsNullOrEmpty(record.ActiveConfigRaw))
		{
			ClearCountersAndQuota(record);
			record.ActiveConfigRaw = string.Empty;
		}
	}

	private static void ClearCountersAndQuota(SetbackExemptionRecord record)
	{
		record.RestartCount = 0;
		record.ExitCount = 0;
		record.ExemptTimes = 0;
		record.ExemptBatchTotal = 0;
		record.QuotaExpireLevelKey = 0;
		record.CooldownUntilLevelKey = 0;
	}

	private static string BuildStateSummary(SetbackExemptionRecord record, SetbackExemptionConfig config)
	{
		bool flag = !string.IsNullOrEmpty(config.Raw);
		string text = (config.TrackExit ? (record.ExitCount + "次") : "不统计");
		string text2 = ((record.ExemptTimes > 0) ? (record.ExemptTimes + "次（本批共" + record.ExemptBatchTotal + "次）") : "0次");
		string text3 = ((record.QuotaExpireLevelKey > 0) ? ("第" + record.QuotaExpireLevelKey + "关") : "无");
		string text4 = ((record.CooldownUntilLevelKey > 0) ? ("至第" + record.CooldownUntilLevelKey + "关（第" + (record.CooldownUntilLevelKey + 1) + "关起恢复）") : "无");
		return string.Format("实验={0} 配置={1} 当前关卡={2} 本关重开={3}次 本关退出={4} 剩余豁免={5} 额度有效期至{6} 冷却={7}", flag ? "开启" : "关闭", flag ? config.Raw : "未开启", record.LevelKey, record.RestartCount, text, text2, text3, text4);
	}

	private static SetbackExemptionRecord GetRecord()
	{
		return GameApp.Save.GetOrCreate<SetbackExemptionRecord>();
	}
}
