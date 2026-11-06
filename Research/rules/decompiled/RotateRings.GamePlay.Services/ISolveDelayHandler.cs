using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using RotateRings.GamePlay.Events;

namespace RotateRings.GamePlay.Services;

public interface ISolveDelayHandler : IDisposable
{
	int Priority { get; }

	bool GatesBreakReady { get; }

	bool IsNeedToDeferSolve(IGameplayObject obj, BombRelative sourceBomb);

	UniTask WaitSolveGateAsync(IGameplayObject obj, CancellationToken ct);

	bool IsNeedToWait(RingAfterSolveEvent evt);

	UniTask WaitReadyAsync(RingAfterSolveEvent evt, CancellationToken ct);
}
