using UnityEngine;

namespace RotateRings.GamePlay.Services;

public sealed class RingObjectSolver : SolvableSolverBase<RingObject>
{
	public override bool CanSolve(RingObject obj)
	{
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)obj == (Object)null)
		{
			return false;
		}
		RingLockActiveService ringLockActiveService = SingletonComponent<GameplayManager>.Instance?.LockActive;
		if (ringLockActiveService != null && obj.TryGetPartId(out var partId) && ringLockActiveService.HasActiveLockTargets(partId))
		{
			return false;
		}
		RingLockPassiveService ringLockPassiveService = SingletonComponent<GameplayManager>.Instance?.LockPassive;
		if (ringLockPassiveService != null && ringLockPassiveService.HasActiveRingBindings(obj))
		{
			return false;
		}
		return true;
	}

	protected override void MarkSolved(RingObject obj)
	{
		obj.SetSolved();
	}

	protected override void OnPreSolve(RingObject obj, bool isFromBomb)
	{
		obj.OnSolveDragCleanup();
		obj.DisableAllPhysicColliders();
	}

	protected override void SolveRingRelatives(RingObject obj)
	{
		obj.SolveAllRingRelatives();
	}

	protected override void OnPostSolve(RingObject obj)
	{
		obj.InvokeOnSolved();
	}

	protected override GameObject GetPartGameObject(RingObject obj)
	{
		return ((Component)obj).gameObject;
	}
}
