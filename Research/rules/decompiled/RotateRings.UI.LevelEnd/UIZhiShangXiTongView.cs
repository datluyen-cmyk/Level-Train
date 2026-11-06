using System;
using System.Runtime.CompilerServices;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using Game.UI;
using RotateRings.Common;
using RotateRings.GameWinIQ;
using RotateRings.Services;
using TMPro;
using UniLog;
using UnityEngine;
using UnityEngine.UI;
using XingYun.Extensions;
using XingYun.UI.Core;

namespace RotateRings.UI.LevelEnd;

public class UIZhiShangXiTongView : GameWidgetView
{
	[Serializable]
	[CompilerGenerated]
	private sealed class _003C_003Ec
	{
		public static readonly _003C_003Ec _003C_003E9 = new _003C_003Ec();

		public static TweenCallback _003C_003E9__22_0;

		internal void _003CScheduleIqIncreaseSound_003Eb__22_0()
		{
			try
			{
				AudioService.PlayUiSound(AudioKeys.LevelWinIQ.Value);
			}
			catch (Exception ex)
			{
				Log.LogError("UI", "[UIZhiShangXiTongView] IQ 上涨音效播放异常 {0}", new object[1] { ex });
			}
		}
	}

	private const string TextIqKey = "Text_IQ";

	private const string IqIncreaseClip = "ani_iq_increases_01";

	private const float IncreaseDuration = 1f;

	private const float IqBubbleParticleDelay = 11f / 12f;

	private Animation iqAnimation;

	private Tween growthTween;

	private Tween iqAudioDelayTween;

	private float preparedFromSlider;

	private float preparedToSlider;

	private int preparedFromScore;

	private int preparedToScore;

	private bool canGrow;

	private bool autoPlayGrowth = true;

	public TMP_Text TxtM_Text;

	public Slider Silder_IQ;

	public void SetAutoPlayGrowth(bool autoPlay)
	{
		autoPlayGrowth = autoPlay;
	}

	public void ApplySettlement(GameWinIQSettlementSnapshot snapshot)
	{
		EnsureSliderRange();
		KillTweens();
		int num = (snapshot.PlayGrowthAnimation ? snapshot.PreviousDisplayScore : snapshot.DisplayScore);
		int displayScore = snapshot.DisplayScore;
		GameWinIQRuntimeConfig gameWinIQRuntimeConfig = ResolveConfig();
		float value = ((gameWinIQRuntimeConfig != null) ? GameWinIQCalculator.MapDisplayScoreToSlider(gameWinIQRuntimeConfig, num) : ((float)num));
		float value2 = ((gameWinIQRuntimeConfig != null) ? GameWinIQCalculator.MapDisplayScoreToSlider(gameWinIQRuntimeConfig, displayScore) : ((float)displayScore));
		if ((Object)(object)Silder_IQ != (Object)null)
		{
			Silder_IQ.value = value;
		}
		SetIqText(num);
		bool flag = snapshot.PlayGrowthAnimation && displayScore > num;
		preparedFromSlider = value;
		preparedToSlider = value2;
		preparedFromScore = num;
		preparedToScore = displayScore;
		canGrow = flag;
		if (!flag)
		{
			if ((Object)(object)Silder_IQ != (Object)null)
			{
				Silder_IQ.value = value2;
			}
			SetIqText(displayScore);
		}
		else if (autoPlayGrowth)
		{
			PlayPreparedGrowth(1f, null);
		}
	}

	public void PlayPreparedGrowth(float duration, Action onComplete)
	{
		//IL_00e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00eb: Expected O, but got Unknown
		if ((Object)(object)((UIBaseView)this).Behaviour == (Object)null || !canGrow)
		{
			onComplete?.Invoke();
			return;
		}
		Tween obj = growthTween;
		if (obj != null)
		{
			TweenExtensions.Kill(obj, false);
		}
		PlayIncreaseAnimation();
		ScheduleIqIncreaseSound();
		if (duration == 0f)
		{
			Apply(1f);
			canGrow = false;
			onComplete?.Invoke();
			return;
		}
		float current = 0f;
		growthTween = (Tween)(object)TweenSettingsExtensions.OnComplete<TweenerCore<float, float, FloatOptions>>(TweenSettingsExtensions.SetLink<TweenerCore<float, float, FloatOptions>>(TweenSettingsExtensions.SetTarget<TweenerCore<float, float, FloatOptions>>(TweenSettingsExtensions.SetEase<TweenerCore<float, float, FloatOptions>>(DOTween.To((DOGetter<float>)(() => current), (DOSetter<float>)delegate(float value)
		{
			current = value;
			Apply(value);
		}, 1f, duration), (Ease)1), (object)((Component)((UIBaseView)this).Behaviour).gameObject), ((Component)((UIBaseView)this).Behaviour).gameObject, (LinkBehaviour)5), (TweenCallback)delegate
		{
			growthTween = null;
			canGrow = false;
			Apply(1f);
			onComplete?.Invoke();
		});
		void Apply(float progress)
		{
			if ((Object)(object)Silder_IQ != (Object)null)
			{
				Silder_IQ.value = Mathf.Lerp(preparedFromSlider, preparedToSlider, progress);
			}
			SetIqText(Mathf.RoundToInt(Mathf.Lerp((float)preparedFromScore, (float)preparedToScore, progress)));
		}
	}

