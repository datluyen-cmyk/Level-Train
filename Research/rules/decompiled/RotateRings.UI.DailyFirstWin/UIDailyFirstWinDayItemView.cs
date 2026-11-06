using System;
using Basis.Common.UI;
using Game.UI;
using RotateRings.Common;
using RotateRings.DailyFirstWin;
using RotateRings.Services;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Vibration;
using XingYun.Extensions;
using XingYun.UI.Core;
using XingYun.Utility;

namespace RotateRings.UI.DailyFirstWin;

public class UIDailyFirstWinDayItemView : GameWidgetView
{
	private const float CompleteEffectDuration = 0.5f;

	private const string CompleteAnimName = "anim_dayitem_complete";

	private const string ChestClickLoopAnimName = "anim_imaBoxClose_chest_click";

	private const string WeekdayLangKeyPrefix = "FirstWin_Weekday_";

	private const int FinalMilestoneDay = 7;

	public Button buttonBox;

	public TMP_Text textDay;

	public TMP_Text textToday;

	public Image imgComplete;

	public Image imaBoxClose;

	public Image imgBoxOpen;

	public Image imgBg;

	public UIEffectLoader eff_complete;

	private Animation anim;

	private int completeTimerId = -1;

	private int cachedDay;

	private bool cachedIsMilestone;

	public override void Inject(UIBehaviour behaviour)
	{
		base.Inject(behaviour);
		buttonBox = behaviour.Get<Button>("buttonBox");
		textDay = behaviour.Get<TMP_Text>("textDay");
		textToday = behaviour.Get<TMP_Text>("textToday");
		imgComplete = behaviour.Get<Image>("imgComplete");
		imaBoxClose = behaviour.Get<Image>("imaBoxClose");
		imgBoxOpen = behaviour.Get<Image>("imgBoxOpen");
		imgBg = behaviour.Get<Image>("imgBg");
		eff_complete = behaviour.Get<UIEffectLoader>("eff_complete");
		anim = behaviour.Get<Animation>("anim");
	}

	public void Apply(int day, int periodDays, int currentDay, bool isMilestone, int pendingRewardDay, int targetDay, Action<int, RectTransform> onBoxClicked)
	{
		KillCompleteTimer();
		cachedDay = day;
		cachedIsMilestone = isMilestone;
		bool completed = day <= currentDay && day != pendingRewardDay;
		ApplyCompletionVisual(isMilestone, completed);
		SetCompleteEffectVisible(visible: false);
		ApplyWeekdayMarker(day, targetDay);
		BindBoxClick(day, isMilestone, onBoxClicked);
	}

	public void SetRewardPreviewInteractable(bool interactable)
	{
		if ((Object)(object)buttonBox != (Object)null)
		{
			((Selectable)buttonBox).interactable = interactable && cachedIsMilestone;
		}
	}

	public void SetCompleteVisual(bool isMilestone, bool completed)
	{
		KillCompleteTimer();
		ApplyCompletionVisual(isMilestone, completed);
		SetCompleteEffectVisible(visible: false);
	}

	public void PlayCompletePresentation(bool isMilestone, Action onComplete = null)
	{
		KillCompleteTimer();
		StopChestClickLoop();
		PlayCompleteEffect();
		if (!isMilestone)
		{
			PlayCompleteAnimation();
		}
		if (onComplete != null)
		{
			completeTimerId = GameApp.Timer.Delay((Object)(object)((UIBaseView)this).Behaviour, 0.5f, (Action)delegate
			{
				ApplyCompletionVisual(isMilestone, completed: true);
				completeTimerId = -1;
				onComplete();
			});
		}
	}

	public void RefreshState(int day, int periodDays, int currentDay, bool isMilestone, int pendingRewardDay, int targetDay)
	{
		cachedDay = day;
		cachedIsMilestone = isMilestone;
		bool completed = day <= currentDay && day != pendingRewardDay;
		ApplyCompletionVisual(isMilestone, completed);
		ApplyWeekdayMarker(day, targetDay);
	}

	public override void Dispose()
	{
		KillCompleteTimer();
		StopCompleteAnimation();
		StopChestClickLoop();
		if ((Object)(object)buttonBox != (Object)null)
		{
			((UnityEventBase)buttonBox.onClick).RemoveAllListeners();
		}
		base.Dispose();
	}

	private void ApplyCompletionVisual(bool isMilestone, bool completed)
	{
		SetActive(imgBg, !isMilestone);
		if (isMilestone)
		{
			SetActive(imaBoxClose, !completed);
			SetActive(imgBoxOpen, completed);
			SetActive(imgComplete, active: false);
		}
		else
		{
			SetActive(imaBoxClose, active: false);
			SetActive(imgBoxOpen, active: false);
			SetActive(imgComplete, completed);
		}
		RefreshChestClickLoop(cachedDay, cachedIsMilestone, completed);
	}

	private void ApplyWeekdayMarker(int day, int targetDay)
	{
		bool flag = day == targetDay;
		if ((Object)(object)textToday != (Object)null)
		{
			((Component)textToday).gameObject.SetActive(flag);
		}
		if ((Object)(object)textDay != (Object)null)
		{
			((Component)textDay).gameObject.SetActive(!flag);
		}
		TMP_Text val = (flag ? textToday : textDay);
		if (!((Object)(object)val == (Object)null))
		{
			int num = ResolveWeekdayIndexForDay(day, targetDay);
			TMPTextExtensions.SetLocalizedTextValue(val, DailyFirstWinModule.UIHost.GetText(string.Format("{0}{1}", "FirstWin_Weekday_", num)));
		}
	}

