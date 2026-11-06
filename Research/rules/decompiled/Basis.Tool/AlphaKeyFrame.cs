using System;
using UnityEngine;

namespace Basis.Tool;

[Serializable]
public class AlphaKeyFrame
{
	[Tooltip("目标透明度 (0-1)")]
	[Range(0f, 1f)]
	public float targetAlpha = 1f;

	[Tooltip("动画持续时间 (秒)")]
	public float duration = 0.2f;

	[Tooltip("动画曲线")]
	public AnimationCurve curve = AnimationCurve.Linear(0f, 0f, 1f, 1f);
}
