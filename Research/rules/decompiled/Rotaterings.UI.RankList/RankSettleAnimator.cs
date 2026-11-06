using System;
using System.Collections.Generic;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Core.PathCore;
using DG.Tweening.Plugins.Options;
using RotateRings.Common;
using RotateRings.Services;
using Rotaterings.Rank;
using Rotaterings.SO;
using Rotaterings.Tools;
using SuperScrollView;
using TMPro;
using UnityEngine;
using Vibration;

namespace Rotaterings.UI.RankList;

public class RankSettleAnimator
{
	private const int SettleTopRankCount = 3;

	private const int MaxCollectionFlyCount = 9;

	private const string CollectReceiveAnimName = "anim_rank_collect_receive_gp1";

	private const float CollectReceiveHideDelay = 0.1f;

	private const string CollectFlyImageName = "Image";

	private const string CollectFlyEffectName = "eff_rank_fly_xiangguo_2";

	private const string CollectFed2AnimName = "anim_rank_collect_fed2_gp1";

	private const string CollectFed2TailEffectName = "eff_rank_tail_02_gp1";

	private const string SettleRankItemPrefabName = "RankItem";

	private const float PostAnimationDelay = 0.5f;

	private const float UnchangedTopThreeAnimationDelay = 0.4f;

	private const float CaidaiShowDelayAfterTop3DownStart = 0.28f;

	private const float CaidaiHideDelay = 5f;

	private readonly IRankSettleAnimatorContext context;

	private readonly List<Tween> tweens = new List<Tween>();

	private readonly List<int> timerIds = new List<int>();

	private readonly List<GameObject> collectFlyPool = new List<GameObject>(9);

	private readonly List<GameObject> activeFlyClones = new List<GameObject>(9);

	private readonly List<GameObject> activeRankClones = new List<GameObject>(2);

	private RectTransform temporaryHeader;

	private Transform temporaryHeaderOriginalParent;

	private Vector2 temporaryHeaderAnchoredPosition;

	private Vector3 temporaryHeaderLocalScale;

	private Quaternion temporaryHeaderLocalRotation;

	private TMP_Text[] streakTxts;

	private Tween collectionShowTween;

	private bool isSettlementCompleted;

	private UIRankListView View => context.View;

	private RankListDataBuilder DataBuilder => context.DataBuilder;

	private int SettleTotalCount
	{
		get
		{
			return context.SettleTotalCount;
		}
		set
		{
			context.SettleTotalCount = value;
		}
	}

	public RankSettleAnimator(IRankSettleAnimatorContext context)
	{
		this.context = context;
	}

	public void PlayCollectionShow(int targetIdx)
	{
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fb: Expected O, but got Unknown
		//IL_012b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0135: Expected O, but got Unknown
		//IL_014a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0154: Expected O, but got Unknown
		if (streakTxts == null)
		{
			streakTxts = GetTierTexts();
		}
		if (targetIdx < 0 || targetIdx >= streakTxts.Length)
		{
			return;
		}
		TMP_Text val = streakTxts[targetIdx];
		if ((Object)(object)val == (Object)null || (Object)(object)View.GoCollection == (Object)null)
		{
			return;
		}
		View.GoCollection.transform.SetAsLastSibling();
		View.GoCollection.transform.position = val.transform.position;
		RankListConfig cfg = RankListConfig.Instance;
		Tween obj = collectionShowTween;
		if (obj != null)
		{
			TweenExtensions.Kill(obj, false);
		}
		Sequence val2 = (Sequence)(object)(collectionShowTween = (Tween)(object)DOTween.Sequence());
		tweens.Add((Tween)(object)val2);
		TweenSettingsExtensions.AppendInterval(val2, ((Object)(object)cfg != (Object)null) ? cfg.delayShowStar : 0.2f);
		TweenSettingsExtensions.AppendCallback(val2, (TweenCallback)delegate
		{
			if ((Object)(object)View.imgCollection != (Object)null)
			{
				((Component)View.imgCollection).gameObject.SetActive(true);
			}
		});
		TweenSettingsExtensions.AppendInterval(val2, ((Object)(object)cfg != (Object)null) ? cfg.delayShowScore : 0.2f);
		TweenSettingsExtensions.AppendCallback(val2, (TweenCallback)delegate
		{
			if ((Object)(object)View.AddScore != (Object)null)
			{
				((Component)View.AddScore).gameObject.SetActive(true);
			}
			AudioService.PlayUiSound(AudioKeys.RankUpScoreBegin.Value);
		});
		TweenSettingsExtensions.AppendInterval(val2, 0.5f);
		TweenSettingsExtensions.AppendCallback(val2, (TweenCallback)delegate
		{
			CollectionFlyToTarget(cfg);
		});
	}

	public void PlayZeroScoreSettlement()
	{
		OnSettleAnimationCompleted();
	}

	public void SkipToSettlementEnd()
	{
		StopPendingPresentation();
		RestoreTransientVisuals();
		if (!isSettlementCompleted)
		{
			int num = (GameApp.Proxy.Get<RankModelProxy>()?.Data)?.CurrentSettleRanking ?? 0;
			if (num > 0 && DataBuilder?.AllEntries != null && DataBuilder.AllEntries.Count > 0)
			{
				FinalizeSettle(num, moveSelf: true);
			}
			CompleteSettlement();
		}
	}

	public void Cleanup()
	{
		StopPendingPresentation();
		for (int i = 0; i < collectFlyPool.Count; i++)
		{
			if ((Object)(object)collectFlyPool[i] != (Object)null)
			{
				Object.Destroy((Object)(object)collectFlyPool[i]);
			}
		}
		collectFlyPool.Clear();
		for (int j = 0; j < activeFlyClones.Count; j++)
		{
			if ((Object)(object)activeFlyClones[j] != (Object)null)
			{
				Object.Destroy((Object)(object)activeFlyClones[j]);
			}
		}
		activeFlyClones.Clear();
		DestroyActiveRankClones();
		RestoreTemporaryHeaderParent();
	}

	private void MoveHeaderToTemporaryParent(RectTransform header, Transform temporaryParent)
	{
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		if (!((Object)(object)header == (Object)null) && !((Object)(object)temporaryParent == (Object)null) && !((Object)(object)((Transform)header).parent == (Object)null))
		{
			temporaryHeader = header;
			temporaryHeaderOriginalParent = ((Transform)header).parent;
			temporaryHeaderAnchoredPosition = header.anchoredPosition;
			temporaryHeaderLocalScale = ((Transform)header).localScale;
			temporaryHeaderLocalRotation = ((Transform)header).localRotation;
			((Transform)header).SetParent(temporaryParent, true);
		}
	}

