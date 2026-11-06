using System;

namespace Game.Common.Services;

public static class SettlementPipeline
{
	private static readonly ActivityPipeline<ISettlementStep> _pipeline = new ActivityPipeline<ISettlementStep>();

	private static Action _terminalAction;

	public static void Register(ISettlementStep step)
	{
		_pipeline.Register(step);
	}

	public static void SetTerminalAction(Action action)
	{
		_terminalAction = action;
	}

	public static void BuildAndExecute()
	{
		_pipeline.Build();
		ContinueNext();
	}

	public static void ContinueNext()
	{
		if (!_pipeline.TryDequeue(out var step))
		{
			_terminalAction?.Invoke();
		}
		else
		{
			step.Execute();
		}
	}
}
