using System;
using UnityEngine;

namespace RotateRings.UI.RewardedLuckyDraw;

[Serializable]
public struct RewardedLuckyDrawWheelAnimationSettings
{
	public const float MinimumDuration = 0.01f;

	public const float MaximumDuration = 10f;

	public const float MaximumRevolutions = 30f;

	[InspectorName("加速时长(秒)")]
	public float accelDuration;

	[InspectorName("加速圈数")]
	public float accelRevolutions;

	[InspectorName("加速曲线")]
	public AnimationCurve accelCurve;

	[InspectorName("匀速时长(秒)")]
	public float cruiseDuration;

	[InspectorName("匀速圈数")]
	public float cruiseRevolutions;

	[InspectorName("匀速曲线")]
	public AnimationCurve cruiseCurve;

	[InspectorName("减速时长(秒)")]
	public float decelDuration;

	[InspectorName("减速额外圈数")]
	public float decelExtraRevolutions;

	[InspectorName("减速曲线")]
	public AnimationCurve decelCurve;

	[InspectorName("启用微振回弹")]
	public bool enableSettle;

	[InspectorName("回弹过冲角度(度)")]
	public float settleAngle;

	[InspectorName("回弹时长(秒)")]
	public float settleDuration;

	[InspectorName("回弹曲线")]
	public AnimationCurve settleCurve;

	[InspectorName("顺时针旋转")]
	public bool clockwise;

	[InspectorName("转停后领奖间隔(秒)")]
	[Min(0f)]
	public float rewardPopupDelay;

	public static RewardedLuckyDrawWheelAnimationSettings Default
	{
		get
		{
			//IL_0057: Unknown result type (might be due to invalid IL or missing references)
			//IL_0058: Unknown result type (might be due to invalid IL or missing references)
			//IL_008c: Unknown result type (might be due to invalid IL or missing references)
			//IL_008d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0092: Unknown result type (might be due to invalid IL or missing references)
			//IL_009c: Expected O, but got Unknown
			//IL_00eb: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ec: Unknown result type (might be due to invalid IL or missing references)
			//IL_0120: Unknown result type (might be due to invalid IL or missing references)
			//IL_0121: Unknown result type (might be due to invalid IL or missing references)
			//IL_0126: Unknown result type (might be due to invalid IL or missing references)
			//IL_0130: Expected O, but got Unknown
			//IL_017f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0180: Unknown result type (might be due to invalid IL or missing references)
			//IL_01b4: Unknown result type (might be due to invalid IL or missing references)
			//IL_01b5: Unknown result type (might be due to invalid IL or missing references)
			//IL_01ba: Unknown result type (might be due to invalid IL or missing references)
			//IL_01c4: Expected O, but got Unknown
			//IL_021b: Unknown result type (might be due to invalid IL or missing references)
			//IL_021c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0250: Unknown result type (might be due to invalid IL or missing references)
			//IL_0251: Unknown result type (might be due to invalid IL or missing references)
			//IL_0256: Unknown result type (might be due to invalid IL or missing references)
			//IL_0260: Expected O, but got Unknown
			RewardedLuckyDrawWheelAnimationSettings result = new RewardedLuckyDrawWheelAnimationSettings
			{
				accelDuration = 0.4f,
				accelRevolutions = 0.5f
			};
			Keyframe[] array = new Keyframe[2];
			Keyframe val = default(Keyframe);
			((Keyframe)(ref val))._002Ector(0f, 0f, 0f, 0f, 0f, 0f);
			((Keyframe)(ref val)).weightedMode = (WeightedMode)0;
			array[0] = val;
			((Keyframe)(ref val))._002Ector(1f, 1f, 3.0851035f, 3.0851035f, 0.05742824f, 0f);
			((Keyframe)(ref val)).weightedMode = (WeightedMode)0;
			array[1] = val;
			result.accelCurve = new AnimationCurve((Keyframe[])(object)array);
			result.cruiseDuration = 1.5f;
			result.cruiseRevolutions = 4f;
			Keyframe[] array2 = new Keyframe[2];
			((Keyframe)(ref val))._002Ector(0f, 0f, 0f, 1f, 0f, 0f);
			((Keyframe)(ref val)).weightedMode = (WeightedMode)0;
			array2[0] = val;
			((Keyframe)(ref val))._002Ector(1f, 1f, 1f, 0f, 0f, 0f);
			((Keyframe)(ref val)).weightedMode = (WeightedMode)0;
			array2[1] = val;
			result.cruiseCurve = new AnimationCurve((Keyframe[])(object)array2);
			result.decelDuration = 2f;
			result.decelExtraRevolutions = 2f;
			Keyframe[] array3 = new Keyframe[2];
			((Keyframe)(ref val))._002Ector(0f, 0f, 2.6560295f, 2.6560295f, 0f, 0.06991261f);
			((Keyframe)(ref val)).weightedMode = (WeightedMode)0;
			array3[0] = val;
			((Keyframe)(ref val))._002Ector(1f, 1f, 0f, 0f, 0f, 0f);
			((Keyframe)(ref val)).weightedMode = (WeightedMode)0;
			array3[1] = val;
			result.decelCurve = new AnimationCurve((Keyframe[])(object)array3);
			result.enableSettle = true;
			result.settleAngle = 5f;
			result.settleDuration = 0.6f;
			Keyframe[] array4 = new Keyframe[2];
			((Keyframe)(ref val))._002Ector(0f, 0f, 0f, 0f, 0f, 0f);
			((Keyframe)(ref val)).weightedMode = (WeightedMode)0;
			array4[0] = val;
			((Keyframe)(ref val))._002Ector(1f, 1f, 0f, 0f, 0.7940075f, 0f);
			((Keyframe)(ref val)).weightedMode = (WeightedMode)3;
			array4[1] = val;
			result.settleCurve = new AnimationCurve((Keyframe[])(object)array4);
			result.clockwise = true;
			result.rewardPopupDelay = 0.5f;
			return result;
		}
	}

