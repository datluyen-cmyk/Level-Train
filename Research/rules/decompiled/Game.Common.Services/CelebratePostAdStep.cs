using System;
using RotateRings.UI.LevelEnd;
using XingYun.Experiment;

namespace Game.Common.Services;

public class CelebratePostAdStep : ISettlementStep, IActivityStep
{
	private const string Tag = "[CelebratePostAdStep] ";

	private const float TypeFiveAndSixCloseAnimationTimeout = 3f;

	private const int PriorityValue = 30;

	private bool isTypeFiveAndSixCloseCompleted;

	private int typeFiveAndSixCloseTimeoutTimerId = -1;

	public int Priority => 30;

	public bool ShouldRun()
	{
		return LevelEndCelebrateService.IsEnabled();
	}

	public void Execute()
	{
		int num = FunctionDataCenter.Get<LevelEndFunction>()?.WinCelebrateType ?? 0;
		if (num == 5 || num == 6)
		{
			RequestTypeFiveAndSixCloseAnimation();
		}
		else
		{
			LevelEndCelebrateService.PlayExit(SettlementPipeline.ContinueNext);
		}
	}

	private void RequestTypeFiveAndSixCloseAnimation()
	{
		isTypeFiveAndSixCloseCompleted = false;
		StopTypeFiveAndSixCloseAnimationTimeout();
		GameApp.Event.Listen<LevelEndCelebrateCloseCompletedEvent>((Action<LevelEndCelebrateCloseCompletedEvent>)OnTypeFiveAndSixCloseAnimationCompleted);
		typeFiveAndSixCloseTimeoutTimerId = GameApp.Timer.Delay(3f, (Action)OnTypeFiveAndSixCloseAnimationTimedOut);
		GameApp.Event.Send<LevelEndCelebrateCloseRequestedEvent>(new LevelEndCelebrateCloseRequestedEvent());
	}

	private void OnTypeFiveAndSixCloseAnimationCompleted(LevelEndCelebrateCloseCompletedEvent eventData)
	{
		CompleteTypeFiveAndSixCloseAnimation(isTimedOut: false);
	}

	private void OnTypeFiveAndSixCloseAnimationTimedOut()
	{
		typeFiveAndSixCloseTimeoutTimerId = -1;
		CompleteTypeFiveAndSixCloseAnimation(isTimedOut: true);
	}

	private void StopTypeFiveAndSixCloseAnimationTimeout()
	{
		if (typeFiveAndSixCloseTimeoutTimerId != -1)
		{
			GameApp.Timer.Kill(typeFiveAndSixCloseTimeoutTimerId);
			typeFiveAndSixCloseTimeoutTimerId = -1;
		}
	}

	private void CompleteTypeFiveAndSixCloseAnimation(bool isTimedOut)
	{
		if (!isTypeFiveAndSixCloseCompleted)
		{
			isTypeFiveAndSixCloseCompleted = true;
			StopTypeFiveAndSixCloseAnimationTimeout();
			GameApp.Event.Unlisten<LevelEndCelebrateCloseCompletedEvent>((Action<LevelEndCelebrateCloseCompletedEvent>)OnTypeFiveAndSixCloseAnimationCompleted);
			SettlementPipeline.ContinueNext();
		}
	}
}
