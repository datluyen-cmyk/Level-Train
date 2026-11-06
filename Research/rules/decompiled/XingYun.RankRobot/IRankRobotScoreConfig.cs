namespace XingYun.RankRobot;

public interface IRankRobotScoreConfig
{
	int BOTS_NUM { get; }

	float GRAD_COEF { get; }

	int EXP_GAMES_DEFAULT { get; }

	float EXP_GAMES_ACTIVE_BASE { get; }

	float EXP_GAMES_ACTIVE_OFFSET { get; }

	int[] INIT_POINTS_KEYS { get; }

	int[] INIT_POINTS_WEIGHTS { get; }

	float JUMP_COEF { get; }

	float JUMP_OFFSET_X { get; }

	float JUMP_OFFSET_Y { get; }

	int BOT_CHASING_COUNT { get; }

	int BOT_CHASING_INTERVAL { get; }

	int CHASING_BOTS_MAX_POINT { get; }

	int[] ADD_POINTS_TOP_KEYS { get; }

	int[] ADD_POINTS_TOP_WEIGHTS { get; }

	int[] ODDS_LIST { get; }

	int[] oddsWeight { get; }

	int[] STAG_POINTS_KEYS { get; }

	int[] STAG_POINTS_WEIGHTS { get; }

	long STAG_TIME_INTERVAL { get; }

	float STAG_TIME_CONTROL { get; }
}
