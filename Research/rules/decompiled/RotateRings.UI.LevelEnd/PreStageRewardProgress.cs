using System;
using DG.Tweening;
using DG.Tweening.Core;
using Game.UI;
using RotateRings.Common;
using RotateRings.Services;
using UnityEngine;
using UnityEngine.UI;
using XingYun.UI.Core;

namespace RotateRings.UI.LevelEnd;

public class PreStageRewardProgress : GameWidgetView
{
	private const float FrontWidthOffset = 10f;

	private const float GrowDuration = 0.4f;

	private const float LightFadeDuration = 0.4f;

	private const float TrailRecycleDelay = 1.1f;

	private RectTransform imgFront;

	private RectTransform imgBack;

	private RectTransform aniLight;

	private Image aniLightImage;

	private RectTransform widgetRoot;

	private Tween growTween;

	private Tween lightHideTween;

	private GameObject trailPrefab;

	private GameObject trailInstance;

	private RectTransform trailRect;

	private float gridWidth;

	public override void Inject(UIBehaviour behaviour)
	{
		base.Inject(behaviour);
		ref RectTransform reference = ref widgetRoot;
		Transform transform = ((Component)behaviour).transform;
		reference = (RectTransform)(object)((transform is RectTransform) ? transform : null);
		imgFront = behaviour.Get<RectTransform>("Img_Front");
		imgBack = behaviour.Get<RectTransform>("Img_Back");
		aniLight = behaviour.Get<RectTransform>("AniLight");
		aniLightImage = (((Object)(object)aniLight != (Object)null) ? ((Component)aniLight).GetComponent<Image>() : null);
	}

	public void InitializeWidth()
	{
		gridWidth = 0f;
		SetBackWidth(0f);
		SetFrontWidth(0f);
		SetFrontVisible(visible: false);
		SetAniLightVisible(visible: false);
	}

	public void RefreshWidthAfterLayout()
	{
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		float num;
		if (!((Object)(object)widgetRoot != (Object)null))
		{
			num = 0f;
		}
		else
		{
			Rect rect = widgetRoot.rect;
			num = ((Rect)(ref rect)).width;
		}
		gridWidth = num;
		SetBackWidth(gridWidth);
		SetFrontWidth(0f);
		SetFrontVisible(visible: false);
		SetAniLightVisible(visible: false);
	}

	public void SetTrailPrefab(GameObject prefab)
	{
		if (!((Object)(object)trailPrefab == (Object)(object)prefab))
		{
			RecycleTrail();
			trailPrefab = prefab;
		}
	}

	public void SetLit(bool lit)
	{
		Tween obj = growTween;
		if (obj != null)
		{
			TweenExtensions.Kill(obj, false);
		}
		Tween obj2 = lightHideTween;
		if (obj2 != null)
		{
			TweenExtensions.Kill(obj2, false);
		}
		RecycleTrail();
		SetAniLightVisible(visible: false);
		SetFrontVisible(lit);
		SetFrontWidth(lit ? GetFrontWidth() : 0f);
	}

	public void PlayAnimate(Action onComplete, float duration = 0.4f)
	{
		//IL_00f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_0101: Expected O, but got Unknown
		AudioService.PlayUiSound(AudioKeys.ProgressUp.Value);
		Tween obj = growTween;
		if (obj != null)
		{
			TweenExtensions.Kill(obj, false);
		}
		Tween obj2 = lightHideTween;
		if (obj2 != null)
		{
			TweenExtensions.Kill(obj2, false);
		}
		RecycleTrail();
		if ((Object)(object)imgFront == (Object)null && (Object)(object)aniLight == (Object)null)
		{
			onComplete?.Invoke();
			return;
		}
		SetFrontVisible(visible: true);
		SetProgressWidth(0f);
		SetAniLightVisible(visible: true);
		SpawnTrail();
		if (duration == 0f)
		{
			SetProgressWidth(GetFrontWidth());
			SetAniLightVisible(visible: false);
			RecycleTrail();
			onComplete?.Invoke();
			return;
		}
		TweenCallback val2 = default(TweenCallback);
		growTween = (Tween)(object)TweenSettingsExtensions.OnComplete<Tweener>(TweenSettingsExtensions.SetEase<Tweener>(DOTween.To((DOSetter<float>)SetProgressWidth, 0f, GetFrontWidth(), duration), (Ease)3), (TweenCallback)delegate
		{
			//IL_0076: Unknown result type (might be due to invalid IL or missing references)
			//IL_0080: Expected O, but got Unknown
			//IL_0053: Unknown result type (might be due to invalid IL or missing references)
			//IL_0058: Unknown result type (might be due to invalid IL or missing references)
			//IL_005a: Expected O, but got Unknown
			//IL_005f: Expected O, but got Unknown
			Sequence val = DOTween.Sequence();
			if ((Object)(object)aniLightImage != (Object)null)
			{
				TweenSettingsExtensions.Append(val, (Tween)(object)DOTweenModuleUI.DOFade(aniLightImage, 0f, 0.4f));
			}
			PreStageRewardProgress preStageRewardProgress = this;
			TweenCallback obj3 = val2;
			if (obj3 == null)
			{
				TweenCallback val3 = delegate
				{
					SetAniLightVisible(visible: false);
					SetAniLightAlpha(1f);
				};
				TweenCallback val4 = val3;
				val2 = val3;
				obj3 = val4;
			}
			preStageRewardProgress.lightHideTween = (Tween)(object)TweenSettingsExtensions.InsertCallback(TweenSettingsExtensions.AppendCallback(val, obj3), 1.1f, new TweenCallback(RecycleTrail));
			onComplete?.Invoke();
		});
	}

