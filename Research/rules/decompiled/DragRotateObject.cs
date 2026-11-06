using RotateRings.Common;
using RotateRings.GamePlay.Services;
using UnityEngine;

public class DragRotateObject : MonoBehaviour
{
	private const string TAG = "[GamePlay][DragRotateObject] ";

	internal const float RotationBlockEpsilon = 0.1f;

	private const int RotationSearchMaxIterations = 12;

	private const float RotationSearchEpsilon = 0.5f;

	private const float PathSampleMinAngle = 3f;

	private const float PathSampleDualAngle = 8f;

	private const float ProbeAngleMatchEpsilon = 0.001f;

	private static readonly float[] PathSampleDualFracs = new float[2] { 0.33f, 0.66f };

	private static readonly float[] PathSampleMidFracs = new float[1] { 0.5f };

	private static readonly string[] RotationDeltaBucketName = new string[3] { "DragDelta<3", "DragDelta3~8", "DragDelta>=8" };

	[SerializeField]
	protected Rigidbody2D rb2D;

	protected bool isDragging;

	protected Vector3 originalPosition;

	protected float startRotation;

	protected float rotateAngle;

	protected float blockDirection;

	protected float lastValidRotationZ;

	protected float lastProbeAngle;

	protected bool hasProbeCacheForAppliedAngle;

	protected bool rotationProbedThisFrame;

	private RingRotateGuardSubstepRotator rotateGuardSubstepRotator;

	private Vector3 legacyLastWorldPosition;

	protected bool IsCanDragging { get; set; } = true;

	internal bool IsDragging => isDragging;

	internal float StartRotation => startRotation;

	internal float BlockDirection
	{
		get
		{
			return blockDirection;
		}
		set
		{
			blockDirection = value;
		}
	}

