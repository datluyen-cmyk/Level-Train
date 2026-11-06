using System;
using System.Collections.Generic;
using RotateRings;
using RotateRings.GamePlay.Events;
using RotateRings.GamePlay.Objects;
using RotateRings.GamePlay.Services;
using UnityEngine;
using UnlockRing.Core.Domain;
using Vibration;

public class PullObject : DraggableObjectWithLimitRange, IGameplaySolveReason, IThreadingScoreProvider, IGameplayObject, IInteractableObject, IGameplayPointerHandler, IGameplayPhysicColliders
{
	private GameplaySolveReason solveReason;

	private PartId passiveByPartId;

	private PartId stepBatchPartId;

	private const string TAG = "[GamePlay][PullObject] ";

	private const int BaseReleaseScore = 50;

	[SerializeField]
	private GameplayPartData partData;

	[NonSerialized]
	private int lockCount;

	[NonSerialized]
	private bool isCurrentPaletteLockKey;

	[NonSerialized]
	private PartId cachedPartId;

	[NonSerialized]
	private bool partIdCached;

	[SerializeField]
	protected Overlap2DHandler overlap2DHandler;

	[SerializeField]
	protected Collider2D activeCollider;

	[SerializeField]
	protected Transform dragArea;

	private Collider2D dragAreaCollider;

	private bool dragAreaColliderResolved;

	[SerializeField]
	protected List<RingRelative> ringRelatives;

	[SerializeField]
	protected LayerMaskHelper[] extendLayerMaskHelpers;

	protected Action onFinishedPull;

	protected Action onStartedPull;

	protected Action<IGameplayObject, Vector3> onFinishedRemoveMode;

	protected Action<IGameplayObject> onSolved;

	protected bool isEnableRemoveMode;

	protected bool deferRemoveModeSolve;

	protected List<IObjectEffectedByLayerArea> blockObjects;

	private readonly Collider2D[] blockQueryBuffer = (Collider2D[])(object)new Collider2D[32];

	private readonly List<Collider2D> stickContactColliders = new List<Collider2D>();

	private readonly List<Collider2D> stickContactProbes = new List<Collider2D>();

	private Collider2D cachedTranslationBlockCollider;

	private Vector2 cachedTranslationBlockContact;

	private List<BombRelative> selfBombs;

	private static readonly ContactFilter2D SelfBombContactFilter = CreateSelfBombContactFilter();

	private readonly Collider2D[] selfBombOverlapBuffer = (Collider2D[])(object)new Collider2D[16];

	private FixedObject hostGuide;

	private Vector2? frozenLockedDragDirection;

	protected LayerAreaBlockedHelper layerAreaBlockedHelper;

	private PullEscapePlanner escapePlanner;

	[NonSerialized]
	private bool isAutoEscaping;

	[NonSerialized]
	private object autoEscapeOwner;

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
			partIdCached = false;
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

	public bool IsCanDragging { get; protected set; } = true;

	internal Overlap2DHandler Overlap2DHandler => overlap2DHandler;

	internal IReadOnlyList<RingRelative> RingRelatives => ringRelatives;

	internal FixedObject HostGuide => hostGuide;

	public bool IsAutoEscaping => isAutoEscaping;

	public object AutoEscapeOwner => autoEscapeOwner;

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

