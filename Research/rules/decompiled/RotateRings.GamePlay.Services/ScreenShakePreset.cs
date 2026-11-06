using System;
using DG.Tweening;
using UnityEngine;

namespace RotateRings.GamePlay.Services;

[Serializable]
public class ScreenShakePreset
{
	[Tooltip("优先级：进行中震屏仅被更高优先级的新触发抢占（严格大于才抢占）")]
	public int Priority = 1;

	[Tooltip("起始延迟（秒）：请求后等待此时长再开始抖动；延迟期间参与优先级抢占")]
	public float Delay;

	[Tooltip("震动持续时长（秒）")]
	public float Duration = 0.3f;

	[Tooltip("位移幅度（世界单位），随进度衰减到 0")]
	public float Amplitude = 0.12f;

	[Tooltip("振动次数：DOTween vibrato 参数，整个震屏期间的总振荡次数")]
	public int Vibrato = 10;

	[Tooltip("是否启用末段渐出衰减（DOShake fadeOut）")]
	public bool FadeOut = true;

	[Tooltip("位置震屏随机度 (0-180)：0 = 确定性正弦振荡，值越大方向越随机自然")]
	[Range(0f, 180f)]
	public float PositionRandomness;

	[Tooltip("震屏缓动曲线：控制振幅随进度的衰减形态（如 InQuad 前段猛、OutQuad 后段柔）")]
	public Ease Ease = (Ease)6;
}
