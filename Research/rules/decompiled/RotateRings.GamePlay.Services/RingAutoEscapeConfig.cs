using UnityEngine;

namespace RotateRings.GamePlay.Services;

public static class RingAutoEscapeConfig
{
	private static readonly float[] TriggerMillimetersByLevel = new float[4] { 2f, 4f, 6f, 8f };

	private static int appliedLevel = -1;

	public static float MinTriggerMillimeters { get; set; } = TriggerMillimetersByLevel[0];

	public static float MaxTriggerGestureMilliseconds { get; set; } = 1300f;

	public static float MinEscapeSpeed { get; set; } = 2f;

	public static float MaxEscapeSpeed { get; set; } = 30f;

	public static float EscapeAccelerateSeconds { get; set; } = 1f;

	public static void ApplyExperimentLevel(int level)
	{
		if (level != appliedLevel)
		{
			appliedLevel = level;
			MinTriggerMillimeters = GetMillimetersForLevel(level);
		}
	}

	private static float GetMillimetersForLevel(int level)
	{
		int num = Mathf.Clamp(level - 1, 0, TriggerMillimetersByLevel.Length - 1);
		return TriggerMillimetersByLevel[num];
	}

	public static void ResetToDefaults()
	{
		appliedLevel = -1;
		MinTriggerMillimeters = TriggerMillimetersByLevel[0];
		MaxTriggerGestureMilliseconds = 1300f;
		MinEscapeSpeed = 2f;
		MaxEscapeSpeed = 30f;
		EscapeAccelerateSeconds = 1f;
	}
}
