using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.CompilerServices;
using RotateRings.GamePlay.Events;
using UnityEngine;

namespace RotateRings.GamePlay.Services;

public sealed class ChainBreakDelayHandler : SolveDelayHandlerBase
{
	[StructLayout(LayoutKind.Auto)]
	[CompilerGenerated]
	private struct _003CWaitReadyAsync_003Ed__4 : IAsyncStateMachine
	{
		public int _003C_003E1__state;

		public AsyncUniTaskMethodBuilder _003C_003Et__builder;

		public RingAfterSolveEvent evt;

		public CancellationToken ct;

		private Awaiter<bool> _003C_003Eu__1;

		private void MoveNext()
		{
			//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c5: Unknown result type (might be due to invalid IL or missing references)
			//IL_00cc: Unknown result type (might be due to invalid IL or missing references)
			//IL_007e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0083: Unknown result type (might be due to invalid IL or missing references)
			//IL_0087: Unknown result type (might be due to invalid IL or missing references)
			//IL_008c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0090: Unknown result type (might be due to invalid IL or missing references)
			//IL_0095: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
			//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
			int num = _003C_003E1__state;
			try
			{
				Awaiter<bool> val;
				if (num == 0)
				{
					val = _003C_003Eu__1;
					_003C_003Eu__1 = default(Awaiter<bool>);
					num = (_003C_003E1__state = -1);
					goto IL_00db;
				}
				if (evt != null && evt.HasChainBreak)
				{
					float num2 = 0f;
					if (evt.BreakRingOrder > 0)
					{
						float num3 = Random.Range(RingLevelConfig.Instance.chainRingBreakIntervalSecondsMin, RingLevelConfig.Instance.chainRingBreakIntervalSecondsMax);
						num2 = (float)evt.BreakRingOrder * num3;
					}
					UniTask val2 = UniTask.Delay(TimeSpan.FromSeconds(RingLevelConfig.Instance.chainBreakStartDelaySeconds + num2), false, (PlayerLoopTiming)8, ct);
					val = ((UniTask)(ref val2)).SuppressCancellationThrow().GetAwaiter();
					if (!val.IsCompleted)
					{
						num = (_003C_003E1__state = 0);
						_003C_003Eu__1 = val;
						((AsyncUniTaskMethodBuilder)(ref _003C_003Et__builder)).AwaitUnsafeOnCompleted<Awaiter<bool>, _003CWaitReadyAsync_003Ed__4>(ref val, ref this);
						return;
					}
					goto IL_00db;
				}
				goto end_IL_0007;
				IL_00db:
				val.GetResult();
				end_IL_0007:;
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

	public override int Priority => 3;

	public ChainBreakDelayHandler(RingSolveBreakDelayService host)
	{
	}

	public override bool IsNeedToWait(RingAfterSolveEvent evt)
	{
		return evt?.HasChainBreak ?? false;
	}

	[AsyncStateMachine(typeof(_003CWaitReadyAsync_003Ed__4))]
	public override UniTask WaitReadyAsync(RingAfterSolveEvent evt, CancellationToken ct)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		_003CWaitReadyAsync_003Ed__4 _003CWaitReadyAsync_003Ed__ = default(_003CWaitReadyAsync_003Ed__4);
		_003CWaitReadyAsync_003Ed__._003C_003Et__builder = AsyncUniTaskMethodBuilder.Create();
		_003CWaitReadyAsync_003Ed__.evt = evt;
		_003CWaitReadyAsync_003Ed__.ct = ct;
		_003CWaitReadyAsync_003Ed__._003C_003E1__state = -1;
		((AsyncUniTaskMethodBuilder)(ref _003CWaitReadyAsync_003Ed__._003C_003Et__builder)).Start<_003CWaitReadyAsync_003Ed__4>(ref _003CWaitReadyAsync_003Ed__);
		return ((AsyncUniTaskMethodBuilder)(ref _003CWaitReadyAsync_003Ed__._003C_003Et__builder)).Task;
	}
}
