using System;
using System.Collections.Generic;
using System.Text;
using Rotaterings.SO;
using XingYun.RankRobot;

namespace Rotaterings.Rank;

public static class RankRobotScoreCalculator
{
	public readonly struct RankRobotRefreshLogContext
	{
		public readonly string RefreshType;

		public readonly int ExpGames;

		public readonly int ActiveGamesTotal;

		public readonly int ActiveDays;

		public readonly int ClearLevels;

		public readonly int LevelGap;

		public readonly int Excess;

		public readonly int ChasingTurn;

		public readonly int PrePoint;

		public readonly int PreRank;

		public readonly int PlayerAdd;

		public RankRobotRefreshLogContext(string refreshType, int expGames, int activeGamesTotal, int activeDays, int clearLevels, int levelGap, int excess, int chasingTurn, int prePoint, int preRank, int playerAdd)
		{
			RefreshType = refreshType ?? string.Empty;
			ExpGames = expGames;
			ActiveGamesTotal = activeGamesTotal;
			ActiveDays = activeDays;
			ClearLevels = clearLevels;
			LevelGap = levelGap;
			Excess = excess;
			ChasingTurn = chasingTurn;
			PrePoint = prePoint;
			PreRank = preRank;
			PlayerAdd = playerAdd;
		}

		internal XingYun.RankRobot.RankRobotScoreCalculator.RankRobotRefreshLogContext ToModule()
		{
			return new XingYun.RankRobot.RankRobotScoreCalculator.RankRobotRefreshLogContext(RefreshType, ExpGames, ActiveGamesTotal, ActiveDays, ClearLevels, LevelGap, Excess, ChasingTurn, PrePoint, PreRank, PlayerAdd);
		}
	}

	private const string Tag = "RankRobotScore";

	private static readonly Random SharedRng = new Random();

	public static int CalcChasingTurn(int levelGap, RankRobotScoreConfig cfg)
	{
		return XingYun.RankRobot.RankRobotScoreCalculator.CalcChasingTurn(levelGap, cfg);
	}

	public static int CalcExpGames(int activeGamesTotal, int activeDays, int totalGames, int dayCount, RankRobotScoreConfig cfg)
	{
		return XingYun.RankRobot.RankRobotScoreCalculator.CalcExpGames(activeGamesTotal, activeDays, cfg);
	}

	public static void LogInitScores(List<RankRobotData> robots)
	{
		StringBuilder stringBuilder = new StringBuilder(robots.Count * 32);
		stringBuilder.Append("[RobotInit] 机器人初始化积分与上限:");
		for (int i = 0; i < robots.Count; i++)
		{
			RankRobotData rankRobotData = robots[i];
			if (rankRobotData != null)
			{
				stringBuilder.Append("\n  robotId=").Append(rankRobotData.RobotId).Append(", order=")
					.Append(i + 1)
					.Append(", score=")
					.Append(rankRobotData.Score)
					.Append(", pointMax=")
					.Append(rankRobotData.BotPointMax);
			}
		}
	}

	public static void UpdateOnFirstClear(List<RankRobotData> robots, RankRobotScoreConfig cfg, RankRobotRefreshLogContext logContext)
	{
		XingYun.RankRobot.RankRobotScoreCalculator.UpdateOnFirstClear(robots, cfg, SharedRng, logContext.ToModule());
	}

	public static void UpdateOnNonFirstClear(List<RankRobotData> robots, RankRobotScoreConfig cfg, RankRobotRefreshLogContext logContext)
	{
		XingYun.RankRobot.RankRobotScoreCalculator.UpdateOnNonFirstClear(robots, cfg, SharedRng, logContext.ToModule());
	}

	public static void ApplyStagnation(List<RankRobotData> robots, RankRobotScoreConfig cfg, bool checkPrePoint, RankRobotRefreshLogContext logContext)
	{
		XingYun.RankRobot.RankRobotScoreCalculator.ApplyStagnation(robots, cfg, SharedRng, checkPrePoint, logContext.ToModule());
	}

	private static Dictionary<int, string> BuildUniformStates(List<RankRobotData> robots, string state)
	{
		Dictionary<int, string> dictionary = new Dictionary<int, string>(robots.Count);
		for (int i = 0; i < robots.Count; i++)
		{
			RankRobotData rankRobotData = robots[i];
			if (rankRobotData != null)
			{
				dictionary[rankRobotData.RobotId] = state;
			}
		}
		return dictionary;
	}

	private static Dictionary<int, int> SnapshotScores(List<RankRobotData> robots)
	{
		Dictionary<int, int> dictionary = new Dictionary<int, int>(robots.Count);
		for (int i = 0; i < robots.Count; i++)
		{
			RankRobotData rankRobotData = robots[i];
			if (rankRobotData != null)
			{
				dictionary[rankRobotData.RobotId] = rankRobotData.Score;
			}
		}
		return dictionary;
	}

	private static void LogRobotScoreChanges(string refreshType, List<RankRobotData> robots, Dictionary<int, int> beforeScores, Dictionary<int, string> states)
	{
		StringBuilder stringBuilder = new StringBuilder(robots.Count * 40);
		stringBuilder.Append("[RobotScore] 机器人加分 ").Append(refreshType).Append(':');
		for (int i = 0; i < robots.Count; i++)
		{
			RankRobotData rankRobotData = robots[i];
			if (rankRobotData != null)
			{
				int value;
				int num = (beforeScores.TryGetValue(rankRobotData.RobotId, out value) ? value : rankRobotData.Score);
				int value2 = rankRobotData.Score - num;
				string value4;
				string value3 = (states.TryGetValue(rankRobotData.RobotId, out value4) ? value4 : "未知");
				stringBuilder.Append("\n  robotId=").Append(rankRobotData.RobotId).Append(", order=")
					.Append(i + 1)
					.Append(", state=")
					.Append(value3)
					.Append(", ")
					.Append(num)
					.Append("->")
					.Append(rankRobotData.Score)
					.Append(" (+")
					.Append(value2)
					.Append(')');
			}
		}
	}

	private static void LogRefreshHeader(string refreshType, RankRobotRefreshLogContext ctx)
	{
	}
}
