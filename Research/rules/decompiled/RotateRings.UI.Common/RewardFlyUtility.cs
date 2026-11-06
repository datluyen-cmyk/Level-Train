using System;
using System.Collections.Generic;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using Game.UI.ClaimReward;
using UniLog;
using UnityEngine;

namespace RotateRings.UI.Common;

public static class RewardFlyUtility
{
	public sealed class Settings
	{
		public float StartDelay;

		public float ItemInterval = 0.05f;

		public float Duration = 0.5f;

		public Ease Ease = (Ease)9;

		public bool DeactivateOnArrive = true;

		public bool DeactivateOnKill = true;

		public bool HideUntilStart;

		public PropFlyPathAsset PathConfig;

		public Action<RectTransform> OnItemStart;

		public Action<RectTransform> OnItemArrived;

		public Action OnFirstArrived;

		public Action OnCompleted;
	}

	public static Sequence Play(IEnumerable<RectTransform> items, RectTransform target, GameObject linkTarget = null, Settings settings = null)
	{
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f7: Expected O, but got Unknown
		//IL_016a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0182: Unknown result type (might be due to invalid IL or missing references)
		//IL_018c: Expected O, but got Unknown
		//IL_0194: Unknown result type (might be due to invalid IL or missing references)
		//IL_019e: Expected O, but got Unknown
		//IL_01a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b0: Expected O, but got Unknown
		if (settings == null)
		{
			settings = new Settings();
		}
		if (items == null || (Object)(object)target == (Object)null)
		{
			return null;
		}
		Sequence val = TweenSettingsExtensions.SetTarget<Sequence>(DOTween.Sequence(), (object)linkTarget);
		RectTransform capturedTarget = target;
		Vector3 lastTargetPosition = ((Transform)target).position;
		bool flag = false;
		bool firstArrivedFired = false;
		int num = 0;
		foreach (RectTransform item in items)
		{
			if ((Object)(object)item == (Object)null)
			{
				continue;
			}
			int num2 = num++;
			float num3 = Mathf.Max(0f, settings.StartDelay);
			float num4 = Mathf.Max(0f, settings.ItemInterval);
			float num5 = Mathf.Max(0.01f, settings.Duration);
			float num6 = num3 + num4 * (float)num2;
			Vector3 startWorldPosition = ((Transform)item).position;
			RectTransform capturedItem = item;
			if (settings.HideUntilStart)
			{
				((Component)item).gameObject.SetActive(false);
			}
			TweenerCore<float, float, FloatOptions> val2 = TweenSettingsExtensions.OnKill<TweenerCore<float, float, FloatOptions>>(TweenSettingsExtensions.OnComplete<TweenerCore<float, float, FloatOptions>>(TweenSettingsExtensions.OnStart<TweenerCore<float, float, FloatOptions>>(TweenSettingsExtensions.SetTarget<TweenerCore<float, float, FloatOptions>>(TweenSettingsExtensions.SetEase<TweenerCore<float, float, FloatOptions>>(DOTween.To((DOGetter<float>)(() => 0f), (DOSetter<float>)delegate(float value)
			{
				//IL_003a: Unknown result type (might be due to invalid IL or missing references)
				//IL_0028: Unknown result type (might be due to invalid IL or missing references)
				//IL_003f: Unknown result type (might be due to invalid IL or missing references)
				//IL_0046: Unknown result type (might be due to invalid IL or missing references)
				//IL_0047: Unknown result type (might be due to invalid IL or missing references)
				//IL_008b: Unknown result type (might be due to invalid IL or missing references)
				//IL_0090: Unknown result type (might be due to invalid IL or missing references)
				//IL_0095: Unknown result type (might be due to invalid IL or missing references)
				//IL_0096: Unknown result type (might be due to invalid IL or missing references)
				//IL_009b: Unknown result type (might be due to invalid IL or missing references)
				//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
				//IL_006b: Unknown result type (might be due to invalid IL or missing references)
				//IL_0070: Unknown result type (might be due to invalid IL or missing references)
				//IL_0072: Unknown result type (might be due to invalid IL or missing references)
				if (!((Object)(object)capturedItem == (Object)null))
				{
					Vector3 val3 = (((Object)(object)capturedTarget != (Object)null) ? ((Transform)capturedTarget).position : lastTargetPosition);
					lastTargetPosition = val3;
					((Transform)capturedItem).position = (((Object)(object)settings.PathConfig != (Object)null) ? Vector2.op_Implicit(settings.PathConfig.Evaluate(value, Vector2.op_Implicit(startWorldPosition), Vector2.op_Implicit(val3))) : Vector3.LerpUnclamped(startWorldPosition, val3, value));
				}
			}, 1f, num5), settings.Ease), (object)linkTarget), (TweenCallback)delegate
			{
				if (settings.HideUntilStart)
				{
					((Component)capturedItem).gameObject.SetActive(true);
				}
				settings.OnItemStart?.Invoke(capturedItem);
			}), (TweenCallback)delegate
			{
				try
				{
					if (settings.DeactivateOnArrive && (Object)(object)capturedItem != (Object)null)
					{
						((Component)capturedItem).gameObject.SetActive(false);
					}
					settings.OnItemArrived?.Invoke(capturedItem);
					if (!firstArrivedFired)
					{
						firstArrivedFired = true;
						settings.OnFirstArrived?.Invoke();
					}
				}
				catch (Exception ex)
				{
					Log.LogError("RewardFlyUtility", "奖励飞抵回调异常: {0}", new object[1] { ex });
				}
			}), (TweenCallback)delegate
			{
				if (settings.DeactivateOnKill && (Object)(object)capturedItem != (Object)null)
				{
					((Component)capturedItem).gameObject.SetActive(false);
				}
			});
			TweenSettingsExtensions.Insert(val, num6, (Tween)(object)val2);
			flag = true;
		}
		if (!flag)
		{
			TweenExtensions.Kill((Tween)(object)val, false);
			return null;
		}
		TweenSettingsExtensions.OnComplete<Sequence>(val, (TweenCallback)delegate
		{
			try
			{
				settings.OnCompleted?.Invoke();
			}
			catch (Exception ex)
			{
				Log.LogError("RewardFlyUtility", "奖励飞行收尾回调异常: {0}", new object[1] { ex });
			}
		});
		if ((Object)(object)linkTarget != (Object)null)
		{
			TweenSettingsExtensions.SetLink<Sequence>(val, linkTarget, (LinkBehaviour)5);
		}
		return val;
	}
}
