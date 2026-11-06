using System;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using UniLog;
using UnityEngine;

namespace RotateRings.GamePlay.PartStyle.Jelly;

internal sealed class JellyShakeAnimator
{
	private const string TAG = "[RotateRing][JellyShakeAnimator] ";

	private static readonly int IdAtlasPadding = Shader.PropertyToID("_AtlasPaddingPixels");

	private readonly MeshRenderer renderer;

	private readonly Action onComplete;

	public MeshRenderer Renderer => renderer;

	public bool IsPlaying { get; private set; }

	public JellyShakeAnimator(MeshRenderer renderer, Action onComplete = null)
	{
		this.renderer = renderer;
		this.onComplete = onComplete;
	}

	public void Trigger()
	{
		//IL_00e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ea: Expected O, but got Unknown
		if (!IsRendererValid())
		{
			IsPlaying = false;
			return;
		}
		RingLevelConfig instance = RingLevelConfig.Instance;
		float overshoot = instance.jellyOvershoot;
		float peak = instance.jellyPeak;
		float damping = instance.jellyDamping;
		float frequency = instance.jellyFrequency;
		ShortcutExtensions.DOKill((Component)(object)renderer, false);
		IsPlaying = true;
		ApplyPadding(overshoot);
		float num = Mathf.Max(instance.jellyDuration, 0.01f);
		float progress = 0f;
		TweenSettingsExtensions.OnComplete<TweenerCore<float, float, FloatOptions>>(TweenSettingsExtensions.SetLink<TweenerCore<float, float, FloatOptions>>(TweenSettingsExtensions.SetTarget<TweenerCore<float, float, FloatOptions>>(TweenSettingsExtensions.SetEase<TweenerCore<float, float, FloatOptions>>(DOTween.To((DOGetter<float>)(() => progress), (DOSetter<float>)delegate(float value)
		{
			progress = value;
			ApplyPadding(EvaluateJelly(value, overshoot, peak, damping, frequency));
		}, 1f, num), instance.GetJellyEase()), (object)renderer), ((Component)renderer).gameObject, (LinkBehaviour)6), (TweenCallback)delegate
		{
			try
			{
				IsPlaying = false;
				ApplyPadding(0f);
				onComplete?.Invoke();
			}
			catch (Exception ex)
			{
				Log.LogError("[RotateRing][JellyShakeAnimator] ", "OnComplete error: {0}", new object[1] { ex.Message });
			}
		});
	}

	public void StopAndReset()
	{
		if ((Object)(object)renderer != (Object)null)
		{
			ShortcutExtensions.DOKill((Component)(object)renderer, false);
		}
		IsPlaying = false;
		if (IsRendererValid())
		{
			ApplyPadding(0f);
		}
	}

	private bool IsRendererValid()
	{
		Material val = (((Object)(object)renderer != (Object)null) ? ((Renderer)renderer).sharedMaterial : null);
		if ((Object)(object)val != (Object)null)
		{
			return val.HasProperty(IdAtlasPadding);
		}
		return false;
	}

	private float EvaluateJelly(float t, float overshoot, float peak, float damping, float frequency)
	{
		float num = frequency * MathF.PI * 2f;
		float num2 = Mathf.Exp((0f - damping) * num * t);
		float num3 = Mathf.Atan2(0f - overshoot, peak);
		return Mathf.Sqrt(overshoot * overshoot + peak * peak) * num2 * Mathf.Sin(num * t + num3);
	}

	private void ApplyPadding(float value)
	{
		Material sharedMaterial = ((Renderer)renderer).sharedMaterial;
		if (!((Object)(object)sharedMaterial == (Object)null) && sharedMaterial.HasProperty(IdAtlasPadding))
		{
			sharedMaterial.SetFloat(IdAtlasPadding, value);
			((Renderer)renderer).SetPropertyBlock((MaterialPropertyBlock)null);
		}
	}
}
