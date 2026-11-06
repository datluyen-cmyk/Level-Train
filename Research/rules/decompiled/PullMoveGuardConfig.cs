using UnityEngine;

public static class PullMoveGuardConfig
{
	public const float ReferenceFps = 60f;

	public static float MaxMovePerFrameWorld { get; set; } = 0.36f;

	public static float MaxMoveStepWorld { get; set; } = 0.12f;

	public static float GetMoveBudgetThisTickWorld()
	{
		return MaxMovePerFrameWorld * Time.deltaTime * 60f;
	}

	public static void ResetToDefaults()
	{
		MaxMovePerFrameWorld = 0.36f;
		MaxMoveStepWorld = 0.12f;
	}
}
