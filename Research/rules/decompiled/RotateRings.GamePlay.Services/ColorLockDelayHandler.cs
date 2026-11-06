using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.CompilerServices;
using RotateRings.GamePlay.Events;
using RotateRings.GamePlay.Objects;
using UnityEngine;

namespace RotateRings.GamePlay.Services;

public sealed class ColorLockDelayHandler : SolveDelayHandlerBase
{
	[CompilerGenerated]
	private sealed class _003C_003Ec__DisplayClass7_0
	{
		public ColorLockDelayHandler _003C_003E4__this;

		public RingObject ring;

		internal bool _003CWaitReadyAsync_003Eb__0()
		{
			return !_003C_003E4__this.IsFlyPending(ring);
		}
	}

	[StructLayout(LayoutKind.Auto)]
	[CompilerGenerated]
	private struct _003CWaitReadyAsync_003Ed__7 : IAsyncStateMachine
	{
		public int _003C_003E1__state;

		public AsyncUniTaskMethodBuilder _003C_003Et__builder;

		public ColorLockDelayHandler _003C_003E4__this;

		public RingAfterSolveEvent evt;

		public CancellationToken ct;

		private Awaiter<bool> _003C_003Eu__1;

		private void MoveNext()
		{
			//IL_009e: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
			//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
			//IL_0143: Unknown result type (might be due to invalid IL or missing references)
			//IL_0148: Unknown result type (might be due to invalid IL or missing references)
			//IL_014f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0059: Unknown result type (might be due to invalid IL or missing references)
			//IL_005e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0062: Unknown result type (might be due to invalid IL or missing references)
			//IL_0067: Unknown result type (might be due to invalid IL or missing references)
			//IL_006b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0070: Unknown result type (might be due to invalid IL or missing references)
			//IL_0084: Unknown result type (might be due to invalid IL or missing references)
			//IL_0085: Unknown result type (might be due to invalid IL or missing references)
			//IL_0101: Unknown result type (might be due to invalid IL or missing references)
			//IL_0106: Unknown result type (might be due to invalid IL or missing references)
			//IL_010a: Unknown result type (might be due to invalid IL or missing references)
			//IL_010f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0113: Unknown result type (might be due to invalid IL or missing references)
			//IL_0118: Unknown result type (might be due to invalid IL or missing references)
			//IL_012c: Unknown result type (might be due to invalid IL or missing references)
			//IL_012d: Unknown result type (might be due to invalid IL or missing references)
			int num = _003C_003E1__state;
			ColorLockDelayHandler colorLockDelayHandler = _003C_003E4__this;
			try
			{
				Awaiter<bool> val;
				if (num == 0)
				{
					val = _003C_003Eu__1;
					_003C_003Eu__1 = default(Awaiter<bool>);
					num = (_003C_003E1__state = -1);
					goto IL_00b9;
				}
				if (num == 1)
				{
					val = _003C_003Eu__1;
					_003C_003Eu__1 = default(Awaiter<bool>);
					num = (_003C_003E1__state = -1);
					goto IL_015e;
				}
				_003C_003Ec__DisplayClass7_0 CS_0024_003C_003E8__locals3 = new _003C_003Ec__DisplayClass7_0
				{
					_003C_003E4__this = _003C_003E4__this
				};
				UniTask val2;
				if (colorLockDelayHandler.TryResolveFlyPendingRing(evt, out CS_0024_003C_003E8__locals3.ring))
				{
					val2 = UniTask.WaitUntil((Func<bool>)(() => !CS_0024_003C_003E8__locals3._003C_003E4__this.IsFlyPending(CS_0024_003C_003E8__locals3.ring)), (PlayerLoopTiming)8, ct);
					val = ((UniTask)(ref val2)).SuppressCancellationThrow().GetAwaiter();
					if (!val.IsCompleted)
					{
						num = (_003C_003E1__state = 0);
						_003C_003Eu__1 = val;
						((AsyncUniTaskMethodBuilder)(ref _003C_003Et__builder)).AwaitUnsafeOnCompleted<Awaiter<bool>, _003CWaitReadyAsync_003Ed__7>(ref val, ref this);
						return;
					}
					goto IL_00b9;
				}
				goto end_IL_000e;
				IL_015e:
				val.GetResult();
				goto end_IL_000e;
				IL_00b9:
				if (!val.GetResult() && evt.GameplayObject is RingObject)
				{
					float paletteLockCascadeSolveDelaySeconds = RingLevelConfig.Instance.paletteLockCascadeSolveDelaySeconds;
					if (paletteLockCascadeSolveDelaySeconds > 0f)
					{
						val2 = UniTask.Delay(TimeSpan.FromSeconds(paletteLockCascadeSolveDelaySeconds), false, (PlayerLoopTiming)8, ct);
						val = ((UniTask)(ref val2)).SuppressCancellationThrow().GetAwaiter();
						if (!val.IsCompleted)
						{
							num = (_003C_003E1__state = 1);
							_003C_003Eu__1 = val;
							((AsyncUniTaskMethodBuilder)(ref _003C_003Et__builder)).AwaitUnsafeOnCompleted<Awaiter<bool>, _003CWaitReadyAsync_003Ed__7>(ref val, ref this);
							return;
						}
						goto IL_015e;
					}
				}
				end_IL_000e:;
			}
			catch (Exception exception)
			{
				_003C_003E1__state = -2;
				((AsyncUniTaskMethodBuilder)(ref _003C_003Et__builder)).SetException(exception);
				return;
			}
			_003C_003E1__state = -2;
			((AsyncUniTaskMethodBuilder)(ref _003C_003Et__builder)).SetResult();
		}

