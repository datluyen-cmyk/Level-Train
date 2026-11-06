using System;
using System.Collections.Generic;
using Coffee.UIExtensions;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using Example.Cfg;
using Game.UI;
using RotateRings.Common;
using RotateRings.DailyFirstWin;
using RotateRings.Halloween;
using RotateRings.Model;
using RotateRings.Services;
using RotateRings.UI.Home;
using TMPro;
using UniLog;
using UnityEngine;
using UnityEngine.UI;
using Vibration;
using XingYun.Extensions;
using XingYun.UI.Core;

namespace RotateRings.UI.DailyFirstWin;

public class UIDailyFirstWinPuzzleView : GameBaseView
{
	private const string OpenAnimName = "anim_dailyfirstwinpuzzle_open";

	private const string LoopAnimName = "anim_dailyfirstwinpuzzle_loop";

	private const string CloseAnimName = "anim_dailyfirstwinpuzzle_close";

	private const string GotoBtnEffectAnimName = "anim_common_btn_effect";

	private const string GotoBtnParticleName = "eff_rank_btn_gp1";

	private const string HalloweenPieceCompleteAnimName = "anim_dailyfirstwinnewpiece_complete_hw";

	private const string DefaultBtnOKLocation = "btnOK";

	private const string HalloweenBtnOKLocation = "HalloweenBtnOK";

	private const float PerFrameSec = 0.0166f;

	private const float VictoryOpenAnimEmptyTailSec = 1.2f;

	private Animation uiAnimation;

	private CanvasGroup btnContentCanvasGroup;

	private int openCompleteTimerId = -1;

	private int settlementOpenCallbackTimerId = -1;

	private int closeTimerId = -1;

	private int pieceLandTimerId = -1;

	private int pieceLandStampTimerId = -1;

	private int pieceLandResultTimerId = -1;

	private int pieceLandRevealDay;

	private int pieceLandRevealPuzzleId;

	private string pieceLandFlySpriteName;

	private readonly DailyFirstWinViewUtil.DailyFirstWinPieceLandFlyRuntime pieceLandFlyRuntime = new DailyFirstWinViewUtil.DailyFirstWinPieceLandFlyRuntime();

	private bool pieceLandCompleteInvoked;

	private bool pieceLandSlotRevealed;

	private bool isHalloweenPuzzleTheme;

	private Material sweepRuntimeMaterial;

	private Material fameGlowRuntimeMaterial;

	private bool dayItemsBound;

	private WidgetHandle[] dayItemHandles;

	private UIDailyFirstWinDayItemView[] dayItemViews;

	private Image[] pieceImages;

	private WidgetHandle btnOKHandle;

	private int defaultBtnOKSiblingIndex = -1;

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

	public GameObject BtnContent;

	public UIBehaviour btnOK;

	public Image imagePuzzleBg;

	public Image imagePiece_1;

	public Image imagePiece_2;

	public Image imagePiece_3;

	public Image imagePiece_4;

	public Image imagePiece_5;

	public Image imagePiece_6;

	public Image imagePiece_7;

	public GameObject deco;

	public Image imagePieceFly;

	public RectTransform eff_DailyFirstWinNewPiecCollect_up;

	public RectTransform eff_DailyFirstWinNewPiecCollect_down;

	public UIParticle sweepRoot;

	public ParticleSystem sweep;

	public UIParticle fameGlowRoot;

	public ParticleSystem fameGlow;

