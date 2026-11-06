using System;
using System.Collections.Generic;
using Game.DataTrack;
using RotateRings.DailyFirstWin;
using RotateRings.GamePlay.Save;
using RotateRings.Prop;
using RotateRings.UI.Level;
using RotateRings.UI.Tips;
using Script.Tools;
using UnityEngine;

namespace RotateRings.UI.DailyFirstWin;

public sealed class DailyFirstWinPresentationHostImpl : IDailyFirstWinPresentationHost
{
	public string GetText(string key, params object[] args)
	{
		return GameApp.Lang.GetText(key, args);
	}

	public Sprite LoadChestSprite(string tierName, string state)
	{
		return SpriteManager.LoadSprite("firstwinl_chest_" + tierName + "_" + state);
	}

	public bool IsInHome()
	{
		return GameApp.UI.IsOpen("UIHome");
	}

	public void EnterMainLevel()
	{
		GameApp.Command.Execute<EnterLevelCommand>((object)new EnterLevelParam
		{
			SkipHomeCloseAnimation = true
		});
	}

	public bool HasUnfinishedLevel(int playerLevel)
	{
		if (playerLevel > 0)
		{
			return GameApp.Proxy.Get<RingLevelSaveProxy>().HasValidSave(playerLevel);
		}
		return false;
	}

	public bool IsHomeButtonEffectEnabled()
	{
		return false;
	}

	public bool IsDifficultLevel(int playerLevel)
	{
		return false;
	}

	public void ShowChestRewards(RectTransform target, List<RewardItemSnapshot> rewards)
	{
		List<ShowRewardItem> rewards2 = ConvertRewardItems(rewards);
		TipsService.ShowChestRewards(target, rewards2);
	}

	public void CloseTips()
	{
		TipsService.CloseIfOpen();
	}

	private static List<ShowRewardItem> ConvertRewardItems(List<RewardItemSnapshot> rewards)
	{
		if (rewards == null || rewards.Count == 0)
		{
			return null;
		}
		List<ShowRewardItem> list = new List<ShowRewardItem>(rewards.Count);
		for (int i = 0; i < rewards.Count; i++)
		{
			RewardItemSnapshot rewardItemSnapshot = rewards[i];
			if (rewardItemSnapshot.Count > 0)
			{
				list.Add(new ShowRewardItem
				{
					PropType = (EPropType)rewardItemSnapshot.PropType,
					Count = rewardItemSnapshot.Count
				});
			}
		}
		if (list.Count <= 0)
		{
			return null;
		}
		return list;
	}

	public int GetSettlementShowCoinCountForAd()
	{
		int propCount = GameApp.Proxy.Get<PropModelProxy>().GetPropCount(EPropType.Coin);
		return Math.Max(0, propCount - 10);
	}

	public void ReportBtnClick(string label)
	{
		RRDataTrack.Instance.ReportBtnClick(label);
	}
}
