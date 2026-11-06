using System;
using UnityEngine;
using XingYun.Core.Singletons;
using XingYun.Level;

namespace RotateRings.Ad;

internal static class AdColdLaunchGameTimeTracker
{
	private const string LogTag = "ColdLaunchGameTime";

	private static float completedAttemptSeconds;

	private static float currentAttemptStartSeconds;

	private static bool hasCurrentAttempt;

	public static void ResetForSession()
	{
		completedAttemptSeconds = 0f;
		currentAttemptStartSeconds = 0f;
		hasCurrentAttempt = false;
	}

	public static void BeginAttempt(float actualSecondsAtStart)
	{
		currentAttemptStartSeconds = Math.Max(0f, actualSecondsAtStart);
		hasCurrentAttempt = true;
	}

	public static void EndAttempt(float actualSecondsAtEnd)
	{
		if (hasCurrentAttempt)
		{
			float num = Math.Max(0f, actualSecondsAtEnd - currentAttemptStartSeconds);
			completedAttemptSeconds += num;
			hasCurrentAttempt = false;
			currentAttemptStartSeconds = 0f;
		}
	}

	public static float GetCurrentSeconds()
	{
		float num = 0f;
		if (hasCurrentAttempt && MainStageGameTimeGate.IsMainLevelSessionTracking)
		{
			GameTimeTracker instance = MonoSingleton<GameTimeTracker>.Instance;
			if ((Object)(object)instance != (Object)null)
			{
				num = Math.Max(0f, instance.ActualGameTime - currentAttemptStartSeconds);
			}
		}
		return completedAttemptSeconds + num;
	}
}
