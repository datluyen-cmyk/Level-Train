using UnityEngine;

namespace RotateRings.GamePlay.Services;

public sealed class DragDefaultLimitStyleHandler : IDragLimitStyleHandler
{
	private const float MinWorldMoveSqr = 0.0009f;

	public void OnPointerDown(IGameplayObject target, GameplayPointerData data)
	{
		DraggableObjectWithLimitRange limitRangeObject;
		if (GameplayObjectQuery.TryGetDragRotateObject(target, out var dragRotate))
		{
			OnRingPointerDown(dragRotate, data);
		}
		else if (GameplayObjectQuery.TryGetObjectWithLimitRange(target, out limitRangeObject))
		{
			OnStickPointerDown(limitRangeObject, data);
		}
	}

	public void OnPointerDrag(IGameplayObject target, GameplayPointerData data)
	{
		DraggableObjectWithLimitRange limitRangeObject;
		if (GameplayObjectQuery.TryGetDragRotateObject(target, out var dragRotate))
		{
			OnRingPointerDrag(dragRotate, data);
		}
		else if (GameplayObjectQuery.TryGetObjectWithLimitRange(target, out limitRangeObject))
		{
			OnStickPointerDrag(limitRangeObject, data);
		}
	}

	public void OnPointerUp(IGameplayObject target)
	{
		DraggableObjectWithLimitRange limitRangeObject;
		if (GameplayObjectQuery.TryGetDragRotateObject(target, out var dragRotate))
		{
			OnRingPointerUp(dragRotate);
		}
		else if (GameplayObjectQuery.TryGetObjectWithLimitRange(target, out limitRangeObject))
		{
			OnStickPointerUp(limitRangeObject);
		}
	}

	public void TickLateUpdate(IGameplayObject activeTarget)
	{
		if (activeTarget != null && GameplayObjectQuery.TryGetDragRotateObject(activeTarget, out var dragRotate))
		{
			OnRingTickLateUpdate(dragRotate);
		}
	}

	public void ClearSession()
	{
	}

	private static void OnRingPointerDown(DragRotateObject ring, GameplayPointerData data)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		ring.LegacyLastWorldPosition = data.WorldPosition;
	}

	private static void OnRingPointerDrag(DragRotateObject ring, GameplayPointerData data)
	{
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a2: Unknown result type (might be due to invalid IL or missing references)
		ring.ResetProbeFlagsForChase();
		if (!ring.IsDragging)
		{
			ring.LogDragBlockedOnceForDragLimit("not dragging");
			return;
		}
		ring.DrawDragDebugLine(data);
		Vector3 worldPosition = data.WorldPosition;
		Vector3 val = Vector2.op_Implicit(ring.GetRingCenterWorld());
		Vector3 val2 = worldPosition - ring.LegacyLastWorldPosition;
		if (((Vector3)(ref val2)).sqrMagnitude < 0.0009f)
		{
			return;
		}
		float num = ring.LegacyLastWorldPosition.x - val.x;
		float num2 = ring.LegacyLastWorldPosition.y - val.y;
		float num3 = worldPosition.x - val.x;
		float num4 = worldPosition.y - val.y;
		ring.LegacyLastWorldPosition = worldPosition;
		float num5 = Mathf.Sqrt((num * num + num2 * num2) * (num3 * num3 + num4 * num4));
		float num6 = 0f;
		if (num5 >= 1E-15f)
		{
			num6 = Mathf.Acos(Mathf.Clamp((num * num3 + num2 * num4) / num5, -1f, 1f)) * 57.29578f;
		}
		if (num * num4 - num2 * num3 < 0f)
		{
			num6 = 0f - num6;
		}
		if (num6 * ring.BlockDirection > 0f)
		{
			ring.LogDragBlockedOnceForDragLimit($"blockDirection={ring.BlockDirection:0.##} angle={num6:0.##}");
			return;
		}
		ring.BlockDirection = 0f;
		float num7 = ring.AdjustRotationDeltaForDragLimit(num6);
		ring.AddRotateAngle(num7);
		float currentRotationZ = ring.GetCurrentRotationZ();
		float num8 = ring.StartRotation + ring.GetRotateAngle();
		ring.MarkRotationProbedThisFrame();
		float num9 = ring.FindLastValidRotationZ(currentRotationZ, num8);
		ring.SetLastValidRotationZ(num9);
		ring.SyncRotateAngleFromTransform();
		float ringAppliedDeltaDeg = Mathf.DeltaAngle(currentRotationZ, num9);
		ring.InvokeRotationDragFrame(num6, num7, ringAppliedDeltaDeg, worldPosition, data.ScreenPosition);
		if (Mathf.Abs(Mathf.DeltaAngle(num9, num8)) >= 0.1f)
		{
			ring.BlockDirection = Mathf.Sign(num6);
			ring.InvokeRotationBlocked(num6);
			ring.LogDragBlockedOnceForDragLimit("overlap blocked");
		}
	}

	private static void OnRingPointerUp(DragRotateObject ring)
	{
	}

	private static void OnStickPointerDown(DraggableObjectWithLimitRange stick, GameplayPointerData data)
	{
	}

	private void OnStickPointerDrag(DraggableObjectWithLimitRange stick, GameplayPointerData data)
	{
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		if (stick.BeingHeldForDragLimit && stick.HasMainCameraForDragLimit)
		{
			if (stick.IsVelocityDragMode)
			{
				stick.LegacyMoveToward(stick.ComputeLockedTargetFromPointer(data));
			}
			else
			{
				stick.ApplyInstanceDrag(data);
			}
		}
	}

	private void OnStickPointerUp(DraggableObjectWithLimitRange stick)
	{
	}

	private void OnRingTickLateUpdate(DragRotateObject ring)
	{
		if (ring.IsDragging)
		{
			ring.PinWorldPosition();
		}
	}
}