	public float TotalDuration => accelDuration + cruiseDuration + decelDuration + (enableSettle ? settleDuration : 0f);

	public float TotalRevolutions => accelRevolutions + cruiseRevolutions + decelExtraRevolutions;

	public bool IsValid
	{
		get
		{
			if (IsFiniteAtLeast(accelDuration, 0.01f) && IsFiniteAtLeast(cruiseDuration, 0.01f) && IsFiniteAtLeast(decelDuration, 0.01f) && IsFiniteAtLeast(settleDuration, 0.01f) && IsFiniteAtLeast(accelRevolutions, 0f) && IsFiniteAtLeast(cruiseRevolutions, 0f) && IsFiniteAtLeast(decelExtraRevolutions, 0f) && IsFiniteAtLeast(settleAngle, 0f) && IsFiniteAtLeast(rewardPopupDelay, 0f) && IsCurveValid(accelCurve) && IsCurveValid(cruiseCurve) && IsCurveValid(decelCurve))
			{
				return IsCurveValid(settleCurve);
			}
			return false;
		}
	}

	public RewardedLuckyDrawWheelAnimationSettings FreezeCurves()
	{
		RewardedLuckyDrawWheelAnimationSettings result = this;
		result.accelCurve = CopyCurve(accelCurve);
		result.cruiseCurve = CopyCurve(cruiseCurve);
		result.decelCurve = CopyCurve(decelCurve);
		result.settleCurve = CopyCurve(settleCurve);
		return result;
	}

	private static AnimationCurve CopyCurve(AnimationCurve curve)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Expected O, but got Unknown
		return new AnimationCurve(curve.keys)
		{
			preWrapMode = curve.preWrapMode,
			postWrapMode = curve.postWrapMode
		};
	}

	private static bool IsCurveValid(AnimationCurve curve)
	{
		if (curve != null)
		{
			return curve.length >= 2;
		}
		return false;
	}

	private static bool IsFiniteAtLeast(float value, float minimum)
	{
		if (!float.IsNaN(value) && !float.IsInfinity(value))
		{
			return value >= minimum;
		}
		return false;
	}
}
