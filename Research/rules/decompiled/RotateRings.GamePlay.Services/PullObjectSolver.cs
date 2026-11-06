using UnityEngine;

namespace RotateRings.GamePlay.Services;

public sealed class PullObjectSolver : SolvableSolverBase<PullObject>
{
	public override bool CanSolve(PullObject obj)
	{
		if ((Object)(object)obj == (Object)null)
		{
			return false;
		}
		RingLockPassiveService ringLockPassiveService = SingletonComponent<GameplayManager>.Instance?.LockPassive;
		if (ringLockPassiveService != null && ringLockPassiveService.HasActivePullBindings(obj))
		{
			return false;
		}
		return true;
	}

	protected override void MarkSolved(PullObject obj)
	{
		obj.SetSolved();
	}

	protected override void OnPreSolve(PullObject obj, bool isFromBomb)
	{
		obj.OnSolveDragCleanup();
		obj.DisableAllPhysicColliders();
	}

	protected override void SolveRingRelatives(PullObject obj)
	{
		obj.SolveAllRingRelatives();
	}

	protected override void OnPostSolve(PullObject obj)
	{
		obj.InvokeOnSolved();
	}

	protected override GameObject GetPartGameObject(PullObject obj)
	{
		return ((Component)obj).gameObject;
	}
}
