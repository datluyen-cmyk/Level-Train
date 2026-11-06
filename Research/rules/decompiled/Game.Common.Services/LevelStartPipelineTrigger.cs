using System;
using RotateRings.GamePlay.Events;
using RotateRings.UI.NewElementIntro;

namespace Game.Common.Services;

public static class LevelStartPipelineTrigger
{
	private const string Tag = "[LevelStartPipeline] ";

	private static bool isListening;

	public static void EnsureListening()
	{
		if (!isListening)
		{
			isListening = true;
			GameApp.Event.Listen<LevelEnterCompletedEvent>((Action<LevelEnterCompletedEvent>)OnLevelEnterCompleted);
		}
	}

	private static void OnLevelEnterCompleted(LevelEnterCompletedEvent eventData)
	{
		ActivityModuleInit.EnsureInitialized();
		NewElementIntroService.RunAfterCurrentPopup(LevelStartPipeline.BuildAndExecute);
	}
}