	private void RestoreTemporaryHeaderParent()
	{
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)temporaryHeader != (Object)null && (Object)(object)temporaryHeaderOriginalParent != (Object)null)
		{
			((Transform)temporaryHeader).SetParent(temporaryHeaderOriginalParent, false);
			temporaryHeader.anchoredPosition = temporaryHeaderAnchoredPosition;
			((Transform)temporaryHeader).localScale = temporaryHeaderLocalScale;
			((Transform)temporaryHeader).localRotation = temporaryHeaderLocalRotation;
		}
		temporaryHeader = null;
		temporaryHeaderOriginalParent = null;
	}

	private void StopPendingPresentation()
	{
		for (int i = 0; i < timerIds.Count; i++)
		{
			GameApp.Timer.Kill(timerIds[i]);
		}
		timerIds.Clear();
		Tween obj = collectionShowTween;
		if (obj != null)
		{
			TweenExtensions.Kill(obj, false);
		}
		collectionShowTween = null;
		for (int j = 0; j < tweens.Count; j++)
		{
			Tween obj2 = tweens[j];
			if (obj2 != null)
			{
				TweenExtensions.Kill(obj2, false);
			}
		}
		tweens.Clear();
	}

	private void RestoreTransientVisuals()
	{
		if ((Object)(object)View.imgCollection != (Object)null)
		{
			((Component)View.imgCollection).gameObject.SetActive(false);
		}
		if ((Object)(object)View.AddScore != (Object)null)
		{
			((Component)View.AddScore).gameObject.SetActive(false);
		}
		View.HideCaidaiObj();
		for (int i = 0; i < activeFlyClones.Count; i++)
		{
			if ((Object)(object)activeFlyClones[i] != (Object)null)
			{
				Object.Destroy((Object)(object)activeFlyClones[i]);
			}
		}
		activeFlyClones.Clear();
		DestroyActiveRankClones();
		RestoreTemporaryHeaderParent();
		LoopListView2 settlementLoopList = View.SettlementLoopList;
		LoopListViewItem2 obj = ((settlementLoopList != null) ? settlementLoopList.GetShownItemByItemIndex(0) : null);
		RankTopHeader rankTopHeader = ((obj != null) ? ((Component)obj).GetComponent<RankTopHeader>() : null);
		if ((Object)(object)rankTopHeader != (Object)null)
		{
			rankTopHeader.HideAllRankEffects();
			rankTopHeader.FirstRankItem?.ResetHeadRoot();
			rankTopHeader.SecondRankItem?.ResetHeadRoot();
			rankTopHeader.ThirdRankItem?.ResetHeadRoot();
		}
		LoopListView2 settlementLoopList2 = View.SettlementLoopList;
		if ((Object)(object)settlementLoopList2 == (Object)null)
		{
			return;
		}
		for (int j = 0; j < settlementLoopList2.ShownItemCount; j++)
		{
			LoopListViewItem2 shownItemByIndexWithoutCheck = settlementLoopList2.GetShownItemByIndexWithoutCheck(j);
			RankItem rankItem = ((shownItemByIndexWithoutCheck != null) ? ((Component)shownItemByIndexWithoutCheck).GetComponent<RankItem>() : null);
			if (!((Object)(object)rankItem == (Object)null))
			{
				rankItem.ResetHeadRoot();
				rankItem.SetVisualOffsetY(0f);
				rankItem.SetVisible(visible: true);
			}
		}
	}

	private void DestroyActiveRankClones()
	{
		for (int i = 0; i < activeRankClones.Count; i++)
		{
			if ((Object)(object)activeRankClones[i] != (Object)null)
			{
				activeRankClones[i].GetComponent<RankItem>()?.ResetHeadRoot();
				activeRankClones[i].SetActive(false);
				Object.Destroy((Object)(object)activeRankClones[i]);
			}
		}
		activeRankClones.Clear();
	}

	private void CompleteSettlement()
	{
		RestoreTemporaryHeaderParent();
		if (!isSettlementCompleted)
		{
			isSettlementCompleted = true;
			context.OnSettleAnimationCompleted();
		}
	}

	private TMP_Text[] GetTierTexts()
	{
		return (TMP_Text[])(object)new TMP_Text[5] { View.txtX1, View.txtX2, View.txtX3, View.txtX4, View.txtX5 };
	}

	private int GetSelfDataIndex()
	{
		return context.GetSelfDataIndex();
	}

	private int GetSettleLoopItemCount()
	{
		return context.GetSettleLoopItemCount();
	}

	private int SettleDataIndexToLoopIndex(int dataIndex)
	{
		return context.SettleDataIndexToLoopIndex(dataIndex);
	}

	private RankItem GetSettleRankItem(LoopListViewItem2 listItem, int loopIndex, RankItemData entry)
	{
		return context.GetSettleRankItem(listItem, loopIndex, entry);
	}

	private void RefreshTopRankItems()
	{
		context.RefreshTopRankItems();
	}

	private void OnSettleAnimationCompleted()
	{
		CompleteSettlement();
	}

	private void CollectionFlyToTarget(RankListConfig cfg)
	{
		//IL_017a: Unknown result type (might be due to invalid IL or missing references)
		//IL_017f: Unknown result type (might be due to invalid IL or missing references)
		//IL_018f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0194: Unknown result type (might be due to invalid IL or missing references)
		//IL_0196: Unknown result type (might be due to invalid IL or missing references)
		//IL_0199: Unknown result type (might be due to invalid IL or missing references)
		//IL_019e: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01be: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01fb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0205: Unknown result type (might be due to invalid IL or missing references)
		//IL_020a: Unknown result type (might be due to invalid IL or missing references)
		//IL_020e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0213: Unknown result type (might be due to invalid IL or missing references)
		//IL_0218: Unknown result type (might be due to invalid IL or missing references)
		//IL_021c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0221: Unknown result type (might be due to invalid IL or missing references)
		//IL_0223: Unknown result type (might be due to invalid IL or missing references)
		//IL_02fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_0304: Expected O, but got Unknown
		int playerIndex = GetSelfDataIndex();
		if (playerIndex < 0)
		{
			RankItemJumpAnimation(0, 0);
			return;
		}
		int num = SettleDataIndexToLoopIndex(playerIndex);
		RankItemData entry = DataBuilder.AllEntries[playerIndex];
		LoopListViewItem2 shownItemByItemIndex = View.SettlementLoopList.GetShownItemByItemIndex(num);
		RankItem rankItem = GetSettleRankItem(shownItemByItemIndex, num, entry);
		if ((Object)(object)rankItem == (Object)null || (Object)(object)rankItem.ImgCollect == (Object)null || (Object)(object)View.imgCollection == (Object)null)
		{
			FlyImgCollectionEnd();
			return;
		}
		RankModelProxy rankModelProxy = GameApp.Proxy.Get<RankModelProxy>();
		int valueOrDefault = (rankModelProxy?.Data?.CurrentSettleScore).GetValueOrDefault();
		int totalScore = (rankModelProxy?.Data?.PlayerRankScore).GetValueOrDefault();
		int num2 = Mathf.Max(0, totalScore - valueOrDefault);
		rankItem.SetCollectScore(num2);
		if (valueOrDefault <= 0)
		{
			FlyImgCollectionEnd();
			return;
		}
		RectTransform original = View.imgCollection;
		Vector3 startPos = ((Component)original).transform.position;
		Vector3 position = rankItem.ImgCollect.position;
		Vector3 val = position - startPos;
		Vector3 val2 = new Vector3(0f - val.y, val.x, 0f);
		Vector3 normalized = ((Vector3)(ref val2)).normalized;
		float num3 = (((Object)(object)cfg != (Object)null) ? cfg.collectionFlyArcStrength : (-1.5f));
		float magnitude = ((Vector3)(ref val)).magnitude;
		float num4 = num3 * (magnitude / 3f);
		Vector3 ctrlPointPos = (startPos + position) * 0.5f + normalized * num4;
		Vector3[] paths = BezierUtil.GetBezier2Path(startPos, position, ctrlPointPos, 50);
		float flyDuration = (((Object)(object)cfg != (Object)null) ? cfg.collectionFlyDuration : 0.6f);
		float num5 = (((Object)(object)cfg != (Object)null) ? cfg.collectionFlyInterval : 0.1f);
		AnimationCurve flyCurve = cfg?.CollectionFlyCurve;
		int num6 = Mathf.Min(valueOrDefault, 9);
		int perArrival = valueOrDefault / num6;
		int extraRemainder = valueOrDefault % num6;
		int currentDisplayNum = num2;
		Sequence val3 = DOTween.Sequence();
		tweens.Add((Tween)(object)val3);
		for (int i = 0; i < num6; i++)
		{
			int idx = i;
			bool isLast = idx == num6 - 1;
			TweenSettingsExtensions.InsertCallback(val3, (float)idx * num5, (TweenCallback)delegate
			{
				//IL_0029: Unknown result type (might be due to invalid IL or missing references)
				//IL_01c8: Unknown result type (might be due to invalid IL or missing references)
				//IL_01d2: Expected O, but got Unknown
				GameObject val4 = AcquireCollectFlyClone(original, startPos);
				Transform flyObj = val4.transform;
				Animation component = ((Component)original).GetComponent<Animation>();
				if ((Object)(object)(((Object)(object)component != (Object)null) ? component.GetClip("anim_rank_collect_receive_gp1") : null) != (Object)null)
				{
					component.Stop();
					component.Rewind("anim_rank_collect_receive_gp1");
					component.Play("anim_rank_collect_receive_gp1");
				}
				if (isLast)
				{
					timerIds.Add(GameApp.Timer.Delay(0.1f, (Action)delegate
					{
						((Component)original).gameObject.SetActive(false);
					}));
				}
				Transform val5 = flyObj.Find("Image");
				if ((Object)(object)val5 != (Object)null)
				{
					((Component)val5).gameObject.SetActive(false);
				}
				Transform val6 = flyObj.Find("eff_rank_fly_xiangguo_2");
				if ((Object)(object)val6 != (Object)null)
				{
					((Component)val6).gameObject.SetActive(true);
				}
				if (flyObj.childCount > 0)
				{
					((Component)flyObj.GetChild(0)).gameObject.SetActive(true);
				}
				TweenerCore<Vector3, Path, PathOptions> item = TweenSettingsExtensions.OnComplete<TweenerCore<Vector3, Path, PathOptions>>(TweenSettingsExtensions.SetEase<TweenerCore<Vector3, Path, PathOptions>>(TweenSettingsExtensions.SetOptions(ShortcutExtensions.DOPath(flyObj, paths, flyDuration, (PathType)1, (PathMode)1, 10, (Color?)null), false, (AxisConstraint)0, (AxisConstraint)0), flyCurve ?? AnimationCurve.EaseInOut(0f, 0f, 1f, 1f)), (TweenCallback)delegate
				{
					currentDisplayNum += perArrival + ((idx < extraRemainder) ? 1 : 0);
					rankItem.SetCollectScore(currentDisplayNum);
					if (idx >= 0 && idx < AudioKeys.RankUpScores.Length)
					{
						AudioService.PlayUiSound(AudioKeys.RankUpScores[idx].Value);
					}
					VibrateUtility.Vibrate(1500);
					Animation component2 = ((Component)flyObj).GetComponent<Animation>();
					if ((Object)(object)component2 != (Object)null && (Object)(object)component2.GetClip("anim_rank_collect_fed2_gp1") != (Object)null)
					{
						component2.Play("anim_rank_collect_fed2_gp1");
						float length = component2["anim_rank_collect_fed2_gp1"].length;
						int item2 = GameApp.Timer.Delay(length, (Action)delegate
						{
							if ((Object)(object)flyObj != (Object)null)
							{
								ReleaseCollectFlyClone(((Component)flyObj).gameObject);
							}
						});
						timerIds.Add(item2);
					}
					else
					{
						ReleaseCollectFlyClone(((Component)flyObj).gameObject);
					}
					if (isLast)
					{
						DataBuilder.AllEntries[playerIndex].Collect = totalScore;
						FlyImgCollectionEnd();
					}
				});
				tweens.Add((Tween)(object)item);
			});
		}
	}

	private GameObject AcquireCollectFlyClone(RectTransform original, Vector3 startPos)
	{
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		using (new ProfilerScope("RankSettleAnimator.AcquireCollectFlyClone"))
		{
			GameObject val = null;
			while (collectFlyPool.Count > 0)
			{
				val = collectFlyPool[collectFlyPool.Count - 1];
				collectFlyPool.RemoveAt(collectFlyPool.Count - 1);
				if ((Object)(object)val != (Object)null)
				{
					break;
				}
			}
			if ((Object)(object)val == (Object)null)
			{
				val = Object.Instantiate<GameObject>(((Component)original).gameObject, ((Component)original).transform.parent);
			}
			val.transform.position = startPos;
			val.SetActive(true);
			activeFlyClones.Add(val);
			return val;
		}
	}

	private void ReleaseCollectFlyClone(GameObject clone)
	{
		if ((Object)(object)clone == (Object)null)
		{
			return;
		}
		Animation component = clone.GetComponent<Animation>();
		if ((Object)(object)component != (Object)null)
		{
			component.Stop();
			if ((Object)(object)component.GetClip("anim_rank_collect_fed2_gp1") != (Object)null)
			{
				component.Rewind("anim_rank_collect_fed2_gp1");
			}
		}
		Transform val = clone.transform.Find("eff_rank_tail_02_gp1");
		if ((Object)(object)val != (Object)null)
		{
			((Component)val).gameObject.SetActive(false);
		}
		clone.SetActive(false);
		activeFlyClones.Remove(clone);
		collectFlyPool.Add(clone);
	}

	private void FlyToTopThree(int playerLoopIndex, RankItemData playerEntry, int newRank, int oldRank)
	{
		//IL_0296: Unknown result type (might be due to invalid IL or missing references)
		//IL_029b: Unknown result type (might be due to invalid IL or missing references)
		//IL_03c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_03cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0143: Unknown result type (might be due to invalid IL or missing references)
		//IL_014b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0150: Unknown result type (might be due to invalid IL or missing references)
		//IL_015a: Unknown result type (might be due to invalid IL or missing references)
		//IL_015f: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_0482: Unknown result type (might be due to invalid IL or missing references)
		//IL_01cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_0206: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_020b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0242: Unknown result type (might be due to invalid IL or missing references)
		if (oldRank <= 3)
		{
			LoopListView2 listQuick = View.SettlementLoopList;
			LoopListViewItem2 shownItemByItemIndex = listQuick.GetShownItemByItemIndex(0);
			if (oldRank == newRank)
			{
				timerIds.Add(GameApp.Timer.Delay(0.4f, (Action)delegate
				{
					FinalizeSettle(newRank, moveSelf: true);
					VibrateUtility.VibrateRankLandDrop("RankSettle-前三名次未变化");
					LoopListViewItem2 shownItemByItemIndex5 = listQuick.GetShownItemByItemIndex(0);
					RankTopHeader freshHeader = ((shownItemByItemIndex5 != null) ? ((Component)shownItemByItemIndex5).GetComponent<RankTopHeader>() : null);
					if ((Object)(object)freshHeader != (Object)null)
					{
						RectTransform cachedRectTransform2 = shownItemByItemIndex5.CachedRectTransform;
						if ((Object)(object)cachedRectTransform2 != (Object)null && (Object)(object)((Transform)cachedRectTransform2).parent != (Object)null)
						{
							MoveHeaderToTemporaryParent(cachedRectTransform2, ((Transform)listQuick.ViewPortTrans).parent);
						}
						freshHeader.HideAllRankEffects();
						freshHeader.SetRankAnimationAsLastSibling(newRank);
						AudioService.PlayUiSound(AudioKeys.RankUp5.Value);
						freshHeader.PlayUnchangedRankAnimation(newRank);
						View.PlayTopUI(newRank);
						View.ShowCaidaiObj();
						timerIds.Add(GameApp.Timer.Delay(5f, (Action)delegate
						{
							View.HideCaidaiObj();
						}));
						timerIds.Add(GameApp.Timer.Delay(0.5f, (Action)delegate
						{
							RestoreTemporaryHeaderParent();
							OnSettleAnimationCompleted();
						}));
						timerIds.Add(GameApp.Timer.Delay(5f, (Action)delegate
						{
							freshHeader.HideUnchangedRankEffect(newRank);
						}));
					}
					else
					{
						timerIds.Add(GameApp.Timer.Delay(0.5f, (Action)OnSettleAnimationCompleted));
					}
				}));
				return;
			}
			RankTopHeader rankTopHeader = ((shownItemByItemIndex != null) ? ((Component)shownItemByItemIndex).GetComponent<RankTopHeader>() : null);
			LoopListViewItem2 shownItemByItemIndex2 = listQuick.GetShownItemByItemIndex(playerLoopIndex);
			RankItem playerRankItemQuick = (((Object)(object)shownItemByItemIndex2 != (Object)null) ? GetSettleRankItem(shownItemByItemIndex2, playerLoopIndex, playerEntry) : null);
			if ((Object)(object)rankTopHeader == (Object)null || (Object)(object)playerRankItemQuick == (Object)null)
			{
				FinalizeSettle(newRank, moveSelf: true);
				timerIds.Add(GameApp.Timer.Delay(0.5f, (Action)OnSettleAnimationCompleted));
				return;
			}
			RectTransform cachedRectTransform = shownItemByItemIndex.CachedRectTransform;
			Vector3[] array = (Vector3[])(object)new Vector3[4];
			cachedRectTransform.GetWorldCorners(array);
			Vector3 val = (array[0] + array[2]) * 0.5f;
			RankItem rankItem = newRank switch
			{
				1 => rankTopHeader.FirstRankItem, 
				2 => rankTopHeader.SecondRankItem, 
				3 => rankTopHeader.ThirdRankItem, 
				_ => null, 
			};
			Vector3 targetHeadWorldPos = (((Object)(object)rankItem != (Object)null) ? rankItem.HeadWorldPosition : playerRankItemQuick.HeadWorldPosition);
			Transform headTransform = playerRankItemQuick.HeadRootTransform;
			Vector3 originalHeadLocalPos = (((Object)(object)headTransform != (Object)null) ? headTransform.localPosition : Vector3.zero);
			if ((Object)(object)headTransform != (Object)null)
			{
				headTransform.SetAsLastSibling();
				AudioService.PlayUiSound(AudioKeys.RankUp2.Value);
				TweenSettingsExtensions.SetEase<TweenerCore<Vector3, Vector3, VectorOptions>>(ShortcutExtensions.DOMove(headTransform, val, 0.6f, false), (Ease)9);
			}
			timerIds.Add(GameApp.Timer.Delay(0.6f, (Action)delegate
			{
				float num2 = playerRankItemQuick.PlayTop3UpAnimation();
				timerIds.Add(GameApp.Timer.Delay(num2, (Action)delegate
				{
					//IL_003d: Unknown result type (might be due to invalid IL or missing references)
					float num3 = playerRankItemQuick.PlayTop3DownAnimation();
					playerRankItemQuick.ReparentHeadRoot(((Component)View.SettlementLoopList).transform.parent);
					playerRankItemQuick.MoveHeadRootTo(targetHeadWorldPos, 0.3f);
					ScheduleCaidaiOnTop3DownStart();
					timerIds.Add(GameApp.Timer.Delay(num3, (Action)delegate
					{
						//IL_004f: Unknown result type (might be due to invalid IL or missing references)
						VibrateUtility.VibrateRankLandDrop("RankSettle-前三名头像下砸");
						playerRankItemQuick.ReturnHeadRoot(((Component)playerRankItemQuick).transform);
						if ((Object)(object)headTransform != (Object)null)
						{
							ShortcutExtensions.DOKill((Component)(object)headTransform, false);
							headTransform.localPosition = originalHeadLocalPos;
						}
						GameObject val2 = GameObject.Find("eff_imghead_yuan_gp1");
						if ((Object)(object)val2 != (Object)null)
						{
							val2.SetActive(false);
						}
						FinalizeSettle(newRank, moveSelf: true);
						LoopListViewItem2 shownItemByItemIndex5 = listQuick.GetShownItemByItemIndex(0);
						RankTopHeader freshHeader = ((shownItemByItemIndex5 != null) ? ((Component)shownItemByItemIndex5).GetComponent<RankTopHeader>() : null);
						RectTransform val3 = ((shownItemByItemIndex5 != null) ? shownItemByItemIndex5.CachedRectTransform : null);
						Transform val4 = ((val3 != null) ? ((Transform)val3).parent : null);
						if ((Object)(object)freshHeader != (Object)null)
						{
							if ((Object)(object)val3 != (Object)null && (Object)(object)val4 != (Object)null)
							{
								MoveHeaderToTemporaryParent(val3, ((Transform)listQuick.ViewPortTrans).parent);
							}
							freshHeader.ShowRankEffect(newRank);
							freshHeader.PlayRankAnimation(newRank);
							View.PlayTopUI(newRank);
							timerIds.Add(GameApp.Timer.Delay(0.5f, (Action)delegate
							{
								RestoreTemporaryHeaderParent();
								OnSettleAnimationCompleted();
							}));
							timerIds.Add(GameApp.Timer.Delay(5f, (Action)delegate
							{
								freshHeader.HideRankEffect(newRank);
							}));
						}
					}));
				}));
			}));
			return;
		}
		LoopListView2 list = View.SettlementLoopList;
		ItemPosStruct firstShownItemIndexAndOffset = list.GetFirstShownItemIndexAndOffset();
		int num = Mathf.Clamp(firstShownItemIndexAndOffset.mItemIndex, 0, SettleTotalCount - 1);
		float mItemOffset = firstShownItemIndexAndOffset.mItemOffset;
		EnsureSettlementListItemsAboveCurrentCreated();
		list.MovePanelToItemIndexImmediately(num, mItemOffset);
		list.MovePanelToItemIndexImmediately(0, 0f);
		LoopListViewItem2 shownItemByItemIndex3 = list.GetShownItemByItemIndex(0);
		RankTopHeader header = ((shownItemByItemIndex3 != null) ? ((Component)shownItemByItemIndex3).GetComponent<RankTopHeader>() : null);
		list.MovePanelToItemIndexImmediately(num, mItemOffset);
		list.RefreshAllShownItem();
		if ((Object)(object)shownItemByItemIndex3 == (Object)null || (Object)(object)header == (Object)null)
		{
			timerIds.Add(GameApp.Timer.Delay(0.2f, (Action)delegate
			{
				RankItemJumpAnimation(oldRank, newRank);
			}));
			return;
		}
		RectTransform headerRtf = shownItemByItemIndex3.CachedRectTransform;
		LoopListViewItem2 shownItemByItemIndex4 = list.GetShownItemByItemIndex(playerLoopIndex);
		RankItem playerRankItem = (((Object)(object)shownItemByItemIndex4 != (Object)null) ? GetSettleRankItem(shownItemByItemIndex4, playerLoopIndex, playerEntry) : null);
		if ((Object)(object)playerRankItem == (Object)null)
		{
			timerIds.Add(GameApp.Timer.Delay(0.2f, (Action)delegate
			{
				RankItemJumpAnimation(oldRank, newRank);
			}));
			return;
		}
		Vector3 position = ((Component)playerRankItem).transform.position;
		LoopListViewItem2 cloneListItem = list.NewListViewItem("RankItem");
		if ((Object)(object)cloneListItem != (Object)null)
		{
			activeRankClones.Add(((Component)cloneListItem).gameObject);
		}
		LoopListViewItem2 obj = cloneListItem;
		RankItem cloneRankItem = ((obj != null) ? ((Component)obj).GetComponent<RankItem>() : null);
		if ((Object)(object)cloneRankItem == (Object)null)
		{
			timerIds.Add(GameApp.Timer.Delay(0.2f, (Action)delegate
			{
				RankItemJumpAnimation(oldRank, newRank);
			}));
			return;
		}
		RectTransform cloneRtf = cloneListItem.CachedRectTransform;
		((Transform)cloneRtf).SetParent((Transform)(object)list.ViewPortTrans, true);
		((Transform)cloneRtf).position = position;
		((Transform)cloneRtf).SetAsLastSibling();
		cloneRankItem.SetSettleTopThree(value: true);
		cloneRankItem.SetRankType(ERankContent.Settle);
		cloneRankItem.CopyCollectPositionFrom(playerRankItem);
		cloneRankItem.Render(playerEntry);
		playerRankItem.SetVisible(visible: false);
		timerIds.Add(GameApp.Timer.Delay(0.3f, (Action)delegate
		{
			AudioService.PlayUiSound(AudioKeys.RankUp3.Value);
			cloneRankItem.PlayRankItemAnimation(RankItem.ERanItemAnimType.ItemUp);
			cloneRankItem.PlayRankUpEffect(active: true);
			timerIds.Add(GameApp.Timer.Delay(0.2f, (Action)delegate
			{
				//IL_002e: Unknown result type (might be due to invalid IL or missing references)
				//IL_005f: Unknown result type (might be due to invalid IL or missing references)
				//IL_0066: Unknown result type (might be due to invalid IL or missing references)
				//IL_006b: Unknown result type (might be due to invalid IL or missing references)
				//IL_0075: Unknown result type (might be due to invalid IL or missing references)
				//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
				//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
				//IL_00c4: Unknown result type (might be due to invalid IL or missing references)
				//IL_00ff: Unknown result type (might be due to invalid IL or missing references)
				//IL_0104: Unknown result type (might be due to invalid IL or missing references)
				//IL_015f: Unknown result type (might be due to invalid IL or missing references)
				//IL_0179: Unknown result type (might be due to invalid IL or missing references)
				//IL_0183: Expected O, but got Unknown
				//IL_01b9: Unknown result type (might be due to invalid IL or missing references)
				//IL_01c3: Expected O, but got Unknown
				list.ScrollRect.StopMovement();
				float startContentY = list.ScrollRect.content.anchoredPosition.y;
				float num2 = 0f - startContentY;
				RectTransform viewPortTrans = list.ViewPortTrans;
				Vector3[] array2 = (Vector3[])(object)new Vector3[4];
				viewPortTrans.GetWorldCorners(array2);
				Vector3 val2 = (array2[1] + array2[2]) * 0.5f;
				Vector3[] array3 = (Vector3[])(object)new Vector3[4];
				headerRtf.GetWorldCorners(array3);
				float num3 = array3[1].y - array3[0].y;
				Vector3 val3 = val2 - new Vector3(0f, num3 * 0.5f, 0f);
				int rankDelta = Mathf.Abs(oldRank - newRank);
				AnimationCurve curve = RankListConfig.Instance?.listMoveCurve;
				int notchPlayed = 0;
				Rect rect = cloneRtf.rect;
				float itemSize = ((Rect)(ref rect)).height + cloneListItem.Padding;
				Tweener val4 = DOVirtual.Float(0f, num2, 0.6f, (TweenCallback<float>)delegate(float delta)
				{
					//IL_0015: Unknown result type (might be due to invalid IL or missing references)
					//IL_001a: Unknown result type (might be due to invalid IL or missing references)
					//IL_003f: Unknown result type (might be due to invalid IL or missing references)
					Vector2 anchoredPosition = list.ScrollRect.content.anchoredPosition;
					anchoredPosition.y = startContentY + delta;
					list.ScrollRect.content.anchoredPosition = anchoredPosition;
					for (int num4 = (int)(Mathf.Abs(delta) / itemSize); notchPlayed < num4 && notchPlayed < rankDelta; notchPlayed++)
					{
						AudioService.PlayUiSound(AudioKeys.RankUpNotch.Value);
						VibrateUtility.Vibrate(1501);
					}
				});
				ApplyTweenEase((Tween)(object)val4, curve, (Ease)6);
				bool cloneEffectsTriggered = false;
				Vector3[] headerCheckCorners = (Vector3[])(object)new Vector3[4];
				TweenerCore<Vector3, Vector3, VectorOptions> val5 = TweenSettingsExtensions.OnUpdate<TweenerCore<Vector3, Vector3, VectorOptions>>(TweenSettingsExtensions.SetEase<TweenerCore<Vector3, Vector3, VectorOptions>>(ShortcutExtensions.DOMove((Transform)(object)cloneRtf, val3, 0.6f, false), (Ease)3), (TweenCallback)delegate
				{
					//IL_003c: Unknown result type (might be due to invalid IL or missing references)
					if (!cloneEffectsTriggered)
					{
						headerRtf.GetWorldCorners(headerCheckCorners);
						float y = headerCheckCorners[0].y;
						if (((Transform)cloneRtf).position.y > y)
						{
							cloneEffectsTriggered = true;
							cloneRankItem.PlayRankUpEffect(active: false);
							cloneRankItem.PlayGlowClose();
						}
					}
				});
				Sequence scrollSeq = DOTween.Sequence();
				TweenSettingsExtensions.Join(scrollSeq, (Tween)(object)val4);
				TweenSettingsExtensions.Join(scrollSeq, (Tween)(object)val5);
				TweenSettingsExtensions.OnComplete<Sequence>(scrollSeq, (TweenCallback)delegate
				{
					float num4 = cloneRankItem.PlayUptoTop3Animation();
					timerIds.Add(GameApp.Timer.Delay(num4, (Action)delegate
					{
						float num5 = cloneRankItem.PlayTop3UpAnimation();
						timerIds.Add(GameApp.Timer.Delay(num5, (Action)delegate
						{
							//IL_0070: Unknown result type (might be due to invalid IL or missing references)
							//IL_0068: Unknown result type (might be due to invalid IL or missing references)
							//IL_0075: Unknown result type (might be due to invalid IL or missing references)
							//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
							float num6 = cloneRankItem.PlayTop3DownAnimation();
							RankItem rankItem2 = newRank switch
							{
								1 => header.FirstRankItem, 
								2 => header.SecondRankItem, 
								3 => header.ThirdRankItem, 
								_ => null, 
							};
							Vector3 worldPos = (((Object)(object)rankItem2 != (Object)null) ? rankItem2.HeadWorldPosition : ((Transform)cloneRtf).position);
							cloneRankItem.ReparentHeadRoot(((Component)View.SettlementLoopList).transform.parent);
							cloneRankItem.MoveHeadRootTo(worldPos, 0.3f);
							ScheduleCaidaiOnTop3DownStart();
							timerIds.Add(GameApp.Timer.Delay(num6, (Action)delegate
							{
								VibrateUtility.VibrateRankLandDrop("RankSettle-飞行升入前三分支");
								if ((Object)(object)cloneListItem != (Object)null && (Object)(object)((Component)cloneListItem).gameObject != (Object)null)
								{
									cloneRankItem.ReturnHeadRoot(((Component)cloneListItem).transform);
								}
								Object.Destroy((Object)(object)((Component)cloneListItem).gameObject);
								playerRankItem.SetVisible(visible: true);
								FinalizeSettle(newRank, moveSelf: true);
								LoopListViewItem2 shownItemByItemIndex5 = list.GetShownItemByItemIndex(0);
								RectTransform val6 = ((shownItemByItemIndex5 != null) ? shownItemByItemIndex5.CachedRectTransform : null);
								Transform val7 = ((val6 != null) ? ((Transform)val6).parent : null);
								if ((Object)(object)val6 != (Object)null && (Object)(object)val7 != (Object)null)
								{
									MoveHeaderToTemporaryParent(val6, ((Transform)list.ViewPortTrans).parent);
								}
								header.ShowRankEffect(newRank);
								header.PlayRankAnimation(newRank);
								View.PlayTopUI(newRank);
								timerIds.Add(GameApp.Timer.Delay(0.5f, (Action)delegate
								{
									RestoreTemporaryHeaderParent();
									OnSettleAnimationCompleted();
								}));
								timerIds.Add(GameApp.Timer.Delay(5f, (Action)delegate
								{
									header.HideRankEffect(newRank);
								}));
							}));
						}));
					}));
					tweens.Add((Tween)(object)scrollSeq);
				});
			}));
		}));
	}

	private void FlyImgCollectionEnd()
	{
		//IL_0237: Unknown result type (might be due to invalid IL or missing references)
		//IL_023c: Unknown result type (might be due to invalid IL or missing references)
		//IL_025b: Unknown result type (might be due to invalid IL or missing references)
		RankModelProxy rankModelProxy = GameApp.Proxy.Get<RankModelProxy>();
		int selfDataIndex = GetSelfDataIndex();
		int oldRank = ResolveSettleOldRank(selfDataIndex, rankModelProxy?.Data);
		int newRank = rankModelProxy?.Data?.CurrentSettleRanking ?? oldRank;
		LoopListView2 list = View.SettlementLoopList;
		if (selfDataIndex < 0 || selfDataIndex >= DataBuilder.AllEntries.Count || (Object)(object)list == (Object)null)
		{
			timerIds.Add(GameApp.Timer.Delay(0.2f, (Action)delegate
			{
				RankItemJumpAnimation(oldRank, newRank);
			}));
			return;
		}
		int playerLoopIndex = SettleDataIndexToLoopIndex(selfDataIndex);
		RankItemData rankItemData = DataBuilder.AllEntries[selfDataIndex];
		LoopListViewItem2 shownItemByItemIndex = list.GetShownItemByItemIndex(playerLoopIndex);
		RankItem originalRankItem = GetSettleRankItem(shownItemByItemIndex, playerLoopIndex, rankItemData);
		if ((Object)(object)originalRankItem == (Object)null)
		{
			timerIds.Add(GameApp.Timer.Delay(0.2f, (Action)delegate
			{
				RankItemJumpAnimation(oldRank, newRank);
			}));
			return;
		}
		if (newRank <= 3)
		{
			FlyToTopThree(playerLoopIndex, rankItemData, newRank, oldRank);
			return;
		}
		LoopListViewItem2 cloneListItem = list.NewListViewItem("RankItem");
		if ((Object)(object)cloneListItem != (Object)null)
		{
			activeRankClones.Add(((Component)cloneListItem).gameObject);
		}
		LoopListViewItem2 obj = cloneListItem;
		RankItem cloneRankItem = ((obj != null) ? ((Component)obj).GetComponent<RankItem>() : null);
		if ((Object)(object)cloneRankItem == (Object)null)
		{
			timerIds.Add(GameApp.Timer.Delay(0.2f, (Action)delegate
			{
				RankItemJumpAnimation(oldRank, newRank);
			}));
			return;
		}
		RectTransform cloneRtf = cloneListItem.CachedRectTransform;
		RectTransform val = ((shownItemByItemIndex != null) ? shownItemByItemIndex.CachedRectTransform : null);
		if ((Object)(object)val != (Object)null)
		{
			Vector3 position = ((Transform)val).position;
			((Transform)cloneRtf).SetParent((Transform)(object)list.ViewPortTrans, true);
			((Transform)cloneRtf).position = position;
		}
		else
		{
			((Transform)cloneRtf).SetParent((Transform)(object)list.ViewPortTrans, false);
		}
		((Transform)cloneRtf).SetAsLastSibling();
		cloneRankItem.SetSettleTopThree(playerLoopIndex == 0);
		cloneRankItem.SetRankType(ERankContent.Settle);
		cloneRankItem.CopyCollectPositionFrom(originalRankItem);
		cloneRankItem.Render(rankItemData);
		originalRankItem.SetVisible(visible: false);
		timerIds.Add(GameApp.Timer.Delay(0.3f, (Action)delegate
		{
			cloneRankItem.PlayRankItemAnimation(RankItem.ERanItemAnimType.ItemUp);
			cloneRankItem.PlayRankUpEffect(active: true);
			timerIds.Add(GameApp.Timer.Delay(0.2f, (Action)delegate
			{
				//IL_00e8: Unknown result type (might be due to invalid IL or missing references)
				//IL_00ed: Unknown result type (might be due to invalid IL or missing references)
				//IL_0158: Unknown result type (might be due to invalid IL or missing references)
				//IL_0173: Unknown result type (might be due to invalid IL or missing references)
				//IL_0178: Unknown result type (might be due to invalid IL or missing references)
				//IL_0187: Unknown result type (might be due to invalid IL or missing references)
				//IL_018c: Unknown result type (might be due to invalid IL or missing references)
				//IL_01cd: Unknown result type (might be due to invalid IL or missing references)
				//IL_01d5: Unknown result type (might be due to invalid IL or missing references)
				//IL_01da: Unknown result type (might be due to invalid IL or missing references)
				//IL_01e4: Unknown result type (might be due to invalid IL or missing references)
				//IL_0270: Unknown result type (might be due to invalid IL or missing references)
				//IL_0275: Unknown result type (might be due to invalid IL or missing references)
				//IL_0282: Unknown result type (might be due to invalid IL or missing references)
				//IL_02b0: Unknown result type (might be due to invalid IL or missing references)
				//IL_02b2: Unknown result type (might be due to invalid IL or missing references)
				//IL_02b7: Unknown result type (might be due to invalid IL or missing references)
				//IL_02b9: Unknown result type (might be due to invalid IL or missing references)
				//IL_02bb: Unknown result type (might be due to invalid IL or missing references)
				//IL_02c0: Unknown result type (might be due to invalid IL or missing references)
				//IL_02c8: Unknown result type (might be due to invalid IL or missing references)
				//IL_0301: Unknown result type (might be due to invalid IL or missing references)
				//IL_030b: Expected O, but got Unknown
				int dataIndex = Mathf.Clamp(newRank - 1, 0, DataBuilder.AllEntries.Count - 1);
				int targetLoopIndex = SettleDataIndexToLoopIndex(dataIndex);
				LoopListViewItem2 shownItemByItemIndex2 = list.GetShownItemByItemIndex(playerLoopIndex);
				LoopListViewItem2 shownItemByItemIndex3 = list.GetShownItemByItemIndex(targetLoopIndex);
				if (AreItemsBothFullyInViewport(list, shownItemByItemIndex2, shownItemByItemIndex3))
				{
					PlayViewportSwapAnimation(list, cloneListItem, cloneRankItem, originalRankItem, playerLoopIndex, targetLoopIndex, oldRank, newRank);
				}
				else
				{
					AudioService.PlayUiSound(AudioKeys.RankUp1.Value);
					list.ScrollRect.StopMovement();
					float firstShownFloatItemIndexInViewPort = list.GetFirstShownFloatItemIndexInViewPort();
					Rect rect = cloneRtf.rect;
					float itemSize = ((Rect)(ref rect)).height + cloneListItem.Padding;
					EnsureSettlementListItemsAboveCurrentCreated();
					LoopListViewItem2 shownItemByItemIndex4 = list.GetShownItemByItemIndex(playerLoopIndex);
					if (shownItemByItemIndex4 != null)
					{
						((Component)shownItemByItemIndex4).GetComponent<RankItem>()?.SetVisible(visible: false);
					}
					firstShownFloatItemIndexInViewPort = list.GetFirstShownFloatItemIndexInViewPort();
					float startContentY = list.ScrollRect.content.anchoredPosition.y;
					RectTransform viewPortTrans = list.ViewPortTrans;
					rect = viewPortTrans.rect;
					float height = ((Rect)(ref rect)).height;
					rect = cloneRtf.rect;
					float height2 = ((Rect)(ref rect)).height;
					float num = (height * 0.5f - height2 * 0.5f) / itemSize;
					float num2 = (float)targetLoopIndex - num;
					Vector3[] array = (Vector3[])(object)new Vector3[4];
					viewPortTrans.GetWorldCorners(array);
					Vector3 val2 = (array[0] + array[2]) * 0.5f;
					RankListConfig instance = RankListConfig.Instance;
					AnimationCurve curve = instance?.listMoveCurve;
					AnimationCurve gapCurve = instance?.settleGapAnimCurve;
					float num3 = (num2 - firstShownFloatItemIndexInViewPort) * itemSize;
					int rankDelta = Mathf.Abs(oldRank - newRank);
					int notchPlayed = 0;
					Tweener val3 = DOVirtual.Float(0f, num3, 1f, (TweenCallback<float>)delegate(float delta)
					{
						//IL_0015: Unknown result type (might be due to invalid IL or missing references)
						//IL_001a: Unknown result type (might be due to invalid IL or missing references)
						//IL_003f: Unknown result type (might be due to invalid IL or missing references)
						Vector2 anchoredPosition = list.ScrollRect.content.anchoredPosition;
						anchoredPosition.y = startContentY + delta;
						list.ScrollRect.content.anchoredPosition = anchoredPosition;
						for (int num5 = (int)(Mathf.Abs(delta) / itemSize); notchPlayed < num5 && notchPlayed < rankDelta; notchPlayed++)
						{
							AudioService.PlayUiSound(AudioKeys.RankUpNotch.Value);
							VibrateUtility.Vibrate(1501);
						}
					});
					ApplyTweenEase((Tween)(object)val3, curve, (Ease)6);
					Rect rect2 = cloneRtf.rect;
					float num4 = (0.5f - cloneRtf.pivot.y) * ((Rect)(ref rect2)).height;
					Vector3 val4 = default(Vector3);
					((Vector3)(ref val4))._002Ector(0f, num4, 0f);
					Vector3 val5 = ((Transform)cloneRtf).TransformVector(val4);
					Vector3 val6 = val2 - val5;
					TweenerCore<Vector3, Vector3, VectorOptions> val7 = TweenSettingsExtensions.SetEase<TweenerCore<Vector3, Vector3, VectorOptions>>(ShortcutExtensions.DOMove((Transform)(object)cloneRtf, val6, 1f, false), (Ease)6);
					Sequence val8 = DOTween.Sequence();
					TweenSettingsExtensions.Join(val8, (Tween)(object)val3);
					TweenSettingsExtensions.Join(val8, (Tween)(object)val7);
					TweenSettingsExtensions.OnComplete<Sequence>(val8, (TweenCallback)delegate
					{
						float num5 = 0f - itemSize;
						for (int i = 0; i < list.ShownItemCount; i++)
						{
							LoopListViewItem2 shownItemByIndex = list.GetShownItemByIndex(i);
							if (!((Object)(object)shownItemByIndex == (Object)null) && shownItemByIndex.ItemIndex >= targetLoopIndex)
							{
								RankItem ri = ((Component)shownItemByIndex).GetComponent<RankItem>();
								if (!((Object)(object)ri == (Object)null))
								{
									Tweener val9 = DOVirtual.Float(0f, num5, 0.25f, (TweenCallback<float>)delegate(float v)
									{
										ri.SetVisualOffsetY(v);
									});
									ApplyTweenEase((Tween)(object)val9, gapCurve, (Ease)9);
									tweens.Add((Tween)(object)val9);
								}
							}
						}
						timerIds.Add(GameApp.Timer.Delay(0.25f, (Action)delegate
						{
							cloneRankItem.PlayRankUpEffect(active: false);
							cloneRankItem.PlayRankItemAnimation(RankItem.ERanItemAnimType.ItemDown, skipGlow: true);
							cloneRankItem.FadeInRewardBoxAndOwnShadow(newRank, 0.1f);
							timerIds.Add(GameApp.Timer.Delay(0.1f, (Action)delegate
							{
								cloneRankItem.PlayGlowClose();
							}));
							timerIds.Add(GameApp.Timer.Delay(0.183f, (Action)delegate
							{
								VibrateUtility.VibrateRankLandDrop("RankSettle-滚动爬升分支");
								FinalizeSettle(newRank, moveSelf: true);
								LoopListViewItem2 shownItemByItemIndex5 = list.GetShownItemByItemIndex(targetLoopIndex);
								RankItem rankItem = ((shownItemByItemIndex5 != null) ? ((Component)shownItemByItemIndex5).GetComponent<RankItem>() : null);
								GameObject movedDownEffect = cloneRankItem.TransferRankDownEffect(rankItem);
								if ((Object)(object)cloneListItem != (Object)null && (Object)(object)((Component)cloneListItem).gameObject != (Object)null)
								{
									cloneRankItem.SetVisible(visible: false);
									Object.Destroy((Object)(object)((Component)cloneListItem).gameObject);
								}
								rankItem?.SetVisible(visible: true);
								if (shownItemByItemIndex5 != null)
								{
									((Component)shownItemByItemIndex5).transform.SetAsLastSibling();
								}
								timerIds.Add(GameApp.Timer.Delay(0.5f, (Action)OnSettleAnimationCompleted));
								timerIds.Add(GameApp.Timer.Delay(1.5f, (Action)delegate
								{
									if ((Object)(object)movedDownEffect != (Object)null)
									{
										movedDownEffect.SetActive(false);
									}
								}));
							}));
						}));
					});
					tweens.Add((Tween)(object)val8);
				}
			}));
		}));
	}

	private static void ApplyTweenEase(Tween tween, AnimationCurve curve, Ease fallbackEase)
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		if (curve != null && curve.length > 0)
		{
			TweenSettingsExtensions.SetEase<Tween>(tween, curve);
		}
		else
		{
			TweenSettingsExtensions.SetEase<Tween>(tween, fallbackEase);
		}
	}

	private int ResolveSettleOldRank(int selfIdx, RankModelData data)
	{
		if (selfIdx < 0)
		{
			return RankConst.MaxRankBot + 1;
		}
		int rankIndex = DataBuilder.AllEntries[selfIdx].RankIndex;
		if (rankIndex > RankConst.MaxRankBot)
		{
			return rankIndex;
		}
		if (data != null && data.PlayerRankScore - data.CurrentSettleScore <= 0)
		{
			return RankConst.MaxRankBot + 1;
		}
		return rankIndex;
	}

	private void MoveSelfEntryInAllEntries(int newRank)
	{
		int num = Mathf.Clamp(newRank - 1, 0, DataBuilder.AllEntries.Count - 1);
		int selfDataIndex = GetSelfDataIndex();
		if (selfDataIndex >= 0)
		{
			RankItemData item = DataBuilder.AllEntries[selfDataIndex];
			DataBuilder.AllEntries.RemoveAt(selfDataIndex);
			if (num > selfDataIndex)
			{
				num--;
			}
			DataBuilder.AllEntries.Insert(num, item);
			for (int i = 0; i < DataBuilder.AllEntries.Count; i++)
			{
				DataBuilder.AllEntries[i].RankIndex = i + 1;
			}
		}
	}

	private void FinalizeSettle(int newRank, bool moveSelf)
	{
		if (moveSelf)
		{
			int selfDataIndex = GetSelfDataIndex();
			if (selfDataIndex >= 0)
			{
				int collect = GameApp.Proxy.Get<RankModelProxy>()?.Data?.PlayerRankScore ?? DataBuilder.AllEntries[selfDataIndex].Collect;
				DataBuilder.AllEntries[selfDataIndex].Collect = collect;
			}
			MoveSelfEntryInAllEntries(newRank);
		}
		LoopListView2 settlementLoopList = View.SettlementLoopList;
		int settleLoopItemCount = GetSettleLoopItemCount();
		if (settleLoopItemCount != SettleTotalCount)
		{
			SettleTotalCount = settleLoopItemCount;
			settlementLoopList.SetListItemCount(SettleTotalCount, false);
		}
		settlementLoopList.RefreshAllShownItem();
		RefreshTopRankItems();
	}

	private void RankItemJumpAnimation(int oldRank, int newRank)
	{
		int selfDataIndex = GetSelfDataIndex();
		if (selfDataIndex >= 0)
		{
			int collect = GameApp.Proxy.Get<RankModelProxy>()?.Data?.PlayerRankScore ?? DataBuilder.AllEntries[selfDataIndex].Collect;
			DataBuilder.AllEntries[selfDataIndex].Collect = collect;
		}
		MoveSelfEntryInAllEntries(newRank);
		LoopListView2 settlementLoopList = View.SettlementLoopList;
		if ((Object)(object)settlementLoopList != (Object)null)
		{
			settlementLoopList.RefreshAllShownItem();
			int selfDataIndex2 = GetSelfDataIndex();
			int num = SettleDataIndexToLoopIndex(selfDataIndex2);
			int num2 = ((num > 2) ? Mathf.Clamp(num - 2, 0, SettleTotalCount - 1) : 0);
			settlementLoopList.MovePanelToItemIndex(num2, 0f, 0f);
			settlementLoopList.RefreshAllShownItem();
		}
		FinalizeSettle(newRank, moveSelf: false);
		OnSettleAnimationCompleted();
	}

	private static bool AreItemsBothFullyInViewport(LoopListView2 list, LoopListViewItem2 itemA, LoopListViewItem2 itemB)
	{
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)itemA == (Object)null || (Object)(object)itemB == (Object)null)
		{
			return false;
		}
		RectTransform viewPortTrans = list.ViewPortTrans;
		Vector3[] array = (Vector3[])(object)new Vector3[4];
		viewPortTrans.GetWorldCorners(array);
		float vpTopY = ((Transform)list.ContainerTrans).InverseTransformPoint(array[1]).y;
		float vpBottomY = ((Transform)list.ContainerTrans).InverseTransformPoint(array[0]).y;
		if (IsFullyVisible(itemA))
		{
			return IsFullyVisible(itemB);
		}
		return false;
		bool IsFullyVisible(LoopListViewItem2 item)
		{
			if (item.TopY <= vpTopY)
			{
				return item.BottomY >= vpBottomY;
			}
			return false;
		}
	}

	private void PlayViewportSwapAnimation(LoopListView2 list, LoopListViewItem2 cloneListItem, RankItem cloneRankItem, RankItem originalRankItem, int playerLoopIndex, int targetLoopIndex, int oldRank, int newRank)
	{
		//IL_0165: Unknown result type (might be due to invalid IL or missing references)
		//IL_016a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0184: Unknown result type (might be due to invalid IL or missing references)
		int num = playerLoopIndex - targetLoopIndex;
		LoopListViewItem2 shownItemByItemIndex = list.GetShownItemByItemIndex(playerLoopIndex);
		LoopListViewItem2 shownItemByItemIndex2 = list.GetShownItemByItemIndex(targetLoopIndex);
		if ((Object)(object)shownItemByItemIndex == (Object)null || (Object)(object)shownItemByItemIndex2 == (Object)null)
		{
			LoopListViewItem2 obj = cloneListItem;
			if ((Object)(object)((obj != null) ? ((Component)obj).gameObject : null) != (Object)null)
			{
				Object.Destroy((Object)(object)((Component)cloneListItem).gameObject);
			}
			originalRankItem.SetVisible(visible: true);
			RankItemJumpAnimation(oldRank, newRank);
			return;
		}
		float itemSizeWithPadding = shownItemByItemIndex.ItemSizeWithPadding;
		List<RankItem> list2 = new List<RankItem>(num);
		for (int i = 0; i < num; i++)
		{
			LoopListViewItem2 shownItemByItemIndex3 = list.GetShownItemByItemIndex(targetLoopIndex + i);
			if ((Object)(object)shownItemByItemIndex3 == (Object)null)
			{
				LoopListViewItem2 obj2 = cloneListItem;
				if ((Object)(object)((obj2 != null) ? ((Component)obj2).gameObject : null) != (Object)null)
				{
					Object.Destroy((Object)(object)((Component)cloneListItem).gameObject);
				}
				originalRankItem.SetVisible(visible: true);
				RankItemJumpAnimation(oldRank, newRank);
				return;
			}
			RankItem component = ((Component)shownItemByItemIndex3).GetComponent<RankItem>();
			if ((Object)(object)component != (Object)null)
			{
				list2.Add(component);
			}
		}
		RectTransform cachedRectTransform = cloneListItem.CachedRectTransform;
		Vector3 position = ((Transform)shownItemByItemIndex2.CachedRectTransform).position;
		AudioService.PlayUiSound(AudioKeys.RankUpSwap.Value);
		tweens.Add((Tween)(object)TweenSettingsExtensions.SetEase<TweenerCore<Vector3, Vector3, VectorOptions>>(ShortcutExtensions.DOMove((Transform)(object)cachedRectTransform, position, 0.4f, false), (Ease)9));
		foreach (RankItem item in list2)
		{
			RankItem captured = item;
			tweens.Add((Tween)(object)TweenSettingsExtensions.SetEase<Tweener>(DOVirtual.Float(0f, 0f - itemSizeWithPadding, 0.4f, (TweenCallback<float>)delegate(float v)
			{
				captured.SetVisualOffsetY(v);
			}), (Ease)9));
		}
		timerIds.Add(GameApp.Timer.Delay(0.4f, (Action)delegate
		{
			cloneRankItem.PlayRankUpEffect(active: false);
			cloneRankItem.PlayRankItemAnimation(RankItem.ERanItemAnimType.ItemDown, skipGlow: true);
			cloneRankItem.FadeInRewardBoxAndOwnShadow(newRank, 0.1f);
			timerIds.Add(GameApp.Timer.Delay(0.1f, (Action)delegate
			{
				cloneRankItem.PlayGlowClose();
			}));
			timerIds.Add(GameApp.Timer.Delay(0.183f, (Action)delegate
			{
				VibrateUtility.VibrateRankLandDrop("RankSettle-同视口交换下砸");
				FinalizeSettle(newRank, moveSelf: true);
				LoopListViewItem2 shownItemByItemIndex4 = list.GetShownItemByItemIndex(targetLoopIndex);
				RankItem rankItem = ((shownItemByItemIndex4 != null) ? ((Component)shownItemByItemIndex4).GetComponent<RankItem>() : null);
				GameObject movedDownEffect = cloneRankItem.TransferRankDownEffect(rankItem);
				LoopListViewItem2 obj3 = cloneListItem;
				if ((Object)(object)((obj3 != null) ? ((Component)obj3).gameObject : null) != (Object)null)
				{
					cloneRankItem.SetVisible(visible: false);
					Object.Destroy((Object)(object)((Component)cloneListItem).gameObject);
				}
				rankItem?.SetVisible(visible: true);
				if (shownItemByItemIndex4 != null)
				{
					((Component)shownItemByItemIndex4).transform.SetAsLastSibling();
				}
				timerIds.Add(GameApp.Timer.Delay(0.5f, (Action)OnSettleAnimationCompleted));
				timerIds.Add(GameApp.Timer.Delay(1.5f, (Action)delegate
				{
					if ((Object)(object)movedDownEffect != (Object)null)
					{
						movedDownEffect.SetActive(false);
					}
				}));
			}));
		}));
	}

	private void EnsureSettlementListItemsAboveCurrentCreated()
	{
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		LoopListView2 settlementLoopList = View.SettlementLoopList;
		if (!((Object)(object)settlementLoopList == (Object)null) && SettleTotalCount > 0)
		{
			int num = GetSelfDataIndex();
			if (num < 0)
			{
				num = 0;
			}
			int num2 = SettleDataIndexToLoopIndex(num);
			if (num2 < 0)
			{
				num2 = 0;
			}
			ItemPosStruct firstShownItemIndexAndOffset = settlementLoopList.GetFirstShownItemIndexAndOffset();
			int num3 = Mathf.Clamp(firstShownItemIndexAndOffset.mItemIndex, 0, SettleTotalCount - 1);
			float mItemOffset = firstShownItemIndexAndOffset.mItemOffset;
			int num4 = Mathf.Max(1, settlementLoopList.ShownItemCount - 1);
			settlementLoopList.MovePanelToItemIndexImmediately(0, 0f);
			for (int i = 1; i <= num2; i += num4)
			{
				settlementLoopList.MovePanelToItemIndexImmediately(i, 0f);
			}
			settlementLoopList.MovePanelToItemIndexImmediately(num3, mItemOffset);
			settlementLoopList.RefreshAllShownItem();
		}
	}

	private void ScheduleCaidaiOnTop3DownStart()
	{
		timerIds.Add(GameApp.Timer.Delay(0.28f, (Action)delegate
		{
			if (!isSettlementCompleted)
			{
				View.ShowCaidaiObj();
				timerIds.Add(GameApp.Timer.Delay(5f, (Action)delegate
				{
					View.HideCaidaiObj();
				}));
			}
		}));
	}
}
