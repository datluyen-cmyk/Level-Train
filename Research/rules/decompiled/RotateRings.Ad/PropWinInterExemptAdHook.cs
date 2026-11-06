using System;
using RotateRings.Model;

namespace RotateRings.Ad;

public static class PropWinInterExemptAdHook
{
	private static bool isListening;

	public static void EnsureListening()
	{
		if (!isListening)
		{
			isListening = true;
			GameApp.Event.Listen<InLevelPropUseRecordedEvent>((Action<InLevelPropUseRecordedEvent>)OnInLevelPropUseRecorded);
		}
	}

	private static void OnInLevelPropUseRecorded(InLevelPropUseRecordedEvent eventData)
	{
		PropWinInterExemptExperiment.OnPropUsed();
	}
}
