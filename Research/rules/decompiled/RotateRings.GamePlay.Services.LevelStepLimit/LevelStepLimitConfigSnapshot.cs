namespace RotateRings.GamePlay.Services.LevelStepLimit;

public struct LevelStepLimitConfigSnapshot
{
	public int GroupId;

	public bool Enable;

	public int MaxSteps;

	public int AddStepsPerAd;

	public int MaxAdWatchCount;

	public LevelStepLimitLevelMode LevelMode;

	public float AngleThreshold;

	public float DistanceThreshold;
}
