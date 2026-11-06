using System.Collections.Generic;
using XingYun.RankRobot;

namespace Rotaterings.Rank;

internal static class RankRobotDataConverter
{
	private static RankRobotData ToProject(RankRobotCoreData src)
	{
		if (src == null)
		{
			return null;
		}
		return new RankRobotData
		{
			RobotId = src.RobotId,
			RobotNameId = src.RobotNameId,
			HeadId = src.HeadId,
			HeadFrameId = src.HeadFrameId,
			Score = src.Score,
			BotPointMax = src.BotPointMax
		};
	}

	internal static List<RankRobotData> ToProjectList(List<RankRobotCoreData> src)
	{
		if (src == null)
		{
			return null;
		}
		List<RankRobotData> list = new List<RankRobotData>(src.Count);
		for (int i = 0; i < src.Count; i++)
		{
			list.Add(ToProject(src[i]));
		}
		return list;
	}
}
