using System;
using Game.UI;
using RotateRings.DailyFirstWin;
using Script.Tools;
using Spine;
using Spine.Unity;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using XingYun.Extensions;
using XingYun.UI.Core;

namespace RotateRings.UI.DailyFirstWin;

public class UIButtonItemDailyFirstWinView : GameWidgetView
{
	private const string HomeBtnWinKey = "FirstWin_Home_Btn_Win";

	private const string AnimIdle = "idle";

	private const int TrackIndex = 0;

	public Button ButtonItem;

	public Image Bg;

	public Image Icon;

	public SkeletonGraphic icon_spine;

	public TMP_Text Desc;

	public TMP_Text LockDesc;

	public Image IconLock;

	public Image imgDay;

	private Action onClick;

	private bool isClickBound;

	private int countdownTimerId = -1;

	private int lastDisplayedDay = int.MinValue;

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
		imgDay = behaviour.Get<Image>("imgDay");
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
		lastDisplayedDay = int.MinValue;
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
		if (val == null || val.FindAnimation("idle") == null)
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
		iconSpinePlayEntry = animationState.SetAnimation(0, "idle", false);
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
		onClick?.Invoke();
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
		if (!TryGetConfig(out var config) || config.OpenLevel <= 0)
		{
			SetRootVisible(visible: false);
			return;
		}
		if (!DailyFirstWinService.Instance.IsEnabled())
		{
			SetRootVisible(visible: false);
			return;
		}
		SetRootVisible(visible: true);
		SetupVisualElements();
		RefreshDaySprite();
		RefreshUnlockedState();
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

	private void RefreshDaySprite()
	{
		if ((Object)(object)imgDay == (Object)null)
		{
			return;
		}
		int num = GameApp.Proxy.Get<DailyFirstWinProxy>()?.GetCurrentDayInCycle() ?? 0;
		if (num != lastDisplayedDay)
		{
			lastDisplayedDay = num;
			int num2 = Mathf.Clamp(num, 0, 7);
			Sprite val = SpriteManager.LoadSprite($"firstwin_txt_{num2}");
			if ((Object)(object)val != (Object)null)
			{
				imgDay.sprite = val;
			}
		}
	}

	private void RefreshUnlockedState()
	{
		SetButtonInteractable(interactable: true);
		SetCountdownText();
	}

	private void SetRootVisible(bool visible)
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
		if (DailyFirstWinService.Instance.IsEnabled())
		{
			OnSecondTick();
			if (countdownTimerId < 0)
			{
				countdownTimerId = GameApp.Timer.Repeat(1f, (Action)OnSecondTick);
			}
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
			long remainSecondsUntilReset = DailyFirstWinService.Instance.GetRemainSecondsUntilReset();
			if (remainSecondsUntilReset > 0 && HasFirstWinToday())
			{
				TMPTextExtensions.SetLocalizedTextValue(Desc, GetWinText());
			}
			else
			{
				TMPTextExtensions.SetLocalizedTextValue(Desc, FormatCountdown(remainSecondsUntilReset));
			}
		}
	}

	private void OnSecondTick()
	{
		if (!DailyFirstWinService.Instance.IsEnabled())
		{
			StopCountdown();
			return;
		}
		DailyFirstWinService.Instance.RefreshOnHome();
		RefreshDaySprite();
		SetCountdownText();
	}

	private bool HasFirstWinToday()
	{
		return GameApp.Proxy.Get<DailyFirstWinProxy>()?.HasFirstWinToday() ?? false;
	}

	private string GetWinText()
	{
		if (DailyFirstWinModule.UIHost != null)
		{
			return DailyFirstWinModule.UIHost.GetText("FirstWin_Home_Btn_Win");
		}
		return "FirstWin_Home_Btn_Win";
	}

	private bool TryGetConfig(out DailyFirstWinConfigSnapshot config)
	{
		config = default(DailyFirstWinConfigSnapshot);
		if (!DailyFirstWinModule.IsInitialized)
		{
			return false;
		}
		DailyFirstWinConfigSnapshot? dailyFirstWinConfigSnapshot = DailyFirstWinModule.Host?.GetConfig();
		if (!dailyFirstWinConfigSnapshot.HasValue)
		{
			return false;
		}
		config = dailyFirstWinConfigSnapshot.Value;
		return true;
	}

	private string FormatCountdown(long totalSeconds)
	{
		if (totalSeconds <= 0)
		{
			return "00:00:00";
		}
		int num = (int)(totalSeconds / 3600);
		int num2 = (int)(totalSeconds % 3600 / 60);
		int num3 = (int)(totalSeconds % 60);
		return $"{num:D2}:{num2:D2}:{num3:D2}";
	}
}
