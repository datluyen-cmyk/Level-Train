using System;
using System.Collections.Generic;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using Game.UI;
using RotateRings.GamePlay.Events;
using RotateRings.GamePlay.Save;
using RotateRings.Model;
using RotateRings.Theme;
using TMPro;
using UniLog;
using UnityEngine;
using UnityEngine.UI;
using XingYun.Extensions;
using XingYun.UI.Core;

namespace RotateRings.UI.Game;

public class UILevelProgressView : GameWidgetView
{
	private const float ScoreScrollDuration = 0.5f;

	private const string ScoreChangeAnim = "anim_score_start";

	private const float ProgressTweenDuration = 0.3f;

	private int totalGameplayObjectCount;

	private int remainingGameplayObjectCount;

	private int displayedScore;

	private Tween scoreTween;

	private Tween progressTween;

	public GameObject BrightNode;

	public Slider BrightSlider;

	public TMP_Text BrightScoreTxt;

	public GameObject DarkNode;

	public Slider DarkSlider;

	public TMP_Text DarkScoreTxt;

	public Animation BrightAnim;

	public Animation DarkAnim;

	protected override void OnWidgetFirstLoaded()
	{
		base.OnWidgetFirstLoaded();
		GameApp.Event.Listen<ThemeChangedEvent>((Action<ThemeChangedEvent>)OnThemeChanged);
		GameApp.Event.Listen<LevelLoadingStartedEvent>((Action<LevelLoadingStartedEvent>)OnLevelLoadingStarted);
		GameApp.Event.Listen<LevelEnterCompletedEvent>((Action<LevelEnterCompletedEvent>)OnLevelEnterCompleted);
		GameApp.Event.Listen<RingSolvedEvent>((Action<RingSolvedEvent>)OnRingSolved);
		GameApp.Event.Listen<ScoreAdded>((Action<ScoreAdded>)OnScoreAdded);
		GameApp.Event.Listen<ScoreReset>((Action<ScoreReset>)OnScoreReset);
		ApplyTheme(ThemeService.CurrentThemeId);
		LevelModelProxy levelModelProxy = GameApp.Proxy.Get<LevelModelProxy>();
		SetScore(levelModelProxy?.GetCurrentScore() ?? 0);
		CaptureLevelProgress(levelModelProxy?.Level ?? 0, animate: false);
	}

	private void ApplyTheme(ThemeId themeId)
	{
		bool flag = themeId == ThemeId.Night;
		if ((Object)(object)BrightNode != (Object)null)
		{
			BrightNode.SetActive(!flag);
		}
		if ((Object)(object)DarkNode != (Object)null)
		{
			DarkNode.SetActive(flag);
		}
	}

	private void CaptureLevelProgress(int level, bool animate)
	{
		if (TryCaptureProgressFromSave(level) || TryCaptureProgressFromObjects())
		{
			RefreshProgress(animate);
		}
	}

	private bool TryCaptureProgressFromSave(int level)
	{
		if (level <= 0)
		{
			return false;
		}
		RingLevelSaveProxy ringLevelSaveProxy = GameApp.Proxy.Get<RingLevelSaveProxy>();
		if (ringLevelSaveProxy == null || !ringLevelSaveProxy.TryGetSave(level, out var save, validateContentHash: false) || save.Parts == null || save.Parts.Length == 0)
		{
			return false;
		}
		int num = 0;
		int num2 = 0;
		RotateRingPartSave[] parts = save.Parts;
		foreach (RotateRingPartSave rotateRingPartSave in parts)
		{
			if (rotateRingPartSave != null)
			{
				num++;
				if (!rotateRingPartSave.IsSolved)
				{
					num2++;
				}
			}
		}
		if (num <= 0)
		{
			return false;
		}
		totalGameplayObjectCount = num;
		remainingGameplayObjectCount = num2;
		return true;
	}

	private bool TryCaptureProgressFromObjects()
	{
		IReadOnlyList<IGameplayObject> readOnlyList = SingletonComponent<GameplayManager>.Instance?.GetGameplayObjects();
		if (readOnlyList == null)
		{
			return false;
		}
		int num = 0;
		int num2 = 0;
		for (int i = 0; i < readOnlyList.Count; i++)
		{
			IGameplayObject gameplayObject = readOnlyList[i];
			if (IsProgressPart(gameplayObject))
			{
				num++;
				if (!gameplayObject.IsSolved)
				{
					num2++;
				}
			}
		}
		if (num <= 0)
		{
			return false;
		}
		totalGameplayObjectCount = num;
		remainingGameplayObjectCount = num2;
		return true;
	}

	private static bool IsProgressPart(IGameplayObject gameplayObject)
	{
		if (gameplayObject != null)
		{
			if (!(gameplayObject is RingObject) && !(gameplayObject is PullObject))
			{
				return gameplayObject is FixedObject;
			}
			return true;
		}
		return false;
	}

	private void RefreshProgress(bool animate)
	{
		//IL_00cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d6: Expected O, but got Unknown
		float num = ((totalGameplayObjectCount > 0) ? (1f - Mathf.Clamp01((float)remainingGameplayObjectCount / (float)totalGameplayObjectCount)) : 0f);
		StopProgressTween();
		if (!animate)
		{
			SetProgress(num);
			return;
		}
		float currentProgress = GetCurrentProgress();
		if (Mathf.Approximately(currentProgress, num) || (Object)(object)((UIBaseView)this).Behaviour == (Object)null)
		{
			SetProgress(num);
			return;
		}
		progressTween = (Tween)(object)TweenSettingsExtensions.OnComplete<TweenerCore<float, float, FloatOptions>>(TweenSettingsExtensions.SetLink<TweenerCore<float, float, FloatOptions>>(TweenSettingsExtensions.SetTarget<TweenerCore<float, float, FloatOptions>>(TweenSettingsExtensions.SetEase<TweenerCore<float, float, FloatOptions>>(DOTween.To((DOGetter<float>)(() => currentProgress), (DOSetter<float>)SetProgress, num, 0.3f), (Ease)9), (object)((Component)((UIBaseView)this).Behaviour).gameObject), ((Component)((UIBaseView)this).Behaviour).gameObject, (LinkBehaviour)5), (TweenCallback)delegate
		{
			progressTween = null;
		});
	}

