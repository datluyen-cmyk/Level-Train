using System;
using Game.DataTrack;
using RotateRings.Ad;
using UnityEngine;

namespace RotateRings.UI.ClaimReward;

public static class UIClaimRewardPresets
{
	private const string DailyFirstWinBoxPrefabLocation = "eff_first_win_box";

	public static UIClaimRewardParam CreateDailyFirstWin(int rewardId, int rewardStage, int milestoneDay, bool enableClaimX2, Func<bool> claimHandler, Func<int, bool, bool> claimMultiplierHandler, Action onClaimSucceeded, RectTransform collectTarget, RectTransform boxStartTarget, Action onOpen, Action onClosed = null, Action onFlyStart = null, Action onFirstPropArrive = null)
	{
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		return new UIClaimRewardParam
		{
			RewardId = rewardId,
			EnableClaimX2 = enableClaimX2,
			ClaimHandler = claimHandler,
			ClaimMultiplierHandler = claimMultiplierHandler,
			OnClaimSucceeded = onClaimSucceeded,
			OnOpen = onOpen,
			OnClosed = onClosed,
			OnFlyStart = onFlyStart,
			OnFirstPropArrive = onFirstPropArrive,
			BoxStartWorldPosition = GetWorldPosition((Component)(object)boxStartTarget),
			BoxPrefabLocation = "eff_first_win_box",
			BoxAnimName = ResolveBoxAnimName(milestoneDay),
			PropFlyTargetPos = GetScreenPosition((Component)(object)collectTarget),
			AdPopPosition = AdPopPosition.DailyFirstWinMultiReward,
			RewardReasonId = "每日首胜奖励",
			RewardReasonDetail = $"stage={rewardStage}",
			OnClaimClickTrack = delegate
			{
				RRDataTrack.Instance.ReportBtnClick("每日首胜-领奖按钮");
			},
			OnClaimX2ClickTrack = delegate
			{
				RRDataTrack.Instance.ReportBtnClick("每日首胜-多倍领奖");
			},
			OnAdEnterTrack = delegate
			{
				RRDataTrack.Instance.ReportAdRewardEnter(RRDataTrack.AdAction.Click, AdPopPosition.DailyFirstWinMultiReward);
			}
		};
	}

	private static string ResolveBoxAnimName(int milestoneDay)
	{
		if (milestoneDay != 7)
		{
			return "01";
		}
		return "02";
	}

	private static Vector2 GetWorldPosition(Component component)
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		if (!((Object)(object)component != (Object)null))
		{
			return Vector2.zero;
		}
		return new Vector2(component.transform.position.x, component.transform.position.y);
	}

	private static Vector2 GetScreenPosition(Component component)
	{
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		if (!((Object)(object)component == (Object)null))
		{
			return RectTransformUtility.WorldToScreenPoint((Camera)null, component.transform.position);
		}
		return Vector2.zero;
	}
}
