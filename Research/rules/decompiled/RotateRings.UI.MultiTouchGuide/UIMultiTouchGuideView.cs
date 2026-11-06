using System;
using Game.UI;
using RotateRings.Common;
using RotateRings.GamePlay.MultiTouchGuide;
using UnityEngine;
using UnityEngine.UI;
using XingYun.UI.Core;

namespace RotateRings.UI.MultiTouchGuide;

public class UIMultiTouchGuideView : GameBaseView
{
	private const string BgMaskOpenAnim = "anim_bgmask_open";

	private const string BgMaskCloseAnim = "anim_bgmask_close";

	public GameObject ZoomIn;

	public GameObject zoomOut;

	public GameObject Move;

	public Animation ZoomInAnim;

	public Animation zoomOutAnim;

	public Animation MoveAnim;

	public Button ZoomInBtn;

	public Button ZoomOutBtn;

	public Button MoveBtn;

	public void ShowStep(MultiTouchGuideStep step, Action onCurrentStepShown = null)
	{
		ApplyStepVisibility(ZoomIn, ZoomInAnim, step == MultiTouchGuideStep.ZoomIn);
		ApplyStepVisibility(zoomOut, zoomOutAnim, step == MultiTouchGuideStep.ZoomOut);
		ApplyStepVisibility(Move, MoveAnim, step == MultiTouchGuideStep.Pan);
		float clipLength = GetClipLength(GetStepAnimation(step), "anim_bgmask_open");
		ScheduleAfterSeconds(clipLength, onCurrentStepShown);
	}

	private Animation GetStepAnimation(MultiTouchGuideStep step)
	{
		return (Animation)(step switch
		{
			MultiTouchGuideStep.ZoomIn => ZoomInAnim, 
			MultiTouchGuideStep.ZoomOut => zoomOutAnim, 
			MultiTouchGuideStep.Pan => MoveAnim, 
			_ => null, 
		});
	}

	private static float GetClipLength(Animation anim, string clipName)
	{
		if ((Object)(object)anim == (Object)null)
		{
			return 0f;
		}
		AnimationClip clip = anim.GetClip(clipName);
		if (!((Object)(object)clip != (Object)null))
		{
			return 0f;
		}
		return clip.length;
	}

	public void HideAllSteps()
	{
		ApplyStepVisibility(ZoomIn, ZoomInAnim, shouldShow: false);
		ApplyStepVisibility(zoomOut, zoomOutAnim, shouldShow: false);
		ApplyStepVisibility(Move, MoveAnim, shouldShow: false);
	}

	public void HideStepGestureNodesForOpen()
	{
		SetStepGestureNodeInactive(ZoomIn);
		SetStepGestureNodeInactive(zoomOut);
		SetStepGestureNodeInactive(Move);
	}

	private static void SetStepGestureNodeInactive(GameObject stepNode)
	{
		if ((Object)(object)stepNode != (Object)null)
		{
			stepNode.SetActive(false);
		}
	}

	public void PlayStepSuccess(Action onComplete)
	{
		onComplete?.Invoke();
	}

	public void PlayOpen()
	{
	}

	public void PlayClose(Action onComplete)
	{
		float num = 0f;
		num = Mathf.Max(num, HideStepNodeWithCloseAnim(ZoomIn, ZoomInAnim));
		num = Mathf.Max(num, HideStepNodeWithCloseAnim(zoomOut, zoomOutAnim));
		num = Mathf.Max(num, HideStepNodeWithCloseAnim(Move, MoveAnim));
		if (num <= 0f)
		{
			onComplete?.Invoke();
		}
		else
		{
			ScheduleAfterSeconds(num, onComplete);
		}
	}

	private void ApplyStepVisibility(GameObject stepNode, Animation stepAnim, bool shouldShow)
	{
		if ((Object)(object)stepNode == (Object)null)
		{
			return;
		}
		if (shouldShow)
		{
			if (!stepNode.activeSelf)
			{
				stepNode.SetActive(true);
			}
			PlayOpenAnimation(stepAnim);
		}
		else if (stepNode.activeSelf)
		{
			PlayCloseAnimation(stepNode, stepAnim, delegate
			{
				stepNode.SetActive(false);
			});
		}
	}

	private float HideStepNodeWithCloseAnim(GameObject stepNode, Animation stepAnim)
	{
		if ((Object)(object)stepNode == (Object)null || !stepNode.activeSelf)
		{
			return 0f;
		}
		float duration = 0f;
		PlayCloseAnimation(stepNode, stepAnim, delegate
		{
			if ((Object)(object)stepNode != (Object)null)
			{
				stepNode.SetActive(false);
			}
		}, out duration);
		return duration;
	}

	private static void PlayOpenAnimation(Animation anim)
	{
		UIAnimUtil.PlayUIOpenAnimation(anim, "anim_bgmask_open");
	}

	private void PlayCloseAnimation(GameObject timerHost, Animation anim, Action onComplete, out float duration)
	{
		duration = 0f;
		if ((Object)(object)anim == (Object)null || (Object)(object)anim.GetClip("anim_bgmask_close") == (Object)null)
		{
			onComplete?.Invoke();
			return;
		}
		anim.Rewind("anim_bgmask_close");
		anim.Play("anim_bgmask_close");
		duration = anim.GetClip("anim_bgmask_close").length;
		ScheduleAfterSeconds(timerHost, duration, onComplete);
	}

	private void PlayCloseAnimation(GameObject timerHost, Animation anim, Action onComplete)
	{
		PlayCloseAnimation(timerHost, anim, onComplete, out var _);
	}

	private void ScheduleAfterSeconds(float seconds, Action onComplete)
	{
		ScheduleAfterSeconds(((Object)(object)((UIBaseView)this).Behaviour != (Object)null) ? ((Component)((UIBaseView)this).Behaviour).gameObject : null, seconds, onComplete);
	}

	private static void ScheduleAfterSeconds(GameObject timerHost, float seconds, Action onComplete)
	{
		if (onComplete != null)
		{
			if ((Object)(object)timerHost != (Object)null)
			{
				GameApp.Timer.Delay((Object)(object)timerHost, seconds, onComplete);
			}
			else
			{
				GameApp.Timer.Delay(seconds, onComplete);
			}
		}
	}

	public override void Inject(UIBehaviour behaviour)
	{
		((UIBaseView)this).Inject(behaviour);
		ZoomIn = behaviour.Get<GameObject>("ZoomIn");
		zoomOut = behaviour.Get<GameObject>("zoomOut");
		Move = behaviour.Get<GameObject>("Move");
		ZoomInAnim = behaviour.Get<Animation>("ZoomInAnim");
		zoomOutAnim = behaviour.Get<Animation>("zoomOutAnim");
		MoveAnim = behaviour.Get<Animation>("MoveAnim");
		ZoomInBtn = behaviour.Get<Button>("ZoomInBtn");
		ZoomOutBtn = behaviour.Get<Button>("ZoomOutBtn");
		MoveBtn = behaviour.Get<Button>("MoveBtn");
	}
}
