using System;
using System.Collections.Generic;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using Rotaterings.Rank;
using Rotaterings.SO;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using XingYun.Extensions;

namespace Rotaterings.UI.RankList;

public sealed class RankStreakDisplayPresenter
{
	private const float TierNormalFontSize = 50f;

	private const float TierHighlightFontSize = 60f;

	private readonly UIRankListView view;

	private readonly Action<int> onCollectionShow;

	private readonly List<int> timerIds = new List<int>();

	private readonly List<Tween> tweens = new List<Tween>();

	private readonly RectTransform[] tierBgs;

	private readonly RectTransform[] tierGreenBgs;

	private readonly TMP_Text[] tierTexts;

	private Tween streakTween;

	private Color tierHighlightColor = new Color(0.09019608f, 31f / 85f, 0.11372549f, 1f);

	private Color transitionHighlightColor;

	private Color transitionNormalColor;

	public RankStreakDisplayPresenter(UIRankListView view, Action<int> onCollectionShow)
	{
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		this.view = view;
		this.onCollectionShow = onCollectionShow;
		tierBgs = (RectTransform[])(object)new RectTransform[5] { view.bg1, view.bg2, view.bg3, view.bg4, view.bg5 };
		tierGreenBgs = (RectTransform[])(object)new RectTransform[5] { view.greenbg1, view.greenbg2, view.greenbg3, view.greenbg4, view.greenbg5 };
		tierTexts = (TMP_Text[])(object)new TMP_Text[5] { view.txtX1, view.txtX2, view.txtX3, view.txtX4, view.txtX5 };
	}

	public void RefreshTexts()
	{
		for (int i = 0; i < 5; i++)
		{
			if (i < tierTexts.Length && !((Object)(object)tierTexts[i] == (Object)null))
			{
				string value = string.Format("{0}{1}", GameApp.Lang.GetText("Word_X", Array.Empty<object>()), RankScoreService.GetTierByIndex(i + 1));
				SetTierText(tierTexts[i], value);
			}
		}
	}

	public void Play(RankModelProxy rankProxy, bool skipCollectionPresentation = false)
	{
		RefreshTexts();
		int tierIndex = RankScoreService.GetTierIndex((rankProxy?.Data?.MultiplierTier).GetValueOrDefault());
		int tierIndex2 = RankScoreService.GetTierIndex((rankProxy?.Data?.LastMultiplierTier).GetValueOrDefault());
		SetAllUnselected();
		if ((Object)(object)view.moveArrow != (Object)null)
		{
			((Component)view.moveArrow).gameObject.SetActive(false);
		}
		if (tierIndex <= 0)
		{
			if (skipCollectionPresentation)
			{
				SetSelected(0);
			}
			return;
		}
		int displayIndex = Mathf.Clamp(tierIndex - 1, 0, 4);
		bool hasArrowMove = tierIndex > 1 && tierIndex != tierIndex2;
		if (hasArrowMove)
		{
			SetSelected(displayIndex - 1);
		}
		else
		{
			SetSelected(displayIndex);
		}
		RankListConfig instance = RankListConfig.Instance;
		float num = (((Object)(object)instance != (Object)null) ? instance.initSelectedDuration : 0.2f);
		float num2 = (((Object)(object)instance != (Object)null) ? instance.noArrowMoveDelay : 0.2f);
		float num3 = (hasArrowMove ? num : num2);
		timerIds.Add(GameApp.Timer.Delay(num3, (Action)delegate
		{
			OnInitialDisplayCompleted(hasArrowMove, displayIndex, skipCollectionPresentation);
		}));
	}

	public void Cleanup()
	{
		for (int i = 0; i < timerIds.Count; i++)
		{
			GameApp.Timer.Kill(timerIds[i]);
		}
		timerIds.Clear();
		Tween obj = streakTween;
		if (obj != null)
		{
			TweenExtensions.Kill(obj, false);
		}
		streakTween = null;
		for (int num = tweens.Count - 1; num >= 0; num--)
		{
			Tween obj2 = tweens[num];
			tweens.RemoveAt(num);
			if (obj2 != null)
			{
				TweenExtensions.Kill(obj2, false);
			}
		}
		if ((Object)(object)view.moveArrow != (Object)null)
		{
			((Component)view.moveArrow).gameObject.SetActive(false);
		}
	}

	private void TrackTween(Tween tween)
	{
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Expected O, but got Unknown
		if (tween != null)
		{
			tweens.Add(tween);
			TweenSettingsExtensions.OnKill<Tween>(tween, (TweenCallback)delegate
			{
				tweens.Remove(tween);
			});
		}
	}

	private void OnInitialDisplayCompleted(bool hasArrowMove, int displayIndex, bool skipCollectionPresentation)
	{
		if (hasArrowMove)
		{
			AnimateStreak(displayIndex - 1, displayIndex, skipCollectionPresentation);
		}
		else if (!skipCollectionPresentation)
		{
			onCollectionShow?.Invoke(displayIndex);
		}
	}

