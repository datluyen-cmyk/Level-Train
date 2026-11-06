using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.CompilerServices;

namespace UnlockRing.Presentation;

public abstract class PresentationNodeBase : IPresentationNode
{
	public delegate UniTask PlayChildAsync(IPresentationNode child, CancellationToken ct);

	[StructLayout(LayoutKind.Auto)]
	[CompilerGenerated]
	private struct _003CExecuteWithChildrenAsync_003Ed__5 : IAsyncStateMachine
	{
		public int _003C_003E1__state;

		public AsyncUniTaskMethodBuilder _003C_003Et__builder;

		public PresentationNodeBase _003C_003E4__this;

		public CancellationToken ct;

		public PlayChildAsync playChildAsync;

		private Awaiter _003C_003Eu__1;

		private void MoveNext()
		{
			//IL_005d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0062: Unknown result type (might be due to invalid IL or missing references)
			//IL_006a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0127: Unknown result type (might be due to invalid IL or missing references)
			//IL_012c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0134: Unknown result type (might be due to invalid IL or missing references)
			//IL_001f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0024: Unknown result type (might be due to invalid IL or missing references)
			//IL_0028: Unknown result type (might be due to invalid IL or missing references)
			//IL_002d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0042: Unknown result type (might be due to invalid IL or missing references)
			//IL_0044: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d1: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d6: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ec: Unknown result type (might be due to invalid IL or missing references)
			//IL_00f1: Unknown result type (might be due to invalid IL or missing references)
			//IL_00f5: Unknown result type (might be due to invalid IL or missing references)
			//IL_00fa: Unknown result type (might be due to invalid IL or missing references)
			//IL_010f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0111: Unknown result type (might be due to invalid IL or missing references)
			int num = _003C_003E1__state;
			PresentationNodeBase presentationNodeBase = _003C_003E4__this;
			try
			{
				Awaiter val;
				UniTask val2;
				if (num != 0)
				{
					if (num == 1)
					{
						val = _003C_003Eu__1;
						_003C_003Eu__1 = default(Awaiter);
						num = (_003C_003E1__state = -1);
						goto IL_0143;
					}
					val2 = presentationNodeBase.ExecuteAsync(ct);
					val = ((UniTask)(ref val2)).GetAwaiter();
					if (!((Awaiter)(ref val)).IsCompleted)
					{
						num = (_003C_003E1__state = 0);
						_003C_003Eu__1 = val;
						((AsyncUniTaskMethodBuilder)(ref _003C_003Et__builder)).AwaitUnsafeOnCompleted<Awaiter, _003CExecuteWithChildrenAsync_003Ed__5>(ref val, ref this);
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
				if (!ct.IsCancellationRequested)
				{
					List<IPresentationNode> children = presentationNodeBase.Children;
					if (children != null && children.Count != 0)
					{
						UniTask[] array = (UniTask[])(object)new UniTask[children.Count];
						for (int i = 0; i < children.Count; i++)
						{
							array[i] = playChildAsync(children[i], ct);
						}
						val2 = UniTask.WhenAll(array);
						val = ((UniTask)(ref val2)).GetAwaiter();
						if (!((Awaiter)(ref val)).IsCompleted)
						{
							num = (_003C_003E1__state = 1);
							_003C_003Eu__1 = val;
							((AsyncUniTaskMethodBuilder)(ref _003C_003Et__builder)).AwaitUnsafeOnCompleted<Awaiter, _003CExecuteWithChildrenAsync_003Ed__5>(ref val, ref this);
							return;
						}
						goto IL_0143;
					}
				}
				goto end_IL_000e;
				IL_0143:
				((Awaiter)(ref val)).GetResult();
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

	public List<IPresentationNode> Children { get; } = new List<IPresentationNode>(2);

	public virtual bool Excute()
	{
		return false;
	}

	public abstract UniTask ExecuteAsync(CancellationToken ct);

	[AsyncStateMachine(typeof(_003CExecuteWithChildrenAsync_003Ed__5))]
	public virtual UniTask ExecuteWithChildrenAsync(CancellationToken ct, PlayChildAsync playChildAsync)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		_003CExecuteWithChildrenAsync_003Ed__5 _003CExecuteWithChildrenAsync_003Ed__ = default(_003CExecuteWithChildrenAsync_003Ed__5);
		_003CExecuteWithChildrenAsync_003Ed__._003C_003Et__builder = AsyncUniTaskMethodBuilder.Create();
		_003CExecuteWithChildrenAsync_003Ed__._003C_003E4__this = this;
		_003CExecuteWithChildrenAsync_003Ed__.ct = ct;
		_003CExecuteWithChildrenAsync_003Ed__.playChildAsync = playChildAsync;
		_003CExecuteWithChildrenAsync_003Ed__._003C_003E1__state = -1;
		((AsyncUniTaskMethodBuilder)(ref _003CExecuteWithChildrenAsync_003Ed__._003C_003Et__builder)).Start<_003CExecuteWithChildrenAsync_003Ed__5>(ref _003CExecuteWithChildrenAsync_003Ed__);
		return ((AsyncUniTaskMethodBuilder)(ref _003CExecuteWithChildrenAsync_003Ed__._003C_003Et__builder)).Task;
	}

	public void AddNode(IPresentationNode node)
	{
		Children.Add(node);
	}
}
