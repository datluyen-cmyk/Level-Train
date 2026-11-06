using UnityEngine;

namespace Game.UI.ClaimReward;

[CreateAssetMenu(fileName = "PropFlyPath", menuName = "Zoodoku/PropFlyPath")]
public class PropFlyPathAsset : ScriptableObject
{
	[Tooltip("X 轴轨迹（0=起点X，1=终点X）")]
	public AnimationCurve curveX = AnimationCurve.Linear(0f, 0f, 1f, 1f);

	[Tooltip("Y 轴轨迹（0=起点Y，1=终点Y，中间峰值>1 可形成上弧）")]
	public AnimationCurve curveY = new AnimationCurve((Keyframe[])(object)new Keyframe[3]
	{
		new Keyframe(0f, 0f, 0f, 4f),
		new Keyframe(0.35f, 1.5f, 0f, 0f),
		new Keyframe(1f, 1f, -2f, 0f)
	});

	public Vector2 Evaluate(float t, Vector2 start, Vector2 end)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		return new Vector2(Mathf.LerpUnclamped(start.x, end.x, curveX.Evaluate(t)), Mathf.LerpUnclamped(start.y, end.y, curveY.Evaluate(t)));
	}
}
