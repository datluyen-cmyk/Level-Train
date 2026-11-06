using System;
using System.Collections.Generic;
using RotateRings;
using RotateRings.Common;
using RotateRings.GamePlay.Events;
using RotateRings.GamePlay.Objects;
using RotateRings.GamePlay.Services;
using UnityEngine;
using UnlockRing.Core.Domain;
using Vibration;
using XingYun.RingGame;

public class RingObject : DragRotateObject, IGameplaySolveReason, IInteractableObject, IThreadingScoreProvider, IGameplayObject, IGameplayPointerHandler, IGameplayPhysicColliders
{
	private readonly List<Collider2D> catchGuideOverlapScratch = new List<Collider2D>(4);

	private readonly Collider2D[] primaryOverlapCache = (Collider2D[])(object)new Collider2D[16];

	private bool primaryOverlapEnabled;

	private bool primaryOverlapValid;

	private int primaryOverlapCount;

	private GameplaySolveReason solveReason;

	private PartId passiveByPartId;

	private PartId stepBatchPartId;

	private const string TAG = "[GamePlay][RingObject] ";

	public const float InterlockPenetrationEpsilon = 0.02f;

	private const float PenetrationEpsilon = 0.02f;

	private const float HardBlockStickClearance = 0.03f;

	private const float BombProbeRotationEpsilon = 0.001f;

	private const float StopRollbackAngle = 0.1f;

	private const int BaseReleaseScore = 100;

	[SerializeField]
	private GameplayPartData partData;

	[NonSerialized]
	private float initialLocalEulerZ;

	[NonSerialized]
	private int lockCount;

	[NonSerialized]
	private PaletteLockFlags paletteLockFlags = PaletteLockFlags.None;

	[NonSerialized]
	private bool isCurrentPaletteLockKey;

	[NonSerialized]
	private PartId cachedPartId;

	[NonSerialized]
	private bool partIdCached;

	[SerializeField]
	protected Overlap2DHandler overlap2DHandler;

	[SerializeField]
	protected List<RingRelative> ringRelatives;

	[SerializeField]
	protected Collider2D activeCollider;

	[SerializeField]
	protected Collider2D[] hardBlockColliders;

	[SerializeField]
	protected Vector2 moveOutDirection;

	[SerializeField]
	protected float moveOutDuration;

	[SerializeField]
	protected Transform dragArea;

	private Collider2D dragAreaCollider;

	private bool dragAreaColliderResolved;

	private OpenRingThreadableGapQuery.RingGapGeometry gapGeometry;

	private readonly RingRotationFeelApplicator rotationFeel = new RingRotationFeelApplicator();

	protected Action onFinishedRotate;

	protected Action onStartedRotate;

	protected Action<IGameplayObject, Vector3> onFinishedRemoveMode;

	protected Action<IGameplayObject> onSolved;

	protected bool isEnableRemoveMode;

	protected bool deferRemoveModeSolve;

	protected List<IObjectEffectedByLayerArea> blockObjects;

	protected LayerAreaBlockedHelper layerAreaBlockedHelper;

	private readonly HashSet<int> currentInterlockColliderIds = new HashSet<int>();

	private int currentInterlockColliderCount;

	private int dragStartExternalOverlapCount;

	private float dragStartMaxExternalPenetration;

	private float dragStartMaxHardBlockStickPenetration;

	private float dragStartMaxInterlockPenetration;

	private readonly HashSet<int> dragStartExternalColliderIds = new HashSet<int>();

	private readonly List<Collider2D> rotationProbeColliders = new List<Collider2D>();

	private readonly List<Collider2D> sensedExternalColliders = new List<Collider2D>();

	private readonly List<Collider2D> dragContactColliders = new List<Collider2D>();

	private readonly List<Collider2D> probeCachedCatchGuideColliders = new List<Collider2D>(4);

	private readonly Collider2D[] overlapBuffer = (Collider2D[])(object)new Collider2D[16];

	private readonly Collider2D[] interlockColliderBuffer = (Collider2D[])(object)new Collider2D[32];

	private readonly Collider2D[] mainBodyInterlockBuffer = (Collider2D[])(object)new Collider2D[16];

	private int mainBodyInterlockCount;

	private readonly List<Collider2D> relativeInterlockColliders = new List<Collider2D>(16);

	private readonly List<int> relativeInterlockOffsets = new List<int>(4);

	private readonly List<int> relativeInterlockCounts = new List<int>(4);

	private readonly List<Collider2D> otherBodyInterlockScratch = new List<Collider2D>(16);

	private readonly List<int> relativeInterlockDirectCounts = new List<int>(4);

	private readonly Collider2D[] relativeWallOverlapBuffer = (Collider2D[])(object)new Collider2D[16];

	private readonly List<Collider2D> relativeWallOverlapResults = new List<Collider2D>(8);

	private static readonly ContactFilter2D RelativeWallOverlapFilter = CreateRelativeWallOverlapFilter();

	private List<BombRelative> selfBombs;

	private static readonly ContactFilter2D SelfBombContactFilter = CreateSelfBombContactFilter();

	private readonly Collider2D[] selfBombOverlapBuffer = (Collider2D[])(object)new Collider2D[16];

	private readonly Collider2D[] blockQueryBuffer = (Collider2D[])(object)new Collider2D[32];

	private Collider2D cachedRotationBlockCollider;

	private Collider2D cachedRotationBlockProbe;

	private Vector2 cachedRotationBlockContact;

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

	public bool IsCurrentPaletteLock => (paletteLockFlags & PaletteLockFlags.ColorLock) != 0;

	public PaletteLockFlags PaletteLockFlags
	{
		get
		{
			return paletteLockFlags;
		}
		set
		{
			paletteLockFlags = value;
			NormalizePaletteLockFlags();
		}
	}

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

	public bool EndedDragWithRotationBlock { get; private set; }

	internal Overlap2DHandler Overlap2DHandler => overlap2DHandler;

	internal IReadOnlyList<RingRelative> RingRelatives => ringRelatives;

	protected override void OnCollisionEnter2D(Collision2D other)
	{
		using (new ProfilerScope("RingObject.OnCollisionEnter2D"))
		{
			if (!isDragging)
			{
				return;
			}
			Collider2D val = ((other != null) ? other.collider : null);
			if ((Object)(object)val != (Object)null && IsOtherRingCollider(this, val))
			{
				RingLockPassiveService ringLockPassiveService = SingletonComponent<GameplayManager>.Instance?.LockPassive;
				RingRelative selfRingRelative = GetSelfRingRelative(other);
				if (ringLockPassiveService != null && ringLockPassiveService.ShouldIgnoreBlock(this, val, selfRingRelative))
				{
					return;
				}
			}
			if (!((Object)(object)val != (Object)null) || !RingColliderQuery.IsCatchGuideBodyCollider(val))
			{
				base.OnCollisionEnter2D(other);
				TrySendStopEvent(other);
			}
		}
	}

	private void TrySendStopEvent(Collision2D collision)
	{
		//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
		if (collision == null || (Object)(object)collision.collider == (Object)null)
		{
			return;
		}
		RingDragCollisionEventService ringDragCollisionEventService = SingletonComponent<GameplayManager>.Instance?.RingDragCollisionEventService;
		if (ringDragCollisionEventService == null)
		{
			return;
		}
		Collider2D collider = collision.collider;
		Collider2D fallbackProbe = ((rotationProbeColliders.Count > 0) ? rotationProbeColliders[0] : activeCollider);
		if (ringDragCollisionEventService.TryGetPosFromCollision(collision, rotationProbeColliders, fallbackProbe, out var outCollider, out cachedRotationBlockContact))
		{
			cachedRotationBlockCollider = collider;
			cachedRotationBlockProbe = outCollider;
			float num = ((blockDirection != 0f) ? blockDirection : Mathf.Sign(rotateAngle));
			if (Mathf.Approximately(num, 0f))
			{
				num = 1f;
			}
			ringDragCollisionEventService.SendStopEvent((Component)(object)this, collider, cachedRotationBlockContact, num, RingDragMoveKind.Rotate);
		}
	}

	public int CollectOverlapCollidersFromSource(RingRelative ownerSource, Collider2D[] buffer, HashSet<int> idOutput)
	{
		idOutput?.Clear();
		int num = 0;
		if ((Object)(object)ownerSource == (Object)null)
		{
			num = CollectOverlapCollidersFromBody(overlap2DHandler, buffer, 0, idOutput);
		}
		else
		{
			int overlapColliders = ownerSource.GetOverlapColliders(overlapBuffer);
			for (int i = 0; i < overlapColliders; i++)
			{
				num += TryAddOtherRingCollider(buffer, num, idOutput, overlapBuffer[i]);
			}
			relativeWallOverlapResults.Clear();
			CollectOverlapCollidersFromWalls(ownerSource, relativeWallOverlapResults);
			for (int j = 0; j < relativeWallOverlapResults.Count; j++)
			{
				num += TryAddOtherRingCollider(buffer, num, idOutput, relativeWallOverlapResults[j]);
			}
		}
		return num;
	}

	public RingObject GetOtherRingByCollider(Collider2D col)
	{
		if (!TryGetOtherRingByCollider(this, col, out var ring))
		{
			return null;
		}
		return ring;
	}

	private bool IsOtherBodyCollider(RingObject self, Collider2D col)
	{
		if ((Object)(object)col != (Object)null && (Object)(object)self != (Object)null && IsOtherRingCollider(self, col))
		{
			return !IsOtherRelativeCollider(self, col);
		}
		return false;
	}

	private bool IsOtherRelativeBlockCollider(Collider2D col, RingObject self)
	{
		if ((Object)(object)col == (Object)null || (Object)(object)self == (Object)null || !IsOtherRingCollider(self, col))
		{
			return false;
		}
		if (!RingColliderQuery.TryGetMarker(col, out var marker))
		{
			return false;
		}
		if (marker.Role != RingDragOtherKind.RingRelativeBody && marker.Role != RingDragOtherKind.RingRelativeWall)
		{
			return false;
		}
		RingObject ringObject = marker.Owner as RingObject;
		if ((Object)(object)ringObject == (Object)null || (Object)(object)ringObject == (Object)(object)self)
		{
			return false;
		}
		return marker.GetRingRelative()?.ContainsPhysicalBlockCollider(col) ?? false;
	}

