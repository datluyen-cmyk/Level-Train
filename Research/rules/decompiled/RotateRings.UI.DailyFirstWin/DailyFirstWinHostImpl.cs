using System;
using System.Collections.Generic;
using Example.Cfg;
using Game.DataTrack;
using RotateRings.DailyFirstWin;
using RotateRings.Halloween;
using RotateRings.Model;
using RotateRings.Prop;
using RotateRings.UI.ClaimReward;
using UnityEngine;
using XingYun.Experiment;
using XingYun.UI.Core;
using XingYun.Utility;

namespace RotateRings.UI.DailyFirstWin;

public sealed class DailyFirstWinHostImpl : IDailyFirstWinHost
{
	private const string TAG = "[DailyFirstWinHostImpl] ";

	public DailyFirstWinConfigSnapshot? GetConfig()
	{
		TbDailyFirstWinConfig tbDailyFirstWinConfig = GameApp.Config.GetTable<ExampleTables>()?.TbDailyFirstWinConfig;
		if (tbDailyFirstWinConfig == null || tbDailyFirstWinConfig.TableRowCount == 0)
		{
			return null;
		}
		int periodId = FunctionDataCenter.Get<DailyFirstWinFunction>().PeriodId;
		if (periodId == 0)
		{
			return null;
		}
		DailyFirstWinConfig orDefault = tbDailyFirstWinConfig.GetOrDefault(periodId);
		if (orDefault == null)
		{
			return null;
		}
		return new DailyFirstWinConfigSnapshot
		{
			PeriodId = orDefault.Id,
			OpenLevel = orDefault.OpenLevel,
			RefreshTime = orDefault.TrigleCheckTime,
			Period = orDefault.Period,
			MilestoneDays = ParseIntArray(orDefault.Days),
			Rewards = ParseRewardEntries(orDefault.Rewards)
		};
	}

	public bool IsExperimentPuzzleEntryMode()
	{
		return string.Equals(FunctionDataCenter.Get<DailyFirstWinFunction>().EntryPlayMode, "puzzle", StringComparison.OrdinalIgnoreCase);
	}

	public bool IsHalloweenFeatureEnabled()
	{
		return HalloweenFeatureService.IsFirstWinEnabled();
	}

	public bool IsHalloweenPuzzleUIAvailable()
	{
		return GameApp.Res.CheckLocationValid("UIDailyFirstWinPuzzleHW");
	}

	public DailyFirstWinPuzzleConfig GetPuzzleConfig(int puzzleId)
	{
		return GameApp.Config.GetTable<ExampleTables>()?.TbDailyFirstWinPuzzleConfig?.GetOrDefault(puzzleId);
	}

	public DailyFirstWinPuzzleRotation GetPuzzleRotation(DailyFirstWinPuzzleTheme theme)
	{
		TbDailyFirstWinPuzzleRotation tbDailyFirstWinPuzzleRotation = GameApp.Config.GetTable<ExampleTables>()?.TbDailyFirstWinPuzzleRotation;
		if (tbDailyFirstWinPuzzleRotation?.DataList == null)
		{
			return null;
		}
		string b = theme.ToString();
		for (int i = 0; i < tbDailyFirstWinPuzzleRotation.DataList.Count; i++)
		{
			DailyFirstWinPuzzleRotation dailyFirstWinPuzzleRotation = tbDailyFirstWinPuzzleRotation.DataList[i];
			if (dailyFirstWinPuzzleRotation != null && string.Equals(dailyFirstWinPuzzleRotation.Theme, b, StringComparison.OrdinalIgnoreCase))
			{
				return dailyFirstWinPuzzleRotation;
			}
		}
		return null;
	}

	public IReadOnlyList<int> GetEffectivePuzzlePoolIds()
	{
		return GetEffectivePuzzlePoolIds(DailyFirstWinPuzzleTheme.Normal);
	}

	public IReadOnlyList<int> GetHalloweenPuzzlePoolIds()
	{
		return GetEffectivePuzzlePoolIds(DailyFirstWinPuzzleTheme.Halloween);
	}

