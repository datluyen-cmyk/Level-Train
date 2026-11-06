using System;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using RotateRings.Common;
using RotateRings.Services;
using UniLog;
using UnityEngine;

internal sealed class RingLockShakeService
{
	private const string TAG = "[UnlockRing][RingBlockedShakeService] ";

	internal void PlayLockShakeService(Transform target, GameObject linkTarget, Action onComplete = null)
	{
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0109: Unknown result type (might be due to invalid IL or missing references)
		//IL_0113: Expected O, but got Unknown
		if ((Object)(object)target == (Object)null || (Object)(object)linkTarget == (Object)null)
		{
			onComplete?.Invoke();
			return;
		}
		RingLevelConfig config = RingLevelConfig.Instance;
		float originZ = target.localEulerAngles.z;
		AudioService.PlayUiSound(AudioKeys.UiDeny.Value);
		AnimationCurve curve = ((config.blockedShakeCurve != null && config.blockedShakeCurve.keys.Length != 0) ? config.blockedShakeCurve : RingLevelConfig.CreateDefaultBlockedShakeCurve());
		ShortcutExtensions.DOKill((Component)(object)target, false);
		float progress = 0f;
		TweenSettingsExtensions.OnComplete<TweenerCore<float, float, FloatOptions>>(TweenSettingsExtensions.SetLink<TweenerCore<float, float, FloatOptions>>(TweenSettingsExtensions.SetTarget<TweenerCore<float, float, FloatOptions>>(TweenSettingsExtensions.SetEase<TweenerCore<float, float, FloatOptions>>(DOTween.To((DOGetter<float>)(() => progress), (DOSetter<float>)delegate(float value)
		{
			//IL_003d: Unknown result type (might be due to invalid IL or missing references)
			progress = value;
			float num = curve.Evaluate(progress) * config.blockedShakeStrength;
			target.localEulerAngles = new Vector3(0f, 0f, originZ + num);
		}, 1f, config.blockedShakeDuration), (Ease)1), (object)target), linkTarget, (LinkBehaviour)6), (TweenCallback)delegate
		{
			//IL_0016: Unknown result type (might be due to invalid IL or missing references)
			try
			{
				target.localEulerAngles = new Vector3(0f, 0f, originZ);
				onComplete?.Invoke();
			}
			catch (Exception ex)
			{
				Log.LogError("[UnlockRing][RingBlockedShakeService] ", "PlayBlockedShake OnComplete error: {0}", new object[1] { ex.Message });
			}
		});
	}
}
