using System;

namespace UnlockRing.Solver.Core;

public sealed record CoreSolverOptions
{
	public double TranslationHorizon { get; }

	public double RotationHorizonDegrees { get; }

	public int CriticalProbeCount { get; }

	public int CriticalRefinementIterations { get; }

	public CoreSolverOptions(double translationHorizon = 100.0, double rotationHorizonDegrees = 360.0, int criticalProbeCount = 32, int criticalRefinementIterations = 28)
	{
		if (!Compatibility.IsFinite(translationHorizon) || translationHorizon <= 0.0)
		{
			throw new ArgumentOutOfRangeException("translationHorizon");
		}
		if (!Compatibility.IsFinite(rotationHorizonDegrees) || rotationHorizonDegrees <= 0.0)
		{
			throw new ArgumentOutOfRangeException("rotationHorizonDegrees");
		}
		if (criticalProbeCount < 2)
		{
			throw new ArgumentOutOfRangeException("criticalProbeCount");
		}
		if (criticalRefinementIterations < 0)
		{
			throw new ArgumentOutOfRangeException("criticalRefinementIterations");
		}
		TranslationHorizon = translationHorizon;
		RotationHorizonDegrees = rotationHorizonDegrees;
		CriticalProbeCount = criticalProbeCount;
		CriticalRefinementIterations = criticalRefinementIterations;
	}
}
