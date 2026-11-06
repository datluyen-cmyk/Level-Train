using System;
using XingYun.RankRobot;

namespace Rotaterings.Rank;

[Serializable]
public class RankRobotData : IRankRobotScoreData
{
	public int RobotId;

	public int RobotNameId;

	public string Name;

	public int HeadId;

	public int HeadFrameId;

	public int Score;

	public int BotPointMax;

	int IRankRobotScoreData.RobotId => RobotId;

	int IRankRobotScoreData.Score
	{
		get
		{
			return Score;
		}
		set
		{
			Score = value;
		}
	}

	int IRankRobotScoreData.BotPointMax
	{
		get
		{
			return BotPointMax;
		}
		set
		{
			BotPointMax = value;
		}
	}
}
