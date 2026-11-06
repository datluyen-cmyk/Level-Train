using System.Collections.Generic;
using Example.Cfg;
using XingYun.Level;
using XingYun.Utility;

namespace Game.Level;

public static class LevelMatchConfigAdapter
{
	private class LevelMatchRuleWrapper : ILevelMatchRule
	{
		private readonly LevelMatchConfig config;

		public string Id => config.Id;

		public int GroupId => GetValue("GroupId", config.GroupId);

		public int GroupPriority => GetValue("GroupPriority", config.GroupPriority);

		public int Priority => GetValue("Priority", config.Priority);

		public int MatchType => GetValue("MatchType", (int)config.Type);

		public int IntParam1 => GetValue("IntParam1", config.IntParam1);

		public int IntParam2 => GetValue("IntParam2", config.IntParam2);

		public string StringParam1 => GetValue("StringParam1", config.StringParam1);

		public string StringParam2 => GetValue("StringParam2", config.StringParam2);

		public float FloatParam1 => GetValue("FloatParam1", config.FloatParam1);

		public string Target => GetValue("Target", config.Target.ToString());

		public int Operator => GetValue("Operator", (int)config.Operator);

		public string StringValue1 => GetValue("StringValue1", config.StringValue1);

		public string StringValue2 => GetValue("StringValue2", config.StringValue2);

		public LevelMatchRuleWrapper(LevelMatchConfig config)
		{
			this.config = config;
		}

		private T GetValue<T>(string key, T defaultValue)
		{
			if (LevelMatchConfigExptData.TryGetExptData(config.Id, key, out var value) && value != null)
			{
				try
				{
					T result = default(T);
					if (Parser.TryParse<T>(value, ref result))
					{
						return result;
					}
					return defaultValue;
				}
				catch
				{
					return defaultValue;
				}
			}
			return defaultValue;
		}
	}

	public static List<ILevelMatchRule> ToRules(List<LevelMatchConfig> configs)
	{
		if (configs == null)
		{
			return new List<ILevelMatchRule>();
		}
		List<ILevelMatchRule> list = new List<ILevelMatchRule>(configs.Count);
		foreach (LevelMatchConfig config in configs)
		{
			list.Add((ILevelMatchRule)(object)new LevelMatchRuleWrapper(config));
		}
		return list;
	}
}
