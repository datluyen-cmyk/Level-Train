using RotateRings;
using RotateRings.GamePlay.Events;
using RotateRings.GamePlay.Services;
using UnityEngine;

public class DraggableObjectWithLimitRange : DraggableObject
{
	public enum LockDirectionType
	{
		VectorUp,
		VectorLeft,
		Custom
	}

	public const float CastSkinWidth = 0.01f;

	private const float EffectiveMoveThresholdCap = 0.03f;

	[SerializeField]
	protected LockDirectionType lockDirectionType;

	[SerializeField]
	protected bool lockLeft;

	[SerializeField]
	protected bool lockRight;

	[SerializeField]
	protected bool lockUp;

	[SerializeField]
	protected bool lockDown;

	protected Vector2 originPos;

	private Collider2D dragCollider;

	private ContactFilter2D castContactFilter;

	private readonly RaycastHit2D[] castHits = (RaycastHit2D[])(object)new RaycastHit2D[8];

	private DragTranslationStopRollback stopRollback;

	private DragTranslationOverlapEscape overlapEscape;

	private readonly PullMoveGuardSubstepMover translationSubstepMover = new PullMoveGuardSubstepMover();

	private Vector2 dragLockedTarget;

	private const float SpeedSampleSmoothing = 0.35f;

	private const float SpeedSampleStaleSeconds = 0.08f;

	private float gestureStartTime;

	private float speedSampleTime;

	private Vector2 speedSamplePosition;

	private float smoothAxisSpeed;

	protected Vector2 lastMoveDirection;

	protected float blockMotionSign;

	protected float persistedBlockMotionSign;

	protected Vector3 dragStartPosition;

	public float BlockMotionSign
	{
		get
		{
			return blockMotionSign;
		}
		set
		{
			blockMotionSign = value;
		}
	}

	public Rigidbody2D Rb => rb;

	public Collider2D DragCollider => dragCollider;

	internal bool BeingHeldForDragLimit => beingHeld;

	internal bool IsVelocityDragMode => dragMode == DragMode.Velocity;

	internal bool HasMainCameraForDragLimit => (Object)(object)mainCamera != (Object)null;

