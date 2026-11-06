using System.Collections.Generic;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using RotateRings.Services;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Vibration;

namespace Basis.Tool;

public class ButtonClickEffect : MonoBehaviour, IPointerDownHandler, IEventSystemHandler, IPointerUpHandler, IPointerClickHandler
{
	[Header("点击效果类型")]
	public ClickEffectType clickEffectType;

	[Header("组件引用")]
	private Button targetButton;

	private CanvasGroup targetCanvasGroup;

	[SerializeField]
	private GameObject tweenObj;

	private ButtonEffectSequence pointerDownSequence;

	private ButtonEffectSequence pointerUpSequence;

	[Header("给美术调试用")]
	public bool isDebug;

	[Header("音效配置")]
	[SerializeField]
	private string clickAudio = "rr_sfx_click";

	private Vector3 _originalScale;

	private float _originalAlpha;

	private Sequence _activeSequence;

	private Sequence _activeSequence1;

	private Sequence _activeSequence2;

	private void Awake()
	{
		//IL_010b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0110: Unknown result type (might be due to invalid IL or missing references)
		ButtonClickAnimationCfg buttonClickAnimationCfg = null;
		buttonClickAnimationCfg = ButtonClickAnimationCfg.Instance;
		switch (clickEffectType)
		{
		case ClickEffectType.Circle:
			pointerDownSequence = buttonClickAnimationCfg.circleClickEffectConfig.pointerDownSequence;
			pointerUpSequence = buttonClickAnimationCfg.circleClickEffectConfig.pointerUpSequence;
			break;
		case ClickEffectType.RectangleFrame:
			pointerDownSequence = buttonClickAnimationCfg.rectangleFrameClickEffectConfig.pointerDownSequence;
			pointerUpSequence = buttonClickAnimationCfg.rectangleFrameClickEffectConfig.pointerUpSequence;
			break;
		case ClickEffectType.SquareFrame:
			pointerDownSequence = buttonClickAnimationCfg.squareFrameClickEffectConfig.pointerDownSequence;
			pointerUpSequence = buttonClickAnimationCfg.squareFrameClickEffectConfig.pointerUpSequence;
			break;
		}
		if ((Object)(object)tweenObj == (Object)null)
		{
			tweenObj = ((Component)this).gameObject;
		}
		if ((Object)(object)targetButton == (Object)null)
		{
			targetButton = ((Component)this).GetComponent<Button>();
		}
		if ((Object)(object)targetCanvasGroup == (Object)null)
		{
			targetCanvasGroup = tweenObj.GetComponent<CanvasGroup>();
			if ((Object)(object)targetCanvasGroup == (Object)null)
			{
				targetCanvasGroup = tweenObj.AddComponent<CanvasGroup>();
			}
		}
		_originalScale = tweenObj.transform.localScale;
		_originalAlpha = targetCanvasGroup.alpha;
	}

	private void OnEnable()
	{
		ResetToOriginalState();
	}

	private void OnDisable()
	{
		KillActiveSequences();
		ResetToOriginalState();
	}

	public void OnPointerDown(PointerEventData eventData)
	{
		if (IsInteractable())
		{
			PlayEffectSequence(pointerDownSequence);
		}
	}

	public void OnPointerUp(PointerEventData eventData)
	{
		if (IsInteractable())
		{
			PlayEffectSequence(pointerUpSequence);
		}
	}

	public void OnPointerClick(PointerEventData eventData)
	{
		VibrateUtility.ButtonVibrate();
		PlayClickSound();
	}