	public bool TryGetPartId(out PartId partId)
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		string physicPartId = PartData.PhysicPartId;
		if (string.IsNullOrEmpty(physicPartId))
		{
			partId = default(PartId);
			return false;
		}
		if (!partIdCached)
		{
			cachedPartId = new PartId(physicPartId);
			partIdCached = true;
		}
		partId = cachedPartId;
		return true;
	}

	private void WarmPartIdCache()
	{
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		string physicPartId = PartData.PhysicPartId;
		if (string.IsNullOrEmpty(physicPartId))
		{
			partIdCached = false;
			return;
		}
		cachedPartId = new PartId(physicPartId);
		partIdCached = true;
	}

	public void Solve(BombRelative sourceBomb = null, GameplaySolveReasonContext releaseContext = default(GameplaySolveReasonContext))
	{
		SingletonComponent<GameplayManager>.Instance?.SolveService?.SolveObject(this, sourceBomb, releaseContext);
	}

	public void RegisterOnPointerDown(Action action)
	{
		onStartedPull = (Action)Delegate.Combine(onStartedPull, action);
	}

	public void RegisterOnPointerUp(Action action)
	{
		onFinishedPull = (Action)Delegate.Combine(onFinishedPull, action);
	}

	public void EnableRemoveMode(Action<IGameplayObject, Vector3> onFinishedRemove = null, bool deferSolve = false)
	{
		onFinishedRemoveMode = onFinishedRemove;
		deferRemoveModeSolve = deferSolve;
		isEnableRemoveMode = true;
	}

	public void DisableRemoveMode()
	{
		isEnableRemoveMode = false;
		deferRemoveModeSolve = false;
	}

	public void CompleteHammerRemove()
	{
		if (!IsSolved)
		{
			Solve();
		}
	}

	public Vector3 GetPosition()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		return ((Component)this).transform.position;
	}

	internal void ApplySavedPosition(Vector3 position)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		((Component)this).transform.position = position;
		ApplyDragOriginPosition(Vector2.op_Implicit(position));
	}

	public int GetReleaseScore()
	{
		return ThreadingScoreCalculator.GetReleaseScore(50, PartData);
	}

	internal void CollectOverlappedPartIds(HashSet<PartId> output)
	{
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		if (overlap2DHandler == null || output == null)
		{
			return;
		}
		int overlapColliders = overlap2DHandler.GetOverlapColliders(blockQueryBuffer);
		for (int i = 0; i < overlapColliders; i++)
		{
			Collider2D val = blockQueryBuffer[i];
			if (!((Object)(object)val == (Object)null) && !TryAddCatchGuideOverlapPartId(val, output))
			{
				IGameplayObject gameplayObject = ResolveThreadingTarget(val, this);
				if (gameplayObject != null && !gameplayObject.IsSolved && gameplayObject.TryGetPartId(out var partId))
				{
					output.Add(partId);
				}
			}
		}
	}

	private bool TryAddCatchGuideOverlapPartId(Collider2D collider, HashSet<PartId> output)
	{
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)collider == (Object)null || output == null || !RingColliderQuery.TryGetMarker(collider, out var marker))
		{
			return false;
		}
		FixedButterflyRelative fixedButterflyRelative = marker.GetFixedButterflyRelative();
		if ((Object)(object)fixedButterflyRelative == (Object)null || !fixedButterflyRelative.TryGetGuidePartId(out var partId))
		{
			return false;
		}
		output.Add(partId);
		return true;
	}

	internal void FillCatchGuideOverlapInstanceIds(HashSet<int> instanceIds)
	{
		if (instanceIds == null || overlap2DHandler == null)
		{
			return;
		}
		instanceIds.Clear();
		int overlapColliders = overlap2DHandler.GetOverlapColliders(blockQueryBuffer);
		for (int i = 0; i < overlapColliders; i++)
		{
			Collider2D val = blockQueryBuffer[i];
			if ((Object)(object)val != (Object)null && RingColliderQuery.IsCatchGuideBodyCollider(val))
			{
				instanceIds.Add(((Object)val).GetInstanceID());
			}
		}
	}

	private static IGameplayObject ResolveThreadingTarget(Collider2D collider, PullObject self)
	{
		if ((Object)(object)collider == (Object)null)
		{
			return null;
		}
		FixedObject componentInParent = ((Component)collider).GetComponentInParent<FixedObject>();
		if ((Object)(object)componentInParent != (Object)null && (Object)(object)componentInParent != (Object)(object)self)
		{
			return componentInParent;
		}
		RingObject componentInParent2 = ((Component)collider).GetComponentInParent<RingObject>();
		if ((Object)(object)componentInParent2 != (Object)null && (Object)(object)componentInParent2 != (Object)(object)self)
		{
			return componentInParent2;
		}
		PullObject componentInParent3 = ((Component)collider).GetComponentInParent<PullObject>();
		if ((Object)(object)componentInParent3 != (Object)null && (Object)(object)componentInParent3 != (Object)(object)self)
		{
			return componentInParent3;
		}
		return null;
	}

	public void RegisterOnSolved(Action<IGameplayObject> action)
	{
		onSolved = (Action<IGameplayObject>)Delegate.Combine(onSolved, action);
	}

	public void SetBlockedByLayer(LayerArea layerArea, bool isBlocked)
	{
		blockObjects = FindAllBlockObjects();
		if (layerAreaBlockedHelper == null)
		{
			return;
		}
		isBlocked = layerAreaBlockedHelper.SetBlockedByLayer(layerArea, isBlocked);
		if (blockObjects != null)
		{
			for (int i = 0; i < blockObjects.Count; i++)
			{
				blockObjects[i]?.SetBlockedByLayer(layerArea, isBlocked);
			}
		}
	}

	protected List<IObjectEffectedByLayerArea> FindAllBlockObjects()
	{
		if (blockObjects != null && blockObjects.Count > 0)
		{
			return blockObjects;
		}
		if (blockObjects == null)
		{
			blockObjects = new List<IObjectEffectedByLayerArea>();
		}
		else
		{
			blockObjects.Clear();
		}
		if (overlap2DHandler != null)
		{
			int overlapColliders = overlap2DHandler.GetOverlapColliders(blockQueryBuffer);
			for (int i = 0; i < overlapColliders && i < blockQueryBuffer.Length; i++)
			{
				Collider2D val = blockQueryBuffer[i];
				if ((Object)(object)val == (Object)null)
				{
					break;
				}
				IObjectEffectedByLayerArea component = ((Component)val).GetComponent<IObjectEffectedByLayerArea>();
				Transform transform = ((Component)val).transform;
				if ((Object)(object)transform == (Object)null)
				{
					break;
				}
				if ((Object)(object)transform.parent != (Object)(object)((Component)this).transform && component != null)
				{
					blockObjects.Add(component);
				}
			}
		}
		return blockObjects;
	}

	private void Awake()
	{
		WarmPartIdCache();
		if ((Object)(object)dragArea == (Object)null)
		{
			Transform val = ((Component)this).transform.Find("DragArea");
			if ((Object)(object)val != (Object)null)
			{
				dragArea = val;
			}
		}
		ringRelatives = new List<RingRelative>(((Component)this).GetComponentsInChildren<RingRelative>());
		selfBombs = new List<BombRelative>(((Component)this).GetComponentsInChildren<BombRelative>(true));
		SetupLayerMaskHelpers();
		GetDragAreaCollider();
	}

	public void SetOverlapCollidersEnabled(bool enabled)
	{
		if ((Object)(object)activeCollider != (Object)null)
		{
			((Behaviour)activeCollider).enabled = enabled;
		}
	}

	public void SetDragCollidersEnabled(bool enabled)
	{
		Collider2D val = GetDragAreaCollider();
		if ((Object)(object)val != (Object)null)
		{
			((Behaviour)val).enabled = enabled;
		}
	}

	private Collider2D GetDragAreaCollider()
	{
		if (dragAreaColliderResolved)
		{
			return dragAreaCollider;
		}
		if ((Object)(object)dragArea == (Object)null)
		{
			return null;
		}
		dragAreaColliderResolved = true;
		dragAreaCollider = ((Component)dragArea).GetComponent<Collider2D>();
		return dragAreaCollider;
	}

	public void DisableAllPhysicColliders()
	{
		SetOverlapCollidersEnabled(enabled: false);
		SetDragCollidersEnabled(enabled: false);
	}

	public void EnableAllPhysicColliders()
	{
		SetOverlapCollidersEnabled(enabled: true);
		SetDragCollidersEnabled(enabled: true);
	}

	internal void SetSolved()
	{
		IsSolved = true;
	}

	internal void OnSolveDragCleanup()
	{
		if (beingHeld)
		{
			SetOperateHighlight(highlight: false);
			HandlePointerUp(default(GameplayPointerData));
		}
	}

	internal void CacheHostGuide()
	{
		if ((Object)(object)hostGuide != (Object)null || overlap2DHandler == null)
		{
			return;
		}
		int overlapColliders = overlap2DHandler.GetOverlapColliders(blockQueryBuffer);
		for (int i = 0; i < overlapColliders && i < blockQueryBuffer.Length; i++)
		{
			Collider2D val = blockQueryBuffer[i];
			if (!((Object)(object)val == (Object)null))
			{
				FixedObject componentInParent = ((Component)val).GetComponentInParent<FixedObject>();
				if ((Object)(object)componentInParent != (Object)null)
				{
					hostGuide = componentInParent;
					break;
				}
			}
		}
	}

	internal void SolveAllRingRelatives()
	{
		if (ringRelatives == null)
		{
			return;
		}
		foreach (RingRelative ringRelative in ringRelatives)
		{
			if ((Object)(object)ringRelative != (Object)null)
			{
				ringRelative.Solve();
			}
		}
	}

	internal void InvokeOnSolved()
	{
		onSolved?.Invoke(this);
	}

	protected override void Start()
	{
		base.Start();
		CacheHostGuide();
	}

	protected override bool IsCastHitAllowed(Collider2D hitCollider)
	{
		RingLockPassiveService ringLockPassiveService = SingletonComponent<GameplayManager>.Instance?.LockPassive;
		if (ringLockPassiveService == null)
		{
			return true;
		}
		if (ringLockPassiveService.IsPullLocklistCollider(this, hitCollider))
		{
			return false;
		}
		if (RingColliderQuery.IsCatchGuideBodyCollider(hitCollider))
		{
			return false;
		}
		return true;
	}

	private void SetupLayerMaskHelpers()
	{
		if (layerAreaBlockedHelper != null)
		{
			layerAreaBlockedHelper.ApplyAllLayerMaskHelpersIn(((Component)this).transform);
		}
		if (extendLayerMaskHelpers != null && extendLayerMaskHelpers.Length >= 1)
		{
			for (int i = 0; i < extendLayerMaskHelpers.Length; i++)
			{
				if (layerAreaBlockedHelper != null)
				{
					layerAreaBlockedHelper.AddLayerMaskHelper(extendLayerMaskHelpers[i]);
				}
			}
		}
		if (overlap2DHandler != null && layerAreaBlockedHelper != null)
		{
			layerAreaBlockedHelper.AddLayerMaskHelper(overlap2DHandler);
		}
	}

	protected virtual void OnDestroy()
	{
		SingletonComponent<GameplayManager>.Instance?.OperateHighlightService?.Remove(((Component)this).gameObject);
	}

	public bool CanReceivePointer()
	{
		GameplayManager instance = SingletonComponent<GameplayManager>.Instance;
		if (instance != null && instance.PropUseService?.IsActive == true && !isEnableRemoveMode)
		{
			return false;
		}
		if (isAutoEscaping)
		{
			return false;
		}
		if (!IsSolved)
		{
			return IsCanDragging;
		}
		return false;
	}

	public bool IsPointerCaptured()
	{
		return beingHeld;
	}

	public void OnPointerDown(GameplayPointerData data)
	{
		//IL_00da: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
		if (!IsCanDragging)
		{
			return;
		}
		if (!isEnableRemoveMode)
		{
			GameplayManager instance = SingletonComponent<GameplayManager>.Instance;
			if (instance == null || instance.LockChain?.IsLockBlocked(this) != true)
			{
				onStartedPull?.Invoke();
				if ((Object)(object)rb != (Object)null)
				{
					rb.bodyType = (RigidbodyType2D)1;
				}
				HandlePointerDown(data);
				SetOperateHighlight(highlight: true);
			}
		}
		else
		{
			isEnableRemoveMode = false;
			if (deferRemoveModeSolve)
			{
				deferRemoveModeSolve = false;
				onFinishedRemoveMode?.Invoke(this, data.WorldPosition);
			}
			else
			{
				Solve();
				onFinishedRemoveMode?.Invoke(this, data.WorldPosition);
				onFinishedPull?.Invoke();
			}
		}
	}

	public override Vector2 GetLockedDragDirection()
	{
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		if (beingHeld && frozenLockedDragDirection.HasValue)
		{
			return frozenLockedDragDirection.Value;
		}
		return base.GetLockedDragDirection();
	}

	private void CacheFrozenLockedDragDirection()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		Vector2 lockedDragDirection = base.GetLockedDragDirection();
		frozenLockedDragDirection = ((((Vector2)(ref lockedDragDirection)).sqrMagnitude > Mathf.Epsilon) ? ((Vector2)(ref lockedDragDirection)).normalized : lockedDragDirection);
	}

	private void SyncStickCollisionContacts()
	{
		if (beingHeld)
		{
			SyncStickCollisionContactsInternal();
		}
	}

	public void SyncEscapeContacts()
	{
		SyncStickCollisionContactsInternal();
	}

	private void SyncStickCollisionContactsInternal()
	{
		RingDragCollisionEventService ringDragCollisionEventService = SingletonComponent<GameplayManager>.Instance?.RingDragCollisionEventService;
		if (ringDragCollisionEventService == null || overlap2DHandler == null)
		{
			return;
		}
		stickContactColliders.Clear();
		int overlapColliders = overlap2DHandler.GetOverlapColliders(blockQueryBuffer);
		for (int i = 0; i < overlapColliders && i < blockQueryBuffer.Length; i++)
		{
			Collider2D val = blockQueryBuffer[i];
			if (!((Object)(object)val == (Object)null) && IsCollisionEligibleCollider(val))
			{
				stickContactColliders.Add(val);
			}
		}
		Collider2D val2 = (((Object)(object)rb != (Object)null) ? ((Component)rb).GetComponent<Collider2D>() : null);
		ringDragCollisionEventService.CollectSelfColliders(stickContactProbes, ringRelatives, -1, (overlap2DHandler != null) ? overlap2DHandler.Collider : null, activeCollider, val2);
		ringDragCollisionEventService.SyncCollidersAndSendEvents((Component)(object)this, stickContactColliders, stickContactProbes, RingDragMoveKind.Translate);
	}

	private void FinalizeStickCollisionContactsOnPointerUp()
	{
		RingDragCollisionEventService ringDragCollisionEventService = SingletonComponent<GameplayManager>.Instance?.RingDragCollisionEventService;
		if (ringDragCollisionEventService == null || overlap2DHandler == null)
		{
			return;
		}
		stickContactColliders.Clear();
		int overlapColliders = overlap2DHandler.GetOverlapColliders(blockQueryBuffer);
		for (int i = 0; i < overlapColliders && i < blockQueryBuffer.Length; i++)
		{
			Collider2D val = blockQueryBuffer[i];
			if (!((Object)(object)val == (Object)null) && IsCollisionEligibleCollider(val))
			{
				stickContactColliders.Add(val);
			}
		}
		Collider2D val2 = (((Object)(object)rb != (Object)null) ? ((Component)rb).GetComponent<Collider2D>() : null);
		ringDragCollisionEventService.CollectSelfColliders(stickContactProbes, ringRelatives, -1, (overlap2DHandler != null) ? overlap2DHandler.Collider : null, activeCollider, val2);
		ringDragCollisionEventService.SyncCollidersAndSendEvents((Component)(object)this, stickContactColliders, stickContactProbes, RingDragMoveKind.Translate);
	}

	private bool IsCollisionEligibleCollider(Collider2D col)
	{
		if ((Object)(object)col == (Object)null || ((Component)col).gameObject.layer == 6)
		{
			return false;
		}
		if (!RingColliderQuery.TryGetMarker(col, out var marker))
		{
			return false;
		}
		return marker.Owner != this;
	}

	public override void HandlePointerDown(GameplayPointerData data)
	{
		SingletonComponent<GameplayManager>.Instance?.RingDragCollisionEventService?.SendAndClearData();
		ClearTranslationBlockCache();
		BeginStickDragCollisionTracking();
		base.HandlePointerDown(data);
		CacheFrozenLockedDragDirection();
		VibrateUtility.Vibrate(1101);
	}

	private void BeginStickDragCollisionTracking()
	{
		RingDragCollisionEventService ringDragCollisionEventService = SingletonComponent<GameplayManager>.Instance?.RingDragCollisionEventService;
		if (ringDragCollisionEventService == null || overlap2DHandler == null)
		{
			return;
		}
		List<Collider2D> list = new List<Collider2D>();
		int overlapColliders = overlap2DHandler.GetOverlapColliders(blockQueryBuffer);
		for (int i = 0; i < overlapColliders && i < blockQueryBuffer.Length; i++)
		{
			Collider2D val = blockQueryBuffer[i];
			if (!((Object)(object)val == (Object)null) && IsCollisionEligibleCollider(val))
			{
				list.Add(val);
			}
		}
		ringDragCollisionEventService.BeginDragEvent((Component)(object)this, list);
	}

	public void OnPointerDrag(GameplayPointerData data)
	{
		HandlePointerDrag(data);
	}

	public bool HasLiveSelfBomb()
	{
		if (selfBombs == null)
		{
			return false;
		}
		for (int i = 0; i < selfBombs.Count; i++)
		{
			BombRelative bombRelative = selfBombs[i];
			if ((Object)(object)bombRelative != (Object)null && !bombRelative.IsSolvedForTrack)
			{
				return true;
			}
		}
		return false;
	}

	public void BeginAutoEscape(object owner)
	{
		isAutoEscaping = true;
		autoEscapeOwner = owner;
	}

	public void EndAutoEscape()
	{
		isAutoEscaping = false;
		autoEscapeOwner = null;
	}

	internal bool IsEscapeRelevantCollider(Collider2D col)
	{
		return IsCollisionEligibleCollider(col);
	}

	internal bool IsEscapeBlockingCollider(Collider2D col)
	{
		if ((Object)(object)col != (Object)null)
		{
			return IsCastHitAllowed(col);
		}
		return false;
	}

	protected override bool IsEscapeTargetCollider(Collider2D targetCollider)
	{
		if (isAutoEscaping && escapePlanner != null)
		{
			return escapePlanner.IsEscapeTargetCollider(targetCollider);
		}
		return false;
	}

	public bool TryPlanEscape(Vector2 direction, out float escapeDistance)
	{
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		if (escapePlanner == null)
		{
			escapePlanner = new PullEscapePlanner(this);
		}
		return escapePlanner.TryPlan(direction, out escapeDistance);
	}

	public override void HandlePointerDrag(GameplayPointerData data)
	{
		base.HandlePointerDrag(data);
		if (beingHeld)
		{
			SyncStickCollisionContacts();
			TryNotifyTranslationBlockWhileHeld();
			SingletonComponent<GameplayManager>.Instance?.RingBoom?.ProbeBombContactsDuringDrag(stickContactProbes, this);
		}
	}

	private void TryNotifyTranslationBlockWhileHeld()
	{
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		if (base.BlockMotionSign == 0f)
		{
			SingletonComponent<GameplayManager>.Instance?.RingDragCollisionEventService?.NotifyStopRearmed();
			return;
		}
		RingDragCollisionEventService ringDragCollisionEventService = SingletonComponent<GameplayManager>.Instance?.RingDragCollisionEventService;
		if (ringDragCollisionEventService != null && !((Object)(object)cachedTranslationBlockCollider == (Object)null))
		{
			ringDragCollisionEventService.SendStopEvent((Component)(object)this, cachedTranslationBlockCollider, cachedTranslationBlockContact, base.BlockMotionSign, RingDragMoveKind.Translate);
		}
	}

	protected override bool TryGetSelfBombTranslationBlock(float moveDirection, out Collider2D blocker, out Vector2 contact)
	{
		return TryCaptureSelfBombContactBlock(out blocker, out contact);
	}

	protected override void NotifySelfBombTranslationBlocked(Collider2D blocker, Vector2 contact, Vector2 direction)
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)blocker != (Object)null)
		{
			cachedTranslationBlockCollider = blocker;
			cachedTranslationBlockContact = contact;
		}
		RingDragCollisionEventService ringDragCollisionEventService = SingletonComponent<GameplayManager>.Instance?.RingDragCollisionEventService;
		if (ringDragCollisionEventService != null && !((Object)(object)blocker == (Object)null))
		{
			Vector2 lockedDragDirection = GetLockedDragDirection();
			float num = Mathf.Sign(Vector2.Dot(direction, lockedDragDirection));
			if (Mathf.Approximately(num, 0f))
			{
				num = 1f;
			}
			ringDragCollisionEventService.SendStopEvent((Component)(object)this, blocker, contact, num, RingDragMoveKind.Translate);
		}
	}

	private bool TryCaptureSelfBombContactBlock(out Collider2D blocker, out Vector2 contact)
	{
		//IL_0004: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_016c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0171: Unknown result type (might be due to invalid IL or missing references)
		//IL_018b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0192: Unknown result type (might be due to invalid IL or missing references)
		//IL_0197: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_017f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0185: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a6: Unknown result type (might be due to invalid IL or missing references)
		blocker = null;
		contact = default(Vector2);
		if (selfBombs == null || selfBombs.Count == 0)
		{
			return false;
		}
		for (int i = 0; i < selfBombs.Count; i++)
		{
			BombRelative bombRelative = selfBombs[i];
			if ((Object)(object)bombRelative == (Object)null || bombRelative.IsSolvedForTrack)
			{
				continue;
			}
			Collider2D val = bombRelative.ActiveCollider;
			if ((Object)(object)val == (Object)null || !((Behaviour)val).enabled)
			{
				continue;
			}
			int num = val.Overlap(SelfBombContactFilter, selfBombOverlapBuffer);
			for (int j = 0; j < num && j < selfBombOverlapBuffer.Length; j++)
			{
				Collider2D val2 = selfBombOverlapBuffer[j];
				if ((Object)(object)val2 == (Object)null || !((Behaviour)val2).enabled || ((Component)val2).gameObject.layer != 7)
				{
					continue;
				}
				Transform transform = ((Component)val2).transform;
				if (!((Object)(object)transform == (Object)(object)((Component)this).transform) && !transform.IsChildOf(((Component)this).transform) && !RingColliderQuery.IsRingRelativeBodyCollider(val2) && !RingColliderQuery.IsCatchGuideBodyCollider(val2))
				{
					bool flag = false;
					PullObject pull;
					FixedObject fixedObject;
					if (RingColliderQuery.TryGetRingObject(val2, out var ring))
					{
						flag = !ring.IsSolved;
					}
					else if (RingColliderQuery.TryGetPullObject(val2, out pull))
					{
						flag = (Object)(object)pull != (Object)(object)this && !pull.IsSolved;
					}
					else if (RingColliderQuery.TryGetFixedObject(val2, out fixedObject))
					{
						flag = !fixedObject.IsSolved;
					}
					if (flag)
					{
						bombRelative.TryExplodeFromSelfContact(val2);
						blocker = val2;
						ColliderDistance2D val3 = Physics2D.Distance(val, val2);
						contact = (Vector2)(((ColliderDistance2D)(ref val3)).isValid ? ((((ColliderDistance2D)(ref val3)).pointA + ((ColliderDistance2D)(ref val3)).pointB) * 0.5f) : default(Vector2));
						return true;
					}
				}
			}
		}
		return false;
	}

	private static ContactFilter2D CreateSelfBombContactFilter()
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		ContactFilter2D noFilter = ContactFilter2D.noFilter;
		noFilter.useTriggers = true;
		noFilter.useLayerMask = true;
		noFilter.layerMask = GameConst.OverlapAreaLayerMask;
		return noFilter;
	}

	protected override void NotifyPhysicsDragBlocked(RaycastHit2D hit, Vector2 direction)
	{
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)((RaycastHit2D)(ref hit)).collider != (Object)null)
		{
			cachedTranslationBlockCollider = ((RaycastHit2D)(ref hit)).collider;
			cachedTranslationBlockContact = ((RaycastHit2D)(ref hit)).point;
		}
		base.NotifyPhysicsDragBlocked(hit, direction);
	}

	private void ClearTranslationBlockCache()
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		cachedTranslationBlockCollider = null;
		cachedTranslationBlockContact = default(Vector2);
	}

	public void OnPointerUp(GameplayPointerData data)
	{
		if (beingHeld)
		{
			SetOperateHighlight(highlight: false);
			HandlePointerUp(data);
			GameplayManager instance = SingletonComponent<GameplayManager>.Instance;
			if (instance != null && instance.AutoEscape?.TryBegin(this) == true)
			{
				return;
			}
			FinishReleaseSettlement();
		}
		ClearDragSessionState();
	}

	public void FinishReleaseSettlement()
	{
		ResolveReleaseOverlap();
		if ((Object)(object)rb != (Object)null)
		{
			rb.bodyType = (RigidbodyType2D)1;
		}
		FinalizeStickCollisionContactsOnPointerUp();
		GameplayManager instance = SingletonComponent<GameplayManager>.Instance;
		if (instance != null && instance.SolveService?.CanSolve(this) == true)
		{
			Solve();
		}
		onFinishedPull?.Invoke();
	}

	public void ClearDragSessionState()
	{
		ClearTranslationBlockCache();
		frozenLockedDragDirection = null;
		SingletonComponent<GameplayManager>.Instance?.RingDragCollisionEventService?.SendAndClearData((Component)(object)this);
		SingletonComponent<GameplayManager>.Instance?.ObjectsSolveAndCheckVictory();
	}

	internal Vector3 GetDragStartWorldPosition()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		return dragStartPosition;
	}

	private void SetOperateHighlight(bool highlight)
	{
		SingletonComponent<GameplayManager>.Instance?.OperateHighlightService?.TrySetHighlight(((Component)this).gameObject, highlight);
	}
}
