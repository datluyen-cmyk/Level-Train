using UnityEngine;

namespace RotateRings.GamePlay.Services;

public sealed class FixedObjectSolver : SolvableSolverBase<FixedObject>
{
	public override bool CanSolve(FixedObject obj)
	{
		if ((Object)(object)obj == (Object)null)
		{
			return false;
		}
		RingLockPassiveService ringLockPassiveService = SingletonComponent<GameplayManager>.Instance?.LockPassive;
		if (ringLockPassiveService != null && ringLockPassiveService.CanCatchGuideSolve(obj))
		{
			return true;
		}
		return false;
	}

	protected override void MarkSolved(FixedObject obj)
	{
		obj.SetSolved();
	}

	protected override void OnPreSolve(FixedObject obj, bool isFromBomb)
	{
		obj.DoFade();
	}

	protected override void OnPostSolve(FixedObject obj)
	{
		obj.InvokeOnSolved();
	}

	protected override GameObject GetPartGameObject(FixedObject obj)
	{
		return ((Component)obj).gameObject;
	}
}
