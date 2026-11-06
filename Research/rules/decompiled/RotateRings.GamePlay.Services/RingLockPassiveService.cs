using System;
using System.Collections.Generic;
using RotateRings.GamePlay.Events;
using RotateRings.GamePlay.Objects;
using UnityEngine;
using UnlockRing.Core.Domain;

namespace RotateRings.GamePlay.Services;

public class RingLockPassiveService : GameplayServiceBase<GameplayManager>
{
	private const string TAG = "[GamePlay][RingLockPassive] ";

	private readonly CatchLockRegistry catchLockRegistry = new CatchLockRegistry();

	private readonly RingPassiveLockDomain ringDomain;

	private readonly PullPassiveLockDomain pullDomain;

	private readonly CatchGuideSolver catchGuideSolver;

	private readonly CatchButterflySolver catchButterflySolver;

	private readonly List<IPassiveLockRelationRegistrar> registrars;

	private readonly HashSet<int> ringSolveStripBuffer = new HashSet<int>();

	private readonly List<PartId> pullSolveLinkedGuideBuffer = new List<PartId>(4);

	private bool eventsRegistered;

	public RingLockPassiveService(GameplayManager host)
		: base(host)
	{
		ringDomain = new RingPassiveLockDomain(catchLockRegistry);
		pullDomain = new PullPassiveLockDomain(catchLockRegistry);
		catchGuideSolver = new CatchGuideSolver(catchLockRegistry, ringDomain, pullDomain);
		catchButterflySolver = new CatchButterflySolver(catchLockRegistry, ringDomain, pullDomain);
		registrars = new List<IPassiveLockRelationRegistrar>
		{
			new RingToRingLinkRegistrar(),
			new PullToRingLinkRegistrar(),
			new PullCatchGuideRegistrar(),
			new RingCatchGuideRegistrar()
		};
		RegisterEvents();
	}

	public void InitPassiveLockBindings(IReadOnlyList<RingObject> rings, IReadOnlyList<PullObject> pullObjects, IReadOnlyList<FixedObject> fixedObjects = null, IReadOnlyList<RingLinkEntry> ringLinks = null, IReadOnlyList<GuideLinkEntry> guideLinks = null)
	{
		ClearLevel();
		if (rings != null || pullObjects != null || fixedObjects != null)
		{
			catchLockRegistry.Build(guideLinks);
			PassiveLockRegistrationContext ctx = new PassiveLockRegistrationContext(ringLinks, guideLinks, ringDomain, pullDomain, catchLockRegistry);
			for (int i = 0; i < registrars.Count; i++)
			{
				registrars[i].Register(ctx);
			}
			ringDomain.CloneToActive();
			pullDomain.CloneToActive();
			catchLockRegistry.SeedSolvedFromGameplay();
		}
	}

	public override void ClearLevel()
	{
		_ = ringDomain.StaticOwnerCount;
		_ = pullDomain.StaticOwnerCount;
		catchLockRegistry.Clear();
		ringDomain.Clear();
		pullDomain.Clear();
		ringSolveStripBuffer.Clear();
		base.ClearLevel();
	}

	public override void Dispose()
	{
		if (!base.IsDisposed)
		{
			base.Dispose();
			UnRegisterEvents();
			ClearLevel();
		}
	}

	private void RegisterEvents()
	{
		if (!eventsRegistered)
		{
			GameApp.Event.Listen<GameplayPointerDownEvent>((Action<GameplayPointerDownEvent>)OnGameplayPointerDown);
			GameApp.Event.Listen<RingDragCollisionEnterEvent>((Action<RingDragCollisionEnterEvent>)OnDragCollisionEnter);
			GameApp.Event.Listen<RingDragCollisionExitEvent>((Action<RingDragCollisionExitEvent>)OnDragCollisionExit);
			GameApp.Event.Listen<RingDragCollisionResetEvent>((Action<RingDragCollisionResetEvent>)OnDragCollisionReset);
			GameApp.Event.Listen<RingAfterSolveEvent>((Action<RingAfterSolveEvent>)OnGameplayObjectAfterSolve);
			eventsRegistered = true;
		}
	}

