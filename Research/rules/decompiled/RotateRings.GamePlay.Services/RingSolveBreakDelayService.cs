using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.CompilerServices;
using RotateRings.GamePlay.Events;

namespace RotateRings.GamePlay.Services;

public sealed class RingSolveBreakDelayService : GameplayServiceBase<GameplayManager>
{
	[StructLayout(LayoutKind.Auto)]
	[CompilerGenerated]
	private struct _003CWaitBreakHandlersAsync_003Ed__9 : IAsyncStateMachine
	{
		public int _003C_003E1__state;

		public AsyncUniTaskMethodBuilder _003C_003Et__builder;

		public RingAfterSolveEvent evt;

		public RingSolveBreakDelayService _003C_003E4__this;

		public bool gatesBreakReady;

		private CancellationToken _003CdestroyToken_003E5__2;

		private int _003Ci_003E5__3;

		private Awaiter<bool> _003C_003Eu__1;

		private void MoveNext()
		{
			//IL_00cf: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d4: Unknown result type (might be due to invalid IL or missing references)
			//IL_00db: Unknown result type (might be due to invalid IL or missing references)
			//IL_008a: Unknown result type (might be due to invalid IL or missing references)
			//IL_008f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0093: Unknown result type (might be due to invalid IL or missing references)
			//IL_0098: Unknown result type (might be due to invalid IL or missing references)
			//IL_009c: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
			int num = _003C_003E1__state;
			RingSolveBreakDelayService ringSolveBreakDelayService = _003C_003E4__this;
			try
			{
				Awaiter<bool> val;
				if (num == 0)
				{
					val = _003C_003Eu__1;
					_003C_003Eu__1 = default(Awaiter<bool>);
					num = (_003C_003E1__state = -1);
					goto IL_00ea;
				}
				if (evt != null && ringSolveBreakDelayService.delayHandlers.Count != 0)
				{
					_003CdestroyToken_003E5__2 = ringSolveBreakDelayService.DestroyToken;
					_003Ci_003E5__3 = 0;
					goto IL_0107;
				}
				goto end_IL_000e;
				IL_0107:
				if (_003Ci_003E5__3 < ringSolveBreakDelayService.delayHandlers.Count)
				{
					ISolveDelayHandler solveDelayHandler = ringSolveBreakDelayService.delayHandlers[_003Ci_003E5__3];
					if (solveDelayHandler != null && solveDelayHandler.GatesBreakReady == gatesBreakReady && solveDelayHandler.IsNeedToWait(evt))
					{
						UniTask val2 = solveDelayHandler.WaitReadyAsync(evt, _003CdestroyToken_003E5__2);
						val = ((UniTask)(ref val2)).SuppressCancellationThrow().GetAwaiter();
						if (!val.IsCompleted)
						{
							num = (_003C_003E1__state = 0);
							_003C_003Eu__1 = val;
							((AsyncUniTaskMethodBuilder)(ref _003C_003Et__builder)).AwaitUnsafeOnCompleted<Awaiter<bool>, _003CWaitBreakHandlersAsync_003Ed__9>(ref val, ref this);
							return;
						}
						goto IL_00ea;
					}
					goto IL_00f5;
				}
				goto end_IL_000e;
				IL_00f5:
				_003Ci_003E5__3++;
				goto IL_0107;
				IL_00ea:
				if (!val.GetResult())
				{
					goto IL_00f5;
				}
				end_IL_000e:;
			}
			catch (Exception exception)
			{
				_003C_003E1__state = -2;
				_003CdestroyToken_003E5__2 = default(CancellationToken);
				((AsyncUniTaskMethodBuilder)(ref _003C_003Et__builder)).SetException(exception);
				return;
			}
			_003C_003E1__state = -2;
			_003CdestroyToken_003E5__2 = default(CancellationToken);
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

	[StructLayout(LayoutKind.Auto)]
	[CompilerGenerated]
	private struct _003CWaitSolveGateAsync_003Ed__6 : IAsyncStateMachine
	{
		public int _003C_003E1__state;

		public AsyncUniTaskMethodBuilder _003C_003Et__builder;

		public IGameplayObject obj;

		public RingSolveBreakDelayService _003C_003E4__this;

		private CancellationToken _003CdestroyToken_003E5__2;

		private int _003Ci_003E5__3;

		private Awaiter<bool> _003C_003Eu__1;

		private void MoveNext()
		{
			//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c4: Unknown result type (might be due to invalid IL or missing references)
			//IL_00cb: Unknown result type (might be due to invalid IL or missing references)
			//IL_007a: Unknown result type (might be due to invalid IL or missing references)
			//IL_007f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0083: Unknown result type (might be due to invalid IL or missing references)
			//IL_0088: Unknown result type (might be due to invalid IL or missing references)
			//IL_008c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0091: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
			int num = _003C_003E1__state;
			RingSolveBreakDelayService ringSolveBreakDelayService = _003C_003E4__this;
			try
			{
				Awaiter<bool> val;
				if (num == 0)
				{
					val = _003C_003Eu__1;
					_003C_003Eu__1 = default(Awaiter<bool>);
					num = (_003C_003E1__state = -1);
					goto IL_00da;
				}
				if (obj != null && ringSolveBreakDelayService.delayHandlers.Count != 0)
				{
					_003CdestroyToken_003E5__2 = ringSolveBreakDelayService.DestroyToken;
					_003Ci_003E5__3 = 0;
					goto IL_00f7;
				}
				goto end_IL_000e;
				IL_00f7:
				if (_003Ci_003E5__3 < ringSolveBreakDelayService.delayHandlers.Count)
				{
					ISolveDelayHandler solveDelayHandler = ringSolveBreakDelayService.delayHandlers[_003Ci_003E5__3];
					if (solveDelayHandler != null && solveDelayHandler.IsNeedToDeferSolve(obj, null))
					{
						UniTask val2 = solveDelayHandler.WaitSolveGateAsync(obj, _003CdestroyToken_003E5__2);
						val = ((UniTask)(ref val2)).SuppressCancellationThrow().GetAwaiter();
						if (!val.IsCompleted)
						{
							num = (_003C_003E1__state = 0);
							_003C_003Eu__1 = val;
							((AsyncUniTaskMethodBuilder)(ref _003C_003Et__builder)).AwaitUnsafeOnCompleted<Awaiter<bool>, _003CWaitSolveGateAsync_003Ed__6>(ref val, ref this);
							return;
						}
						goto IL_00da;
					}
					goto IL_00e5;
				}
				goto end_IL_000e;
				IL_00da:
				if (!val.GetResult())
				{
					goto IL_00e5;
				}
				goto end_IL_000e;
				IL_00e5:
				_003Ci_003E5__3++;
				goto IL_00f7;
				end_IL_000e:;
			}
			catch (Exception exception)
			{
				_003C_003E1__state = -2;
				_003CdestroyToken_003E5__2 = default(CancellationToken);
				((AsyncUniTaskMethodBuilder)(ref _003C_003Et__builder)).SetException(exception);
				return;
			}
			_003C_003E1__state = -2;
			_003CdestroyToken_003E5__2 = default(CancellationToken);
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

	private const string TAG = "[GamePlay][RingSolveBreakDelay] ";

	private readonly List<ISolveDelayHandler> delayHandlers = new List<ISolveDelayHandler>();

	public RingSolveBreakDelayService(GameplayManager host)
		: base(host)
	{
		RegisterHandler(new ColorLockDelayHandler(this));
		RegisterHandler(new ChainBreakDelayHandler(this));
		RegisterHandler(new BombBreakDelayHandler(this));
		delayHandlers.Sort((ISolveDelayHandler a, ISolveDelayHandler b) => a.Priority.CompareTo(b.Priority));
	}

	public T GetHandler<T>() where T : class, ISolveDelayHandler
	{
		for (int i = 0; i < delayHandlers.Count; i++)
		{
			if (delayHandlers[i] is T result)
			{
				return result;
			}
		}
		return null;
	}

	private void RegisterHandler(ISolveDelayHandler handler)
	{
		if (handler != null && !delayHandlers.Contains(handler))
		{
			delayHandlers.Add(handler);
		}
	}

	public bool IsNeedToDeferSolve(IGameplayObject obj, BombRelative sourceBomb)
	{
		if (obj == null || delayHandlers.Count == 0)
		{
			return false;
		}
		for (int i = 0; i < delayHandlers.Count; i++)
		{
			ISolveDelayHandler solveDelayHandler = delayHandlers[i];
			if (solveDelayHandler != null && solveDelayHandler.IsNeedToDeferSolve(obj, sourceBomb))
			{
				return true;
			}
		}
		return false;
	}

	[AsyncStateMachine(typeof(_003CWaitSolveGateAsync_003Ed__6))]
	public UniTask WaitSolveGateAsync(IGameplayObject obj)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		_003CWaitSolveGateAsync_003Ed__6 _003CWaitSolveGateAsync_003Ed__ = default(_003CWaitSolveGateAsync_003Ed__6);
		_003CWaitSolveGateAsync_003Ed__._003C_003Et__builder = AsyncUniTaskMethodBuilder.Create();
		_003CWaitSolveGateAsync_003Ed__._003C_003E4__this = this;
		_003CWaitSolveGateAsync_003Ed__.obj = obj;
		_003CWaitSolveGateAsync_003Ed__._003C_003E1__state = -1;
		((AsyncUniTaskMethodBuilder)(ref _003CWaitSolveGateAsync_003Ed__._003C_003Et__builder)).Start<_003CWaitSolveGateAsync_003Ed__6>(ref _003CWaitSolveGateAsync_003Ed__);
		return ((AsyncUniTaskMethodBuilder)(ref _003CWaitSolveGateAsync_003Ed__._003C_003Et__builder)).Task;
	}

	public UniTask WaitBreakReadyGateAsync(RingAfterSolveEvent evt)
	{
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		return WaitBreakHandlersAsync(evt, gatesBreakReady: true);
	}

	public UniTask WaitRemoveDelayAsync(RingAfterSolveEvent evt)
	{
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		return WaitBreakHandlersAsync(evt, gatesBreakReady: false);
	}

	[AsyncStateMachine(typeof(_003CWaitBreakHandlersAsync_003Ed__9))]
	private UniTask WaitBreakHandlersAsync(RingAfterSolveEvent evt, bool gatesBreakReady)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		_003CWaitBreakHandlersAsync_003Ed__9 _003CWaitBreakHandlersAsync_003Ed__ = default(_003CWaitBreakHandlersAsync_003Ed__9);
		_003CWaitBreakHandlersAsync_003Ed__._003C_003Et__builder = AsyncUniTaskMethodBuilder.Create();
		_003CWaitBreakHandlersAsync_003Ed__._003C_003E4__this = this;
		_003CWaitBreakHandlersAsync_003Ed__.evt = evt;
		_003CWaitBreakHandlersAsync_003Ed__.gatesBreakReady = gatesBreakReady;
		_003CWaitBreakHandlersAsync_003Ed__._003C_003E1__state = -1;
		((AsyncUniTaskMethodBuilder)(ref _003CWaitBreakHandlersAsync_003Ed__._003C_003Et__builder)).Start<_003CWaitBreakHandlersAsync_003Ed__9>(ref _003CWaitBreakHandlersAsync_003Ed__);
		return ((AsyncUniTaskMethodBuilder)(ref _003CWaitBreakHandlersAsync_003Ed__._003C_003Et__builder)).Task;
	}

	public override void Dispose()
	{
		if (!base.IsDisposed)
		{
			for (int i = 0; i < delayHandlers.Count; i++)
			{
				delayHandlers[i]?.Dispose();
			}
			delayHandlers.Clear();
			base.Dispose();
		}
	}
}
