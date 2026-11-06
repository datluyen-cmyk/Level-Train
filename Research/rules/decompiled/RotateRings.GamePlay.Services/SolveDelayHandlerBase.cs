using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using RotateRings.GamePlay.Events;

namespace RotateRings.GamePlay.Services;

public abstract class SolveDelayHandlerBase : ISolveDelayHandler, IDisposable
{
	public virtual int Priority => int.MaxValue;

	public virtual bool GatesBreakReady => false;

	public virtual bool IsNeedToDeferSolve(IGameplayObject obj, BombRelative sourceBomb)
	{
		return false;
	}

	public virtual UniTask WaitSolveGateAsync(IGameplayObject obj, CancellationToken ct)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		return UniTask.CompletedTask;
	}

	public virtual bool IsNeedToWait(RingAfterSolveEvent evt)
	{
		return false;
	}

	public virtual UniTask WaitReadyAsync(RingAfterSolveEvent evt, CancellationToken ct)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		return UniTask.CompletedTask;
	}

	public virtual void Dispose()
	{
	}
}