	private static void SetTierText(TMP_Text label, string value)
	{
		if (!((Object)(object)label == (Object)null))
		{
			TMPTextExtensions.SetLocalizedTextValue(label, value);
			Transform val = label.transform.Find("green");
			TMP_Text val2 = (((Object)(object)val != (Object)null) ? ((Component)val).GetComponent<TMP_Text>() : null);
			if ((Object)(object)val2 != (Object)null)
			{
				TMPTextExtensions.SetLocalizedTextValue(val2, value);
			}
		}
	}

	private void AnimateStreak(int fromIndex, int toIndex, bool skipCollectionPresentation)
	{
		//IL_00ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fe: Unknown result type (might be due to invalid IL or missing references)
		//IL_0103: Unknown result type (might be due to invalid IL or missing references)
		//IL_0109: Unknown result type (might be due to invalid IL or missing references)
		//IL_010e: Unknown result type (might be due to invalid IL or missing references)
		//IL_011b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0125: Expected O, but got Unknown
		//IL_0135: Unknown result type (might be due to invalid IL or missing references)
		//IL_015a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0164: Expected O, but got Unknown
		if (!((Object)(object)view.moveArrow == (Object)null) && fromIndex >= 0 && toIndex >= 0 && fromIndex < tierBgs.Length && toIndex < tierBgs.Length && !((Object)(object)tierBgs[fromIndex] == (Object)null) && !((Object)(object)tierBgs[toIndex] == (Object)null))
		{
			RankListConfig instance = RankListConfig.Instance;
			float moveTime = (((Object)(object)instance != (Object)null) ? instance.arrowMoveDuration : 0.2f);
			RectTransform moveArrow = view.moveArrow;
			((Transform)moveArrow).position = ((Transform)tierBgs[fromIndex]).position;
			((Component)moveArrow).gameObject.SetActive(true);
			Tween obj = streakTween;
			if (obj != null)
			{
				TweenExtensions.Kill(obj, false);
			}
			Sequence val = DOTween.Sequence();
			transitionHighlightColor = tierHighlightColor;
			transitionNormalColor = Color.white;
			TweenSettingsExtensions.AppendCallback(val, (TweenCallback)delegate
			{
				SetupTierTransition(fromIndex, toIndex, moveTime);
			});
			TweenSettingsExtensions.Append(val, (Tween)(object)TweenSettingsExtensions.SetEase<TweenerCore<Vector3, Vector3, VectorOptions>>(ShortcutExtensions.DOMove((Transform)(object)moveArrow, ((Transform)tierBgs[toIndex]).position, moveTime, false), (Ease)1));
			TweenSettingsExtensions.AppendCallback(val, (TweenCallback)delegate
			{
				CompleteTierTransition(fromIndex, toIndex, skipCollectionPresentation);
			});
			streakTween = (Tween)(object)val;
			TrackTween((Tween)(object)val);
			TweenExtensions.Play<Sequence>(val);
		}
	}

	private void SetupTierTransition(int fromIndex, int toIndex, float moveTime)
	{
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_011d: Unknown result type (might be due to invalid IL or missing references)
		//IL_01dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_0234: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)tierGreenBgs[fromIndex] != (Object)null)
		{
			((Component)tierGreenBgs[fromIndex]).gameObject.SetActive(false);
		}
		TMP_Text obj = tierTexts[fromIndex];
		Transform val = ((obj != null) ? obj.transform.Find("green") : null);
		if ((Object)(object)val != (Object)null)
		{
			TMP_Text oldText = ((Component)val).GetComponent<TMP_Text>();
			CanvasGroup component = ((Component)val).GetComponent<CanvasGroup>();
			if ((Object)(object)oldText != (Object)null)
			{
				tierHighlightColor = ((Graphic)oldText).color;
				transitionHighlightColor = ((Graphic)oldText).color;
				transitionNormalColor = (((Object)(object)tierTexts[fromIndex] != (Object)null) ? ((Graphic)tierTexts[fromIndex]).color : Color.white);
				oldText.fontSize = 60f;
				TrackTween((Tween)(object)TweenSettingsExtensions.SetEase<TweenerCore<float, float, FloatOptions>>(DOTween.To((DOGetter<float>)(() => oldText.fontSize), (DOSetter<float>)delegate(float value)
				{
					oldText.fontSize = value;
				}, 50f, moveTime), (Ease)1));
				TrackTween((Tween)(object)TweenSettingsExtensions.SetEase<TweenerCore<Color, Color, ColorOptions>>(DOTween.To((DOGetter<Color>)(() => ((Graphic)oldText).color), (DOSetter<Color>)delegate(Color value)
				{
					//IL_0006: Unknown result type (might be due to invalid IL or missing references)
					((Graphic)oldText).color = value;
				}, transitionNormalColor, moveTime), (Ease)1));
			}
			if ((Object)(object)component != (Object)null)
			{
				component.alpha = 1f;
				TrackTween((Tween)(object)TweenSettingsExtensions.SetEase<TweenerCore<float, float, FloatOptions>>(DOTweenModuleUI.DOFade(component, 0f, moveTime), (Ease)1));
			}
		}
		TMP_Text obj2 = tierTexts[toIndex];
		Transform val2 = ((obj2 != null) ? obj2.transform.Find("green") : null);
		if ((Object)(object)val2 != (Object)null)
		{
			TMP_Text newText = ((Component)val2).GetComponent<TMP_Text>();
			CanvasGroup component2 = ((Component)val2).GetComponent<CanvasGroup>();
			((Component)val2).gameObject.SetActive(true);
			if ((Object)(object)newText != (Object)null)
			{
				newText.fontSize = 50f;
				((Graphic)newText).color = transitionNormalColor;
				TrackTween((Tween)(object)TweenSettingsExtensions.SetEase<TweenerCore<float, float, FloatOptions>>(DOTween.To((DOGetter<float>)(() => newText.fontSize), (DOSetter<float>)delegate(float value)
				{
					newText.fontSize = value;
				}, 60f, moveTime), (Ease)1));
				TrackTween((Tween)(object)TweenSettingsExtensions.SetEase<TweenerCore<Color, Color, ColorOptions>>(DOTween.To((DOGetter<Color>)(() => ((Graphic)newText).color), (DOSetter<Color>)delegate(Color value)
				{
					//IL_0006: Unknown result type (might be due to invalid IL or missing references)
					((Graphic)newText).color = value;
				}, transitionHighlightColor, moveTime), (Ease)1));
			}
			if ((Object)(object)component2 != (Object)null)
			{
				component2.alpha = 0f;
				TrackTween((Tween)(object)TweenSettingsExtensions.SetEase<TweenerCore<float, float, FloatOptions>>(DOTweenModuleUI.DOFade(component2, 1f, moveTime), (Ease)1));
			}
		}
		if ((Object)(object)tierTexts[fromIndex] != (Object)null)
		{
			((Behaviour)tierTexts[fromIndex]).enabled = true;
		}
	}

