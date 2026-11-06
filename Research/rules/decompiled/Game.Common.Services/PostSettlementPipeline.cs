using System;

namespace Game.Common.Services;

public static class PostSettlementPipeline
{
	private static readonly ActivityPipeline<IPostSettlementStep> _pipeline = new ActivityPipeline<IPostSettlementStep>();

	private static Action _terminalAction;

	public static void Register(IPostSettlementStep step)
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
			Action terminalAction = _terminalAction;
			_terminalAction = null;
			terminalAction?.Invoke();
		}
		else
		{
			step.Execute();
		}
	}

	public static void Abort()
	{
		_pipeline.Clear();
		_terminalAction = null;
	}
}
