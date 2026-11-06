using System.Collections.Generic;
using Example.Cfg;
using XingYun.Level;
using XingYun.Level.Action;
using XingYun.Level.Condition;

namespace Game.Level;

public static class LevelSystemBusinessExtensions
{
	private static ILevelStrategy currentRuleStrategy;

	public static ILevelMatchFilter LevelMatchFilter;

	private static bool rulesInitialized;

	public static void InitializeRules()
	{
		if (!rulesInitialized)
		{
			RegisterBaseConditions();
			RegisterBaseActions();
			rulesInitialized = true;
		}
	}

	private static void RegisterBaseConditions()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Expected O, but got Unknown
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Expected O, but got Unknown
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Expected O, but got Unknown
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Expected O, but got Unknown
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Expected O, but got Unknown
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Expected O, but got Unknown
		UniversalLevelStrategy.RegisterCondition(1, (IMatchCondition)new FixedLevelCondition());
		UniversalLevelStrategy.RegisterCondition(2, (IMatchCondition)new RangeLevelCondition());
		UniversalLevelStrategy.RegisterCondition(6, (IMatchCondition)new GreaterLevelCondition());
		UniversalLevelStrategy.RegisterCondition(3, (IMatchCondition)new MultipleLevelCondition());
		UniversalLevelStrategy.RegisterCondition(4, (IMatchCondition)new NotMultipleLevelCondition());
		UniversalLevelStrategy.RegisterCondition(5, (IMatchCondition)new AfterMultipleCondition());
		UniversalLevelStrategy.RegisterCondition(11, (IMatchCondition)(object)new LessLevelCondition());
		UniversalLevelStrategy.RegisterCondition(9, (IMatchCondition)(object)new WinRate10LevelGreaterCondition());
		UniversalLevelStrategy.RegisterCondition(10, (IMatchCondition)(object)new WinRate10GameGreaterCondition());
	}

	private static void RegisterBaseActions()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Expected O, but got Unknown
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Expected O, but got Unknown
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Expected O, but got Unknown
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Expected O, but got Unknown
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Expected O, but got Unknown
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Expected O, but got Unknown
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Expected O, but got Unknown
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Expected O, but got Unknown
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Expected O, but got Unknown
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Expected O, but got Unknown
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Expected O, but got Unknown
		UniversalLevelStrategy.RegisterAction(0, (IMatchAction)new NoneAction());
		UniversalLevelStrategy.RegisterAction(1, (IMatchAction)(object)new KindEqualAction());
		UniversalLevelStrategy.RegisterAction(2, (IMatchAction)new GreaterAction());
		UniversalLevelStrategy.RegisterAction(3, (IMatchAction)(object)new PartsAction());
		UniversalLevelStrategy.RegisterAction(4, (IMatchAction)new InAction());
		UniversalLevelStrategy.RegisterAction(5, (IMatchAction)new BetweenAction());
		UniversalLevelStrategy.RegisterAction(6, (IMatchAction)new DecreaseAction());
		UniversalLevelStrategy.RegisterAction(7, (IMatchAction)new IncreaseAction());
		UniversalLevelStrategy.RegisterAction(8, (IMatchAction)new KeepSameAction());
		UniversalLevelStrategy.RegisterAction(9, (IMatchAction)new RandomAction());
		UniversalLevelStrategy.RegisterAction(10, (IMatchAction)new SetTagAction());
		UniversalLevelStrategy.RegisterAction(11, (IMatchAction)new SwitchRepositoryAction());
		UniversalLevelStrategy.RegisterAction(17, (IMatchAction)(object)new RandomWeightAction());
		UniversalLevelStrategy.RegisterAction(18, (IMatchAction)new IncreaseAction());
	}

	public static void RegisterCondition(int matchType, IMatchCondition handler)
	{
		UniversalLevelStrategy.RegisterCondition(matchType, handler);
	}

	public static void RegisterAction(int @operator, IMatchAction handler)
	{
		UniversalLevelStrategy.RegisterAction(@operator, handler);
	}

	public static void AttachRuleStrategy(int groupId, List<LevelMatchConfig> configs)
	{
		AttachRuleStrategy(new int[1] { groupId }, configs);
	}

	public static void AttachRuleStrategy(IEnumerable<int> groupIds, List<LevelMatchConfig> configs)
	{
		AttachRuleStrategy(configs, new GroupLevelMatchFilter(groupIds));
	}

	public static void AttachRuleStrategy(List<LevelMatchConfig> configs, ILevelMatchFilter matchFilter = null)
	{
		if (configs == null || configs.Count == 0)
		{
			DetachRuleStrategy();
			return;
		}
		if (matchFilter != null)
		{
			LevelMatchFilter = matchFilter;
		}
		List<LevelMatchConfig> list = LevelMatchFilter?.Filter(configs) ?? configs;
		if (list == null || list.Count == 0)
		{
			DetachRuleStrategy();
			return;
		}
		List<ILevelMatchRule> rules = LevelMatchConfigAdapter.ToRules(list);
		currentRuleStrategy = (ILevelStrategy)(object)new RandomLevelMatchStrategy(GetMatchFilterInfo(), rules);
		LevelSystemExtensions.SetStrategy(currentRuleStrategy);
	}

	public static void DetachRuleStrategy()
	{
		currentRuleStrategy = null;
		LevelMatchFilter = null;
		LevelSystemExtensions.ClearStrategy();
	}

	public static bool HasRuleStrategy()
	{
		return currentRuleStrategy != null;
	}

	public static string GetMatchFilterInfo()
	{
		if (LevelMatchFilter != null)
		{
			return LevelMatchFilter.GetMatchFilterInfo();
		}
		return "NoMatchFilter";
	}
}
