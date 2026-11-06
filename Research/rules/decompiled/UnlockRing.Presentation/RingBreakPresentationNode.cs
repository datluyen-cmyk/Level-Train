using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.ExceptionServices;
using System.Runtime.InteropServices;
using System.Threading;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.CompilerServices;
using UniLog;
using UnityEngine;

namespace UnlockRing.Presentation;

public class RingBreakPresentationNode : PresentationNodeBase
{
	[StructLayout(LayoutKind.Auto)]
	[CompilerGenerated]
	private struct _003CExecuteAsync_003Ed__13 : IAsyncStateMachine
	{
		public int _003C_003E1__state;

		public AsyncUniTaskMethodBuilder _003C_003Et__builder;

		public RingBreakPresentationNode _003C_003E4__this;

		public CancellationToken ct;

		private Awaiter _003C_003Eu__1;

		private void MoveNext()
		{
			//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
			//IL_0071: Unknown result type (might be due to invalid IL or missing references)
			//IL_0076: Unknown result type (might be due to invalid IL or missing references)
			//IL_0079: Unknown result type (might be due to invalid IL or missing references)
			//IL_007e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0092: Unknown result type (might be due to invalid IL or missing references)
			//IL_0093: Unknown result type (might be due to invalid IL or missing references)
			int num = _003C_003E1__state;
			RingBreakPresentationNode ringBreakPresentationNode = _003C_003E4__this;
			try
			{
				if (num == 0 || (!((Object)(object)ringBreakPresentationNode.breakResult?.PartGo == (Object)null) && ringBreakPresentationNode.context?.BreakEffectService != null && !ct.IsCancellationRequested))
				{
					try
					{
						Awaiter val2;
						if (num != 0)
						{
							UniTask val = ringBreakPresentationNode.context.BreakEffectService.PlayBreakFromResultAsync(ringBreakPresentationNode.breakResult, ct);
							val2 = ((UniTask)(ref val)).GetAwaiter();
							if (!((Awaiter)(ref val2)).IsCompleted)
							{
								num = (_003C_003E1__state = 0);
								_003C_003Eu__1 = val2;
								((AsyncUniTaskMethodBuilder)(ref _003C_003Et__builder)).AwaitUnsafeOnCompleted<Awaiter, _003CExecuteAsync_003Ed__13>(ref val2, ref this);
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
						Log.LogError("[Presentation][RingBreakNode] ", "碎环节点播放异常: {0}", new object[1] { ex2 });
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

	[StructLayout(LayoutKind.Auto)]
	[CompilerGenerated]
	private struct _003CExecuteWithChildrenAsync_003Ed__14 : IAsyncStateMachine
	{
		public int _003C_003E1__state;

		public AsyncUniTaskMethodBuilder _003C_003Et__builder;

		public RingBreakPresentationNode _003C_003E4__this;

		public CancellationToken ct;

		public PlayChildAsync playChildAsync;

		private UniTask _003CselfTask_003E5__2;

		private Awaiter _003C_003Eu__1;

		private object _003C_003E7__wrap2;

		private int _003C_003E7__wrap3;

		private void MoveNext()
		{
			//IL_0383: Unknown result type (might be due to invalid IL or missing references)
			//IL_007b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0080: Unknown result type (might be due to invalid IL or missing references)
			//IL_0088: Unknown result type (might be due to invalid IL or missing references)
			//IL_03a6: Unknown result type (might be due to invalid IL or missing references)
			//IL_003d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0042: Unknown result type (might be due to invalid IL or missing references)
			//IL_0046: Unknown result type (might be due to invalid IL or missing references)
			//IL_004b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0174: Unknown result type (might be due to invalid IL or missing references)
			//IL_0179: Unknown result type (might be due to invalid IL or missing references)
			//IL_0181: Unknown result type (might be due to invalid IL or missing references)
			//IL_013f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0144: Unknown result type (might be due to invalid IL or missing references)
			//IL_029b: Unknown result type (might be due to invalid IL or missing references)
			//IL_02a0: Unknown result type (might be due to invalid IL or missing references)
			//IL_02a8: Unknown result type (might be due to invalid IL or missing references)
			//IL_0241: Unknown result type (might be due to invalid IL or missing references)
			//IL_0246: Unknown result type (might be due to invalid IL or missing references)
			//IL_024d: Unknown result type (might be due to invalid IL or missing references)
			//IL_024f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0256: Unknown result type (might be due to invalid IL or missing references)
			//IL_0258: Unknown result type (might be due to invalid IL or missing references)
			//IL_025d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0262: Unknown result type (might be due to invalid IL or missing references)
			//IL_0266: Unknown result type (might be due to invalid IL or missing references)
			//IL_026b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0323: Unknown result type (might be due to invalid IL or missing references)
			//IL_0328: Unknown result type (might be due to invalid IL or missing references)
			//IL_0330: Unknown result type (might be due to invalid IL or missing references)
			//IL_02ee: Unknown result type (might be due to invalid IL or missing references)
			//IL_02f3: Unknown result type (might be due to invalid IL or missing references)
			//IL_0060: Unknown result type (might be due to invalid IL or missing references)
			//IL_0062: Unknown result type (might be due to invalid IL or missing references)
			//IL_0159: Unknown result type (might be due to invalid IL or missing references)
			//IL_015b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0280: Unknown result type (might be due to invalid IL or missing references)
			//IL_0282: Unknown result type (might be due to invalid IL or missing references)
			//IL_0308: Unknown result type (might be due to invalid IL or missing references)
			//IL_030a: Unknown result type (might be due to invalid IL or missing references)
			//IL_011d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0122: Unknown result type (might be due to invalid IL or missing references)
			//IL_01a1: Unknown result type (might be due to invalid IL or missing references)
			//IL_01a6: Unknown result type (might be due to invalid IL or missing references)
			//IL_0201: Unknown result type (might be due to invalid IL or missing references)
			//IL_0206: Unknown result type (might be due to invalid IL or missing references)
			//IL_0225: Unknown result type (might be due to invalid IL or missing references)
			//IL_022a: Unknown result type (might be due to invalid IL or missing references)
			//IL_01de: Unknown result type (might be due to invalid IL or missing references)
			//IL_01e3: Unknown result type (might be due to invalid IL or missing references)
			//IL_01fa: Unknown result type (might be due to invalid IL or missing references)
			//IL_01ff: Unknown result type (might be due to invalid IL or missing references)
			int num = _003C_003E1__state;
			RingBreakPresentationNode ringBreakPresentationNode = _003C_003E4__this;
			try
			{
				UniTask val2;
				Awaiter val;
				UniTask val3 = default(UniTask);
				UniTask val4 = default(UniTask);
				switch (num)
				{
				default:
				{
					List<IPresentationNode> children = ringBreakPresentationNode.Children;
					if (children == null || children.Count == 0)
					{
						val2 = ringBreakPresentationNode.ExecuteAsync(ct);
						val = ((UniTask)(ref val2)).GetAwaiter();
						if (!((Awaiter)(ref val)).IsCompleted)
						{
							num = (_003C_003E1__state = 0);
							_003C_003Eu__1 = val;
							((AsyncUniTaskMethodBuilder)(ref _003C_003Et__builder)).AwaitUnsafeOnCompleted<Awaiter, _003CExecuteWithChildrenAsync_003Ed__14>(ref val, ref this);
							return;
						}
						goto IL_0097;
					}
					List<IPresentationNode> list = null;
					List<IPresentationNode> list2 = null;
					for (int i = 0; i < children.Count; i++)
					{
						IPresentationNode presentationNode = children[i];
						if (presentationNode is RingBreakPresentationNode ringBreakPresentationNode2 && ringBreakPresentationNode2.GameplayObject is FixedObject)
						{
							if (list == null)
							{
								list = new List<IPresentationNode>(children.Count);
							}
							list.Add(presentationNode);
						}
						else
						{
							if (list2 == null)
							{
								list2 = new List<IPresentationNode>(children.Count);
							}
							list2.Add(presentationNode);
						}
					}
					_003CselfTask_003E5__2 = ringBreakPresentationNode.ExecuteAsync(ct);
					if (ct.IsCancellationRequested)
					{
						goto case 1;
					}
					val3 = UniTask.CompletedTask;
					if (list != null && list.Count > 0)
					{
						UniTask[] array = (UniTask[])(object)new UniTask[list.Count];
						for (int j = 0; j < list.Count; j++)
						{
							array[j] = playChildAsync(list[j], ct);
						}
						val3 = UniTask.WhenAll(array);
					}
					val4 = UniTask.CompletedTask;
					if (list2 != null && list2.Count > 0)
					{
						val4 = ringBreakPresentationNode.PlayDelayedChildrenAsync(list2, playChildAsync, ct);
					}
					_003C_003E7__wrap3 = 0;
					goto case 2;
				}
				case 0:
					val = _003C_003Eu__1;
					_003C_003Eu__1 = default(Awaiter);
					num = (_003C_003E1__state = -1);
					goto IL_0097;
				case 1:
					try
					{
						if (num != 1)
						{
							val = ((UniTask)(ref _003CselfTask_003E5__2)).GetAwaiter();
							if (!((Awaiter)(ref val)).IsCompleted)
							{
								num = (_003C_003E1__state = 1);
								_003C_003Eu__1 = val;
								((AsyncUniTaskMethodBuilder)(ref _003C_003Et__builder)).AwaitUnsafeOnCompleted<Awaiter, _003CExecuteWithChildrenAsync_003Ed__14>(ref val, ref this);
								return;
							}
						}
						else
						{
							val = _003C_003Eu__1;
							_003C_003Eu__1 = default(Awaiter);
							num = (_003C_003E1__state = -1);
						}
						((Awaiter)(ref val)).GetResult();
					}
					catch (OperationCanceledException)
					{
					}
					goto end_IL_000e;
				case 2:
				{
					try
					{
						if (num != 2)
						{
							val2 = UniTask.WhenAll((UniTask[])(object)new UniTask[3] { _003CselfTask_003E5__2, val3, val4 });
							val = ((UniTask)(ref val2)).GetAwaiter();
							if (!((Awaiter)(ref val)).IsCompleted)
							{
								num = (_003C_003E1__state = 2);
								_003C_003Eu__1 = val;
								((AsyncUniTaskMethodBuilder)(ref _003C_003Et__builder)).AwaitUnsafeOnCompleted<Awaiter, _003CExecuteWithChildrenAsync_003Ed__14>(ref val, ref this);
								return;
							}
						}
						else
						{
							val = _003C_003Eu__1;
							_003C_003Eu__1 = default(Awaiter);
							num = (_003C_003E1__state = -1);
						}
						((Awaiter)(ref val)).GetResult();
					}
					catch (OperationCanceledException ex)
					{
						_003C_003E7__wrap2 = ex;
						_003C_003E7__wrap3 = 1;
					}
					int num2 = _003C_003E7__wrap3;
					if (num2 != 1)
					{
						break;
					}
					goto case 3;
				}
				case 3:
					{
						try
						{
							if (num != 3)
							{
								val = ((UniTask)(ref _003CselfTask_003E5__2)).GetAwaiter();
								if (!((Awaiter)(ref val)).IsCompleted)
								{
									num = (_003C_003E1__state = 3);
									_003C_003Eu__1 = val;
									((AsyncUniTaskMethodBuilder)(ref _003C_003Et__builder)).AwaitUnsafeOnCompleted<Awaiter, _003CExecuteWithChildrenAsync_003Ed__14>(ref val, ref this);
									return;
								}
							}
							else
							{
								val = _003C_003Eu__1;
								_003C_003Eu__1 = default(Awaiter);
								num = (_003C_003E1__state = -1);
							}
							((Awaiter)(ref val)).GetResult();
						}
						catch (OperationCanceledException)
						{
						}
						ExceptionDispatchInfo.Capture((_003C_003E7__wrap2 as Exception) ?? throw _003C_003E7__wrap2).Throw();
						break;
					}
					IL_0097:
					((Awaiter)(ref val)).GetResult();
					goto end_IL_000e;
				}
				_003C_003E7__wrap2 = null;
				end_IL_000e:;
			}
			catch (Exception exception)
			{
				_003C_003E1__state = -2;
				_003CselfTask_003E5__2 = default(UniTask);
				((AsyncUniTaskMethodBuilder)(ref _003C_003Et__builder)).SetException(exception);
				return;
			}
			_003C_003E1__state = -2;
			_003CselfTask_003E5__2 = default(UniTask);
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
	private struct _003CPlayDelayedChildrenAsync_003Ed__15 : IAsyncStateMachine
	{
		public int _003C_003E1__state;

		public AsyncUniTaskMethodBuilder _003C_003Et__builder;

		public CancellationToken ct;

		public List<IPresentationNode> delayedChildren;

		public PlayChildAsync playChildAsync;

		private Awaiter<bool> _003C_003Eu__1;

		private Awaiter _003C_003Eu__2;

		private void MoveNext()
		{
			//IL_006b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0070: Unknown result type (might be due to invalid IL or missing references)
			//IL_0077: Unknown result type (might be due to invalid IL or missing references)
			//IL_0120: Unknown result type (might be due to invalid IL or missing references)
			//IL_0125: Unknown result type (might be due to invalid IL or missing references)
			//IL_012d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0027: Unknown result type (might be due to invalid IL or missing references)
			//IL_002c: Unknown result type (might be due to invalid IL or missing references)
			//IL_002f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0034: Unknown result type (might be due to invalid IL or missing references)
			//IL_0038: Unknown result type (might be due to invalid IL or missing references)
			//IL_003d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0051: Unknown result type (might be due to invalid IL or missing references)
			//IL_0052: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
			//IL_00cb: Unknown result type (might be due to invalid IL or missing references)
			//IL_00e6: Unknown result type (might be due to invalid IL or missing references)
			//IL_00eb: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ee: Unknown result type (might be due to invalid IL or missing references)
			//IL_00f3: Unknown result type (might be due to invalid IL or missing references)
			//IL_0108: Unknown result type (might be due to invalid IL or missing references)
			//IL_010a: Unknown result type (might be due to invalid IL or missing references)
			int num = _003C_003E1__state;
			try
			{
				Awaiter val;
				UniTask val2;
				Awaiter<bool> val3;
				if (num != 0)
				{
					if (num == 1)
					{
						val = _003C_003Eu__2;
						_003C_003Eu__2 = default(Awaiter);
						num = (_003C_003E1__state = -1);
						goto IL_013c;
					}
					val2 = UniTask.Delay(TimeSpan.FromSeconds(0.30000001192092896), false, (PlayerLoopTiming)8, ct);
					val3 = ((UniTask)(ref val2)).SuppressCancellationThrow().GetAwaiter();
					if (!val3.IsCompleted)
					{
						num = (_003C_003E1__state = 0);
						_003C_003Eu__1 = val3;
						((AsyncUniTaskMethodBuilder)(ref _003C_003Et__builder)).AwaitUnsafeOnCompleted<Awaiter<bool>, _003CPlayDelayedChildrenAsync_003Ed__15>(ref val3, ref this);
						return;
					}
				}
				else
				{
					val3 = _003C_003Eu__1;
					_003C_003Eu__1 = default(Awaiter<bool>);
					num = (_003C_003E1__state = -1);
				}
				if (!val3.GetResult())
				{
					UniTask[] array = (UniTask[])(object)new UniTask[delayedChildren.Count];
					for (int i = 0; i < delayedChildren.Count; i++)
					{
						array[i] = playChildAsync(delayedChildren[i], ct);
					}
					val2 = UniTask.WhenAll(array);
					val = ((UniTask)(ref val2)).GetAwaiter();
					if (!((Awaiter)(ref val)).IsCompleted)
					{
						num = (_003C_003E1__state = 1);
						_003C_003Eu__2 = val;
						((AsyncUniTaskMethodBuilder)(ref _003C_003Et__builder)).AwaitUnsafeOnCompleted<Awaiter, _003CPlayDelayedChildrenAsync_003Ed__15>(ref val, ref this);
						return;
					}
					goto IL_013c;
				}
				goto end_IL_0007;
				IL_013c:
				((Awaiter)(ref val)).GetResult();
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

	private const string TAG = "[Presentation][RingBreakNode] ";

	private const float DelayedChildStartSeconds = 0.3f;

	private readonly RingBreakResult breakResult;

	private readonly PresentationContext context;

	public GameObject Source { get; }

	public GameObject CauseSource { get; }

	public IGameplayObject GameplayObject => breakResult?.GameplayObject;

	public RingBreakPresentationNode(RingBreakResult breakResult, PresentationContext context, GameObject source = null, GameObject causeSource = null)
	{
		this.breakResult = breakResult;
		this.context = context;
		Source = source;
		CauseSource = causeSource;
	}

	[AsyncStateMachine(typeof(_003CExecuteAsync_003Ed__13))]
	public override UniTask ExecuteAsync(CancellationToken ct)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		_003CExecuteAsync_003Ed__13 _003CExecuteAsync_003Ed__ = default(_003CExecuteAsync_003Ed__13);
		_003CExecuteAsync_003Ed__._003C_003Et__builder = AsyncUniTaskMethodBuilder.Create();
		_003CExecuteAsync_003Ed__._003C_003E4__this = this;
		_003CExecuteAsync_003Ed__.ct = ct;
		_003CExecuteAsync_003Ed__._003C_003E1__state = -1;
		((AsyncUniTaskMethodBuilder)(ref _003CExecuteAsync_003Ed__._003C_003Et__builder)).Start<_003CExecuteAsync_003Ed__13>(ref _003CExecuteAsync_003Ed__);
		return ((AsyncUniTaskMethodBuilder)(ref _003CExecuteAsync_003Ed__._003C_003Et__builder)).Task;
	}

	[AsyncStateMachine(typeof(_003CExecuteWithChildrenAsync_003Ed__14))]
	public override UniTask ExecuteWithChildrenAsync(CancellationToken ct, PlayChildAsync playChildAsync)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		_003CExecuteWithChildrenAsync_003Ed__14 _003CExecuteWithChildrenAsync_003Ed__ = default(_003CExecuteWithChildrenAsync_003Ed__14);
		_003CExecuteWithChildrenAsync_003Ed__._003C_003Et__builder = AsyncUniTaskMethodBuilder.Create();
		_003CExecuteWithChildrenAsync_003Ed__._003C_003E4__this = this;
		_003CExecuteWithChildrenAsync_003Ed__.ct = ct;
		_003CExecuteWithChildrenAsync_003Ed__.playChildAsync = playChildAsync;
		_003CExecuteWithChildrenAsync_003Ed__._003C_003E1__state = -1;
		((AsyncUniTaskMethodBuilder)(ref _003CExecuteWithChildrenAsync_003Ed__._003C_003Et__builder)).Start<_003CExecuteWithChildrenAsync_003Ed__14>(ref _003CExecuteWithChildrenAsync_003Ed__);
		return ((AsyncUniTaskMethodBuilder)(ref _003CExecuteWithChildrenAsync_003Ed__._003C_003Et__builder)).Task;
	}

	[AsyncStateMachine(typeof(_003CPlayDelayedChildrenAsync_003Ed__15))]
	private UniTask PlayDelayedChildrenAsync(List<IPresentationNode> delayedChildren, PlayChildAsync playChildAsync, CancellationToken ct)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		_003CPlayDelayedChildrenAsync_003Ed__15 _003CPlayDelayedChildrenAsync_003Ed__ = default(_003CPlayDelayedChildrenAsync_003Ed__15);
		_003CPlayDelayedChildrenAsync_003Ed__._003C_003Et__builder = AsyncUniTaskMethodBuilder.Create();
		_003CPlayDelayedChildrenAsync_003Ed__.delayedChildren = delayedChildren;
		_003CPlayDelayedChildrenAsync_003Ed__.playChildAsync = playChildAsync;
		_003CPlayDelayedChildrenAsync_003Ed__.ct = ct;
		_003CPlayDelayedChildrenAsync_003Ed__._003C_003E1__state = -1;
		((AsyncUniTaskMethodBuilder)(ref _003CPlayDelayedChildrenAsync_003Ed__._003C_003Et__builder)).Start<_003CPlayDelayedChildrenAsync_003Ed__15>(ref _003CPlayDelayedChildrenAsync_003Ed__);
		return ((AsyncUniTaskMethodBuilder)(ref _003CPlayDelayedChildrenAsync_003Ed__._003C_003Et__builder)).Task;
	}
}
