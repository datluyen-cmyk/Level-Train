using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.CompilerServices;
using RotateRings.GamePlay.Events;
using UnlockRing;

namespace RotateRings.GamePlay.Services;

public sealed class BombBreakDelayHandler : SolveDelayHandlerBase
{
	[CompilerGenerated]
	private sealed class _003C_003Ec__DisplayClass9_0
	{
		public BombBreakDelayHandler _003C_003E4__this;

		public UniTaskCompletionSource tcs;

		public CancellationToken ct;

		internal void _003CWaitReadyAsync_003Eb__0()
		{
			_003C_003E4__this.queueWaiters.Remove(tcs);
			tcs.TrySetCanceled(ct);
		}
	}

	[StructLayout(LayoutKind.Auto)]
	[CompilerGenerated]
	private struct _003CWaitReadyAsync_003Ed__9 : IAsyncStateMachine
	{
		public int _003C_003E1__state;

		public AsyncUniTaskMethodBuilder _003C_003Et__builder;

		public BombBreakDelayHandler _003C_003E4__this;

		public CancellationToken ct;

		public RingAfterSolveEvent evt;

		private CancellationTokenRegistration _003C_003E7__wrap1;

		private Awaiter<bool> _003C_003Eu__1;

		private void MoveNext()
		{
			//IL_00db: Unknown result type (might be due to invalid IL or missing references)
			//IL_00e0: Unknown result type (might be due to invalid IL or missing references)
			//IL_00e7: Unknown result type (might be due to invalid IL or missing references)
			//IL_0099: Unknown result type (might be due to invalid IL or missing references)
			//IL_009e: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c4: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c5: Unknown result type (might be due to invalid IL or missing references)
			//IL_0057: Unknown result type (might be due to invalid IL or missing references)
			//IL_0061: Expected O, but got Unknown
			int num = _003C_003E1__state;
			BombBreakDelayHandler bombBreakDelayHandler = _003C_003E4__this;
			try
			{
				if (num == 0)
				{
					goto IL_0090;
				}
				_003C_003Ec__DisplayClass9_0 _003C_003Ec__DisplayClass9_ = new _003C_003Ec__DisplayClass9_0
				{
					_003C_003E4__this = _003C_003E4__this,
					ct = ct
				};
				if (!_003C_003Ec__DisplayClass9_.ct.IsCancellationRequested && evt != null && evt.IsFromBomb)
				{
					_003C_003Ec__DisplayClass9_.tcs = new UniTaskCompletionSource();
					bombBreakDelayHandler.queueWaiters.Add(_003C_003Ec__DisplayClass9_.tcs);
					_003C_003E7__wrap1 = _003C_003Ec__DisplayClass9_.ct.Register(_003C_003Ec__DisplayClass9_._003CWaitReadyAsync_003Eb__0);
					goto IL_0090;
				}
				goto end_IL_000e;
				IL_0090:
				try
				{
					Awaiter<bool> val;
					if (num != 0)
					{
						UniTask task = _003C_003Ec__DisplayClass9_.tcs.Task;
						val = ((UniTask)(ref task)).SuppressCancellationThrow().GetAwaiter();
						if (!val.IsCompleted)
						{
							num = (_003C_003E1__state = 0);
							_003C_003Eu__1 = val;
							((AsyncUniTaskMethodBuilder)(ref _003C_003Et__builder)).AwaitUnsafeOnCompleted<Awaiter<bool>, _003CWaitReadyAsync_003Ed__9>(ref val, ref this);
							return;
						}
					}
					else
					{
						val = _003C_003Eu__1;
						_003C_003Eu__1 = default(Awaiter<bool>);
						num = (_003C_003E1__state = -1);
					}
					val.GetResult();
				}
				finally
				{
					if (num < 0)
					{
						((IDisposable)_003C_003E7__wrap1/*cast due to .constrained prefix*/).Dispose();
					}
				}
				_003C_003E7__wrap1 = default(CancellationTokenRegistration);
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

	private readonly List<UniTaskCompletionSource> queueWaiters = new List<UniTaskCompletionSource>();

	private readonly RingSolveBreakDelayService host;

	private bool disposed;

	public override int Priority => 1;

	public override bool GatesBreakReady => true;

	public BombBreakDelayHandler(RingSolveBreakDelayService host)
	{
		this.host = host;
		GameApp.Event.Listen<BombBreakTickEvent>((Action<BombBreakTickEvent>)OnBombBreakTick);
	}

	public override bool IsNeedToWait(RingAfterSolveEvent evt)
	{
		return evt?.IsFromBomb ?? false;
	}

	[AsyncStateMachine(typeof(_003CWaitReadyAsync_003Ed__9))]
	public override UniTask WaitReadyAsync(RingAfterSolveEvent evt, CancellationToken ct)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		_003CWaitReadyAsync_003Ed__9 _003CWaitReadyAsync_003Ed__ = default(_003CWaitReadyAsync_003Ed__9);
		_003CWaitReadyAsync_003Ed__._003C_003Et__builder = AsyncUniTaskMethodBuilder.Create();
		_003CWaitReadyAsync_003Ed__._003C_003E4__this = this;
		_003CWaitReadyAsync_003Ed__.evt = evt;
		_003CWaitReadyAsync_003Ed__.ct = ct;
		_003CWaitReadyAsync_003Ed__._003C_003E1__state = -1;
		((AsyncUniTaskMethodBuilder)(ref _003CWaitReadyAsync_003Ed__._003C_003Et__builder)).Start<_003CWaitReadyAsync_003Ed__9>(ref _003CWaitReadyAsync_003Ed__);
		return ((AsyncUniTaskMethodBuilder)(ref _003CWaitReadyAsync_003Ed__._003C_003Et__builder)).Task;
	}

	private void OnBombBreakTick(BombBreakTickEvent evt)
	{
		for (int i = 0; i < queueWaiters.Count; i++)
		{
			queueWaiters[i].TrySetResult();
		}
		queueWaiters.Clear();
	}

	public override void Dispose()
	{
		if (!disposed)
		{
			disposed = true;
			GameApp.Event.Unlisten<BombBreakTickEvent>((Action<BombBreakTickEvent>)OnBombBreakTick);
			for (int i = 0; i < queueWaiters.Count; i++)
			{
				queueWaiters[i].TrySetCanceled(default(CancellationToken));
			}
			queueWaiters.Clear();
		}
	}
}
