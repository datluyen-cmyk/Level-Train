using System;
using System.Collections.Generic;

namespace UnlockRing.Game.Gameplay;

public readonly struct HeldGestureRange : IEquatable<HeldGestureRange>
{
	public double MinimumSignedAmount { get; }

	public double MaximumSignedAmount { get; }

	public HeldGestureRange(double MinimumSignedAmount, double MaximumSignedAmount)
	{
		this.MinimumSignedAmount = MinimumSignedAmount;
		this.MaximumSignedAmount = MaximumSignedAmount;
	}

	public void Deconstruct(out double MinimumSignedAmount, out double MaximumSignedAmount)
	{
		MinimumSignedAmount = this.MinimumSignedAmount;
		MaximumSignedAmount = this.MaximumSignedAmount;
	}

	public override string ToString()
	{
		return $"HeldGestureRange {{ MinimumSignedAmount = {MinimumSignedAmount}, MaximumSignedAmount = {MaximumSignedAmount} }}";
	}

	public bool Equals(HeldGestureRange other)
	{
		if (EqualityComparer<double>.Default.Equals(MinimumSignedAmount, other.MinimumSignedAmount))
		{
			return EqualityComparer<double>.Default.Equals(MaximumSignedAmount, other.MaximumSignedAmount);
		}
		return false;
	}

	public override bool Equals(object? obj)
	{
		if (obj is HeldGestureRange other)
		{
			return Equals(other);
		}
		return false;
	}

	public override int GetHashCode()
	{
		return EqualityComparer<double>.Default.GetHashCode(MinimumSignedAmount) * -1521134295 + EqualityComparer<double>.Default.GetHashCode(MaximumSignedAmount);
	}

	public static bool operator ==(HeldGestureRange left, HeldGestureRange right)
	{
		return left.Equals(right);
	}

	public static bool operator !=(HeldGestureRange left, HeldGestureRange right)
	{
		return !left.Equals(right);
	}

	public double Clamp(double amount)
	{
		return Math.Max(MinimumSignedAmount, Math.Min(MaximumSignedAmount, amount));
	}
}
