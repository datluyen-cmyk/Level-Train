using System;
using Game.DataTrack;
using Game.UI;
using RotateRings.UI.Toast;
using Rotaterings.Rank;
using Rotaterings.UI.RankList;
using Spine;
using Spine.Unity;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using XingYun.Extensions;
using XingYun.UI.Controller;
using XingYun.UI.Core;
using XingYun.Utility;

namespace RotateRings.UI.RankList;

public class UIButtonItemRankListView : GameWidgetView
{
	private const string UnlockLevelKey = "Booster_UnlockPrompt";

	private const string AnimShow = "show";

	private const int TrackIndex = 0;

	public Button ButtonItem;

	public Image Bg;

	public Image Icon;

	public SkeletonGraphic icon_spine;

	public TMP_Text Desc;

	public TMP_Text LockDesc;

	public Image IconLock;

	public TMP_Text txtRank;

	private Action onClick;

	private bool isClickBound;

	private int countdownTimerId = -1;

	private TrackEntry iconSpinePlayEntry;

	private TrackEntryDelegate iconSpineCompleteHandler;

	private Action iconSpinePlayCompleteCallback;

	private int iconSpineCompleteTimerId = -1;

	private bool iconSpineStaticInitialized;

	public override void Inject(UIBehaviour behaviour)
	{
		base.Inject(behaviour);
		ButtonItem = behaviour.Get<Button>("ButtonItem");
		Bg = behaviour.Get<Image>("Bg");
		Icon = behaviour.Get<Image>("Icon");
		icon_spine = behaviour.Get<SkeletonGraphic>("icon_spine");
		Desc = behaviour.Get<TMP_Text>("Desc");
		LockDesc = behaviour.Get<TMP_Text>("LockDesc");
		IconLock = behaviour.Get<Image>("IconLock");
		txtRank = behaviour.Get<TMP_Text>("txtRank");
	}

	public void BindEvents(Action clickHandler)
	{
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Expected O, but got Unknown
		onClick = clickHandler;
		if (!((Object)(object)ButtonItem == (Object)null) && !isClickBound)
		{
			((UnityEvent)ButtonItem.onClick).AddListener(new UnityAction(OnButtonClick));
			isClickBound = true;
		}
	}

	public void Refresh()
	{
		StopCountdown();
		RefreshDisplay();
		StartCountdownIfNeeded();
	}

	public bool TryPlayIconSpineAnimation(Action onComplete)
	{
		//IL_00c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d1: Expected O, but got Unknown
		if ((Object)(object)icon_spine == (Object)null)
		{
			return false;
		}
		if ((Object)(object)((UIBaseView)this).Behaviour != (Object)null && !((Component)((UIBaseView)this).Behaviour).gameObject.activeInHierarchy)
		{
			return false;
		}
		if (!((Component)icon_spine).gameObject.activeInHierarchy)
		{
			return false;
		}
		icon_spine.Initialize(false);
		Skeleton skeleton = icon_spine.Skeleton;
		SkeletonData val = ((skeleton != null) ? skeleton.Data : null);
		if (val == null || val.FindAnimation("show") == null)
		{
			return false;
		}
		AnimationState animationState = icon_spine.AnimationState;
		if (animationState == null)
		{
			return false;
		}
		KillIconSpineCompleteFallback();
		UnsubscribeIconSpineComplete();
		icon_spine.freeze = false;
		iconSpinePlayCompleteCallback = onComplete;
		iconSpinePlayEntry = animationState.SetAnimation(0, "show", false);
		iconSpineCompleteHandler = (TrackEntryDelegate)delegate
		{
			OnIconSpinePlayFinished();
		};
		iconSpinePlayEntry.Complete += iconSpineCompleteHandler;
		ScheduleIconSpineCompleteFallback(iconSpinePlayEntry.Animation.Duration);
		return true;
	}

	public void StopIconSpineAnimation()
	{
		iconSpinePlayCompleteCallback = null;
		KillIconSpineCompleteFallback();
		UnsubscribeIconSpineComplete();
		ResetIconSpineToStatic();
	}

	public override void Dispose()
	{
		StopCountdown();
		UnbindEvents();
		StopIconSpineAnimation();
		base.Dispose();
	}

