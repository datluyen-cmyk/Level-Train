using System.Collections.Generic;
using RotateRings.GamePlay.Objects;
using UnityEngine;
using UnlockRing.Core.Domain;

namespace RotateRings.GamePlay.Services;

internal sealed class RingPassiveLockDomain
{
	private const string TAG = "[GamePlay][RingLockPassive] ";

	private readonly PassiveLockBinding passiveBindings = new PassiveLockBinding();

	private readonly CatchLockRegistry catchLockRegistry;

	private readonly Dictionary<PartId, RingObject> ringObjectByGuidePartId = new Dictionary<PartId, RingObject>();

	private readonly HashSet<RingObject> interlockedPartners = new HashSet<RingObject>();

	private readonly HashSet<int> catchGuideOverlapInstanceIds = new HashSet<int>();

	private readonly HashSet<PartId> activeLockersBuffer = new HashSet<PartId>();

	private readonly RingPassiveBlockCheck blockCheck;

	internal int StaticOwnerCount => passiveBindings.StaticOwnerCount;

	internal RingPassiveLockDomain(CatchLockRegistry catchLockRegistry)
	{
		this.catchLockRegistry = catchLockRegistry;
		blockCheck = new RingPassiveBlockCheck(this);
	}

	internal int RegisterPartnerColliders(PartId ownerPartId, PartId partnerPartId, HashSet<int> colliderIds, float baseline = 0f)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		Dictionary<int, LockPassiveColliderBinding> orCreateStatic = passiveBindings.GetOrCreateStatic(ownerPartId);
		return passiveBindings.RegisterMainBody(orCreateStatic, partnerPartId, colliderIds, baseline);
	}

	internal void MapCatchGuide(PartId guidePartId, RingObject ownerRing)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		ringObjectByGuidePartId[guidePartId] = ownerRing;
	}

	internal void CloneToActive()
	{
		passiveBindings.CloneToActive(BuildActiveForOwner);
	}

	private Dictionary<int, LockPassiveColliderBinding> BuildActiveForOwner(PartId ownerPartId, Dictionary<int, LockPassiveColliderBinding> staticBindings)
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		_ = staticBindings.Count;
		Dictionary<int, LockPassiveColliderBinding> dictionary = new Dictionary<int, LockPassiveColliderBinding>(staticBindings);
		RemoveBindingsFromActive(ownerPartId, dictionary);
		if (TryGetRing(ownerPartId, out var ring))
		{
			_ = ((Object)ring).name;
		}
		return dictionary;
	}

	private void RemoveBindingsFromActive(PartId ownerPartId, Dictionary<int, LockPassiveColliderBinding> activeBindings)
	{
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		//IL_0115: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c4: Unknown result type (might be due to invalid IL or missing references)
		if (activeBindings.Count == 0 || !TryGetRing(ownerPartId, out var ring))
		{
			return;
		}
		HashSet<PartId> hashSet = new HashSet<PartId>();
		bool flag = ring.TryCollectInterlockedPartners(interlockedPartners);
		foreach (LockPassiveColliderBinding value in activeBindings.Values)
		{
			if (!hashSet.Contains(value.PartId) && TryGetRing(value.PartId, out var ring2) && (!flag || !interlockedPartners.Contains(ring2)))
			{
				GameplayManager instance = SingletonComponent<GameplayManager>.Instance;
				if (instance == null || instance.LockFeature?.ArePartsConnected(ownerPartId, value.PartId) != true)
				{
					hashSet.Add(value.PartId);
				}
			}
		}
		if (hashSet.Count == 0)
		{
			return;
		}
		List<int> list = new List<int>();
		foreach (KeyValuePair<int, LockPassiveColliderBinding> activeBinding in activeBindings)
		{
			if (hashSet.Contains(activeBinding.Value.PartId))
			{
				list.Add(activeBinding.Key);
			}
		}
		for (int i = 0; i < list.Count; i++)
		{
			activeBindings.Remove(list[i]);
		}
	}

	internal void Clear()
	{
		passiveBindings.Clear();
		ringObjectByGuidePartId.Clear();
	}

	internal void OnRingSolved(RingObject solvedRing, HashSet<int> stripBuffer, out PartId solvedPartId, out bool hasSolvedPartId)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		solvedPartId = default(PartId);
		hasSolvedPartId = false;
		if (!((Object)(object)solvedRing == (Object)null))
		{
			stripBuffer.Clear();
			solvedRing.CollectAllEnabledColliderIds(stripBuffer);
			_ = stripBuffer.Count;
			hasSolvedPartId = TryGetPartId(solvedRing, out solvedPartId);
			if (hasSolvedPartId)
			{
				passiveBindings.RemoveOwner(solvedPartId);
			}
			passiveBindings.RemoveSolvedPartnerEverywhere(stripBuffer, solvedPartId, hasSolvedPartId);
			passiveBindings.RebuildLockedBy();
			RemoveGuideLinksForRingObject(solvedRing);
		}
	}

	internal void OnButterflyBarSolved(FixedButterflyObject butterflyBar)
	{
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)butterflyBar == (Object)null)
		{
			return;
		}
		int num = 0;
		PartId val = default(PartId);
		for (int i = 0; i <= 1; i++)
		{
			if (butterflyBar.TryGetSlotGuidePartId(i, out var guidePartId) && !string.IsNullOrEmpty(guidePartId))
			{
				((PartId)(ref val))._002Ector(guidePartId);
				passiveBindings.RemovePartnerEverywhere(val);
				if (ringObjectByGuidePartId.Remove(val))
				{
					num++;
				}
			}
		}
		passiveBindings.RebuildLockedBy();
	}

	internal void TryActivateBinding(Component moving, Collider2D other)
	{
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		if (!((Object)(object)moving == (Object)null) && !((Object)(object)other == (Object)null) && TryResolveConfigured(moving, other, out var ownerPartId, out var colliderId, out var binding))
		{
			passiveBindings.TryAddActiveBinding(ownerPartId, colliderId, binding);
		}
	}

	internal void TryDeactivateBinding(Component moving, Collider2D other)
	{
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)moving == (Object)null || (Object)(object)other == (Object)null || !TryResolveOwnerPartId(moving, out var partId))
		{
			return;
		}
		int instanceID = ((Object)other).GetInstanceID();
		if (passiveBindings.TryGetActive(partId, out var bindings) && bindings.TryGetValue(instanceID, out var value) && (!TryGetRing(value.PartId, out var ring) || !TryGetRing(partId, out var ring2) || !ring2.HasRingOverlapWith(ring)))
		{
			GameplayManager instance = SingletonComponent<GameplayManager>.Instance;
			if (instance == null || instance.LockFeature?.ShouldPreserveBindingOnDragExit(partId) != true)
			{
				passiveBindings.TryRemoveActiveBinding(partId, instanceID, out var _);
			}
		}
	}

	internal void CleanSeparatedActiveBindings(Component movingRoot)
	{
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		bool ownerInterlockActive;
		bool catchGuideCollected;
		if (!((Object)(object)movingRoot == (Object)null) && TryResolveOwnerPartId(movingRoot, out var ownerPartId) && passiveBindings.TryGetActive(ownerPartId, out var bindings) && bindings.Count != 0 && TryGetRing(ownerPartId, out var ownerRing))
		{
			ownerInterlockActive = ownerRing.TryCollectInterlockedPartners(interlockedPartners);
			catchGuideCollected = false;
			passiveBindings.CleanSeparated(ownerPartId, ShouldRemove, OnRemoved);
		}
		void OnRemoved(int colliderId, LockPassiveColliderBinding binding)
		{
			//IL_0007: Unknown result type (might be due to invalid IL or missing references)
			HasCatchGuideLink(binding.PartId);
		}
		bool ShouldRemove(int colliderId, LockPassiveColliderBinding binding)
		{
			//IL_0021: Unknown result type (might be due to invalid IL or missing references)
			//IL_0050: Unknown result type (might be due to invalid IL or missing references)
			//IL_0084: Unknown result type (might be due to invalid IL or missing references)
			GameplayManager instance = SingletonComponent<GameplayManager>.Instance;
			if (instance != null && instance.LockFeature?.ShouldPreserveBindingOnDragExit(ownerPartId) == true)
			{
				return false;
			}
			if (TryGetRing(binding.PartId, out var ring))
			{
				if (ownerInterlockActive && interlockedPartners.Contains(ring))
				{
					return false;
				}
				return true;
			}
			if (!HasCatchGuideLink(binding.PartId))
			{
				return false;
			}
			if (!catchGuideCollected)
			{
				ownerRing.FillCatchGuideOverlapInstanceIds(catchGuideOverlapInstanceIds);
				catchGuideCollected = true;
			}
			return !catchGuideOverlapInstanceIds.Contains(colliderId);
		}
	}

	internal bool HasActiveRingBindings(RingObject ring)
	{
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)ring == (Object)null || ring.IsSolved)
		{
			return false;
		}
		if (!TryGetPartId(ring, out var partId))
		{
			return false;
		}
		if (passiveBindings.HasLockers(partId))
		{
			return true;
		}
		if (passiveBindings.TryGetActive(partId, out var bindings))
		{
			return bindings.Count > 0;
		}
		return false;
	}

	internal bool TryGetActiveLockers(RingObject ring, out IReadOnlyCollection<PartId> lockers)
	{
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		lockers = null;
		if ((Object)(object)ring == (Object)null || ring.IsSolved)
		{
			return false;
		}
		if (!TryGetPartId(ring, out var partId))
		{
			return false;
		}
		HashSet<PartId> hashSet = null;
		if (passiveBindings.TryGetLockers(partId, out var lockers2) && lockers2.Count > 0)
		{
			hashSet = activeLockersBuffer;
			hashSet.Clear();
			hashSet.UnionWith(lockers2);
		}
		if (passiveBindings.TryGetActive(partId, out var bindings) && bindings.Count > 0)
		{
			if (hashSet == null)
			{
				hashSet = activeLockersBuffer;
				hashSet.Clear();
			}
			foreach (LockPassiveColliderBinding value in bindings.Values)
			{
				hashSet.Add(value.PartId);
			}
		}
		if (hashSet == null || hashSet.Count == 0)
		{
			return false;
		}
		lockers = hashSet;
		return true;
	}

	internal bool HasCatchGuideLink(PartId guidePartId)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		return ringObjectByGuidePartId.ContainsKey(guidePartId);
	}

	internal bool CanCatchGuidePartSolve(PartId guidePartId)
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		if (string.IsNullOrEmpty(((PartId)(ref guidePartId)).Value))
		{
			return false;
		}
		if (ringObjectByGuidePartId.TryGetValue(guidePartId, out var value) && (Object)(object)value != (Object)null)
		{
			return EvaluateRingCatchGuidePartSolve(value, guidePartId);
		}
		if (!catchLockRegistry.TryGetCatchLockedPartId(guidePartId, out var lockedPartId) || !TryGetRing(lockedPartId, out var ring) || (Object)(object)ring == (Object)null)
		{
			return false;
		}
		return EvaluateRingCatchGuidePartSolve(ring, guidePartId);
	}

	private bool EvaluateRingCatchGuidePartSolve(RingObject linkedRing, PartId guidePartId)
	{
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		if (linkedRing.IsSolved)
		{
			return true;
		}
		if (!TryGetPartId(linkedRing, out var partId))
		{
			return false;
		}
		return !passiveBindings.OwnerHasActiveBindingToPartner(partId, guidePartId);
	}

	internal bool TryGetStaticBindings(RingObject ring, out Dictionary<int, LockPassiveColliderBinding> bindings)
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		bindings = null;
		if (TryGetPartId(ring, out var partId))
		{
			return passiveBindings.TryGetStatic(partId, out bindings);
		}
		return false;
	}

	internal bool TryGetRing(PartId partId, out RingObject ring)
	{
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		ring = null;
		return (SingletonComponent<GameplayManager>.Instance?.ObjectsService)?.TryGetRing(partId, out ring) ?? false;
	}

	internal void OnRingPointerDown(RingObject ring)
	{
		blockCheck.OnRingPointerDown(ring);
	}

	internal bool ShouldIgnoreBlock(RingObject ring, Collider2D col, RingRelative ownerSource = null)
	{
		return blockCheck.ShouldIgnoreBlock(ring, col, ownerSource);
	}

	internal bool HasNewCollider(RingObject ring, RingRelative ownerSource, Collider2D[] colliders, int count)
	{
		return blockCheck.HasNewCollider(ring, ownerSource, colliders, count);
	}

	internal bool TryResolveBlocker(RingObject ring, RingRelative ownerSource, out Collider2D blocker)
	{
		return blockCheck.TryResolveBlocker(ring, ownerSource, out blocker);
	}

	private void RemoveGuideLinksForRingObject(RingObject solvedRing)
	{
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)solvedRing == (Object)null || ringObjectByGuidePartId.Count == 0)
		{
			return;
		}
		List<PartId> list = new List<PartId>();
		foreach (KeyValuePair<PartId, RingObject> item in ringObjectByGuidePartId)
		{
			if ((Object)(object)item.Value == (Object)(object)solvedRing)
			{
				list.Add(item.Key);
			}
		}
		for (int i = 0; i < list.Count; i++)
		{
			ringObjectByGuidePartId.Remove(list[i]);
		}
	}

	private bool TryGetPartId(RingObject ring, out PartId partId)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		partId = default(PartId);
		if ((Object)(object)ring != (Object)null)
		{
			return ring.TryGetPartId(out partId);
		}
		return false;
	}

	private bool TryResolveConfigured(Component moving, Collider2D other, out PartId ownerPartId, out int colliderId, out LockPassiveColliderBinding binding)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		ownerPartId = default(PartId);
		colliderId = 0;
		binding = default(LockPassiveColliderBinding);
		if ((Object)(object)moving == (Object)null || (Object)(object)other == (Object)null || !TryResolveOwnerPartId(moving, out ownerPartId))
		{
			return false;
		}
		colliderId = ((Object)other).GetInstanceID();
		if (!passiveBindings.TryGetStatic(ownerPartId, out var bindings))
		{
			return false;
		}
		return bindings.TryGetValue(colliderId, out binding);
	}

	private bool TryResolveOwnerPartId(Component component, out PartId partId)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		partId = default(PartId);
		if ((Object)(object)component == (Object)null)
		{
			return false;
		}
		return ((component as IGameplayObject) ?? component.GetComponentInParent<IGameplayObject>())?.TryGetPartId(out partId) ?? false;
	}
}
