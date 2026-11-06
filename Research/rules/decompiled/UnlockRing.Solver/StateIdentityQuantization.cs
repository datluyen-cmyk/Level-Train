using System;

namespace UnlockRing.Solver;

public sealed record StateIdentityQuantization
{
	public double Position { get; }

	public double RotationRadians { get; }

	public double SlideOffset { get; }

	public StateIdentityQuantization(double position = 1E-06, double rotationRadians = 1E-08, double slideOffset = 1E-06)
	{
		RequirePositiveFinite(position, "position");
		RequirePositiveFinite(rotationRadians, "rotationRadians");
		RequirePositiveFinite(slideOffset, "slideOffset");
		Position = position;
		RotationRadians = rotationRadians;
		SlideOffset = slideOffset;
	}

	private static void RequirePositiveFinite(double value, string name)
	{
		if (!Compatibility.IsFinite(value) || value <= 0.0)
		{
			throw new ArgumentOutOfRangeException(name);
		}
	}
}
