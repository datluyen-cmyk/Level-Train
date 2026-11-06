using System.Collections.Generic;
using UnityEngine;
using UnlockRing.Core.Authoring;

namespace RotateRings.GamePlay.Services;

internal static class RingPaletteLockBindingUtil
{
	internal static bool TryFindRingByAssemblyId(IReadOnlyList<RingObject> rings, int assemblyId, out RingObject ring)
	{
		ring = null;
		if (assemblyId <= 0 || rings == null)
		{
			return false;
		}
		for (int i = 0; i < rings.Count; i++)
		{
			RingObject ringObject = rings[i];
			if ((Object)(object)ringObject != (Object)null && ringObject.PartData.Id == assemblyId)
			{
				ring = ringObject;
				return true;
			}
		}
		return false;
	}

	internal static bool IsAssemblyInBindingLockedList(int assemblyId, PaletteLockKeyBindingEntry entry)
	{
		string[] lockedAssemblyNames = entry.lockedAssemblyNames;
		if (assemblyId <= 0 || lockedAssemblyNames == null)
		{
			return false;
		}
		for (int i = 0; i < lockedAssemblyNames.Length; i++)
		{
			if (EditablePartDraft.ParseAssemblyId(lockedAssemblyNames[i]) == assemblyId)
			{
				return true;
			}
		}
		return false;
	}
}