	private void PlayEffectSequence(ButtonEffectSequence sequence)
	{
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		if (_activeSequence != _activeSequence1)
		{
			Sequence activeSequence = _activeSequence1;
			if (activeSequence != null)
			{
				TweenExtensions.Kill((Tween)(object)activeSequence, false);
			}
			_activeSequence1 = DOTween.Sequence();
			_activeSequence = _activeSequence1;
		}
		else
		{
			Sequence activeSequence2 = _activeSequence2;
			if (activeSequence2 != null)
			{
				TweenExtensions.Kill((Tween)(object)activeSequence2, false);
			}
			_activeSequence2 = DOTween.Sequence();
			_activeSequence = _activeSequence2;
		}
		float num = 0f;
		foreach (ScaleKeyFrame scaleKeyFrame in sequence.scaleKeyFrames)
		{
			TweenSettingsExtensions.Insert(_activeSequence, num, (Tween)(object)TweenSettingsExtensions.SetEase<TweenerCore<Vector3, Vector3, VectorOptions>>(ShortcutExtensions.DOScale(tweenObj.transform, scaleKeyFrame.targetScale, scaleKeyFrame.duration), scaleKeyFrame.curve));
			num += scaleKeyFrame.duration;
		}
		float num2 = 0f;
		foreach (AlphaKeyFrame alphaKeyFrame in sequence.alphaKeyFrames)
		{
			TweenSettingsExtensions.Insert(_activeSequence, num2, (Tween)(object)TweenSettingsExtensions.SetEase<TweenerCore<float, float, FloatOptions>>(DOTweenModuleUI.DOFade(targetCanvasGroup, alphaKeyFrame.targetAlpha, alphaKeyFrame.duration), alphaKeyFrame.curve));
			num2 += alphaKeyFrame.duration;
		}
	}

	private void ResetToOriginalState()
	{
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)tweenObj != (Object)null)
		{
			tweenObj.transform.localScale = _originalScale;
		}
		if ((Object)(object)targetCanvasGroup != (Object)null)
		{
			targetCanvasGroup.alpha = _originalAlpha;
		}
	}

	private void KillActiveSequences()
	{
		Sequence activeSequence = _activeSequence1;
		if (activeSequence != null)
		{
			TweenExtensions.Kill((Tween)(object)activeSequence, false);
		}
		Sequence activeSequence2 = _activeSequence2;
		if (activeSequence2 != null)
		{
			TweenExtensions.Kill((Tween)(object)activeSequence2, false);
		}
		_activeSequence = null;
		_activeSequence1 = null;
		_activeSequence2 = null;
	}

	private bool IsInteractable()
	{
		if (!((Object)(object)targetButton == (Object)null))
		{
			return ((Selectable)targetButton).interactable;
		}
		return true;
	}

	private void PlayClickSound()
	{
		if (clickAudio != string.Empty)
		{
			AudioService.PlayUiSound(clickAudio);
		}
	}

	private void OnApplicationFocus(bool hasFocus)
	{
		if (!hasFocus && IsInteractable())
		{
			PlayEffectSequence(pointerUpSequence);
		}
	}

	[ContextMenu("Apply Default Down Animation")]
	private void ApplyDefaultDownAnimation()
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		pointerDownSequence = new ButtonEffectSequence
		{
			scaleKeyFrames = new List<ScaleKeyFrame>
			{
				new ScaleKeyFrame
				{
					targetScale = Vector3.one * 0.95f,
					duration = 0.1f,
					curve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f)
				}
			},
			alphaKeyFrames = new List<AlphaKeyFrame>
			{
				new AlphaKeyFrame
				{
					targetAlpha = 0.9f,
					duration = 0.1f,
					curve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f)
				}
			}
		};
	}

	[ContextMenu("Apply Default Up Animation")]
	private void ApplyDefaultUpAnimation()
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		pointerUpSequence = new ButtonEffectSequence
		{
			scaleKeyFrames = new List<ScaleKeyFrame>
			{
				new ScaleKeyFrame
				{
					targetScale = Vector3.one,
					duration = 0.2f,
					curve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f)
				}
			},
			alphaKeyFrames = new List<AlphaKeyFrame>
			{
				new AlphaKeyFrame
				{
					targetAlpha = 1f,
					duration = 0.2f,
					curve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f)
				}
			}
		};
	}
}
