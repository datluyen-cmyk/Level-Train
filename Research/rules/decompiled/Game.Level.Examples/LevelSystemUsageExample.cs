using System.Collections.Generic;
using Example.Cfg;
using XingYun.Config;
using XingYun.Core.Singletons;
using XingYun.Level;

namespace Game.Level.Examples;

public static class LevelSystemUsageExample
{
	public static void Example1_BasicUsage()
	{
		GameApp.Level.InitializeWithPath("DefaultLevelStyle1");
		GameApp.Level.GetLevel(15, (Dictionary<string, object>)null);
		GameApp.Level.GetLevelInfo(15, (Dictionary<string, object>)null);
		GameApp.Level.UpdateAbility(15, 18.5f, 0);
	}

	public static void Example2_WithRuleStrategy()
	{
		GameApp.Level.InitializeWithPath("DefaultLevelStyle1");
		GameApp.Level.InitializeRules();
		List<LevelMatchConfig> dataList = Singleton<ConfigManager>.Instance.GetTable<ExampleTables>().TbLevelMatchConfig.DataList;
		GameApp.Level.AttachRuleStrategy(1001, dataList);
		GameApp.Level.GetLevel(15, (Dictionary<string, object>)null);
		GameApp.Level.HasRuleStrategy();
		GameApp.Level.DetachRuleStrategy();
	}

	public static void Example3_RegisterBusinessExtensions()
	{
		GameApp.Level.InitializeWithPath("DefaultLevelStyle1");
		GameApp.Level.InitializeRules();
		GameApp.Level.RegisterCondition(100, (IMatchCondition)(object)new BusinessCustomCondition());
		GameApp.Level.RegisterAction(100, (IMatchAction)(object)new BusinessCustomAction());
		List<LevelMatchConfig> dataList = Singleton<ConfigManager>.Instance.GetTable<ExampleTables>().TbLevelMatchConfig.DataList;
		GameApp.Level.AttachRuleStrategy(1, dataList);
		GameApp.Level.GetLevel(15, (Dictionary<string, object>)null);
	}

	public static void Example4_CompleteGameFlow()
	{
		GameApp.Level.InitializeWithPath("DefaultLevelStyle1");
		GameApp.Level.InitializeRules();
		GameApp.Level.SetGlobalData("death_weights", (object)new int[4] { 10, 30, 40, 20 });
		List<LevelMatchConfig> dataList = Singleton<ConfigManager>.Instance.GetTable<ExampleTables>().TbLevelMatchConfig.DataList;
		GameApp.Level.AttachRuleStrategy(1, dataList);
		int num = 15;
		GameApp.Level.GetLevelInfo(num, (Dictionary<string, object>)null);
		float num2 = 18.5f;
		int num3 = 0;
		GameApp.Level.UpdateAbility(num, num2, num3);
		num++;
		GameApp.Level.GetLevelInfo(num, (Dictionary<string, object>)null);
	}
}