	public override void Dispose()
	{
		StopPlayback();
		trailPrefab = null;
		imgFront = null;
		imgBack = null;
		widgetRoot = null;
		aniLight = null;
		aniLightImage = null;
		gridWidth = 0f;
		base.Dispose();
	}

	public void StopPlayback()
	{
		Tween obj = growTween;
		if (obj != null)
		{
			TweenExtensions.Kill(obj, false);
		}
		Tween obj2 = lightHideTween;
		if (obj2 != null)
		{
			TweenExtensions.Kill(obj2, false);
		}
		growTween = null;
		lightHideTween = null;
		RecycleTrail();
	}

	private float GetFrontWidth()
	{
		return gridWidth + 10f;
	}

	private void SetProgressWidth(float logicalWidth)
	{
		SetFrontWidth(logicalWidth);
		SetAniLightWidth(logicalWidth);
		UpdateTrailPosition();
	}

	private void SetFrontWidth(float logicalWidth)
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		if (!((Object)(object)imgFront == (Object)null))
		{
			Vector2 sizeDelta = imgFront.sizeDelta;
			sizeDelta.x = logicalWidth;
			imgFront.sizeDelta = sizeDelta;
		}
	}

	private void SetFrontVisible(bool visible)
	{
		if (!((Object)(object)imgFront == (Object)null) && ((Component)imgFront).gameObject.activeSelf != visible)
		{
			((Component)imgFront).gameObject.SetActive(visible);
		}
	}

	private void SetAniLightVisible(bool visible)
	{
		if (!((Object)(object)aniLight == (Object)null) && ((Component)aniLight).gameObject.activeSelf != visible)
		{
			if (visible)
			{
				SetAniLightAlpha(1f);
			}
			((Component)aniLight).gameObject.SetActive(visible);
		}
	}

	private void SetAniLightAlpha(float alpha)
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		if (!((Object)(object)aniLightImage == (Object)null))
		{
			Color color = ((Graphic)aniLightImage).color;
			color.a = alpha;
			((Graphic)aniLightImage).color = color;
		}
	}

	private void SpawnTrail()
	{
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		if (!((Object)(object)trailPrefab == (Object)null) && !((Object)(object)aniLight == (Object)null))
		{
			trailInstance = GameApp.Pool.Spawn(trailPrefab, (Transform)(object)aniLight);
			trailRect = (RectTransform)(((Object)(object)trailInstance != (Object)null) ? /*isinst with value type is only supported in some contexts*/: null);
			if ((Object)(object)trailRect != (Object)null)
			{
				((Transform)trailRect).localScale = Vector3.one;
			}
			UpdateTrailPosition();
		}
	}

	private void RecycleTrail()
	{
		if ((Object)(object)trailInstance != (Object)null)
		{
			GameApp.Pool.Recycle(trailInstance);
		}
		trailInstance = null;
		trailRect = null;
	}

	private void UpdateTrailPosition()
	{
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		if (!((Object)(object)trailRect == (Object)null) && !((Object)(object)imgFront == (Object)null) && !((Object)(object)aniLight == (Object)null))
		{
			RectTransform obj = imgFront;
			Rect rect = imgFront.rect;
			float xMax = ((Rect)(ref rect)).xMax;
			rect = imgFront.rect;
			Vector3 val = ((Transform)obj).TransformPoint(new Vector3(xMax, ((Rect)(ref rect)).center.y));
			Vector3 localPosition = ((Transform)aniLight).InverseTransformPoint(val);
			localPosition.x -= 5f;
			((Transform)trailRect).localPosition = localPosition;
		}
	}

	private void SetAniLightWidth(float logicalWidth)
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		if (!((Object)(object)aniLight == (Object)null))
		{
			Vector2 sizeDelta = aniLight.sizeDelta;
			sizeDelta.x = logicalWidth;
			aniLight.sizeDelta = sizeDelta;
		}
	}

	private void SetBackWidth(float logicalWidth)
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		if (!((Object)(object)imgBack == (Object)null))
		{
			Vector2 sizeDelta = imgBack.sizeDelta;
			sizeDelta.x = logicalWidth;
			imgBack.sizeDelta = sizeDelta;
		}
	}
}
