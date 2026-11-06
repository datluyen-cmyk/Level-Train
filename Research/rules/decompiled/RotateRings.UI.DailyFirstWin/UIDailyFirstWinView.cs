using System;
using System.Collections.Generic;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using Game.UI;
using RotateRings.Common;
using RotateRings.Model;
using RotateRings.Services;
using RotateRings.UI.Home;
using Script.Tools;
using Spine.Unity;
using TMPro;
using UniLog;
using UnityEngine;
using UnityEngine.UI;
using Vibration;
using XingYun.Extensions;
using XingYun.UI.Core;

namespace RotateRings.UI.DailyFirstWin;

public class UIDailyFirstWinView : GameBaseView
{
	private const string OpenAnimName = "anim_dailyfirstwin_open";

	private const string LoopAnimName = "anim_dailyfirstwin_loop";

	private const string VictoryOpenAnimName = "anim_dailyfirstwinvicotry_open";

	private const string VictoryLoopAnimName = "anim_dailyfirstwinvicotry_loop";

	private const string CloseAnimName = "anim_dailyfirstwin_close";

	private const string GotoBtnEffectAnimName = "anim_common_btn_effect";

	private const string GotoBtnParticleName = "eff_rank_btn_gp1";

	private const float PerFrameSec = 0.0166f;

	private const float VictoryOpenAnimEmptyTailSec = 1.2f;

	private Animation uiAnimation;

	private CanvasGroup btnContentCanvasGroup;

	private int openCompleteTimerId = -1;

	private int settlementOpenCallbackTimerId = -1;

	private int closeTimerId = -1;

	private bool dayItemsBound;

	private WidgetHandle[] dayItemHandles;

	private UIDailyFirstWinDayItemView[] dayItemViews;

	private WidgetHandle btnOKHandle;

	public Image imgDay;

	public GameObject firstwin_img_light2;

	public TMP_Text TxtTitle;

	public TMP_Text TxtDetail;

	public Button BtnClose;

	public UIBehaviour DayItem_1;

	public UIBehaviour DayItem_2;

	public UIBehaviour DayItem_3;

	public UIBehaviour DayItem_4;

	public UIBehaviour DayItem_5;

	public UIBehaviour DayItem_6;

	public UIBehaviour DayItem_7;

	public SkeletonGraphic spine_open;

	public SkeletonGraphic spine_idle;

	public BoneFollowerGraphic imgDayFollower;

	public BoneFollowerGraphic eff_sweep_medal_loop;

	public BoneFollowerGraphic eff_sweep_crown_loop;

	public GameObject BtnContent;

	public UIBehaviour btnOK;

	public UIHomeBtnOKView BtnOKView => btnOKHandle?.GetView<UIHomeBtnOKView>();

	public void SetTitle(string text)
	{
		if ((Object)(object)TxtTitle != (Object)null)
		{
			TMPTextExtensions.SetLocalizedTextValue(TxtTitle, text ?? string.Empty);
		}
	}

	public void SetTitleVisible(bool visible)
	{
		if ((Object)(object)TxtTitle != (Object)null)
		{
			((Component)TxtTitle).gameObject.SetActive(visible);
		}
		if ((Object)(object)firstwin_img_light2 != (Object)null)
		{
			firstwin_img_light2.SetActive(visible);
		}
	}

	public void SetDetail(string text)
	{
		if ((Object)(object)TxtDetail != (Object)null)
		{
			TMPTextExtensions.SetLocalizedTextValue(TxtDetail, text ?? string.Empty);
		}
	}

	public void InitBtnOK(bool playIdleAnimation)
	{
		ShutdownBtnOK();
		if (!((Object)(object)btnOK == (Object)null))
		{
			btnOKHandle = AttachWidget<UIHomeBtnOKView>(btnOK);
			if (btnOKHandle == null)
			{
				Log.LogError("UI", "[UIDailyFirstWinView] AttachWidget UIHomeBtnOKView failed");
			}
			else
			{
				BtnOKView?.Initialize(playIdleAnimation);
			}
		}
	}

	public void ShutdownBtnOK()
	{
		if (btnOKHandle != null)
		{
			DetachWidget(btnOKHandle);
			btnOKHandle = null;
		}
	}

