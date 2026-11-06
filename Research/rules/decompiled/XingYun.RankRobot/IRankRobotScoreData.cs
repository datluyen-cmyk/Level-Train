namespace XingYun.RankRobot;

public interface IRankRobotScoreData
{
	int RobotId { get; }

	int Score { get; set; }

	int BotPointMax { get; set; }
}
