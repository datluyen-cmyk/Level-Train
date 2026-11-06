using System;
using System.Collections.Generic;
using UnityEngine;
using UnlockRing.Core.Domain;
using XingYun.Rendering;
using XingYun.RingGame;

namespace RotateRings.GamePlay.Objects;

public class RingPaletteLockGapObject : MonoBehaviour, IGameplaySolveReason, IGameplayObject, IGameplayPhysicColliders
{
	private GameplaySolveReason solveReason;

	private PartId passiveByPartId;

	private PartId stepBatchPartId;

	private const string PaletteLockGapPartIdSuffix = "#palette_lock_gap";

	internal static readonly Color GapFillerVertexColor = Color32.op_Implicit(new Color32((byte)204, (byte)204, (byte)204, byte.MaxValue));

	private const float LockLocalRotationZDeg = 270f;

	private const string LockSolveFragmentEffectPrefabName = "eff_cycle_boom_style_fragment_lock";

	[SerializeField]
	private GameplayPartData partData;

	[SerializeField]
	private PolygonCollider2D bodyCollider;

	[SerializeField]
	private Transform lockTransform;

	[SerializeField]
	private RingSegmentController lockSegment;

	[SerializeField]
	private RingStyleLineExtraQuad lockHeartQuad;

	[NonSerialized]
	private int lockCount;

	[NonSerialized]
	private bool isCurrentPaletteLockKey;

	[NonSerialized]
	private RingObject hostRing;

	[NonSerialized]
	private Action<IGameplayObject> onSolved;

	[NonSerialized]
	private Collider2D[] blockColliders;

	public GameplaySolveReason SolveReason => solveReason;

	public PartId PassiveByPartId => passiveByPartId;

	public PartId StepBatchPartId => stepBatchPartId;

	public RingObject HostRing => hostRing;

	public PolygonCollider2D BodyCollider => bodyCollider;

	public bool IsSolved { get; private set; }

	public GameplayPartData PartData
	{
		get
		{
			return partData;
		}
		set
		{
			partData = value;
		}
	}

	public int LockCount
	{
		get
		{
			return lockCount;
		}
		set
		{
			lockCount = value;
		}
	}

	public bool IsCurrentPaletteLock => false;

	public bool IsCurrentPaletteLockKey
	{
		get
		{
			return isCurrentPaletteLockKey;
		}
		set
		{
			isCurrentPaletteLockKey = value;
		}
	}