	private void OnButtonClick()
	{
		if (!CanOpenRank())
		{
			ToastService.Show(GetLevelText(GetRankOpenLevel()));
			RRDataTrack.Instance.ReportBtnClick("主页-排行榜-未解锁");
		}
		else
		{
			onClick?.Invoke();
		}
	}

	private void UnbindEvents()
	{
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Expected O, but got Unknown
		if (isClickBound && !((Object)(object)ButtonItem == (Object)null))
		{
			((UnityEvent)ButtonItem.onClick).RemoveListener(new UnityAction(OnButtonClick));
			isClickBound = false;
			onClick = null;
		}
	}

	private void RefreshDisplay()
	{
		RankModelData rankModelData = GameApp.Proxy.Get<RankModelProxy>()?.Data;
		bool num = rankModelData != null && rankModelData.HasActivityRecord && rankModelData.IsActivityEnd && rankModelData.IsEndSettlementProcessed;
		IController val = default(IController);
		bool flag = GameApp.UI.IsOpen(RankActivityEntry.GetRankListUIPath(), ref val) && val != null && (!(val is UIRankListController uIRankListController) || !uIRankListController.IsPrewarmOpen);
		if (num || (RankActivityEntry.Service?.IsPeriodSettlePending ?? false) || flag)
		{
			SetRootVisible(visible: false);
			return;
		}
		SetRootVisible(visible: true);
		SetupVisualElements();
		RankActivityEntry.Service?.EnsureActivityState();
		RefreshRankText();
		if (!CanOpenRank())
		{
			RefreshLockedState(GetRankOpenLevel());
		}
		else
		{
			RefreshUnlockedState();
		}
	}

	private void SetupVisualElements()
	{
		if ((Object)(object)Icon != (Object)null)
		{
			((Component)Icon).gameObject.SetActive(false);
		}
		if ((Object)(object)icon_spine != (Object)null)
		{
			((Component)icon_spine).gameObject.SetActive(true);
			if (!iconSpineStaticInitialized)
			{
				ResetIconSpineToStatic();
				iconSpineStaticInitialized = true;
			}
		}
		if ((Object)(object)IconLock != (Object)null)
		{
			((Component)IconLock).gameObject.SetActive(false);
		}
		if ((Object)(object)LockDesc != (Object)null)
		{
			((Component)LockDesc).gameObject.SetActive(false);
		}
		if ((Object)(object)Bg != (Object)null)
		{
			((Component)Bg).gameObject.SetActive(true);
		}
		if ((Object)(object)Desc != (Object)null)
		{
			((Component)Desc).gameObject.SetActive(true);
		}
	}

	private void ResetIconSpineToStatic()
	{
		if (!((Object)(object)icon_spine == (Object)null))
		{
			icon_spine.Initialize(false);
			AnimationState animationState = icon_spine.AnimationState;
			if (animationState != null)
			{
				animationState.ClearTracks();
			}
			Skeleton skeleton = icon_spine.Skeleton;
			if (skeleton != null)
			{
				skeleton.SetToSetupPose();
			}
			icon_spine.freeze = true;
			icon_spine.Update(0f);
		}
	}

	private void FreezeIconSpineAtEnd()
	{
		if (!((Object)(object)icon_spine == (Object)null))
		{
			AnimationState animationState = icon_spine.AnimationState;
			TrackEntry val = ((animationState != null) ? animationState.GetCurrent(0) : null);
			if (val != null)
			{
				val.TimeScale = 0f;
			}
			icon_spine.freeze = true;
			icon_spine.Update(0f);
		}
	}

	private void OnIconSpinePlayFinished()
	{
		if (iconSpinePlayCompleteCallback != null)
		{
			KillIconSpineCompleteFallback();
			UnsubscribeIconSpineComplete();
			FreezeIconSpineAtEnd();
			InvokeIconSpinePlayCompleteCallback();
		}
	}

	private void ScheduleIconSpineCompleteFallback(float duration)
	{
		if (!((Object)(object)((UIBaseView)this).Behaviour == (Object)null) && !(duration <= 0f))
		{
			iconSpineCompleteTimerId = GameApp.Timer.Delay((Object)(object)((UIBaseView)this).Behaviour, duration, (Action)OnIconSpinePlayFinished);
		}
	}

