using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.CompilerServices;
using UniLog;

namespace UnlockRing.Presentation;

public sealed class ChainBreakPresentationNode : PresentationNodeBase
{
	[StructLayout(LayoutKind.Auto)]
	[CompilerGenerated]
	private struct _003CExecuteAsync_003Ed__4 : IAsyncStateMachine
	{
		public int _003C_003E1__state;

		public AsyncUniTaskMethodBuilder _003C_003Et__builder;

		public ChainBreakPresentationNode _003C_003E4__this;

		public CancellationToken ct;

		private Awaiter _003C_003Eu__1;

		private void MoveNext()
		{
			//IL_0097: Unknown result type (might be due to invalid IL or missing references)
			//IL_009c: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
			//IL_005f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0064: Unknown result type (might be due to invalid IL or missing references)
			//IL_0067: Unknown result type (might be due to invalid IL or missing references)
			//IL_006c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0080: Unknown result type (might be due to invalid IL or missing references)
			//IL_0081: Unknown result type (might be due to invalid IL or missing references)
			int num = _003C_003E1__state;
			ChainBreakPresentationNode chainBreakPresentationNode = _003C_003E4__this;
			try
			{
				if (num == 0 || (chainBreakPresentationNode.gameplayObject != null && chainBreakPresentationNode.context?.ChainService != null && !ct.IsCancellationRequested))
				{
					try
					{
						Awaiter val2;
						if (num != 0)
						{
							UniTask val = chainBreakPresentationNode.context.ChainService.PlayPendingChainBreakAsync(chainBreakPresentationNode.gameplayObject, ct);
							val2 = ((UniTask)(ref val)).GetAwaiter();
							if (!((Awaiter)(ref val2)).IsCompleted)
							{
								num = (_003C_003E1__state = 0);
								_003C_003Eu__1 = val2;
								((AsyncUniTaskMethodBuilder)(ref _003C_003Et__builder)).AwaitUnsafeOnCompleted<Awaiter, _003CExecuteAsync_003Ed__4>(ref val2, ref this);
								return;
							}
						}
						else
						{
							val2 = _003C_003Eu__1;
							_003C_003Eu__1 = default(Awaiter);
							num = (_003C_003E1__state = -1);
						}
						((Awaiter)(ref val2)).GetResult();
					}
					catch (OperationCanceledException)
					{
						throw;
					}
					catch (Exception ex2)
					{
						Log.LogError("[Presentation][ChainBreakNode] ", "锁链断环节点播放异常: {0}", new object[1] { ex2 });
					}
				}
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

	private const string TAG = "[Presentation][ChainBreakNode] ";

	private readonly IGameplayObject gameplayObject;

	private readonly PresentationContext context;

	public ChainBreakPresentationNode(IGameplayObject gameplayObject, PresentationContext context)
	{
		this.gameplayObject = gameplayObject;
		this.context = context;
	}

	[AsyncStateMachine(typeof(_003CExecuteAsync_003Ed__4))]
	public override UniTask ExecuteAsync(CancellationToken ct)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		_003CExecuteAsync_003Ed__4 _003CExecuteAsync_003Ed__ = default(_003CExecuteAsync_003Ed__4);
		_003CExecuteAsync_003Ed__._003C_003Et__builder = AsyncUniTaskMethodBuilder.Create();
		_003CExecuteAsync_003Ed__._003C_003E4__this = this;
		_003CExecuteAsync_003Ed__.ct = ct;
		_003CExecuteAsync_003Ed__._003C_003E1__state = -1;
		((AsyncUniTaskMethodBuilder)(ref _003CExecuteAsync_003Ed__._003C_003Et__builder)).Start<_003CExecuteAsync_003Ed__4>(ref _003CExecuteAsync_003Ed__);
		return ((AsyncUniTaskMethodBuilder)(ref _003CExecuteAsync_003Ed__._003C_003Et__builder)).Task;
	}
}
