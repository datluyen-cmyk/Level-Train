using System;
using System.Collections.Generic;
using UnityEngine;
using UnlockRing.Core.Domain;

public class FixedObject : MonoBehaviour, IGameplaySolveReason, IGameplayObject, IGameplayPhysicColliders
{
	private GameplaySolveReason solveReason;

	private PartId passiveByPartId;

	private PartId stepBatchPartId;

	[SerializeField]
	private GameplayPartData partData;

	[NonSerialized]
	private int lockCount;

	[NonSerialized]
	private bool isCurrentPaletteLockKey;

	[SerializeField]
	protected string shapeId;

	[SerializeField]
	protected Overlap2DHandler overlap2DHandler;

	[SerializeField]
	protected Collider2D selfCollider;

	[SerializeField]
	protected Collider2D[] blockColliders;

	[SerializeField]
	protected LayerMaskHelper[] extendLayerMaskHelpers;

	protected Action<IGameplayObject> onSolved;

	protected LayerAreaBlockedHelper layerAreaBlockedHelper;

	public GameplaySolveReason SolveReason => solveReason;

	public PartId PassiveByPartId => passiveByPartId;

	public PartId StepBatchPartId => stepBatchPartId;

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

	public bool IsSolved { get; protected set; }

	internal Overlap2DHandler Overlap2DHandler => overlap2DHandler;

	internal Collider2D[] BlockColliders => blockColliders;

	internal Collider2D SelfCollider => selfCollider;

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

	public void EnableRemoveMode(Action<IGameplayObject, Vector3> onFinishedRemove = null, bool deferSolve = false)
	{
	}

	public void DisableRemoveMode()
	{
	}

	public void CompleteHammerRemove()
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

	public void SetBlockedByLayer(LayerArea layerArea, bool isBlocked)
	{
		if (layerAreaBlockedHelper != null)
		{
			layerAreaBlockedHelper.SetBlockedByLayer(layerArea, isBlocked);
		}
	}

	public virtual int GetOverlapCount()
	{
		return overlap2DHandler.GetOverlapCount();
	}

	public virtual bool IsBlockedByRing()
	{
		return GetOverlapCount() > 0;
	}

	public virtual void DoFade()
	{
		if (blockColliders == null)
		{
			return;
		}
		for (int i = 0; i < blockColliders.Length; i++)
		{
			Behaviour val = (Behaviour)(object)blockColliders[i];
			if ((Object)(object)val != (Object)null)
			{
				val.enabled = false;
			}
		}
	}

	public void SetOverlapCollidersEnabled(bool enabled)
	{
		if (overlap2DHandler != null && (Object)(object)overlap2DHandler.Collider != (Object)null)
		{
			((Behaviour)overlap2DHandler.Collider).enabled = enabled;
		}
	}

	public void SetBlockCollidersEnabled(bool enabled)
	{
		if ((Object)(object)selfCollider != (Object)null)
		{
			((Behaviour)selfCollider).enabled = enabled;
		}
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

	public void DisableAllPhysicColliders()
	{
		SetOverlapCollidersEnabled(enabled: false);
		SetBlockCollidersEnabled(enabled: false);
	}

	public void EnableAllPhysicColliders()
	{
		SetOverlapCollidersEnabled(enabled: true);
		SetBlockCollidersEnabled(enabled: true);
	}

	internal void SetBlockColliders(Collider2D[] colliders)
	{
		blockColliders = colliders;
	}

	internal bool TryCollectSelfColliderId(HashSet<int> output)
	{
		if (output == null || (Object)(object)selfCollider == (Object)null)
		{
			return false;
		}
		output.Add(((Object)selfCollider).GetInstanceID());
		return true;
	}

	internal void SetSolved()
	{
		IsSolved = true;
	}

	internal void InvokeOnSolved()
	{
		onSolved?.Invoke(this);
	}

	public void SolveFromExternalTrigger(BombRelative sourceBomb = null, GameplaySolveReasonContext releaseContext = default(GameplaySolveReasonContext))
	{
		Solve(sourceBomb, releaseContext);
	}

	protected void Solve(BombRelative sourceBomb = null, GameplaySolveReasonContext releaseContext = default(GameplaySolveReasonContext))
	{
		SingletonComponent<GameplayManager>.Instance?.SolveService?.SolveObject(this, sourceBomb, releaseContext);
	}

	private void Awake()
	{
		SetupLayerMaskHelpers();
	}

	private void SetupLayerMaskHelpers()
	{
		if (layerAreaBlockedHelper == null)
		{
			return;
		}
		layerAreaBlockedHelper.ApplyAllLayerMaskHelpersIn(((Component)this).transform);
		if (extendLayerMaskHelpers != null)
		{
			LayerMaskHelper[] array = extendLayerMaskHelpers;
			foreach (LayerMaskHelper layerMaskHelper in array)
			{
				if ((Object)(object)layerMaskHelper != (Object)null)
				{
					layerAreaBlockedHelper.AddLayerMaskHelper(layerMaskHelper);
				}
			}
		}
		if (overlap2DHandler != null)
		{
			layerAreaBlockedHelper.AddLayerMaskHelper(overlap2DHandler);
		}
	}
}