	internal Vector3 LegacyLastWorldPosition
	{
		get
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			return legacyLastWorldPosition;
		}
		set
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			legacyLastWorldPosition = value;
		}
	}

	protected virtual void Awake()
	{
		rb2D = ((Component)this).GetComponent<Rigidbody2D>();
		if ((Object)(object)rb2D != (Object)null)
		{
			rb2D.gravityScale = 0f;
		}
	}

	protected void ApplyRotationState(float rotationZ)
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		startRotation = rotationZ;
		lastValidRotationZ = rotationZ;
		originalPosition = ((Component)this).transform.position;
	}

	protected virtual float AdjustRotationDelta(float chaseStepDeg)
	{
		return chaseStepDeg;
	}

	protected virtual void OnRotationGestureBegin()
	{
	}

	protected virtual void OnRotationGestureEnd()
	{
	}

	protected virtual void OnRotationDragFrame(float fingerDeltaDeg, float adjustedDeltaDeg, float ringAppliedDeltaDeg, Vector3 fingerWorldPos, Vector2 fingerScreenPos)
	{
	}

	protected virtual bool IsRotationBlocked()
	{
		return false;
	}

	protected virtual void OnRotationBlocked(float attemptedAngle)
	{
	}

	public float GetGestureRotationDegreesFromStart()
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		return Mathf.Abs(Mathf.DeltaAngle(startRotation, ((Component)this).transform.localEulerAngles.z));
	}

	protected virtual void HandlePointerDown(GameplayPointerData data)
	{
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		using (new ProfilerScope("DragRotateObject.HandlePointerDown"))
		{
			if (IsCanDragging)
			{
				isDragging = true;
				blockDirection = 0f;
				startRotation = ((Component)this).transform.localEulerAngles.z;
				lastValidRotationZ = startRotation;
				if ((Object)(object)rb2D != (Object)null)
				{
					rb2D.bodyType = (RigidbodyType2D)2;
				}
				originalPosition = ((Component)this).transform.position;
				rotateAngle = 0f;
				OnRotationGestureBegin();
				NotifyDragLimitPointerDown(data);
			}
		}
	}

	protected virtual void HandlePointerDrag(GameplayPointerData data)
	{
		using (new ProfilerScope("DragRotateObject.HandlePointerDrag"))
		{
			NotifyDragLimitPointerDrag(data);
		}
	}

	protected virtual void HandlePointerUp(GameplayPointerData data)
	{
		using (new ProfilerScope("DragRotateObject.HandlePointerUp"))
		{
			NotifyDragLimitPointerUp();
			isDragging = false;
			blockDirection = 0f;
			OnRotationGestureEnd();
			if ((Object)(object)rb2D != (Object)null)
			{
				rb2D.bodyType = (RigidbodyType2D)1;
			}
		}
	}

	internal RingRotateGuardSubstepRotator EnsureRotateGuardSubstepRotator()
	{
		return rotateGuardSubstepRotator ?? (rotateGuardSubstepRotator = new RingRotateGuardSubstepRotator(this));
	}

	internal void MarkRotationProbedThisFrame()
	{
		rotationProbedThisFrame = true;
	}

	private static GameBordDragLimitStyleService ResolveDragLimitService()
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

	internal void PinWorldPosition()
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		((Component)this).transform.position = originalPosition;
	}

	internal Vector3 GetDragStartWorldPosition()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		return originalPosition;
	}

	internal float GetCurrentRotationZ()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		return ((Component)this).transform.localEulerAngles.z;
	}

	internal void SyncRotateAngleFromTransform()
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		rotateAngle = Mathf.DeltaAngle(startRotation, ((Component)this).transform.localEulerAngles.z);
	}

	internal void AddRotateAngle(float deltaDeg)
	{
		rotateAngle += deltaDeg;
	}

	internal float GetRotateAngle()
	{
		return rotateAngle;
	}

	internal float AdjustRotationDeltaForDragLimit(float deltaDeg)
	{
		return AdjustRotationDelta(deltaDeg);
	}

	internal void ResetProbeFlagsForChase()
	{
		hasProbeCacheForAppliedAngle = false;
		rotationProbedThisFrame = false;
	}

	internal Vector2 GetRingCenterWorld()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		Vector3 position = ((Component)this).transform.position;
		return new Vector2(position.x, position.y);
	}

	internal void DrawDragDebugLine(GameplayPointerData data)
	{
	}

	internal void SetLastValidRotationZ(float z)
	{
		lastValidRotationZ = z;
	}

	internal void InvokeRotationDragFrame(float fingerDeltaDeg, float adjustedDeltaDeg, float ringAppliedDeltaDeg, Vector3 fingerWorldPos, Vector2 fingerScreenPos)
	{
		//IL_0004: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		OnRotationDragFrame(fingerDeltaDeg, adjustedDeltaDeg, ringAppliedDeltaDeg, fingerWorldPos, fingerScreenPos);
	}

	internal void InvokeRotationBlocked(float attemptedAngle)
	{
		OnRotationBlocked(attemptedAngle);
	}

	internal void LogDragBlockedOnceForDragLimit(string reason)
	{
		LogDragBlockedOnce(reason);
	}

	internal void GuardOnGestureBegin(GameplayPointerData data)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		Vector2 ringCenter = default(Vector2);
		((Vector2)(ref ringCenter))._002Ector(((Component)this).transform.position.x, ((Component)this).transform.position.y);
		EnsureRotateGuardSubstepRotator().OnGestureBegin(data, ringCenter);
	}

	internal void GuardRefreshFingerTarget(GameplayPointerData data)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		Vector2 ringCenter = default(Vector2);
		((Vector2)(ref ringCenter))._002Ector(((Component)this).transform.position.x, ((Component)this).transform.position.y);
		EnsureRotateGuardSubstepRotator().RefreshFingerTarget(ringCenter, data, startRotation);
	}

	private bool ProbeRotationBlockedAt(float z)
	{
		lastProbeAngle = z;
		SetRotationZ(z);
		SyncRotationZ();
		return IsRotationBlocked();
	}

	private bool ProbeRotationPathBlocked(float fromZ, float toZ)
	{
		float num = Mathf.DeltaAngle(fromZ, toZ);
		float num2 = Mathf.Abs(num);
		int num3 = ((!(num2 < 3f)) ? ((num2 < 8f) ? 1 : 2) : 0);
		using (new ProfilerScope(RotationDeltaBucketName[num3]))
		{
			float[] array;
			switch (num3)
			{
			case 0:
				return false;
			default:
				array = PathSampleDualFracs;
				break;
			case 1:
				array = PathSampleMidFracs;
				break;
			}
			float[] array2 = array;
			for (int i = 0; i < array2.Length; i++)
			{
				if (ProbeRotationBlockedAt(fromZ + num * array2[i]))
				{
					return true;
				}
			}
			return false;
		}
	}

	internal float FindLastValidRotationZ(float fromZ, float toZ)
	{
		using (new ProfilerScope("DragRotateObject.FindLastValidRotationZ"))
		{
			if (ProbeRotationPathBlocked(fromZ, toZ) || ProbeRotationBlockedAt(toZ))
			{
				if (ProbeRotationBlockedAt(fromZ))
				{
					hasProbeCacheForAppliedAngle = true;
					return fromZ;
				}
				float num = Mathf.DeltaAngle(fromZ, toZ);
				float num2 = fromZ;
				float num3 = 0f;
				float num4 = 1f;
				try
				{
					for (int i = 0; i < 12; i++)
					{
						if (Mathf.Abs(num) * (num4 - num3) < 0.5f)
						{
							break;
						}
						float num5 = (num3 + num4) * 0.5f;
						float num6 = fromZ + num * num5;
						if (ProbeRotationBlockedAt(num6) || ProbeRotationPathBlocked(fromZ, num6))
						{
							num4 = num5;
							continue;
						}
						num2 = num6;
						num3 = num5;
					}
				}
				finally
				{
					if (Mathf.Abs(Mathf.DeltaAngle(num2, lastProbeAngle)) >= 0.001f)
					{
						ApplyRotationZ(num2);
					}
				}
				hasProbeCacheForAppliedAngle = Mathf.Abs(Mathf.DeltaAngle(num2, lastProbeAngle)) < 0.001f;
				return num2;
			}
			hasProbeCacheForAppliedAngle = Mathf.Abs(Mathf.DeltaAngle(toZ, lastProbeAngle)) < 0.001f;
			return toZ;
		}
	}

	protected void SetRotationZ(float z)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		Vector3 localEulerAngles = ((Component)this).transform.localEulerAngles;
		((Component)this).transform.localEulerAngles = new Vector3(localEulerAngles.x, localEulerAngles.y, z);
	}

	protected void SyncRotationZ()
	{
		Physics2D.SyncTransforms();
	}

	internal void ApplyRotationZ(float z)
	{
		SetRotationZ(z);
		SyncRotationZ();
	}

	protected virtual void OnCollisionEnter2D(Collision2D other)
	{
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		using (new ProfilerScope("DragRotateObject.OnCollisionEnter2D"))
		{
			if (isDragging && rotateAngle != 0f)
			{
				float z = ((Component)this).transform.localEulerAngles.z;
				float num = lastValidRotationZ;
				blockDirection = Mathf.Sign(Mathf.DeltaAngle(num, z));
				if (Mathf.Approximately(blockDirection, 0f))
				{
					blockDirection = Mathf.Sign(rotateAngle);
				}
				ApplyRotationZ(num);
				rotateAngle = Mathf.DeltaAngle(startRotation, num);
			}
		}
	}

	protected void LogDragBlockedOnce(string reason)
	{
	}
}
