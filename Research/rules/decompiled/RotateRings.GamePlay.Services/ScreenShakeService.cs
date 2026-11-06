using System;
using System.Collections.Generic;
using DG.Tweening;
using UniLog;
using UnityEngine;
using XingYun.Experiment;

namespace RotateRings.GamePlay.Services;

public sealed class ScreenShakeService : GameplayServiceBase<GameplayManager>
{
	private const string TAG = "[RotateRing][ScreenShakeService] ";

	private readonly Dictionary<ScreenShakeTriggerType, ScreenShakePreset> presetLookup = new Dictionary<ScreenShakeTriggerType, ScreenShakePreset>();

	private Tween currentShake;

	private int currentPriority;

	private Transform currentTarget;

	private Vector3 baseLocalPos;

	private bool hasBasePos;

	private Transform DefaultShakeTarget
	{
		get
		{
			GameplayManager host = base.Host;
			if (host == null)
			{
				return null;
			}
			return ((Component)host.GameplayCam).transform;
		}
	}

	public ScreenShakeService(GameplayManager host)
		: base(host)
	{
		try
		{
			presetLookup = ScreenShakeConfigAsset.Instance.BuildLookup();
		}
		catch (Exception ex)
		{
			Log.LogError("[RotateRing][ScreenShakeService] ", "配置加载失败，震屏功能不可用: {0}", new object[1] { ex.Message });
		}
	}

	public void RequestShake(ScreenShakeTriggerType triggerType, Transform target = null)
	{
		if (!base.IsDisposed && IsScreenShakeEnabled() && presetLookup.TryGetValue(triggerType, out var value) && value != null && (currentShake == null || value.Priority > currentPriority))
		{
			StartShake(value, target);
		}
	}

	public void Stop()
	{
		if (currentShake != null)
		{
			TweenExtensions.Kill(currentShake, false);
			currentShake = null;
			currentPriority = 0;
			RestoreBasePosition();
		}
	}

	public override void Dispose()
	{
		if (currentShake != null)
		{
			TweenExtensions.Kill(currentShake, false);
			currentShake = null;
		}
		RestoreBasePosition();
		base.Dispose();
	}

	private static bool IsScreenShakeEnabled()
	{
		GameBord gameBord = FunctionDataCenter.Get<GameBord>();
		if (gameBord != null)
		{
			return gameBord.GameBordScreenShake == 1;
		}
		return false;
	}

	private void StartShake(ScreenShakePreset preset, Transform target)
	{
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f1: Unknown result type (might be due to invalid IL or missing references)
		//IL_0116: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ac: Expected O, but got Unknown
		Transform val = (((Object)(object)target != (Object)null) ? target : DefaultShakeTarget);
		if ((Object)(object)val == (Object)null)
		{
			return;
		}
		if ((Object)(object)currentTarget != (Object)null && (Object)(object)currentTarget != (Object)(object)val)
		{
			Tween obj = currentShake;
			if (obj != null)
			{
				TweenExtensions.Kill(obj, false);
			}
			currentShake = null;
			ShortcutExtensions.DOKill((Component)(object)currentTarget, false);
			RestoreBasePosition();
		}
		currentTarget = val;
		if (!hasBasePos)
		{
			baseLocalPos = currentTarget.localPosition;
			hasBasePos = true;
		}
		ShortcutExtensions.DOKill((Component)(object)currentTarget, false);
		Tween obj2 = currentShake;
		if (obj2 != null)
		{
			TweenExtensions.Kill(obj2, false);
		}
		currentTarget.localPosition = baseLocalPos;
		currentPriority = preset.Priority;
		float num = Mathf.Max(preset.Duration, 0.01f);
		Tween val2 = (Tween)(object)TweenSettingsExtensions.SetLink<Tweener>(TweenSettingsExtensions.SetTarget<Tweener>(TweenSettingsExtensions.SetEase<Tweener>(ShortcutExtensions.DOShakePosition(currentTarget, num, new Vector3(preset.Amplitude, preset.Amplitude, 0f), Mathf.Max(1, preset.Vibrato), preset.PositionRandomness, false, preset.FadeOut, (ShakeRandomnessMode)0), preset.Ease), (object)((Component)currentTarget).gameObject), ((Component)currentTarget).gameObject, (LinkBehaviour)5);
		if (preset.Delay > 0f)
		{
			currentShake = (Tween)(object)TweenSettingsExtensions.Append(TweenSettingsExtensions.AppendInterval(TweenSettingsExtensions.SetLink<Sequence>(TweenSettingsExtensions.SetTarget<Sequence>(DOTween.Sequence(), (object)((Component)currentTarget).gameObject), ((Component)currentTarget).gameObject, (LinkBehaviour)5), preset.Delay), val2);
		}
		else
		{
			currentShake = val2;
		}
		TweenSettingsExtensions.OnComplete<Tween>(currentShake, (TweenCallback)delegate
		{
			try
			{
				FinishShake();
			}
			catch (Exception ex)
			{
				Log.LogError("[RotateRing][ScreenShakeService] ", "OnComplete error: {0}", new object[1] { ex.Message });
			}
		});
	}

	private void FinishShake()
	{
		currentShake = null;
		currentPriority = 0;
		RestoreBasePosition();
		currentTarget = null;
	}

	private void RestoreBasePosition()
	{
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		if (hasBasePos)
		{
			if ((Object)(object)currentTarget != (Object)null)
			{
				currentTarget.localPosition = baseLocalPos;
			}
			hasBasePos = false;
		}
	}
}
