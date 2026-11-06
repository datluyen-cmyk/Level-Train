using System;
using System.Collections.Generic;
using Example.Cfg;
using RotateRings.Common;
using Rotaterings.Rank;
using UnityEngine;
using XingYun.Config;

namespace Rotaterings.UI.RankList;

public class RankListDataBuilder
{
	private const string Tag = "Rank";

	private const int TopCount = 3;

	private static readonly Random NameRepairRng = new Random();

	public List<RankItemData> AllEntries { get; } = new List<RankItemData>();

	public List<RankItemData> ListEntries { get; } = new List<RankItemData>();

	public int TotalItemCount { get; private set; }

	public RankItemData SelfEntry { get; private set; }

	public void BuildForMain()
	{
		RankModelData rankModelData = LoadAndAddRobots();
		if (rankModelData != null)
		{
			SortByScoreDesc();
			int num = RankActivityService.CalcPlayerRanking(rankModelData);
			bool flag = num > 0 && num <= RankConst.MaxRankBot;
			SelfEntry = CreatePlayerEntry(flag ? num : (-1), rankModelData.PlayerRankScore);
			if (flag)
			{
				InsertPlayer(num, rankModelData.PlayerRankScore);
			}
			TrimToMaxRankCount();
			AssignRankIndices();
			SplitTopAndList();
			if (!flag)
			{
				ListEntries.Add(SelfEntry);
				TotalItemCount = ListEntries.Count;
			}
		}
	}

	public void BuildForSettle()
	{
		using (new ProfilerScope("RankListDataBuilder.BuildForSettle"))
		{
			RankModelData rankModelData = LoadAndAddRobots();
			if (rankModelData == null)
			{
				return;
			}
			RankActivityService service = RankActivityEntry.Service;
			if (service != null)
			{
				int num = rankModelData.PlayerRankScore - rankModelData.CurrentSettleScore;
				if (rankModelData.Robots.Count < RankConst.MaxRankBot && num <= 0)
				{
					AllEntries.Add(CreatePlaceholderRobot(rankModelData));
				}
				SortByScoreDesc();
				int num2 = service.CalcRankingByScore(num);
				int score = num;
				if (num2 > 0 && num2 <= RankConst.MaxRankBot)
				{
					InsertPlayer(num2, score);
					TrimToMaxRankCount();
				}
				else
				{
					AllEntries.Add(CreatePlayerEntry(num2, score));
				}
				AssignRankIndices();
				SplitTopAndList();
				SelfEntry = AllEntries.Find((RankItemData e) => e.IsSelf);
			}
		}
	}

	private RankModelData LoadAndAddRobots()
	{
		using (new ProfilerScope("RankListDataBuilder.LoadAndAddRobots"))
		{
			AllEntries.Clear();
			ListEntries.Clear();
			TotalItemCount = 0;
			SelfEntry = null;
			RankModelProxy rankModelProxy = GameApp.Proxy.Get<RankModelProxy>();
			if (rankModelProxy?.Data?.Robots == null || rankModelProxy.Data.Robots.Count == 0)
			{
				return null;
			}
			RankModelData data = rankModelProxy.Data;
			ConfigManager config = GameApp.Config;
			ExampleTables exampleTables = ((config != null) ? config.GetTable<ExampleTables>() : null);
			if (exampleTables != null)
			{
				RankRobotNameHelper.RepairRobotsIfNeeded(data.Robots, exampleTables, data.RobotNameConfig, NameRepairRng);
			}
			for (int i = 0; i < data.Robots.Count; i++)
			{
				RankRobotData rankRobotData = data.Robots[i];
				if (rankRobotData != null)
				{
					AllEntries.Add(new RankItemData
					{
						Name = rankRobotData.Name,
						Collect = rankRobotData.Score,
						IsSelf = false,
						HeadId = rankRobotData.HeadId,
						HeadFrameId = rankRobotData.HeadFrameId
					});
				}
			}
			return data;
		}
	}

	private void SortByScoreDesc()
	{
		AllEntries.Sort((RankItemData a, RankItemData b) => b.Collect.CompareTo(a.Collect));
	}

	private static RankItemData CreatePlayerEntry(int rank, int score)
	{
		return new RankItemData
		{
			Name = RankPlayerService.GetName(),
			Collect = score,
			IsSelf = true,
			RankIndex = rank
		};
	}

	private void InsertPlayer(int rank, int score)
	{
		int index = Mathf.Clamp(rank - 1, 0, AllEntries.Count);
		AllEntries.Insert(index, CreatePlayerEntry(rank, score));
	}

	private void TrimToMaxRankCount()
	{
		int maxRankBot = RankConst.MaxRankBot;
		while (AllEntries.Count > maxRankBot)
		{
			AllEntries.RemoveAt(AllEntries.Count - 1);
		}
	}

	private void AssignRankIndices()
	{
		for (int i = 0; i < AllEntries.Count; i++)
		{
			AllEntries[i].RankIndex = i + 1;
		}
	}

	private void SplitTopAndList()
	{
		for (int i = 3; i < AllEntries.Count; i++)
		{
			ListEntries.Add(AllEntries[i]);
		}
		TotalItemCount = ListEntries.Count;
	}

	private static RankItemData CreatePlaceholderRobot(RankModelData data)
	{
		string name = "You";
		int headId = 0;
		int headFrameId = 0;
		if (data.Robots != null && data.Robots.Count > 0)
		{
			RankRobotData rankRobotData = data.Robots[Random.Range(0, data.Robots.Count)];
			name = rankRobotData.Name;
			headId = rankRobotData.HeadId;
			headFrameId = rankRobotData.HeadFrameId;
		}
		return new RankItemData
		{
			Name = name,
			Collect = 0,
			IsSelf = false,
			HeadId = headId,
			HeadFrameId = headFrameId
		};
	}
}
