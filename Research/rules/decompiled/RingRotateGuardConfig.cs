public static class RingRotateGuardConfig
{
	public static float MaxFingerDeltaDegPerFrame { get; set; } = 35f;

	public static float MaxRotationStepDeg { get; set; } = 4f;

	public static void ResetToDefaults()
	{
		MaxFingerDeltaDegPerFrame = 35f;
		MaxRotationStepDeg = 4f;
	}
}