	public float PlayAppearance(AnimationClip clip)
	{
		if ((Object)(object)clip == (Object)null || (Object)(object)((UIBaseView)this).Behaviour == (Object)null)
		{
			return 0f;
		}
		if (iqAnimation == null)
		{
			iqAnimation = ((Component)((UIBaseView)this).Behaviour).GetComponent<Animation>();
		}
		if ((Object)(object)iqAnimation == (Object)null)
		{
			throw new InvalidOperationException("IQ挂件缺少Animation组件");
		}
		if ((Object)(object)iqAnimation.GetClip(((Object)clip).name) != (Object)(object)clip)
		{
			iqAnimation.AddClip(clip, ((Object)clip).name);
		}
		iqAnimation.Stop();
		AnimationState obj = iqAnimation[((Object)clip).name];
		obj.time = 0f;
		obj.speed = 1f;
		obj.wrapMode = (WrapMode)8;
		iqAnimation.Play(((Object)clip).name);
		return obj.length;
	}

	public void StopPlayback()
	{
		KillTweens();
		Animation obj = iqAnimation;
		if (obj != null)
		{
			obj.Stop();
		}
	}

	public void SetRootVisible(bool visible)
	{
		if ((Object)(object)((UIBaseView)this).Behaviour != (Object)null)
		{
			((Component)((UIBaseView)this).Behaviour).gameObject.SetActive(visible);
		}
	}

	public override void Dispose()
	{
		KillTweens();
		if ((Object)(object)((UIBaseView)this).Behaviour != (Object)null)
		{
			DOTween.Kill((object)((Component)((UIBaseView)this).Behaviour).gameObject, false);
		}
		base.Dispose();
	}

	private void EnsureSliderRange()
	{
		if (!((Object)(object)Silder_IQ == (Object)null))
		{
			GameWinIQRuntimeConfig gameWinIQRuntimeConfig = ResolveConfig();
			Silder_IQ.minValue = 0f;
			Silder_IQ.maxValue = gameWinIQRuntimeConfig?.GetSliderTrackMax() ?? 3.3f;
		}
	}

	private static GameWinIQRuntimeConfig ResolveConfig()
	{
		if (!GameWinIQConfigProvider.TryGetConfig(out var config))
		{
			return null;
		}
		return config;
	}

	private void ScheduleIqIncreaseSound()
	{
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Expected O, but got Unknown
		Tween obj = iqAudioDelayTween;
		if (obj != null)
		{
			TweenExtensions.Kill(obj, false);
		}
		iqAudioDelayTween = null;
		if ((Object)(object)((UIBaseView)this).Behaviour == (Object)null)
		{
			return;
		}
		object obj2 = _003C_003Ec._003C_003E9__22_0;
		if (obj2 == null)
		{
			TweenCallback val = delegate
			{
				try
				{
					AudioService.PlayUiSound(AudioKeys.LevelWinIQ.Value);
				}
				catch (Exception ex)
				{
					Log.LogError("UI", "[UIZhiShangXiTongView] IQ 上涨音效播放异常 {0}", new object[1] { ex });
				}
			};
			_003C_003Ec._003C_003E9__22_0 = val;
			obj2 = (object)val;
		}
		iqAudioDelayTween = TweenSettingsExtensions.SetLink<Tween>(TweenSettingsExtensions.SetTarget<Tween>(DOVirtual.DelayedCall(11f / 12f, (TweenCallback)obj2, true), (object)((Component)((UIBaseView)this).Behaviour).gameObject), ((Component)((UIBaseView)this).Behaviour).gameObject, (LinkBehaviour)5);
	}

	private void PlayIncreaseAnimation()
	{
		if (!((Object)(object)((UIBaseView)this).Behaviour == (Object)null))
		{
			if (iqAnimation == null)
			{
				iqAnimation = ((Component)((UIBaseView)this).Behaviour).GetComponent<Animation>();
			}
			if (!((Object)(object)iqAnimation == (Object)null) && !((TrackedReference)(object)iqAnimation["ani_iq_increases_01"] == (TrackedReference)null))
			{
				AnimationState obj = iqAnimation["ani_iq_increases_01"];
				iqAnimation.Stop("ani_iq_increases_01");
				obj.time = 0f;
				iqAnimation.Sample();
				obj.wrapMode = (WrapMode)8;
				iqAnimation.Play("ani_iq_increases_01");
			}
		}
	}

	private void SetIqText(int score)
	{
		if (!((Object)(object)TxtM_Text == (Object)null))
		{
			TMPTextExtensions.SetLocalizedText(TxtM_Text, "Text_IQ", new object[1] { score });
		}
	}

	private void KillTweens()
	{
		Tween obj = iqAudioDelayTween;
		if (obj != null)
		{
			TweenExtensions.Kill(obj, false);
		}
		iqAudioDelayTween = null;
		Tween obj2 = growthTween;
		if (obj2 != null)
		{
			TweenExtensions.Kill(obj2, false);
		}
		growthTween = null;
	}

	public override void Inject(UIBehaviour behaviour)
	{
		base.Inject(behaviour);
		TxtM_Text = behaviour.Get<TMP_Text>("TxtM_Text");
		Silder_IQ = behaviour.Get<Slider>("Silder_IQ");
	}
}