	public bool IsOtherWhiteListCollider(Collider2D col)
	{
		if (!IsOtherBodyCollider(this, col))
		{
			return IsOtherRelativeBlockCollider(col, this);
		}
		return true;
	}

	public void CollectSelfRelativeBlockColliderIds(HashSet<int> output, string featureIdFilter = null)
	{
		if (output == null)
		{
			return;
		}
		bool flag = !string.IsNullOrEmpty(featureIdFilter);
		if (ringRelatives == null)
		{
			return;
		}
		for (int i = 0; i < ringRelatives.Count; i++)
		{
			RingRelative ringRelative = ringRelatives[i];
			if (!((Object)(object)ringRelative == (Object)null) && (!flag || !(ringRelative.FeatureId != featureIdFilter)))
			{
				ringRelative.CollectSelfColliderIds(output);
			}
		}
	}

	protected override void HandlePointerDrag(GameplayPointerData data)
	{
		using (new ProfilerScope("RingObject.HandlePointerDrag"))
		{
			base.HandlePointerDrag(data);
			if (isDragging)
			{
				SyncDragCollisionContacts();
				TryNotifyRotationBlockWhileHeld();
				if (blockDirection == 0f)
				{
					SingletonComponent<GameplayManager>.Instance?.RingDragCollisionEventService?.NotifyStopRearmed();
				}
			}
		}
	}

	private void TryNotifyRotationBlockWhileHeld()
	{
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		if (blockDirection == 0f)
		{
			return;
		}
		RingDragCollisionEventService ringDragCollisionEventService = SingletonComponent<GameplayManager>.Instance?.RingDragCollisionEventService;
		if (ringDragCollisionEventService != null && TryGetRotationBlockCollider(out var blocker, out var _, out var contact))
		{
			ringDragCollisionEventService.SendStopEvent((Component)(object)this, blocker, contact, blockDirection, RingDragMoveKind.Rotate);
			SingletonComponent<GameplayManager>.Instance?.RingBoom?.ProbeBombContactsDuringDrag(rotationProbeColliders, this);
			if ((Object)(object)cachedRotationBlockCollider != (Object)null)
			{
				BombRelative obj = ((Component)cachedRotationBlockCollider).GetComponent<BombRelative>() ?? ((Component)cachedRotationBlockCollider).GetComponentInParent<BombRelative>();
				Collider2D target = cachedRotationBlockProbe ?? ((rotationProbeColliders.Count > 0) ? rotationProbeColliders[0] : activeCollider);
				obj?.TryExplodeFromSelfContact(target);
			}
		}
	}

