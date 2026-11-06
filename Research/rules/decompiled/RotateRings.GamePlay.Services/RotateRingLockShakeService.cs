using System;
using System.Collections.Generic;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using RotateRings.Common;
using RotateRings.Services;
using UniLog;
using UnityEngine;

namespace RotateRings.GamePlay.Services;

public sealed class RotateRingLockShakeService : GameplayServiceBase<GameplayManager>
{
	private sealed class ActiveShake
	{
		public readonly Transform Target;

		public readonly MeshRenderer[] Renderers;

		public readonly Action OnComplete;

		public bool Done;

		public ActiveShake(Transform target, MeshRenderer[] renderers, Action onComplete)
		{
			Target = target;
			Renderers = renderers;
			OnComplete = onComplete;
		}
	}

	private const string TAG = "[RotateRing][RingBlockedShakeService] ";

	private static readonly int IdBlockedShakeAngle = Shader.PropertyToID("_BlockedShakeAngle");

	private static readonly int IdRectMaskRect = Shader.PropertyToID("_StyleLineRectMaskRect");

	private static readonly int IdRectMaskSoftness = Shader.PropertyToID("_StyleLineRectMaskSoftness");

	private static readonly List<MeshRenderer> RendererBuffer = new List<MeshRenderer>(8);

	private static MaterialPropertyBlock s_mpb;

	private readonly List<ActiveShake> activeShakes = new List<ActiveShake>();

	public RotateRingLockShakeService(GameplayManager host)
		: base(host)
	{
	}

	public void PlayLockShakeService(Transform target, GameObject linkTarget, Action onComplete = null)
	{
		//IL_00fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_0104: Expected O, but got Unknown
		if ((Object)(object)target == (Object)null || (Object)(object)linkTarget == (Object)null)
		{
			onComplete?.Invoke();
			return;
		}
		RemoveActiveShake(target);
		CollectShakeRenderers(target);
		if (RendererBuffer.Count == 0)
		{
			onComplete?.Invoke();
			return;
		}
		RingLevelConfig config = RingLevelConfig.Instance;
		AnimationCurve curve = GetShakeCurve(config);
		ActiveShake shake = new ActiveShake(target, RendererBuffer.ToArray(), onComplete);
		activeShakes.Add(shake);
		AudioService.PlayUiSound(AudioKeys.UiDeny.Value);
		ShortcutExtensions.DOKill((Component)(object)target, false);
		float progress = 0f;
		TweenSettingsExtensions.OnComplete<TweenerCore<float, float, FloatOptions>>(TweenSettingsExtensions.SetLink<TweenerCore<float, float, FloatOptions>>(TweenSettingsExtensions.SetTarget<TweenerCore<float, float, FloatOptions>>(TweenSettingsExtensions.SetEase<TweenerCore<float, float, FloatOptions>>(DOTween.To((DOGetter<float>)(() => progress), (DOSetter<float>)delegate(float value)
		{
			progress = value;
			ApplyShakeAngle(shake.Renderers, curve.Evaluate(value) * config.blockedShakeStrength);
		}, 1f, config.blockedShakeDuration), (Ease)1), (object)target), linkTarget, (LinkBehaviour)6), (TweenCallback)delegate
		{
			try
			{
				FinishShake(shake);
			}
			catch (Exception ex)
			{
				Log.LogError("[RotateRing][RingBlockedShakeService] ", "OnComplete error: {0}", new object[1] { ex.Message });
			}
		});
	}

	public void StopActiveShake()
	{
		FinishAll(null);
	}

	public void StopActiveShakeExcept(Transform exceptTarget)
	{
		FinishAll(exceptTarget);
	}

	private void FinishShake(ActiveShake shake)
	{
		if (shake.Done)
		{
			return;
		}
		shake.Done = true;
		activeShakes.Remove(shake);
		Transform target = shake.Target;
		if (target != null)
		{
			ShortcutExtensions.DOKill((Component)(object)target, false);
		}
		ApplyShakeAngle(shake.Renderers, 0f);
		try
		{
			shake.OnComplete?.Invoke();
		}
		catch (Exception ex)
		{
			Log.LogError("[RotateRing][RingBlockedShakeService] ", "Finish error: {0}", new object[1] { ex.Message });
		}
	}

	private void RemoveActiveShake(Transform target)
	{
		for (int num = activeShakes.Count - 1; num >= 0; num--)
		{
			if ((Object)(object)activeShakes[num].Target == (Object)(object)target)
			{
				FinishShake(activeShakes[num]);
			}
		}
	}

	private void FinishAll(Transform exceptTarget)
	{
		for (int num = activeShakes.Count - 1; num >= 0; num--)
		{
			ActiveShake activeShake = activeShakes[num];
			if (!((Object)(object)exceptTarget != (Object)null) || !((Object)(object)activeShake.Target == (Object)(object)exceptTarget))
			{
				FinishShake(activeShake);
			}
		}
	}

	private static AnimationCurve GetShakeCurve(RingLevelConfig config)
	{
		if (config.blockedShakeCurve == null || config.blockedShakeCurve.keys.Length == 0)
		{
			return RingLevelConfig.CreateDefaultBlockedShakeCurve();
		}
		return config.blockedShakeCurve;
	}

	private static void CollectShakeRenderers(Transform target)
	{
		RendererBuffer.Clear();
		if ((Object)(object)target == (Object)null)
		{
			return;
		}
		((Component)target).GetComponentsInChildren<MeshRenderer>(true, RendererBuffer);
		for (int num = RendererBuffer.Count - 1; num >= 0; num--)
		{
			MeshRenderer val = RendererBuffer[num];
			Material val2 = (((Object)(object)val != (Object)null) ? ((Renderer)val).sharedMaterial : null);
			if ((Object)(object)val2 == (Object)null || !val2.HasProperty(IdBlockedShakeAngle))
			{
				RendererBuffer.RemoveAt(num);
			}
		}
	}

	private static void ApplyShakeAngle(MeshRenderer[] renderers, float angleDeg)
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Expected O, but got Unknown
		if (renderers == null)
		{
			return;
		}
		if (s_mpb == null)
		{
			s_mpb = new MaterialPropertyBlock();
		}
		foreach (MeshRenderer val in renderers)
		{
			if (!((Object)(object)val == (Object)null))
			{
				Material sharedMaterial = ((Renderer)val).sharedMaterial;
				if (!((Object)(object)sharedMaterial == (Object)null) && sharedMaterial.HasProperty(IdBlockedShakeAngle))
				{
					((Renderer)val).GetPropertyBlock(s_mpb);
					EnsureRectMaskOnPropertyBlock(sharedMaterial, s_mpb);
					s_mpb.SetFloat(IdBlockedShakeAngle, angleDeg);
					((Renderer)val).SetPropertyBlock(s_mpb);
				}
			}
		}
	}

	private static void EnsureRectMaskOnPropertyBlock(Material mat, MaterialPropertyBlock mpb)
	{
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		if (mat.HasProperty(IdRectMaskRect) && !mpb.HasVector(IdRectMaskRect))
		{
			mpb.SetVector(IdRectMaskRect, mat.GetVector(IdRectMaskRect));
			if (mat.HasProperty(IdRectMaskSoftness))
			{
				mpb.SetVector(IdRectMaskSoftness, mat.GetVector(IdRectMaskSoftness));
			}
		}
	}
}