	public Image firstwin_plz_cloud;

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
	}

	public void RefreshPuzzleVisuals(int puzzleId, int currentDay, bool playPieceUnlockAnim, bool isHalloweenPuzzleTheme = false, bool playPieceDoneSound = true, bool playAllDoneSound = true, bool playStampResultSounds = true)
	{
		this.isHalloweenPuzzleTheme = isHalloweenPuzzleTheme;
		DailyFirstWinPuzzleConfig dailyFirstWinPuzzleConfig = DailyFirstWinModule.Host?.GetPuzzleConfig(puzzleId);
		if (dailyFirstWinPuzzleConfig == null)
		{
			return;
		}
		NewPieceEffectTexturesUtil.TryApplyPieceSprite(imagePuzzleBg, dailyFirstWinPuzzleConfig.OutlineImage);
		RefreshPuzzleCloudSprite(puzzleId, isHalloweenPuzzleTheme);
		int num = Mathf.Clamp(currentDay, 0, 7);
		bool flag = num >= 7;
		if ((Object)(object)deco != (Object)null)
		{
			deco.SetActive(flag);
		}
		EnsurePieceImages();
		IList<string> pieces = dailyFirstWinPuzzleConfig.Pieces;
		for (int i = 0; i < pieceImages.Length; i++)
		{
			Image val = pieceImages[i];
			if (!((Object)(object)val == (Object)null))
			{
				bool flag2 = i < num;
				((Component)val).gameObject.SetActive(flag2);
				if (flag2 && pieces != null && i < pieces.Count)
				{
					NewPieceEffectTexturesUtil.TryApplyPieceSprite(val, pieces[i]);
				}
			}
		}
		if (!playPieceUnlockAnim || num <= 0)
		{
			return;
		}
		if (playPieceDoneSound)
		{
			DailyFirstWinViewUtil.PlayPuzzlePieceReveal();
		}
		if (flag)
		{
			if (playAllDoneSound)
			{
				DailyFirstWinViewUtil.PlayPuzzleAllDone();
			}
			if (playStampResultSounds)
			{
				DailyFirstWinViewUtil.PlayPuzzleStamp();
				DailyFirstWinViewUtil.PlayPuzzleResult();
			}
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
		//IL_00c9: Unknown result type (might be due to invalid IL or missing references)
		ShutdownBtnOK();
		if ((Object)(object)btnOK == (Object)null)
		{
			return;
		}
		Transform transform = ((Component)btnOK).transform;
		Transform parent = transform.parent;
		if ((Object)(object)parent == (Object)null)
		{
			return;
		}
		defaultBtnOKSiblingIndex = transform.GetSiblingIndex();
		((Component)transform).gameObject.SetActive(false);
		string text = (HalloweenFeatureService.IsFirstWinEnabled() ? "HalloweenBtnOK" : "btnOK");
		btnOKHandle = AttachWidget<UIHomeBtnOKView>(text, parent);
		if (btnOKHandle == null)
		{
			Log.LogError("UI", "[UIDailyFirstWinPuzzleView] AttachWidget UIHomeBtnOKView failed: {0}", new object[1] { text });
			RestoreDefaultBtnOK();
			return;
		}
		Transform transform2 = btnOKHandle.GameObject.transform;
		RectTransform val = (RectTransform)(object)((transform2 is RectTransform) ? transform2 : null);
		RectTransform val2 = (RectTransform)(object)((transform is RectTransform) ? transform : null);
		if ((Object)(object)val != (Object)null && (Object)(object)val2 != (Object)null)
		{
			val.anchoredPosition = val2.anchoredPosition;
		}
		btnOKHandle.GameObject.transform.SetSiblingIndex(defaultBtnOKSiblingIndex);
		BtnOKView?.Initialize(playIdleAnimation);
	}

	public void ShutdownBtnOK()
	{
		if (btnOKHandle != null)
		{
			DetachWidget(btnOKHandle);
			btnOKHandle = null;
		}
		RestoreDefaultBtnOK();
	}

	private void RestoreDefaultBtnOK()
	{
		if ((Object)(object)btnOK == (Object)null)
		{
			defaultBtnOKSiblingIndex = -1;
			return;
		}
		((Component)btnOK).gameObject.SetActive(true);
		Transform parent = ((Component)btnOK).transform.parent;
		if (defaultBtnOKSiblingIndex >= 0 && (Object)(object)parent != (Object)null)
		{
			((Component)btnOK).transform.SetSiblingIndex(Mathf.Min(defaultBtnOKSiblingIndex, parent.childCount - 1));
		}
		defaultBtnOKSiblingIndex = -1;
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
			SetRewardPreviewInteractable(interactable: false);
			SetGotoInteractable(interactable: false);
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
			DailyFirstWinViewUtil.PlayLoopClip(GetUIAnimation(), loopClip);
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

	public float GetOpenLoopSwitchDelay(bool isSettlement, bool hasFirstWinToday)
	{
		ResolveOpenClips(isSettlement, hasFirstWinToday, out var openClip, out var _);
		Animation uIAnimation = GetUIAnimation();
		AnimationClip val = ((uIAnimation != null) ? uIAnimation.GetClip(openClip) : null);
		if (!((Object)(object)val != (Object)null))
		{
			return 3f;
		}
		return val.length + 0.0166f;
	}

	public void PrepareOpenFirstFrame(bool isSettlement, bool hasFirstWinToday)
	{
		ResolveOpenClips(isSettlement, hasFirstWinToday, out var openClip, out var _);
		DailyFirstWinViewUtil.SampleAnimationFrame(GetUIAnimation(), openClip, 0f);
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

	public void PrepareNewPieceLandPresentation(int pieceDay, string spriteName, int puzzleId)
	{
		pieceLandFlySpriteName = spriteName;
		pieceLandRevealDay = Mathf.Clamp(pieceDay, 1, 7);
		pieceLandRevealPuzzleId = puzzleId;
		pieceLandSlotRevealed = false;
		DailyFirstWinViewUtil.PreparePieceFlyTemplate(imagePieceFly, spriteName);
		ApplyPieceDayLandEffectTextures(pieceDay);
		AlignPieceCollectEffects(pieceDay);
	}

	public void PlayNewPieceLandPresentation(bool isLastPiece, Action onComplete = null)
	{
		KillOpenAndSettlementTimers();
		KillCloseTimer();
		KillPieceLandTimer();
		pieceLandCompleteInvoked = false;
		pieceLandSlotRevealed = false;
		string clipName = ((!isLastPiece) ? "anim_dailyfirstwinnewpiece_drop" : (isHalloweenPuzzleTheme ? "anim_dailyfirstwinnewpiece_complete_hw" : "anim_dailyfirstwinnewpiece_complete"));
		Animation uIAnimation = GetUIAnimation();
		try
		{
			float num = DailyFirstWinViewUtil.PlayPieceLandLegacyClip(uIAnimation, clipName);
			if (num > 0f)
			{
				if (isLastPiece)
				{
					DailyFirstWinViewUtil.PlayPuzzleAllDone();
					DailyFirstWinViewUtil.SchedulePuzzleCompleteCelebrationSounds((Component)(object)((UIBaseView)this).NodeRect, ref pieceLandStampTimerId, ref pieceLandResultTimerId);
				}
				pieceLandTimerId = GameApp.Timer.Delay((Object)(object)((UIBaseView)this).NodeRect, num, (Action)delegate
				{
					pieceLandTimerId = -1;
					InvokePieceLandCompleteOnce(onComplete);
				});
			}
			else if (isLastPiece)
			{
				DailyFirstWinViewUtil.PlayPuzzleAllDone();
				DailyFirstWinViewUtil.SchedulePuzzleCompleteCelebrationSounds((Component)(object)((UIBaseView)this).NodeRect, ref pieceLandStampTimerId, ref pieceLandResultTimerId);
			}
			AlignPieceCollectEffects(pieceLandRevealDay);
			Image pieceImage = GetPieceImage(pieceLandRevealDay);
			DailyFirstWinViewUtil.DailyFirstWinPieceLandFlyPlayArgs args = new DailyFirstWinViewUtil.DailyFirstWinPieceLandFlyPlayArgs(imagePieceFly, pieceLandFlySpriteName, ((Object)(object)pieceImage != (Object)null) ? ((Graphic)pieceImage).rectTransform : null, base.LoadAssetGroup, RevealLandPieceSlot, delegate
			{
				DailyFirstWinViewUtil.RecyclePieceLandFly(pieceLandFlyRuntime);
			});
			if (!DailyFirstWinViewUtil.TryPlayPieceLandFly(pieceLandFlyRuntime, in args))
			{
				RevealLandPieceSlot();
			}
			if (num <= 0f)
			{
				InvokePieceLandCompleteOnce(onComplete);
			}
		}
		catch (Exception)
		{
			if (isLastPiece)
			{
				DailyFirstWinViewUtil.PlayPuzzleAllDone();
				DailyFirstWinViewUtil.SchedulePuzzleCompleteCelebrationSounds((Component)(object)((UIBaseView)this).NodeRect, ref pieceLandStampTimerId, ref pieceLandResultTimerId);
			}
			RevealLandPieceSlot();
			InvokePieceLandCompleteOnce(onComplete);
		}
	}

	private void InvokePieceLandCompleteOnce(Action onComplete)
	{
		if (!pieceLandCompleteInvoked)
		{
			pieceLandCompleteInvoked = true;
			KillPieceLandStampResultTimers();
			KillPieceLandCompleteTimer();
			DailyFirstWinViewUtil.RecyclePieceLandFly(pieceLandFlyRuntime);
			onComplete?.Invoke();
		}
	}

	public void PlayClose(Action onComplete)
	{
		KillOpenAndSettlementTimers();
		KillCloseTimer();
		KillPieceLandTimer();
		Animation uIAnimation = GetUIAnimation();
		AnimationClip val = (((Object)(object)uIAnimation != (Object)null) ? uIAnimation.GetClip("anim_dailyfirstwinpuzzle_close") : null);
		if ((Object)(object)uIAnimation == (Object)null || (Object)(object)val == (Object)null)
		{
			onComplete?.Invoke();
			return;
		}
		uIAnimation.Stop();
		uIAnimation.Rewind("anim_dailyfirstwinpuzzle_close");
		uIAnimation.Play("anim_dailyfirstwinpuzzle_close", (PlayMode)4);
		closeTimerId = GameApp.Timer.Delay((Object)(object)((UIBaseView)this).NodeRect, val.length, (Action)delegate
		{
			closeTimerId = -1;
			onComplete?.Invoke();
		});
	}

	public override void Dispose()
	{
		KillOpenAndSettlementTimers();
		KillCloseTimer();
		KillPieceLandTimer();
		ReleaseLandEffectRuntimeMaterials();
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

	private void KillOpenAndSettlementTimers()
	{
		if (openCompleteTimerId >= 0)
		{
			GameApp.Timer.Kill(openCompleteTimerId);
			openCompleteTimerId = -1;
		}
		if (settlementOpenCallbackTimerId >= 0)
		{
			GameApp.Timer.Kill(settlementOpenCallbackTimerId);
			settlementOpenCallbackTimerId = -1;
		}
	}

	private void KillCloseTimer()
	{
		if (closeTimerId >= 0)
		{
			GameApp.Timer.Kill(closeTimerId);
			closeTimerId = -1;
		}
	}

	private void KillPieceLandCompleteTimer()
	{
		if (pieceLandTimerId >= 0)
		{
			GameApp.Timer.Kill(pieceLandTimerId);
			pieceLandTimerId = -1;
		}
	}

	private void KillPieceLandStampResultTimers()
	{
		DailyFirstWinViewUtil.KillPuzzleCompleteCelebrationTimers(ref pieceLandStampTimerId, ref pieceLandResultTimerId);
	}

	private void KillPieceLandTimer()
	{
		KillPieceLandStampResultTimers();
		KillPieceLandCompleteTimer();
		DailyFirstWinViewUtil.RecyclePieceLandFly(pieceLandFlyRuntime);
	}

	private void RevealLandPieceSlot()
	{
		if (pieceLandSlotRevealed)
		{
			return;
		}
		pieceLandSlotRevealed = true;
		int num = pieceLandRevealDay;
		if (num < 1 || num > 7)
		{
			return;
		}
		DailyFirstWinPuzzleConfig dailyFirstWinPuzzleConfig = DailyFirstWinModule.Host?.GetPuzzleConfig(pieceLandRevealPuzzleId);
		Image pieceImage = GetPieceImage(num);
		if (!((Object)(object)pieceImage == (Object)null))
		{
			IList<string> list = dailyFirstWinPuzzleConfig?.Pieces;
			if (list != null && num - 1 < list.Count)
			{
				NewPieceEffectTexturesUtil.TryApplyPieceSprite(pieceImage, list[num - 1]);
			}
			((Component)pieceImage).gameObject.SetActive(true);
			DailyFirstWinViewUtil.PlayPuzzlePieceReveal();
			if (num >= 7 && (Object)(object)deco != (Object)null)
			{
				deco.SetActive(true);
			}
		}
	}

	private Image GetPieceImage(int day)
	{
		EnsurePieceImages();
		if (day < 1 || day > pieceImages.Length)
		{
			return null;
		}
		return pieceImages[day - 1];
	}

	private void RefreshPuzzleCloudSprite(int puzzleId, bool isHalloweenPuzzleTheme)
	{
		if (!((Object)(object)firstwin_plz_cloud == (Object)null) && puzzleId > 0 && !isHalloweenPuzzleTheme)
		{
			string spriteName = $"firstwin_plz_cloud_{puzzleId:D2}";
			NewPieceEffectTexturesUtil.TryApplyPieceSprite(firstwin_plz_cloud, spriteName);
		}
	}

	private void EnsurePieceImages()
	{
		if (pieceImages == null)
		{
			pieceImages = (Image[])(object)new Image[7] { imagePiece_1, imagePiece_2, imagePiece_3, imagePiece_4, imagePiece_5, imagePiece_6, imagePiece_7 };
		}
	}

	private void ApplyPieceDayLandEffectTextures(int pieceDay)
	{
		NewPieceEffectTexturesUtil.ApplyLandParticleTextures(pieceDay, "UIDailyFirstWinPuzzleView", sweep, sweepRoot, ref sweepRuntimeMaterial, fameGlow, fameGlowRoot, ref fameGlowRuntimeMaterial, RefreshEffectUIParticleRenderers);
	}

	private void RefreshEffectUIParticleRenderers()
	{
		UIParticle obj = sweepRoot;
		if (obj != null)
		{
			obj.RefreshParticles();
		}
		UIParticle obj2 = fameGlowRoot;
		if (obj2 != null)
		{
			obj2.RefreshParticles();
		}
	}

	private void ReleaseLandEffectRuntimeMaterials()
	{
		NewPieceEffectTexturesUtil.ReleaseRuntimeMaterial(ref sweepRuntimeMaterial, delegate
		{
			NewPieceEffectTexturesUtil.ClearParticleMaterial(sweep);
		});
		NewPieceEffectTexturesUtil.ReleaseRuntimeMaterial(ref fameGlowRuntimeMaterial, delegate
		{
			NewPieceEffectTexturesUtil.ClearParticleMaterial(fameGlow);
		});
	}

	private void AlignPieceCollectEffects(int pieceDay)
	{
		int day = Mathf.Clamp(pieceDay, 1, 7);
		Image pieceImage = GetPieceImage(day);
		RectTransform val = ((pieceImage != null) ? ((Graphic)pieceImage).rectTransform : null);
		if (!((Object)(object)val == (Object)null))
		{
			DailyFirstWinViewUtil.AlignRectTransformToPiece(eff_DailyFirstWinNewPiecCollect_up, val);
			DailyFirstWinViewUtil.AlignRectTransformToPiece(eff_DailyFirstWinNewPiecCollect_down, val);
		}
	}

	private static void ResolveOpenClips(bool isSettlement, bool hasFirstWinToday, out string openClip, out string loopClip)
	{
		openClip = "anim_dailyfirstwinpuzzle_open";
		loopClip = "anim_dailyfirstwinpuzzle_loop";
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
		BtnContent = behaviour.Get<GameObject>("BtnContent");
		btnOK = behaviour.Get<UIBehaviour>("btnOK");
		imagePuzzleBg = behaviour.Get<Image>("imagePuzzleBg");
		imagePiece_1 = behaviour.Get<Image>("imagePiece_1");
		imagePiece_2 = behaviour.Get<Image>("imagePiece_2");
		imagePiece_3 = behaviour.Get<Image>("imagePiece_3");
		imagePiece_4 = behaviour.Get<Image>("imagePiece_4");
		imagePiece_5 = behaviour.Get<Image>("imagePiece_5");
		imagePiece_6 = behaviour.Get<Image>("imagePiece_6");
		imagePiece_7 = behaviour.Get<Image>("imagePiece_7");
		deco = behaviour.Get<GameObject>("deco");
		imagePieceFly = behaviour.Get<Image>("imagePieceFly");
		eff_DailyFirstWinNewPiecCollect_up = behaviour.Get<RectTransform>("eff_DailyFirstWinNewPiecCollect_up");
		eff_DailyFirstWinNewPiecCollect_down = behaviour.Get<RectTransform>("eff_DailyFirstWinNewPiecCollect_down");
		sweepRoot = behaviour.Get<UIParticle>("sweepRoot");
		sweep = behaviour.Get<ParticleSystem>("sweep");
		fameGlowRoot = behaviour.Get<UIParticle>("fameGlowRoot");
		fameGlow = behaviour.Get<ParticleSystem>("fameGlow");
		firstwin_plz_cloud = behaviour.Get<Image>("firstwin_plz_cloud");
	}
}
