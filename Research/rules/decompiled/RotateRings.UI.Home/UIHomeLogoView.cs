using System;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using Game.UI;
using Spine;
using Spine.Unity;
using UniLog;
using UnityEngine;
using XingYun.UI.Core;

namespace RotateRings.UI.Home;

public class UIHomeLogoView : GameWidgetView
{
	private const string AnimOpen = "open";

	private const string AnimIdle = "idle";

	private const int TrackIndex = 0;

	public SkeletonGraphic spineLogo;

	public CanvasGroup canvasGroup;

	public override void Inject(UIBehaviour behaviour)
	{
		base.Inject(behaviour);
		spineLogo = behaviour.Get<SkeletonGraphic>("spineLogo");
		canvasGroup = behaviour.Get<CanvasGroup>("canvasGroup");
	}

	public void SetAlphaImmediate(float alpha)
	{
		if ((Object)(object)canvasGroup != (Object)null)
		{
			canvasGroup.alpha = alpha;
		}
	}

	public void FadeOut(float duration, Action onComplete)
	{
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Expected O, but got Unknown
		if ((Object)(object)canvasGroup == (Object)null)
		{
			onComplete?.Invoke();
			return;
		}
		UIBehaviour behaviour = ((UIBaseView)this).Behaviour;
		GameObject val = ((behaviour != null) ? ((Component)behaviour).gameObject : null);
		if ((Object)(object)val == (Object)null)
		{
			onComplete?.Invoke();
			return;
		}
		DOTween.Kill((object)val, false);
		canvasGroup.alpha = 1f;
		TweenSettingsExtensions.OnComplete<TweenerCore<float, float, FloatOptions>>(TweenSettingsExtensions.SetLink<TweenerCore<float, float, FloatOptions>>(TweenSettingsExtensions.SetTarget<TweenerCore<float, float, FloatOptions>>(DOTweenModuleUI.DOFade(canvasGroup, 0f, duration), (object)val), val, (LinkBehaviour)6), (TweenCallback)delegate
		{
			try
			{
				onComplete?.Invoke();
			}
			catch (Exception ex)
			{
				Log.LogError("UI", "[UIHomeLogoView] FadeOut callback error: {0}", new object[1] { ex.Message });
			}
		});
	}

	public void PlayOpenAnimation()
	{
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b8: Expected O, but got Unknown
		if ((Object)(object)spineLogo == (Object)null)
		{
			return;
		}
		AnimationState state = spineLogo.AnimationState;
		if (state == null)
		{
			return;
		}
		spineLogo.Initialize(false);
		Skeleton skeleton = spineLogo.Skeleton;
		SkeletonData skeletonData = ((skeleton != null) ? skeleton.Data : null);
		if (skeletonData == null)
		{
			return;
		}
		if (skeletonData.FindAnimation("open") == null)
		{
			PlayIdle(state, skeletonData);
			return;
		}
		TrackEntry obj = state.SetAnimation(0, "open", false);
		TrackEntryDelegate handler = null;
		handler = (TrackEntryDelegate)delegate
		{
			state.Complete -= handler;
			PlayIdle(state, skeletonData);
		};
		obj.Complete += handler;
	}

	private void PlayIdle(AnimationState state, SkeletonData skeletonData)
	{
		if (skeletonData.FindAnimation("idle") != null)
		{
			state.SetAnimation(0, "idle", true);
		}
	}

	public override void Dispose()
	{
		if ((Object)(object)((UIBaseView)this).Behaviour != (Object)null && (Object)(object)((Component)((UIBaseView)this).Behaviour).gameObject != (Object)null)
		{
			DOTween.Kill((object)((Component)((UIBaseView)this).Behaviour).gameObject, false);
		}
		if ((Object)(object)spineLogo != (Object)null && spineLogo.AnimationState != null)
		{
			spineLogo.AnimationState.ClearTracks();
		}
		spineLogo = null;
		canvasGroup = null;
		base.Dispose();
	}
}
