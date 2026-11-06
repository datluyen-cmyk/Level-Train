using System.Collections.Generic;
using RotateRings.GamePlay.Objects;
using UnityEngine;
using UnlockRing.Core.Domain;

namespace RotateRings.GamePlay.Services;

internal sealed class CatchButterflySolver : CatchGuideSolveBase
{
	private readonly HashSet<PartId> butterflyBarHostPartIds = new HashSet<PartId>();

	internal CatchButterflySolver(CatchLockRegistry catchLockRegistry, RingPassiveLockDomain ringDomain, PullPassiveLockDomain pullDomain)
		: base(catchLockRegistry, ringDomain, pullDomain)
	{
	}

	internal bool CanCatchButterflySolve(FixedButterflyObject butterfly)
	{
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)butterfly == (Object)null || butterfly.IsSolved)
		{
			return false;
		}
		bool result = false;
		PartId guidePartId2 = default(PartId);
		for (int i = 0; i <= 1; i++)
		{
			if (!butterfly.TryGetSlotGuidePartId(i, out var guidePartId))
			{
				continue;
			}
			((PartId)(ref guidePartId2))._002Ector(guidePartId);
			if (catchLockRegistry.TryGetCatchLockedPartId(guidePartId2, out var _))
			{
				result = true;
				if (!CanCatchGuidePartSolve(guidePartId2))
				{
					return false;
				}
			}
		}
		return result;
	}

	internal void TryButterflyForLockedPartId(PartId lockedPartId)
	{
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ef: Unknown result type (might be due to invalid IL or missing references)
		if (string.IsNullOrEmpty(((PartId)(ref lockedPartId)).Value))
		{
			return;
		}
		RingObjectsService ringObjectsService = SingletonComponent<GameplayManager>.Instance?.ObjectsService;
		RingObjectSolveService ringObjectSolveService = SingletonComponent<GameplayManager>.Instance?.SolveService;
		if (ringObjectsService == null || ringObjectSolveService == null)
		{
			return;
		}
		butterflyBarHostPartIds.Clear();
		foreach (KeyValuePair<PartId, PartId> guideLockEntry in catchLockRegistry.GuideLockEntries)
		{
			PartId value = guideLockEntry.Value;
			if (((PartId)(ref value)).Equals(lockedPartId) && catchLockRegistry.IsButterflySlotGuide(guideLockEntry.Key) && catchLockRegistry.TryGetButterflyHost(guideLockEntry.Key, out var hostPartId) && !string.IsNullOrEmpty(((PartId)(ref hostPartId)).Value) && butterflyBarHostPartIds.Add(hostPartId) && ringObjectsService.TryGetPartObject(hostPartId, out var gameplayObject) && gameplayObject is FixedButterflyObject { IsSolved: false } fixedButterflyObject && CanCatchButterflySolve(fixedButterflyObject))
			{
				ringObjectSolveService.SolveObject(fixedButterflyObject, null, GameplaySolveReasonContext.PassivePointerUp(lockedPartId));
			}
		}
	}
}
