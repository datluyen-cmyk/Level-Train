using System;
using System.Collections.Generic;
using Example.Cfg;
using Game.DataTrack;
using RotateRings.Ad;
using RotateRings.Prop;
using RotateRings.UI.ClaimReward;
using RotateRings.UI.Tips;
using Rotaterings.Rank;
using SuperScrollView;
using UnityEngine;
using UnityEngine.UI;
using XingYun.UI.Core;

namespace Rotaterings.UI.RankList;

public sealed class RankRewardFlow
{
	private const string Tag = "Rank";

	private const int SettleTopRankCount = 3;

	private const string BoxPrefabLocation = "eff_gamewin_box_01_gp1";

	private const float ButtonFeedbackDuration = 0.5f;

	private const float NormalRankRewardBoxFadeDuration = 0.1f;

	private readonly UIRankListView view;

	private readonly Func<ERankContent> getRankType;

	private readonly Func<RankTopHeader> getMainRankTopHeader;

	private readonly Vector3[] viewportCheckCorners = (Vector3[])(object)new Vector3[4];

	private Action pendingFlyCompletion;

	private int flyCompletionTimerId = -1;

	public RankRewardFlow(UIRankListView view, Func<ERankContent> getRankType, Func<RankTopHeader> getMainRankTopHeader)
	{
		this.view = view;
		this.getRankType = getRankType;
		this.getMainRankTopHeader = getMainRankTopHeader;
	}

	public void ShowRewardBoxPreview(int rank, Transform itemTransform)
	{
		ERankContent eRankContent = getRankType();
		if ((eRankContent != ERankContent.Main && eRankContent != ERankContent.Settle) || rank < 1 || rank > RankConst.CurrentRewardRankLimit)
		{
			return;
		}
		RectTransform val = (RectTransform)(object)((itemTransform is RectTransform) ? itemTransform : null);
		if (val == null)
		{
			return;
		}
		RewardConfig rankRewards = RewardService.GetRankRewards(rank);
		if (rankRewards?.get == null || rankRewards.get.Count == 0)
		{
			return;
		}
		PropModelProxy propModelProxy = GameApp.Proxy.Get<PropModelProxy>();
		List<ShowRewardItem> list = new List<ShowRewardItem>(rankRewards.get.Count);
		foreach (KeyValuePair<int, int> item in rankRewards.get)
		{
			if (item.Key > 0 && item.Value > 0)
			{
				EPropType ePropType = propModelProxy?.GetRewardPropType(item.Key) ?? EPropType.None;
				if (ePropType != EPropType.None)
				{
					list.Add(new ShowRewardItem
					{
						PropType = ePropType,
						Count = item.Value
					});
				}
			}
		}
		if (list.Count != 0)
		{
			TipsService.ShowChestRewards(val, list, null, UITips_CommonTips.TipsPlacement.Above);
		}
	}

	public void ShowClaimReward(Action onBeforeClose = null)
	{
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ca: Unknown result type (might be due to invalid IL or missing references)
		RankModelProxy rankProxy = GameApp.Proxy.Get<RankModelProxy>();
		int rewardId = RewardService.GetRewardId(rankProxy?.Data);
		if (rewardId <= 0)
		{
			rankProxy?.MarkPeriodSettled();
			return;
		}
		rankProxy?.SetEnteredRewardPage(entered: true);
		int num = RankActivityService.CalcPlayerRanking(rankProxy?.Data);
		RankItem rewardBoxItemForRank = GetRewardBoxItemForRank(num);
		Vector2 boxStartWorldPosition = (((Object)(object)rewardBoxItemForRank != (Object)null) ? rewardBoxItemForRank.GetRewardBoxWorldPosition() : GetRewardBoxFallbackPosition());
		FadeOutNormalRankRewardBox(rewardBoxItemForRank, num);
		Vector2 rewardPropFlyTargetPos = GetRewardPropFlyTargetPos();
		RRDataTrack.Instance.ReportBtnClick("排行榜-结算-奖励弹窗-弹窗展现");
		RewardService.PopupClaimRewardUI(new UIClaimRewardParam
		{
			BoxStartWorldPosition = boxStartWorldPosition,
			PropFlyTargetPos = rewardPropFlyTargetPos,
			BoxPrefabLocation = "eff_gamewin_box_01_gp1",
			EnableBoxFlyFadeIn = false,
			BoxAnimName = ((num <= 3) ? Mathf.Clamp(num + 1, 2, 4).ToString("D2") : "05"),
			RewardId = rewardId,
			RankRewardRank = num,
			IsRankReward = true,
			EnableClaimX2 = true,
			AdPopPosition = AdPopPosition.RankReward,
			OnAdEnterTrack = delegate
			{
				RRDataTrack.Instance.ReportAdRewardEnter(RRDataTrack.AdAction.Click, AdPopPosition.RankReward);
			},
			ClaimHandler = () => RewardService.ClaimReward(rankProxy?.Data),
			ClaimMultiplierHandler = (int multiplier, bool isFreeDoubleReward) => RewardService.ClaimReward(rankProxy?.Data, multiplier, isFreeDoubleReward),
			OnBeforeClose = delegate
			{
				HandleRewardFlyCompleted(onBeforeClose);
			},
			RewardReasonId = "排行榜活动奖励",
			RewardReasonDetail = $"第{num}名",
			OnClaimClickTrack = delegate
			{
				RRDataTrack.Instance.ReportBtnClick("排行榜-结算-奖励弹窗-普通领取");
			},
			OnClaimX2ClickTrack = delegate
			{
				RRDataTrack.Instance.ReportBtnClick("排行榜-结算-奖励弹窗-看广告领取");
			}
		});
	}

