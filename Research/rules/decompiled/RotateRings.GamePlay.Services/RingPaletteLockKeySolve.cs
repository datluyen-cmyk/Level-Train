using System;
using System.Collections.Generic;
using RotateRings.Common;
using RotateRings.Services;
using UnityEngine;
using UnlockRing.Core.Authoring;
using UnlockRing.Core.Domain;

namespace RotateRings.GamePlay.Services;

internal sealed class RingPaletteLockKeySolve
{
	private readonly RingPaletteLockService owner;

	private readonly HashSet<RingObject> pendingPresentationKeyRings = new HashSet<RingObject>();

	public RingPaletteLockKeySolve(RingPaletteLockService owner)
	{
		this.owner = owner ?? throw new ArgumentNullException("owner");
	}

	public void ClearLevel()
	{
		pendingPresentationKeyRings.Clear();
	}

	internal void OnRingSolved(RingObject ring)
	{
		if ((Object)(object)ring == (Object)null)
		{
			return;
		}
		if (ring.PartData.IsPaletteLockKey)
		{
			HandleKeyRingImmediateDataLayer(ring);
			if (owner.FlyGate.IsPending(ring))
			{
				pendingPresentationKeyRings.Add(ring);
			}
			else
			{
				PlayKeyRingPresentation(ring);
			}
		}
		else
		{
			TryDismissKeyUiWhenAllLocksSolved(ring);
		}
	}

	internal void OnDeferredKeyRingFlyUnlocked(RingObject lockedRing)
	{
		if ((Object)(object)lockedRing != (Object)null && pendingPresentationKeyRings.Remove(lockedRing))
		{
			PlayKeyRingPresentation(lockedRing);
		}
	}

	internal void TryDismissKeyUiWhenAllLocksSolved(RingObject triggerRing)
	{
		if ((Object)(object)triggerRing == (Object)null)
		{
			return;
		}
		IReadOnlyList<PaletteLockKeyBindingEntry> readOnlyList = owner.HostInstance.CurrentLevelInfo?.PaletteLockKeyBindings;
		if (readOnlyList == null || readOnlyList.Count == 0)
		{
			return;
		}
		IReadOnlyList<RingObject> readOnlyList2 = owner.HostInstance.ObjectsService?.Rings;
		if (readOnlyList2 == null)
		{
			return;
		}
		int id = triggerRing.PartData.Id;
		if (id <= 0)
		{
			return;
		}
		for (int i = 0; i < readOnlyList.Count; i++)
		{
			PaletteLockKeyBindingEntry entry = readOnlyList[i];
			if (RingPaletteLockBindingUtil.IsAssemblyInBindingLockedList(id, entry) && RingPaletteLockBindingUtil.TryFindRingByAssemblyId(readOnlyList2, EditablePartDraft.ParseAssemblyId(entry.keyAssemblyName), out var ring) && !((Object)(object)ring == (Object)null) && !ring.IsSolved && ring.IsCurrentPaletteLockKey && owner.KeyUi.HasKeyUi(ring) && AreAllLockedRingsSolvedForEntry(entry, readOnlyList2) && !AreAnyLockedRingsFlyPendingForEntry(entry, readOnlyList2))
			{
				ClearKeyRingRuntimeState(ring);
				owner.KeyUi.PlayDisappearAndDetach(ring);
			}
		}
	}

	private void HandleKeyRingImmediateDataLayer(RingObject keyRing)
	{
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		if (!((Object)(object)keyRing == (Object)null) && TryResolveKeyRingLockedRings(keyRing, out var lockedRings))
		{
			owner.GameplayStep.BeginBatch(keyRing, lockedRings);
			owner.KeyUnlockFly.PrepareKeyUnlockFly(keyRing, lockedRings);
			TryGetKeyRingBatchAnchorPartId(keyRing, out var stepBatchPartId);
			IReadOnlyList<RingObject> colorLockReleasedTargets = SolveLockedChainImmediately(lockedRings, stepBatchPartId);
			owner.GameplayStep.SetBatchGateTargets(keyRing, colorLockReleasedTargets);
			owner.GameplayStep.MarkDataLayerReady(keyRing);
		}
	}

	private void PlayKeyRingPresentation(RingObject keyRing)
	{
		if (!((Object)(object)keyRing == (Object)null))
		{
			AudioService.PlayUiSound(AudioKeys.ColorLockKey.Value);
			owner.KeyUnlockFly.PlayPreparedKeyUnlockFly(keyRing);
			owner.KeyUi.OnKeyRingSolved(keyRing);
		}
	}

