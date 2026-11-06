using System.Collections.Generic;
using UnityEngine;
using UnlockRing.Core.Domain;

namespace RotateRings.GamePlay.Services;

internal sealed class PullPassiveLockDomain
{
	private const string TAG = "[GamePlay][RingLockPassive] ";

	private readonly PassiveLockBinding passiveBindings = new PassiveLockBinding();

	private readonly CatchLockRegistry catchLockRegistry;

	private readonly Dictionary<PartId, PullObject> pullObjectByGuidePartId = new Dictionary<PartId, PullObject>();

	private readonly HashSet<PartId> overlappedPartIds = new HashSet<PartId>();

	private readonly HashSet<int> catchGuideOverlapInstanceIds = new HashSet<int>();

	private readonly List<PartId> tempLinkedGuidePartIdsBuffer = new List<PartId>(2);

	internal int StaticOwnerCount => passiveBindings.StaticOwnerCount;

	internal PullPassiveLockDomain(CatchLockRegistry catchLockRegistry)
	{
		this.catchLockRegistry = catchLockRegistry;
	}

	internal int RegisterPartnerColliders(PartId ownerPartId, PartId partnerPartId, HashSet<int> colliderIds)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		Dictionary<int, LockPassiveColliderBinding> orCreateStatic = passiveBindings.GetOrCreateStatic(ownerPartId);
		return passiveBindings.RegisterMainBody(orCreateStatic, partnerPartId, colliderIds);
	}

	internal void MapCatchGuide(PartId guidePartId, PullObject ownerPull)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		pullObjectByGuidePartId[guidePartId] = ownerPull;
	}

	internal int GetStaticBindingCount(PartId ownerPartId)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		if (!passiveBindings.TryGetStatic(ownerPartId, out var bindings))
		{
			return 0;
		}
		return bindings.Count;
	}

	internal void CloneToActive()
	{
		passiveBindings.CloneToActive(BuildActiveForOwner);
	}

	private Dictionary<int, LockPassiveColliderBinding> BuildActiveForOwner(PartId ownerPartId, Dictionary<int, LockPassiveColliderBinding> staticBindings)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		_ = staticBindings.Count;
		if (!TryGetPull(ownerPartId, out var pull))
		{
			return null;
		}
		overlappedPartIds.Clear();
		pull.CollectOverlappedPartIds(overlappedPartIds);
		Dictionary<int, LockPassiveColliderBinding> dictionary = new Dictionary<int, LockPassiveColliderBinding>(staticBindings.Count);
		foreach (KeyValuePair<int, LockPassiveColliderBinding> staticBinding in staticBindings)
		{
			if (overlappedPartIds.Contains(staticBinding.Value.PartId))
			{
				dictionary[staticBinding.Key] = staticBinding.Value;
			}
		}
		return dictionary;
	}

	internal void Clear()
	{
		passiveBindings.Clear();
		pullObjectByGuidePartId.Clear();
	}

	internal void OnPullObjectSolved(PullObject solvedPull, List<PartId> linkedGuidePartIdsOut)
	{
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		linkedGuidePartIdsOut?.Clear();
		if (!((Object)(object)solvedPull == (Object)null) && TryGetPullPartId(solvedPull, out var partId))
		{
			tempLinkedGuidePartIdsBuffer.Clear();
			TryGetLinkedGuidePartIdsForPull(solvedPull, tempLinkedGuidePartIdsBuffer);
			if (linkedGuidePartIdsOut != null && tempLinkedGuidePartIdsBuffer.Count > 0)
			{
				linkedGuidePartIdsOut.AddRange(tempLinkedGuidePartIdsBuffer);
			}
			passiveBindings.RemoveOwner(partId);
			RemoveGuideLinksForPullObject(solvedPull);
		}
	}

	internal void OnFixedObjectSolved(FixedObject solvedFixed)
	{
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		if (!((Object)(object)solvedFixed == (Object)null) && TryGetFixedPartId(solvedFixed, out var partId))
		{
			pullObjectByGuidePartId.TryGetValue(partId, out var _);
			passiveBindings.RemovePartnerEverywhere(partId);
			pullObjectByGuidePartId.Remove(partId);
		}
	}

	internal void RemoveStrippedBindingsForSolvedRing(HashSet<int> strippedColliderIds, PartId solvedPartId, bool hasSolvedPartId)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		passiveBindings.RemoveSolvedPartnerEverywhere(strippedColliderIds, solvedPartId, hasSolvedPartId);
	}

	internal void TryActivateBinding(Component moving, Collider2D other)
	{
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		if (!((Object)(object)moving == (Object)null) && !((Object)(object)other == (Object)null) && TryResolveConfiguredBinding(moving, other, out var ownerPartId, out var colliderId, out var binding))
		{
			passiveBindings.TryAddActiveBinding(ownerPartId, colliderId, binding);
		}
	}

	internal void TryDeactivateBinding(Component moving, Collider2D other)
	{
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)moving == (Object)null || (Object)(object)other == (Object)null || !TryResolveOwnerPartId(moving, out var partId))
		{
			return;
		}
		int instanceID = ((Object)other).GetInstanceID();
		if (passiveBindings.TryGetActive(partId, out var bindings) && bindings.TryGetValue(instanceID, out var _))
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
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		PartId ownerPartId;
		if (!((Object)(object)movingRoot == (Object)null))
		{
			PullObject pullObject = (movingRoot as PullObject) ?? movingRoot.GetComponentInParent<PullObject>();
			if (!((Object)(object)pullObject == (Object)null) && !pullObject.IsSolved && pullObject.TryGetPartId(out ownerPartId) && passiveBindings.TryGetActive(ownerPartId, out var bindings) && bindings.Count != 0)
			{
				overlappedPartIds.Clear();
				pullObject.CollectOverlappedPartIds(overlappedPartIds);
				pullObject.FillCatchGuideOverlapInstanceIds(catchGuideOverlapInstanceIds);
				passiveBindings.CleanSeparated(ownerPartId, ShouldRemove, OnRemoved);
			}
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
			//IL_007d: Unknown result type (might be due to invalid IL or missing references)
			GameplayManager instance = SingletonComponent<GameplayManager>.Instance;
			if (instance != null && instance.LockFeature?.ShouldPreserveBindingOnDragExit(ownerPartId) == true)
			{
				return false;
			}
			if (HasCatchGuideLink(binding.PartId))
			{
				return !catchGuideOverlapInstanceIds.Contains(colliderId);
			}
			return !overlappedPartIds.Contains(binding.PartId);
		}
	}

	internal bool TryGetLinkedPullObject(FixedObject guide, out PullObject pullObject)
	{
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		pullObject = null;
		if ((Object)(object)guide == (Object)null || guide.IsSolved || !TryGetFixedPartId(guide, out var partId))
		{
			return false;
		}
		if (!pullObjectByGuidePartId.TryGetValue(partId, out pullObject) || (Object)(object)pullObject == (Object)null || pullObject.IsSolved)
		{
			pullObject = null;
			return false;
		}
		return true;
	}

	internal bool IsPullLocklistCollider(PullObject pull, Collider2D col)
	{
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)pull == (Object)null || (Object)(object)col == (Object)null || !TryGetPullPartId(pull, out var partId))
		{
			return false;
		}
		if (passiveBindings.TryGetActive(partId, out var bindings))
		{
			return bindings.ContainsKey(((Object)col).GetInstanceID());
		}
		return false;
	}

	internal bool HasActivePullBindings(PullObject pull)
	{
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)pull == (Object)null || pull.IsSolved)
		{
			return false;
		}
		if (!TryGetPullPartId(pull, out var partId))
		{
			return false;
		}
		if (passiveBindings.TryGetActive(partId, out var bindings))
		{
			return bindings.Count > 0;
		}
		return false;
	}

	internal bool TryGetActiveHeldLockers(PullObject pull, out IReadOnlyCollection<PartId> lockers)
	{
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		lockers = null;
		if ((Object)(object)pull == (Object)null || pull.IsSolved)
		{
			return false;
		}
		if (!TryGetPullPartId(pull, out var partId))
		{
			return false;
		}
		if (!passiveBindings.TryGetActive(partId, out var bindings) || bindings.Count == 0)
		{
			return false;
		}
		HashSet<PartId> hashSet = new HashSet<PartId>();
		foreach (LockPassiveColliderBinding value in bindings.Values)
		{
			hashSet.Add(value.PartId);
		}
		if (hashSet.Count == 0)
		{
			return false;
		}
		lockers = hashSet;
		return true;
	}

	internal bool HasCatchGuideLink(PartId guidePartId)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		return pullObjectByGuidePartId.ContainsKey(guidePartId);
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
		if (pullObjectByGuidePartId.TryGetValue(guidePartId, out var value) && (Object)(object)value != (Object)null)
		{
			return EvaluatePullCatchGuidePartSolve(value, guidePartId);
		}
		if (!catchLockRegistry.TryGetCatchLockedPartId(guidePartId, out var lockedPartId) || !TryGetPull(lockedPartId, out var pull) || (Object)(object)pull == (Object)null)
		{
			return false;
		}
		return EvaluatePullCatchGuidePartSolve(pull, guidePartId);
	}

	private bool EvaluatePullCatchGuidePartSolve(PullObject linkedPull, PartId guidePartId)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		if (linkedPull.IsSolved)
		{
			return true;
		}
		return !HasActiveBindingToPart(linkedPull, guidePartId);
	}

	private bool HasActiveBindingToPart(PullObject pull, PartId partnerPartId)
	{
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)pull == (Object)null || pull.IsSolved || !TryGetPullPartId(pull, out var partId))
		{
			return false;
		}
		LockPassiveColliderBinding binding;
		if (passiveBindings.TryGetActive(partId, out var bindings))
		{
			return PassiveLockBinding.TryFindPartnerBinding(bindings, partnerPartId, out binding);
		}
		return false;
	}

	private void TryGetLinkedGuidePartIdsForPull(PullObject pull, List<PartId> output)
	{
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)pull == (Object)null || output == null || pullObjectByGuidePartId.Count == 0)
		{
			return;
		}
		foreach (KeyValuePair<PartId, PullObject> item in pullObjectByGuidePartId)
		{
			if ((Object)(object)item.Value == (Object)(object)pull)
			{
				output.Add(item.Key);
			}
		}
	}

	private void RemoveGuideLinksForPullObject(PullObject solvedPull)
	{
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)solvedPull == (Object)null || pullObjectByGuidePartId.Count == 0)
		{
			return;
		}
		List<PartId> list = new List<PartId>();
		foreach (KeyValuePair<PartId, PullObject> item in pullObjectByGuidePartId)
		{
			if ((Object)(object)item.Value == (Object)(object)solvedPull)
			{
				list.Add(item.Key);
			}
		}
		for (int i = 0; i < list.Count; i++)
		{
			pullObjectByGuidePartId.Remove(list[i]);
		}
	}

	private bool TryResolveConfiguredBinding(Component moving, Collider2D other, out PartId ownerPartId, out int colliderId, out LockPassiveColliderBinding binding)
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

	private bool TryGetPull(PartId partId, out PullObject pull)
	{
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		pull = null;
		return (SingletonComponent<GameplayManager>.Instance?.ObjectsService)?.TryGetPull(partId, out pull) ?? false;
	}

	private bool TryGetFixed(PartId partId, out FixedObject fixedObj)
	{
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		fixedObj = null;
		return (SingletonComponent<GameplayManager>.Instance?.ObjectsService)?.TryGetFixed(partId, out fixedObj) ?? false;
	}

	private bool TryGetPullPartId(PullObject pull, out PartId partId)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		partId = default(PartId);
		if ((Object)(object)pull != (Object)null)
		{
			return pull.TryGetPartId(out partId);
		}
		return false;
	}

	private bool TryGetFixedPartId(FixedObject fixedObj, out PartId partId)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		partId = default(PartId);
		if ((Object)(object)fixedObj == (Object)null || string.IsNullOrEmpty(fixedObj.PartData.PhysicPartId))
		{
			return false;
		}
		partId = new PartId(fixedObj.PartData.PhysicPartId);
		return true;
	}
}
