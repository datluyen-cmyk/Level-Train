using System;
using System.Collections.Generic;

namespace UnlockRing.Solver;

public readonly struct SolverCost : IComparable<SolverCost>, IEquatable<SolverCost>
{
	public int IrreversibleRisk { get; }

	public int Actions { get; }

	public double MovementDistance { get; }

	public static SolverCost Zero => new SolverCost(0, 0, 0.0);

	public SolverCost(int irreversibleRisk, int actions, double movementDistance)
	{
		if (irreversibleRisk < 0)
		{
			throw new ArgumentOutOfRangeException("irreversibleRisk");
		}
		if (actions < 0)
		{
			throw new ArgumentOutOfRangeException("actions");
		}
		if (!Compatibility.IsFinite(movementDistance) || movementDistance < 0.0)
		{
			throw new ArgumentOutOfRangeException("movementDistance");
		}
		IrreversibleRisk = irreversibleRisk;
		Actions = actions;
		MovementDistance = movementDistance;
	}

	public SolverCost Add(SolverAction action)
	{
		return checked(new SolverCost(IrreversibleRisk + action.IrreversibleRisk, Actions + 1, MovementDistance + action.MovementDistance));
	}

	public int CompareTo(SolverCost other)
	{
		int num = IrreversibleRisk.CompareTo(other.IrreversibleRisk);
		if (num != 0)
		{
			return num;
		}
		num = Actions.CompareTo(other.Actions);
		if (num == 0)
		{
			return MovementDistance.CompareTo(other.MovementDistance);
		}
		return num;
	}

	public bool Equals(SolverCost other)
	{
		if (IrreversibleRisk == other.IrreversibleRisk && Actions == other.Actions)
		{
			return MovementDistance.Equals(other.MovementDistance);
		}
		return false;
	}

	public override bool Equals(object? obj)
	{
		if (obj is SolverCost other)
		{
			return Equals(other);
		}
		return false;
	}

	public override int GetHashCode()
	{
		return (EqualityComparer<int>.Default.GetHashCode(IrreversibleRisk) * -1521134295 + EqualityComparer<int>.Default.GetHashCode(Actions)) * -1521134295 + EqualityComparer<double>.Default.GetHashCode(MovementDistance);
	}

	public override string ToString()
	{
		return "SolverCost { IrreversibleRisk = " + IrreversibleRisk + ", Actions = " + Actions + ", MovementDistance = " + MovementDistance + " }";
	}

	public static bool operator ==(SolverCost left, SolverCost right)
	{
		return left.Equals(right);
	}

	public static bool operator !=(SolverCost left, SolverCost right)
	{
		return !left.Equals(right);
	}
}
