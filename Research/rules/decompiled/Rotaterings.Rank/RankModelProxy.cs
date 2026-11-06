using UnityEngine;
using XingYun.Proxy;

namespace Rotaterings.Rank;

public class RankModelProxy : BaseProxy
{
	private const string Tag = "Rank";

	private const int MaxCollectCount = 3;

	public RankModelData Data { get; }

	public EnsureResult CurLevelRankState { get; set; }

	public RankModelProxy()
		: base("RankModelProxy")
	{
		Data = GameApp.Save.GetOrCreate<RankModelData>();
	}

	public int GetCurrentCollectCount()
	{
		return Data.CurrentCollectCount;
	}

	private void ResetCurrentCollectCount()
	{
		Data.CurrentCollectCount = 0;
	}

	public (bool success, int collectCount) AddCurrentCollectCount()
	{
		if (!IsRankActiveThisRound())
		{
			return (success: false, collectCount: Data.CurrentCollectCount);
		}
		Data.CurrentCollectCount++;
		return (success: true, collectCount: Data.CurrentCollectCount);
	}

	public int CalculateCollectCount(int collectableCount)
	{
		return Mathf.Min(collectableCount, 3);
	}

	public void InitRankStarOnLevelLoad(bool isStartGame, int collectableCount)
	{
		ResolveCurLevelRankState(isStartGame);
		if (!IsRankActiveThisRound())
		{
			ResetCurrentCollectCount();
			SetStarCount(0);
		}
		else if (isStartGame)
		{
			InitializeRoundCollectState(collectableCount);
		}
	}

	private void ResolveCurLevelRankState(bool isNewGame)
	{
		RankActivityService service = RankActivityEntry.Service;
		service?.TryResetRankStateIfBelowOpenLevel();
		CurLevelRankState = service?.GetActivityState() ?? EnsureResult.None;
		if (isNewGame)
		{
			bool flag = CurLevelRankState == EnsureResult.None && (service?.CanOpenRank() ?? false);
			if (CurLevelRankState == EnsureResult.OpenedNew || flag)
			{
				RankActivityEntry.EnsureBuildActivityData();
				CurLevelRankState = RankActivityEntry.Service?.GetActivityState() ?? EnsureResult.None;
			}
		}
	}

	private void InitializeRoundCollectState(int collectableCount)
	{
		ResetCurrentCollectCount();
		int starCount = CalculateCollectCount(collectableCount);
		SetStarCount(starCount);
	}

	private void SetStarCount(int starCountOneRound)
	{
		Data.StarCountOneRound = starCountOneRound;
	}

	public void RestartGame()
	{
		RankScoreService.ResetMultiplierIfActive();
	}

	public void SetClaimState(bool isClaim)
	{
		Data.IsClaimReward = isClaim;
		if (isClaim)
		{
			Data.IsPeriodSettled = true;
			RankActivityEntry.Service?.SetNextOpenTime();
		}
	}

	public void MarkPeriodSettled()
	{
		Data.IsPeriodSettled = true;
		RankActivityEntry.Service?.SetNextOpenTime();
	}

	public void SetEnteredRewardPage(bool entered)
	{
		Data.HasEnteredRewardPage = entered;
		GameApp.Save.SaveAll();
	}

	public bool IsRankActiveThisRound()
	{
		return CurLevelRankState == EnsureResult.Ongoing;
	}
}
