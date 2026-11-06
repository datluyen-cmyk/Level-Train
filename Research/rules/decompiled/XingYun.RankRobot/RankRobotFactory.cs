using System;
using System.Collections.Generic;

namespace XingYun.RankRobot;

public static class RankRobotFactory
{
	public static (List<RankRobotCoreData> robots, List<int> usedNameIds) CreateRobotsForNewActivity(int robotCount, IRobotProfileSource profileSource, IRankRobotScoreConfig scoreCfg, List<int> lastActivityUsedNameIds = null, Random rng = null)
	{
		if (profileSource == null)
		{
			throw new ArgumentNullException("profileSource");
		}
		if (scoreCfg == null)
		{
			throw new ArgumentNullException("scoreCfg");
		}
		if (rng == null)
		{
			rng = new Random();
		}
		robotCount = Math.Max(0, Math.Min(500, robotCount));
		List<RankRobotCoreData> list = new List<RankRobotCoreData>(robotCount);
		List<int> list2 = new List<int>(robotCount);
		HashSet<int> usedIdsThis = new HashSet<int>(robotCount);
		HashSet<int> hashSet = new HashSet<int>(lastActivityUsedNameIds ?? new List<int>());
		List<int> allNameIds = profileSource.GetAllNameIds();
		if (allNameIds == null || allNameIds.Count == 0)
		{
			throw new Exception("Robot name id list is empty.");
		}
		List<int> list3 = new List<int>(allNameIds.Count);
		for (int i = 0; i < allNameIds.Count; i++)
		{
			int item = allNameIds[i];
			if (!hashSet.Contains(item))
			{
				list3.Add(item);
			}
		}
		if (list3.Count < robotCount)
		{
			for (int j = 0; j < allNameIds.Count; j++)
			{
				int item2 = allNameIds[j];
				if (!list3.Contains(item2))
				{
					list3.Add(item2);
				}
			}
		}
		Shuffle(list3, rng);
		for (int k = 0; k < robotCount; k++)
		{
			int num = TakeNextDistinctId(list3, usedIdsThis, rng);
			RobotProfile randomProfile = profileSource.GetRandomProfile(num, rng);
			RankRobotCoreData item3 = new RankRobotCoreData
			{
				RobotId = k + 1,
				RobotNameId = num,
				HeadId = randomProfile.HeadId,
				HeadFrameId = randomProfile.HeadFrameId,
				Score = 0
			};
			list.Add(item3);
			list2.Add(num);
		}
		RankRobotScoreCalculator.InitBotPoints(list, scoreCfg, rng);
		return (robots: list, usedNameIds: list2);
	}

	private static int TakeNextDistinctId(List<int> pool, HashSet<int> usedIdsThis, Random rng)
	{
		for (int i = 0; i < pool.Count; i++)
		{
			int num = pool[i];
			if (usedIdsThis.Add(num))
			{
				return num;
			}
		}
		int index = rng.Next(0, pool.Count);
		return pool[index];
	}

	private static void Shuffle<T>(List<T> list, Random rng)
	{
		for (int num = list.Count - 1; num > 0; num--)
		{
			int num2 = rng.Next(0, num + 1);
			int index = num;
			int index2 = num2;
			T val = list[num2];
			T val2 = list[num];
			T val3 = (list[index] = val);
			val3 = (list[index2] = val2);
		}
	}
}