		void IAsyncStateMachine.MoveNext()
		{
			//ILSpy generated this explicit interface implementation from .override directive in MoveNext
			this.MoveNext();
		}

		[DebuggerHidden]
		private void SetStateMachine(IAsyncStateMachine stateMachine)
		{
			((AsyncUniTaskMethodBuilder)(ref _003C_003Et__builder)).SetStateMachine(stateMachine);
		}

		void IAsyncStateMachine.SetStateMachine(IAsyncStateMachine stateMachine)
		{
			//ILSpy generated this explicit interface implementation from .override directive in SetStateMachine
			this.SetStateMachine(stateMachine);
		}
	}

	private readonly RingSolveBreakDelayService host;

	public override int Priority => 2;

	public override bool GatesBreakReady => true;

	public ColorLockDelayHandler(RingSolveBreakDelayService host)
	{
		this.host = host;
	}

	public override bool IsNeedToWait(RingAfterSolveEvent evt)
	{
		RingObject ring;
		return TryResolveFlyPendingRing(evt, out ring);
	}

	[AsyncStateMachine(typeof(_003CWaitReadyAsync_003Ed__7))]
	public override UniTask WaitReadyAsync(RingAfterSolveEvent evt, CancellationToken ct)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		_003CWaitReadyAsync_003Ed__7 _003CWaitReadyAsync_003Ed__ = default(_003CWaitReadyAsync_003Ed__7);
		_003CWaitReadyAsync_003Ed__._003C_003Et__builder = AsyncUniTaskMethodBuilder.Create();
		_003CWaitReadyAsync_003Ed__._003C_003E4__this = this;
		_003CWaitReadyAsync_003Ed__.evt = evt;
		_003CWaitReadyAsync_003Ed__.ct = ct;
		_003CWaitReadyAsync_003Ed__._003C_003E1__state = -1;
		((AsyncUniTaskMethodBuilder)(ref _003CWaitReadyAsync_003Ed__._003C_003Et__builder)).Start<_003CWaitReadyAsync_003Ed__7>(ref _003CWaitReadyAsync_003Ed__);
		return ((AsyncUniTaskMethodBuilder)(ref _003CWaitReadyAsync_003Ed__._003C_003Et__builder)).Task;
	}

	private bool TryResolveFlyPendingRing(RingAfterSolveEvent evt, out RingObject ring)
	{
		ring = null;
		if (evt == null)
		{
			return false;
		}
		IGameplayObject gameplayObject = evt.GameplayObject;
		if (!(gameplayObject is RingObject ringObject))
		{
			if (gameplayObject is RingPaletteLockGapObject ringPaletteLockGapObject)
			{
				RingPaletteLockGapObject ringPaletteLockGapObject2 = ringPaletteLockGapObject;
				if ((Object)(object)ringPaletteLockGapObject2.HostRing != (Object)null && IsFlyPending(ringPaletteLockGapObject2.HostRing))
				{
					ring = ringPaletteLockGapObject2.HostRing;
					return true;
				}
			}
		}
		else
		{
			RingObject ringObject2 = ringObject;
			if (IsFlyPending(ringObject2))
			{
				ring = ringObject2;
				return true;
			}
			RingObject boundRing = ringObject;
			if (TryResolveBreakGateRing(boundRing, out var gateRing))
			{
				ring = gateRing;
				return true;
			}
		}
		return false;
	}

	private bool TryResolveBreakGateRing(RingObject boundRing, out RingObject gateRing)
	{
		gateRing = null;
		RingPaletteLockService ringPaletteLockService = SingletonComponent<GameplayManager>.Instance?.PaletteLock;
		if ((Object)(object)boundRing == (Object)null || ringPaletteLockService == null)
		{
			return false;
		}
		if (ringPaletteLockService.TryGetKeyUnlockFlyBreakGate(boundRing, out gateRing))
		{
			return IsFlyPending(gateRing);
		}
		return false;
	}

	private bool IsFlyPending(RingObject ring)
	{
		RingPaletteLockService ringPaletteLockService = SingletonComponent<GameplayManager>.Instance?.PaletteLock;
		if ((Object)(object)ring != (Object)null && ringPaletteLockService != null)
		{
			return ringPaletteLockService.IsKeyUnlockFlyPending(ring);
		}
		return false;
	}
}
