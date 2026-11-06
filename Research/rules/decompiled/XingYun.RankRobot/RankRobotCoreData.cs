using System;

namespace XingYun.RankRobot;

[Serializable]
public class RankRobotCoreData : IRankRobotScoreData
{
	public int RobotNameId;

	public int HeadId;

	public int HeadFrameId;

	public int RobotId { get; set; }

	public int Score { get; set; }

	public int BotPointMax { get; set; }
}
