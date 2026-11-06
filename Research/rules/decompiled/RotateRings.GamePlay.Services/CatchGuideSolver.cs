using System.Collections.Generic;
using UnlockRing.Core.Domain;

namespace RotateRings.GamePlay.Services;

internal sealed class CatchGuideSolver : CatchGuideSolveBase
{
	internal CatchGuideSolver(CatchLockRegistry catchLockRegistry, RingPassiveLockDomain ringDomain, PullPassiveLockDomain pullDomain)
		: base(catchLockRegistry, ringDomain, pullDomain)
	{
	}

	internal void TryPlainCatchGuides(IReadOnlyList<PartId> linkedGuidePartIds, PartId passiveByPartId)
	{
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		if (linkedGuidePartIds == null || linkedGuidePartIds.Count == 0)
		{
			return;
		}
		RingObjectSolveService ringObjectSolveService = SingletonComponent<GameplayManager>.Instance?.SolveService;
		RingObjectsService ringObjectsService = SingletonComponent<GameplayManager>.Instance?.ObjectsService;
		if (ringObjectSolveService == null || ringObjectsService == null)
		{
			return;
		}
		for (int i = 0; i < linkedGuidePartIds.Count; i++)
		{
			PartId val = linkedGuidePartIds[i];
			if (!catchLockRegistry.IsButterflySlotGuide(val) && ringObjectsService.TryGetFixed(val, out var fixedObj))
			{
				ringObjectSolveService.SolveObject(fixedObj, null, GameplaySolveReasonContext.PassivePointerUp(passiveByPartId));
			}
		}
	}
}