	public void RefreshPlayButton(ELevelDifficulty difficulty, bool hasUnfinishedLevel, GameMode gameMode, int level)
	{
		BtnOKView?.Refresh(difficulty, hasUnfinishedLevel, gameMode, level);
	}

	public void RefreshSettlementPlayButton()
	{
		BtnOKView?.RefreshSettlementContinue();
	}

	public void SetGotoInteractable(bool interactable)
	{
		BtnOKView?.SetInteractable(interactable);
	}

	public void SetGotoVisible(bool visible)
	{
		if ((Object)(object)BtnContent != (Object)null)
		{
			BtnContent.SetActive(visible);
		}
	}

	public void PlayGotoFadeIn(float duration = 0.2f, Action onComplete = null)
	{
		//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c5: Expected O, but got Unknown
		if ((Object)(object)BtnContent == (Object)null)
		{
			onComplete?.Invoke();
			return;
		}
		BtnContent.SetActive(true);
		RefreshSettlementPlayButton();
		if ((Object)(object)btnContentCanvasGroup == (Object)null)
		{
			btnContentCanvasGroup = BtnContent.GetComponent<CanvasGroup>();
			if ((Object)(object)btnContentCanvasGroup == (Object)null)
			{
				btnContentCanvasGroup = BtnContent.AddComponent<CanvasGroup>();
			}
		}
		btnContentCanvasGroup.alpha = 0f;
		TweenSettingsExtensions.OnComplete<TweenerCore<float, float, FloatOptions>>(TweenSettingsExtensions.SetLink<TweenerCore<float, float, FloatOptions>>(TweenSettingsExtensions.SetTarget<TweenerCore<float, float, FloatOptions>>(DOTweenModuleUI.DOFade(btnContentCanvasGroup, 1f, duration), (object)BtnContent), BtnContent, (LinkBehaviour)5), (TweenCallback)delegate
		{
			onComplete?.Invoke();
		});
	}

	public void EnsureGotoVisibleForFly()
	{
		if (!((Object)(object)BtnContent == (Object)null))
		{
			BtnContent.SetActive(true);
			RefreshSettlementPlayButton();
		}
	}

	public void PlayGotoBtnEffect()
	{
		if (!((Object)(object)BtnContent == (Object)null))
		{
			BtnContent.SetActive(true);
			DailyFirstWinViewUtil.PlayClipWithRewind((Component)(object)BtnContent.transform, "anim_common_btn_effect");
			PlayGotoBtnParticleEffect();
			VibrateUtility.VibrateExtra(1505);
		}
	}

	public void SetDaySprite(int day)
	{
		if (!((Object)(object)imgDay == (Object)null))
		{
			int num = Mathf.Clamp(day, 0, 7);
			Sprite val = SpriteManager.LoadSprite($"firstwin_txt_{num}");
			if ((Object)(object)val != (Object)null)
			{
				imgDay.sprite = val;
			}
		}
	}

	public RectTransform GetDayItemRectTransform(int day)
	{
		if (day < 1 || day > 7)
		{
			return null;
		}
		UIBehaviour val = (UIBehaviour)(day switch
		{
			1 => DayItem_1, 
			2 => DayItem_2, 
			3 => DayItem_3, 
			4 => DayItem_4, 
			5 => DayItem_5, 
			6 => DayItem_6, 
			7 => DayItem_7, 
			_ => null, 
		});
		if (!((Object)(object)val != (Object)null))
		{
			return null;
		}
		Transform transform = ((Component)val).transform;
		return (RectTransform)(object)((transform is RectTransform) ? transform : null);
	}

