using RotateRings.GamePlay.Objects;
using UnityEngine;

namespace RotateRings.GamePlay.Services;

public sealed class FixedButterflyObjectSolver : SolvableSolverBase<FixedButterflyObject>
{
	public override bool CanSolve(FixedButterflyObject obj)
	{
		if ((Object)(object)obj == (Object)null)
		{
			return false;
		}
		return (SingletonComponent<GameplayManager>.Instance?.LockPassive)?.CanCatchButterflyBarSolve(obj) ?? false;
	}

	protected override void MarkSolved(FixedButterflyObject obj)
	{
		obj.SetSolved();
	}

	protected override void OnPreSolve(FixedButterflyObject obj, bool isFromBomb)
	{
		obj.DoFade();
	}

	protected override void OnPostSolve(FixedButterflyObject obj)
	{
		obj.InvokeOnSolved();
	}

	protected override GameObject GetPartGameObject(FixedButterflyObject obj)
	{
		return ((Component)obj).gameObject;
	}
}