	public void ClearSolveReason()
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		solveReason = GameplaySolveReason.None;
		passiveByPartId = default(PartId);
		stepBatchPartId = default(PartId);
	}

	public void SetSolveReason(GameplaySolveReason reason, PartId passiveByPartId, PartId stepBatchPartId)
	{
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		solveReason = reason;
		this.passiveByPartId = (PartId)(reason.IsPassive() ? passiveByPartId : default(PartId));
		this.stepBatchPartId = stepBatchPartId;
	}

	internal Vector3 GetLockHeartWorldPosition()
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)lockHeartQuad == (Object)null)
		{
			return ((Component)this).transform.position;
		}
		Transform transform = ((Component)lockHeartQuad).transform;
		float num = (((Object)(object)lockSegment != (Object)null && lockSegment.CenterRadius > 0.0001f) ? lockSegment.CenterRadius : 0f);
		Vector3 lossyScale = transform.lossyScale;
		float num2 = Mathf.Max(Mathf.Abs(lossyScale.x), Mathf.Abs(lossyScale.y));
		float num3 = num * num2;
		return transform.position + transform.up * num3;
	}

	public void InitializeForHostRing(RingObject ring)
	{
		hostRing = ring;
		if (!((Object)(object)ring == (Object)null))
		{
			GameplayPartData gameplayPartData = ring.PartData;
			partData = GameplayPartData.Create(gameplayPartData.PhysicPartId + "#palette_lock_gap", gameplayPartData.Id, canCollect: false, 0, 0f, 0f, 0f, randomBombCanSelect: false, hasBomb: false, gameplayPartData.PaletteColor);
			partData.GapAngleDeg = gameplayPartData.GapAngleDeg;
			partData.InnerRadius = gameplayPartData.InnerRadius;
			partData.OuterRadius = gameplayPartData.OuterRadius;
			RefreshBlockColliders();
		}
	}

	internal void ResetLockLocalPosition()
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		if (!((Object)(object)lockTransform == (Object)null))
		{
			float z = lockTransform.localPosition.z;
			lockTransform.localPosition = new Vector3(0f, 0f, z);
			lockTransform.localRotation = Quaternion.Euler(0f, 0f, 270f);
		}
	}

	internal void SyncLockSegmentWithGap(RingSegmentController gapSegment)
	{
		//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cb: Unknown result type (might be due to invalid IL or missing references)
		if (!((Object)(object)lockSegment == (Object)null) && !((Object)(object)gapSegment == (Object)null) && !(gapSegment.CenterRadius <= 0.0001f))
		{
			float gapAngleDeg = lockSegment.GapAngleDeg;
			float gapDirectionDeg = lockSegment.GapDirectionDeg;
			Material sharedMaterial = lockSegment.SharedMaterial;
			RingSegmentBuildSettings buildSettings = lockSegment.BuildSettings;
			buildSettings.width = Mathf.Max(0.01f, gapSegment.BandWidth);
			lockSegment.Setup(gapSegment.CenterRadius, gapAngleDeg, gapDirectionDeg, buildSettings, sharedMaterial);
			if ((Object)(object)lockHeartQuad != (Object)null)
			{
				lockHeartQuad.SetRingRadius(gapSegment.CenterRadius);
				Transform transform = ((Component)lockHeartQuad).transform;
				Vector3 localPosition = transform.localPosition;
				localPosition.x = 0f;
				localPosition.y = 0f;
				transform.localPosition = localPosition;
			}
		}
	}

	internal void ApplyLockVertexColors(Color lockBody, Color lockHeart)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		lockSegment?.ApplyVertexColor(lockBody);
		lockHeartQuad?.ApplyVertexColor(lockHeart);
	}

	internal PolygonCollider2D ResolveBodyCollider()
	{
		if ((Object)(object)bodyCollider == (Object)null)
		{
			bodyCollider = ((Component)this).GetComponent<PolygonCollider2D>();
		}
		return bodyCollider;
	}

	internal void RefreshBlockColliders()
	{
		PolygonCollider2D val = ResolveBodyCollider();
		Collider2D[] array;
		if (!((Object)(object)val != (Object)null))
		{
			array = Array.Empty<Collider2D>();
		}
		else
		{
			Collider2D[] array2 = (Collider2D[])(object)new PolygonCollider2D[1] { val };
			array = array2;
		}
		blockColliders = array;
	}

	public void EnableRemoveMode(Action<IGameplayObject, Vector3> onFinishedRemove = null, bool deferSolve = false)
	{
	}

	public void DisableRemoveMode()
	{
	}

	public void CompleteHammerRemove()
	{
	}

	public void SetBlockedByLayer(LayerArea layerArea, bool isBlocked)
	{
	}

	public Vector3 GetPosition()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		return ((Component)this).transform.position;
	}

	public void RegisterOnSolved(Action<IGameplayObject> action)
	{
		onSolved = (Action<IGameplayObject>)Delegate.Combine(onSolved, action);
	}

	public void DisableBlockCollidersOnSolve()
	{
		SetBlockCollidersEnabled(enabled: false);
	}

	public void DisableAllPhysicColliders()
	{
		SetBlockCollidersEnabled(enabled: false);
	}

	public void EnableAllPhysicColliders()
	{
		SetBlockCollidersEnabled(enabled: true);
	}

	private void SetBlockCollidersEnabled(bool enabled)
	{
		if (blockColliders == null)
		{
			return;
		}
		for (int i = 0; i < blockColliders.Length; i++)
		{
			Collider2D val = blockColliders[i];
			if ((Object)(object)val != (Object)null)
			{
				((Behaviour)val).enabled = enabled;
			}
		}
	}

	public void CollectSolveFragmentSegments(List<SolveFragmentSegment> buffer)
	{
		if (buffer != null)
		{
			TryAddSolveFragmentSegment(((Component)this).transform, buffer);
			if ((Object)(object)lockSegment != (Object)null)
			{
				TryAddSolveFragmentSegment(((Component)lockSegment).transform, buffer, "eff_cycle_boom_style_fragment_lock");
			}
		}
	}

	private static void TryAddSolveFragmentSegment(Transform partTransform, List<SolveFragmentSegment> buffer, string effectPrefabName = null)
	{
		if (!((Object)(object)partTransform == (Object)null))
		{
			MeshFilter component = ((Component)partTransform).GetComponent<MeshFilter>();
			Mesh val = (((Object)(object)component != (Object)null) ? component.sharedMesh : null);
			if (!((Object)(object)val == (Object)null) && val.vertexCount != 0)
			{
				buffer.Add(new SolveFragmentSegment(partTransform, val, ((Component)partTransform).gameObject, effectPrefabName));
			}
		}
	}

	internal void SetSolved()
	{
		IsSolved = true;
	}

	internal void InvokeOnSolved()
	{
		onSolved?.Invoke(this);
	}
}
