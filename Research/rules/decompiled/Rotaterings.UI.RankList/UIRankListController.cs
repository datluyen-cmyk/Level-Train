using System;
using System.Collections;
using System.Collections.Generic;
using Game.DataTrack;
using Game.UI;
using Game.UI.Language;
using RotateRings.Common;
using RotateRings.UI.Home;
using RotateRings.UI.Level;
using RotateRings.UI.RankList;
using RotateRings.UI.Tips;
using Rotaterings.Rank;
using Rotaterings.SO;
using SuperScrollView;
using TMPro;
using UniLog;
using UnityEngine;
using UnityEngine.Events;
using XingYun.Extensions;
using XingYun.UI.Controller;
using XingYun.UI.Core;

namespace Rotaterings.UI.RankList;

[UIPath("UIRankList")]
[UIPath("UIRankListHalloween")]
public class UIRankListController : GameBaseController<UIRankListView>, IRankSettleAnimatorContext
{
	private const string Tag = "UIRankListController";

	private RankListDataBuilder dataBuilder;

	private RankSettleAnimator settleAnimator;

	private ERankContent rankType;

	private int countdownTimerId = -1;

	private int registerMainCoroutineId = -1;

	private int registerSettleCoroutineId = -1;

	private int centerItemCoroutineId = -1;

	private int centerItemRequestVersion;

	private CanvasGroup listCanvasGroup;

	private CanvasGroup settleListCanvasGroup;

	private Coroutine staggerCoroutine;

	private Coroutine refreshMainSelfStickyCoroutine;

	private UnityAction<Vector2> mainListScrollListener;

	private RankMainListSelfSticky mainListSelfSticky;

	private readonly List<int> timerIds = new List<int>();

	private RankStreakDisplayPresenter streakDisplayPresenter;

	private RankRewardFlow rewardFlow;

	private UnityAction<Vector2> settleScrollListener;

	private int settleTotalCount;

	private readonly Vector3[] viewportCheckCorners = (Vector3[])(object)new Vector3[4];

	private bool settleHeaderEffectsHidden;

	private RankTopHeader cachedSettleHeader;

	private bool isClosing;

	private bool isEndSettlement;

	private bool isEndSettlementClaimStarted;

	private bool isSettleClaimStarted;

	private bool rankListInited;

	private bool settleListInited;

	private bool isBottomAnimationPlayedOnSettleOpen;

	private bool rewardBoxEffectsRevealReady;

	private Action onClosedCallback;

	private int formalCollectIconSide;

	private bool isPrewarmOpen;

	private bool isRemoved;

	private Action<bool> prewarmReadyCallback;

	private const int SettleTopRankCount = 3;

	private const string SettleHeaderPrefabName = "RankTopHeader";

	private const string SettleRankItemPrefabName = "RankItem";

	private const string ContinueTextKey = "Button_Continue";

	private const string ClaimTextKey = "Button_Claim";

	private const string HalloweenTitleKey = "Ring_Halloween_Title";

	private const float RewardBoxEffectDelayFromPanelOpen = 0.8f;

	public bool IsPrewarmOpen => isPrewarmOpen;

	public bool IsRemoved => isRemoved;

	public RankListDataBuilder DataBuilder => dataBuilder;

	public int SettleTotalCount
	{
		get
		{
			return settleTotalCount;
		}
		set
		{
			settleTotalCount = value;
		}
	}

	public UIRankListController(UIRankListView view)
		: base(view)
	{
	}

	private void ResetViewState()
	{
		isClosing = false;
		isRemoved = false;
		CancelPendingCenterItem();
		streakDisplayPresenter?.Cleanup();
		streakDisplayPresenter = null;
		rewardFlow?.Cleanup();
		rewardFlow = null;
		isEndSettlement = false;
		isEndSettlementClaimStarted = false;
		isSettleClaimStarted = false;
		settleHeaderEffectsHidden = false;
		settleAnimator = null;
		isBottomAnimationPlayedOnSettleOpen = false;
		rewardBoxEffectsRevealReady = false;
		((BaseController<UIRankListView>)this).view.HideTopUI();
	}

	public override void OnRegister(object param = null)
	{
		using (new ProfilerScope("UIRankListController.OnRegister"))
		{
			ResetViewState();
			formalCollectIconSide = RankCollectPositionExperiment.ReadIconSide();
			if (param is UIRankListParam uIRankListParam)
			{
				rankType = uIRankListParam.Content;
				onClosedCallback = uIRankListParam.OnClosed;
				isPrewarmOpen = uIRankListParam.Prewarm;
				prewarmReadyCallback = (uIRankListParam.Prewarm ? uIRankListParam.OnPrewarmReady : null);
			}
			else
			{
				rankType = ((param is ERankContent eRankContent) ? eRankContent : ERankContent.Main);
				onClosedCallback = null;
				isPrewarmOpen = false;
				prewarmReadyCallback = null;
			}
			((BaseController<UIRankListView>)this).view.Init(rankType);
			rewardFlow = new RankRewardFlow(((BaseController<UIRankListView>)this).view, () => rankType, GetMainRankTopHeader);
			RankActivityService service = RankActivityEntry.Service;
			isEndSettlement = rankType == ERankContent.Main && service != null && service.IsPeriodSettlePending;
			if (rankType == ERankContent.Main && !isPrewarmOpen)
			{
				HideHomeRankEntryOnOpen();
			}
			if (rankType != ERankContent.Settle && !isEndSettlement)
			{
				RankActivityEntry.EnsureBuildActivityData();
			}
			dataBuilder = new RankListDataBuilder();
			if (rankType == ERankContent.Settle)
			{
				dataBuilder.BuildForSettle();
			}
			else
			{
				dataBuilder.BuildForMain();
			}
			SetTitle();
			RefreshSettleTitle();
			RefreshLevelText();
			StartCountdown();
			((BaseController<UIRankListView>)this).ListenEvent<LanguageChangeSuccess>((Action<LanguageChangeSuccess>)OnLanguageChangeSuccess);
			if ((Object)(object)((BaseController<UIRankListView>)this).view.MainBg != (Object)null)
			{
				((BaseController<UIRankListView>)this).view.MainBg.SetActive(rankType == ERankContent.Main);
			}
			if (rankType == ERankContent.Main && !isPrewarmOpen)
			{
				((BaseController<UIRankListView>)this).view.PlayMainPanelOpen();
				ScheduleTopRewardBoxExperimentReveal();
			}
			if (rankType == ERankContent.Settle)
			{
				registerSettleCoroutineId = GameApp.Coroutine.Run(RegisterSettle());
			}
			else
			{
				registerMainCoroutineId = GameApp.Coroutine.Run(RegisterMain());
			}
		}
	}