	protected void ApplyDragOriginPosition(Vector2 position)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		originPos = position;
	}

	public float GetGestureTranslationWorldFromStart()
	{
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		Vector2 val = (((Object)(object)rb != (Object)null) ? rb.position : Vector2.op_Implicit(((Component)this).transform.position));
		return Vector2.Distance(new Vector2(dragStartPosition.x, dragStartPosition.y), val);
	}

	public float GetGestureAxisOffsetFromStart()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		Vector2 lockedDragDirection = GetLockedDragDirection();
		if ((Object)(object)rb == (Object)null || ((Vector2)(ref lockedDragDirection)).sqrMagnitude < 1E-08f)
		{
			return 0f;
		}
		((Vector2)(ref lockedDragDirection)).Normalize();
		Vector2 val = default(Vector2);
		((Vector2)(ref val))._002Ector(dragStartPosition.x, dragStartPosition.y);
		return Vector2.Dot(rb.position - val, lockedDragDirection);
	}

	public float GetGestureDurationSeconds()
	{
		if (gestureStartTime <= 0f)
		{
			return 0f;
		}
		return Mathf.Max(0f, Time.unscaledTime - gestureStartTime);
	}

	public float GetAxisSpeedAtRelease()
	{
		if (Time.unscaledTime - speedSampleTime > 0.08f)
		{
			return 0f;
		}
		return smoothAxisSpeed;
	}

	internal float MoveEscapeStep(float distance, Vector2 direction)
	{
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_013d: Unknown result type (might be due to invalid IL or missing references)
		//IL_013e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0140: Unknown result type (might be due to invalid IL or missing references)
		//IL_0145: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)rb == (Object)null || (Object)(object)dragCollider == (Object)null || distance <= 0f || ((Vector2)(ref direction)).sqrMagnitude < 1E-08f)
		{
			return 0f;
		}
		Vector2 normalized = ((Vector2)(ref direction)).normalized;
		Vector2 position = rb.position;
		float num = distance;
		int num2 = dragCollider.Cast(normalized, castContactFilter, castHits, num);
		if (num2 > 0)
		{
			float num3 = num;
			bool flag = false;
			for (int i = 0; i < num2 && i < castHits.Length; i++)
			{
				if ((Object)(object)((RaycastHit2D)(ref castHits[i])).collider != (Object)null && ((RaycastHit2D)(ref castHits[i])).distance < num3 && !IsEscapeTargetCollider(((RaycastHit2D)(ref castHits[i])).collider) && IsCastHitAllowed(((RaycastHit2D)(ref castHits[i])).collider))
				{
					num3 = ((RaycastHit2D)(ref castHits[i])).distance;
					flag = true;
				}
			}
			if (flag)
			{
				num = Mathf.Max(0f, num3 - 0.01f);
			}
		}
		if (num <= 1E-06f)
		{
			return 0f;
		}
		rb.position = position + normalized * num;
		return num;
	}

	internal void SampleDragSpeed()
	{
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)rb == (Object)null)
		{
			return;
		}
		float unscaledTime = Time.unscaledTime;
		float num = unscaledTime - speedSampleTime;
		if (!(num <= 0.0001f))
		{
			Vector2 lockedDragDirection = GetLockedDragDirection();
			Vector2 position = rb.position;
			if (((Vector2)(ref lockedDragDirection)).sqrMagnitude > 1E-08f)
			{
				((Vector2)(ref lockedDragDirection)).Normalize();
				float num2 = Vector2.Dot(position - speedSamplePosition, lockedDragDirection) / num;
				smoothAxisSpeed = Mathf.Lerp(smoothAxisSpeed, num2, 0.35f);
			}
			speedSampleTime = unscaledTime;
			speedSamplePosition = position;
		}
	}

	public virtual Vector2 GetLockedDragDirection()
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		if (lockDirectionType == LockDirectionType.VectorUp || lockDirectionType == LockDirectionType.VectorLeft)
		{
			return ResolveLockedDragDirection(lockDirectionType);
		}
		bool flag = lockLeft && lockRight;
		bool flag2 = lockUp && lockDown;
		if (!flag && flag2)
		{
			if (!lockRight)
			{
				return Vector2.right;
			}
			if (!lockLeft)
			{
				return Vector2.left;
			}
		}
		if (flag && !flag2)
		{
			if (!lockUp)
			{
				return Vector2.up;
			}
			if (!lockDown)
			{
				return Vector2.down;
			}
		}
		return Vector2.up;
	}

	protected Vector2 ResolveLockedDragDirection(LockDirectionType type)
	{
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		if (type == LockDirectionType.VectorUp)
		{
			Vector2 val = Vector2.op_Implicit(((Component)this).transform.up);
			if (!(((Vector2)(ref val)).sqrMagnitude > 0f))
			{
				return Vector2.up;
			}
			return ((Vector2)(ref val)).normalized;
		}
		Vector2 val2 = Vector2.op_Implicit(-((Component)this).transform.right);
		if (!(((Vector2)(ref val2)).sqrMagnitude > 0f))
		{
			return Vector2.up;
		}
		return ((Vector2)(ref val2)).normalized;
	}

	protected override void Start()
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		base.Start();
		originPos = Vector2.op_Implicit(((Component)this).transform.position);
		if ((Object)(object)rb != (Object)null)
		{
			dragCollider = ((Component)rb).GetComponent<Collider2D>();
		}
		castContactFilter.useTriggers = false;
		castContactFilter.useLayerMask = true;
		castContactFilter.layerMask = GameConst.OverlapAreaLayerMask;
		stopRollback = new DragTranslationStopRollback(rb, ((Component)this).transform, dragCollider, castContactFilter, castHits, 0.01f, IsCastHitAllowed);
		overlapEscape = new DragTranslationOverlapEscape(this);
	}

	public override void HandlePointerDown(GameplayPointerData data)
	{
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		blockMotionSign = 0f;
		persistedBlockMotionSign = 0f;
		dragStartPosition = (Vector3)(((Object)(object)rb != (Object)null) ? new Vector3(rb.position.x, rb.position.y, ((Component)this).transform.position.z) : ((Component)this).transform.position);
		ResetDragSpeedSample();
		base.HandlePointerDown(data);
		NotifyDragLimitPointerDown(data);
	}

	private void ResetDragSpeedSample()
	{
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		gestureStartTime = Time.unscaledTime;
		speedSampleTime = Time.unscaledTime;
		speedSamplePosition = (((Object)(object)rb != (Object)null) ? rb.position : Vector2.op_Implicit(((Component)this).transform.position));
		smoothAxisSpeed = 0f;
	}

	public override void HandlePointerUp(GameplayPointerData data)
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		stopRollback?.RollbackAfterStop(GetLockedDragDirection(), dragStartPosition, persistedBlockMotionSign);
		NotifyDragLimitPointerUp();
		base.HandlePointerUp(data);
		lastMoveDirection = Vector2.zero;
		blockMotionSign = 0f;
		persistedBlockMotionSign = 0f;
	}

	public override void HandlePointerDrag(GameplayPointerData data)
	{
		NotifyDragLimitPointerDrag(data);
		SampleDragSpeed();
	}

	internal void GuardChaseTowardDragTarget()
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		if (TryGetAxisGapToTarget(dragLockedTarget, out var axisMove, out var lockedDir))
		{
			float moveBudgetThisTickWorld = PullMoveGuardConfig.GetMoveBudgetThisTickWorld();
			axisMove = Mathf.Clamp(axisMove, 0f - moveBudgetThisTickWorld, moveBudgetThisTickWorld);
			if (!(Mathf.Abs(axisMove) < 1E-06f))
			{
				MoveTowardAxisGap(axisMove, lockedDir, moveBudgetThisTickWorld);
			}
		}
	}

	internal void LegacyMoveToward(Vector2 target)
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_011a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0235: Unknown result type (might be due to invalid IL or missing references)
		//IL_0236: Unknown result type (might be due to invalid IL or missing references)
		//IL_023a: Unknown result type (might be due to invalid IL or missing references)
		//IL_023f: Unknown result type (might be due to invalid IL or missing references)
		//IL_01be: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_018e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0193: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f2: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)rb == (Object)null)
		{
			return;
		}
		rb.linearVelocity = Vector2.zero;
		Vector2 position = rb.position;
		Vector2 val = target - position;
		Vector2 lockedDragDirection = GetLockedDragDirection();
		if (((Vector2)(ref lockedDragDirection)).sqrMagnitude < Mathf.Epsilon)
		{
			return;
		}
		((Vector2)(ref lockedDragDirection)).Normalize();
		float num = Vector2.Dot(val, lockedDragDirection);
		float num2 = Mathf.Min(moveThreshold, 0.03f);
		if (Mathf.Abs(num) < num2)
		{
			return;
		}
		float num3 = Mathf.Sign(num);
		if (num3 * blockMotionSign > 0f)
		{
			return;
		}
		blockMotionSign = 0f;
		Vector2 val2 = lockedDragDirection * num3;
		if (TryGetSelfBombTranslationBlock(num3, out var blocker, out var contact))
		{
			blockMotionSign = num3;
			persistedBlockMotionSign = num3;
			NotifySelfBombTranslationBlocked(blocker, contact, val2);
			return;
		}
		float num4 = Mathf.Abs(num);
		lastMoveDirection = val2;
		if ((Object)(object)dragCollider != (Object)null)
		{
			int num5 = dragCollider.Cast(val2, castContactFilter, castHits, num4);
			if (num5 > 0)
			{
				float num6 = num4;
				RaycastHit2D hit = default(RaycastHit2D);
				bool flag = false;
				for (int i = 0; i < num5; i++)
				{
					if ((Object)(object)((RaycastHit2D)(ref castHits[i])).collider != (Object)null && ((RaycastHit2D)(ref castHits[i])).distance < num6 && IsCastHitAllowed(((RaycastHit2D)(ref castHits[i])).collider))
					{
						num6 = ((RaycastHit2D)(ref castHits[i])).distance;
						hit = castHits[i];
						flag = true;
					}
				}
				if (flag)
				{
					DragTranslationOverlapEscape dragTranslationOverlapEscape = overlapEscape;
					if (dragTranslationOverlapEscape != null && dragTranslationOverlapEscape.TryEscape(((RaycastHit2D)(ref hit)).collider, num6, val2, num, position))
					{
						return;
					}
				}
				if (flag && num6 + 0.01f < num4)
				{
					blockMotionSign = num3;
					persistedBlockMotionSign = num3;
					NotifyPhysicsDragBlocked(hit, val2);
				}
				else
				{
					NotifyDragStopRearmed();
				}
				num4 = Mathf.Max(0f, num6 - 0.01f);
			}
			else
			{
				NotifyDragStopRearmed();
			}
		}
		else
		{
			NotifyDragStopRearmed();
		}
		if (!(num4 <= 1E-06f))
		{
			rb.position = position + val2 * num4;
		}
	}

	private bool TryGetAxisGapToTarget(Vector2 target, out float axisMove, out Vector2 lockedDir)
	{
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		axisMove = 0f;
		lockedDir = GetLockedDragDirection();
		if ((Object)(object)rb == (Object)null || ((Vector2)(ref lockedDir)).sqrMagnitude < Mathf.Epsilon)
		{
			return false;
		}
		((Vector2)(ref lockedDir)).Normalize();
		axisMove = Vector2.Dot(target - rb.position, lockedDir);
		float num = Mathf.Min(moveThreshold, 0.03f);
		return Mathf.Abs(axisMove) >= num;
	}

	private void MoveTowardAxisGap(float axisMove, Vector2 lockedDir, float maxFrameMoveWorld)
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		rb.linearVelocity = Vector2.zero;
		float moveDirection = Mathf.Sign(axisMove);
		if (moveDirection * blockMotionSign > 0f)
		{
			return;
		}
		blockMotionSign = 0f;
		Vector2 direction = lockedDir * moveDirection;
		if (TryGetSelfBombTranslationBlock(moveDirection, out var blocker, out var contact))
		{
			blockMotionSign = moveDirection;
			persistedBlockMotionSign = moveDirection;
			NotifySelfBombTranslationBlocked(blocker, contact, direction);
			return;
		}
		float totalDist = translationSubstepMover.ClampFrameMoveDistance(Mathf.Abs(axisMove), maxFrameMoveWorld);
		lastMoveDirection = direction;
		translationSubstepMover.MoveInSteps(totalDist, (float stepDist) => TryMoveOneStep(stepDist, direction, moveDirection));
	}

	private float TryMoveOneStep(float stepDist, Vector2 direction, float moveDirection)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_0166: Unknown result type (might be due to invalid IL or missing references)
		//IL_0167: Unknown result type (might be due to invalid IL or missing references)
		//IL_0169: Unknown result type (might be due to invalid IL or missing references)
		//IL_016e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ee: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_011f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0121: Unknown result type (might be due to invalid IL or missing references)
		Vector2 position = rb.position;
		float num = stepDist;
		float axisMove = moveDirection * stepDist;
		if ((Object)(object)dragCollider != (Object)null)
		{
			int num2 = dragCollider.Cast(direction, castContactFilter, castHits, num);
			if (num2 > 0)
			{
				float num3 = num;
				RaycastHit2D hit = default(RaycastHit2D);
				bool flag = false;
				for (int i = 0; i < num2; i++)
				{
					if ((Object)(object)((RaycastHit2D)(ref castHits[i])).collider != (Object)null && ((RaycastHit2D)(ref castHits[i])).distance < num3 && IsCastHitAllowed(((RaycastHit2D)(ref castHits[i])).collider))
					{
						num3 = ((RaycastHit2D)(ref castHits[i])).distance;
						hit = castHits[i];
						flag = true;
					}
				}
				if (flag)
				{
					DragTranslationOverlapEscape dragTranslationOverlapEscape = overlapEscape;
					if (dragTranslationOverlapEscape != null && dragTranslationOverlapEscape.TryEscape(((RaycastHit2D)(ref hit)).collider, num3, direction, axisMove, position))
					{
						return 0f;
					}
				}
				if (flag && num3 + 0.01f < num)
				{
					blockMotionSign = moveDirection;
					persistedBlockMotionSign = moveDirection;
					NotifyPhysicsDragBlocked(hit, direction);
				}
				else
				{
					NotifyDragStopRearmed();
				}
				num = Mathf.Max(0f, num3 - 0.01f);
			}
			else
			{
				NotifyDragStopRearmed();
			}
		}
		else
		{
			NotifyDragStopRearmed();
		}
		if (num <= 1E-06f)
		{
			return 0f;
		}
		rb.position = position + direction * num;
		return num;
	}

	protected virtual bool IsCastHitAllowed(Collider2D hitCollider)
	{
		return true;
	}

	protected virtual bool IsEscapeTargetCollider(Collider2D targetCollider)
	{
		return false;
	}

	internal int CastForEscape(Vector2 direction, float distance, RaycastHit2D[] results)
	{
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)dragCollider == (Object)null || results == null || distance <= 0f || ((Vector2)(ref direction)).sqrMagnitude < 1E-08f)
		{
			return 0;
		}
		return dragCollider.Cast(((Vector2)(ref direction)).normalized, castContactFilter, results, distance);
	}

	protected virtual bool TryGetSelfBombTranslationBlock(float moveDirection, out Collider2D blocker, out Vector2 contact)
	{
		//IL_0004: Unknown result type (might be due to invalid IL or missing references)
		blocker = null;
		contact = default(Vector2);
		return false;
	}

	protected virtual void NotifySelfBombTranslationBlocked(Collider2D blocker, Vector2 contact, Vector2 direction)
	{
	}

	protected virtual void NotifyPhysicsDragBlocked(RaycastHit2D hit, Vector2 direction)
	{
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		RingDragCollisionEventService ringDragCollisionEventService = SingletonComponent<GameplayManager>.Instance?.RingDragCollisionEventService;
		if (ringDragCollisionEventService != null && !((Object)(object)((RaycastHit2D)(ref hit)).collider == (Object)null))
		{
			Vector2 lockedDragDirection = GetLockedDragDirection();
			float num = Mathf.Sign(Vector2.Dot(direction, lockedDragDirection));
			if (Mathf.Approximately(num, 0f))
			{
				num = 1f;
			}
			ringDragCollisionEventService.SendStopEvent((Component)(object)this, ((RaycastHit2D)(ref hit)).collider, ((RaycastHit2D)(ref hit)).point, num, RingDragMoveKind.Translate);
		}
	}

	public void NotifyDragStopRearmed()
	{
		SingletonComponent<GameplayManager>.Instance?.RingDragCollisionEventService?.NotifyStopRearmed();
	}

	internal Vector2 ComputeLockedTargetFromPointer(GameplayPointerData data)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		Vector3 worldPosition = data.WorldPosition;
		Vector2 target = default(Vector2);
		((Vector2)(ref target))._002Ector(worldPosition.x - holdPos.x, worldPosition.y - holdPos.y);
		return ApplyDirectionLock(target);
	}

	internal void SetDragLockedTarget(Vector2 target)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		dragLockedTarget = target;
	}

	internal void ApplyInstanceDrag(GameplayPointerData data)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		Vector3 worldPosition = data.WorldPosition;
		Vector2 val = default(Vector2);
		((Vector2)(ref val))._002Ector(worldPosition.x - holdPos.x, worldPosition.y - holdPos.y);
		((Component)this).transform.position = new Vector3(val.x, val.y + yOffset, 0f);
	}

	private GameBordDragLimitStyleService ResolveDragLimitService()
	{
		return SingletonComponent<GameplayManager>.Instance?.DragLimitStyle;
	}

	private void NotifyDragLimitPointerDown(GameplayPointerData data)
	{
		if (this is IGameplayObject target)
		{
			GameBordDragLimitStyleService gameBordDragLimitStyleService = ResolveDragLimitService();
			if (gameBordDragLimitStyleService != null)
			{
				gameBordDragLimitStyleService.OnPointerDown(target, data);
			}
			else
			{
				new DragDefaultLimitStyleHandler().OnPointerDown(target, data);
			}
		}
	}

	private void NotifyDragLimitPointerDrag(GameplayPointerData data)
	{
		if (this is IGameplayObject target)
		{
			GameBordDragLimitStyleService gameBordDragLimitStyleService = ResolveDragLimitService();
			if (gameBordDragLimitStyleService != null)
			{
				gameBordDragLimitStyleService.OnPointerDrag(target, data);
			}
			else
			{
				new DragDefaultLimitStyleHandler().OnPointerDrag(target, data);
			}
		}
	}

	private void NotifyDragLimitPointerUp()
	{
		if (this is IGameplayObject target)
		{
			ResolveDragLimitService()?.OnPointerUp(target);
		}
	}

	private Vector2 ApplyDirectionLock(Vector2 target)
	{
		//IL_00c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fe: Unknown result type (might be due to invalid IL or missing references)
		//IL_00df: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		if (lockDirectionType == LockDirectionType.Custom)
		{
			if ((lockLeft && target.x < originPos.x) || (lockRight && target.x > originPos.x))
			{
				target.x = originPos.x;
			}
			if ((lockUp && target.y > originPos.y) || (lockDown && target.y < originPos.y))
			{
				target.y = originPos.y;
			}
			return target;
		}
		Vector2 val = (((Object)(object)rb != (Object)null) ? rb.position : Vector2.op_Implicit(((Component)this).transform.position));
		Vector2 lockedDragDirection = GetLockedDragDirection();
		if (((Vector2)(ref lockedDragDirection)).sqrMagnitude < Mathf.Epsilon)
		{
			return target;
		}
		((Vector2)(ref lockedDragDirection)).Normalize();
		float num = Vector2.Dot(target - val, lockedDragDirection);
		return val + lockedDragDirection * num;
	}

	protected void ResolveReleaseOverlap()
	{
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)rb == (Object)null || (Object)(object)dragCollider == (Object)null || lastMoveDirection == Vector2.zero)
		{
			return;
		}
		Vector2 position = rb.position;
		Vector2 val = -lastMoveDirection;
		float num = 0.02f;
		for (int i = 0; i < 20; i++)
		{
			float num2 = ((i == 0) ? num : ((float)i * 0.05f + 0.01f));
			if (dragCollider.Cast(val, castContactFilter, castHits, num2) == 0)
			{
				Vector2 val2 = position + val * (num2 - 0.01f);
				rb.MovePosition(val2);
				break;
			}
		}
	}
}