	public void RefreshProgress(int currentDay, int periodDays, bool hasFirstWinToday, IReadOnlyList<int> milestoneDays, int pendingRewardDay, Action<int, RectTransform> onBoxClicked)
	{
		EnsureDayItemsBound();
		int num = Mathf.Clamp((periodDays <= 0) ? 7 : periodDays, 1, 7);
		int currentDay2 = Mathf.Clamp(currentDay, 0, num);
		HashSet<int> hashSet = DailyFirstWinViewUtil.BuildMilestoneSet(milestoneDays, num);
		int targetDay = DailyFirstWinViewUtil.ResolveTargetDay(currentDay2, num, hasFirstWinToday);
		for (int i = 0; i < dayItemViews.Length; i++)
		{
			int num2 = i + 1;
			UIDailyFirstWinDayItemView uIDailyFirstWinDayItemView = dayItemViews[i];
			if (!((Object)(object)((uIDailyFirstWinDayItemView != null) ? ((UIBaseView)uIDailyFirstWinDayItemView).Behaviour : null) == (Object)null))
			{
				bool flag = num2 <= num;
				((Component)((UIBaseView)uIDailyFirstWinDayItemView).Behaviour).gameObject.SetActive(flag);
				if (flag)
				{
					bool isMilestone = hashSet.Contains(num2);
					uIDailyFirstWinDayItemView.Apply(num2, num, currentDay2, isMilestone, pendingRewardDay, targetDay, onBoxClicked);
				}
			}
		}
	}

	public void SetRewardPreviewInteractable(bool interactable)
	{
		EnsureDayItemsBound();
		for (int i = 0; i < dayItemViews.Length; i++)
		{
			dayItemViews[i]?.SetRewardPreviewInteractable(interactable);
		}
	}

	public void PlayOpen(bool isSettlement, bool hasFirstWinToday, Action onComplete = null, Action onOpenClipFinished = null, bool playSound = true)
	{
		GameApp.Timer.Kill(openCompleteTimerId);
		openCompleteTimerId = -1;
		GameApp.Timer.Kill(settlementOpenCallbackTimerId);
		settlementOpenCallbackTimerId = -1;
		if (isSettlement)
		{
			AudioService.PlayUiSound(AudioKeys.Complete.Value);
			VibrateUtility.Vibrate(1503);
		}
		ResolveOpenClips(isSettlement, hasFirstWinToday, out var openClip, out var loopClip);
		Animation uIAnimation = GetUIAnimation();
		AnimationClip val = (((Object)(object)uIAnimation != (Object)null) ? uIAnimation.GetClip(openClip) : null);
		if ((Object)(object)uIAnimation == (Object)null || (Object)(object)val == (Object)null)
		{
			onComplete?.Invoke();
			onOpenClipFinished?.Invoke();
			return;
		}
		uIAnimation.Rewind(openClip);
		uIAnimation.Play(openClip, (PlayMode)4);
		float num = val.length + 0.0166f;
		openCompleteTimerId = GameApp.Timer.Delay((Object)(object)((UIBaseView)this).NodeRect, num, (Action)delegate
		{
			openCompleteTimerId = -1;
			PlayLoopClip(loopClip);
			if (!isSettlement)
			{
				onComplete?.Invoke();
			}
			onOpenClipFinished?.Invoke();
		});
		if (isSettlement && onComplete != null)
		{
			float num2 = Mathf.Max(num - 1.2f, 0f);
			settlementOpenCallbackTimerId = GameApp.Timer.Delay((Object)(object)((UIBaseView)this).NodeRect, num2, (Action)delegate
			{
				settlementOpenCallbackTimerId = -1;
				onComplete();
			});
		}
	}

	public void PrepareOpenFirstFrame(bool isSettlement, bool hasFirstWinToday)
	{
		ResolveOpenClips(isSettlement, hasFirstWinToday, out var openClip, out var _);
		DailyFirstWinViewUtil.SampleAnimationFrame(GetUIAnimation(), openClip, 0f);
		SkeletonGraphic boneFollowersSkeleton = ((isSettlement || hasFirstWinToday) ? spine_open : spine_idle);
		SetBoneFollowersSkeleton(boneFollowersSkeleton);
	}

	public void PrepareProgressCompletePresentation(int day, IReadOnlyList<int> milestoneDays)
	{
		UIDailyFirstWinDayItemView dayItemView = GetDayItemView(day);
		if (dayItemView != null)
		{
			int periodDays = dayItemViews.Length;
			bool isMilestone = DailyFirstWinViewUtil.BuildMilestoneSet(milestoneDays, periodDays).Contains(day);
			dayItemView.SetCompleteVisual(isMilestone, completed: false);
		}
	}

