using System;
using UnityEngine;

namespace Basis.Tool;

[Serializable]
public class ScaleKeyFrame
{
	[Tooltip("目标缩放比例")]
	public Vector3 targetScale = Vector3.one;

	[Tooltip("动画持续时间 (秒)")]
	public float duration = 0.2f;

	[Tooltip("动画曲线")]
	public AnimationCurve curve = AnimationCurve.Linear(0f, 0f, 1f, 1f);
}
