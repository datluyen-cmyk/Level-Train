using System;
using Example.Cfg;
using XingYun.Experiment;

namespace RotateRings.RestartRetention;

public static class RestartRetentionConfigResolver
{
	public static RestartRetentionConfig Resolve()
	{
		RestartRetentionFunction restartRetentionFunction = FunctionDataCenter.Get<RestartRetentionFunction>();
		if (restartRetentionFunction != null)
		{
			return LoadScheme(restartRetentionFunction.GroupId);
		}
		return LoadScheme(0);
	}

	public static RestartRetentionConfig LoadScheme(int groupId)
	{
		groupId = NormalizeGroup(groupId);
		try
		{
			TbRestartRetentionConfig tbRestartRetentionConfig = GameApp.Config.GetTable<ExampleTables>()?.TbRestartRetentionConfig;
			Example.Cfg.RestartRetentionConfig restartRetentionConfig = tbRestartRetentionConfig?.GetOrDefault(groupId);
			if (restartRetentionConfig != null)
			{
				RestartRetentionConfig restartRetentionConfig2 = ToSnapshot(restartRetentionConfig);
				if (IsValid(restartRetentionConfig2))
				{
					return restartRetentionConfig2;
				}
			}
			Example.Cfg.RestartRetentionConfig restartRetentionConfig3 = tbRestartRetentionConfig?.GetOrDefault(0);
			if (restartRetentionConfig3 != null)
			{
				RestartRetentionConfig restartRetentionConfig4 = ToSnapshot(restartRetentionConfig3);
				if (IsValid(restartRetentionConfig4))
				{
					return restartRetentionConfig4;
				}
			}
		}
		catch (Exception)
		{
		}
		return SafeBottom();
	}

	private static RestartRetentionConfig ToSnapshot(Example.Cfg.RestartRetentionConfig row)
	{
		return new RestartRetentionConfig
		{
			GroupId = row.Id,
			StreakMin = row.StreakMin,
			RestartMax = row.RestartMax,
			LowStreakContent = (RestartRetentionContent)row.LowStreakContent,
			HighStreakContent = (RestartRetentionContent)row.HighStreakContent,
			LowUseBaseArt = row.LowUseBaseArt,
			HighUseBaseArt = row.HighUseBaseArt,
			RankInactiveContent = (RestartRetentionContent)row.RankInactiveContent,
			RankInactiveUseBaseArt = row.RankInactiveUseBaseArt,
			CancelTarget = (RestartRetentionCancelTarget)row.CancelTarget
		};
	}

	private static bool IsValid(RestartRetentionConfig config)
	{
		if (config.GroupId >= 0 && ((config.LowStreakContent == RestartRetentionContent.None && config.HighStreakContent == RestartRetentionContent.None) || (config.RestartMax >= 0 && config.RestartMax < int.MaxValue && config.StreakMin == config.RestartMax + 1)) && (uint)config.LowStreakContent <= 3u && (uint)config.HighStreakContent <= 3u && (uint)config.RankInactiveContent <= 3u)
		{
			return (uint)config.CancelTarget <= 1u;
		}
		return false;
	}

	private static RestartRetentionConfig SafeBottom()
	{
		return new RestartRetentionConfig
		{
			GroupId = 0,
			StreakMin = 2,
			RestartMax = 1,
			LowStreakContent = RestartRetentionContent.None,
			HighStreakContent = RestartRetentionContent.None,
			LowUseBaseArt = true,
			HighUseBaseArt = true,
			RankInactiveContent = RestartRetentionContent.None,
			RankInactiveUseBaseArt = true,
			CancelTarget = RestartRetentionCancelTarget.Gameplay
		};
	}

	public static RestartRetentionConfig Validate(RestartRetentionConfig config)
	{
		if (!IsValid(config))
		{
			return LoadScheme(0);
		}
		return config;
	}

	public static int NormalizeGroup(int groupId)
	{
		if (groupId < 0)
		{
			return 0;
		}
		return groupId;
	}

	public static int ParseGroup(string value)
	{
		if (!int.TryParse(value, out var result))
		{
			return 0;
		}
		return NormalizeGroup(result);
	}
}
