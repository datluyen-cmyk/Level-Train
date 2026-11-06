using UnityEngine;

namespace RotateRings.GamePlay.Services;

public sealed class DragGuardLimitStyleHandler : IDragLimitStyleHandler
{
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
		if (activeTarget != null)
		{
			DraggableObjectWithLimitRange limitRangeObject;
			if (GameplayObjectQuery.TryGetDragRotateObject(activeTarget, out var dragRotate) && dragRotate.IsDragging)
			{
				ChaseRingTowardFinger(dragRotate);
				dragRotate.PinWorldPosition();
			}
			else if (GameplayObjectQuery.TryGetObjectWithLimitRange(activeTarget, out limitRangeObject) && limitRangeObject.BeingHeldForDragLimit && limitRangeObject.IsVelocityDragMode)
			{
				limitRangeObject.GuardChaseTowardDragTarget();
			}
		}
	}

	public void ClearSession()
	{
	}

	private void OnRingPointerDown(DragRotateObject ring, GameplayPointerData data)
	{
		ring.GuardOnGestureBegin(data);
	}

	private void OnRingPointerDrag(DragRotateObject ring, GameplayPointerData data)
	{
		if (!ring.IsDragging)
		{
			ring.LogDragBlockedOnceForDragLimit("not dragging");
			return;
		}
		ring.DrawDragDebugLine(data);
		ring.GuardRefreshFingerTarget(data);
	}

	private void OnRingPointerUp(DragRotateObject ring)
	{
	}

	private void OnStickPointerDown(DraggableObjectWithLimitRange stick, GameplayPointerData data)
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		if (stick.IsVelocityDragMode)
		{
			stick.SetDragLockedTarget(stick.ComputeLockedTargetFromPointer(data));
		}
	}

	private void OnStickPointerDrag(DraggableObjectWithLimitRange stick, GameplayPointerData data)
	{
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		if (stick.BeingHeldForDragLimit && stick.HasMainCameraForDragLimit)
		{
			if (stick.IsVelocityDragMode)
			{
				stick.SetDragLockedTarget(stick.ComputeLockedTargetFromPointer(data));
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

	private void ChaseRingTowardFinger(DragRotateObject ring)
	{
		//IL_00cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d3: Unknown result type (might be due to invalid IL or missing references)
		RingRotateGuardSubstepRotator ringRotateGuardSubstepRotator = ring.EnsureRotateGuardSubstepRotator();
		float currentRotationZ = ring.GetCurrentRotationZ();
		float cachedRawTargetZ = ringRotateGuardSubstepRotator.CachedRawTargetZ;
		float num = Mathf.DeltaAngle(currentRotationZ, cachedRawTargetZ);
		if (Mathf.Abs(num) <= 0.1f)
		{
			return;
		}
		ring.ResetProbeFlagsForChase();
		float maxFingerDeltaDegPerFrame = RingRotateGuardConfig.MaxFingerDeltaDegPerFrame;
		float num2 = Mathf.Clamp(num, 0f - maxFingerDeltaDegPerFrame, maxFingerDeltaDegPerFrame);
		if (Mathf.Abs(num2) <= 0.1f)
		{
			return;
		}
		if (num2 * ring.BlockDirection > 0f)
		{
			ring.LogDragBlockedOnceForDragLimit($"blockDirection={ring.BlockDirection:0.##} angle={num2:0.##}");
			return;
		}
		ring.BlockDirection = 0f;
		float num3 = ring.AdjustRotationDeltaForDragLimit(num2);
		float num4 = currentRotationZ + num3;
		ringRotateGuardSubstepRotator.ApplySubsteps(num4, out var finalZ, out var totalAppliedDeg);
		ring.SetLastValidRotationZ(finalZ);
		ring.SyncRotateAngleFromTransform();
		ringRotateGuardSubstepRotator.TryConsumePointerSampleForDragFrame(out var fingerDeltaDeg);
		ring.InvokeRotationDragFrame(fingerDeltaDeg, num3, totalAppliedDeg, ringRotateGuardSubstepRotator.LastFingerWorldPosition, ringRotateGuardSubstepRotator.LastFingerScreen);
		if (Mathf.Abs(Mathf.DeltaAngle(finalZ, num4)) >= 0.1f)
		{
			ring.BlockDirection = Mathf.Sign(num2);
			ring.InvokeRotationBlocked(num2);
			ring.LogDragBlockedOnceForDragLimit("overlap blocked");
		}
	}
}
