using System;
using System.Collections.Generic;

namespace XingYun.RankRobot;

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

		public RankRobotRefreshLogContext WithPhaseData(string refreshType, int levelGap, int excess, int chasingTurn)
		{
			return new RankRobotRefreshLogContext(refreshType, ExpGames, ActiveGamesTotal, ActiveDays, ClearLevels, levelGap, excess, chasingTurn, PrePoint, PreRank, PlayerAdd);
		}
	}

	private static readonly Random SharedRng = new Random();

	private static int RangeInt(Random rng, int minInclusive, int maxExclusive)
	{
		if (maxExclusive <= minInclusive)
		{
			return minInclusive;
		}
		return rng.Next(minInclusive, maxExclusive);
	}

	public static int WeightedRandom(int[] keys, int[] weights, Random rng)
	{
		if (keys == null || weights == null || keys.Length == 0 || keys.Length != weights.Length)
		{
			return 0;
		}
		int num = 0;
		for (int i = 0; i < weights.Length; i++)
		{
			num += weights[i];
		}
		if (num <= 0)
		{
			return keys[0];
		}
		int num2 = RangeInt(rng, 0, num);
		int num3 = 0;
		for (int j = 0; j < weights.Length; j++)
		{
			num3 += weights[j];
			if (num2 < num3)
			{
				return keys[j];
			}
		}
		return keys[^1];
	}

	public static void SortByScoreDesc<T>(List<T> robots) where T : IRankRobotScoreData
	{
		robots.Sort(delegate(T a, T b)
		{
			int num = b.Score.CompareTo(a.Score);
			return (num != 0) ? num : a.RobotId.CompareTo(b.RobotId);
		});
	}

	public static void AssignRankAndPointMax<T>(List<T> robots, IRankRobotScoreConfig cfg) where T : IRankRobotScoreData
	{
		SortByScoreDesc(robots);
		for (int i = 0; i < robots.Count; i++)
		{
			int num = i + 1;
			int num2 = cfg.BOTS_NUM - num;
			T val = robots[i];
			val.BotPointMax = (int)Math.Floor(cfg.GRAD_COEF * (float)num2 * (float)num2);
		}
	}

	public static int CalcChasingTurn(int levelGap, IRankRobotScoreConfig cfg)
	{
		if (cfg == null || levelGap > 0)
		{
			return 0;
		}
		int num = cfg.BOT_CHASING_COUNT + cfg.BOT_CHASING_INTERVAL;
		if (num <= 0)
		{
			return 0;
		}
		return -levelGap / num;
	}

	public static void InitBotPoints<T>(List<T> robots, IRankRobotScoreConfig cfg, Random rng = null) where T : IRankRobotScoreData
	{
		if (robots == null)
		{
			throw new ArgumentNullException("robots");
		}
		if (cfg == null)
		{
			throw new ArgumentNullException("cfg");
		}
		if (rng == null)
		{
			rng = SharedRng;
		}
		for (int i = 0; i < robots.Count; i++)
		{
			int score = WeightedRandom(cfg.INIT_POINTS_KEYS, cfg.INIT_POINTS_WEIGHTS, rng);
			T val = robots[i];
			val.Score = score;
		}
		AssignRankAndPointMax(robots, cfg);
	}

	public static int CalcExpGames(int activeGamesTotal, int activeDays, IRankRobotScoreConfig cfg)
	{
		if (cfg == null)
		{
			throw new ArgumentNullException("cfg");
		}
		if (activeDays < 1)
		{
			return cfg.EXP_GAMES_DEFAULT;
		}
		int num = Math.Max(1, activeGamesTotal / activeDays);
		float eXP_GAMES_ACTIVE_BASE = cfg.EXP_GAMES_ACTIVE_BASE;
		if (eXP_GAMES_ACTIVE_BASE <= 0f || Math.Abs(eXP_GAMES_ACTIVE_BASE - 1f) < 1E-06f)
		{
			return cfg.EXP_GAMES_DEFAULT;
		}
		float num2 = (float)Math.Log(num);
		float num3 = (float)Math.Log(eXP_GAMES_ACTIVE_BASE);
		if (Math.Abs(num3) < 1E-06f)
		{
			return cfg.EXP_GAMES_DEFAULT;
		}
		int val = (int)Math.Floor(num2 / num3 + cfg.EXP_GAMES_ACTIVE_OFFSET) + 1;
		return Math.Max(1, val);
	}

	public static void UpdateOnFirstClear<T>(List<T> robots, IRankRobotScoreConfig cfg, Random rng, RankRobotRefreshLogContext logContext) where T : IRankRobotScoreData
	{
		if (robots == null)
		{
			throw new ArgumentNullException("robots");
		}
		if (cfg == null)
		{
			throw new ArgumentNullException("cfg");
		}
		if (rng == null)
		{
			rng = SharedRng;
		}
		AssignRankAndPointMax(robots, cfg);
		for (int i = 0; i < robots.Count; i++)
		{
			T val = robots[i];
			int num = i + 1;
			int num2 = (int)Math.Floor((float)val.BotPointMax * (cfg.JUMP_COEF / ((float)num + cfg.JUMP_OFFSET_X) + cfg.JUMP_OFFSET_Y));
			if (num2 < 2)
			{
				num2 = 2;
			}
			int score = val.Score + num2;
			val.Score = score;
		}
		AssignRankAndPointMax(robots, cfg);
	}

	public static void UpdateOnNonFirstClear<T>(List<T> robots, IRankRobotScoreConfig cfg, Random rng, RankRobotRefreshLogContext logContext, IDictionary<int, string> stateLabels = null) where T : IRankRobotScoreData
	{
		if (robots == null)
		{
			throw new ArgumentNullException("robots");
		}
		if (cfg == null)
		{
			throw new ArgumentNullException("cfg");
		}
		if (rng == null)
		{
			rng = SharedRng;
		}
		AssignRankAndPointMax(robots, cfg);
		int levelGap = logContext.LevelGap;
		if (levelGap > 0)
		{
			int excess = logContext.PreRank / levelGap;
			RankRobotRefreshLogContext logContext2 = logContext.WithPhaseData("非首次通关-未达期望局数", levelGap, excess, 0);
			ApplyThreeCategories(robots, cfg, rng, logContext2, stateLabels);
		}
		else
		{
			int num = cfg.BOT_CHASING_COUNT + cfg.BOT_CHASING_INTERVAL;
			int num2 = CalcChasingTurn(levelGap, cfg);
			int num3 = num * num2 + logContext.ExpGames + cfg.BOT_CHASING_COUNT;
			if (logContext.ClearLevels < num3)
			{
				ApplyChasingOnly(robots, cfg, rng, logContext, stateLabels);
			}
			else
			{
				RankRobotRefreshLogContext logContext3 = logContext.WithPhaseData("非首次通关-摆烂阶段", levelGap, 0, num2);
				ApplyStagnation(robots, cfg, rng, checkPrePoint: true, logContext3, stateLabels);
			}
		}
		SortByScoreDesc(robots);
	}

	private static void ApplyThreeCategories<T>(List<T> robots, IRankRobotScoreConfig cfg, Random rng, RankRobotRefreshLogContext logContext, IDictionary<int, string> stateLabels) where T : IRankRobotScoreData
	{
		for (int i = 0; i < robots.Count; i++)
		{
			T val = robots[i];
			int num = ((i < logContext.PreRank - 1) ? (i + 1) : (i + 2));
			if (num > logContext.PreRank)
			{
				if (stateLabels != null)
				{
					stateLabels[val.RobotId] = "追分区";
				}
				int num2 = logContext.PrePoint - val.Score + logContext.PlayerAdd;
				int num3 = ((num2 > 0) ? RangeInt(rng, 0, num2 + 1) : 0);
				if (num3 < 2)
				{
					num3 = 2;
				}
				if (num3 > cfg.CHASING_BOTS_MAX_POINT)
				{
					num3 = cfg.CHASING_BOTS_MAX_POINT - 2;
				}
				int score = val.Score + num3;
				val.Score = score;
			}
			else if (num < logContext.PreRank && num >= logContext.PreRank - logContext.Excess)
			{
				if (stateLabels != null)
				{
					stateLabels[val.RobotId] = "被超越区";
				}
				int num4 = val.Score - logContext.PrePoint;
				int num5 = ((num4 > 0) ? RangeInt(rng, 0, num4 + 1) : 0);
				if (num5 < 2)
				{
					num5 = 2;
				}
				int score2 = val.Score + num5;
				val.Score = score2;
			}
			else if (num < logContext.PreRank - logContext.Excess)
			{
				if (stateLabels != null)
				{
					stateLabels[val.RobotId] = "顶部加分区";
				}
				int num6 = 0;
				if (val.Score < val.BotPointMax)
				{
					num6 = WeightedRandom(cfg.ADD_POINTS_TOP_KEYS, cfg.ADD_POINTS_TOP_WEIGHTS, rng);
					int num7 = WeightedRandom(cfg.ODDS_LIST, cfg.oddsWeight, rng);
					num6 *= num7;
					int score3 = val.Score + num6;
					val.Score = score3;
				}
				int score4 = Math.Min(val.Score, val.BotPointMax);
				val.Score = score4;
			}
		}
	}

	private static void ApplyChasingOnly<T>(List<T> robots, IRankRobotScoreConfig cfg, Random rng, RankRobotRefreshLogContext logContext, IDictionary<int, string> stateLabels) where T : IRankRobotScoreData
	{
		for (int i = 0; i < robots.Count; i++)
		{
			T val = robots[i];
			if (((i < logContext.PreRank - 1) ? (i + 1) : (i + 2)) > logContext.PreRank)
			{
				if (stateLabels != null)
				{
					stateLabels[val.RobotId] = "追分区";
				}
				int num = logContext.PrePoint - val.Score + logContext.PlayerAdd;
				int num2 = ((num > 0) ? RangeInt(rng, 0, num + 1) : 0);
				if (num2 < 2)
				{
					num2 = 2;
				}
				if (num2 > cfg.CHASING_BOTS_MAX_POINT)
				{
					num2 = cfg.CHASING_BOTS_MAX_POINT - 2;
				}
				int score = val.Score + num2;
				val.Score = score;
			}
			else if (stateLabels != null)
			{
				stateLabels[val.RobotId] = "无操作";
			}
		}
	}

	public static void ApplyStagnation<T>(List<T> robots, IRankRobotScoreConfig cfg, Random rng, bool checkPrePoint, RankRobotRefreshLogContext logContext, IDictionary<int, string> stateLabels = null) where T : IRankRobotScoreData
	{
		if (robots == null || cfg == null)
		{
			return;
		}
		if (rng == null)
		{
			rng = SharedRng;
		}
		for (int i = 0; i < robots.Count; i++)
		{
			T val = robots[i];
			if (stateLabels != null)
			{
				stateLabels[val.RobotId] = "摆烂";
			}
			int num = WeightedRandom(cfg.STAG_POINTS_KEYS, cfg.STAG_POINTS_WEIGHTS, rng);
			if (checkPrePoint && val.Score + num > logContext.PrePoint)
			{
				num = 0;
			}
			int score = val.Score + num;
			val.Score = score;
		}
		SortByScoreDesc(robots);
	}
}
