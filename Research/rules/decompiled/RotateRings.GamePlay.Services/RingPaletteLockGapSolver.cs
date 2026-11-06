using RotateRings.GamePlay.Objects;
using UnityEngine;

namespace RotateRings.GamePlay.Services;

public sealed class RingPaletteLockGapSolver : SolvableSolverBase<RingPaletteLockGapObject>
{
	public override bool CanSolve(RingPaletteLockGapObject obj)
	{
		return false;
	}

	protected override void MarkSolved(RingPaletteLockGapObject obj)
	{
		obj.SetSolved();
	}

	protected override void OnPreSolve(RingPaletteLockGapObject obj, bool isFromBomb)
	{
		obj.DisableBlockCollidersOnSolve();
	}

	protected override void OnPostSolve(RingPaletteLockGapObject obj)
	{
		SingletonComponent<GameplayManager>.Instance?.PaletteLock?.OnGapSolved(obj);
		obj.InvokeOnSolved();
	}

	protected override GameObject GetPartGameObject(RingPaletteLockGapObject obj)
	{
		return ((Component)obj).gameObject;
	}
}
