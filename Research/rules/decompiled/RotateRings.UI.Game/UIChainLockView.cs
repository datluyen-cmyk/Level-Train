using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using Game.UI;
using RotateRings.Common;
using Spine;
using Spine.Unity;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using XingYun.Extensions;
using XingYun.UI.Core;

namespace RotateRings.UI.Game;

public class UIChainLockView : GameWidgetView
{
	private const string Tag = "[UIChainLockView] ";

	private const string IdleAnimName = "rr_suo_jingzhi";

	private const string OpenAnimName = "rr_suo_open";

	private const string HuangdongAnimName = "rr_suo_huangdong";

	private const float BaseRingInnerRadius = 40f;

	private const float MaxLockScaleRatio = 2.57f;

	private const float LockScaleFactor = 0.7f;

	private Tween numFadeTween;

	private float baseScaleRatio = 1f;

	public TMP_Text txtNum;

	public RectTransform eff_junei_unlock_suotou;

	public SkeletonGraphic suotou;

	public void Bind(RingObject ring, RectTransform parentRect)
	{
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)ring == (Object)null)
		{
			return;
		}
		if ((Object)(object)parentRect == (Object)null)
		{
			UpdateLockCount(ring.LockCount);
			return;
		}
		if (!WorldToUIPostionUtil.WorldToLocalPoint(ring.GetPosition(), parentRect, out var _))
		{
			UpdateLockCount(ring.LockCount);
			return;
		}
		float ringInnerDiameterPixels = GetRingInnerDiameterPixels(ring, parentRect);
		baseScaleRatio = 1f;
		if (ringInnerDiameterPixels > 0f)
		{
			float num = Mathf.Clamp(ringInnerDiameterPixels / 40f, 1f, 2.57f);
			baseScaleRatio = Mathf.Max(1f, num * 0.7f);
		}
		if ((Object)(object)((UIBaseView)this).NodeRect != (Object)null)
		{
			((Transform)((UIBaseView)this).NodeRect).localRotation = Quaternion.identity;
		}
		RefreshPosition(ring, parentRect, 1f);
		UpdateLockCount(ring.LockCount);
		PlayIdleAnim();
	}

	public void SetEnterScaleFactor(float factor)
	{
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)((UIBaseView)this).NodeRect != (Object)null)
		{
			float num = baseScaleRatio * factor;
			((Transform)((UIBaseView)this).NodeRect).localScale = new Vector3(num, num, 1f);
		}
	}

	public void RefreshPosition(RingObject ring, RectTransform parentRect, float cameraScaleFactor)
	{
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		if (!((Object)(object)ring == (Object)null) && !((Object)(object)parentRect == (Object)null) && !((Object)(object)((UIBaseView)this).NodeRect == (Object)null) && WorldToUIPostionUtil.WorldToLocalPoint(ring.GetPosition(), parentRect, out var localPoint))
		{
			((Transform)((UIBaseView)this).NodeRect).localPosition = new Vector3(localPoint.x, localPoint.y, 0f);
			float num = baseScaleRatio * cameraScaleFactor;
			((Transform)((UIBaseView)this).NodeRect).localScale = new Vector3(num, num, 1f);
		}
	}

	private void PlayIdleAnim()
	{
		if (!((Object)(object)suotou == (Object)null) && suotou.AnimationState != null)
		{
			suotou.AnimationState.SetAnimation(0, "rr_suo_jingzhi", true);
		}
	}

	public void PlayHuangdongAnim()
	{
		if (!((Object)(object)suotou == (Object)null) && suotou.AnimationState != null)
		{
			Skeleton skeleton = suotou.Skeleton;
			object obj;
			if (skeleton == null)
			{
				obj = null;
			}
			else
			{
				SkeletonData data = skeleton.Data;
				obj = ((data != null) ? data.FindAnimation("rr_suo_huangdong") : null);
			}
			if (obj == null)
			{
				PlayIdleAnim();
			}
			else
			{
				suotou.AnimationState.SetAnimation(0, "rr_suo_huangdong", false);
			}
		}
	}

	public float PlayOpenAnim()
	{
		if ((Object)(object)suotou == (Object)null || suotou.AnimationState == null)
		{
			return 0f;
		}
		Skeleton skeleton = suotou.Skeleton;
		object obj;
		if (skeleton == null)
		{
			obj = null;
		}
		else
		{
			SkeletonData data = skeleton.Data;
			obj = ((data != null) ? data.FindAnimation("rr_suo_open") : null);
		}
		Animation val = (Animation)obj;
		if (val == null)
		{
			return 0f;
		}
		suotou.AnimationState.SetAnimation(0, "rr_suo_open", false);
		PlayNumFadeWithConfig(val.Duration);
		return val.Duration;
	}

	private float GetRingInnerDiameterPixels(RingObject ring, RectTransform parentRect)
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		if (!GameCoordScaleUtil.TryGetRingInnerDiameterWorld(ring, out var innerDiameterWorld))
		{
			return 0f;
		}
		return WorldToUIPostionUtil.WorldRadiusToLocal(ring.GetPosition(), innerDiameterWorld * 0.5f, parentRect);
	}

	public void UpdateLockCount(int count)
	{
		if ((Object)(object)txtNum != (Object)null)
		{
			TMPTextExtensions.SetLocalizedTextValue(txtNum, count.ToString());
		}
	}

	private void PlayNumFadeWithConfig(float duration)
	{
		//IL_00cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d7: Expected O, but got Unknown
		if ((Object)(object)txtNum == (Object)null || duration <= 0f)
		{
			return;
		}
		AnimationCurve curve = RingLevelConfig.Instance.lockOpenNumFadeCurve;
		if (curve != null)
		{
			Tween obj = numFadeTween;
			if (obj != null)
			{
				TweenExtensions.Kill(obj, false);
			}
			SetTxtNumAlpha(curve.Evaluate(0f));
			GameObject val = (((Object)(object)((UIBaseView)this).Behaviour != (Object)null) ? ((Component)((UIBaseView)this).Behaviour).gameObject : null);
			numFadeTween = (Tween)(object)TweenSettingsExtensions.OnComplete<TweenerCore<float, float, FloatOptions>>(TweenSettingsExtensions.SetLink<TweenerCore<float, float, FloatOptions>>(TweenSettingsExtensions.SetEase<TweenerCore<float, float, FloatOptions>>(DOTween.To((DOGetter<float>)(() => 0f), (DOSetter<float>)delegate(float progress)
			{
				SetTxtNumAlpha(curve.Evaluate(progress));
			}, 1f, duration), (Ease)1), val, (LinkBehaviour)5), (TweenCallback)delegate
			{
				numFadeTween = null;
			});
		}
	}

	private void SetTxtNumAlpha(float alpha)
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		if (!((Object)(object)txtNum == (Object)null))
		{
			Color color = ((Graphic)txtNum).color;
			color.a = Mathf.Clamp01(alpha);
			((Graphic)txtNum).color = color;
		}
	}

	public override void Dispose()
	{
		Tween obj = numFadeTween;
		if (obj != null)
		{
			TweenExtensions.Kill(obj, false);
		}
		numFadeTween = null;
		base.Dispose();
	}

	public override void Inject(UIBehaviour behaviour)
	{
		base.Inject(behaviour);
		txtNum = behaviour.Get<TMP_Text>("txtNum");
		eff_junei_unlock_suotou = behaviour.Get<RectTransform>("eff_junei_unlock_suotou");
		suotou = behaviour.Get<SkeletonGraphic>("suotou");
	}
}
