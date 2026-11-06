using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using RotateRings.GamePlay.Events;
using RotateRings.GamePlay.Objects;
using RotateRings.Services;
using UnityEngine;
using UnlockRing.Core.Domain;
using XingYun.Core.Singletons;
using XingYun.Event;

namespace RotateRings.GamePlay.Services;

public sealed class RingPaletteLockService : GameplayServiceBase<GameplayManager>, IPartLockFeature
{
	private const string ColorLockAudioVariantId = "variant_79d612c5";

	private readonly RingPaletteLockKeyColors keyColors = new RingPaletteLockKeyColors();

	private readonly Collider2D[] gapOverlapBuffer = (Collider2D[])(object)new Collider2D[16];

	private ContactFilter2D gapOverlapFilter;

	public const PaletteLockFlags LockedRingEnterFlags = PaletteLockFlags.ColorLock;

	internal GameplayManager HostInstance => base.Host;

	internal RingPaletteLockGaps Gaps { get; }

	internal RingPaletteLockKeyUi KeyUi { get; }

	internal RingPaletteLockKeyUnlockFly KeyUnlockFly { get; }

	internal RingPaletteLockGameplayStep GameplayStep { get; }

	internal RingPaletteLockRemainingLocks RemainingLocks { get; }

	internal RingPaletteLockColorLockRelease ColorRelease { get; }

	internal RingPaletteLockFlyGate FlyGate { get; }

	internal RingPaletteLockKeySolve KeySolve { get; }

	public bool HasActiveKeyUnlockPresentation => FlyGate.HasActivePresentation;

	public RingPaletteLockService(GameplayManager host)
		: base(host)
	{
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		Gaps = new RingPaletteLockGaps(this);
		KeyUi = new RingPaletteLockKeyUi(this);
		KeyUnlockFly = new RingPaletteLockKeyUnlockFly(this);
		GameplayStep = new RingPaletteLockGameplayStep(this);
		RemainingLocks = new RingPaletteLockRemainingLocks();
		ColorRelease = new RingPaletteLockColorLockRelease(this);
		FlyGate = new RingPaletteLockFlyGate(this);
		KeySolve = new RingPaletteLockKeySolve(this);
		gapOverlapFilter = default(ContactFilter2D);
		((ContactFilter2D)(ref gapOverlapFilter)).NoFilter();
		gapOverlapFilter.useTriggers = true;
		GameApp.Event.Listen<RingPreSolveEvent>((Action<RingPreSolveEvent>)OnRingPreSolve);
		Singleton<EventManager>.Instance.Listen<RingSolvedEvent>((Action<RingSolvedEvent>)OnRingSolved);
		AudioService.ActivateExperiment("variant_79d612c5");
	}

	public override void Dispose()
	{
		if (!base.IsDisposed)
		{
			GameApp.Event.Unlisten<RingPreSolveEvent>((Action<RingPreSolveEvent>)OnRingPreSolve);
			Singleton<EventManager>.Instance.Unlisten<RingSolvedEvent>((Action<RingSolvedEvent>)OnRingSolved);
			KeyUnlockFly.Dispose();
			KeyUi.Dispose();
			base.Dispose();
		}
	}

	public override void ClearLevel()
	{
		KeyUnlockFly.ClearLevel();
		Gaps.Clear();
		keyColors.Clear();
		KeyUi.ClearKeyUi();
		FlyGate.ClearLevel();
		RemainingLocks.Clear();
		KeySolve.ClearLevel();
		GameplayStep.ClearLevel();
		base.ClearLevel();
	}

	public void BuildKeyColorAssignment(GameLevelInfo levelInfo, IReadOnlyDictionary<int, GameplayPartData> partByAssemblyId)
	{
		if ((Object)(object)levelInfo == (Object)null)
		{
			keyColors.Clear();
		}
		else
		{
			keyColors.Rebuild(levelInfo.PaletteLockKeyBindings, partByAssemblyId);
		}
	}

	public bool TryGetKeyColor(RingObject keyRing, out PaletteLockKeyColor color)
	{
		return keyColors.TryGetKeyColor(keyRing, out color);
	}

	public bool TryGetLockHeartColor(RingObject lockedRing, out PaletteLockKeyColor color)
	{
		return keyColors.TryGetLockHeartColor(lockedRing, out color);
	}

	public void SetupKeyUiOnLevelLoaded()
	{
		KeyUi.SetupKeyUiOnLevelLoaded();
	}

	public void NotifyRingKeyScaleFactor(RingObject ring, float factor)
	{
		KeyUi.OnRingScaleFactor(ring, factor);
	}

