using System;
using UnityEngine;

public sealed class DragTranslationStopRollback
{
	private const string TAG = "[GamePlay][DragTranslationStopRollback] ";

	private const float StopRollbackDistance = 0.05f;

	private readonly Rigidbody2D rb;

	private readonly Transform ownerTransform;

	private readonly Collider2D dragCollider;

	private readonly RaycastHit2D[] castHits;

	private readonly float castSkinWidth;

	private readonly Func<Collider2D, bool> isCastHitAllowed;

	private ContactFilter2D castContactFilter;

	public DragTranslationStopRollback(Rigidbody2D rb, Transform ownerTransform, Collider2D dragCollider, ContactFilter2D castContactFilter, RaycastHit2D[] castHits, float castSkinWidth, Func<Collider2D, bool> isCastHitAllowed)
	{
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		this.rb = rb;
		this.ownerTransform = ownerTransform;
		this.dragCollider = dragCollider;
		this.castContactFilter = castContactFilter;
		this.castHits = castHits;
		this.castSkinWidth = castSkinWidth;
		this.isCastHitAllowed = isCastHitAllowed;
	}

	public void RollbackAfterStop(Vector2 lockedDir, Vector3 dragStartPosition, float moveDirection)
	{
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_0168: Unknown result type (might be due to invalid IL or missing references)
		//IL_0169: Unknown result type (might be due to invalid IL or missing references)
		//IL_016c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0171: Unknown result type (might be due to invalid IL or missing references)
		//IL_0176: Unknown result type (might be due to invalid IL or missing references)
		//IL_019b: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01af: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_018c: Unknown result type (might be due to invalid IL or missing references)
		if (Mathf.Approximately(moveDirection, 0f) || ((Vector2)(ref lockedDir)).sqrMagnitude < Mathf.Epsilon)
		{
			return;
		}
		((Vector2)(ref lockedDir)).Normalize();
		Vector2 val = -lockedDir * moveDirection;
		Vector2 val2 = (((Object)(object)rb != (Object)null) ? rb.position : Vector2.op_Implicit(ownerTransform.position));
		float num = Mathf.Abs(Vector2.Dot(new Vector2(val2.x - dragStartPosition.x, val2.y - dragStartPosition.y), lockedDir));
		float num2 = 0f;
		bool flag = false;
		if ((Object)(object)dragCollider != (Object)null)
		{
			float num3 = 0.05f + castSkinWidth;
			int num4 = dragCollider.Cast(val, castContactFilter, castHits, num3);
			for (int i = 0; i < num4; i++)
			{
				if ((Object)(object)((RaycastHit2D)(ref castHits[i])).collider != (Object)null && isCastHitAllowed(((RaycastHit2D)(ref castHits[i])).collider) && (!flag || ((RaycastHit2D)(ref castHits[i])).distance < num2))
				{
					num2 = ((RaycastHit2D)(ref castHits[i])).distance;
					flag = true;
				}
			}
		}
		float num5 = Mathf.Min(Mathf.Max(0f, 0.05f - num2), num);
		if (!(num5 <= 0f))
		{
			Vector2 val3 = val2 + val * num5;
			if ((Object)(object)rb != (Object)null)
			{
				rb.position = val3;
			}
			else
			{
				ownerTransform.position = new Vector3(val3.x, val3.y, ownerTransform.position.z);
			}
			Physics2D.SyncTransforms();
		}
	}
}
