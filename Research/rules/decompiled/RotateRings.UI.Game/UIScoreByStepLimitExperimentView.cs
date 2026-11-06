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

namespace RotateRings.UI.Game;

public class UIScoreByStepLimitExperimentView : GameWidgetView
{
	private const string ScoreChangeAnim = "anim_score_start";

	private const float ScoreScrollDuration = 0.5f;

	private const string ScoreTitleKey = "LevelComplete_Score";

	private int displayedScore;

	private Tween scoreTween;

	public TMP_Text TextTitle;

	public TMP_Text TextScore;

	public Animation ScoreChangeAni;

	protected override void OnWidgetFirstLoaded()
	{
		base.OnWidgetFirstLoaded();
		RefreshTitle();
	}

	public void RefreshTitle()
	{
		if ((Object)(object)TextTitle != (Object)null)
		{
			TMPTextExtensions.SetLocalizedText(TextTitle, "LevelComplete_Score", Array.Empty<object>());
		}
	}

	public void SetScore(int score)
	{
		StopScoreTween();
		displayedScore = score;
		SetScoreText(score);
	}

	public void AddScore(int score)
	{
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b8: Expected O, but got Unknown
		if (score == displayedScore)
		{
			return;
		}
		StopScoreTween();
		int num = displayedScore;
		int targetScore = score;
		float currentScore = num;
		scoreTween = (Tween)(object)TweenSettingsExtensions.OnComplete<TweenerCore<float, float, FloatOptions>>(TweenSettingsExtensions.SetLink<TweenerCore<float, float, FloatOptions>>(TweenSettingsExtensions.SetTarget<TweenerCore<float, float, FloatOptions>>(TweenSettingsExtensions.SetEase<TweenerCore<float, float, FloatOptions>>(DOTween.To((DOGetter<float>)(() => currentScore), (DOSetter<float>)delegate(float value)
		{
			currentScore = value;
			displayedScore = Mathf.RoundToInt(value);
			SetScoreText(displayedScore);
		}, (float)targetScore, 0.5f), (Ease)10), (object)(((Object)(object)((UIBaseView)this).Behaviour != (Object)null) ? ((Component)((UIBaseView)this).Behaviour).gameObject : null)), ((Object)(object)((UIBaseView)this).Behaviour != (Object)null) ? ((Component)((UIBaseView)this).Behaviour).gameObject : null, (LinkBehaviour)5), (TweenCallback)delegate
		{
			try
			{
				displayedScore = targetScore;
				SetScoreText(targetScore);
				scoreTween = null;
			}
			catch (Exception ex)
			{
				Log.LogError("UI", "[UIScoreByStepLimitExperimentView] 分数滚动完成回调异常: {0}", new object[1] { ex });
				scoreTween = null;
			}
		});
		PlayScoreAnimation();
	}

	private void SetScoreText(int score)
	{
		if ((Object)(object)TextScore != (Object)null)
		{
			TMPTextExtensions.SetLocalizedTextValue(TextScore, score.ToString());
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

	private void PlayScoreAnimation()
	{
		if (!((Object)(object)ScoreChangeAni == (Object)null) && !((Object)(object)ScoreChangeAni.GetClip("anim_score_start") == (Object)null))
		{
			ScoreChangeAni.Stop();
			ScoreChangeAni.Rewind("anim_score_start");
			ScoreChangeAni.Play("anim_score_start", (PlayMode)4);
		}
	}

	public override void Dispose()
	{
		StopScoreTween();
		base.Dispose();
	}

	public override void Inject(UIBehaviour behaviour)
	{
		base.Inject(behaviour);
		TextTitle = behaviour.Get<TMP_Text>("TextTitle");
		TextScore = behaviour.Get<TMP_Text>("TextScore");
		ScoreChangeAni = behaviour.Get<Animation>("ScoreChangeAni");
	}
}
