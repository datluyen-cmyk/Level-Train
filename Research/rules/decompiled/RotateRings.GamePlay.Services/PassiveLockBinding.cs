using System;
using System.Collections.Generic;
using UnityEngine;
using UnlockRing.Core.Domain;

namespace RotateRings.GamePlay.Services;

internal sealed class PassiveLockBinding
{
	private readonly Dictionary<PartId, Dictionary<int, LockPassiveColliderBinding>> staticByPartId = new Dictionary<PartId, Dictionary<int, LockPassiveColliderBinding>>();

	private readonly Dictionary<PartId, Dictionary<int, LockPassiveColliderBinding>> activeByPartId = new Dictionary<PartId, Dictionary<int, LockPassiveColliderBinding>>();

	private readonly Dictionary<PartId, HashSet<PartId>> lockedByMap = new Dictionary<PartId, HashSet<PartId>>();

	private readonly List<PartId> emptyActiveKeyBuffer = new List<PartId>();

	private readonly List<int> resetRemoveColliderBuffer = new List<int>();

	internal int StaticOwnerCount => staticByPartId.Count;

	internal IReadOnlyDictionary<PartId, Dictionary<int, LockPassiveColliderBinding>> ActiveByPartId => activeByPartId;

	internal IEnumerable<KeyValuePair<PartId, Dictionary<int, LockPassiveColliderBinding>>> StaticEntries => staticByPartId;

	internal int LockedByCount => lockedByMap.Count;

