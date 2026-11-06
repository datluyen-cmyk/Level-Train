using System.Collections.Generic;
using UnityEngine;

public sealed class PullEscapePlanner
{
	private const float MaxEscapeDistanceFallback = 60f;

	private const float EscapeMargin = 0.05f;

	private const float TangentTolerance = 0.01f;

	private const float MeasureStep = 0.02f;

	private const float MeasureCoarseStep = 0.2f;

	private const float EscapeLimitSpanRatio = 4f;

	private readonly PullObject owner;

	private readonly Collider2D[] overlapBuffer = (Collider2D[])(object)new Collider2D[16];

	private readonly RaycastHit2D[] castBuffer = (RaycastHit2D[])(object)new RaycastHit2D[256];

	private bool syncEachStep;

	private readonly HashSet<int> ignoringColliderIds = new HashSet<int>();

	public PullEscapePlanner(PullObject owner)
	{
		this.owner = owner;
	}

	public bool TryPlan(Vector2 direction, out float escapeDistance)
	{
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_011d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0127: Unknown result type (might be due to invalid IL or missing references)
		//IL_0154: Unknown result type (might be due to invalid IL or missing references)
		escapeDistance = 0f;
		if ((Object)(object)owner == (Object)null || ((Vector2)(ref direction)).sqrMagnitude < 1E-08f)
		{
			return false;
		}
		Overlap2DHandler overlap2DHandler = owner.Overlap2DHandler;
		Collider2D dragCollider = owner.DragCollider;
		if (overlap2DHandler == null || (Object)(object)dragCollider == (Object)null)
		{
			return false;
		}
		Physics2D.SyncTransforms();
		Rigidbody2D rb = owner.Rb;
		Collider2D collider = overlap2DHandler.Collider;
		syncEachStep = (Object)(object)rb == (Object)null || (Object)(object)dragCollider.attachedRigidbody != (Object)(object)rb || (Object)(object)collider == (Object)null || (Object)(object)collider.attachedRigidbody != (Object)(object)rb;
		Vector2 normalized = ((Vector2)(ref direction)).normalized;
		int overlapColliders = overlap2DHandler.GetOverlapColliders(overlapBuffer);
		ignoringColliderIds.Clear();
		for (int i = 0; i < overlapColliders && i < overlapBuffer.Length; i++)
		{
			Collider2D val = overlapBuffer[i];
			if (owner.IsEscapeRelevantCollider(val) && val.isTrigger)
			{
				ignoringColliderIds.Add(((Object)val).GetInstanceID());
			}
		}
		if (ignoringColliderIds.Count == 0)
		{
			return false;
		}
		float escapeDistanceLimit = GetEscapeDistanceLimit(normalized);
		float nearestBlockDistance = GetNearestBlockDistance(normalized, escapeDistanceLimit);
		if (nearestBlockDistance <= 0f)
		{
			return false;
		}
		if (nearestBlockDistance < 0.01f)
		{
			return false;
		}
		float limit = Mathf.Min(escapeDistanceLimit, nearestBlockDistance);
		if (!MeasureEscapeDistance(normalized, limit, out var distance))
		{
			return false;
		}
		escapeDistance = Mathf.Min(distance + 0.05f, escapeDistanceLimit);
		return true;
	}

	private float GetNearestBlockDistance(Vector2 axis, float distance)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		int num = owner.CastForEscape(axis, distance, castBuffer);
		if (num >= castBuffer.Length)
		{
			return 0f;
		}
		float num2 = float.MaxValue;
		for (int i = 0; i < num; i++)
		{
			Collider2D collider = ((RaycastHit2D)(ref castBuffer[i])).collider;
			if (!((Object)(object)collider == (Object)null) && !IsEscapeTargetCollider(collider) && owner.IsEscapeBlockingCollider(collider))
			{
				float distance2 = ((RaycastHit2D)(ref castBuffer[i])).distance;
				if (!(distance2 >= distance - 0.01f) && !(distance2 >= num2))
				{
					num2 = distance2;
				}
			}
		}
		return num2;
	}

	private float GetEscapeDistanceLimit(Vector2 axis)
	{
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		float num = (((Object)(object)owner.DragCollider != (Object)null) ? ProjectSpan(owner.DragCollider, axis) : 0f);
		if (num <= 0f)
		{
			return 60f;
		}
		return num * 4f + 0.1f;
	}

	private static float ProjectSpan(Collider2D col, Vector2 axis)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		Bounds bounds = col.bounds;
		return (Mathf.Abs(axis.x) * ((Bounds)(ref bounds)).extents.x + Mathf.Abs(axis.y) * ((Bounds)(ref bounds)).extents.y) * 2f;
	}

	private bool MeasureEscapeDistance(Vector2 axis, float limit, out float distance)
	{
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		distance = 0f;
		Rigidbody2D rb = owner.Rb;
		if ((Object)(object)rb == (Object)null)
		{
			return false;
		}
		Vector2 position = rb.position;
		float num = 0f;
		float num2 = 0f;
		bool flag = false;
		try
		{
			while (num2 < limit)
			{
				num = num2;
				num2 = Mathf.Min(num2 + 0.2f, limit);
				if (!MoveAndStillOverlaps(rb, position, axis, num2))
				{
					flag = true;
					break;
				}
			}
			if (flag)
			{
				float num3 = num;
				float num4 = num2;
				while (num3 + 0.02f < num4)
				{
					num3 += 0.02f;
					if (!MoveAndStillOverlaps(rb, position, axis, num3))
					{
						num4 = num3;
						break;
					}
				}
				num2 = num4;
			}
		}
		finally
		{
			rb.position = position;
			if (syncEachStep)
			{
				Physics2D.SyncTransforms();
			}
		}
		if (!flag)
		{
			return false;
		}
		distance = num2;
		return true;
	}

	private bool MoveAndStillOverlaps(Rigidbody2D body, Vector2 origin, Vector2 axis, float advanced)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		body.position = origin + axis * advanced;
		if (syncEachStep)
		{
			Physics2D.SyncTransforms();
		}
		return StillOverlapsAnyTarget();
	}

	private bool StillOverlapsAnyTarget()
	{
		Overlap2DHandler overlap2DHandler = owner.Overlap2DHandler;
		if (overlap2DHandler == null)
		{
			return false;
		}
		int overlapColliders = overlap2DHandler.GetOverlapColliders(overlapBuffer);
		for (int i = 0; i < overlapColliders && i < overlapBuffer.Length; i++)
		{
			Collider2D val = overlapBuffer[i];
			if ((Object)(object)val != (Object)null && IsEscapeTargetCollider(val))
			{
				return true;
			}
		}
		return false;
	}

	internal bool IsEscapeTargetCollider(Collider2D col)
	{
		if ((Object)(object)col != (Object)null)
		{
			return ignoringColliderIds.Contains(((Object)col).GetInstanceID());
		}
		return false;
	}
}
