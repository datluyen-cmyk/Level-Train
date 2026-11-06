using System.Collections.Generic;
using Example.Cfg;
using RotateRings.Model;
using UnityEngine;
using XingYun.Experiment;
using XingYun.Proxy;

namespace RotateRings.Chest;

public class ChestProxy : BaseProxy
{
	private const string TAG = "[ChestProxy]";

	private static readonly List<int> FallbackProgress = new List<int> { 6, 6, 8, 10 };

	private const int FallbackLoopMode = 2;

	private const int RewardLoopMode = 1;

	private LevelModelData Gmd => GameApp.Save.GetOrCreate<LevelModelData>();

	public ChestProxy()
		: base("ChestProxy")
	{
	}

	private static ChestFunction GetConfig()
	{
		return FunctionDataCenter.Get<ChestFunction>() ?? new ChestFunction();
	}

	private static StageReward GetStageReward()
	{
		ChestFunction config = GetConfig();
		return GameApp.Config.GetTable<ExampleTables>()?.TbStageReward?.Get(config.StageRewardId);
	}

	public int GetUnlockLevel()
	{
		return GetConfig().UnlockLevel;
	}

	private static List<int> ResolveProgressList()
	{
		StageReward stageReward = GetStageReward();
		if (stageReward?.Progress != null && stageReward.Progress.Count > 0)
		{
			return stageReward.Progress;
		}
		return FallbackProgress;
	}

	private static int ResolveLoopMode()
	{
		StageReward stageReward = GetStageReward();
		if ((stageReward != null && stageReward.looptype == 1) || (stageReward != null && stageReward.looptype == 2))
		{
			return stageReward.looptype;
		}
		return 2;
	}

	public bool IsUnlocked(int currentLevel)
	{
		if (currentLevel >= GetConfig().UnlockLevel)
		{
			return GetStageReward() != null;
		}
		return false;
	}

	public bool IsDisabled(int currentLevel)
	{
		return !IsUnlocked(currentLevel);
	}

	public bool IsAccumulating()
	{
		if (Gmd.ChestCurrentCycleIndex >= 0)
		{
			return Gmd.ChestCurrentLevelProgress < GetCurrentThreshold();
		}
		return false;
	}

	public bool IsClaimable()
	{
		if (Gmd.ChestCurrentCycleIndex >= 0 && Gmd.ChestCurrentLevelProgress >= GetCurrentThreshold())
		{
			return !IsCycleClaimed(Gmd.ChestCurrentCycleIndex);
		}
		return false;
	}

	public bool IsClaimed()
	{
		if (Gmd.ChestCurrentCycleIndex >= 0)
		{
			return IsCycleClaimed(Gmd.ChestCurrentCycleIndex);
		}
		return false;
	}

	public int GetCurrentCycleIndex()
	{
		return Gmd.ChestCurrentCycleIndex;
	}

	public int GetCurrentThreshold()
	{
		List<int> list = ResolveProgressList();
		int idx = ((Gmd.ChestCurrentCycleIndex >= 0) ? Gmd.ChestCurrentCycleIndex : 0);
		return list[ResolveIndex(idx, list.Count, ResolveLoopMode())];
	}

	public int GetProgressTotalCount()
	{
		return GetCurrentThreshold();
	}

	public int GetProgressLitCount()
	{
		return Mathf.Clamp(Gmd.ChestCurrentLevelProgress, 0, GetCurrentThreshold());
	}

	public int GetAnimatingIndex()
	{
		int progressLitCount = GetProgressLitCount();
		int progressTotalCount = GetProgressTotalCount();
		return Mathf.Clamp(progressLitCount - 1, 0, Mathf.Max(0, progressTotalCount - 1));
	}

	public int GetLevelsLeft()
	{
		return Mathf.Max(0, GetCurrentThreshold() - GetProgressLitCount());
	}

	public int GetCurrentRewardConfigId()
	{
		List<int> list = GetStageReward()?.Reward;
		if (list == null || list.Count == 0)
		{
			return 1;
		}
		int idx = ((Gmd.ChestCurrentCycleIndex >= 0) ? Gmd.ChestCurrentCycleIndex : 0);
		return list[ResolveIndex(idx, list.Count, 1)];
	}

	public int GetUiPosition()
	{
		return GetConfig().UI;
	}

	public List<int> GetProgressList()
	{
		return ResolveProgressList();
	}

	public int GetLoopMode()
	{
		return ResolveLoopMode();
	}

	public bool IsCycleClaimed(int cycleIndex)
	{
		if (cycleIndex >= 0)
		{
			return cycleIndex <= Gmd.LastClaimedCycleIndex;
		}
		return false;
	}

	public void OnLevelWin(int newLevel)
	{
		if (IsUnlocked(newLevel))
		{
			if (Gmd.ChestCurrentCycleIndex < 0)
			{
				Gmd.ChestCurrentCycleIndex = 0;
				Gmd.ChestCurrentLevelProgress = 1;
				((BaseProxy)this).SendEvent<ChestUnlocked>(new ChestUnlocked
				{
					Level = newLevel
				});
			}
			else
			{
				Gmd.ChestCurrentLevelProgress++;
			}
			((BaseProxy)this).SendEvent<ChestProgressChanged>(new ChestProgressChanged
			{
				CycleIndex = Gmd.ChestCurrentCycleIndex,
				LevelProgress = Gmd.ChestCurrentLevelProgress,
				Thresholds = GetCurrentThreshold()
			});
			if (Gmd.ChestCurrentLevelProgress >= GetCurrentThreshold())
			{
				((BaseProxy)this).SendEvent<ChestClaimable>(new ChestClaimable
				{
					CycleIndex = Gmd.ChestCurrentCycleIndex,
					RewardConfigId = GetCurrentRewardConfigId()
				});
			}
		}
	}

	public void MarkClaimed(bool isAutoClaim = false)
	{
		int chestCurrentCycleIndex = Gmd.ChestCurrentCycleIndex;
		if (chestCurrentCycleIndex >= 0)
		{
			if (chestCurrentCycleIndex > Gmd.LastClaimedCycleIndex)
			{
				Gmd.LastClaimedCycleIndex = chestCurrentCycleIndex;
			}
			int currentRewardConfigId = GetCurrentRewardConfigId();
			Gmd.ChestCurrentCycleIndex = chestCurrentCycleIndex + 1;
			Gmd.ChestCurrentLevelProgress = 0;
			((BaseProxy)this).SendEvent<ChestClaimed>(new ChestClaimed
			{
				CycleIndex = chestCurrentCycleIndex,
				RewardConfigId = currentRewardConfigId,
				IsAutoClaim = isAutoClaim
			});
		}
	}

	public List<int> GetPendingAutoClaimCycles()
	{
		List<int> list = new List<int>();
		for (int i = 0; i < Gmd.ChestCurrentCycleIndex; i++)
		{
			if (i > Gmd.LastClaimedCycleIndex)
			{
				list.Add(i);
			}
		}
		return list;
	}

	public bool HasPendingClaimable()
	{
		return IsClaimable();
	}

	private static int ResolveIndex(int idx, int length, int loopMode)
	{
		if (length <= 0)
		{
			return 0;
		}
		if (loopMode == 2)
		{
			return Mathf.Clamp(idx, 0, length - 1);
		}
		return (idx % length + length) % length;
	}
}