	private IEnumerator RegisterMain()
	{
		if (!isPrewarmOpen)
		{
			RRDataTrack.Instance.ReportPageShow("排行榜页");
		}
		if ((Object)(object)((BaseController<UIRankListView>)this).view.btnBack != (Object)null)
		{
			((UnityEventBase)((BaseController<UIRankListView>)this).view.btnBack.onClick).RemoveAllListeners();
			((UnityEvent)((BaseController<UIRankListView>)this).view.btnBack.onClick).AddListener(new UnityAction(OnCloseClick));
		}
		if ((Object)(object)((BaseController<UIRankListView>)this).view.btnTip != (Object)null)
		{
			((UnityEventBase)((BaseController<UIRankListView>)this).view.btnTip.onClick).RemoveAllListeners();
			((UnityEvent)((BaseController<UIRankListView>)this).view.btnTip.onClick).AddListener(new UnityAction(OnBtnTipClick));
		}
		if ((Object)(object)((BaseController<UIRankListView>)this).view.btnLevel != (Object)null)
		{
			((UnityEventBase)((BaseController<UIRankListView>)this).view.btnLevel.onClick).RemoveAllListeners();
			((UnityEvent)((BaseController<UIRankListView>)this).view.btnLevel.onClick).AddListener(new UnityAction(OnMainButtonClick));
		}
		RefreshLevelText();
		SetupHardStartButton();
		listCanvasGroup = GetOrAddListCanvasGroup(((BaseController<UIRankListView>)this).view.RankLoopList);
		SetCanvasGroupShow(listCanvasGroup, show: false);
		yield return null;
		FillTopRankItems();
		RankItem.InitializeMainVisualStyle(((BaseController<UIRankListView>)this).view.ListRankItemSelf, ((BaseController<UIRankListView>)this).view.RankItem);
		InitRankList();
		SetupMainListSelfSticky();
		BindMainListSelfStickyScroll();
		yield return null;
		yield return null;
		yield return null;
		yield return null;
		yield return null;
		if (isPrewarmOpen)
		{
			registerMainCoroutineId = -1;
			NotifyPrewarmReady(rankListInited);
			yield break;
		}
		SetCanvasGroupShow(listCanvasGroup, show: true);
		StaggerShowItems(((BaseController<UIRankListView>)this).view.RankLoopList);
		while (staggerCoroutine != null)
		{
			yield return null;
		}
		while ((Object)(object)((BaseController<UIRankListView>)this).view.PanelAnimation != (Object)null && ((BaseController<UIRankListView>)this).view.PanelAnimation.isPlaying)
		{
			yield return null;
		}
		((BaseController<UIRankListView>)this).view.PlayBottomAnimation();
		ScheduleRefreshMainListSelfSticky();
		registerMainCoroutineId = -1;
	}

	private void NotifyPrewarmReady(bool success)
	{
		Action<bool> action = prewarmReadyCallback;
		prewarmReadyCallback = null;
		action?.Invoke(success);
	}

	private static CanvasGroup GetOrAddListCanvasGroup(LoopListView2 list)
	{
		if ((Object)(object)list == (Object)null)
		{
			return null;
		}
		CanvasGroup val = ((Component)list).GetComponent<CanvasGroup>();
		if ((Object)(object)val == (Object)null)
		{
			val = ((Component)list).gameObject.AddComponent<CanvasGroup>();
		}
		return val;
	}

	private static void SetCanvasGroupShow(CanvasGroup group, bool show)
	{
		if (!((Object)(object)group == (Object)null))
		{
			group.alpha = (show ? 1f : 0f);
			group.blocksRaycasts = show;
		}
	}

	private void SetupMainListSelfSticky()
	{
		if ((Object)(object)((BaseController<UIRankListView>)this).view?.RankLoopList == (Object)null)
		{
			return;
		}
		if (mainListSelfSticky == null)
		{
			mainListSelfSticky = new RankMainListSelfSticky(((BaseController<UIRankListView>)this).view.RankLoopList, GetSelfListIndex, () => dataBuilder.SelfEntry, () => formalCollectIconSide);
		}
		mainListSelfSticky.Setup();
	}

	private void BindMainListSelfStickyScroll()
	{
		LoopListView2 val = ((BaseController<UIRankListView>)this).view?.RankLoopList;
		if ((Object)(object)((val != null) ? val.ScrollRect : null) == (Object)null)
		{
			return;
		}
		if (mainListScrollListener == null)
		{
			mainListScrollListener = delegate
			{
				UpdateMainListSelfSticky();
			};
		}
		((UnityEvent<Vector2>)(object)val.ScrollRect.onValueChanged).AddListener(mainListScrollListener);
	}

	private void UnbindMainListSelfStickyScroll()
	{
		LoopListView2 val = ((BaseController<UIRankListView>)this).view?.RankLoopList;
		if (!((Object)(object)val == (Object)null) && (Object)(object)val.ScrollRect != (Object)null && mainListScrollListener != null)
		{
			((UnityEvent<Vector2>)(object)val.ScrollRect.onValueChanged).RemoveListener(mainListScrollListener);
		}
	}

	private void BindSettleListScrollListener()
	{
		LoopListView2 val = ((BaseController<UIRankListView>)this).view?.SettlementLoopList;
		if ((Object)(object)((val != null) ? val.ScrollRect : null) == (Object)null)
		{
			return;
		}
		if (settleScrollListener == null)
		{
			settleScrollListener = delegate
			{
				CheckAndHideHeaderEffects();
			};
		}
		((UnityEvent<Vector2>)(object)val.ScrollRect.onValueChanged).AddListener(settleScrollListener);
	}

	private void UnbindSettleListScrollListener()
	{
		LoopListView2 val = ((BaseController<UIRankListView>)this).view?.SettlementLoopList;
		if ((Object)(object)((val != null) ? val.ScrollRect : null) != (Object)null && settleScrollListener != null)
		{
			((UnityEvent<Vector2>)(object)val.ScrollRect.onValueChanged).RemoveListener(settleScrollListener);
		}
	}

	private void CheckAndHideHeaderEffects()
	{
		LoopListView2 val = ((BaseController<UIRankListView>)this).view?.SettlementLoopList;
		if ((Object)(object)val == (Object)null)
		{
			return;
		}
		LoopListViewItem2 shownItemByItemIndex = val.GetShownItemByItemIndex(0);
		RankTopHeader rankTopHeader = ((!((Object)(object)shownItemByItemIndex != (Object)null)) ? cachedSettleHeader : (cachedSettleHeader = ((Component)shownItemByItemIndex).GetComponent<RankTopHeader>()));
		if (!((Object)(object)rankTopHeader == (Object)null))
		{
			if (IsItemFullyInViewport(val, shownItemByItemIndex))
			{
				settleHeaderEffectsHidden = false;
			}
			else if (!settleHeaderEffectsHidden)
			{
				rankTopHeader.HideAllRankEffects();
				settleHeaderEffectsHidden = true;
			}
		}
	}

