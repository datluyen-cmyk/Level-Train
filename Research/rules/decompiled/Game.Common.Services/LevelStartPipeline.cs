using System;
using RotateRings.GamePlay.Events;
using RotateRings.Model;

namespace Game.Common.Services;

public static class LevelStartPipeline
{
	private static readonly ActivityPipeline<ILevelStartStep> pipeline = new ActivityPipeline<ILevelStartStep>();

	private static Action terminalAction;

	private static bool alreadyTriggered;

	private static bool finishedEventFired;

	private static int round;

	public static int CurrentRound => round;

	public static void Register(ILevelStartStep step)
	{
		pipeline.Register(step);
	}

	public static void SetTerminalAction(Action action)
	{
		terminalAction = action;
	}

	public static void BuildAndExecute()
	{
		if (!alreadyTriggered)
		{
			alreadyTriggered = true;
			finishedEventFired = false;
			pipeline.Build();
			ContinueNextForRound(round);
		}
	}

	public static void ContinueNext()
	{
		ContinueNextForRound(round);
	}

	public static void ContinueNextForRound(int stepRound)
	{
		if (stepRound == round && alreadyTriggered)
		{
			ContinueNextInternal();
		}
	}

	public static void Reset()
	{
		round++;
		terminalAction = null;
		alreadyTriggered = false;
		finishedEventFired = false;
		pipeline.Clear();
	}

	private static void ContinueNextInternal()
	{
		ILevelStartStep step;
		while (pipeline.TryDequeue(out step))
		{
			try
			{
				step.Execute();
				return;
			}
			catch (Exception)
			{
			}
		}
		if (!finishedEventFired)
		{
			finishedEventFired = true;
			Action action = terminalAction;
			terminalAction = null;
			action?.Invoke();
			GameApp.Event.Send<LevelStartPipelineFinishedEvent>(new LevelStartPipelineFinishedEvent
			{
				Level = (GameApp.Proxy.Get<LevelModelProxy>()?.Level ?? 0),
				Round = round
			});
		}
	}
}
