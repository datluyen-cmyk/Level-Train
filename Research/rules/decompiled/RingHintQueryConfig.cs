public static class RingHintQueryConfig
{
	public const int SolverBonus = 1000;

	public const int ReleaseWeight = 500;

	public const int OperableWeight = 100;

	public const int OrderBonusMax = 50;

	public const int AmountBonusMax = 30;

	public const double AmountRef = 90.0;

	public const int SolverMs = 3000;

	public const bool SkipSolverOnRelease = true;

	public const bool SkipSolverOnOperable = false;

	public const int ReleaseProgressEligibleBonus = 100;

	public const int ReleaseProgressWeightPerBlocker = 50;

	public const double ReleaseTransHorizon = 50.0;

	public const double ReleaseRotHorizon = 180.0;

	public const int ReleaseRefineSteps = 16;
}