	public void PlayProgressCompletePresentation(int day, IReadOnlyList<int> milestoneDays, Action onComplete = null)
	{
		UIDailyFirstWinDayItemView dayItemView = GetDayItemView(day);
		if (dayItemView == null)
		{
			onComplete?.Invoke();
			return;
		}
		int periodDays = dayItemViews.Length;
		bool isMilestone = DailyFirstWinViewUtil.BuildMilestoneSet(milestoneDays, periodDays).Contains(day);
		dayItemView.PlayCompletePresentation(isMilestone, onComplete);
	}

	public void RefreshProgressState(int currentDay, int periodDays, bool hasFirstWinToday, IReadOnlyList<int> milestoneDays, int pendingRewardDay)
	{
		EnsureDayItemsBound();
		int num = Mathf.Clamp((periodDays <= 0) ? 7 : periodDays, 1, 7);
		int currentDay2 = Mathf.Clamp(currentDay, 0, num);
		HashSet<int> hashSet = DailyFirstWinViewUtil.BuildMilestoneSet(milestoneDays, num);
		int targetDay = DailyFirstWinViewUtil.ResolveTargetDay(currentDay2, num, hasFirstWinToday);
		for (int i = 0; i < dayItemViews.Length; i++)
		{
			int num2 = i + 1;
			UIDailyFirstWinDayItemView uIDailyFirstWinDayItemView = dayItemViews[i];
			if (!((Object)(object)((uIDailyFirstWinDayItemView != null) ? ((UIBaseView)uIDailyFirstWinDayItemView).Behaviour : null) == (Object)null) && ((Component)((UIBaseView)uIDailyFirstWinDayItemView).Behaviour).gameObject.activeSelf)
			{
				bool isMilestone = hashSet.Contains(num2);
				uIDailyFirstWinDayItemView.RefreshState(num2, num, currentDay2, isMilestone, pendingRewardDay, targetDay);
			}
		}
	}

	public void PlayClose(Action onComplete)
	{
		GameApp.Timer.Kill(openCompleteTimerId);
		openCompleteTimerId = -1;
		GameApp.Timer.Kill(closeTimerId);
		closeTimerId = -1;
		Animation uIAnimation = GetUIAnimation();
		AnimationClip val = (((Object)(object)uIAnimation != (Object)null) ? uIAnimation.GetClip("anim_dailyfirstwin_close") : null);
		if ((Object)(object)uIAnimation == (Object)null || (Object)(object)val == (Object)null)
		{
			onComplete?.Invoke();
			return;
		}
		uIAnimation.Stop();
		uIAnimation.Rewind("anim_dailyfirstwin_close");
		uIAnimation.Play("anim_dailyfirstwin_close", (PlayMode)4);
		closeTimerId = GameApp.Timer.Delay((Object)(object)((UIBaseView)this).NodeRect, val.length, (Action)delegate
		{
			closeTimerId = -1;
			onComplete?.Invoke();
		});
	}

	public override void Dispose()
	{
		GameApp.Timer.Kill(openCompleteTimerId);
		openCompleteTimerId = -1;
		GameApp.Timer.Kill(settlementOpenCallbackTimerId);
		settlementOpenCallbackTimerId = -1;
		GameApp.Timer.Kill(closeTimerId);
		closeTimerId = -1;
		ShutdownBtnOK();
		DetachAllWidgets();
		dayItemsBound = false;
		dayItemHandles = null;
		dayItemViews = null;
		((UIBaseView)this).Dispose();
	}

	private void EnsureDayItemsBound()
	{
		if (dayItemsBound)
		{
			return;
		}
		dayItemsBound = true;
		dayItemHandles = new WidgetHandle[7];
		dayItemViews = new UIDailyFirstWinDayItemView[7];
		UIBehaviour[] array = (UIBehaviour[])(object)new UIBehaviour[7] { DayItem_1, DayItem_2, DayItem_3, DayItem_4, DayItem_5, DayItem_6, DayItem_7 };
		for (int i = 0; i < array.Length; i++)
		{
			if (!((Object)(object)array[i] == (Object)null))
			{
				dayItemHandles[i] = AttachWidget<UIDailyFirstWinDayItemView>(array[i]);
				dayItemViews[i] = dayItemHandles[i]?.GetView<UIDailyFirstWinDayItemView>();
			}
		}
	}

