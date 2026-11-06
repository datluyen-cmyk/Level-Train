using System;
using UnityEngine;

namespace RotateRings.UI.LevelEnd;

[Serializable]
public struct LevelWinAnimationTiming
{
	[InspectorName("参照阶段")]
	public LevelWinAnimationStage Reference;

	[InspectorName("参照时点")]
	public LevelWinAnimationAnchor Anchor;

	[InspectorName("延迟秒数")]
	[Min(0f)]
	[Tooltip("开始：参照阶段实际开始后计时；结束：实际完成后计时。仅支持0及正数，0同次触发，不等待下一帧。")]
	public float OffsetSeconds;

	public LevelWinAnimationTiming(LevelWinAnimationStage reference, LevelWinAnimationAnchor anchor = LevelWinAnimationAnchor.End, float offsetSeconds = 0f)
	{
		Reference = reference;
		Anchor = anchor;
		OffsetSeconds = offsetSeconds;
	}

	internal void Validate(LevelWinAnimationStage target)
	{
		if (!Enum.IsDefined(typeof(LevelWinAnimationStage), Reference) || Reference == target || !Enum.IsDefined(typeof(LevelWinAnimationAnchor), Anchor) || !IsFinite(OffsetSeconds) || OffsetSeconds < 0f)
		{
			throw new InvalidOperationException($"结算动画时机非法：{target}，请检查参照阶段、开始/结束；延迟秒数只允许0及正数");
		}
	}

	internal static bool IsFinite(float value)
	{
		if (!float.IsNaN(value))
		{
			return !float.IsInfinity(value);
		}
		return false;
	}
}
