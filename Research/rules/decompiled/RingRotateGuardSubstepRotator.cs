using UnityEngine;

public sealed class RingRotateGuardSubstepRotator
{
	private const float MinWorldMoveSqr = 0.0009f;

	private const int MaxSubstepIterations = 16;

	private readonly DragRotateObject host;

	private float gestureStartFingerAngleDeg;

	private float lastFingerAngleDeg;

	private bool hasLastFingerAngle;

	private Vector3 lastWorldPosition;

	private float cachedRawTargetZ;

	private Vector3 lastPointerWorldForCallback;

	private float lastRefreshFingerDeltaDeg;

	private Vector2 lastFingerScreen;

	private bool pointerSamplePending;

	public Vector3 LastFingerWorldPosition => lastPointerWorldForCallback;

	public Vector2 LastFingerScreen => lastFingerScreen;

	public float CachedRawTargetZ => cachedRawTargetZ;

	public RingRotateGuardSubstepRotator(DragRotateObject dragRotateObject)
	{
		host = dragRotateObject;
	}

	public void OnGestureBegin(GameplayPointerData data, Vector2 ringCenter)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		lastWorldPosition = data.WorldPosition;
		lastPointerWorldForCallback = data.WorldPosition;
		lastFingerScreen = data.ScreenPosition;
		lastRefreshFingerDeltaDeg = 0f;
		pointerSamplePending = true;
		gestureStartFingerAngleDeg = FingerAngleDeg(ringCenter, data.WorldPosition);
		lastFingerAngleDeg = gestureStartFingerAngleDeg;
		hasLastFingerAngle = true;
		cachedRawTargetZ = ((Component)host).transform.localEulerAngles.z;
	}

	public void RefreshFingerTarget(Vector2 ringCenter, GameplayPointerData data, float startRotationZ)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		lastFingerScreen = data.ScreenPosition;
		lastPointerWorldForCallback = data.WorldPosition;
		pointerSamplePending = true;
		float num = 0f;
		Vector3 worldPosition = data.WorldPosition;
		Vector3 val = worldPosition - lastWorldPosition;
		if (((Vector3)(ref val)).sqrMagnitude < 0.0009f)
		{
			lastRefreshFingerDeltaDeg = 0f;
			return;
		}
		float num2 = FingerAngleDeg(ringCenter, worldPosition);
		if (hasLastFingerAngle)
		{
			num = Mathf.DeltaAngle(lastFingerAngleDeg, num2);
		}
		float num3 = Mathf.DeltaAngle(gestureStartFingerAngleDeg, num2);
		cachedRawTargetZ = startRotationZ + num3;
		lastFingerAngleDeg = num2;
		hasLastFingerAngle = true;
		lastWorldPosition = worldPosition;
		lastRefreshFingerDeltaDeg = num;
	}

	public bool TryConsumePointerSampleForDragFrame(out float fingerDeltaDeg)
	{
		if (!pointerSamplePending)
		{
			fingerDeltaDeg = 0f;
			return false;
		}
		pointerSamplePending = false;
		fingerDeltaDeg = lastRefreshFingerDeltaDeg;
		return true;
	}

	public void ApplySubsteps(float targetZ, out float finalZ, out float totalAppliedDeg)
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		float num = 0.1f;
		float num2 = ((Component)host).transform.localEulerAngles.z;
		totalAppliedDeg = 0f;
		float num3 = Mathf.DeltaAngle(num2, targetZ);
		if (Mathf.Abs(num3) <= num)
		{
			finalZ = num2;
			return;
		}
		float maxRotationStepDeg = RingRotateGuardConfig.MaxRotationStepDeg;
		float maxFingerDeltaDegPerFrame = RingRotateGuardConfig.MaxFingerDeltaDegPerFrame;
		if (Mathf.Abs(num3) <= maxFingerDeltaDegPerFrame)
		{
			ApplyOneRotationStep(num2, targetZ, ref totalAppliedDeg, out finalZ);
			return;
		}
		float num4 = maxRotationStepDeg;
		int num5 = 0;
		while (Mathf.Abs(num3) > num && num5 < 16)
		{
			num5++;
			float num6 = Mathf.Min(Mathf.Abs(num3), num4);
			float num7 = num2 + Mathf.Sign(num3) * num6;
			ApplyOneRotationStep(num2, num7, ref totalAppliedDeg, out var safeZ);
			num2 = safeZ;
			num3 = Mathf.DeltaAngle(num2, targetZ);
			if (Mathf.Abs(Mathf.DeltaAngle(safeZ, num7)) >= num)
			{
				break;
			}
			num4 = Mathf.Min(num4 * 2f, maxFingerDeltaDegPerFrame);
		}
		finalZ = num2;
	}

	private void ApplyOneRotationStep(float fromZ, float stepTargetZ, ref float totalAppliedDeg, out float safeZ)
	{
		host.MarkRotationProbedThisFrame();
		safeZ = host.FindLastValidRotationZ(fromZ, stepTargetZ);
		totalAppliedDeg += Mathf.DeltaAngle(fromZ, safeZ);
	}

	private static float FingerAngleDeg(Vector2 center, Vector3 fingerWorld)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		Vector2 val = new Vector2(fingerWorld.x, fingerWorld.y) - center;
		return Mathf.Atan2(val.y, val.x) * 57.29578f;
	}
}
