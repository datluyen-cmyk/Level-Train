using UnityEngine;

namespace RotateRings.UI.NewElementIntro;

public static class NewElementIntroRuntimeRules
{
	public const float DefaultMinimumDisplayTimeSeconds = 1f;

	private static float minimumDisplayTimeSeconds = 1f;

	public static float MinimumDisplayTimeSeconds
	{
		get
		{
			return minimumDisplayTimeSeconds;
		}
		set
		{
			minimumDisplayTimeSeconds = Mathf.Max(0f, value);
		}
	}

	public static void Reset()
	{
		minimumDisplayTimeSeconds = 1f;
	}
}
