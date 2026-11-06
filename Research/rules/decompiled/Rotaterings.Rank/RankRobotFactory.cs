using System;
using System.Collections.Generic;
using Example.Cfg;
using RotateRings.Common;
using RotateRings.Model;
using Rotaterings.SO;
using Rotaterings.UI.RankList;
using XingYun.RankRobot;

namespace Rotaterings.Rank;

public static class RankRobotFactory
{
	private const string Tag = "RankRobotScore";

	private static readonly Random SharedRng = new Random();

	public static List<RankRobotData> CreateRobotsForNewActivity(int robotCount, ExampleTables tables, RankModelData saveData, RankRobotScoreConfig scoreCfg)
	{
		int robotNameConfig = saveData.RobotNameConfig;
		int num = RankRobotNameConfigProvider.ResolveAvailableConfig(tables, RankFunctionResolver.GetRankRobotNameConfig());
		saveData.UsedRobotNameIdCooldownLastActivity.Clear();
		if (robotNameConfig == num)
		{
			List<int> usedRobotNameIdsThisActivity = saveData.UsedRobotNameIdsThisActivity;
			if (usedRobotNameIdsThisActivity != null && usedRobotNameIdsThisActivity.Count > 0)
			{
				saveData.UsedRobotNameIdCooldownLastActivity.AddRange(saveData.UsedRobotNameIdsThisActivity);
			}
		}
		saveData.UsedRobotNameIdsThisActivity.Clear();
		saveData.RobotNameConfig = num;
		RankRobotProfileSource profileSource = RankRobotNameConfigProvider.CreateProfileSource(tables, num, GetExcludedPlayerNameIds(tables, num));
		(List<RankRobotCoreData> robots, List<int> usedNameIds) tuple = XingYun.RankRobot.RankRobotFactory.CreateRobotsForNewActivity(robotCount, profileSource, scoreCfg, saveData.UsedRobotNameIdCooldownLastActivity, SharedRng);
		List<RankRobotCoreData> item = tuple.robots;
		List<int> item2 = tuple.usedNameIds;
		List<RankRobotData> list = RankRobotDataConverter.ToProjectList(item);
		FillDisplayFields(list, profileSource);
		saveData.UsedRobotNameIdsThisActivity.AddRange(item2);
		saveData.Robots.Clear();
		saveData.Robots.AddRange(list);
		return list;
	}

	private static void FillDisplayFields(List<RankRobotData> robots, RankRobotProfileSource profileSource)
	{
		for (int i = 0; i < robots.Count; i++)
		{
			RankRobotData rankRobotData = robots[i];
			rankRobotData.Name = profileSource.GetName(rankRobotData.RobotNameId);
		}
	}

	private static HashSet<int> GetExcludedPlayerNameIds(ExampleTables tables, int appliedNameConfig)
	{
		string text = GameApp.Save.GetOrCreate<PlayerModelData>().PlayerName;
		if (string.IsNullOrEmpty(text))
		{
			text = PlayerService.GetName();
		}
		if (string.IsNullOrEmpty(text))
		{
			return null;
		}
		return RankRobotNameConfigProvider.GetNameIdsMatching(tables, appliedNameConfig, text);
	}
}