	public bool TryCompletePendingFly()
	{
		if (pendingFlyCompletion == null)
		{
			return false;
		}
		view.ResetBottomCommonEffect();
		CompletePendingFly();
		return true;
	}

	public void Cleanup()
	{
		if (flyCompletionTimerId >= 0)
		{
			GameApp.Timer.Kill(flyCompletionTimerId);
			flyCompletionTimerId = -1;
		}
		pendingFlyCompletion = null;
	}

	private Vector2 GetRewardPropFlyTargetPos()
	{
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		Button val = ((getRankType() == ERankContent.Settle) ? view.btnContinue : view.btnLevel);
		if ((Object)(object)val != (Object)null && ((Component)val).gameObject.activeInHierarchy)
		{
			return new Vector2(((Component)val).transform.position.x, ((Component)val).transform.position.y);
		}
		return new Vector2((float)Screen.width * 0.5f, (float)Screen.height * 0.5f);
	}

	private void HandleRewardFlyCompleted(Action onBeforeClose)
	{
		view.PlayBottomCommonEffect();
		if (onBeforeClose != null)
		{
			pendingFlyCompletion = onBeforeClose;
			flyCompletionTimerId = GameApp.Timer.Delay(0.5f, (Action)CompletePendingFly);
		}
	}

	private void CompletePendingFly()
	{
		Action action = pendingFlyCompletion;
		Cleanup();
		action?.Invoke();
	}

	private static Vector2 GetRewardBoxFallbackPosition()
	{
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		RectTransform val = UIRoot.Instance?.normalRect;
		if ((Object)(object)val != (Object)null)
		{
			return new Vector2(((Transform)val).position.x, ((Transform)val).position.y);
		}
		return Vector2.zero;
	}

	private RankItem GetRewardBoxItemForRank(int rank)
	{
		if (rank < 1)
		{
			return null;
		}
		if (rank <= 3)
		{
			return GetTopRewardBoxItem(rank);
		}
		if (getRankType() != ERankContent.Settle)
		{
			return GetMainListRewardBoxItem(rank);
		}
		return GetSettleListRewardBoxItem(rank);
	}

	private RankItem GetTopRewardBoxItem(int rank)
	{
		if (getRankType() == ERankContent.Settle)
		{
			LoopListView2 val = view?.SettlementLoopList;
			object header;
			if (val == null)
			{
				header = null;
			}
			else
			{
				LoopListViewItem2 shownItemByItemIndex = val.GetShownItemByItemIndex(0);
				header = ((shownItemByItemIndex != null) ? ((Component)shownItemByItemIndex).GetComponent<RankTopHeader>() : null);
			}
			RankItem topRankItem = GetTopRankItem((RankTopHeader)header, rank);
			if (!IsCurrentVisibleTopRewardBoxItem(topRankItem, rank, val))
			{
				return null;
			}
			return topRankItem;
		}
		return GetTopRankItem(getMainRankTopHeader(), rank);
	}

	private static RankItem GetTopRankItem(RankTopHeader header, int rank)
	{
		return rank switch
		{
			1 => header?.FirstRankItem, 
			2 => header?.SecondRankItem, 
			3 => header?.ThirdRankItem, 
			_ => null, 
		};
	}