	private IReadOnlyList<int> GetEffectivePuzzlePoolIds(DailyFirstWinPuzzleTheme theme)
	{
		DailyFirstWinPuzzleRotation puzzleRotation = GetPuzzleRotation(theme);
		if (puzzleRotation == null)
		{
			return Array.Empty<int>();
		}
		return BuildEffectivePuzzlePoolIds(puzzleRotation, GetAllPuzzleConfigIdsSorted());
	}

	private static List<int> GetAllPuzzleConfigIdsSorted()
	{
		TbDailyFirstWinPuzzleConfig tbDailyFirstWinPuzzleConfig = GameApp.Config.GetTable<ExampleTables>()?.TbDailyFirstWinPuzzleConfig;
		if (tbDailyFirstWinPuzzleConfig != null && tbDailyFirstWinPuzzleConfig.TableRowCount > 0 && tbDailyFirstWinPuzzleConfig.DataMap != null)
		{
			List<int> list = new List<int>(tbDailyFirstWinPuzzleConfig.DataMap.Keys);
			list.Sort();
			return list;
		}
		return new List<int> { 1, 2, 3, 4 };
	}

	private static List<int> BuildEffectivePuzzlePoolIds(DailyFirstWinPuzzleRotation rotation, IReadOnlyList<int> allPuzzleIdsSorted)
	{
		List<int> list = new List<int>();
		if (allPuzzleIdsSorted == null || allPuzzleIdsSorted.Count == 0)
		{
			return list;
		}
		HashSet<int> hashSet = new HashSet<int>(allPuzzleIdsSorted);
		IReadOnlyList<int> readOnlyList = rotation?.PoolIds;
		if (readOnlyList != null && readOnlyList.Count > 0)
		{
			for (int i = 0; i < readOnlyList.Count; i++)
			{
				int item = readOnlyList[i];
				if (hashSet.Contains(item))
				{
					list.Add(item);
				}
			}
			list.Sort();
		}
		else
		{
			for (int j = 0; j < allPuzzleIdsSorted.Count; j++)
			{
				list.Add(allPuzzleIdsSorted[j]);
			}
		}
		if (list.Count == 0)
		{
			return list;
		}
		int num = rotation?.PuzzlesPerCycle ?? 0;
		int val = ((num <= 0) ? list.Count : num);
		val = Math.Min(val, list.Count);
		if (val < list.Count)
		{
			list = list.GetRange(0, val);
		}
		return list;
	}

	public long GetCurrentTimestamp()
	{
		return Time.GetCurrentTimestamp();
	}

	public bool IsSameDay(long ts1, long ts2)
	{
		return Time.IsSameDay(ts1, ts2);
	}

	public int GetDaysBetween(long startTs, long endTs)
	{
		return Time.GetDaysBetween(startTs, endTs);
	}

	public int GetPlayerCompletedMainLevel()
	{
		int mainLevel = GameApp.Proxy.Get<LevelModelProxy>().MainLevel;
		return Math.Max(0, mainLevel - 1);
	}

	public bool GrantReward(int rewardConfigId, int multiplier, string reasonId, string reasonInfo, bool isFreeDoubleReward)
	{
		RewardConfig rewardConfig = GetRewardConfig(rewardConfigId);
		if (rewardConfig?.get == null || rewardConfig.get.Count == 0)
		{
			return false;
		}
		PropModelProxy propModelProxy = GameApp.Proxy.Get<PropModelProxy>();
		int num = Math.Max(1, multiplier);
		foreach (KeyValuePair<int, int> item in rewardConfig.get)
		{
			if (item.Value > 0)
			{
				EPropType key = (EPropType)item.Key;
				int addCount = item.Value * num;
				string customReasonInfo = ((multiplier <= 1) ? reasonInfo : (isFreeDoubleReward ? (reasonInfo + ",免费翻倍") : $"{reasonInfo},mult={multiplier}"));
				propModelProxy.AddProp(key, addCount, TrackPropGainReason.DailyFirstWinReward, reasonId, customReasonInfo);
			}
		}
		return true;
	}