	private float GetCurrentProgress()
	{
		bool flag = ThemeService.CurrentThemeId == ThemeId.Night;
		if (flag && (Object)(object)DarkSlider != (Object)null)
		{
			return DarkSlider.value;
		}
		if (!flag && (Object)(object)BrightSlider != (Object)null)
		{
			return BrightSlider.value;
		}
		if ((Object)(object)BrightSlider != (Object)null)
		{
			return BrightSlider.value;
		}
		if (!((Object)(object)DarkSlider != (Object)null))
		{
			return 0f;
		}
		return DarkSlider.value;
	}

	private void SetProgress(float progress)
	{
		if ((Object)(object)BrightSlider != (Object)null)
		{
			BrightSlider.SetValueWithoutNotify(progress);
		}
		if ((Object)(object)DarkSlider != (Object)null)
		{
			DarkSlider.SetValueWithoutNotify(progress);
		}
	}

	private void StopProgressTween()
	{
		Tween obj = progressTween;
		if (obj != null)
		{
			TweenExtensions.Kill(obj, false);
		}
		progressTween = null;
	}

	private void SetScore(int score)
	{
		StopScoreTween();
		displayedScore = score;
		SetScoreText(score);
	}

	private void AddScore(int score)
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
				Log.LogError("UI", "[UILevelProgressView] 积分滚动完成回调异常: {0}", new object[1] { ex });
				scoreTween = null;
			}
		});
		PlayScoreAnimation();
	}

	private void PlayScoreAnimation()
	{
		Animation val = ((ThemeService.CurrentThemeId == ThemeId.Night) ? DarkAnim : BrightAnim);
		if (!((Object)(object)val == (Object)null) && !((Object)(object)val.GetClip("anim_score_start") == (Object)null))
		{
			val.Stop();
			val.Rewind("anim_score_start");
			val.Play("anim_score_start", (PlayMode)4);
		}
	}

	private void SetScoreText(int score)
	{
		string text = score.ToString();
		if ((Object)(object)BrightScoreTxt != (Object)null)
		{
			TMPTextExtensions.SetLocalizedTextValue(BrightScoreTxt, text);
		}
		if ((Object)(object)DarkScoreTxt != (Object)null)
		{
			TMPTextExtensions.SetLocalizedTextValue(DarkScoreTxt, text);
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

	private void OnThemeChanged(ThemeChangedEvent eventData)
	{
		ApplyTheme(eventData.CurrentThemeId);
	}

	private void OnLevelLoadingStarted(LevelLoadingStartedEvent eventData)
	{
		if (!TryCaptureProgressFromSave(eventData.Level))
		{
			totalGameplayObjectCount = 0;
			remainingGameplayObjectCount = 0;
		}
		RefreshProgress(animate: false);
	}

	private void OnLevelEnterCompleted(LevelEnterCompletedEvent eventData)
	{
		CaptureLevelProgress(eventData.Level, animate: false);
	}

	private void OnRingSolved(RingSolvedEvent eventData)
	{
		if (totalGameplayObjectCount > 0 && IsProgressPart(eventData.GameplayObject))
		{
			remainingGameplayObjectCount = Mathf.Max(0, remainingGameplayObjectCount - 1);
			RefreshProgress(animate: true);
		}
	}

	private void OnScoreAdded(ScoreAdded eventData)
	{
		AddScore(eventData.Score);
	}

	private void OnScoreReset(ScoreReset eventData)
	{
		SetScore(eventData.Score);
	}

	public override void Dispose()
	{
		StopProgressTween();
		StopScoreTween();
		GameApp.Event.Unlisten<ThemeChangedEvent>((Action<ThemeChangedEvent>)OnThemeChanged);
		GameApp.Event.Unlisten<LevelLoadingStartedEvent>((Action<LevelLoadingStartedEvent>)OnLevelLoadingStarted);
		GameApp.Event.Unlisten<LevelEnterCompletedEvent>((Action<LevelEnterCompletedEvent>)OnLevelEnterCompleted);
		GameApp.Event.Unlisten<RingSolvedEvent>((Action<RingSolvedEvent>)OnRingSolved);
		GameApp.Event.Unlisten<ScoreAdded>((Action<ScoreAdded>)OnScoreAdded);
		GameApp.Event.Unlisten<ScoreReset>((Action<ScoreReset>)OnScoreReset);
		base.Dispose();
	}

	public override void Inject(UIBehaviour behaviour)
	{
		base.Inject(behaviour);
		BrightNode = behaviour.Get<GameObject>("BrightNode");
		BrightSlider = behaviour.Get<Slider>("BrightSlider");
		BrightScoreTxt = behaviour.Get<TMP_Text>("BrightScoreTxt");
		DarkNode = behaviour.Get<GameObject>("DarkNode");
		DarkSlider = behaviour.Get<Slider>("DarkSlider");
		DarkScoreTxt = behaviour.Get<TMP_Text>("DarkScoreTxt");
		BrightAnim = behaviour.Get<Animation>("BrightAnim");
		DarkAnim = behaviour.Get<Animation>("DarkAnim");
	}
}