	internal Dictionary<int, LockPassiveColliderBinding> GetOrCreateStatic(PartId partId)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		if (!staticByPartId.TryGetValue(partId, out var value))
		{
			value = new Dictionary<int, LockPassiveColliderBinding>();
			staticByPartId[partId] = value;
		}
		return value;
	}

	internal bool TryGetStatic(PartId partId, out Dictionary<int, LockPassiveColliderBinding> bindings)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		return staticByPartId.TryGetValue(partId, out bindings);
	}

	internal int RegisterMainBody(Dictionary<int, LockPassiveColliderBinding> bindings, PartId partnerPartId, HashSet<int> colliderIds, float baselinePartnerAngleZ = 0f)
	{
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		if (bindings == null || colliderIds == null || colliderIds.Count == 0)
		{
			return 0;
		}
		int num = 0;
		foreach (int colliderId in colliderIds)
		{
			if (bindings.TryGetValue(colliderId, out var value) && ((PartId)(ref value.PartId)).Equals(partnerPartId))
			{
				if (!Mathf.Approximately(value.BaselinePartnerAngleZ, baselinePartnerAngleZ))
				{
					bindings[colliderId] = new LockPassiveColliderBinding(partnerPartId, value.RingRelativeId, baselinePartnerAngleZ);
				}
			}
			else
			{
				bindings[colliderId] = new LockPassiveColliderBinding(partnerPartId, 0, baselinePartnerAngleZ);
				num++;
			}
		}
		return num;
	}

	internal static bool TryFindPartnerBinding(Dictionary<int, LockPassiveColliderBinding> bindings, PartId partnerPartId, out LockPassiveColliderBinding binding)
	{
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		binding = default(LockPassiveColliderBinding);
		if (bindings == null)
		{
			return false;
		}
		foreach (LockPassiveColliderBinding value in bindings.Values)
		{
			LockPassiveColliderBinding current = value;
			if (((PartId)(ref current.PartId)).Equals(partnerPartId))
			{
				binding = current;
				return true;
			}
		}
		return false;
	}

	internal bool TryGetActive(PartId partId, out Dictionary<int, LockPassiveColliderBinding> bindings)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		return activeByPartId.TryGetValue(partId, out bindings);
	}

	internal void RemoveOwner(PartId partId)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		staticByPartId.Remove(partId);
		activeByPartId.Remove(partId);
	}

	internal void RemoveActiveOwner(PartId partId)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		activeByPartId.Remove(partId);
	}

	internal void CloneToActive(Func<PartId, Dictionary<int, LockPassiveColliderBinding>, Dictionary<int, LockPassiveColliderBinding>> buildActiveForOwner)
	{
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		activeByPartId.Clear();
		foreach (KeyValuePair<PartId, Dictionary<int, LockPassiveColliderBinding>> item in staticByPartId)
		{
			Dictionary<int, LockPassiveColliderBinding> value = item.Value;
			if (value != null && value.Count != 0)
			{
				Dictionary<int, LockPassiveColliderBinding> dictionary = buildActiveForOwner(item.Key, value);
				if (dictionary != null && dictionary.Count > 0)
				{
					activeByPartId[item.Key] = dictionary;
				}
			}
		}
		RebuildLockedBy();
	}

	internal void CleanSeparated(PartId ownerPartId, Func<int, LockPassiveColliderBinding, bool> shouldRemove, Action<int, LockPassiveColliderBinding> onRemoved)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
		if (!activeByPartId.TryGetValue(ownerPartId, out var value) || value.Count == 0)
		{
			return;
		}
		resetRemoveColliderBuffer.Clear();
		foreach (KeyValuePair<int, LockPassiveColliderBinding> item in value)
		{
			if (shouldRemove(item.Key, item.Value))
			{
				resetRemoveColliderBuffer.Add(item.Key);
			}
		}
		if (resetRemoveColliderBuffer.Count == 0)
		{
			return;
		}
		for (int i = 0; i < resetRemoveColliderBuffer.Count; i++)
		{
			int num = resetRemoveColliderBuffer[i];
			if (value.TryGetValue(num, out var value2) && value.Remove(num))
			{
				SyncLockedByAfterPartnerChange(ownerPartId, value2.PartId);
				onRemoved?.Invoke(num, value2);
			}
		}
		if (value.Count == 0)
		{
			activeByPartId.Remove(ownerPartId);
		}
	}

	internal void RemoveSolvedPartnerEverywhere(HashSet<int> strippedColliderIds, PartId solvedPartId, bool hasSolvedPartId)
	{
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
		foreach (Dictionary<int, LockPassiveColliderBinding> value in staticByPartId.Values)
		{
			RemoveSolvedPartner(value, strippedColliderIds, solvedPartId, hasSolvedPartId);
		}
		emptyActiveKeyBuffer.Clear();
		foreach (KeyValuePair<PartId, Dictionary<int, LockPassiveColliderBinding>> item in activeByPartId)
		{
			RemoveSolvedPartner(item.Value, strippedColliderIds, solvedPartId, hasSolvedPartId);
			if (item.Value.Count == 0)
			{
				emptyActiveKeyBuffer.Add(item.Key);
			}
		}
		for (int i = 0; i < emptyActiveKeyBuffer.Count; i++)
		{
			activeByPartId.Remove(emptyActiveKeyBuffer[i]);
		}
	}

	internal void RemovePartnerEverywhere(PartId partnerPartId)
	{
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
		if (string.IsNullOrEmpty(((PartId)(ref partnerPartId)).Value))
		{
			return;
		}
		foreach (Dictionary<int, LockPassiveColliderBinding> value in staticByPartId.Values)
		{
			RemoveBindingsWithPartner(value, partnerPartId);
		}
		emptyActiveKeyBuffer.Clear();
		foreach (KeyValuePair<PartId, Dictionary<int, LockPassiveColliderBinding>> item in activeByPartId)
		{
			RemoveBindingsWithPartner(item.Value, partnerPartId);
			if (item.Value.Count == 0)
			{
				emptyActiveKeyBuffer.Add(item.Key);
			}
		}
		for (int i = 0; i < emptyActiveKeyBuffer.Count; i++)
		{
			activeByPartId.Remove(emptyActiveKeyBuffer[i]);
		}
	}

	private void RemoveSolvedPartner(Dictionary<int, LockPassiveColliderBinding> bindings, HashSet<int> strippedColliderIds, PartId solvedPartId, bool hasSolvedPartId)
	{
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		if (bindings == null)
		{
			return;
		}
		List<int> list = null;
		foreach (KeyValuePair<int, LockPassiveColliderBinding> binding in bindings)
		{
			if (ShouldRemoveBinding(binding.Key, binding.Value, strippedColliderIds, solvedPartId, hasSolvedPartId))
			{
				if (list == null)
				{
					list = new List<int>();
				}
				list.Add(binding.Key);
			}
		}
		if (list != null)
		{
			for (int i = 0; i < list.Count; i++)
			{
				bindings.Remove(list[i]);
			}
		}
	}

	private static bool ShouldRemoveBinding(int colliderId, LockPassiveColliderBinding binding, HashSet<int> strippedColliderIds, PartId solvedPartId, bool hasSolvedPartId)
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		if (!strippedColliderIds.Contains(colliderId))
		{
			if (hasSolvedPartId)
			{
				return ((PartId)(ref binding.PartId)).Equals(solvedPartId);
			}
			return false;
		}
		return true;
	}

	private static void RemoveBindingsWithPartner(Dictionary<int, LockPassiveColliderBinding> bindings, PartId partnerPartId)
	{
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		if (bindings == null)
		{
			return;
		}
		List<int> list = null;
		foreach (KeyValuePair<int, LockPassiveColliderBinding> binding in bindings)
		{
			LockPassiveColliderBinding value = binding.Value;
			if (((PartId)(ref value.PartId)).Equals(partnerPartId))
			{
				if (list == null)
				{
					list = new List<int>();
				}
				list.Add(binding.Key);
			}
		}
		if (list != null)
		{
			for (int i = 0; i < list.Count; i++)
			{
				bindings.Remove(list[i]);
			}
		}
	}

	internal bool TryGetLockers(PartId lockedPartId, out HashSet<PartId> lockers)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		return lockedByMap.TryGetValue(lockedPartId, out lockers);
	}

	internal bool HasLockers(PartId lockedPartId)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		if (lockedByMap.TryGetValue(lockedPartId, out var value))
		{
			return value.Count > 0;
		}
		return false;
	}

	internal void RebuildLockedBy()
	{
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		lockedByMap.Clear();
		foreach (KeyValuePair<PartId, Dictionary<int, LockPassiveColliderBinding>> item in activeByPartId)
		{
			PartId key = item.Key;
			foreach (LockPassiveColliderBinding value in item.Value.Values)
			{
				AddLockedByEntry(value.PartId, key);
			}
		}
	}

	internal bool OwnerHasActiveBindingToPartner(PartId ownerPartId, PartId partnerPartId)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		if (!activeByPartId.TryGetValue(ownerPartId, out var value))
		{
			return false;
		}
		foreach (LockPassiveColliderBinding value2 in value.Values)
		{
			LockPassiveColliderBinding current = value2;
			if (((PartId)(ref current.PartId)).Equals(partnerPartId))
			{
				return true;
			}
		}
		return false;
	}

	private void AddLockedByEntry(PartId lockedPartId, PartId lockerPartId)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		if (!lockedByMap.TryGetValue(lockedPartId, out var value))
		{
			value = new HashSet<PartId>();
			lockedByMap[lockedPartId] = value;
		}
		value.Add(lockerPartId);
	}

	private void RemoveLockedByEntry(PartId lockedPartId, PartId lockerPartId)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		if (lockedByMap.TryGetValue(lockedPartId, out var value) && value.Remove(lockerPartId) && value.Count == 0)
		{
			lockedByMap.Remove(lockedPartId);
		}
	}

	internal void Clear()
	{
		staticByPartId.Clear();
		activeByPartId.Clear();
		lockedByMap.Clear();
		emptyActiveKeyBuffer.Clear();
		resetRemoveColliderBuffer.Clear();
	}

	internal bool TryAddActiveBinding(PartId ownerPartId, int colliderId, LockPassiveColliderBinding binding)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		if (!activeByPartId.TryGetValue(ownerPartId, out var value))
		{
			value = new Dictionary<int, LockPassiveColliderBinding>();
			activeByPartId[ownerPartId] = value;
		}
		if (value.ContainsKey(colliderId))
		{
			return false;
		}
		value[colliderId] = binding;
		AddLockedByEntry(binding.PartId, ownerPartId);
		return true;
	}

	internal bool TryRemoveActiveBinding(PartId ownerPartId, int colliderId, out LockPassiveColliderBinding binding)
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		binding = default(LockPassiveColliderBinding);
		if (!activeByPartId.TryGetValue(ownerPartId, out var value))
		{
			return false;
		}
		if (!value.TryGetValue(colliderId, out binding))
		{
			return false;
		}
		if (!value.Remove(colliderId))
		{
			return false;
		}
		if (value.Count == 0)
		{
			activeByPartId.Remove(ownerPartId);
		}
		SyncLockedByAfterPartnerChange(ownerPartId, binding.PartId);
		return true;
	}

	private void SyncLockedByAfterPartnerChange(PartId ownerPartId, PartId partnerPartId)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		if (!OwnerHasActiveBindingToPartner(ownerPartId, partnerPartId))
		{
			RemoveLockedByEntry(partnerPartId, ownerPartId);
		}
	}
}