	public UniTask SpawnForNewGameAsync(IReadOnlyList<RingObject> rings)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		return Gaps.SpawnForNewGameAsync(rings);
	}

	internal void OnGapSolved(RingPaletteLockGapObject gap)
	{
		Gaps.OnGapSolved(gap);
	}

	public bool ColorGapOverlapsPart(RingObject colorLockedRing, PartId otherPartId)
	{
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)colorLockedRing == (Object)null || colorLockedRing.IsSolved || !HasColorLock(colorLockedRing))
		{
			return false;
		}
		if (!Gaps.TryGetGapBodyCollider(colorLockedRing, out var gapBody))
		{
			return false;
		}
		int num = gapBody.Overlap(gapOverlapFilter, gapOverlapBuffer);
		for (int i = 0; i < num; i++)
		{
			Collider2D val = gapOverlapBuffer[i];
			if (!((Object)(object)val == (Object)null) && ((Component)val).gameObject.layer != 6 && RingColliderQuery.TryGetRingObject(val, out var ring) && (Object)(object)ring != (Object)null && ring.TryGetPartId(out var partId) && ((PartId)(ref partId)).Equals(otherPartId))
			{
				return true;
			}
		}
		return false;
	}

	private void OnRingSolved(RingSolvedEvent evt)
	{
		if (!base.IsDisposed && evt?.GameplayObject is RingObject ring)
		{
			KeySolve.OnRingSolved(ring);
		}
	}

	private void OnRingPreSolve(RingPreSolveEvent evt)
	{
		if (evt?.GameplayObject is RingObject ring)
		{
			ColorRelease.HandleRingPreSolve(ring, FlyGate.IsPending(ring));
		}
	}

	public bool IsKeyUnlockFlyPending(RingObject ring)
	{
		return FlyGate.IsPending(ring);
	}

	public bool TryGetKeyUnlockFlyBreakGate(RingObject dependentRing, out RingObject gateRing)
	{
		return FlyGate.TryGetBreakGate(dependentRing, out gateRing);
	}

	internal void MarkKeyUnlockFlyPending(RingObject ring)
	{
		FlyGate.MarkPending(ring);
	}

	internal void ClearKeyUnlockFlyPending()
	{
		FlyGate.ClearAllPending();
	}

	public void ResetRemainingLockCounts()
	{
		RemainingLocks.Clear();
	}

	public void AddRemainingLock(RingObject ring)
	{
		RemainingLocks.Add(ring);
	}

	public void SetRemainingLockCount(RingObject ring, int count)
	{
		RemainingLocks.Set(ring, count);
	}

	public int GetRemainingLockCount(RingObject ring)
	{
		return RemainingLocks.Get(ring);
	}

	internal void CompleteUnlockForRing(RingObject lockedRing)
	{
		FlyGate.CompleteUnlockForRing(lockedRing);
	}

	internal bool TryGetLockHeartWorldPosition(RingObject host, out Vector3 worldPosition)
	{
		return FlyGate.TryGetLockHeartWorldPosition(host, out worldPosition);
	}

	public bool HasColorLock(IGameplayObject obj)
	{
		if (obj is RingObject ringObject)
		{
			return (ringObject.PaletteLockFlags & PaletteLockFlags.ColorLock) != 0;
		}
		return false;
	}

	public void ClearColorLock(RingObject ring)
	{
		if (!((Object)(object)ring == (Object)null))
		{
			ring.PaletteLockFlags &= ~PaletteLockFlags.ColorLock;
		}
	}

	public bool ShouldKeepLocksOnExit(PartId partId)
	{
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		if (string.IsNullOrEmpty(((PartId)(ref partId)).Value))
		{
			return false;
		}
		RingObjectsService objectsService = base.Host.ObjectsService;
		if (objectsService == null || !objectsService.TryGetPartObject(partId, out var gameplayObject))
		{
			return false;
		}
		return HasColorLock(gameplayObject);
	}

	public bool ShouldPreserveBindingOnDragExit(PartId partId)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		return ShouldKeepLocksOnExit(partId);
	}

	public bool ArePartsConnected(PartId lockedPartId, PartId lockerPartId)
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		RingObjectsService objectsService = base.Host.ObjectsService;
		if (objectsService == null)
		{
			return false;
		}
		if (objectsService.TryGetRing(lockedPartId, out var ring) && ColorGapOverlapsPart(ring, lockerPartId))
		{
			return true;
		}
		if (objectsService.TryGetRing(lockerPartId, out var ring2) && ColorGapOverlapsPart(ring2, lockedPartId))
		{
			return true;
		}
		return false;
	}

	public void OnObjectSolved(RingObject solvedRing)
	{
		ClearColorLock(solvedRing);
		RemainingLocks.ClearForRing(solvedRing);
	}
}
