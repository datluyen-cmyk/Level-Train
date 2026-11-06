using System.Collections.Generic;
using Example.Cfg;
using XingYun.Level;

namespace Game.Level;

public static class LevelSystemFacadeExtensions
{
	public static void InitializeRules(this LevelSystemFacade facade)
	{
		LevelSystemBusinessExtensions.InitializeRules();
	}

	public static void RegisterCondition(this LevelSystemFacade facade, int matchType, IMatchCondition handler)
	{
		LevelSystemBusinessExtensions.RegisterCondition(matchType, handler);
	}

	public static void RegisterAction(this LevelSystemFacade facade, int @operator, IMatchAction handler)
	{
		LevelSystemBusinessExtensions.RegisterAction(@operator, handler);
	}

	public static void AttachRuleStrategy(this LevelSystemFacade facade, int groupId, List<LevelMatchConfig> configs)
	{
		LevelSystemBusinessExtensions.AttachRuleStrategy(groupId, configs);
	}

	public static void DetachRuleStrategy(this LevelSystemFacade facade)
	{
		LevelSystemBusinessExtensions.DetachRuleStrategy();
	}

	public static bool HasRuleStrategy(this LevelSystemFacade facade)
	{
		return LevelSystemBusinessExtensions.HasRuleStrategy();
	}

	public static string GetMatchFilterInfo(this LevelSystemFacade facade)
	{
		return LevelSystemBusinessExtensions.GetMatchFilterInfo();
	}
}
