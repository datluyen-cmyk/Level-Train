using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UniLog;
using UnityEngine;
using XingYun.Level;

namespace Game.Level;

public class CustomAbilityUpdateStrategy : IAbilityUpdateStrategy
{
	public void Update(LevelContext ctx)
	{
		GameLevelProgress gameLevelProgress = GameApp.Proxy.Get<GameLevelProxy>().GetGameLevelProgress();
		string levelIntervalId = ctx.LevelIntervalId;
		EnsureIntervalExists(gameLevelProgress, levelIntervalId, ctx);
		IntervalData val = gameLevelProgress.Intervals[levelIntervalId];
		float num = Mathf.Max((float)val.Ability, 0.001f);
		float num2 = (float)val.Uncertainty;
		float num3 = Mathf.Max(gameLevelProgress.CurrentLevelDifficulty, 0.001f);
		float timeFactorM = ctx.AbilityConfig.GetTimeFactorM();
		float learningRateK = ctx.AbilityConfig.GetLearningRateK();
		AbilityUpdateConfig abilityConfig = ctx.AbilityConfig;
		float levelPassTime = ctx.LevelPassTime;
		float num4 = num3 * (num3 / num) * (1f / timeFactorM);
		levelPassTime = Mathf.Max(levelPassTime, 0.001f);
		float num5 = num4 / levelPassTime;
		float num6 = (float)Math.Log(num5);
		float num7 = Mathf.Min((float)Math.Log(Mathf.Max(ctx.LevelIndex - 1, 0) + 1, abilityConfig.LogBase), 10f);
		float num8 = num2 / abilityConfig.MaxUncertainty;
		float num9 = learningRateK * num6 * num8 * num7 / 10f;
		float num10 = (float)val.MaxDifficulty;
		float num11 = (float)val.MinDifficulty;
		float num12 = Mathf.Max(num * abilityConfig.MinUpdateRatio, num11);
		double ability = Mathf.Clamp(num + num9, num12, num10);
		val.Ability = ability;
		float num13 = Mathf.Abs(1.25f - num5);
		if (num13 > abilityConfig.RtThreshold)
		{
			val.Uncertainty = Math.Min(num2 + abilityConfig.DeUpscale * num13, abilityConfig.MaxUncertainty);
			val.Uncertainty = Mathf.Max((float)val.Uncertainty, 30f);
		}
		else
		{
			val.Uncertainty = Math.Max(num2 * abilityConfig.DeDownscale, abilityConfig.MinUncertainty);
		}
		UpdateGlobalAbility(gameLevelProgress);
		ctx.PlayerAbility = gameLevelProgress.GlobalAbility;
		ctx.Data["IntervalId"] = levelIntervalId;
		ctx.Data["GlobalAbility"] = gameLevelProgress.GlobalAbility;
		ctx.Data["IntervalMinDifficulty"] = val.MinDifficulty;
		ctx.Data["IntervalMaxDifficulty"] = val.MaxDifficulty;
		ctx.Data["TE"] = num4;
		ctx.Data["TA"] = levelPassTime;
		ctx.Data["RT"] = num5;
		ctx.Data["KT"] = num6;
		ctx.Data["DeltaR"] = num9;
		ctx.Data["NewRu"] = val.Ability;
		ctx.Data["NewDu"] = val.Uncertainty;
		ctx.Data["LevelWeight"] = num7;
		ctx.Data["DuFactor"] = num8;
		ctx.Data["Ru"] = num;
		ctx.Data["Du"] = num2;
		ctx.Data["S"] = num3;
		ctx.Data["LevelIndex"] = ctx.LevelIndex;
		ctx.Data["TimeFactorM"] = timeFactorM;
		ctx.Data["LearningRateK"] = learningRateK;
		StringBuilder stringBuilder = new StringBuilder();
		foreach (KeyValuePair<string, object> datum in ctx.Data)
		{
			stringBuilder.Append($"{datum.Key}={datum.Value}, ");
		}
	}

	private void EnsureIntervalExists(GameLevelProgress gameLevelProgress, string intervalId, LevelContext ctx)
	{
		//IL_00d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fb: Unknown result type (might be due to invalid IL or missing references)
		//IL_010e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0126: Expected O, but got Unknown
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ca: Expected O, but got Unknown
		if (!gameLevelProgress.Intervals.ContainsKey(intervalId))
		{
			KeyValuePair<string, (float, float)> keyValuePair = LevelSystem.GetRepository().GetRangesDictionary().FirstOrDefault<KeyValuePair<string, (float, float)>>((KeyValuePair<string, (float min, float max)> kv) => kv.Key == intervalId);
			if (keyValuePair.Key != null)
			{
				double ability = Math.Max(keyValuePair.Value.Item1, Math.Min(keyValuePair.Value.Item2, gameLevelProgress.GlobalAbility));
				gameLevelProgress.Intervals[intervalId] = new IntervalData
				{
					Ability = ability,
					Uncertainty = ctx.AbilityConfig.InitMaxUncertainty,
					MinDifficulty = keyValuePair.Value.Item1,
					MaxDifficulty = keyValuePair.Value.Item2
				};
			}
			else
			{
				gameLevelProgress.Intervals[intervalId] = new IntervalData
				{
					Ability = gameLevelProgress.GlobalAbility,
					Uncertainty = ctx.AbilityConfig.InitMaxUncertainty,
					MinDifficulty = keyValuePair.Value.Item1,
					MaxDifficulty = keyValuePair.Value.Item2
				};
				Log.LogError("LevelSystem", "[AbilityUpdate] Interval ID " + intervalId + " not found in repository. Using default difficulty range.");
			}
		}
	}

	public static List<IntervalData> GetUncertainIntervals(Dictionary<string, IntervalData> intervals, float maxUncertainty)
	{
		List<IntervalData> list = new List<IntervalData>(intervals.Count);
		foreach (KeyValuePair<string, IntervalData> interval in intervals)
		{
			IntervalData value = interval.Value;
			if (value.Uncertainty < (double)maxUncertainty)
			{
				list.Add(value);
			}
		}
		return list;
	}

	private double ValidIntervalsSum(List<IntervalData> validIntervals, bool ability = true)
	{
		double num = 0.0;
		for (int i = 0; i < validIntervals.Count; i++)
		{
			IntervalData val = validIntervals[i];
			if (val.Uncertainty != 0.0)
			{
				num = ((!ability) ? (num + 1.0 / val.Uncertainty) : (num + val.Ability / val.Uncertainty));
			}
		}
		return num;
	}

	private void UpdateGlobalAbility(GameLevelProgress gameLevelProgress)
	{
		List<IntervalData> uncertainIntervals = GetUncertainIntervals(gameLevelProgress.Intervals, 400f);
		if (uncertainIntervals.Count != 0)
		{
			double num = ValidIntervalsSum(uncertainIntervals);
			double num2 = ValidIntervalsSum(uncertainIntervals, ability: false);
			gameLevelProgress.GlobalAbility = (float)(num / num2);
		}
	}
}