	private void KillIconSpineCompleteFallback()
	{
		if (iconSpineCompleteTimerId != -1)
		{
			GameApp.Timer.Kill(iconSpineCompleteTimerId);
			iconSpineCompleteTimerId = -1;
		}
	}

	private void UnsubscribeIconSpineComplete()
	{
		if (iconSpinePlayEntry != null && iconSpineCompleteHandler != null)
		{
			iconSpinePlayEntry.Complete -= iconSpineCompleteHandler;
		}
		iconSpinePlayEntry = null;
		iconSpineCompleteHandler = null;
	}

	private void InvokeIconSpinePlayCompleteCallback()
	{
		Action action = iconSpinePlayCompleteCallback;
		iconSpinePlayCompleteCallback = null;
		action?.Invoke();
	}

	private void RefreshLockedState(int level)
	{
		SetButtonInteractable(interactable: true);
		if ((Object)(object)Desc != (Object)null)
		{
			TMPTextExtensions.SetLocalizedText(Desc, "Common_Icon_UnlockLv", new object[1] { level });
		}
	}

	private void RefreshUnlockedState()
	{
		SetButtonInteractable(interactable: true);
		SetCountdownText();
	}

	private void RefreshRankText()
	{
		if (!((Object)(object)txtRank == (Object)null))
		{
			int num = RankActivityService.CalcPlayerRanking(GameApp.Proxy.Get<RankModelProxy>()?.Data);
			bool flag = num > 0 && num <= RankConst.MaxRankBot;
			TMPTextExtensions.SetLocalizedTextValue(txtRank, flag ? $"#{num}" : "");
		}
	}

	public void SetRootVisible(bool visible)
	{
		if ((Object)(object)((UIBaseView)this).Behaviour != (Object)null)
		{
			((Component)((UIBaseView)this).Behaviour).gameObject.SetActive(visible);
		}
	}

	private void SetButtonInteractable(bool interactable)
	{
		if ((Object)(object)ButtonItem != (Object)null)
		{
			((Selectable)ButtonItem).interactable = interactable;
		}
	}

	private void StartCountdownIfNeeded()
	{
		if (CanOpenRank())
		{
			if (!IsActivityOngoing())
			{
				SetCountdownText();
				return;
			}
			SetCountdownText();
			countdownTimerId = GameApp.Timer.Repeat(1f, (Action)UpdateCountdownText);
		}
	}

	private void StopCountdown()
	{
		if (countdownTimerId >= 0)
		{
			GameApp.Timer.Kill(countdownTimerId);
			countdownTimerId = -1;
		}
	}

	private void SetCountdownText()
	{
		if (!((Object)(object)Desc == (Object)null))
		{
			TMPTextExtensions.SetLocalizedTextValue(Desc, RankActivityService.FormatCountdown(GetRemainSeconds()));
		}
	}

	private void UpdateCountdownText()
	{
		if (!((Object)(object)Desc == (Object)null) && countdownTimerId >= 0)
		{
			long remainSeconds = GetRemainSeconds();
			TMPTextExtensions.SetLocalizedTextValue(Desc, RankActivityService.FormatCountdown(remainSeconds));
		}
	}

	private string GetLevelText(int openLevel)
	{
		string text = GameApp.Lang.GetText("Booster_UnlockPrompt", new object[1] { openLevel });
		if (!string.IsNullOrEmpty(text))
		{
			return text;
		}
		return openLevel.ToString();
	}

	private bool CanOpenRank()
	{
		return RankActivityEntry.Service?.CanOpenRank() ?? false;
	}

	private static int GetRankOpenLevel()
	{
		return RankFunctionResolver.GetRankOpenLevel();
	}

	private bool IsActivityOngoing()
	{
		return RankActivityEntry.Service?.IsActivityOngoing(Time.GetCurrentTimestamp()) ?? false;
	}

	private long GetRemainSeconds()
	{
		return RankActivityEntry.Service?.GetRemainSeconds() ?? 0;
	}
}
