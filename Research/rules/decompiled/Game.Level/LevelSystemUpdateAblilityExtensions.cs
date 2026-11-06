using System;
using System.Collections.Generic;
using XingYun.Level;

namespace Game.Level;

public static class LevelSystemUpdateAblilityExtensions
{
	public static void Initialize()
	{
		LevelSystem.GetConfig().AbilityStrategy = (IAbilityUpdateStrategy)(object)new CustomAbilityUpdateStrategy();
	}

	public static AbilityUpdateResult UpdateAbility(int levelIndex, int step)
	{
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Expected O, but got Unknown
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bd: Expected O, but got Unknown
		if (!LevelSystem.IsInitialized)
		{
			throw new InvalidOperationException("系统未初始化，请先调用 Initialize()");
		}
		GameLevelProgress gameLevelProgress = GameApp.Proxy.Get<GameLevelProxy>().GetGameLevelProgress();
		LevelContext val = new LevelContext(gameLevelProgress.GlobalAbility, levelIndex)
		{
			AbilityConfig = LevelSystem.GetConfig().AbilityConfig,
			SamplerConfig = LevelSystem.GetConfig().SamplerConfig
		};
		val.Data["step"] = step;
		LevelSystem.GetConfig().AbilityStrategy.Update(val);
		LevelSystem.ClearTemporaryAbilityOverrides();
		return new AbilityUpdateResult
		{
			IntervalId = gameLevelProgress.CurrentIntervalId,
			NewAbility = val.PlayerAbility,
			LevelIndex = levelIndex,
			FailPopupCount = gameLevelProgress.FailPopupCount,
			DebugData = new Dictionary<string, object>(val.Data)
		};
	}
}
