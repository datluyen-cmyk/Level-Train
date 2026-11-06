using UnityEngine;

public sealed class DragTranslationOverlapEscape
{
	private readonly DraggableObjectWithLimitRange owner;

	public DragTranslationOverlapEscape(DraggableObjectWithLimitRange owner)
	{
		this.owner = owner;
	}

	public bool TryEscape(Collider2D blockCollider, float allowedDist, Vector2 direction, float axisMove, Vector2 currentPos)
	{
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		Collider2D dragCollider = owner.DragCollider;
		if ((Object)(object)dragCollider == (Object)null || (Object)(object)blockCollider == (Object)null || allowedDist > 0.01f)
		{
			return false;
		}
		ColliderDistance2D val = Physics2D.Distance(dragCollider, blockCollider);
		if (!((ColliderDistance2D)(ref val)).isValid || ((ColliderDistance2D)(ref val)).distance >= 0f || Vector2.Dot(direction, ((ColliderDistance2D)(ref val)).normal) >= 0f)
		{
			return false;
		}
		float num = Mathf.Min(Mathf.Abs(axisMove), 0f - ((ColliderDistance2D)(ref val)).distance + 0.01f);
		owner.BlockMotionSign = 0f;
		owner.NotifyDragStopRearmed();
		owner.Rb.position = currentPos + direction * num;
		return true;
	}
}