	private void CompleteTierTransition(int fromIndex, int toIndex, bool skipCollectionPresentation)
	{
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		TMP_Text obj = tierTexts[fromIndex];
		Transform val = ((obj != null) ? obj.transform.Find("green") : null);
		if ((Object)(object)val != (Object)null)
		{
			((Component)val).gameObject.SetActive(false);
			TMP_Text component = ((Component)val).GetComponent<TMP_Text>();
			CanvasGroup component2 = ((Component)val).GetComponent<CanvasGroup>();
			if ((Object)(object)component != (Object)null)
			{
				component.fontSize = 60f;
				((Graphic)component).color = transitionHighlightColor;
			}
			if ((Object)(object)component2 != (Object)null)
			{
				component2.alpha = 1f;
			}
		}
		SetSelected(toIndex);
		((Component)view.moveArrow).gameObject.SetActive(false);
		if (!skipCollectionPresentation)
		{
			onCollectionShow?.Invoke(toIndex);
		}
	}

	private void SetSelected(int index)
	{
		if (index >= 0 && index < 5)
		{
			if ((Object)(object)tierGreenBgs[index] != (Object)null)
			{
				((Component)tierGreenBgs[index]).gameObject.SetActive(true);
			}
			TMP_Text obj = tierTexts[index];
			Transform val = ((obj != null) ? obj.transform.Find("green") : null);
			if ((Object)(object)val != (Object)null)
			{
				((Component)val).gameObject.SetActive(true);
				ResetTierGreenVisual(val);
			}
			if ((Object)(object)tierTexts[index] != (Object)null)
			{
				((Behaviour)tierTexts[index]).enabled = false;
			}
		}
	}

	private void ResetTierGreenVisual(Transform greenTextTransform)
	{
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		if (!((Object)(object)greenTextTransform == (Object)null))
		{
			TMP_Text component = ((Component)greenTextTransform).GetComponent<TMP_Text>();
			if ((Object)(object)component != (Object)null)
			{
				component.fontSize = 60f;
				((Graphic)component).color = tierHighlightColor;
			}
			CanvasGroup component2 = ((Component)greenTextTransform).GetComponent<CanvasGroup>();
			if ((Object)(object)component2 != (Object)null)
			{
				component2.alpha = 1f;
			}
		}
	}

	private void SetUnselected(int index)
	{
		if (index >= 0 && index < 5)
		{
			if ((Object)(object)tierGreenBgs[index] != (Object)null)
			{
				((Component)tierGreenBgs[index]).gameObject.SetActive(false);
			}
			TMP_Text obj = tierTexts[index];
			Transform val = ((obj != null) ? obj.transform.Find("green") : null);
			if ((Object)(object)val != (Object)null)
			{
				((Component)val).gameObject.SetActive(false);
			}
			if ((Object)(object)tierTexts[index] != (Object)null)
			{
				((Behaviour)tierTexts[index]).enabled = true;
			}
		}
	}

	private void SetAllUnselected()
	{
		for (int i = 0; i < 5; i++)
		{
			SetUnselected(i);
		}
	}
}
