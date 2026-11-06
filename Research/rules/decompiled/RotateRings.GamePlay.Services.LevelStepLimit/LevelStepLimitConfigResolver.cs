using System;
using Example.Cfg;
using XingYun.Experiment;

namespace RotateRings.GamePlay.Services.LevelStepLimit;

public static class LevelStepLimitConfigResolver
{
	public static LevelStepLimitConfigSnapshot Resolve(int levelKey)
	{
		LevelStepLimitFunction levelStepLimitFunction = FunctionDataCenter.Get<LevelStepLimitFunction>();
		if (levelStepLimitFunction != null)
		{
			return LoadScheme(levelStepLimitFunction.SchemaId, levelKey);
		}
		return LoadScheme(0, levelKey);
	}

	public static LevelStepLimitConfigSnapshot LoadScheme(int groupId, int levelKey)
	{
		if (groupId < 0)
		{
			groupId = 0;
		}
		try
		{
			ExampleTables table = GameApp.Config.GetTable<ExampleTables>();
			LevelStepLimitSchema levelStepLimitSchema = table?.TbLevelStepLimitSchema.GetOrDefault(groupId);
			if (levelStepLimitSchema == null)
			{
				return SafeBottom();
			}
			LevelStepLimitConfig levelStepLimitConfig = table?.TbLevelStepLimitConfig.GetOrDefault(levelKey);
			if (levelStepLimitConfig == null)
			{
				return SafeBottom();
			}
			LevelStepLimitConfigSnapshot levelStepLimitConfigSnapshot = ToSnapshot(levelStepLimitSchema, levelStepLimitConfig);
			if (IsValid(levelStepLimitConfigSnapshot))
			{
				return levelStepLimitConfigSnapshot;
			}
		}
		catch (Exception)
		{
		}
		return SafeBottom();
	}

	private static LevelStepLimitConfigSnapshot ToSnapshot(LevelStepLimitSchema schema, LevelStepLimitConfig row)
	{
		return new LevelStepLimitConfigSnapshot
		{
			GroupId = schema.Id,
			Enable = true,
			MaxSteps = ((schema.StepLimitType == 2) ? row.StrictStepCount : row.LooseStepCount),
			AddStepsPerAd = schema.AddStepsPerAd,
			MaxAdWatchCount = -1,
			LevelMode = schema.LevelType switch
			{
				1 => LevelStepLimitLevelMode.Main, 
				2 => LevelStepLimitLevelMode.Endless, 
				100 => LevelStepLimitLevelMode.All, 
				200 => LevelStepLimitLevelMode.HardLevel, 
				_ => (LevelStepLimitLevelMode)0, 
			},
			AngleThreshold = 1f,
			DistanceThreshold = 5f
		};
	}

	private static bool IsValid(LevelStepLimitConfigSnapshot config)
	{
		if (config.GroupId >= 0 && config.MaxSteps >= 0 && config.AddStepsPerAd >= 0)
		{
			return config.LevelMode != (LevelStepLimitLevelMode)0;
		}
		return false;
	}

	public static LevelStepLimitConfigSnapshot SafeBottom()
	{
		return new LevelStepLimitConfigSnapshot
		{
			GroupId = 0,
			Enable = false,
			MaxSteps = 0,
			AddStepsPerAd = 0,
			MaxAdWatchCount = 0,
			LevelMode = LevelStepLimitLevelMode.All,
			AngleThreshold = 1f,
			DistanceThreshold = 5f
		};
	}
}
