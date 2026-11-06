using System;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using Game.UI;
using RotateRings.UI.Game;
using TMPro;
using UniLog;
using UnityEngine;
using UnityEngine.UI;
using XingYun.Experiment;
using XingYun.Extensions;
using XingYun.UI.Core;

namespace RotateRings.UI.Score;

public class UIScoreView : GameWidgetView
{
	private const string ScoreChangeAnim = "anim_score_start";

	private const float ScoreScrollDuration = 0.5f;

	private const string LevelProgressWidgetLocation = "UILevelProgress";

	private int displayedScore;

	private Tween scoreTween;

	private WidgetHandle levelProgressHandle;

	public UIBehaviour RootNode;

	public TMP_Text TxtScore;

	public Image ScoreBg;

	public Animation ScoreAni;

	public GameObject NormalScoreNode;

	protected override void OnWidgetFirstLoaded()
	{
		base.OnWidgetFirstLoaded();
		InitializeScoreDisplay();
	}

	private void InitializeScoreDisplay()
	{
		bool flag = IsLevelProgressEnabled();
		if ((Object)(object)NormalScoreNode != (Object)null)
		{
			NormalScoreNode.SetActive(!flag);
		}
		if (flag && !((Object)(object)RootNode == (Object)null) && levelProgressHandle == null)
		{
			levelProgressHandle = AttachWidget<UILevelProgressView>("UILevelProgress", ((Component)RootNode).transform);
			if (levelProgressHandle == null)
			{
				Log.LogError("UI", "[UIScoreView] 关卡进度挂件加载失败: {0}", new object[1] { "UILevelProgress" });
			}
		}
	}

	private static bool IsLevelProgressEnabled()
	{
		LevelUIFunction levelUIFunction = FunctionDataCenter.Get<LevelUIFunction>();
		if (levelUIFunction == null)
		{
			return false;
		}
		return levelUIFunction.ShowScoreProgress == 1;
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
				Log.LogError("UI", "[UIScoreView] 分数滚动完成回调异常: {0}", new object[1] { ex });
				scoreTween = null;
			}
		});
		PlayScoreAnimation();
	}

	private void SetScoreText(int score)
	{
		if ((Object)(object)TxtScore != (Object)null)
		{
			TMPTextExtensions.SetLocalizedTextValue(TxtScore, score.ToString());
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

	public void PlayScoreAnimation()
	{
		PlayAnimation("anim_score_start");
	}

	private void PlayAnimation(string animName)
	{
		if (!((Object)(object)ScoreAni == (Object)null) && !((Object)(object)ScoreAni.GetClip(animName) == (Object)null))
		{
			ScoreAni.Stop();
			ScoreAni.Rewind(animName);
			ScoreAni.Play(animName, (PlayMode)4);
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
		RootNode = behaviour.Get<UIBehaviour>("RootNode");
		TxtScore = behaviour.Get<TMP_Text>("TxtScore");
		ScoreBg = behaviour.Get<Image>("ScoreBg");
		ScoreAni = behaviour.Get<Animation>("ScoreAni");
		NormalScoreNode = behaviour.Get<GameObject>("NormalScoreNode");
	}
}