	private static int ResolveWeekdayIndexForDay(int day, int targetDay)
	{
		return ((ToLangWeekdayIndex(GetTodayDayOfWeek()) - 1 + day - targetDay) % 7 + 7) % 7 + 1;
	}

	private static DayOfWeek GetTodayDayOfWeek()
	{
		return Time.TimeStamp2DateTime(DailyFirstWinModule.Host.GetCurrentTimestamp()).DayOfWeek;
	}

	private static int ToLangWeekdayIndex(DayOfWeek dayOfWeek)
	{
		if (dayOfWeek != DayOfWeek.Sunday)
		{
			return (int)dayOfWeek;
		}
		return 7;
	}

	private void BindBoxClick(int day, bool isMilestone, Action<int, RectTransform> onBoxClicked)
	{
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Expected O, but got Unknown
		if ((Object)(object)buttonBox == (Object)null)
		{
			return;
		}
		((UnityEventBase)buttonBox.onClick).RemoveAllListeners();
		if (!isMilestone)
		{
			((Selectable)buttonBox).interactable = false;
			return;
		}
		((Selectable)buttonBox).interactable = true;
		RectTransform target = default(RectTransform);
		ref RectTransform reference = ref target;
		Transform transform = ((Component)buttonBox).transform;
		reference = (RectTransform)(object)((transform is RectTransform) ? transform : null);
		int capturedDay = day;
		((UnityEvent)buttonBox.onClick).AddListener((UnityAction)delegate
		{
			onBoxClicked?.Invoke(capturedDay, target);
		});
	}

	private void SetCompleteEffectVisible(bool visible)
	{
		if ((Object)(object)eff_complete != (Object)null)
		{
			((Component)eff_complete).gameObject.SetActive(visible);
		}
		if (!visible)
		{
			StopCompleteAnimation();
		}
	}

	private void PlayCompleteAnimation()
	{
		if ((Object)(object)anim == (Object)null || (Object)(object)anim.GetClip("anim_dayitem_complete") == (Object)null)
		{
			return;
		}
		StopChestClickLoop();
		SetActive(imgComplete, active: true);
		AnimationState val = anim["anim_dayitem_complete"];
		if (!((TrackedReference)(object)val == (TrackedReference)null))
		{
			if (anim.IsPlaying("anim_dayitem_complete"))
			{
				anim.Stop("anim_dayitem_complete");
				val.time = 0f;
				anim.Sample();
			}
			anim.Rewind("anim_dayitem_complete");
			val.wrapMode = (WrapMode)8;
			anim.Play("anim_dayitem_complete");
		}
	}

	private void StopCompleteAnimation()
	{
		if (!((Object)(object)anim == (Object)null) && !((Object)(object)anim.GetClip("anim_dayitem_complete") == (Object)null))
		{
			anim.Stop("anim_dayitem_complete");
			anim.Rewind("anim_dayitem_complete");
			anim.Sample();
		}
	}

	private void RefreshChestClickLoop(int day, bool isMilestone, bool completed)
	{
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Invalid comparison between Unknown and I4
		if (!(day == 7 && isMilestone) || completed)
		{
			StopChestClickLoop();
			return;
		}
		if ((Object)(object)anim == (Object)null || (Object)(object)anim.GetClip("anim_imaBoxClose_chest_click") == (Object)null)
		{
			StopChestClickLoop();
			return;
		}
		AnimationState val = anim["anim_imaBoxClose_chest_click"];
		if ((TrackedReference)(object)val == (TrackedReference)null)
		{
			StopChestClickLoop();
		}
		else if (!anim.IsPlaying("anim_imaBoxClose_chest_click") || (int)val.wrapMode != 2)
		{
			StopCompleteAnimation();
			anim.Stop("anim_imaBoxClose_chest_click");
			anim.Rewind("anim_imaBoxClose_chest_click");
			val.wrapMode = (WrapMode)2;
			anim.Play("anim_imaBoxClose_chest_click");
		}
	}

	private void StopChestClickLoop()
	{
		if (!((Object)(object)anim == (Object)null) && !((Object)(object)anim.GetClip("anim_imaBoxClose_chest_click") == (Object)null))
		{
			anim.Stop("anim_imaBoxClose_chest_click");
			anim.Rewind("anim_imaBoxClose_chest_click");
			anim.Sample();
		}
	}

	private void PlayCompleteEffect()
	{
		if (!((Object)(object)eff_complete == (Object)null))
		{
			((Component)eff_complete).gameObject.SetActive(true);
			eff_complete.Play();
			AudioService.PlayUiSound(AudioKeys.Unlock.Value);
			VibrateUtility.VibrateDailyFirstWinLightUp("DailyFirstWin-DayItem打勾点亮");
		}
	}

	private void KillCompleteTimer()
	{
		if (completeTimerId >= 0)
		{
			GameApp.Timer.Kill(completeTimerId);
			completeTimerId = -1;
		}
	}

	private static void SetActive(Image image, bool active)
	{
		if ((Object)(object)image != (Object)null)
		{
			((Component)image).gameObject.SetActive(active);
		}
	}
}
