using UnityEngine;
using UnlockRing.Core.Domain;

namespace RotateRings.GamePlay.Services;

internal abstract class CatchGuideSolveBase
{
	private readonly RingPassiveLockDomain ringDomain;

	private readonly PullPassiveLockDomain pullDomain;

	protected readonly CatchLockRegistry catchLockRegistry;

	protected CatchGuideSolveBase(CatchLockRegistry catchLockRegistry, RingPassiveLockDomain ringDomain, PullPassiveLockDomain pullDomain)
	{
		this.catchLockRegistry = catchLockRegistry;
		this.ringDomain = ringDomain;
		this.pullDomain = pullDomain;
	}

	internal bool CanCatchGuideSolve(FixedObject guide)
	{
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)guide == (Object)null || guide.IsSolved || string.IsNullOrEmpty(guide.PartData.PhysicPartId))
		{
			return false;
		}
		return CanCatchGuidePartSolve(new PartId(guide.PartData.PhysicPartId));
	}

	protected bool CanCatchGuidePartSolve(PartId guidePartId)
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		if (string.IsNullOrEmpty(((PartId)(ref guidePartId)).Value))
		{
			return false;
		}
		if (catchLockRegistry.TryGetCatchLockedPartId(guidePartId, out var lockedPartId) && catchLockRegistry.IsCatchLockedPartSolved(lockedPartId))
		{
			return true;
		}
		if (pullDomain.HasCatchGuideLink(guidePartId))
		{
			return pullDomain.CanCatchGuidePartSolve(guidePartId);
		}
		if (ringDomain.HasCatchGuideLink(guidePartId))
		{
			return ringDomain.CanCatchGuidePartSolve(guidePartId);
		}
		if (!catchLockRegistry.TryGetCatchLockedPartId(guidePartId, out var _))
		{
			return false;
		}
		if (!ringDomain.CanCatchGuidePartSolve(guidePartId))
		{
			return pullDomain.CanCatchGuidePartSolve(guidePartId);
		}
		return true;
	}
}
