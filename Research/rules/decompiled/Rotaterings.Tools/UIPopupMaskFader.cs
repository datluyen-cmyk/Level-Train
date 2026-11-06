using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using UnityEngine;
using UnityEngine.UI;

namespace Rotaterings.Tools;

public class UIPopupMaskFader : MonoBehaviour
{
	[SerializeField]
	private Image maskImage;

	[SerializeField]
	private float fadeDuration;

	[SerializeField]
	private bool onlyManageStackTransitions = true;

	[SerializeField]
	private float visibleAlpha = 0.8f;

	[SerializeField]
	private float hiddenAlpha;

	private bool hasBeenCovered;

	private Tween fadeTween;

	private void OnDisable()
	{
		KillFadeTween();
	}

	public void OnUiStackActiveChanged(bool isActive)
	{
		if ((Object)(object)maskImage == (Object)null)
		{
			return;
		}
		if (isActive)
		{
			if (!onlyManageStackTransitions || hasBeenCovered)
			{
				FadeTo(visibleAlpha, restoreRaycast: true);
			}
		}
		else
		{
			hasBeenCovered = true;
			SetRaycastBlocked(blocked: false);
			FadeTo(hiddenAlpha, restoreRaycast: false);
		}
	}

	private void FadeTo(float endAlpha, bool restoreRaycast)
	{
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Expected O, but got Unknown
		KillFadeTween();
		if (fadeDuration <= 0f)
		{
			SetAlpha(endAlpha);
			SetRaycastBlocked(restoreRaycast);
			return;
		}
		if (Mathf.Approximately(((Graphic)maskImage).color.a, endAlpha))
		{
			if (restoreRaycast)
			{
				SetRaycastBlocked(blocked: true);
			}
			return;
		}
		fadeTween = (Tween)(object)TweenSettingsExtensions.OnComplete<TweenerCore<Color, Color, ColorOptions>>(TweenSettingsExtensions.SetLink<TweenerCore<Color, Color, ColorOptions>>(TweenSettingsExtensions.SetTarget<TweenerCore<Color, Color, ColorOptions>>(DOTweenModuleUI.DOFade(maskImage, endAlpha, fadeDuration), (object)((Component)this).gameObject), ((Component)this).gameObject, (LinkBehaviour)5), (TweenCallback)delegate
		{
			if (restoreRaycast)
			{
				SetRaycastBlocked(blocked: true);
			}
		});
	}

	private void SetAlpha(float alpha)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		Color color = ((Graphic)maskImage).color;
		color.a = alpha;
		((Graphic)maskImage).color = color;
	}

	private void SetRaycastBlocked(bool blocked)
	{
		((Graphic)maskImage).raycastTarget = blocked;
	}

	private void KillFadeTween()
	{
		Tween obj = fadeTween;
		if (obj != null)
		{
			TweenExtensions.Kill(obj, false);
		}
		fadeTween = null;
	}
}
