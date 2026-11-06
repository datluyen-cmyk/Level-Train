using System;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using Game.UI;
using TMPro;
using UniLog;
using UnityEngine;
using XingYun.Extensions;
using XingYun.UI.Core;

namespace RotateRings.UI.LevelEnd;

public class UILevelWinScoreView : GameWidgetView
{
	private TMP_Text scoreText;

	private Tween scoreTween;

	private Vector3 initialScale;

	public override void Inject(UIBehaviour behaviour)
	{
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		base.Inject(behaviour);
		scoreText = behaviour.Get<TMP_Text>("TxtM_ScoreNum");
		initialScale = (((Object)(object)scoreText != (Object)null) ? scoreText.transform.localScale : Vector3.one);
	}

	public void SetScore(int score)
	{
		StopScoreTween();
		SetScoreText(score);
	}

	public void PlayScoreScroll(int fromScore, int toScore, float duration, Action onComplete)
	{
		//IL_00ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d4: Expected O, but got Unknown
		SetScore(fromScore);
		if (duration == 0f || fromScore == toScore || (Object)(object)scoreText == (Object)null || (Object)(object)((UIBaseView)this).Behaviour == (Object)null)
		{
			SetScoreText(toScore);
			onComplete?.Invoke();
			return;
		}
		float currentScore = fromScore;
		scoreTween = (Tween)(object)TweenSettingsExtensions.OnComplete<TweenerCore<float, float, FloatOptions>>(TweenSettingsExtensions.SetLink<TweenerCore<float, float, FloatOptions>>(TweenSettingsExtensions.SetTarget<TweenerCore<float, float, FloatOptions>>(TweenSettingsExtensions.SetEase<TweenerCore<float, float, FloatOptions>>(DOTween.To((DOGetter<float>)(() => currentScore), (DOSetter<float>)delegate(float value)
		{
			currentScore = value;
			SetScoreText(Mathf.RoundToInt(value));
		}, (float)toScore, duration), (Ease)10), (object)((Component)((UIBaseView)this).Behaviour).gameObject), ((Component)((UIBaseView)this).Behaviour).gameObject, (LinkBehaviour)5), (TweenCallback)delegate
		{
			try
			{
				scoreTween = null;
				SetScoreText(toScore);
				onComplete?.Invoke();
			}
			catch (Exception ex)
			{
				Log.LogError("UI", "[UILevelWinScoreView] 分数滚动完成回调异常: {0}", new object[1] { ex });
				scoreTween = null;
			}
		});
	}

	public override void Dispose()
	{
		StopPlayback();
		base.Dispose();
	}

	public float PlayEmphasis(AnimationClip clip)
	{
		if ((Object)(object)clip == (Object)null || !clip.legacy)
		{
			return 0f;
		}
		Animation val = (((Object)(object)scoreText != (Object)null) ? ((Component)scoreText).GetComponent<Animation>() : null);
		if ((Object)(object)val == (Object)null || !((Behaviour)val).isActiveAndEnabled)
		{
			return 0f;
		}
		if ((Object)(object)val.GetClip(((Object)clip).name) != (Object)(object)clip)
		{
			val.AddClip(clip, ((Object)clip).name);
		}
		AnimationState val2 = val[((Object)clip).name];
		if ((TrackedReference)(object)val2 == (TrackedReference)null)
		{
			return 0f;
		}
		val.Stop();
		val2.time = 0f;
		val2.speed = 1f;
		val2.wrapMode = (WrapMode)8;
		if (!val.Play(((Object)clip).name))
		{
			return 0f;
		}
		return val2.length;
	}

	public void StopPlayback()
	{
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		StopScoreTween();
		if (!((Object)(object)scoreText == (Object)null))
		{
			Animation component = ((Component)scoreText).GetComponent<Animation>();
			if (component != null)
			{
				component.Stop();
			}
			scoreText.transform.localScale = initialScale;
		}
	}

	private void SetScoreText(int score)
	{
		if ((Object)(object)scoreText != (Object)null)
		{
			TMPTextExtensions.SetLocalizedTextValue(scoreText, score.ToString());
		}
	}

	private void StopScoreTween()
	{
		Tween obj = scoreTween;
		if (obj != null)
		{
			TweenExtensions.Kill(obj, false);
		}
		scoreTween = null;
	}
}
