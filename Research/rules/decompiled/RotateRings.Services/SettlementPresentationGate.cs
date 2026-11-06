using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.CompilerServices;
using RotateRings.UI.Game;
using XingYun.UI.Controller;

namespace RotateRings.Services;

public static class SettlementPresentationGate
{
	[StructLayout(LayoutKind.Auto)]
	[CompilerGenerated]
	private struct _003CWaitRankStarFlyAsync_003Ed__2 : IAsyncStateMachine
	{
		public int _003C_003E1__state;

		public AsyncUniTaskMethodBuilder<bool> _003C_003Et__builder;

		public CancellationToken ct;

		private Awaiter<bool> _003C_003Eu__1;

		private void MoveNext()
		{
			//IL_0076: Unknown result type (might be due to invalid IL or missing references)
			//IL_007b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0083: Unknown result type (might be due to invalid IL or missing references)
			//IL_003b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0040: Unknown result type (might be due to invalid IL or missing references)
			//IL_0044: Unknown result type (might be due to invalid IL or missing references)
			//IL_0049: Unknown result type (might be due to invalid IL or missing references)
			//IL_005e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0060: Unknown result type (might be due to invalid IL or missing references)
			int num = _003C_003E1__state;
			bool result;
			try
			{
				Awaiter<bool> val;
				if (num == 0)
				{
					val = _003C_003Eu__1;
					_003C_003Eu__1 = default(Awaiter<bool>);
					num = (_003C_003E1__state = -1);
					goto IL_0092;
				}
				IController val2 = default(IController);
				if (GameApp.UI.IsOpen("UIGame", ref val2) && val2 is UIGameController { RankStarService: { } rankStarService })
				{
					val = rankStarService.WaitUntilAllFlyFinished(ct).GetAwaiter();
					if (!val.IsCompleted)
					{
						num = (_003C_003E1__state = 0);
						_003C_003Eu__1 = val;
						_003C_003Et__builder.AwaitUnsafeOnCompleted<Awaiter<bool>, _003CWaitRankStarFlyAsync_003Ed__2>(ref val, ref this);
						return;
					}
					goto IL_0092;
				}
				result = false;
				goto end_IL_0007;
				IL_0092:
				result = val.GetResult();
				end_IL_0007:;
			}
			catch (Exception exception)
			{
				_003C_003E1__state = -2;
				_003C_003Et__builder.SetException(exception);
				return;
			}
			_003C_003E1__state = -2;
			_003C_003Et__builder.SetResult(result);
		}

		void IAsyncStateMachine.MoveNext()
		{
			//ILSpy generated this explicit interface implementation from .override directive in MoveNext
			this.MoveNext();
		}

		[DebuggerHidden]
		private void SetStateMachine(IAsyncStateMachine stateMachine)
		{
			_003C_003Et__builder.SetStateMachine(stateMachine);
		}

		void IAsyncStateMachine.SetStateMachine(IAsyncStateMachine stateMachine)
		{
			//ILSpy generated this explicit interface implementation from .override directive in SetStateMachine
			this.SetStateMachine(stateMachine);
		}
	}

	[StructLayout(LayoutKind.Auto)]
	[CompilerGenerated]
	private struct _003CWaitThen_003Ed__1 : IAsyncStateMachine
	{
		public int _003C_003E1__state;

		public AsyncUniTaskMethodBuilder _003C_003Et__builder;

		public CancellationToken ct;

		public Action onFinished;

		private Func<CancellationToken, UniTask<bool>>[] _003C_003E7__wrap1;

		private int _003C_003E7__wrap2;

		private Awaiter<bool> _003C_003Eu__1;

		private void MoveNext()
		{
			//IL_006f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0074: Unknown result type (might be due to invalid IL or missing references)
			//IL_007b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0034: Unknown result type (might be due to invalid IL or missing references)
			//IL_0039: Unknown result type (might be due to invalid IL or missing references)
			//IL_003c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0041: Unknown result type (might be due to invalid IL or missing references)
			//IL_0055: Unknown result type (might be due to invalid IL or missing references)
			//IL_0056: Unknown result type (might be due to invalid IL or missing references)
			int num = _003C_003E1__state;
			try
			{
				if (num != 0)
				{
					_003C_003E7__wrap1 = Waiters;
					_003C_003E7__wrap2 = 0;
					goto IL_00a3;
				}
				Awaiter<bool> val = _003C_003Eu__1;
				_003C_003Eu__1 = default(Awaiter<bool>);
				num = (_003C_003E1__state = -1);
				goto IL_008a;
				IL_008a:
				if (!val.GetResult())
				{
					_003C_003E7__wrap2++;
					goto IL_00a3;
				}
				goto end_IL_0007;
				IL_00a3:
				if (_003C_003E7__wrap2 < _003C_003E7__wrap1.Length)
				{
					val = _003C_003E7__wrap1[_003C_003E7__wrap2](ct).GetAwaiter();
					if (!val.IsCompleted)
					{
						num = (_003C_003E1__state = 0);
						_003C_003Eu__1 = val;
						((AsyncUniTaskMethodBuilder)(ref _003C_003Et__builder)).AwaitUnsafeOnCompleted<Awaiter<bool>, _003CWaitThen_003Ed__1>(ref val, ref this);
						return;
					}
					goto IL_008a;
				}
				_003C_003E7__wrap1 = null;
				if (!ct.IsCancellationRequested)
				{
					onFinished();
				}
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

	private static readonly Func<CancellationToken, UniTask<bool>>[] Waiters = new Func<CancellationToken, UniTask<bool>>[1] { WaitRankStarFlyAsync };

	[AsyncStateMachine(typeof(_003CWaitThen_003Ed__1))]
	public static UniTask WaitThen(Action onFinished, CancellationToken ct)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		_003CWaitThen_003Ed__1 _003CWaitThen_003Ed__ = default(_003CWaitThen_003Ed__1);
		_003CWaitThen_003Ed__._003C_003Et__builder = AsyncUniTaskMethodBuilder.Create();
		_003CWaitThen_003Ed__.onFinished = onFinished;
		_003CWaitThen_003Ed__.ct = ct;
		_003CWaitThen_003Ed__._003C_003E1__state = -1;
		((AsyncUniTaskMethodBuilder)(ref _003CWaitThen_003Ed__._003C_003Et__builder)).Start<_003CWaitThen_003Ed__1>(ref _003CWaitThen_003Ed__);
		return ((AsyncUniTaskMethodBuilder)(ref _003CWaitThen_003Ed__._003C_003Et__builder)).Task;
	}

	[AsyncStateMachine(typeof(_003CWaitRankStarFlyAsync_003Ed__2))]
	private static UniTask<bool> WaitRankStarFlyAsync(CancellationToken ct)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		_003CWaitRankStarFlyAsync_003Ed__2 _003CWaitRankStarFlyAsync_003Ed__ = default(_003CWaitRankStarFlyAsync_003Ed__2);
		_003CWaitRankStarFlyAsync_003Ed__._003C_003Et__builder = AsyncUniTaskMethodBuilder<bool>.Create();
		_003CWaitRankStarFlyAsync_003Ed__.ct = ct;
		_003CWaitRankStarFlyAsync_003Ed__._003C_003E1__state = -1;
		_003CWaitRankStarFlyAsync_003Ed__._003C_003Et__builder.Start<_003CWaitRankStarFlyAsync_003Ed__2>(ref _003CWaitRankStarFlyAsync_003Ed__);
		return _003CWaitRankStarFlyAsync_003Ed__._003C_003Et__builder.Task;
	}
}
