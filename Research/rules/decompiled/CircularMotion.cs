using System;
using UnityEngine;

public class CircularMotion : GuidMotion
{
	private const float DefaultAngularSpeed = 90f;

	private const float PositionEpsilon = 0.0001f;

	private const int WarnFrameInterval = 60;

	private const string TAG = "[RotateRing][CircularMotion] ";

	[SerializeField]
	private Transform center;

	[SerializeField]
	private CircularMotionMode motionMode;

	[SerializeField]
	private RotationDirection direction = RotationDirection.Clockwise;

	[SerializeField]
	private float totalRotation = 360f;

	[SerializeField]
	private float rotationDuration;

	[SerializeField]
	private float pauseDuration;

	[SerializeField]
	private bool moveAfterStart;

	private float radius;

	private float startAngle;

	private float currentAngle;

	private float totalRotationRad;

	private float effectiveDegreesPerSecond;

	private float initialStartAngle;

	private Quaternion initialLocalRotation;

	private float originalTotalRotation;

	private int lastWarnFrame = -1;

	public float Radius => radius;

	public CircularMotionMode MotionMode => motionMode;

	public RotationDirection Direction => direction;

	public Transform Center => center;

	public float TotalRotation => totalRotation;

	public float OriginalTotalRotation => originalTotalRotation;

	public float RotationDuration => rotationDuration;

	public float PauseDuration => pauseDuration;

	public float CurrentAngle => currentAngle;

	public void SetMotionMode(CircularMotionMode mode)
	{
		motionMode = mode;
	}

	public void ConfigureMotion(RotationDirection newDirection, float newTotalRotation, float newRotationDuration, float newPauseDuration)
	{
		direction = newDirection;
		totalRotation = newTotalRotation;
		rotationDuration = newRotationDuration;
		pauseDuration = newPauseDuration;
	}

	public void SetInitialLocalRotationZ(float zAngle)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		initialLocalRotation = Quaternion.Euler(0f, 0f, zAngle);
		if (motionMode == CircularMotionMode.SelfRotate)
		{
			((Component)this).transform.localRotation = initialLocalRotation;
		}
	}

	public override void StartMove()
	{
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		if (motionMode == CircularMotionMode.Orbit && (Object)(object)center == (Object)null)
		{
			NotifyMotionCompleted();
			base.StopMove();
			return;
		}
		PrepareMotionCycle();
		if (motionMode == CircularMotionMode.Orbit)
		{
			startAngle = initialStartAngle;
			radius = Vector2.Distance(Vector2.op_Implicit(((Component)this).transform.position), Vector2.op_Implicit(center.position));
		}
		UpdateMotionPose();
		base.StartMove();
	}

	public override void StopMove()
	{
		base.StopMove();
	}

	public override void Finish()
	{
		base.Finish();
	}

	private void Awake()
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		CaptureInitialMotionState();
		initialLocalRotation = ((Component)this).transform.localRotation;
		originalTotalRotation = Mathf.Abs(totalRotation);
	}

	private void Start()
	{
		if (moveAfterStart)
		{
			StartMove();
		}
	}

	private void Update()
	{
		if (!base.IsMoving)
		{
			if (motionMode == CircularMotionMode.Orbit && (Object)(object)center == (Object)null)
			{
				WarnOrbitEarlyExit();
			}
		}
		else
		{
			UpdateRotating();
		}
	}

	private void UpdateMotionPose()
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		if (motionMode == CircularMotionMode.SelfRotate)
		{
			((Component)this).transform.localRotation = initialLocalRotation * Quaternion.Euler(0f, 0f, currentAngle * 57.29578f);
		}
		else
		{
			UpdatePosition();
		}
	}

	private void PrepareMotionCycle()
	{
		totalRotationRad = Mathf.Abs(totalRotation) * (MathF.PI / 180f);
		effectiveDegreesPerSecond = ((totalRotationRad > 0f && rotationDuration > 0f) ? (Mathf.Abs(totalRotation) / rotationDuration) : 90f);
		currentAngle = 0f;
	}

	private void CaptureInitialMotionState()
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		if (!((Object)(object)center == (Object)null))
		{
			Vector2 val = Vector2.op_Implicit(((Component)this).transform.position - center.position);
			initialStartAngle = Mathf.Atan2(val.y, val.x);
		}
	}

	private void UpdateRotating()
	{
		currentAngle += (float)direction * effectiveDegreesPerSecond * (MathF.PI / 180f) * Time.deltaTime;
		bool flag = totalRotationRad <= 0f;
		if (!flag)
		{
			flag = ((direction == RotationDirection.CounterClockwise) ? (currentAngle >= totalRotationRad) : (currentAngle <= 0f - totalRotationRad));
		}
		if (!flag)
		{
			UpdateMotionPose();
			return;
		}
		currentAngle = (float)direction * totalRotationRad;
		UpdateMotionPose();
		NotifyMotionCompleted();
		base.StopMove();
	}

	public void InitFromPosition(Vector3 position)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		Vector2 val = Vector2.op_Implicit(position - center.position);
		radius = ((Vector2)(ref val)).magnitude;
		startAngle = Mathf.Atan2(val.y, val.x);
		initialStartAngle = startAngle;
		currentAngle = 0f;
		UpdateMotionPose();
	}

	public void SetCenter(Transform newCenter)
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		center = newCenter;
		CaptureInitialMotionState();
		InitFromPosition(((Component)this).transform.position);
	}

	private void UpdatePosition()
	{
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)center == (Object)null)
		{
			WarnOrbitEarlyExit();
			return;
		}
		float num = startAngle + currentAngle;
		((Component)this).transform.position = new Vector3(center.position.x + Mathf.Cos(num) * radius, center.position.y + Mathf.Sin(num) * radius, ((Component)this).transform.position.z);
	}

	private void WarnOrbitEarlyExit()
	{
		if (Time.frameCount - lastWarnFrame >= 60)
		{
			lastWarnFrame = Time.frameCount;
		}
	}
}
