using System;

namespace Game.Common.Services;

public static class HomePopupPipeline
{
	private static readonly ActivityPipeline<IHomePopupStep> _pipeline = new ActivityPipeline<IHomePopupStep>();

	private static Action _terminalAction;

	private static Action _pendingWhenIdle;

	private static bool _openAnimPlayedThisRound;

	private static bool _suppressNextBuild;

	private static IHomePopupStep _currentStep;

	private static bool _isExecuting;

	public static bool HasPlayedOpenAnim => _openAnimPlayedThisRound;

	public static bool IsIdle
	{
		get
		{
			if (_pipeline.IsEmpty)
			{
				return !_isExecuting;
			}
			return false;
		}
	}

	public static void MarkOpenAnimPlayed()
	{
		_openAnimPlayedThisRound = true;
	}

	public static void ResetOpenAnimFlag()
	{
		_openAnimPlayedThisRound = false;
	}

	public static void Register(IHomePopupStep step)
	{
		_pipeline.Register(step);
	}

	public static void SetTerminalAction(Action action)
	{
		_terminalAction = action;
	}

	public static bool TryExecuteWhenIdle(Action action)
	{
		if (!IsIdle)
		{
			return false;
		}
		action?.Invoke();
		return true;
	}

	public static void RunWhenIdle(Action action)
	{
		if (action != null)
		{
			if (IsIdle)
			{
				action();
			}
			else
			{
				_pendingWhenIdle = action;
			}
		}
	}

	private static void FlushPendingWhenIdle()
	{
		if (IsIdle)
		{
			Action pendingWhenIdle = _pendingWhenIdle;
			_pendingWhenIdle = null;
			pendingWhenIdle?.Invoke();
		}
	}

	public static void Reset()
	{
		_pipeline.Reset();
		_suppressNextBuild = false;
		_currentStep = null;
		_isExecuting = false;
		_pendingWhenIdle = null;
	}

	public static void Abort()
	{
		if (_currentStep is ICancellableStep cancellableStep)
		{
			cancellableStep.Cancel();
		}
		_pipeline.Clear();
		_terminalAction = null;
		_suppressNextBuild = true;
		_currentStep = null;
		_isExecuting = false;
		_pendingWhenIdle = null;
	}

	public static void BuildAndExecute()
	{
		if (_suppressNextBuild)
		{
			_suppressNextBuild = false;
		}
		else if (!_isExecuting)
		{
			_openAnimPlayedThisRound = false;
			_pipeline.Build();
			ContinueNext();
		}
	}

	public static void BuildAndExecute(params IHomePopupStep[] steps)
	{
		if (_suppressNextBuild)
		{
			_suppressNextBuild = false;
		}
		else if (!_isExecuting)
		{
			_openAnimPlayedThisRound = false;
			_pipeline.BuildFromSteps(steps);
			ContinueNext();
		}
	}

	public static void ContinueNext()
	{
		if (!_pipeline.TryDequeue(out var step))
		{
			_currentStep = null;
			_isExecuting = false;
			Action terminalAction = _terminalAction;
			_terminalAction = null;
			terminalAction?.Invoke();
			FlushPendingWhenIdle();
			return;
		}
		_currentStep = step;
		_isExecuting = true;
		try
		{
			step.Execute();
		}
		catch (Exception)
		{
			_currentStep = null;
			_isExecuting = false;
			FlushPendingWhenIdle();
			throw;
		}
	}
}