	private void UnRegisterEvents()
	{
		if (eventsRegistered)
		{
			GameApp.Event.Unlisten<GameplayPointerDownEvent>((Action<GameplayPointerDownEvent>)OnGameplayPointerDown);
			GameApp.Event.Unlisten<RingDragCollisionEnterEvent>((Action<RingDragCollisionEnterEvent>)OnDragCollisionEnter);
			GameApp.Event.Unlisten<RingDragCollisionExitEvent>((Action<RingDragCollisionExitEvent>)OnDragCollisionExit);
			GameApp.Event.Unlisten<RingDragCollisionResetEvent>((Action<RingDragCollisionResetEvent>)OnDragCollisionReset);
			GameApp.Event.Unlisten<RingAfterSolveEvent>((Action<RingAfterSolveEvent>)OnGameplayObjectAfterSolve);
			eventsRegistered = false;
		}
	}

	private void OnGameplayPointerDown(GameplayPointerDownEvent evt)
	{
		if (evt?.PointerHandler is RingObject ring)
		{
			ringDomain.OnRingPointerDown(ring);
		}
	}

	private void OnDragCollisionEnter(RingDragCollisionEnterEvent evt)
	{
		if (evt != null && evt.IsNewEnter)
		{
			ringDomain.TryActivateBinding(evt.MovingRoot, evt.OtherCollider);
			pullDomain.TryActivateBinding(evt.MovingRoot, evt.OtherCollider);
		}
	}

	private void OnDragCollisionExit(RingDragCollisionExitEvent evt)
	{
		if (evt != null)
		{
			ringDomain.TryDeactivateBinding(evt.MovingRoot, evt.OtherCollider);
			pullDomain.TryDeactivateBinding(evt.MovingRoot, evt.OtherCollider);
		}
	}

	private void OnDragCollisionReset(RingDragCollisionResetEvent evt)
	{
		if (!((Object)(object)evt?.MovingRoot == (Object)null))
		{
			ringDomain.CleanSeparatedActiveBindings(evt.MovingRoot);
			pullDomain.CleanSeparatedActiveBindings(evt.MovingRoot);
		}
	}

	private void OnGameplayObjectAfterSolve(RingAfterSolveEvent evt)
	{
		if (evt?.GameplayObject != null)
		{
			if (evt.GameplayObject is RingObject solvedRing)
			{
				OnRingSolved(solvedRing);
			}
			else if (evt.GameplayObject is PullObject solvedPull)
			{
				OnPullObjectSolved(solvedPull);
			}
			else if (evt.GameplayObject is FixedButterflyObject solvedButterfly)
			{
				OnButterflyBarSolved(solvedButterfly);
			}
			else if (evt.GameplayObject is FixedObject solvedFixed)
			{
				pullDomain.OnFixedObjectSolved(solvedFixed);
			}
		}
	}

	private void OnButterflyBarSolved(FixedButterflyObject solvedButterfly)
	{
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		ringDomain.OnButterflyBarSolved(solvedButterfly);
		pullDomain.OnFixedObjectSolved(solvedButterfly);
		if (solvedButterfly.TryGetPartId(out var partId))
		{
			catchLockRegistry.MarkLockedPartSolvedIfRegistered(partId);
		}
	}