	private IReadOnlyList<RingObject> SolveLockedChainImmediately(IReadOnlyList<RingObject> lockedRings, PartId stepBatchPartId)
	{
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
		if (lockedRings == null)
		{
			return Array.Empty<RingObject>();
		}
		List<RingObject> list = new List<RingObject>(lockedRings.Count);
		for (int i = 0; i < lockedRings.Count; i++)
		{
			RingObject ringObject = lockedRings[i];
			if (!((Object)(object)ringObject == (Object)null) && !ringObject.IsSolved && owner.RemainingLocks.Decrement(ringObject) <= 0)
			{
				list.Add(ringObject);
			}
		}
		for (int j = 0; j < list.Count; j++)
		{
			RingObject ringObject2 = list[j];
			owner.FlyGate.RegisterBreakGateForBoundRings(ringObject2);
			owner.ColorRelease.ReleaseAndGap(ringObject2, stepBatchPartId);
		}
		for (int k = 0; k < list.Count; k++)
		{
			owner.ColorRelease.TrySolveCascade(list[k], stepBatchPartId);
		}
		return list;
	}

	private bool TryResolveKeyRingLockedRings(RingObject keyRing, out List<RingObject> lockedRings)
	{
		lockedRings = null;
		if ((Object)(object)keyRing == (Object)null || !keyRing.PartData.IsPaletteLockKey)
		{
			return false;
		}
		IReadOnlyList<PaletteLockKeyBindingEntry> readOnlyList = owner.HostInstance.CurrentLevelInfo?.PaletteLockKeyBindings;
		if (readOnlyList == null || readOnlyList.Count == 0)
		{
			return false;
		}
		int id = keyRing.PartData.Id;
		if (id <= 0)
		{
			return false;
		}
		IReadOnlyList<RingObject> readOnlyList2 = owner.HostInstance.ObjectsService?.Rings;
		if (readOnlyList2 == null)
		{
			return false;
		}
		for (int i = 0; i < readOnlyList.Count; i++)
		{
			PaletteLockKeyBindingEntry paletteLockKeyBindingEntry = readOnlyList[i];
			if (EditablePartDraft.ParseAssemblyId(paletteLockKeyBindingEntry.keyAssemblyName) != id)
			{
				continue;
			}
			string[] lockedAssemblyNames = paletteLockKeyBindingEntry.lockedAssemblyNames;
			if (lockedAssemblyNames == null)
			{
				continue;
			}
			List<RingObject> list = new List<RingObject>(lockedAssemblyNames.Length);
			for (int j = 0; j < lockedAssemblyNames.Length; j++)
			{
				if (RingPaletteLockBindingUtil.TryFindRingByAssemblyId(readOnlyList2, EditablePartDraft.ParseAssemblyId(lockedAssemblyNames[j]), out var ring))
				{
					list.Add(ring);
				}
			}
			if (list.Count > 0)
			{
				lockedRings = list;
				return true;
			}
			return false;
		}
		return false;
	}

	private bool TryGetKeyRingBatchAnchorPartId(RingObject keyRing, out PartId stepBatchPartId)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		stepBatchPartId = default(PartId);
		return owner.HostInstance.ObjectsService?.TryGetPartId(keyRing, out stepBatchPartId) ?? false;
	}

	private static bool AreAllLockedRingsSolvedForEntry(PaletteLockKeyBindingEntry entry, IReadOnlyList<RingObject> rings)
	{
		string[] lockedAssemblyNames = entry.lockedAssemblyNames;
		if (lockedAssemblyNames == null || lockedAssemblyNames.Length == 0)
		{
			return false;
		}
		for (int i = 0; i < lockedAssemblyNames.Length; i++)
		{
			int num = EditablePartDraft.ParseAssemblyId(lockedAssemblyNames[i]);
			if (num > 0 && RingPaletteLockBindingUtil.TryFindRingByAssemblyId(rings, num, out var ring) && !ring.IsSolved)
			{
				return false;
			}
		}
		return true;
	}

	private bool AreAnyLockedRingsFlyPendingForEntry(PaletteLockKeyBindingEntry entry, IReadOnlyList<RingObject> rings)
	{
		string[] lockedAssemblyNames = entry.lockedAssemblyNames;
		if (lockedAssemblyNames == null || lockedAssemblyNames.Length == 0)
		{
			return false;
		}
		for (int i = 0; i < lockedAssemblyNames.Length; i++)
		{
			int num = EditablePartDraft.ParseAssemblyId(lockedAssemblyNames[i]);
			if (num > 0 && RingPaletteLockBindingUtil.TryFindRingByAssemblyId(rings, num, out var ring) && owner.FlyGate.IsPending(ring))
			{
				return true;
			}
		}
		return false;
	}

	private static void ClearKeyRingRuntimeState(RingObject ring)
	{
		if (!((Object)(object)ring == (Object)null))
		{
			GameplayPartData partData = ring.PartData;
			partData.IsPaletteLockKey = false;
			ring.PartData = partData;
			ring.IsCurrentPaletteLockKey = false;
		}
	}
}
