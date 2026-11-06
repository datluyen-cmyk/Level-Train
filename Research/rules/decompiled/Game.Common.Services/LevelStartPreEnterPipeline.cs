using System;

namespace Game.Common.Services;

public static class LevelStartPreEnterPipeline
{
	private static readonly ActivityPipeline<ILevelStartStep> _pipeline = new ActivityPipeline<ILevelStartStep>();

	private static Action _terminalAction;

	private static bool _terminalFired;

	private static int _round;

	public static int CurrentRound => _round;

	public static void Register(ILevelStartStep step)
	{
		_pipeline.Register(step);
	}

	public static void SetTerminalAction(Action action)
	{
		_terminalAction = action;
	}

	public static void BuildAndExecute()
	{
		_round++;
		_terminalFired = false;
		_pipeline.Build();
		ContinueNext();
	}

	public static void ContinueNextForRound(int round)
	{
		if (round == _round)
		{
			ContinueNext();
		}
	}

	public static void ContinueNext()
	{
		ILevelStartStep step;
		while (_pipeline.TryDequeue(out step))
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
		if (!_terminalFired)
		{
			_terminalFired = true;
			HardLevelToastPreEnterService.MarkCompleted();
			_terminalAction?.Invoke();
		}
	}

	public static void Reset()
	{
		_pipeline.Reset();
		_terminalFired = false;
	}
}
