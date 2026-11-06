using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.CompilerServices;
using UniLog;
using UnityEngine;

namespace UnlockRing.Presentation;

public class BombPresentationNode : PresentationNodeBase
{
	[StructLayout(LayoutKind.Auto)]
	[CompilerGenerated]
	private struct _003CExecuteAsync_003Ed__7 : IAsyncStateMachine
	{
		public int _003C_003E1__state;

		public AsyncUniTaskMethodBuilder _003C_003Et__builder;

		public BombPresentationNode _003C_003E4__this;

		public CancellationToken ct;

		private Awaiter<bool> _003C_003Eu__1;

		private void MoveNext()
		{
			//IL_00e2: Unknown result type (might be due to invalid IL or missing references)
			//IL_00e7: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ee: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
			//IL_00cb: Unknown result type (might be due to invalid IL or missing references)
			//IL_00cc: Unknown result type (might be due to invalid IL or missing references)
			int num = _003C_003E1__state;
			BombPresentationNode bombPresentationNode = _003C_003E4__this;
			try
			{
				if (num == 0 || (!((Object)(object)bombPresentationNode.bombResult?.Bomb == (Object)null) && bombPresentationNode.context?.BombService != null && !ct.IsCancellationRequested))
				{
					try
					{
						Awaiter<bool> val;
						if (num == 0)
						{
							val = _003C_003Eu__1;
							_003C_003Eu__1 = default(Awaiter<bool>);
							num = (_003C_003E1__state = -1);
							goto IL_00fd;
						}
						bombPresentationNode.context.BombService.PlayBoom(bombPresentationNode.bombResult.Bomb);
						float targetBreakDelay = bombPresentationNode.context.BombService.TargetBreakDelay;
						if (targetBreakDelay > 0f)
						{
							UniTask val2 = UniTask.Delay(TimeSpan.FromSeconds(targetBreakDelay), false, (PlayerLoopTiming)8, ct);
							val = ((UniTask)(ref val2)).SuppressCancellationThrow().GetAwaiter();
							if (!val.IsCompleted)
							{
								num = (_003C_003E1__state = 0);
								_003C_003Eu__1 = val;
								((AsyncUniTaskMethodBuilder)(ref _003C_003Et__builder)).AwaitUnsafeOnCompleted<Awaiter<bool>, _003CExecuteAsync_003Ed__7>(ref val, ref this);
								return;
							}
							goto IL_00fd;
						}
						goto end_IL_0057;
						IL_00fd:
						if (!val.GetResult())
						{
						}
						end_IL_0057:;
					}
					catch (OperationCanceledException)
					{
						throw;
					}
					catch (Exception ex2)
					{
						Log.LogError("[Presentation][BombNode] ", "炸弹节点播放异常: {0}", new object[1] { ex2 });
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

	private const string TAG = "[Presentation][BombNode] ";

	private readonly BombBoomResult bombResult;

	private readonly PresentationContext context;

	public GameObject Source { get; }

	public BombPresentationNode(BombBoomResult bombResult, PresentationContext context, GameObject source = null)
	{
		this.bombResult = bombResult;
		this.context = context;
		Source = source;
	}

	[AsyncStateMachine(typeof(_003CExecuteAsync_003Ed__7))]
	public override UniTask ExecuteAsync(CancellationToken ct)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		_003CExecuteAsync_003Ed__7 _003CExecuteAsync_003Ed__ = default(_003CExecuteAsync_003Ed__7);
		_003CExecuteAsync_003Ed__._003C_003Et__builder = AsyncUniTaskMethodBuilder.Create();
		_003CExecuteAsync_003Ed__._003C_003E4__this = this;
		_003CExecuteAsync_003Ed__.ct = ct;
		_003CExecuteAsync_003Ed__._003C_003E1__state = -1;
		((AsyncUniTaskMethodBuilder)(ref _003CExecuteAsync_003Ed__._003C_003Et__builder)).Start<_003CExecuteAsync_003Ed__7>(ref _003CExecuteAsync_003Ed__);
		return ((AsyncUniTaskMethodBuilder)(ref _003CExecuteAsync_003Ed__._003C_003Et__builder)).Task;
	}
}
