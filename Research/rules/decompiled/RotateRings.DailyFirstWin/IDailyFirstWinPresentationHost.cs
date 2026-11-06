using System.Collections.Generic;
using UnityEngine;

namespace RotateRings.DailyFirstWin;

public interface IDailyFirstWinPresentationHost
{
	string GetText(string key, params object[] args);

	Sprite LoadChestSprite(string tierName, string state);

	bool IsInHome();

	void EnterMainLevel();

	bool HasUnfinishedLevel(int playerLevel);

	bool IsHomeButtonEffectEnabled();

	bool IsDifficultLevel(int playerLevel);

	void ShowChestRewards(RectTransform target, List<RewardItemSnapshot> rewards);

	void CloseTips();

	int GetSettlementShowCoinCountForAd();

	void ReportBtnClick(string label);
}