	private RankItem GetSettleListRewardBoxItem(int rank)
	{
		int num = rank - 3;
		LoopListView2 val = view?.SettlementLoopList;
		LoopListViewItem2 val2 = ((val != null) ? val.GetShownItemByItemIndex(num) : null);
		RankItem rankItem = (((Object)(object)val2 != (Object)null) ? ((Component)val2).GetComponent<RankItem>() : null);
		if (!IsCurrentVisibleRewardBoxItem(rankItem, rank, val))
		{
			return null;
		}
		return rankItem;
	}

	private RankItem GetMainListRewardBoxItem(int rank)
	{
		int num = rank - 3 - 1;
		LoopListView2 val = view?.RankLoopList;
		LoopListViewItem2 val2 = ((val != null) ? val.GetShownItemByItemIndex(num) : null);
		RankItem rankItem = (((Object)(object)val2 != (Object)null) ? ((Component)val2).GetComponent<RankItem>() : null);
		if (!IsCurrentVisibleRewardBoxItem(rankItem, rank, val))
		{
			return null;
		}
		return rankItem;
	}

	private bool IsCurrentVisibleTopRewardBoxItem(RankItem rankItem, int rank, LoopListView2 list)
	{
		if ((Object)(object)rankItem == (Object)null || rankItem.RankIndex != rank || !((Component)rankItem).gameObject.activeInHierarchy)
		{
			return false;
		}
		return IsRectTransformVisibleInViewport(rankItem.RewardBoxRectTransform, list);
	}

	private bool IsCurrentVisibleRewardBoxItem(RankItem rankItem, int rank, LoopListView2 list)
	{
		if ((Object)(object)rankItem == (Object)null || rankItem.RankIndex != rank || !((Component)rankItem).gameObject.activeInHierarchy)
		{
			return false;
		}
		RectTransform rewardBoxRectTransform = rankItem.RewardBoxRectTransform;
		if ((Object)(object)rewardBoxRectTransform == (Object)null || !((Component)rewardBoxRectTransform).gameObject.activeInHierarchy)
		{
			return false;
		}
		LoopListViewItem2 component = ((Component)rankItem).GetComponent<LoopListViewItem2>();
		if ((Object)(object)component == (Object)null || (Object)(object)list == (Object)null || (Object)(object)list.GetShownItemByItemIndex(component.ItemIndex) != (Object)(object)component)
		{
			return false;
		}
		return IsRectTransformVisibleInViewport(rewardBoxRectTransform, list);
	}

	private bool IsRectTransformVisibleInViewport(RectTransform target, LoopListView2 list)
	{
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00be: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fe: Unknown result type (might be due to invalid IL or missing references)
		//IL_0104: Unknown result type (might be due to invalid IL or missing references)
		//IL_010d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0113: Unknown result type (might be due to invalid IL or missing references)
		//IL_011b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0121: Unknown result type (might be due to invalid IL or missing references)
		if (!((Object)(object)target == (Object)null))
		{
			RectTransform val = ((list != null) ? list.ViewPortTrans : null);
			if (val != null)
			{
				target.GetWorldCorners(viewportCheckCorners);
				Vector3 val2 = ((Transform)list.ContainerTrans).InverseTransformPoint(viewportCheckCorners[0]);
				Vector3 val3 = val2;
				for (int i = 1; i < viewportCheckCorners.Length; i++)
				{
					Vector3 val4 = ((Transform)list.ContainerTrans).InverseTransformPoint(viewportCheckCorners[i]);
					val2 = Vector3.Min(val2, val4);
					val3 = Vector3.Max(val3, val4);
				}
				val.GetWorldCorners(viewportCheckCorners);
				Vector3 val5 = ((Transform)list.ContainerTrans).InverseTransformPoint(viewportCheckCorners[0]);
				Vector3 val6 = val5;
				for (int j = 1; j < viewportCheckCorners.Length; j++)
				{
					Vector3 val7 = ((Transform)list.ContainerTrans).InverseTransformPoint(viewportCheckCorners[j]);
					val5 = Vector3.Min(val5, val7);
					val6 = Vector3.Max(val6, val7);
				}
				if (val3.x >= val5.x && val2.x <= val6.x && val3.y >= val5.y)
				{
					return val2.y <= val6.y;
				}
				return false;
			}
		}
		return false;
	}

	private static void FadeOutNormalRankRewardBox(RankItem rankItem, int rank)
	{
		if (rank > 3)
		{
			rankItem?.FadeOutRewardBox(0.1f);
		}
	}
}