	private bool IsItemFullyInViewport(LoopListView2 list, LoopListViewItem2 item)
	{
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)item == (Object)null)
		{
			return false;
		}
		list.ViewPortTrans.GetWorldCorners(viewportCheckCorners);
		float y = ((Transform)list.ContainerTrans).InverseTransformPoint(viewportCheckCorners[1]).y;
		float y2 = ((Transform)list.ContainerTrans).InverseTransformPoint(viewportCheckCorners[0]).y;
		if (item.TopY <= y)
		{
			return item.BottomY >= y2;
		}
		return false;
	}

	private void ScheduleRefreshMainListSelfSticky()
	{
		UpdateMainListSelfSticky();
		if (refreshMainSelfStickyCoroutine != null)
		{
			((MonoBehaviour)GameApp.Coroutine).StopCoroutine(refreshMainSelfStickyCoroutine);
		}
		refreshMainSelfStickyCoroutine = ((MonoBehaviour)GameApp.Coroutine).StartCoroutine(RefreshMainListSelfStickyNextFrame());
	}

	private IEnumerator RefreshMainListSelfStickyNextFrame()
	{
		yield return null;
		refreshMainSelfStickyCoroutine = null;
		UpdateMainListSelfSticky();
	}

	private void UpdateMainListSelfSticky()
	{
		if (rankType == ERankContent.Main)
		{
			mainListSelfSticky?.Update();
		}
	}

	private void OnCloseClick()
	{
		if (!isClosing)
		{
			RRDataTrack.Instance.ReportBtnClick("排行榜-返回主页");
			RRDataTrack.Instance.ReportPageShow("排行榜进主页");
			if (isEndSettlement)
			{
				HandleEndSettlementBackClick();
			}
			else
			{
				StartCloseSequence();
			}
		}
	}

	private void OnMainButtonClick()
	{
		if (isEndSettlement)
		{
			HandleEndSettlementButtonClick();
		}
		else
		{
			OnLevelClick();
		}
	}

	private void HandleEndSettlementButtonClick()
	{
		if (!isClosing && !rewardFlow.TryCompletePendingFly() && !isEndSettlementClaimStarted)
		{
			RankModelData rankModelData = GameApp.Proxy.Get<RankModelProxy>()?.Data;
			if (rankModelData == null || !rankModelData.IsPeriodSettlePending)
			{
				StartEndSettlementClose();
			}
			else if (RewardService.GetRewardId(rankModelData) > 0)
			{
				isEndSettlementClaimStarted = true;
				rewardFlow.ShowClaimReward(StartEndSettlementClose);
			}
			else
			{
				MarkPeriodSettledAndClose();
			}
		}
	}

	private void HandleEndSettlementBackClick()
	{
		if (!isClosing && !rewardFlow.TryCompletePendingFly() && !isEndSettlementClaimStarted)
		{
			RankModelData rankModelData = GameApp.Proxy.Get<RankModelProxy>()?.Data;
			if (rankModelData == null || RewardService.GetRewardId(rankModelData) <= 0)
			{
				MarkPeriodSettledAndClose();
				return;
			}
			isEndSettlementClaimStarted = true;
			rewardFlow.ShowClaimReward(StartEndSettlementClose);
		}
	}

	private void MarkPeriodSettledAndClose()
	{
		GameApp.Proxy.Get<RankModelProxy>()?.MarkPeriodSettled();
		StartEndSettlementClose();
	}

	private void StartEndSettlementClose()
	{
		if (!isClosing)
		{
			isClosing = true;
			((BaseController<UIRankListView>)this).CloseView(true);
			InvokeEndSettlementClosed();
		}
	}

	private void InvokeEndSettlementClosed()
	{
		onClosedCallback?.Invoke();
	}

	private void StartCloseSequence()
	{
		if (!isClosing)
		{
			isClosing = true;
			((BaseController<UIRankListView>)this).CloseView(true);
			RefreshHomeRankEntry();
		}
	}

	private void RefreshHomeRankEntry()
	{
		IController val = default(IController);
		if (rankType == ERankContent.Main && !isEndSettlement && GameApp.UI.IsOpen("UIHome", ref val) && val is UIHomeController uIHomeController)
		{
			uIHomeController.View?.RefreshActivityEntries();
		}
	}

	private void SetTitle()
	{
		SetLocalizedActivityTitle(((BaseController<UIRankListView>)this).view.txtTitle);
	}

	private void RefreshSettleTitle()
	{
		SetLocalizedActivityTitle(((BaseController<UIRankListView>)this).view.txtTitle_Settle);
	}

	private void SetLocalizedActivityTitle(TMP_Text titleText)
	{
		if (!((Object)(object)titleText == (Object)null))
		{
			RankModelProxy rankModelProxy = GameApp.Proxy.Get<RankModelProxy>();
			string text = ((rankModelProxy != null && rankModelProxy.Data?.IsHalloweenTheme == true) ? "Ring_Halloween_Title" : RankActivityEntry.Service?.GetCurRankListCfg()?.Name);
			if (!string.IsNullOrEmpty(text))
			{
				string text2 = GameApp.Lang.GetText(text, Array.Empty<object>());
				TMPTextExtensions.SetLocalizedTextValue(titleText, string.IsNullOrEmpty(text2) ? text : text2);
			}
		}
	}

	private void OnLanguageChangeSuccess(LanguageChangeSuccess evt)
	{
		RefreshUI();
	}

	private void RefreshUI()
	{
		SetTitle();
		RefreshSettleTitle();
		RefreshLevelText();
		UpdateCountdownText();
		if (rankType == ERankContent.Settle)
		{
			streakDisplayPresenter?.RefreshTexts();
			LoopListView2 settlementLoopList = ((BaseController<UIRankListView>)this).view.SettlementLoopList;
			if (settlementLoopList != null)
			{
				settlementLoopList.RefreshAllShownItem();
			}
		}
		else
		{
			LoopListView2 rankLoopList = ((BaseController<UIRankListView>)this).view.RankLoopList;
			if (rankLoopList != null)
			{
				rankLoopList.RefreshAllShownItem();
			}
		}
	}

	private void OnBtnTipClick()
	{
		RRDataTrack.Instance.ReportBtnClick("排行榜-玩法说明");
		GameApp.UI.Open(RankActivityEntry.GetRankListTipUIPath(), (UILayer)2, (object)null, (Action)null);
	}

	private void OnLevelClick()
	{
		if (!isClosing)
		{
			RRDataTrack.Instance.ReportBtnClick("排行榜-开始游戏");
			StartCloseSequenceForEnterLevel();
		}
	}

	private void StartCloseSequenceForEnterLevel()
	{
		if (!isClosing)
		{
			isClosing = true;
			EnterLevelCommand.PrepareOverlayEnterLevel();
			((BaseController<UIRankListView>)this).CloseView(true);
			((BaseController<UIRankListView>)this).ExecuteCommand<EnterLevelCommand>((object)new EnterLevelParam
			{
				SkipHomeCloseAnimation = true
			});
		}
	}

	private void RefreshLevelText()
	{
		RankModelData rankModelData = GameApp.Proxy.Get<RankModelProxy>()?.Data;
		int num;
		if (isEndSettlement && rankModelData != null)
		{
			num = ((RewardService.GetRewardId(rankModelData) > 0) ? 1 : 0);
			if (num != 0)
			{
				GameApp.Lang.GetText("Button_Claim", Array.Empty<object>());
			}
		}
		else
		{
			num = 0;
		}
		if (num != 0 && !string.IsNullOrEmpty("Button_Claim"))
		{
			if ((Object)(object)((BaseController<UIRankListView>)this).view.txtContinue != (Object)null)
			{
				TMPTextExtensions.SetLocalizedText(((BaseController<UIRankListView>)this).view.txtContinue, "Button_Claim", Array.Empty<object>());
			}
			if ((Object)(object)((BaseController<UIRankListView>)this).view.txtContinue_1 != (Object)null)
			{
				TMPTextExtensions.SetLocalizedText(((BaseController<UIRankListView>)this).view.txtContinue_1, "Button_Claim", Array.Empty<object>());
			}
		}
		else
		{
			if ((Object)(object)((BaseController<UIRankListView>)this).view.txtContinue != (Object)null)
			{
				TMPTextExtensions.SetLocalizedText(((BaseController<UIRankListView>)this).view.txtContinue, "Button_Continue", Array.Empty<object>());
			}
			if ((Object)(object)((BaseController<UIRankListView>)this).view.txtContinue_1 != (Object)null)
			{
				TMPTextExtensions.SetLocalizedText(((BaseController<UIRankListView>)this).view.txtContinue_1, "Button_Continue", Array.Empty<object>());
			}
		}
	}

	private void SetupHardStartButton()
	{
	}

	private void InitRankList()
	{
		if ((Object)(object)((BaseController<UIRankListView>)this).view?.RankLoopList == (Object)null)
		{
			Log.LogError("Rank", "[UIRankListController] RankLoopList is null");
			return;
		}
		int selfListIndex = GetSelfListIndex();
		if (!rankListInited)
		{
			((BaseController<UIRankListView>)this).view.RankLoopList.InitListView(dataBuilder.TotalItemCount, (Func<LoopListView2, int, LoopListViewItem2>)OnGetItemByIndex, (LoopListViewInitParam)null, (Func<int, ValueTuple<float, float>>)null);
			rankListInited = true;
		}
		else
		{
			((BaseController<UIRankListView>)this).view.RankLoopList.SetListItemCount(dataBuilder.TotalItemCount, false);
			((BaseController<UIRankListView>)this).view.RankLoopList.RefreshAllShownItem();
		}
		if (selfListIndex >= 0 && dataBuilder.TotalItemCount > 0)
		{
			((BaseController<UIRankListView>)this).view.RankLoopList.MovePanelToItemIndex(selfListIndex, 0f, 0f);
			ScheduleCenterItemToViewport(((BaseController<UIRankListView>)this).view.RankLoopList, selfListIndex);
		}
	}

	private LoopListViewItem2 OnGetItemByIndex(LoopListView2 listView, int index)
	{
		if (index < 0 || index >= dataBuilder.TotalItemCount || index >= dataBuilder.ListEntries.Count)
		{
			return null;
		}
		RankItemData rankItemData = dataBuilder.ListEntries[index];
		LoopListViewItem2 val = listView.NewListViewItem("RankItem");
		if ((Object)(object)val == (Object)null)
		{
			return null;
		}
		RankItem component = ((Component)val).GetComponent<RankItem>();
		if ((Object)(object)component != (Object)null)
		{
			component.SetSettleTopThree(value: false);
			component.SetRankType(ERankContent.Main);
			component.InitializeCollectPosition(topHeader: false, formalCollectIconSide);
			component.Render(rankItemData);
			component.OnRewardBoxClicked = rewardFlow.ShowRewardBoxPreview;
		}
		if (rankItemData.IsSelf)
		{
			UpdateMainListSelfSticky();
		}
		val.ItemId = index;
		return val;
	}

	private int GetSelfListIndex()
	{
		for (int i = 0; i < dataBuilder.ListEntries.Count; i++)
		{
			if (dataBuilder.ListEntries[i].IsSelf)
			{
				return i;
			}
		}
		return -1;
	}

	private void StaggerShowItems(LoopListView2 list)
	{
		if (!((Object)(object)list == (Object)null))
		{
			if (staggerCoroutine != null)
			{
				((MonoBehaviour)GameApp.Coroutine).StopCoroutine(staggerCoroutine);
			}
			staggerCoroutine = ((MonoBehaviour)GameApp.Coroutine).StartCoroutine(StaggerShowItemsCoroutine(list));
		}
	}

	private IEnumerator StaggerShowItemsCoroutine(LoopListView2 list)
	{
		int shownCount = list.ShownItemCount;
		for (int i = 0; i < shownCount; i++)
		{
			LoopListViewItem2 shownItemByIndex = list.GetShownItemByIndex(i);
			(((Object)(object)shownItemByIndex != (Object)null) ? ((Component)shownItemByIndex).GetComponent<RankItem>() : null)?.SetVisible(visible: false);
		}
		float num = (((Object)(object)RankListConfig.Instance != (Object)null) ? RankListConfig.Instance.itemShowInterval : 0.2f);
		WaitForSeconds waitInterval = new WaitForSeconds(num);
		for (int j = 0; j < shownCount; j++)
		{
			LoopListViewItem2 shownItemByIndex2 = list.GetShownItemByIndex(j);
			RankItem rankItem = (((Object)(object)shownItemByIndex2 != (Object)null) ? ((Component)shownItemByIndex2).GetComponent<RankItem>() : null);
			if (!((Object)(object)rankItem == (Object)null))
			{
				rankItem.SetVisible(visible: true);
				rankItem.PlayRankItemAnimation(RankItem.ERanItemAnimType.ItemOpen);
				yield return waitInterval;
			}
		}
		staggerCoroutine = null;
	}

	private void FillTopRankItems()
	{
		RankTopHeader mainRankTopHeader = GetMainRankTopHeader();
		RankItem[] array = new RankItem[3]
		{
			mainRankTopHeader?.FirstRankItem,
			mainRankTopHeader?.SecondRankItem,
			mainRankTopHeader?.ThirdRankItem
		};
		for (int i = 0; i < array.Length; i++)
		{
			if (!((Object)(object)array[i] == (Object)null))
			{
				if (i < dataBuilder.AllEntries.Count)
				{
					array[i].SetSettleTopThree(value: false);
					array[i].SetRankType(ERankContent.Main);
					array[i].InitializeCollectPosition(topHeader: true, formalCollectIconSide);
					array[i].Render(dataBuilder.AllEntries[i]);
				}
				else
				{
					((Component)array[i]).gameObject.SetActive(false);
				}
				array[i].OnRewardBoxClicked = rewardFlow.ShowRewardBoxPreview;
			}
		}
		RefreshTopRewardBoxExperimentVisualsIfReady();
	}

	private void ScheduleTopRewardBoxExperimentReveal()
	{
		timerIds.Add(GameApp.Timer.Delay(0.8f, (Action)delegate
		{
			if (!isClosing && ((BaseController<UIRankListView>)this).view != null)
			{
				rewardBoxEffectsRevealReady = true;
				RefreshTopRewardBoxExperimentVisuals();
			}
		}));
	}

	private void RefreshTopRewardBoxExperimentVisuals()
	{
		RevealRewardBoxExperimentVisualsOnHeader(GetActiveRankTopHeader());
	}

	private RankTopHeader GetMainRankTopHeader()
	{
		if ((Object)(object)((BaseController<UIRankListView>)this).view?.FirstRankItem == (Object)null)
		{
			return null;
		}
		return ((Component)((BaseController<UIRankListView>)this).view.FirstRankItem).GetComponentInParent<RankTopHeader>(true);
	}

	private RankTopHeader GetActiveRankTopHeader()
	{
		if (rankType == ERankContent.Settle)
		{
			return GetSettleRankTopHeaderForRewardBoxFx();
		}
		return GetMainRankTopHeader();
	}

	private RankTopHeader GetSettleRankTopHeaderForRewardBoxFx()
	{
		if ((Object)(object)((BaseController<UIRankListView>)this).view?.SettlementLoopList == (Object)null)
		{
			return cachedSettleHeader;
		}
		LoopListViewItem2 shownItemByItemIndex = ((BaseController<UIRankListView>)this).view.SettlementLoopList.GetShownItemByItemIndex(0);
		RankTopHeader settleTopHeader = GetSettleTopHeader(shownItemByItemIndex);
		if ((Object)(object)settleTopHeader != (Object)null)
		{
			cachedSettleHeader = settleTopHeader;
			return settleTopHeader;
		}
		return cachedSettleHeader;
	}

	private void RevealRewardBoxExperimentVisualsOnHeader(RankTopHeader header)
	{
		if (!((Object)(object)header == (Object)null))
		{
			header.FirstRankItem?.RevealRewardBoxExperimentVisuals();
			header.SecondRankItem?.RevealRewardBoxExperimentVisuals();
			header.ThirdRankItem?.RevealRewardBoxExperimentVisuals();
		}
	}

	private void RefreshTopRewardBoxExperimentVisualsIfReady()
	{
		if (rewardBoxEffectsRevealReady)
		{
			RefreshTopRewardBoxExperimentVisuals();
		}
	}

	private IEnumerator RegisterSettle()
	{
		settleAnimator = new RankSettleAnimator(this);
		streakDisplayPresenter = new RankStreakDisplayPresenter(((BaseController<UIRankListView>)this).view, delegate(int targetIndex)
		{
			settleAnimator?.PlayCollectionShow(targetIndex);
		});
		((BaseController<UIRankListView>)this).view.PlaySettlePanelOpen();
		ScheduleTopRewardBoxExperimentReveal();
		PlayBottomAnimationOnSettleOpenIfNeeded();
		if ((Object)(object)((BaseController<UIRankListView>)this).view.btnContinue != (Object)null)
		{
			((UnityEventBase)((BaseController<UIRankListView>)this).view.btnContinue.onClick).RemoveAllListeners();
			((UnityEvent)((BaseController<UIRankListView>)this).view.btnContinue.onClick).AddListener(new UnityAction(OnContinueClick));
		}
		SetMaskActive(active: true);
		settleTotalCount = GetSettleLoopItemCount();
		settleListCanvasGroup = GetOrAddListCanvasGroup(((BaseController<UIRankListView>)this).view.SettlementLoopList);
		SetCanvasGroupShow(settleListCanvasGroup, show: false);
		RankItem.InitializeMainVisualStyle(((BaseController<UIRankListView>)this).view.ListRankItemSelf, ((BaseController<UIRankListView>)this).view.RankItem);
		InitSettlementList();
		RankModelProxy rankModelProxy = GameApp.Proxy.Get<RankModelProxy>();
		int valueOrDefault = (rankModelProxy?.Data?.CurrentSettleScore).GetValueOrDefault();
		bool isZeroScoreSettlement = valueOrDefault <= 0;
		if ((Object)(object)((BaseController<UIRankListView>)this).view.Score != (Object)null)
		{
			TMPTextExtensions.SetLocalizedTextValue(((BaseController<UIRankListView>)this).view.Score, $"+{valueOrDefault}");
		}
		if ((Object)(object)((BaseController<UIRankListView>)this).view.AddScore != (Object)null)
		{
			((Component)((BaseController<UIRankListView>)this).view.AddScore).gameObject.SetActive(false);
		}
		if ((Object)(object)((BaseController<UIRankListView>)this).view.imgCollection != (Object)null)
		{
			((Component)((BaseController<UIRankListView>)this).view.imgCollection).gameObject.SetActive(false);
		}
		if (rankModelProxy != null)
		{
			if (rankModelProxy.IsRankActiveThisRound())
			{
				streakDisplayPresenter.Play(rankModelProxy);
			}
			else if (rankModelProxy.Data?.IsPeriodSettlePending ?? false)
			{
				streakDisplayPresenter.Play(rankModelProxy, skipCollectionPresentation: true);
			}
			else
			{
				streakDisplayPresenter.Play(rankModelProxy);
			}
		}
		RefreshSettleTopRankItems();
		RefreshTopRewardBoxExperimentVisualsIfReady();
		yield return (object)new WaitForSeconds(0.25f);
		AnimateTopThreeSettleOpen();
		yield return (object)new WaitForSeconds(0.1f);
		SetCanvasGroupShow(settleListCanvasGroup, show: true);
		StaggerShowItems(((BaseController<UIRankListView>)this).view.SettlementLoopList);
		while (staggerCoroutine != null)
		{
			yield return null;
		}
		BindSettleListScrollListener();
		if (isZeroScoreSettlement)
		{
			yield return (object)new WaitForSeconds(0.5f);
			settleAnimator.PlayZeroScoreSettlement();
		}
		registerSettleCoroutineId = -1;
	}

	private void AnimateTopThreeSettleOpen()
	{
		LoopListView2 settlementLoopList = ((BaseController<UIRankListView>)this).view.SettlementLoopList;
		LoopListViewItem2 obj = ((settlementLoopList != null) ? settlementLoopList.GetShownItemByItemIndex(0) : null);
		RankTopHeader rankTopHeader = ((obj != null) ? ((Component)obj).GetComponent<RankTopHeader>() : null);
		if ((Object)(object)rankTopHeader == (Object)null)
		{
			return;
		}
		RankItem[] array = new RankItem[3] { rankTopHeader.FirstRankItem, rankTopHeader.SecondRankItem, rankTopHeader.ThirdRankItem };
		for (int i = 0; i < array.Length; i++)
		{
			if ((Object)(object)array[i] != (Object)null && ((Component)array[i]).gameObject.activeSelf)
			{
				array[i].SetVisible(visible: true);
				array[i].PlayRankItemAnimation(RankItem.ERanItemAnimType.ItemOpen);
			}
		}
	}

	private void PlayBottomAnimationOnSettleOpenIfNeeded()
	{
		if (RankFunctionResolver.ShouldPlayBottomAnimationOnSettleOpen())
		{
			((BaseController<UIRankListView>)this).view.PlayBottomAnimation();
			isBottomAnimationPlayedOnSettleOpen = true;
		}
	}

	public void OnSettleAnimationCompleted()
	{
		RankActivityService service = RankActivityEntry.Service;
		if (service != null && service.IsPeriodSettlePending)
		{
			RankActivityEntry.Service?.TryReportCampaignEnd();
		}
		SetMaskActive(active: false);
		if (!isBottomAnimationPlayedOnSettleOpen)
		{
			((BaseController<UIRankListView>)this).view.PlayBottomAnimation();
		}
	}

	private void InitSettlementList()
	{
		using (new ProfilerScope("UIRankListController.InitSettlementList"))
		{
			if ((Object)(object)((BaseController<UIRankListView>)this).view?.SettlementLoopList == (Object)null)
			{
				Log.LogError("Rank", "[UIRankListController] SettlementLoopList is null");
				return;
			}
			if (!settleListInited)
			{
				((BaseController<UIRankListView>)this).view.SettlementLoopList.InitListView(settleTotalCount, (Func<LoopListView2, int, LoopListViewItem2>)OnGetSettleItemByIndex, (LoopListViewInitParam)null, (Func<int, ValueTuple<float, float>>)null);
				settleListInited = true;
			}
			else
			{
				((BaseController<UIRankListView>)this).view.SettlementLoopList.SetListItemCount(settleTotalCount, false);
				((BaseController<UIRankListView>)this).view.SettlementLoopList.RefreshAllShownItem();
			}
			((BaseController<UIRankListView>)this).view.SettlementLoopList.MovePanelToItemIndexImmediately(0, 0f);
			((BaseController<UIRankListView>)this).view.SettlementLoopList.RefreshAllShownItem();
			LoopListViewItem2 shownItemByItemIndex = ((BaseController<UIRankListView>)this).view.SettlementLoopList.GetShownItemByItemIndex(0);
			cachedSettleHeader = ((shownItemByItemIndex != null) ? ((Component)shownItemByItemIndex).GetComponent<RankTopHeader>() : null);
			int selfDataIndex = GetSelfDataIndex();
			int num = SettleDataIndexToLoopIndex(selfDataIndex);
			if (num >= 0)
			{
				((BaseController<UIRankListView>)this).view.SettlementLoopList.MovePanelToItemIndex(num, 0f, 0f);
				ScheduleCenterItemToViewport(((BaseController<UIRankListView>)this).view.SettlementLoopList, num);
			}
			RefreshSettleTopRankItems();
		}
	}

	private void RefreshSettleTopRankItems()
	{
		LoopListView2 settlementLoopList = ((BaseController<UIRankListView>)this).view.SettlementLoopList;
		LoopListViewItem2 headerItem = ((settlementLoopList != null) ? settlementLoopList.GetShownItemByItemIndex(0) : null);
		FillSettleTopRankItems(headerItem);
	}

	private void CancelPendingCenterItem()
	{
		centerItemRequestVersion++;
		if (centerItemCoroutineId >= 0)
		{
			GameApp.Coroutine.Kill(centerItemCoroutineId);
			centerItemCoroutineId = -1;
		}
	}

	private void ScheduleCenterItemToViewport(LoopListView2 list, int itemIndex)
	{
		if (!((Object)(object)list == (Object)null) && itemIndex >= 0)
		{
			if (centerItemCoroutineId >= 0)
			{
				GameApp.Coroutine.Kill(centerItemCoroutineId);
			}
			int requestVersion = ++centerItemRequestVersion;
			centerItemCoroutineId = GameApp.Coroutine.Run(CenterItemToViewportNextFrame(list, itemIndex, requestVersion));
		}
	}

	private IEnumerator CenterItemToViewportNextFrame(LoopListView2 list, int itemIndex, int requestVersion)
	{
		yield return null;
		if (requestVersion == centerItemRequestVersion && !isClosing && !((Object)(object)list == (Object)null))
		{
			float centeredItemOffset = GetCenteredItemOffset(list, itemIndex);
			if (Mathf.Abs(centeredItemOffset) > 0.01f)
			{
				list.MovePanelByOffset(centeredItemOffset);
			}
			if (requestVersion == centerItemRequestVersion)
			{
				centerItemCoroutineId = -1;
			}
		}
	}

	private static float GetCenteredItemOffset(LoopListView2 list, int itemIndex)
	{
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)list == (Object)null || itemIndex < 0)
		{
			return 0f;
		}
		LoopListViewItem2 shownItemByItemIndex = list.GetShownItemByItemIndex(itemIndex);
		if ((Object)(object)shownItemByItemIndex == (Object)null)
		{
			return 0f;
		}
		if (!list.IsVertList)
		{
			return 0f;
		}
		RectTransform viewPortTrans = list.ViewPortTrans;
		RectTransform containerTrans = list.ContainerTrans;
		if ((Object)(object)viewPortTrans == (Object)null || (Object)(object)containerTrans == (Object)null)
		{
			return 0f;
		}
		Vector3[] array = (Vector3[])(object)new Vector3[4];
		viewPortTrans.GetWorldCorners(array);
		float num = (((Transform)containerTrans).InverseTransformPoint(array[1]).y + ((Transform)containerTrans).InverseTransformPoint(array[0]).y) * 0.5f;
		float num2 = (shownItemByItemIndex.TopY + shownItemByItemIndex.BottomY) * 0.5f;
		return num - num2;
	}

	public int GetSettleLoopItemCount()
	{
		if (dataBuilder?.AllEntries == null)
		{
			return 0;
		}
		int num = Mathf.Max(0, dataBuilder.AllEntries.Count - 3);
		return 1 + num;
	}

	public int SettleDataIndexToLoopIndex(int dataIndex)
	{
		if (dataIndex < 0)
		{
			return -1;
		}
		if (dataIndex < 3)
		{
			return 0;
		}
		return dataIndex - 3 + 1;
	}

	private int SettleLoopIndexToDataIndex(int loopIndex)
	{
		if (loopIndex <= 0)
		{
			return -1;
		}
		return loopIndex + 3 - 1;
	}

	public RankItem GetSettleRankItem(LoopListViewItem2 listItem, int loopIndex, RankItemData entry)
	{
		if ((Object)(object)listItem == (Object)null)
		{
			return null;
		}
		if (loopIndex == 0)
		{
			return GetSettleTopHeader(listItem)?.GetRankItemForEntry(entry);
		}
		return ((Component)listItem).GetComponent<RankItem>();
	}

	private RankTopHeader GetSettleTopHeader(LoopListViewItem2 headerItem)
	{
		if (!((Object)(object)headerItem != (Object)null))
		{
			return null;
		}
		return ((Component)headerItem).GetComponent<RankTopHeader>();
	}

	private void FillSettleTopRankItems(LoopListViewItem2 headerItem)
	{
		RankTopHeader settleTopHeader = GetSettleTopHeader(headerItem);
		if (!((Object)(object)settleTopHeader == (Object)null))
		{
			cachedSettleHeader = settleTopHeader;
			settleTopHeader.Render(dataBuilder.AllEntries, rankType, rewardFlow.ShowRewardBoxPreview, formalCollectIconSide);
			RefreshTopRewardBoxExperimentVisualsIfReady();
		}
	}

	private LoopListViewItem2 OnGetSettleItemByIndex(LoopListView2 listView, int index)
	{
		using (new ProfilerScope("UIRankListController.OnGetSettleItemByIndex"))
		{
			if (index < 0 || index >= settleTotalCount)
			{
				return null;
			}
			if (index == 0)
			{
				LoopListViewItem2 val = listView.NewListViewItem("RankTopHeader");
				if ((Object)(object)val == (Object)null)
				{
					return null;
				}
				FillSettleTopRankItems(val);
				val.ItemId = index;
				return val;
			}
			int num = SettleLoopIndexToDataIndex(index);
			if (num < 0 || num >= dataBuilder.AllEntries.Count)
			{
				return null;
			}
			RankItemData entry = dataBuilder.AllEntries[num];
			LoopListViewItem2 val2 = listView.NewListViewItem("RankItem");
			if ((Object)(object)val2 == (Object)null)
			{
				return null;
			}
			RankItem component = ((Component)val2).GetComponent<RankItem>();
			if ((Object)(object)component != (Object)null)
			{
				component.SetSettleTopThree(value: false);
				component.SetRankType(rankType);
				component.InitializeCollectPosition(topHeader: false, formalCollectIconSide);
				component.Render(entry);
				component.OnRewardBoxClicked = rewardFlow.ShowRewardBoxPreview;
			}
			val2.ItemId = index;
			return val2;
		}
	}

	public int GetSelfDataIndex()
	{
		return dataBuilder.AllEntries.FindIndex((RankItemData e) => e?.IsSelf ?? false);
	}

	private void OnContinueClick()
	{
		if (isClosing || rewardFlow.TryCompletePendingFly() || isSettleClaimStarted)
		{
			return;
		}
		RankModelData rankModelData = GameApp.Proxy.Get<RankModelProxy>()?.Data;
		bool flag = rankModelData != null && rankModelData.IsPeriodSettlePending && RewardService.GetRewardId(rankModelData) > 0;
		StopSettlePresentationForContinue(flag);
		RRDataTrack.Instance.ReportBtnClick("排行榜-结算-点击继续按钮");
		if (rankModelData != null && rankModelData.IsPeriodSettlePending)
		{
			RankActivityEntry.Service?.TryReportCampaignEnd();
			if (flag)
			{
				isSettleClaimStarted = true;
				rewardFlow.ShowClaimReward(CloseSettleAfterClaim);
				return;
			}
			GameApp.Proxy.Get<RankModelProxy>()?.MarkPeriodSettled();
		}
		if (rankModelData != null && rankModelData.IsActivityEnd && rankModelData.IsEndSettlementProcessed)
		{
			RankActivityEntry.EnsureBuildActivityData();
		}
		StartSettleCloseAndContinue();
	}

	private void StopSettlePresentationForContinue(bool shouldRefreshToFinalState)
	{
		if (registerSettleCoroutineId >= 0)
		{
			GameApp.Coroutine.Kill(registerSettleCoroutineId);
			registerSettleCoroutineId = -1;
		}
		if (staggerCoroutine != null)
		{
			((MonoBehaviour)GameApp.Coroutine).StopCoroutine(staggerCoroutine);
			staggerCoroutine = null;
		}
		for (int i = 0; i < timerIds.Count; i++)
		{
			GameApp.Timer.Kill(timerIds[i]);
		}
		timerIds.Clear();
		streakDisplayPresenter?.Cleanup();
		if (shouldRefreshToFinalState)
		{
			settleAnimator?.SkipToSettlementEnd();
		}
		CleanupSettleEffects();
		((BaseController<UIRankListView>)this).view.ResetBottomCommonEffect();
	}

	private void StartSettleCloseAndContinue()
	{
		if (!isClosing)
		{
			isClosing = true;
			((BaseController<UIRankListView>)this).CloseView(true);
			InvokeSettleClosed();
		}
	}

	private void CloseSettleAfterClaim()
	{
		RankModelData rankModelData = GameApp.Proxy.Get<RankModelProxy>()?.Data;
		if (rankModelData == null || !rankModelData.IsEndSettlementProcessed)
		{
			isSettleClaimStarted = false;
			return;
		}
		RankActivityEntry.EnsureBuildActivityData();
		StartSettleCloseAndContinue();
	}

	private void InvokeSettleClosed()
	{
		onClosedCallback?.Invoke();
	}

	private void SetMaskActive(bool active)
	{
		if ((Object)(object)((BaseController<UIRankListView>)this).view.SettleMask != (Object)null)
		{
			((BaseController<UIRankListView>)this).view.SettleMask.SetActive(active);
		}
	}

	private void StartCountdown()
	{
		StopCountdown();
		UpdateCountdownText();
		countdownTimerId = GameApp.Timer.Repeat(1f, (Action)UpdateCountdownText);
	}

	private void StopCountdown()
	{
		if (countdownTimerId >= 0)
		{
			GameApp.Timer.Kill(countdownTimerId);
			countdownTimerId = -1;
		}
	}

	private void UpdateCountdownText()
	{
		if ((Object)(object)((BaseController<UIRankListView>)this).view?.txtCountDown == (Object)null)
		{
			return;
		}
		long num = RankActivityEntry.Service?.GetRemainSeconds() ?? 0;
		if (rankType == ERankContent.Main && !isEndSettlement)
		{
			RankActivityService service = RankActivityEntry.Service;
			if (service != null && service.IsPeriodSettlePending)
			{
				isEndSettlement = true;
				RefreshLevelText();
			}
		}
		TMPTextExtensions.SetLocalizedTextValue(((BaseController<UIRankListView>)this).view.txtCountDown, RankActivityService.FormatCountdown(num));
		if (num <= 0)
		{
			StopCountdown();
		}
	}

	public override void OnEscape()
	{
		if (rankType == ERankContent.Main && !isClosing)
		{
			if (isEndSettlement)
			{
				HandleEndSettlementBackClick();
			}
			else
			{
				StartCloseSequence();
			}
		}
	}

	public override void OnRemove()
	{
		isRemoved = true;
		NotifyPrewarmReady(success: false);
		TipsService.CloseIfOpen();
		StopCountdown();
		CancelPendingCenterItem();
		rewardFlow?.Cleanup();
		rewardFlow = null;
		if (registerMainCoroutineId >= 0)
		{
			GameApp.Coroutine.Kill(registerMainCoroutineId);
			registerMainCoroutineId = -1;
		}
		if (registerSettleCoroutineId >= 0)
		{
			GameApp.Coroutine.Kill(registerSettleCoroutineId);
			registerSettleCoroutineId = -1;
		}
		UnbindMainListSelfStickyScroll();
		UnbindSettleListScrollListener();
		if (refreshMainSelfStickyCoroutine != null)
		{
			((MonoBehaviour)GameApp.Coroutine).StopCoroutine(refreshMainSelfStickyCoroutine);
			refreshMainSelfStickyCoroutine = null;
		}
		mainListSelfSticky?.Teardown();
		mainListSelfSticky = null;
		if (staggerCoroutine != null)
		{
			((MonoBehaviour)GameApp.Coroutine).StopCoroutine(staggerCoroutine);
			staggerCoroutine = null;
		}
		for (int i = 0; i < timerIds.Count; i++)
		{
			GameApp.Timer.Kill(timerIds[i]);
		}
		timerIds.Clear();
		streakDisplayPresenter?.Cleanup();
		streakDisplayPresenter = null;
		settleAnimator?.Cleanup();
		settleAnimator = null;
		CleanupSettleEffects();
		if ((Object)(object)((BaseController<UIRankListView>)this).view.btnBack != (Object)null)
		{
			((UnityEventBase)((BaseController<UIRankListView>)this).view.btnBack.onClick).RemoveAllListeners();
		}
		if ((Object)(object)((BaseController<UIRankListView>)this).view.btnTip != (Object)null)
		{
			((UnityEventBase)((BaseController<UIRankListView>)this).view.btnTip.onClick).RemoveAllListeners();
		}
		if ((Object)(object)((BaseController<UIRankListView>)this).view.btnLevel != (Object)null)
		{
			((UnityEventBase)((BaseController<UIRankListView>)this).view.btnLevel.onClick).RemoveAllListeners();
		}
		if ((Object)(object)((BaseController<UIRankListView>)this).view.btnContinue != (Object)null)
		{
			((UnityEventBase)((BaseController<UIRankListView>)this).view.btnContinue.onClick).RemoveAllListeners();
		}
		if ((Object)(object)((BaseController<UIRankListView>)this).view.FirstRankItem != (Object)null)
		{
			((BaseController<UIRankListView>)this).view.FirstRankItem.OnRewardBoxClicked = null;
		}
		if ((Object)(object)((BaseController<UIRankListView>)this).view.SecondRankItem != (Object)null)
		{
			((BaseController<UIRankListView>)this).view.SecondRankItem.OnRewardBoxClicked = null;
		}
		if ((Object)(object)((BaseController<UIRankListView>)this).view.ThirdRankItem != (Object)null)
		{
			((BaseController<UIRankListView>)this).view.ThirdRankItem.OnRewardBoxClicked = null;
		}
		LoopListView2 settlementLoopList = ((BaseController<UIRankListView>)this).view.SettlementLoopList;
		LoopListViewItem2 obj = ((settlementLoopList != null) ? settlementLoopList.GetShownItemByItemIndex(0) : null);
		RankTopHeader rankTopHeader = ((obj != null) ? ((Component)obj).GetComponent<RankTopHeader>() : null);
		if ((Object)(object)rankTopHeader != (Object)null)
		{
			if ((Object)(object)rankTopHeader.FirstRankItem != (Object)null)
			{
				rankTopHeader.FirstRankItem.OnRewardBoxClicked = null;
			}
			if ((Object)(object)rankTopHeader.SecondRankItem != (Object)null)
			{
				rankTopHeader.SecondRankItem.OnRewardBoxClicked = null;
			}
			if ((Object)(object)rankTopHeader.ThirdRankItem != (Object)null)
			{
				rankTopHeader.ThirdRankItem.OnRewardBoxClicked = null;
			}
		}
		cachedSettleHeader = null;
		((BaseController<UIRankListView>)this).UnlistenEvent<LanguageChangeSuccess>((Action<LanguageChangeSuccess>)OnLanguageChangeSuccess);
	}

	private void HideHomeRankEntryOnOpen()
	{
		RankListHomeEntry.SetEntryVisible(visible: false);
	}

	private void CleanupSettleEffects()
	{
		((BaseController<UIRankListView>)this).view.HideCaidaiObj();
		((BaseController<UIRankListView>)this).view.HideTopUI();
		LoopListView2 settlementLoopList = ((BaseController<UIRankListView>)this).view.SettlementLoopList;
		LoopListViewItem2 obj = ((settlementLoopList != null) ? settlementLoopList.GetShownItemByItemIndex(0) : null);
		RankTopHeader rankTopHeader = ((obj != null) ? ((Component)obj).GetComponent<RankTopHeader>() : null);
		if ((Object)(object)rankTopHeader != (Object)null)
		{
			rankTopHeader.HideAllRankEffects();
			rankTopHeader.FirstRankItem?.ResetHeadRoot();
			rankTopHeader.SecondRankItem?.ResetHeadRoot();
			rankTopHeader.ThirdRankItem?.ResetHeadRoot();
		}
		LoopListView2 settlementLoopList2 = ((BaseController<UIRankListView>)this).view.SettlementLoopList;
		if ((Object)(object)settlementLoopList2 != (Object)null)
		{
			int shownItemCount = settlementLoopList2.ShownItemCount;
			for (int i = 0; i < shownItemCount; i++)
			{
				LoopListViewItem2 shownItemByIndexWithoutCheck = settlementLoopList2.GetShownItemByIndexWithoutCheck(i);
				RankItem rankItem = ((shownItemByIndexWithoutCheck != null) ? ((Component)shownItemByIndexWithoutCheck).GetComponent<RankItem>() : null);
				if (!((Object)(object)rankItem == (Object)null))
				{
					rankItem.ResetHeadRoot();
				}
			}
		}
		GameObject val = GameObject.Find("eff_imghead_yuan_gp1");
		if ((Object)(object)val != (Object)null)
		{
			val.SetActive(false);
		}
	}

	public void RefreshTopRankItems()
	{
		RefreshSettleTopRankItems();
	}
}
