using UnityEngine;

namespace RotateRings.UI.Common;

[CreateAssetMenu(fileName = "RewardFlyCfg", menuName = "UI/RewardFlyCfg")]
public class RewardFlyCfg : ScriptableObject
{
	[Tooltip("X 轴轨迹（0=起点X，1=终点X，中段>1 可形成侧向弧线）")]
	public AnimationCurve curveX = AnimationCurve.Linear(0f, 0f, 1f, 1f);

	[Tooltip("Y 轴轨迹（0=起点Y，1=终点Y，中段<0 表示超过起点向上起跳再落到终点）")]
	public AnimationCurve curveY = new AnimationCurve((Keyframe[])(object)new Keyframe[3]
	{
		new Keyframe(0f, 0f, -0.9f, -0.9f),
		new Keyframe(0.4f, -0.35f, 0f, 0f),
		new Keyframe(1f, 1f, 2.3f, 2.3f)
	});

	public Vector3 Evaluate(float t, Vector3 start, Vector3 end)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		return new Vector3(Mathf.LerpUnclamped(start.x, end.x, curveX.Evaluate(t)), Mathf.LerpUnclamped(start.y, end.y, curveY.Evaluate(t)), Mathf.LerpUnclamped(start.z, end.z, t));
	}
}