	public List<RewardItemSnapshot> BuildRewardItems(int rewardConfigId)
	{
		List<RewardItemSnapshot> list = new List<RewardItemSnapshot>();
		RewardConfig rewardConfig = GetRewardConfig(rewardConfigId);
		if (rewardConfig?.get == null)
		{
			return list;
		}
		foreach (KeyValuePair<int, int> item in rewardConfig.get)
		{
			if (item.Value > 0)
			{
				list.Add(new RewardItemSnapshot
				{
					PropType = item.Key,
					Count = item.Value
				});
			}
		}
		return list;
	}

	public int SumRewardCoinCount(int rewardConfigId)
	{
		RewardConfig rewardConfig = GetRewardConfig(rewardConfigId);
		if (rewardConfig?.get == null)
		{
			return 0;
		}
		int num = 0;
		foreach (KeyValuePair<int, int> item in rewardConfig.get)
		{
			if (item.Key == 101 && item.Value > 0)
			{
				num += item.Value;
			}
		}
		return num;
	}

	public bool OpenRewardPopup(int rewardConfigId, Action onClosed)
	{
		onClosed?.Invoke();
		return false;
	}

	public bool OpenClaimRewardUI(int rewardConfigId, int rewardStage, int milestoneDay, bool enableMultiplier, Func<bool> claimHandler, Func<int, bool, bool> claimMultiplierHandler, Action onClaimSucceeded, RectTransform collectTarget, RectTransform boxStartTarget, Action onOpen, Action onFlyStart = null, Action onFirstPropArrive = null)
	{
		GameApp.UI.Open("UIClaimReward", (UILayer)2, (object)UIClaimRewardPresets.CreateDailyFirstWin(rewardConfigId, rewardStage, milestoneDay, enableMultiplier, claimHandler, claimMultiplierHandler, onClaimSucceeded, collectTarget, boxStartTarget, onOpen, null, onFlyStart, onFirstPropArrive), (Action)null);
		return true;
	}

	public int GetCurrentCoin()
	{
		return GameApp.Proxy.Get<PropModelProxy>().GetPropCount(EPropType.Coin);
	}

	public void NotifyCoinChanged()
	{
		GameApp.Event.Send<PropChangeEvent>(new PropChangeEvent
		{
			PropType = EPropType.Coin,
			DeltaCount = 0
		});
	}

	public void NotifyAddShowCoin(int count)
	{
		GameApp.Event.Send<PropNumberAniEvent>(new PropNumberAniEvent
		{
			PropType = EPropType.Coin,
			lastNumber = GetCurrentCoin() - count,
			currentNumber = GetCurrentCoin(),
			duration = 0.5f
		});
	}

	public void ReportBtnClick(string label)
	{
		RRDataTrack.Instance.ReportBtnClick(label);
	}

	private static RewardConfig GetRewardConfig(int id)
	{
		return GameApp.Config.GetTable<ExampleTables>()?.TbRewardConfig?.GetOrDefault(id);
	}

	private static int[] ParseIntArray(string s)
	{
		if (string.IsNullOrWhiteSpace(s))
		{
			return Array.Empty<int>();
		}
		string[] array = s.Split(',');
		int[] array2 = new int[array.Length];
		for (int i = 0; i < array.Length; i++)
		{
			if (!int.TryParse(array[i].Trim(), out array2[i]))
			{
				array2[i] = 0;
			}
		}
		return array2;
	}

	private static RewardEntrySnapshot[] ParseRewardEntries(string s)
	{
		if (string.IsNullOrWhiteSpace(s))
		{
			return Array.Empty<RewardEntrySnapshot>();
		}
		string[] array = s.Split(',');
		RewardEntrySnapshot[] array2 = new RewardEntrySnapshot[array.Length];
		for (int i = 0; i < array.Length; i++)
		{
			string[] array3 = array[i].Trim().Split('|');
			int.TryParse(array3[0].Trim(), out var result);
			bool isMultiplierMotivated = array3.Length > 1 && array3[1].Trim() == "1";
			array2[i] = new RewardEntrySnapshot
			{
				RewardConfigId = result,
				IsMultiplierMotivated = isMultiplierMotivated
			};
		}
		return array2;
	}
}
