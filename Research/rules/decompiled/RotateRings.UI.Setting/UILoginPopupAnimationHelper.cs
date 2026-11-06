using System;
using UnityEngine;
using XingYun.UI.Core;

namespace RotateRings.UI.Setting;

internal static class UILoginPopupAnimationHelper
{
	private const string AnimationBindingKey = "UIAnimation";

	private const string OpenAnimationName = "anim_popup_open2";

	private const string CloseAnimationName = "anim_popup_close2";

	public static Animation ResolveAnimation(UIBehaviour behaviour)
	{
		if ((Object)(object)behaviour == (Object)null || !behaviour.Contains("UIAnimation"))
		{
			return null;
		}
		return behaviour.Get<Animation>("UIAnimation");
	}

	public static void PlayOpen(Animation animation)
	{
		Play(animation, "anim_popup_open2");
	}

	public static void PlayClose(Animation animation, RectTransform nodeRect, Action onComplete = null)
	{
		AnimationClip clip = GetClip(animation, "anim_popup_close2");
		if ((Object)(object)clip == (Object)null)
		{
			onComplete?.Invoke();
			return;
		}
		Play(animation, "anim_popup_close2");
		if ((Object)(object)nodeRect != (Object)null)
		{
			GameApp.Timer.Delay((Object)(object)nodeRect, clip.length, (Action)delegate
			{
				onComplete?.Invoke();
			});
		}
		else
		{
			GameApp.Timer.Delay(clip.length, (Action)delegate
			{
				onComplete?.Invoke();
			});
		}
	}

	private static AnimationClip GetClip(Animation animation, string clipName)
	{
		if (!((Object)(object)animation != (Object)null) || string.IsNullOrEmpty(clipName))
		{
			return null;
		}
		return animation.GetClip(clipName);
	}

	private static void Play(Animation animation, string clipName)
	{
		if (!((Object)(object)GetClip(animation, clipName) == (Object)null))
		{
			animation.Stop();
			animation.Rewind(clipName);
			animation.Play(clipName);
		}
	}
}