	private void ClearRotationBlockCache()
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		cachedRotationBlockCollider = null;
		cachedRotationBlockProbe = null;
		cachedRotationBlockContact = default(Vector2);
	}

	private bool TryCaptureRotationBlockCollider()
	{
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		if (!TryResolveRotationBlockCollider(out var blocker, out var probe, out var contact))
		{
			return false;
		}
		cachedRotationBlockCollider = blocker;
		cachedRotationBlockProbe = probe;
		cachedRotationBlockContact = contact;
		return true;
	}

	private bool TryGetRotationBlockCollider(out Collider2D blocker, out Collider2D probe, out Vector2 contact)
	{
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)cachedRotationBlockCollider != (Object)null)
		{
			blocker = cachedRotationBlockCollider;
			RingDragCollisionEventService ringDragCollisionEventService = SingletonComponent<GameplayManager>.Instance?.RingDragCollisionEventService;
			Collider2D val = (((Object)(object)cachedRotationBlockProbe != (Object)null) ? cachedRotationBlockProbe : ((rotationProbeColliders.Count > 0) ? rotationProbeColliders[0] : activeCollider));
			if (ringDragCollisionEventService == null)
			{
				probe = val;
				contact = cachedRotationBlockContact;
				return true;
			}
			ringDragCollisionEventService.TryGetPosFromBlockerCollider(blocker, rotationProbeColliders, val, out probe, out contact);
			cachedRotationBlockProbe = probe;
			cachedRotationBlockContact = contact;
			return true;
		}
		return TryResolveRotationBlockCollider(out blocker, out probe, out contact);
	}

	private void CollectDragContactColliders(List<Collider2D> output)
	{
		output.Clear();
		CollectSensedExternalColliders(output);
		CollectCurrentInterlockColliders(interlockColliderBuffer, currentInterlockColliderIds);
		for (int i = 0; i < currentInterlockColliderCount; i++)
		{
			Collider2D val = interlockColliderBuffer[i];
			if ((Object)(object)val == (Object)null)
			{
				continue;
			}
			bool flag = false;
			for (int j = 0; j < output.Count; j++)
			{
				if ((Object)(object)output[j] == (Object)(object)val)
				{
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				output.Add(val);
			}
		}
		CollectCatchGuideBodyContactColliders(output);
	}

	private void SyncDragCollisionContacts()
	{
		using (new ProfilerScope("RingObject.SyncDragCollisionContacts"))
		{
			if (!rotationProbedThisFrame)
			{
				return;
			}
			RingDragCollisionEventService ringDragCollisionEventService = SingletonComponent<GameplayManager>.Instance?.RingDragCollisionEventService;
			if (ringDragCollisionEventService != null)
			{
				if (!TryBuildDragContactCollidersFromProbeCache(dragContactColliders))
				{
					CollectDragContactColliders(dragContactColliders);
				}
				ringDragCollisionEventService.SyncCollidersAndSendEvents((Component)(object)this, dragContactColliders, rotationProbeColliders, RingDragMoveKind.Rotate);
			}
		}
	}

	private void FinalizeDragCollisionContactsOnPointerUp()
	{
		using (new ProfilerScope("RingObject.FinalizeDragCollisionContactsOnPointerUp"))
		{
			RingDragCollisionEventService ringDragCollisionEventService = SingletonComponent<GameplayManager>.Instance?.RingDragCollisionEventService;
			if (ringDragCollisionEventService != null)
			{
				CollectDragContactColliders(dragContactColliders);
				ringDragCollisionEventService.SyncCollidersAndSendEvents((Component)(object)this, dragContactColliders, rotationProbeColliders, RingDragMoveKind.Rotate);
			}
		}
	}

	internal void FillCatchGuideOverlapInstanceIds(HashSet<int> instanceIds)
	{
		if (instanceIds == null)
		{
			return;
		}
		instanceIds.Clear();
		catchGuideOverlapScratch.Clear();
		CollectCatchGuideBodyContactColliders(catchGuideOverlapScratch);
		for (int i = 0; i < catchGuideOverlapScratch.Count; i++)
		{
			Collider2D val = catchGuideOverlapScratch[i];
			if ((Object)(object)val != (Object)null)
			{
				instanceIds.Add(((Object)val).GetInstanceID());
			}
		}
	}

	private bool TryBuildDragContactCollidersFromProbeCache(List<Collider2D> output)
	{
		if (!hasProbeCacheForAppliedAngle)
		{
			return false;
		}
		output.Clear();
		for (int i = 0; i < sensedExternalColliders.Count; i++)
		{
			output.Add(sensedExternalColliders[i]);
		}
		for (int j = 0; j < mainBodyInterlockCount; j++)
		{
			AddUniqueCollider(output, mainBodyInterlockBuffer[j]);
		}
		if (ringRelatives != null)
		{
			for (int k = 0; k < ringRelatives.Count && k < relativeInterlockDirectCounts.Count; k++)
			{
				int num = relativeInterlockOffsets[k];
				int num2 = relativeInterlockDirectCounts[k];
				for (int l = 0; l < num2; l++)
				{
					AddUniqueCollider(output, relativeInterlockColliders[num + l]);
				}
			}
		}
		AppendProbeCachedCatchGuideColliders(output);
		return true;
	}

	private static void AddUniqueCollider(List<Collider2D> output, Collider2D col)
	{
		if ((Object)(object)col == (Object)null)
		{
			return;
		}
		for (int i = 0; i < output.Count; i++)
		{
			if ((Object)(object)output[i] == (Object)(object)col)
			{
				return;
			}
		}
		output.Add(col);
	}

	private int CollectPrimaryOverlapInto(Overlap2DHandler handler, Collider2D[] buffer)
	{
		if (handler == null)
		{
			return 0;
		}
		if (!primaryOverlapEnabled || handler != overlap2DHandler)
		{
			return handler.GetOverlapColliders(buffer);
		}
		if (!primaryOverlapValid)
		{
			primaryOverlapCount = handler.GetOverlapColliders(primaryOverlapCache);
			primaryOverlapValid = true;
		}
		for (int i = 0; i < primaryOverlapCount; i++)
		{
			buffer[i] = primaryOverlapCache[i];
		}
		return primaryOverlapCount;
	}

	private bool TryBuildDragContactCollidersFromDownCache(List<Collider2D> output)
	{
		if (ringRelatives != null && ringRelatives.Count > 0)
		{
			return false;
		}
		if (rotationProbeColliders.Count == 0)
		{
			return false;
		}
		output.Clear();
		for (int i = 0; i < sensedExternalColliders.Count; i++)
		{
			output.Add(sensedExternalColliders[i]);
		}
		for (int j = 0; j < currentInterlockColliderCount; j++)
		{
			AddUniqueCollider(output, interlockColliderBuffer[j]);
		}
		return true;
	}

	protected override void HandlePointerDown(GameplayPointerData data)
	{
		using (new ProfilerScope("RingObject.HandlePointerDown"))
		{
			SingletonComponent<GameplayManager>.Instance?.RingDragCollisionEventService?.SendAndClearData();
			ClearRotationBlockCache();
			RebuildRotationBlockCaches();
			primaryOverlapEnabled = true;
			primaryOverlapValid = false;
			try
			{
				dragStartExternalOverlapCount = CountExternalOverlaps();
				CaptureDragStartExternalColliderIds();
				dragStartMaxExternalPenetration = GetMaxExternalPenetrationFromCache();
				dragStartMaxHardBlockStickPenetration = GetMaxHardBlockStickPenetration();
				dragStartMaxInterlockPenetration = GetMaxPartnerMainBodyInterlockPenetration();
				if (!TryBuildDragContactCollidersFromDownCache(dragContactColliders))
				{
					CollectDragContactColliders(dragContactColliders);
				}
			}
			finally
			{
				primaryOverlapEnabled = false;
				primaryOverlapValid = false;
			}
			SingletonComponent<GameplayManager>.Instance?.RingDragCollisionEventService?.BeginDragEvent((Component)(object)this, dragContactColliders);
			base.HandlePointerDown(data);
		}
	}

	private void RebuildRotationBlockCaches()
	{
		RingDragCollisionEventService ringDragCollisionEventService = SingletonComponent<GameplayManager>.Instance?.RingDragCollisionEventService;
		if (ringDragCollisionEventService == null)
		{
			rotationProbeColliders.Clear();
			return;
		}
		Collider2D val = (((Object)(object)activeCollider != (Object)null && ((Behaviour)activeCollider).enabled) ? activeCollider : null);
		ringDragCollisionEventService.CollectSelfColliders(rotationProbeColliders, ringRelatives, 6, val);
		AppendHardBlockProbes(rotationProbeColliders);
	}

	private void AppendHardBlockProbes(List<Collider2D> output)
	{
		if (hardBlockColliders == null)
		{
			return;
		}
		for (int i = 0; i < hardBlockColliders.Length; i++)
		{
			Collider2D val = hardBlockColliders[i];
			if (!((Object)(object)val == (Object)null) && ((Behaviour)val).enabled && ((Component)val).gameObject.layer != 6 && !output.Contains(val))
			{
				output.Add(val);
			}
		}
	}

	private bool IsExternalObstacleCollider(Collider2D col)
	{
		if ((Object)(object)col == (Object)null)
		{
			return false;
		}
		if (((Component)col).gameObject.layer == 6)
		{
			return false;
		}
		if (RingColliderQuery.IsCatchGuideBodyCollider(col))
		{
			return false;
		}
		if (RingColliderQuery.TryGetPullObject(col, out var _))
		{
			return true;
		}
		FixedObject fixedObject;
		return RingColliderQuery.TryGetFixedObject(col, out fixedObject);
	}

	private void CollectSensedExternalColliders(List<Collider2D> output)
	{
		using (new ProfilerScope("RingObject.CollectSensedExternalColliders"))
		{
			output.Clear();
			CollectSensedExternalColliders(overlap2DHandler, output);
			if (ringRelatives == null)
			{
				return;
			}
			foreach (RingRelative ringRelative in ringRelatives)
			{
				if ((Object)(object)ringRelative != (Object)null)
				{
					int overlapColliders = ringRelative.GetOverlapColliders(overlapBuffer);
					for (int i = 0; i < overlapColliders; i++)
					{
						AddSensedExternalCollider(output, overlapBuffer[i]);
					}
				}
			}
		}
	}

	private void CollectSensedExternalColliders(Overlap2DHandler handler, List<Collider2D> output)
	{
		if (handler != null)
		{
			int num = CollectPrimaryOverlapInto(handler, overlapBuffer);
			for (int i = 0; i < num; i++)
			{
				AddSensedExternalCollider(output, overlapBuffer[i]);
			}
		}
	}

	private void AddSensedExternalCollider(List<Collider2D> output, Collider2D col)
	{
		if (!IsExternalObstacleCollider(col))
		{
			return;
		}
		for (int i = 0; i < output.Count; i++)
		{
			if ((Object)(object)output[i] == (Object)(object)col)
			{
				return;
			}
		}
		output.Add(col);
	}

	private void AppendProbeCachedCatchGuideColliders(List<Collider2D> output)
	{
		for (int i = 0; i < probeCachedCatchGuideColliders.Count; i++)
		{
			AddUniqueCollider(output, probeCachedCatchGuideColliders[i]);
		}
	}

	private void TryAddCatchGuideFromOverlap(Collider2D col)
	{
		if ((Object)(object)col != (Object)null && RingColliderQuery.IsCatchGuideBodyCollider(col))
		{
			AddUniqueCollider(probeCachedCatchGuideColliders, col);
		}
	}

	private void CollectCatchGuideBodyContactColliders(List<Collider2D> output)
	{
		CollectCatchGuideBodyFromOverlapHandler(overlap2DHandler, output);
		if (ringRelatives == null)
		{
			return;
		}
		foreach (RingRelative ringRelative in ringRelatives)
		{
			if ((Object)(object)ringRelative == (Object)null)
			{
				continue;
			}
			int overlapColliders = ringRelative.GetOverlapColliders(overlapBuffer);
			for (int i = 0; i < overlapColliders; i++)
			{
				Collider2D val = overlapBuffer[i];
				if ((Object)(object)val != (Object)null && RingColliderQuery.IsCatchGuideBodyCollider(val))
				{
					AddUniqueCollider(output, val);
				}
			}
		}
	}

	private void CollectCatchGuideBodyFromOverlapHandler(Overlap2DHandler handler, List<Collider2D> output)
	{
		if (handler == null)
		{
			return;
		}
		int overlapColliders = handler.GetOverlapColliders(overlapBuffer);
		for (int i = 0; i < overlapColliders; i++)
		{
			Collider2D val = overlapBuffer[i];
			if ((Object)(object)val != (Object)null && RingColliderQuery.IsCatchGuideBodyCollider(val))
			{
				AddUniqueCollider(output, val);
			}
		}
	}

	private RingRelative GetSelfRingRelative(Collision2D collision)
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		if (collision == null || collision.contactCount <= 0)
		{
			return null;
		}
		ContactPoint2D contact = collision.GetContact(0);
		Collider2D collider = collision.collider;
		Collider2D val = (((Object)(object)((ContactPoint2D)(ref contact)).collider == (Object)(object)collider) ? ((ContactPoint2D)(ref contact)).otherCollider : ((ContactPoint2D)(ref contact)).collider);
		if ((Object)(object)val == (Object)null)
		{
			return null;
		}
		RingRelative ringRelative = null;
		if (RingColliderQuery.TryGetMarker(val, out var marker))
		{
			ringRelative = marker.GetRingRelative();
		}
		if ((Object)(object)ringRelative == (Object)null)
		{
			ringRelative = ((Component)val).GetComponentInParent<RingRelative>();
		}
		if ((Object)(object)ringRelative == (Object)null || ringRelatives == null || !ringRelatives.Contains(ringRelative))
		{
			return null;
		}
		return ringRelative;
	}

	public bool IsOtherRelativeCollider(RingObject self, Collider2D col)
	{
		if ((Object)(object)col == (Object)null || (Object)(object)self == (Object)null)
		{
			return false;
		}
		if (!RingColliderQuery.TryGetMarker(col, out var marker))
		{
			return false;
		}
		if ((Object)(object)marker.GetRingRelative() == (Object)null)
		{
			return false;
		}
		RingObject ringObject = marker.Owner as RingObject;
		if ((Object)(object)ringObject != (Object)null)
		{
			return (Object)(object)ringObject != (Object)(object)self;
		}
		return false;
	}

	private bool IsOtherRingCollider(RingObject self, Collider2D col)
	{
		RingObject ring;
		return TryGetOtherRingByCollider(self, col, out ring);
	}

	public bool HasRingOverlapWith(RingObject other)
	{
		if ((Object)(object)other == (Object)null || (Object)(object)other == (Object)(object)this || IsSolved || other.IsSolved)
		{
			return false;
		}
		int num = CollectCurrentInterlockColliders(interlockColliderBuffer, currentInterlockColliderIds);
		for (int i = 0; i < num; i++)
		{
			if (TryGetOtherRingByCollider(this, interlockColliderBuffer[i], out var ring) && (Object)(object)ring == (Object)(object)other)
			{
				return true;
			}
		}
		return false;
	}

	public bool TryCollectInterlockedPartners(HashSet<RingObject> output)
	{
		output.Clear();
		if (IsSolved)
		{
			return false;
		}
		int num = CollectCurrentInterlockColliders(interlockColliderBuffer, currentInterlockColliderIds);
		for (int i = 0; i < num; i++)
		{
			if (TryGetOtherRingByCollider(this, interlockColliderBuffer[i], out var ring) && !ring.IsSolved)
			{
				output.Add(ring);
			}
		}
		return true;
	}

	private bool TryGetOtherRingByCollider(RingObject self, Collider2D col, out RingObject ring)
	{
		ring = null;
		if ((Object)(object)col == (Object)null)
		{
			return false;
		}
		if (((Component)col).gameObject.layer == 6)
		{
			return false;
		}
		if (!RingColliderQuery.TryGetRingObject(col, out ring) || (Object)(object)ring == (Object)(object)self)
		{
			ring = null;
			return false;
		}
		return true;
	}

	private int CollectCurrentInterlockColliders(Collider2D[] buffer, HashSet<int> idOutput)
	{
		using (new ProfilerScope("RingObject.CollectCurrentInterlockColliders"))
		{
			idOutput.Clear();
			int num = 0;
			num += CollectOverlapCollidersFromBody(overlap2DHandler, buffer, num, idOutput);
			if (ringRelatives == null)
			{
				currentInterlockColliderCount = num;
				return num;
			}
			foreach (RingRelative ringRelative in ringRelatives)
			{
				if (!((Object)(object)ringRelative == (Object)null))
				{
					int overlapColliders = ringRelative.GetOverlapColliders(overlapBuffer);
					for (int i = 0; i < overlapColliders; i++)
					{
						num += TryAddOtherRingCollider(buffer, num, idOutput, overlapBuffer[i]);
					}
				}
			}
			currentInterlockColliderCount = num;
			return num;
		}
	}

	private int CollectOverlapCollidersFromBody(Overlap2DHandler handler, Collider2D[] buffer, int startIndex, HashSet<int> idOutput)
	{
		if (handler == null)
		{
			return 0;
		}
		int num = 0;
		int num2 = CollectPrimaryOverlapInto(handler, overlapBuffer);
		for (int i = 0; i < num2; i++)
		{
			num += TryAddOtherRingCollider(buffer, startIndex + num, idOutput, overlapBuffer[i]);
		}
		return num;
	}

	private int TryAddOtherRingCollider(Collider2D[] buffer, int index, HashSet<int> idOutput, Collider2D col)
	{
		if (!IsOtherRingCollider(this, col))
		{
			return 0;
		}
		idOutput.Add(((Object)col).GetInstanceID());
		if (buffer != null && index < buffer.Length)
		{
			buffer[index] = col;
		}
		return 1;
	}

	private void CollectInterlockColliderIdsFromHandler(Overlap2DHandler handler, HashSet<int> output)
	{
		if (handler == null)
		{
			return;
		}
		int overlapColliders = handler.GetOverlapColliders(overlapBuffer);
		for (int i = 0; i < overlapColliders; i++)
		{
			if (IsOtherRingCollider(this, overlapBuffer[i]))
			{
				output.Add(((Object)overlapBuffer[i]).GetInstanceID());
			}
		}
	}

	private void CollectOverlapCollidersFromWalls(RingRelative relative, List<Collider2D> output)
	{
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)relative == (Object)null)
		{
			return;
		}
		Collider2D[] blockColliders = relative.BlockColliders;
		if (blockColliders == null)
		{
			return;
		}
		foreach (Collider2D val in blockColliders)
		{
			if ((Object)(object)val == (Object)null || !((Behaviour)val).enabled || val.isTrigger || ((Component)val).gameObject.layer == 6 || ((Object)((Component)val).transform).name == "Bottom")
			{
				continue;
			}
			int num = val.Overlap(RelativeWallOverlapFilter, relativeWallOverlapBuffer);
			for (int j = 0; j < num; j++)
			{
				Collider2D val2 = relativeWallOverlapBuffer[j];
				if ((Object)(object)val2 == (Object)null)
				{
					continue;
				}
				bool flag = false;
				for (int k = 0; k < output.Count; k++)
				{
					if ((Object)(object)output[k] == (Object)(object)val2)
					{
						flag = true;
						break;
					}
				}
				if (!flag)
				{
					output.Add(val2);
				}
			}
		}
	}

	private static ContactFilter2D CreateRelativeWallOverlapFilter()
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

	private int CountExternalOverlaps()
	{
		CollectSensedExternalColliders(sensedExternalColliders);
		return sensedExternalColliders.Count;
	}

	private void CaptureDragStartExternalColliderIds()
	{
		dragStartExternalColliderIds.Clear();
		for (int i = 0; i < sensedExternalColliders.Count; i++)
		{
			Collider2D val = sensedExternalColliders[i];
			if ((Object)(object)val != (Object)null)
			{
				dragStartExternalColliderIds.Add(((Object)val).GetInstanceID());
			}
		}
	}

	private float GetMaxExternalPenetration()
	{
		if (rotationProbeColliders.Count == 0)
		{
			return 0f;
		}
		CollectSensedExternalColliders(sensedExternalColliders);
		return GetMaxExternalPenetration(sensedExternalColliders);
	}

	private float GetMaxExternalPenetrationFromCache()
	{
		if (rotationProbeColliders.Count == 0)
		{
			return 0f;
		}
		return GetMaxExternalPenetration(sensedExternalColliders);
	}

	private float GetMaxExternalPenetration(List<Collider2D> externalColliders)
	{
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		if (rotationProbeColliders.Count == 0 || externalColliders.Count == 0)
		{
			return 0f;
		}
		float num = 0f;
		for (int i = 0; i < rotationProbeColliders.Count; i++)
		{
			Collider2D val = rotationProbeColliders[i];
			if ((Object)(object)val == (Object)null || !((Behaviour)val).enabled)
			{
				continue;
			}
			for (int j = 0; j < externalColliders.Count; j++)
			{
				Collider2D val2 = externalColliders[j];
				if (!((Object)(object)val2 == (Object)null) && ((Behaviour)val2).enabled)
				{
					ColliderDistance2D val3 = Physics2D.Distance(val, val2);
					if (((ColliderDistance2D)(ref val3)).distance < 0f)
					{
						num = Mathf.Max(num, 0f - ((ColliderDistance2D)(ref val3)).distance);
					}
				}
			}
		}
		return num;
	}

	private int CollectInterlockCollidersWithWalls(Collider2D[] buffer, HashSet<int> idOutput)
	{
		idOutput.Clear();
		int num = CollectOverlapCollidersFromBody(overlap2DHandler, buffer, 0, idOutput);
		if (ringRelatives == null)
		{
			return num;
		}
		for (int i = 0; i < ringRelatives.Count; i++)
		{
			RingRelative ringRelative = ringRelatives[i];
			if (!((Object)(object)ringRelative == (Object)null))
			{
				int overlapColliders = ringRelative.GetOverlapColliders(overlapBuffer);
				for (int j = 0; j < overlapColliders; j++)
				{
					num += TryAddOtherRingCollider(buffer, num, idOutput, overlapBuffer[j]);
				}
				relativeWallOverlapResults.Clear();
				CollectOverlapCollidersFromWalls(ringRelative, relativeWallOverlapResults);
				for (int k = 0; k < relativeWallOverlapResults.Count; k++)
				{
					num += TryAddOtherRingCollider(buffer, num, idOutput, relativeWallOverlapResults[k]);
				}
			}
		}
		return num;
	}

	private float GetMaxPartnerMainBodyInterlockPenetration()
	{
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		if (rotationProbeColliders.Count == 0)
		{
			return 0f;
		}
		int num = (currentInterlockColliderCount = CollectInterlockCollidersWithWalls(interlockColliderBuffer, currentInterlockColliderIds));
		if (num <= 0)
		{
			return 0f;
		}
		float num2 = 0f;
		for (int i = 0; i < rotationProbeColliders.Count; i++)
		{
			Collider2D val = rotationProbeColliders[i];
			if ((Object)(object)val == (Object)null || !((Behaviour)val).enabled)
			{
				continue;
			}
			for (int j = 0; j < num; j++)
			{
				Collider2D val2 = interlockColliderBuffer[j];
				if (!((Object)(object)val2 == (Object)null) && ((Behaviour)val2).enabled && IsOtherBodyCollider(this, val2))
				{
					ColliderDistance2D val3 = Physics2D.Distance(val, val2);
					if (((ColliderDistance2D)(ref val3)).isValid && ((ColliderDistance2D)(ref val3)).distance < 0f)
					{
						num2 = Mathf.Max(num2, 0f - ((ColliderDistance2D)(ref val3)).distance);
					}
				}
			}
		}
		return num2;
	}

	protected bool IsBlockedForRotation()
	{
		using (new ProfilerScope("RingObject.IsBlockedForRotation"))
		{
			CollectOverlapCache();
			if (HasNewBlockingExternalObstacle())
			{
				TryCaptureRotationBlockCollider();
				return true;
			}
			if (GetMaxExternalPenetration(sensedExternalColliders) > dragStartMaxExternalPenetration + 0.02f)
			{
				TryCaptureRotationBlockCollider();
				return true;
			}
			if (HasHardBlockOverlap(sensedExternalColliders))
			{
				TryCaptureRotationBlockCollider();
				return true;
			}
			if (GetMaxPartnerMainBodyInterlockPenetrationFromCache() > dragStartMaxInterlockPenetration + 0.02f)
			{
				TryCaptureRotationBlockCollider();
				return true;
			}
			RingLockPassiveService lockPassive = SingletonComponent<GameplayManager>.Instance?.LockPassive;
			if (HasNewInterlockBlockFromCache(lockPassive, null))
			{
				TryCaptureRotationBlockCollider();
				return true;
			}
			for (int i = 0; i < ringRelatives.Count; i++)
			{
				RingRelative ringRelative = ringRelatives[i];
				if (!((Object)(object)ringRelative == (Object)null) && HasNewInterlockBlockFromCache(lockPassive, ringRelative, i))
				{
					TryCaptureRotationBlockCollider();
					return true;
				}
			}
			if (HasSelfBombContactBlock())
			{
				return true;
			}
			return false;
		}
	}

	private bool HasSelfBombContactBlock()
	{
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0161: Unknown result type (might be due to invalid IL or missing references)
		//IL_0166: Unknown result type (might be due to invalid IL or missing references)
		//IL_0180: Unknown result type (might be due to invalid IL or missing references)
		//IL_0187: Unknown result type (might be due to invalid IL or missing references)
		//IL_018c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0196: Unknown result type (might be due to invalid IL or missing references)
		//IL_0174: Unknown result type (might be due to invalid IL or missing references)
		//IL_017a: Unknown result type (might be due to invalid IL or missing references)
		//IL_019b: Unknown result type (might be due to invalid IL or missing references)
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
				if (!((Object)(object)transform == (Object)(object)((Component)this).transform) && !transform.IsChildOf(((Component)this).transform) && !RingColliderQuery.IsCatchGuideBodyCollider(val2))
				{
					bool flag = false;
					PullObject pull;
					FixedObject fixedObject;
					if (RingColliderQuery.TryGetRingObject(val2, out var ring))
					{
						flag = (Object)(object)ring != (Object)(object)this && !ring.IsSolved;
					}
					else if (RingColliderQuery.TryGetPullObject(val2, out pull))
					{
						flag = !pull.IsSolved;
					}
					else if (RingColliderQuery.TryGetFixedObject(val2, out fixedObject))
					{
						flag = !fixedObject.IsSolved;
					}
					if (flag)
					{
						bombRelative.TryExplodeFromSelfContact(val2);
						cachedRotationBlockCollider = val2;
						cachedRotationBlockProbe = val;
						ColliderDistance2D val3 = Physics2D.Distance(val, val2);
						cachedRotationBlockContact = (Vector2)(((ColliderDistance2D)(ref val3)).isValid ? ((((ColliderDistance2D)(ref val3)).pointA + ((ColliderDistance2D)(ref val3)).pointB) * 0.5f) : default(Vector2));
						return true;
					}
				}
			}
		}
		return false;
	}

	private bool HasNewBlockingExternalObstacle()
	{
		for (int i = 0; i < sensedExternalColliders.Count; i++)
		{
			Collider2D val = sensedExternalColliders[i];
			if (!((Object)(object)val == (Object)null) && ((Behaviour)val).enabled && !dragStartExternalColliderIds.Contains(((Object)val).GetInstanceID()))
			{
				return true;
			}
		}
		return false;
	}

	private void CollectOverlapCache()
	{
		using (new ProfilerScope("RingObject.CollectOverlapCache"))
		{
			sensedExternalColliders.Clear();
			probeCachedCatchGuideColliders.Clear();
			mainBodyInterlockCount = 0;
			relativeInterlockColliders.Clear();
			relativeInterlockOffsets.Clear();
			relativeInterlockCounts.Clear();
			relativeInterlockDirectCounts.Clear();
			if (overlap2DHandler != null)
			{
				int overlapColliders = overlap2DHandler.GetOverlapColliders(overlapBuffer);
				for (int i = 0; i < overlapColliders; i++)
				{
					Collider2D col = overlapBuffer[i];
					AddSensedExternalCollider(sensedExternalColliders, col);
					TryAddInterlockCollider(mainBodyInterlockBuffer, ref mainBodyInterlockCount, col, this);
					TryAddCatchGuideFromOverlap(col);
				}
			}
			if (ringRelatives == null)
			{
				return;
			}
			for (int j = 0; j < ringRelatives.Count; j++)
			{
				RingRelative ringRelative = ringRelatives[j];
				relativeInterlockOffsets.Add(relativeInterlockColliders.Count);
				int num = 0;
				int item = 0;
				if ((Object)(object)ringRelative != (Object)null)
				{
					int overlapColliders2 = ringRelative.GetOverlapColliders(overlapBuffer);
					for (int k = 0; k < overlapColliders2; k++)
					{
						Collider2D val = overlapBuffer[k];
						AddSensedExternalCollider(sensedExternalColliders, val);
						TryAddCatchGuideFromOverlap(val);
						if (IsOtherRingCollider(this, val))
						{
							relativeInterlockColliders.Add(val);
							num++;
						}
					}
					item = num;
					relativeWallOverlapResults.Clear();
					CollectOverlapCollidersFromWalls(ringRelative, relativeWallOverlapResults);
					for (int l = 0; l < relativeWallOverlapResults.Count; l++)
					{
						Collider2D val2 = relativeWallOverlapResults[l];
						if (!((Object)(object)val2 == (Object)null) && IsOtherRingCollider(this, val2))
						{
							relativeInterlockColliders.Add(val2);
							num++;
						}
					}
				}
				relativeInterlockDirectCounts.Add(item);
				relativeInterlockCounts.Add(num);
			}
		}
	}

	private void TryAddInterlockCollider(Collider2D[] buffer, ref int count, Collider2D col, RingObject self)
	{
		if (!IsOtherRingCollider(self, col))
		{
			return;
		}
		for (int i = 0; i < count; i++)
		{
			if ((Object)(object)buffer[i] == (Object)(object)col)
			{
				return;
			}
		}
		if (count < buffer.Length)
		{
			buffer[count++] = col;
		}
	}

	private bool HasNewInterlockBlockFromCache(RingLockPassiveService lockPassive, RingRelative ownerSource)
	{
		int num = mainBodyInterlockCount;
		if (num <= 0)
		{
			return false;
		}
		return lockPassive?.HasNewCollider(this, ownerSource, mainBodyInterlockBuffer, num) ?? true;
	}

	private bool HasNewInterlockBlockFromCache(RingLockPassiveService lockPassive, RingRelative ownerSource, int relativeIndex)
	{
		if (relativeIndex >= relativeInterlockCounts.Count)
		{
			return false;
		}
		int num = relativeInterlockCounts[relativeIndex];
		if (num <= 0)
		{
			return false;
		}
		if (lockPassive == null)
		{
			return true;
		}
		int num2 = relativeInterlockOffsets[relativeIndex];
		int num3 = Math.Min(num, interlockColliderBuffer.Length);
		for (int i = 0; i < num3; i++)
		{
			interlockColliderBuffer[i] = relativeInterlockColliders[num2 + i];
		}
		return lockPassive.HasNewCollider(this, ownerSource, interlockColliderBuffer, num3);
	}

	private float GetMaxPartnerMainBodyInterlockPenetrationFromCache()
	{
		if (rotationProbeColliders.Count == 0)
		{
			return 0f;
		}
		CollectOtherBodyInterlocks(mainBodyInterlockBuffer, mainBodyInterlockCount, otherBodyInterlockScratch);
		float maxProbeInterlockPenetration = GetMaxProbeInterlockPenetration(otherBodyInterlockScratch);
		CollectOtherBodyInterlocks(relativeInterlockColliders, otherBodyInterlockScratch);
		return Mathf.Max(maxProbeInterlockPenetration, GetMaxProbeInterlockPenetration(otherBodyInterlockScratch));
	}

	private void CollectOtherBodyInterlocks(Collider2D[] buffer, int count, List<Collider2D> output)
	{
		output.Clear();
		for (int i = 0; i < count; i++)
		{
			Collider2D val = buffer[i];
			if (!((Object)(object)val == (Object)null) && ((Behaviour)val).enabled && IsOtherBodyCollider(this, val))
			{
				output.Add(val);
			}
		}
	}

	private void CollectOtherBodyInterlocks(List<Collider2D> source, List<Collider2D> output)
	{
		output.Clear();
		for (int i = 0; i < source.Count; i++)
		{
			Collider2D val = source[i];
			if (!((Object)(object)val == (Object)null) && ((Behaviour)val).enabled && IsOtherBodyCollider(this, val))
			{
				output.Add(val);
			}
		}
	}

	private float GetMaxProbeInterlockPenetration(List<Collider2D> interlocks)
	{
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		float num = 0f;
		for (int i = 0; i < rotationProbeColliders.Count; i++)
		{
			Collider2D val = rotationProbeColliders[i];
			if ((Object)(object)val == (Object)null || !((Behaviour)val).enabled)
			{
				continue;
			}
			for (int j = 0; j < interlocks.Count; j++)
			{
				ColliderDistance2D val2 = Physics2D.Distance(val, interlocks[j]);
				if (((ColliderDistance2D)(ref val2)).isValid && ((ColliderDistance2D)(ref val2)).distance < 0f)
				{
					num = Mathf.Max(num, 0f - ((ColliderDistance2D)(ref val2)).distance);
				}
			}
		}
		return num;
	}

	private bool HasHardBlockOverlap()
	{
		CollectSensedExternalColliders(sensedExternalColliders);
		return HasHardBlockOverlap(sensedExternalColliders);
	}

	private bool HasHardBlockOverlap(List<Collider2D> externalColliders)
	{
		if (TryResolveHardBlockInterlockOverlap(out var blocker, out var probe, out var contact))
		{
			return true;
		}
		if (GetMaxHardBlockStickPenetration(externalColliders) <= dragStartMaxHardBlockStickPenetration + 0.02f)
		{
			return false;
		}
		return TryResolveHardBlockStickOverlap(externalColliders, out probe, out blocker, out contact);
	}

	private bool TryResolveHardBlockInterlockOverlap(out Collider2D blocker, out Collider2D probe, out Vector2 contact)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d5: Unknown result type (might be due to invalid IL or missing references)
		blocker = null;
		probe = null;
		contact = default(Vector2);
		if (hardBlockColliders == null || hardBlockColliders.Length == 0)
		{
			return false;
		}
		float num = 0f;
		ContactFilter2D noFilter = ContactFilter2D.noFilter;
		for (int i = 0; i < hardBlockColliders.Length; i++)
		{
			Collider2D val = hardBlockColliders[i];
			if ((Object)(object)val == (Object)null || !((Behaviour)val).enabled)
			{
				continue;
			}
			int num2 = val.Overlap(noFilter, interlockColliderBuffer);
			for (int j = 0; j < num2; j++)
			{
				Collider2D val2 = interlockColliderBuffer[j];
				if (!IsOtherRingCollider(this, val2))
				{
					continue;
				}
				ColliderDistance2D val3 = Physics2D.Distance(val, val2);
				if (((ColliderDistance2D)(ref val3)).isValid && ((ColliderDistance2D)(ref val3)).distance < 0f)
				{
					float num3 = 0f - ((ColliderDistance2D)(ref val3)).distance;
					if (num3 > num)
					{
						num = num3;
						blocker = val2;
						probe = val;
						contact = (((ColliderDistance2D)(ref val3)).pointA + ((ColliderDistance2D)(ref val3)).pointB) * 0.5f;
					}
				}
			}
		}
		return (Object)(object)blocker != (Object)null;
	}

	private float MeasureHardBlockStickPenetration(Collider2D hardProbe, Collider2D obstacle)
	{
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)hardProbe == (Object)null || (Object)(object)obstacle == (Object)null || !((Behaviour)obstacle).enabled || !IsExternalObstacleCollider(obstacle))
		{
			return 0f;
		}
		ColliderDistance2D val = Physics2D.Distance(hardProbe, obstacle);
		if (!((ColliderDistance2D)(ref val)).isValid || ((ColliderDistance2D)(ref val)).distance >= 0.03f)
		{
			return 0f;
		}
		return 0.03f - ((ColliderDistance2D)(ref val)).distance;
	}

	private float GetMaxHardBlockStickPenetration()
	{
		if (hardBlockColliders == null || hardBlockColliders.Length == 0)
		{
			return 0f;
		}
		CollectSensedExternalColliders(sensedExternalColliders);
		return GetMaxHardBlockStickPenetration(sensedExternalColliders);
	}

	private float GetMaxHardBlockStickPenetration(List<Collider2D> externalColliders)
	{
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		if (hardBlockColliders == null || hardBlockColliders.Length == 0)
		{
			return 0f;
		}
		float num = 0f;
		ContactFilter2D noFilter = ContactFilter2D.noFilter;
		for (int i = 0; i < hardBlockColliders.Length; i++)
		{
			Collider2D val = hardBlockColliders[i];
			if (!((Object)(object)val == (Object)null) && ((Behaviour)val).enabled)
			{
				for (int j = 0; j < externalColliders.Count; j++)
				{
					num = Mathf.Max(num, MeasureHardBlockStickPenetration(val, externalColliders[j]));
				}
				int num2 = val.Overlap(noFilter, interlockColliderBuffer);
				for (int k = 0; k < num2; k++)
				{
					num = Mathf.Max(num, MeasureHardBlockStickPenetration(val, interlockColliderBuffer[k]));
				}
			}
		}
		return num;
	}

	private bool TryResolveHardBlockStickOverlap(out Collider2D blocker, out Collider2D probe, out Vector2 contact)
	{
		CollectSensedExternalColliders(sensedExternalColliders);
		return TryResolveHardBlockStickOverlap(sensedExternalColliders, out blocker, out probe, out contact);
	}

	private bool TryResolveHardBlockStickOverlap(List<Collider2D> externalColliders, out Collider2D blocker, out Collider2D probe, out Vector2 contact)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0102: Unknown result type (might be due to invalid IL or missing references)
		//IL_0107: Unknown result type (might be due to invalid IL or missing references)
		//IL_012c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0133: Unknown result type (might be due to invalid IL or missing references)
		//IL_0138: Unknown result type (might be due to invalid IL or missing references)
		//IL_0142: Unknown result type (might be due to invalid IL or missing references)
		//IL_0120: Unknown result type (might be due to invalid IL or missing references)
		//IL_0126: Unknown result type (might be due to invalid IL or missing references)
		//IL_0147: Unknown result type (might be due to invalid IL or missing references)
		blocker = null;
		probe = null;
		contact = default(Vector2);
		if (hardBlockColliders == null || hardBlockColliders.Length == 0)
		{
			return false;
		}
		float num = 0f;
		ContactFilter2D noFilter = ContactFilter2D.noFilter;
		for (int i = 0; i < hardBlockColliders.Length; i++)
		{
			Collider2D val = hardBlockColliders[i];
			if ((Object)(object)val == (Object)null || !((Behaviour)val).enabled)
			{
				continue;
			}
			for (int j = 0; j < externalColliders.Count; j++)
			{
				Collider2D val2 = externalColliders[j];
				float num2 = MeasureHardBlockStickPenetration(val, val2);
				if (num2 > num)
				{
					ColliderDistance2D val3 = Physics2D.Distance(val, val2);
					num = num2;
					blocker = val2;
					probe = val;
					contact = (Vector2)(((ColliderDistance2D)(ref val3)).isValid ? ((((ColliderDistance2D)(ref val3)).pointA + ((ColliderDistance2D)(ref val3)).pointB) * 0.5f) : default(Vector2));
				}
			}
			int num3 = val.Overlap(noFilter, interlockColliderBuffer);
			for (int k = 0; k < num3; k++)
			{
				Collider2D val4 = interlockColliderBuffer[k];
				float num4 = MeasureHardBlockStickPenetration(val, val4);
				if (num4 > num)
				{
					ColliderDistance2D val5 = Physics2D.Distance(val, val4);
					num = num4;
					blocker = val4;
					probe = val;
					contact = (Vector2)(((ColliderDistance2D)(ref val5)).isValid ? ((((ColliderDistance2D)(ref val5)).pointA + ((ColliderDistance2D)(ref val5)).pointB) * 0.5f) : default(Vector2));
				}
			}
		}
		return (Object)(object)blocker != (Object)null;
	}

	private bool TryResolveHardBlockOverlap(out Collider2D blocker, out Collider2D probe, out Vector2 contact)
	{
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		if (TryResolveHardBlockInterlockOverlap(out blocker, out probe, out contact))
		{
			return true;
		}
		if (GetMaxHardBlockStickPenetration() <= dragStartMaxHardBlockStickPenetration + 0.02f)
		{
			blocker = null;
			probe = null;
			contact = default(Vector2);
			return false;
		}
		return TryResolveHardBlockStickOverlap(out blocker, out probe, out contact);
	}

	protected override bool IsRotationBlocked()
	{
		return IsBlockedForRotation();
	}

	protected override void OnRotationBlocked(float attemptedAngle)
	{
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		RingDragCollisionEventService ringDragCollisionEventService = SingletonComponent<GameplayManager>.Instance?.RingDragCollisionEventService;
		if (ringDragCollisionEventService != null && (TryGetRotationBlockCollider(out var blocker, out var probe, out var contact) || TryCaptureRotationBlockCollider()) && TryGetRotationBlockCollider(out blocker, out probe, out contact))
		{
			ringDragCollisionEventService.SendStopEvent((Component)(object)this, blocker, contact, Mathf.Sign(attemptedAngle), RingDragMoveKind.Rotate);
			SingletonComponent<GameplayManager>.Instance?.RingBoom?.ProbeBombContactsDuringDrag(rotationProbeColliders, this);
			if ((Object)(object)cachedRotationBlockCollider != (Object)null)
			{
				BombRelative obj = ((Component)cachedRotationBlockCollider).GetComponent<BombRelative>() ?? ((Component)cachedRotationBlockCollider).GetComponentInParent<BombRelative>();
				Collider2D target = cachedRotationBlockProbe ?? ((rotationProbeColliders.Count > 0) ? rotationProbeColliders[0] : activeCollider);
				obj?.TryExplodeFromSelfContact(target);
			}
		}
	}

	private void RollbackRotationAfterStop(float moveDirection)
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		if (!Mathf.Approximately(moveDirection, 0f))
		{
			float z = ((Component)this).transform.localEulerAngles.z;
			float num = ((!(Mathf.Abs(Mathf.DeltaAngle(startRotation, z)) <= 0.1f)) ? Mathf.Repeat(z - moveDirection * 0.1f, 360f) : startRotation);
			ApplyRotationZ(num);
			lastValidRotationZ = num;
			rotateAngle = Mathf.DeltaAngle(startRotation, num);
		}
	}

	private bool TryResolveBlockerContactForRotation(Collider2D blocker, out Collider2D probe, out Vector2 contact)
	{
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		RingDragCollisionEventService ringDragCollisionEventService = SingletonComponent<GameplayManager>.Instance?.RingDragCollisionEventService;
		Collider2D val = ((rotationProbeColliders.Count > 0) ? rotationProbeColliders[0] : activeCollider);
		if (ringDragCollisionEventService == null)
		{
			probe = val;
			contact = default(Vector2);
			return false;
		}
		return ringDragCollisionEventService.TryGetPosFromBlockerCollider(blocker, rotationProbeColliders, val, out probe, out contact);
	}

	private bool TryResolvePartnerMainBodyPenetrationBlocker(out Collider2D blocker, out Collider2D probe, out Vector2 contact)
	{
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_0103: Unknown result type (might be due to invalid IL or missing references)
		//IL_0108: Unknown result type (might be due to invalid IL or missing references)
		blocker = null;
		probe = ((rotationProbeColliders.Count > 0) ? rotationProbeColliders[0] : activeCollider);
		contact = default(Vector2);
		int num = (currentInterlockColliderCount = CollectInterlockCollidersWithWalls(interlockColliderBuffer, currentInterlockColliderIds));
		if (num <= 0)
		{
			return false;
		}
		float num2 = 0f;
		for (int i = 0; i < rotationProbeColliders.Count; i++)
		{
			Collider2D val = rotationProbeColliders[i];
			if ((Object)(object)val == (Object)null || !((Behaviour)val).enabled)
			{
				continue;
			}
			for (int j = 0; j < num; j++)
			{
				Collider2D val2 = interlockColliderBuffer[j];
				if ((Object)(object)val2 == (Object)null || !((Behaviour)val2).enabled || !IsOtherBodyCollider(this, val2))
				{
					continue;
				}
				ColliderDistance2D val3 = Physics2D.Distance(val, val2);
				if (((ColliderDistance2D)(ref val3)).isValid && ((ColliderDistance2D)(ref val3)).distance < 0f)
				{
					float num3 = 0f - ((ColliderDistance2D)(ref val3)).distance;
					if (num3 > num2)
					{
						num2 = num3;
						blocker = val2;
						probe = val;
						contact = (((ColliderDistance2D)(ref val3)).pointA + ((ColliderDistance2D)(ref val3)).pointB) * 0.5f;
					}
				}
			}
		}
		return (Object)(object)blocker != (Object)null;
	}

	private bool TryResolveRotationBlockCollider(out Collider2D blocker, out Collider2D probe, out Vector2 contact)
	{
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e8: Unknown result type (might be due to invalid IL or missing references)
		blocker = null;
		probe = ((rotationProbeColliders.Count > 0) ? rotationProbeColliders[0] : activeCollider);
		contact = default(Vector2);
		CollectSensedExternalColliders(sensedExternalColliders);
		if (sensedExternalColliders.Count > dragStartExternalOverlapCount)
		{
			float num = 0f;
			for (int i = 0; i < sensedExternalColliders.Count; i++)
			{
				Collider2D val = sensedExternalColliders[i];
				if ((Object)(object)val == (Object)null)
				{
					continue;
				}
				for (int j = 0; j < rotationProbeColliders.Count; j++)
				{
					Collider2D val2 = rotationProbeColliders[j];
					if ((Object)(object)val2 == (Object)null)
					{
						continue;
					}
					ColliderDistance2D val3 = Physics2D.Distance(val2, val);
					if (((ColliderDistance2D)(ref val3)).isValid && ((ColliderDistance2D)(ref val3)).distance < 0f)
					{
						float num2 = 0f - ((ColliderDistance2D)(ref val3)).distance;
						if (num2 > num)
						{
							num = num2;
							blocker = val;
							contact = (((ColliderDistance2D)(ref val3)).pointA + ((ColliderDistance2D)(ref val3)).pointB) * 0.5f;
							probe = val2;
						}
					}
				}
			}
			if ((Object)(object)blocker != (Object)null)
			{
				return true;
			}
		}
		if (TryResolveHardBlockOverlap(out blocker, out probe, out contact))
		{
			return true;
		}
		if (GetMaxPartnerMainBodyInterlockPenetration() > dragStartMaxInterlockPenetration + 0.02f && TryResolvePartnerMainBodyPenetrationBlocker(out blocker, out probe, out contact))
		{
			return true;
		}
		CollectCurrentInterlockColliders(interlockColliderBuffer, currentInterlockColliderIds);
		RingLockPassiveService ringLockPassiveService = SingletonComponent<GameplayManager>.Instance?.LockPassive;
		if (ringLockPassiveService != null)
		{
			if (ringLockPassiveService.TryResolveBlocker(this, null, out blocker))
			{
				return TryResolveBlockerContactForRotation(blocker, out probe, out contact);
			}
			if (ringRelatives != null)
			{
				for (int k = 0; k < ringRelatives.Count; k++)
				{
					RingRelative ringRelative = ringRelatives[k];
					if ((Object)(object)ringRelative != (Object)null && ringLockPassiveService.TryResolveBlocker(this, ringRelative, out blocker))
					{
						return TryResolveBlockerContactForRotation(blocker, out probe, out contact);
					}
				}
			}
		}
		else if (currentInterlockColliderCount > 0)
		{
			blocker = interlockColliderBuffer[0];
			if ((Object)(object)blocker != (Object)null)
			{
				return TryResolveBlockerContactForRotation(blocker, out probe, out contact);
			}
		}
		if (GetMaxExternalPenetration() > dragStartMaxExternalPenetration + 0.02f && sensedExternalColliders.Count > 0)
		{
			blocker = sensedExternalColliders[0];
			if ((Object)(object)blocker != (Object)null)
			{
				return TryResolveBlockerContactForRotation(blocker, out probe, out contact);
			}
			return false;
		}
		return false;
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

	public void SetColorLockActive(bool active)
	{
		paletteLockFlags = (active ? (paletteLockFlags | PaletteLockFlags.ColorLock) : (paletteLockFlags & ~PaletteLockFlags.ColorLock));
		NormalizePaletteLockFlags();
	}

	private void NormalizePaletteLockFlags()
	{
		if (paletteLockFlags == (PaletteLockFlags)0)
		{
			paletteLockFlags = PaletteLockFlags.None;
		}
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

	public void SetRotationGeometry(float innerRadius, float outerRadius, float gapAngleDeg, float gapDirectionDeg)
	{
		float num = Mathf.Abs(innerRadius);
		float num2 = Mathf.Abs(outerRadius);
		if (num2 < num)
		{
			float num3 = num;
			num = num2;
			num2 = num3;
		}
		gapGeometry = new OpenRingThreadableGapQuery.RingGapGeometry
		{
			InnerRadius = num,
			OuterRadius = num2,
			GapAngleDeg = Mathf.Repeat(Mathf.Abs(gapAngleDeg), 360f),
			GapDirectionDeg = gapDirectionDeg,
			IsValid = (num2 > 0.0001f)
		};
	}

	public bool TryGetBandArcLength(out float length)
	{
		EnsureGapGeometryFallback();
		length = 0f;
		if (!gapGeometry.IsValid)
		{
			return false;
		}
		float num = 360f - gapGeometry.GapAngleDeg;
		if (num <= 0f)
		{
			return false;
		}
		float num2 = (gapGeometry.InnerRadius + gapGeometry.OuterRadius) * 0.5f;
		if (num2 <= 0.0001f)
		{
			return false;
		}
		length = num / 360f * (MathF.PI * 2f * num2);
		return length > 0.0001f;
	}

	protected override float AdjustRotationDelta(float fingerDeltaDeg)
	{
		EnsureGapGeometryFallback();
		return rotationFeel.Adjust(fingerDeltaDeg, this, in gapGeometry, overlap2DHandler);
	}

	protected override void OnRotationGestureBegin()
	{
		rotationFeel.BeginGesture(((Object)this).name);
	}

	protected override void OnRotationGestureEnd()
	{
		FinalizeDragCollisionContactsOnPointerUp();
		rotationFeel.EndGesture();
		SingletonComponent<GameplayManager>.Instance?.RingDragCollisionEventService?.SendAndClearData((Component)(object)this);
	}

	protected override void OnRotationDragFrame(float fingerDeltaDeg, float adjustedDeltaDeg, float ringAppliedDeltaDeg, Vector3 fingerWorldPos, Vector2 fingerScreenPos)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		rotationFeel.NotifyApplied(ringAppliedDeltaDeg, fingerScreenPos);
		if (Mathf.Abs(ringAppliedDeltaDeg) > 0.001f)
		{
			SingletonComponent<GameplayManager>.Instance?.RingBoom?.ProbeBombContactsDuringDrag(rotationProbeColliders, this);
		}
	}

	private void EnsureGapGeometryFallback()
	{
		//IL_00d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e5: Unknown result type (might be due to invalid IL or missing references)
		if (gapGeometry.IsValid && gapGeometry.GapAngleDeg > 0.5f)
		{
			return;
		}
		RingSegmentController componentInChildren = ((Component)this).GetComponentInChildren<RingSegmentController>(true);
		if ((Object)(object)componentInChildren != (Object)null && componentInChildren.CenterRadius > 0.0001f)
		{
			float num = Mathf.Max(componentInChildren.BandWidth * 0.5f, 0.01f);
			SetRotationGeometry(componentInChildren.CenterRadius - num, componentInChildren.CenterRadius + num, componentInChildren.GapAngleDeg, componentInChildren.GapDirectionDeg);
		}
		else
		{
			if (TryInferRotationGeometryFromPolygon())
			{
				return;
			}
			Transform val = (((Object)(object)dragArea != (Object)null) ? dragArea : ((Component)this).transform.Find("DragArea"));
			if ((Object)(object)val != (Object)null)
			{
				CircleCollider2D component = ((Component)val).GetComponent<CircleCollider2D>();
				if ((Object)(object)component != (Object)null && component.radius > 0.0001f)
				{
					float num2 = component.radius * Mathf.Max(Mathf.Abs(val.lossyScale.x), Mathf.Abs(val.lossyScale.y));
					SetRotationGeometry(num2 * 0.7f, num2, 0f, 0f);
				}
			}
		}
	}

	private bool TryInferRotationGeometryFromPolygon()
	{
		//IL_00d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00da: Unknown result type (might be due to invalid IL or missing references)
		//IL_0100: Unknown result type (might be due to invalid IL or missing references)
		//IL_0107: Unknown result type (might be due to invalid IL or missing references)
		Collider2D obj = activeCollider;
		PolygonCollider2D val = (PolygonCollider2D)(object)((obj is PolygonCollider2D) ? obj : null);
		if ((Object)(object)val == (Object)null)
		{
			Collider2D[] componentsInChildren = ((Component)this).GetComponentsInChildren<Collider2D>(true);
			foreach (Collider2D val2 in componentsInChildren)
			{
				if (!((Object)(object)val2 == (Object)null) && ((Behaviour)val2).enabled && ((Component)val2).gameObject.layer != 6)
				{
					val = (PolygonCollider2D)(object)((val2 is PolygonCollider2D) ? val2 : null);
					if ((Object)(object)val != (Object)null)
					{
						break;
					}
				}
			}
		}
		if ((Object)(object)val == (Object)null || val.pathCount <= 0)
		{
			return false;
		}
		float num = float.MaxValue;
		float num2 = 0f;
		int num3 = 0;
		for (int j = 0; j < val.pathCount; j++)
		{
			num3 += val.GetPath(j).Length;
		}
		if (num3 < 3)
		{
			return false;
		}
		List<(float, float)> list = new List<(float, float)>(num3);
		for (int k = 0; k < val.pathCount; k++)
		{
			Vector2[] path = val.GetPath(k);
			for (int l = 0; l < path.Length; l++)
			{
				Vector2 val3 = path[l];
				float magnitude = ((Vector2)(ref val3)).magnitude;
				if (!(magnitude < 0.0001f))
				{
					if (magnitude < num)
					{
						num = magnitude;
					}
					if (magnitude > num2)
					{
						num2 = magnitude;
					}
					list.Add((Mathf.Atan2(val3.y, val3.x) * 57.29578f, magnitude));
				}
			}
		}
		if (list.Count < 3 || num2 <= num + 0.0001f)
		{
			return false;
		}
		list.Sort(((float angle, float radius) a, (float angle, float radius) b) => a.angle.CompareTo(b.angle));
		float num4 = 0f;
		float gapDirectionDeg = 0f;
		for (int num5 = 0; num5 < list.Count; num5++)
		{
			float item = list[num5].Item1;
			float item2 = list[(num5 + 1) % list.Count].Item1;
			float num6 = ((num5 + 1 < list.Count) ? (item2 - item) : (item2 + 360f - item));
			if (num6 > num4)
			{
				num4 = num6;
				gapDirectionDeg = Mathf.Repeat(item + num6 * 0.5f + 180f, 360f) - 180f;
			}
		}
		if (num4 <= 0.5f || num4 >= 359f)
		{
			return false;
		}
		SetRotationGeometry(num, num2, num4, gapDirectionDeg);
		if (gapGeometry.IsValid)
		{
			return gapGeometry.GapAngleDeg > 0.5f;
		}
		return false;
	}

	public void RegisterOnPointerDown(Action action)
	{
		onStartedRotate = (Action)Delegate.Combine(onStartedRotate, action);
	}

	public void RegisterOnPointerUp(Action action)
	{
		onFinishedRotate = (Action)Delegate.Combine(onFinishedRotate, action);
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
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		if (!IsSolved)
		{
			Solve(Vector2.op_Implicit(moveOutDirection), null, GameplaySolveReasonContext.Hammer());
		}
	}

	public Vector3 GetPosition()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		return ((Component)this).transform.position;
	}

	public float GetInitialAngleZ()
	{
		return initialLocalEulerZ;
	}

	internal void RecaptureInitialAngleZ()
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		initialLocalEulerZ = ((Component)this).transform.localEulerAngles.z;
	}

	public int GetReleaseScore()
	{
		return ThreadingScoreCalculator.GetReleaseScore(100, PartData);
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
			if ((Object)(object)dragArea != (Object)null)
			{
				((Component)dragArea).gameObject.SetActive(!isBlocked);
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

	protected override void Awake()
	{
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		base.Awake();
		WarmPartIdCache();
		if ((Object)(object)dragArea == (Object)null)
		{
			Transform val = ((Component)this).transform.Find("DragArea");
			if ((Object)(object)val != (Object)null)
			{
				dragArea = val;
			}
		}
		initialLocalEulerZ = ((Component)this).transform.localEulerAngles.z;
		ringRelatives = new List<RingRelative>(((Component)this).GetComponentsInChildren<RingRelative>());
		selfBombs = new List<BombRelative>(((Component)this).GetComponentsInChildren<BombRelative>(true));
		SetupLayerMaskHelpers();
		GetDragAreaCollider();
	}

	private void SetupLayerMaskHelpers()
	{
		if (layerAreaBlockedHelper != null)
		{
			layerAreaBlockedHelper.ApplyAllLayerMaskHelpersIn(((Component)this).transform);
			if (overlap2DHandler != null)
			{
				layerAreaBlockedHelper.AddLayerMaskHelper(overlap2DHandler);
			}
		}
	}

	public void AddRingRelative(RingRelative relative)
	{
		if (!((Object)(object)relative == (Object)null))
		{
			if (ringRelatives == null)
			{
				ringRelatives = new List<RingRelative>();
			}
			if (!ringRelatives.Contains(relative))
			{
				ringRelatives.Add(relative);
			}
		}
	}

	public void SetHardBlockColliders(Collider2D[] colliders)
	{
		if (colliders == null || colliders.Length == 0)
		{
			hardBlockColliders = null;
		}
		else
		{
			hardBlockColliders = colliders;
		}
	}

	public void SetOverlapCollidersEnabled(bool enabled)
	{
		if ((Object)(object)activeCollider != (Object)null)
		{
			((Behaviour)activeCollider).enabled = enabled;
		}
		if (hardBlockColliders == null)
		{
			return;
		}
		for (int i = 0; i < hardBlockColliders.Length; i++)
		{
			if ((Object)(object)hardBlockColliders[i] != (Object)null)
			{
				((Behaviour)hardBlockColliders[i]).enabled = enabled;
			}
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

	internal void ApplySavedRotation(float rotationZ)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		Vector3 localEulerAngles = ((Component)this).transform.localEulerAngles;
		localEulerAngles.z = rotationZ;
		((Component)this).transform.localEulerAngles = localEulerAngles;
		ApplyRotationState(rotationZ);
	}

	internal void OnSolveDragCleanup()
	{
		if (isDragging)
		{
			SetOperateHighlight(highlight: false);
			HandlePointerUp(default(GameplayPointerData));
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
		if (!IsSolved)
		{
			return base.IsCanDragging;
		}
		return false;
	}

	public bool IsPointerCaptured()
	{
		return isDragging;
	}

	protected bool IsLockedByRelatives()
	{
		return CheckLockedByRelatives();
	}

	private bool CheckLockedByRelatives()
	{
		if (ringRelatives == null || ringRelatives.Count == 0)
		{
			return false;
		}
		RingLockActiveService ringLockActiveService = SingletonComponent<GameplayManager>.Instance?.LockActive;
		if (ringLockActiveService == null)
		{
			foreach (RingRelative ringRelative in ringRelatives)
			{
				if ((Object)(object)ringRelative != (Object)null && ringRelative.IsBlockedByRing())
				{
					return true;
				}
			}
			return false;
		}
		if (ringLockActiveService.IsBlockedByFeatureActiveLock(this))
		{
			return true;
		}
		foreach (RingRelative ringRelative2 in ringRelatives)
		{
			if ((Object)(object)ringRelative2 == (Object)null || ringRelative2.IsSolved)
			{
				continue;
			}
			int overlapColliders = ringRelative2.GetOverlapColliders(overlapBuffer);
			for (int i = 0; i < overlapColliders; i++)
			{
				Collider2D val = overlapBuffer[i];
				if (!((Object)(object)val == (Object)null) && ringLockActiveService.IsLockedBy(this, val))
				{
					return true;
				}
			}
		}
		return false;
	}

	private bool IsLockedImmovable()
	{
		if (IsSolved)
		{
			return false;
		}
		if (LockCount > 0)
		{
			return true;
		}
		GameplayManager instance = SingletonComponent<GameplayManager>.Instance;
		if (instance != null && instance.PaletteLock?.IsKeyUnlockFlyPending(this) == true)
		{
			return true;
		}
		return CheckLockedByRelatives();
	}

	public void UpdateDragAreaLockZ()
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		if (!((Object)(object)dragArea == (Object)null))
		{
			Vector3 localPosition = dragArea.localPosition;
			float num = (IsLockedImmovable() ? 0.2f : 0.1f);
			if (!Mathf.Approximately(localPosition.z, num))
			{
				localPosition.z = num;
				dragArea.localPosition = localPosition;
			}
		}
	}

	public void OnPointerDown(GameplayPointerData data)
	{
		//IL_0160: Unknown result type (might be due to invalid IL or missing references)
		//IL_0165: Unknown result type (might be due to invalid IL or missing references)
		//IL_0187: Unknown result type (might be due to invalid IL or missing references)
		//IL_0153: Unknown result type (might be due to invalid IL or missing references)
		if (!base.IsCanDragging)
		{
			return;
		}
		VibrateUtility.Vibrate(1101);
		if (!isEnableRemoveMode)
		{
			GameplayManager instance = SingletonComponent<GameplayManager>.Instance;
			if (instance != null && instance.LockChain?.IsLockBlocked(this) == true)
			{
				SingletonComponent<GameplayManager>.Instance?.CompleteLegacyOperation(this, LegacyOperationSource.Blocked);
				SingletonComponent<GameplayManager>.Instance?.LockChain?.PlayChainShake(this);
				return;
			}
			GameplayManager instance2 = SingletonComponent<GameplayManager>.Instance;
			if (instance2 != null && instance2.PaletteLock?.IsKeyUnlockFlyPending(this) == true)
			{
				SingletonComponent<GameplayManager>.Instance?.CompleteLegacyOperation(this, LegacyOperationSource.Blocked);
			}
			else if (CheckLockedByRelatives())
			{
				SingletonComponent<GameplayManager>.Instance?.CompleteLegacyOperation(this, LegacyOperationSource.Blocked);
				if ((Object)(object)((Component)this).transform != (Object)null)
				{
					PlayLockedAnimation();
				}
			}
			else
			{
				onStartedRotate?.Invoke();
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
				Solve(Vector2.op_Implicit(moveOutDirection));
				onFinishedRemoveMode?.Invoke(this, data.WorldPosition);
				onFinishedRotate?.Invoke();
			}
		}
	}

	public void OnPointerDrag(GameplayPointerData data)
	{
		HandlePointerDrag(data);
	}

	public void OnPointerUp(GameplayPointerData data)
	{
		if (isDragging)
		{
			SetOperateHighlight(highlight: false);
		}
		EndedDragWithRotationBlock = isDragging && blockDirection != 0f;
		RollbackRotationAfterStop(blockDirection);
		HandlePointerUp(data);
		ClearRotationBlockCache();
		onFinishedRotate?.Invoke();
		EndedDragWithRotationBlock = false;
		SingletonComponent<GameplayManager>.Instance?.ObjectsSolveAndCheckVictory();
	}

	public void Solve(Vector3 moveOutDirection, BombRelative sourceBomb = null, GameplaySolveReasonContext releaseContext = default(GameplaySolveReasonContext))
	{
		SingletonComponent<GameplayManager>.Instance?.SolveService?.SolveObject(this, sourceBomb, releaseContext);
	}

	public void CollectAllEnabledColliderIds(HashSet<int> output)
	{
		if ((Object)(object)activeCollider != (Object)null && ((Behaviour)activeCollider).enabled)
		{
			output.Add(((Object)activeCollider).GetInstanceID());
		}
		Collider2D[] componentsInChildren = ((Component)this).GetComponentsInChildren<Collider2D>(true);
		foreach (Collider2D val in componentsInChildren)
		{
			if ((Object)(object)val != (Object)null && ((Behaviour)val).enabled && ((Component)val).gameObject.layer != 6)
			{
				output.Add(((Object)val).GetInstanceID());
			}
		}
	}

	private void SetOperateHighlight(bool highlight)
	{
		SingletonComponent<GameplayManager>.Instance?.OperateHighlightService?.TrySetHighlight(((Component)this).gameObject, highlight);
	}

	private void PlayLockedAnimation()
	{
		if (base.IsCanDragging)
		{
			base.IsCanDragging = false;
			SingletonComponent<GameplayManager>.Instance?.LockShakeService?.PlayLockShakeService(((Component)this).transform, ((Component)this).gameObject, delegate
			{
				base.IsCanDragging = true;
			});
		}
	}
}