	private UIDailyFirstWinDayItemView GetDayItemView(int day)
	{
		if (day < 1 || day > dayItemViews?.Length)
		{
			return null;
		}
		return dayItemViews[day - 1];
	}

	private Animation GetUIAnimation()
	{
		if ((Object)(object)uiAnimation == (Object)null && (Object)(object)((UIBaseView)this).Behaviour != (Object)null)
		{
			uiAnimation = ((Component)((UIBaseView)this).Behaviour).GetComponent<Animation>();
		}
		return uiAnimation;
	}

	private static void ResolveOpenClips(bool isSettlement, bool hasFirstWinToday, out string openClip, out string loopClip)
	{
		if (isSettlement || hasFirstWinToday)
		{
			openClip = "anim_dailyfirstwinvicotry_open";
			loopClip = "anim_dailyfirstwinvicotry_loop";
		}
		else
		{
			openClip = "anim_dailyfirstwin_open";
			loopClip = "anim_dailyfirstwin_loop";
		}
	}

	private void PlayLoopClip(string clipName)
	{
		SetBoneFollowersSkeleton(spine_idle);
		DailyFirstWinViewUtil.PlayLoopClip(GetUIAnimation(), clipName);
	}

	private void SetBoneFollowersSkeleton(SkeletonGraphic skeletonGraphic)
	{
		SetBoneFollowerSkeleton(imgDayFollower, skeletonGraphic);
		SetBoneFollowerSkeleton(eff_sweep_medal_loop, skeletonGraphic);
		SetBoneFollowerSkeleton(eff_sweep_crown_loop, skeletonGraphic);
	}

	private static void SetBoneFollowerSkeleton(BoneFollowerGraphic follower, SkeletonGraphic skeletonGraphic)
	{
		if (!((Object)(object)follower == (Object)null) && !((Object)(object)skeletonGraphic == (Object)null))
		{
			follower.SkeletonGraphic = skeletonGraphic;
		}
	}

	private void PlayGotoBtnParticleEffect()
	{
		if (!((Object)(object)BtnContent == (Object)null))
		{
			Transform val = BtnContent.transform.Find("eff_rank_btn_gp1");
			if (!((Object)(object)val == (Object)null))
			{
				((Component)val).gameObject.SetActive(true);
				DailyFirstWinViewUtil.PlayParticleSystemsUnder(((Component)val).gameObject);
			}
		}
	}

	public override void Inject(UIBehaviour behaviour)
	{
		((UIBaseView)this).Inject(behaviour);
		imgDay = behaviour.Get<Image>("imgDay");
		firstwin_img_light2 = behaviour.Get<GameObject>("firstwin_img_light2");
		TxtTitle = behaviour.Get<TMP_Text>("TxtTitle");
		TxtDetail = behaviour.Get<TMP_Text>("TxtDetail");
		BtnClose = behaviour.Get<Button>("BtnClose");
		DayItem_1 = behaviour.Get<UIBehaviour>("DayItem_1");
		DayItem_2 = behaviour.Get<UIBehaviour>("DayItem_2");
		DayItem_3 = behaviour.Get<UIBehaviour>("DayItem_3");
		DayItem_4 = behaviour.Get<UIBehaviour>("DayItem_4");
		DayItem_5 = behaviour.Get<UIBehaviour>("DayItem_5");
		DayItem_6 = behaviour.Get<UIBehaviour>("DayItem_6");
		DayItem_7 = behaviour.Get<UIBehaviour>("DayItem_7");
		spine_open = behaviour.Get<SkeletonGraphic>("spine_open");
		spine_idle = behaviour.Get<SkeletonGraphic>("spine_idle");
		imgDayFollower = behaviour.Get<BoneFollowerGraphic>("imgDayFollower");
		eff_sweep_medal_loop = behaviour.Get<BoneFollowerGraphic>("eff_sweep_medal_loop");
		eff_sweep_crown_loop = behaviour.Get<BoneFollowerGraphic>("eff_sweep_crown_loop");
		BtnContent = behaviour.Get<GameObject>("BtnContent");
		btnOK = behaviour.Get<UIBehaviour>("btnOK");
	}
}
