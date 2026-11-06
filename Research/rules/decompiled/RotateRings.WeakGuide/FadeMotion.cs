using System;
using System.Collections.Generic;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using UnityEngine;
using XingYun.Rendering;

namespace RotateRings.WeakGuide;

public sealed class FadeMotion : GuidMotion
{
	private const string TAG = "[WeakGuide][FadeMotion] ";

	private WeakGuideFadeTarget[] fadeTargets = Array.Empty<WeakGuideFadeTarget>();

	private readonly List<StyleLineUnderlayProxy> underlayProxies = new List<StyleLineUnderlayProxy>(4);

	private Tween tween;

	private float duration;

	private MaterialPropertyBlock sharedMpb;

	private void OnDisable()
	{
		Tween obj = tween;
		if (obj != null)
		{
			TweenExtensions.Kill(obj, false);
		}
		tween = null;
	}

	public void Configure(WeakGuideFadeTarget[] targets, float fadeDuration)
	{
		fadeTargets = targets ?? Array.Empty<WeakGuideFadeTarget>();
		duration = Mathf.Max(0f, fadeDuration);
		CacheUnderlayProxies();
		RestoreVisual();
	}

	public void ResetVisualState()
	{
		Tween obj = tween;
		if (obj != null)
		{
			TweenExtensions.Kill(obj, false);
		}
		tween = null;
		RestoreVisual();
	}

	public override void StartMove()
	{
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Expected O, but got Unknown
		Tween obj = tween;
		if (obj != null)
		{
			TweenExtensions.Kill(obj, false);
		}
		tween = null;
		RestoreVisual();
		base.StartMove();
		if (duration <= 0f)
		{
			SetAlpha(0f);
			NotifyMotionCompleted();
			base.StopMove();
			return;
		}
		float progress = 0f;
		tween = (Tween)(object)TweenSettingsExtensions.OnComplete<TweenerCore<float, float, FloatOptions>>(TweenSettingsExtensions.SetLink<TweenerCore<float, float, FloatOptions>>(TweenSettingsExtensions.SetTarget<TweenerCore<float, float, FloatOptions>>(TweenSettingsExtensions.SetEase<TweenerCore<float, float, FloatOptions>>(DOTween.To((DOGetter<float>)(() => progress), (DOSetter<float>)delegate(float value)
		{
			progress = value;
			SetAlpha(1f - value);
		}, 1f, duration), (Ease)1), (object)((Component)this).gameObject), ((Component)this).gameObject, (LinkBehaviour)5), (TweenCallback)delegate
		{
			try
			{
				tween = null;
				NotifyMotionCompleted();
			}
			catch (Exception)
			{
			}
			finally
			{
				base.StopMove();
			}
		});
	}

	public override void StopMove()
	{
		Tween obj = tween;
		if (obj != null)
		{
			TweenExtensions.Kill(obj, false);
		}
		tween = null;
		RestoreVisual();
		base.StopMove();
	}

	public override void Finish()
	{
		Tween obj = tween;
		if (obj != null)
		{
			TweenExtensions.Kill(obj, false);
		}
		tween = null;
		base.Finish();
	}

	private void RestoreVisual()
	{
		SetAlpha(1f);
	}

	private void SetAlpha(float alpha)
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Expected O, but got Unknown
		for (int i = 0; i < fadeTargets.Length; i++)
		{
			WeakGuideFadeTarget weakGuideFadeTarget = fadeTargets[i];
			Color originalColor = weakGuideFadeTarget.OriginalColor;
			originalColor.a *= alpha;
			Renderer val = weakGuideFadeTarget.Renderer;
			if ((Object)(object)val == (Object)null && (Object)(object)weakGuideFadeTarget.Material == (Object)null && (Object)(object)weakGuideFadeTarget.SourceRenderer != (Object)null)
			{
				val = ResolveProxyRenderer(weakGuideFadeTarget.SourceRenderer);
			}
			if ((Object)(object)val != (Object)null)
			{
				if (sharedMpb == null)
				{
					sharedMpb = new MaterialPropertyBlock();
				}
				val.GetPropertyBlock(sharedMpb);
				sharedMpb.SetColor(weakGuideFadeTarget.ColorProperty, originalColor);
				val.SetPropertyBlock(sharedMpb);
			}
			else if ((Object)(object)weakGuideFadeTarget.Material != (Object)null)
			{
				weakGuideFadeTarget.Material.SetColor(weakGuideFadeTarget.ColorProperty, originalColor);
			}
		}
		ForceSyncUnderlayProxies();
	}

	private static Renderer ResolveProxyRenderer(Renderer source)
	{
		if ((Object)(object)source == (Object)null)
		{
			return null;
		}
		Transform transform = ((Component)source).transform;
		for (int i = 0; i < transform.childCount; i++)
		{
			Transform child = transform.GetChild(i);
			if ((Object)(object)child != (Object)null && ((Object)child).name == "__StyleLineUnderlay")
			{
				Renderer component = (Renderer)(object)((Component)child).GetComponent<MeshRenderer>();
				if ((Object)(object)component != (Object)null)
				{
					return component;
				}
			}
		}
		return null;
	}

	private void CacheUnderlayProxies()
	{
		underlayProxies.Clear();
		for (int i = 0; i < fadeTargets.Length; i++)
		{
			Renderer sourceRenderer = fadeTargets[i].SourceRenderer;
			if (!((Object)(object)sourceRenderer == (Object)null))
			{
				StyleLineUnderlayProxy component = ((Component)sourceRenderer).GetComponent<StyleLineUnderlayProxy>();
				if ((Object)(object)component != (Object)null && !underlayProxies.Contains(component))
				{
					underlayProxies.Add(component);
				}
			}
		}
	}

	private void ForceSyncUnderlayProxies()
	{
		for (int i = 0; i < underlayProxies.Count; i++)
		{
			StyleLineUnderlayProxy val = underlayProxies[i];
			if (!((Object)(object)val == (Object)null) && ((Behaviour)val).isActiveAndEnabled)
			{
				val.Sync();
			}
		}
	}
}