	private void OnRingSolved(RingObject solvedRing)
	{
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)solvedRing != (Object)null && solvedRing.TryGetPartId(out var partId))
		{
			catchLockRegistry.MarkLockedPartSolvedIfRegistered(partId);
		}
		ringDomain.OnRingSolved(solvedRing, ringSolveStripBuffer, out var solvedPartId, out var hasSolvedPartId);
		pullDomain.RemoveStrippedBindingsForSolvedRing(ringSolveStripBuffer, solvedPartId, hasSolvedPartId);
		if ((Object)(object)solvedRing != (Object)null && solvedRing.TryGetPartId(out var partId2))
		{
			catchButterflySolver.TryButterflyForLockedPartId(partId2);
		}
	}

	private void OnPullObjectSolved(PullObject solvedPull)
	{
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)solvedPull != (Object)null && solvedPull.TryGetPartId(out var partId))
		{
			catchLockRegistry.MarkLockedPartSolvedIfRegistered(partId);
		}
		pullDomain.OnPullObjectSolved(solvedPull, pullSolveLinkedGuideBuffer);
		if ((Object)(object)solvedPull != (Object)null && solvedPull.TryGetPartId(out var partId2))
		{
			catchGuideSolver.TryPlainCatchGuides(pullSolveLinkedGuideBuffer, partId2);
			catchButterflySolver.TryButterflyForLockedPartId(partId2);
		}
	}

	public void ReconcilePassiveLocksAfterFeatureReleased(RingObject unlockedRing)
	{
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)unlockedRing == (Object)null || unlockedRing.IsSolved)
		{
			return;
		}
		ringDomain.CleanSeparatedActiveBindings((Component)(object)unlockedRing);
		List<PartLinkEntry> lockedBy = unlockedRing.PartData.LockedBy;
		if (lockedBy == null || lockedBy.Count == 0)
		{
			return;
		}
		RingObjectsService objectsService = base.Host.ObjectsService;
		if (objectsService == null)
		{
			return;
		}
		for (int i = 0; i < lockedBy.Count; i++)
		{
			string physicPartId = lockedBy[i].PhysicPartId;
			if (!string.IsNullOrEmpty(physicPartId) && objectsService.TryGetPartObject(new PartId(physicPartId), out var gameplayObject))
			{
				if (gameplayObject is RingObject movingRoot)
				{
					ringDomain.CleanSeparatedActiveBindings((Component)(object)movingRoot);
				}
				else if (gameplayObject is PullObject movingRoot2)
				{
					pullDomain.CleanSeparatedActiveBindings((Component)(object)movingRoot2);
				}
			}
		}
	}

	public bool TryGetCatchGuideLockedPartId(FixedObject guide, out PartId lockedPartId)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		lockedPartId = default(PartId);
		if ((Object)(object)guide == (Object)null || string.IsNullOrEmpty(guide.PartData.PhysicPartId))
		{
			return false;
		}
		PartId guidePartId = default(PartId);
		((PartId)(ref guidePartId))._002Ector(guide.PartData.PhysicPartId);
		if (catchLockRegistry.IsButterflySlotGuide(guidePartId))
		{
			return false;
		}
		return catchLockRegistry.TryGetCatchLockedPartId(guidePartId, out lockedPartId);
	}

	public bool TryGetLinkedPullObject(FixedObject guide, out PullObject pullObject)
	{
		return pullDomain.TryGetLinkedPullObject(guide, out pullObject);
	}

	public bool IsPullLocklistCollider(PullObject pull, Collider2D col)
	{
		return pullDomain.IsPullLocklistCollider(pull, col);
	}

	public bool HasActivePullBindings(PullObject pull)
	{
		return pullDomain.HasActivePullBindings(pull);
	}

	public bool CanCatchGuideSolve(FixedObject guide)
	{
		return catchGuideSolver.CanCatchGuideSolve(guide);
	}

	public bool CanCatchButterflyBarSolve(FixedButterflyObject butterflyBar)
	{
		return catchButterflySolver.CanCatchButterflySolve(butterflyBar);
	}

	public bool HasActiveRingBindings(RingObject ring)
	{
		return ringDomain.HasActiveRingBindings(ring);
	}

	public bool TryGetActiveHeldLockers(RingObject ring, out IReadOnlyCollection<PartId> lockers)
	{
		return ringDomain.TryGetActiveLockers(ring, out lockers);
	}

	public bool TryGetActiveHeldLockers(PullObject pull, out IReadOnlyCollection<PartId> lockers)
	{
		return pullDomain.TryGetActiveHeldLockers(pull, out lockers);
	}

	public bool ShouldIgnoreBlock(RingObject ring, Collider2D col, RingRelative ownerSource = null)
	{
		return ringDomain.ShouldIgnoreBlock(ring, col, ownerSource);
	}

	public bool HasNewCollider(RingObject ring, RingRelative ownerSource, Collider2D[] colliders, int count)
	{
		return ringDomain.HasNewCollider(ring, ownerSource, colliders, count);
	}

	public bool TryResolveBlocker(RingObject ring, RingRelative ownerSource, out Collider2D blocker)
	{
		return ringDomain.TryResolveBlocker(ring, ownerSource, out blocker);
	}
}
