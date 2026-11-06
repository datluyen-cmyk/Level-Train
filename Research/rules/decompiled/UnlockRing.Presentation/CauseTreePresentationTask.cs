using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.ExceptionServices;
using System.Runtime.InteropServices;
using System.Threading;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.CompilerServices;
using RotateRings.GamePlay.Services;
using RotateRings.GamePlay.Services.CharacterExpression;
using UniLog;

namespace UnlockRing.Presentation;

public sealed class CauseTreePresentationTask : PresentationTaskBase
{
	[StructLayout(LayoutKind.Auto)]
	[CompilerGenerated]
	private struct _003CPlayNodeAsync_003Ed__9 : IAsyncStateMachine
	{
		public int _003C_003E1__state;

		public AsyncUniTaskMethodBuilder _003C_003Et__builder;

		public IPresentationNode node;

		public CancellationToken ct;

		public CauseTreePresentationTask _003C_003E4__this;

		private Awaiter _003C_003Eu__1;

		private void MoveNext()
		{
			//IL_0090: Unknown result type (might be due to invalid IL or missing references)
			//IL_0095: Unknown result type (might be due to invalid IL or missing references)
			//IL_009c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0054: Unknown result type (might be due to invalid IL or missing references)
			//IL_0059: Unknown result type (might be due to invalid IL or missing references)
			//IL_005d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0062: Unknown result type (might be due to invalid IL or missing references)
			//IL_0076: Unknown result type (might be due to invalid IL or missing references)
			//IL_0077: Unknown result type (might be due to invalid IL or missing references)
			//IL_012b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0130: Unknown result type (might be due to invalid IL or missing references)
			//IL_0137: Unknown result type (might be due to invalid IL or missing references)
			//IL_00f2: Unknown result type (might be due to invalid IL or missing references)
			//IL_00f7: Unknown result type (might be due to invalid IL or missing references)
			//IL_00fb: Unknown result type (might be due to invalid IL or missing references)
			//IL_0100: Unknown result type (might be due to invalid IL or missing references)
			//IL_0114: Unknown result type (might be due to invalid IL or missing references)
			//IL_0115: Unknown result type (might be due to invalid IL or missing references)
			int num = _003C_003E1__state;
			CauseTreePresentationTask causeTreePresentationTask = _003C_003E4__this;
			try
			{
				if (num == 0)
				{
					goto IL_0045;
				}
				if (num == 1)
				{
					goto IL_00db;
				}
				PresentationNodeBase presentationNodeBase = default(PresentationNodeBase);
				if (node != null && !ct.IsCancellationRequested)
				{
					presentationNodeBase = node as PresentationNodeBase;
					if (presentationNodeBase == null)
					{
						goto IL_0045;
					}
					goto IL_00db;
				}
				goto end_IL_000e;
				IL_00db:
				UniTask val;
				Awaiter val2;
				try
				{
					if (num != 1)
					{
						val = presentationNodeBase.ExecuteWithChildrenAsync(ct, causeTreePresentationTask.PlayNodeAsync);
						val2 = ((UniTask)(ref val)).GetAwaiter();
						if (!((Awaiter)(ref val2)).IsCompleted)
						{
							num = (_003C_003E1__state = 1);
							_003C_003Eu__1 = val2;
							((AsyncUniTaskMethodBuilder)(ref _003C_003Et__builder)).AwaitUnsafeOnCompleted<Awaiter, _003CPlayNodeAsync_003Ed__9>(ref val2, ref this);
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
					Log.LogError("[Presentation][CauseTreeTask] ", "节点播放异常: {0}", new object[1] { ex2 });
				}
				goto end_IL_000e;
				IL_0045:
				try
				{
					if (num != 0)
					{
						val = node.ExecuteAsync(ct);
						val2 = ((UniTask)(ref val)).GetAwaiter();
						if (!((Awaiter)(ref val2)).IsCompleted)
						{
							num = (_003C_003E1__state = 0);
							_003C_003Eu__1 = val2;
							((AsyncUniTaskMethodBuilder)(ref _003C_003Et__builder)).AwaitUnsafeOnCompleted<Awaiter, _003CPlayNodeAsync_003Ed__9>(ref val2, ref this);
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
				catch (Exception ex4)
				{
					Log.LogError("[Presentation][CauseTreeTask] ", "节点播放异常: {0}", new object[1] { ex4 });
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

	[StructLayout(LayoutKind.Auto)]
	[CompilerGenerated]
	private struct _003CPlayNodeSafeAsync_003Ed__8 : IAsyncStateMachine
	{
		public int _003C_003E1__state;

		public AsyncUniTaskMethodBuilder _003C_003Et__builder;

		public CauseTreePresentationTask _003C_003E4__this;

		public IPresentationNode node;

		public CancellationToken ct;

		private Awaiter _003C_003Eu__1;

		private void MoveNext()
		{
			//IL_0059: Unknown result type (might be due to invalid IL or missing references)
			//IL_005e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0065: Unknown result type (might be due to invalid IL or missing references)
			//IL_0021: Unknown result type (might be due to invalid IL or missing references)
			//IL_0026: Unknown result type (might be due to invalid IL or missing references)
			//IL_0029: Unknown result type (might be due to invalid IL or missing references)
			//IL_002e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0042: Unknown result type (might be due to invalid IL or missing references)
			//IL_0043: Unknown result type (might be due to invalid IL or missing references)
			int num = _003C_003E1__state;
			CauseTreePresentationTask causeTreePresentationTask = _003C_003E4__this;
			try
			{
				try
				{
					Awaiter val2;
					if (num != 0)
					{
						UniTask val = causeTreePresentationTask.PlayNodeAsync(node, ct);
						val2 = ((UniTask)(ref val)).GetAwaiter();
						if (!((Awaiter)(ref val2)).IsCompleted)
						{
							num = (_003C_003E1__state = 0);
							_003C_003Eu__1 = val2;
							((AsyncUniTaskMethodBuilder)(ref _003C_003Et__builder)).AwaitUnsafeOnCompleted<Awaiter, _003CPlayNodeSafeAsync_003Ed__8>(ref val2, ref this);
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
				}
				catch (Exception ex2)
				{
					Log.LogError("[Presentation][CauseTreeTask] ", "因果树播放异常: {0}", new object[1] { ex2 });
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
	private struct _003CRunAsync_003Ed__7 : IAsyncStateMachine
	{
		public int _003C_003E1__state;

		public AsyncUniTaskMethodBuilder _003C_003Et__builder;

		public CancellationToken ct;

		public CauseTreePresentationTask _003C_003E4__this;

		private UniTask _003CtreeTask_003E5__2;

		private object _003C_003E7__wrap2;

		private int _003C_003E7__wrap3;

		private Awaiter<bool> _003C_003Eu__1;

		private Awaiter _003C_003Eu__2;

		private void MoveNext()
		{
			//IL_019f: Unknown result type (might be due to invalid IL or missing references)
			//IL_010b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0110: Unknown result type (might be due to invalid IL or missing references)
			//IL_0125: Unknown result type (might be due to invalid IL or missing references)
			//IL_0127: Unknown result type (might be due to invalid IL or missing references)
			//IL_0140: Unknown result type (might be due to invalid IL or missing references)
			//IL_0145: Unknown result type (might be due to invalid IL or missing references)
			//IL_014d: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c7: Unknown result type (might be due to invalid IL or missing references)
			//IL_00cc: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d3: Unknown result type (might be due to invalid IL or missing references)
			//IL_0083: Unknown result type (might be due to invalid IL or missing references)
			//IL_0088: Unknown result type (might be due to invalid IL or missing references)
			//IL_008b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0090: Unknown result type (might be due to invalid IL or missing references)
			//IL_0094: Unknown result type (might be due to invalid IL or missing references)
			//IL_0099: Unknown result type (might be due to invalid IL or missing references)
			//IL_01c2: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
			//IL_0051: Unknown result type (might be due to invalid IL or missing references)
			//IL_0056: Unknown result type (might be due to invalid IL or missing references)
			int num = _003C_003E1__state;
			CauseTreePresentationTask causeTreePresentationTask = _003C_003E4__this;
			try
			{
				if (num == 0)
				{
					goto IL_006a;
				}
				Awaiter val;
				if (num == 1)
				{
					val = _003C_003Eu__2;
					_003C_003Eu__2 = default(Awaiter);
					num = (_003C_003E1__state = -1);
					goto IL_015c;
				}
				if (!ct.IsCancellationRequested && PresentationMode.UseTaskPresentation && causeTreePresentationTask.rootNode != null)
				{
					_003CtreeTask_003E5__2 = causeTreePresentationTask.PlayNodeSafeAsync(causeTreePresentationTask.rootNode, ct);
					_003C_003E7__wrap2 = null;
					_003C_003E7__wrap3 = 0;
					goto IL_006a;
				}
				goto end_IL_000e;
				IL_015c:
				((Awaiter)(ref val)).GetResult();
				object obj = _003C_003E7__wrap2;
				if (obj != null)
				{
					ExceptionDispatchInfo.Capture((obj as Exception) ?? throw obj).Throw();
				}
				_003C_003E7__wrap2 = null;
				goto end_IL_000e;
				IL_006a:
				try
				{
					Awaiter<bool> val3;
					if (num != 0)
					{
						UniTask val2 = UniTask.Delay(TimeSpan.FromSeconds(0.20000000298023224), false, (PlayerLoopTiming)8, ct);
						val3 = ((UniTask)(ref val2)).SuppressCancellationThrow().GetAwaiter();
						if (!val3.IsCompleted)
						{
							num = (_003C_003E1__state = 0);
							_003C_003Eu__1 = val3;
							((AsyncUniTaskMethodBuilder)(ref _003C_003Et__builder)).AwaitUnsafeOnCompleted<Awaiter<bool>, _003CRunAsync_003Ed__7>(ref val3, ref this);
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
						causeTreePresentationTask.PlayComboEncouragements();
						causeTreePresentationTask.PlayExpressionEncouragements();
					}
				}
				catch (object obj2)
				{
					_003C_003E7__wrap2 = obj2;
				}
				val = ((UniTask)(ref _003CtreeTask_003E5__2)).GetAwaiter();
				if (!((Awaiter)(ref val)).IsCompleted)
				{
					num = (_003C_003E1__state = 1);
					_003C_003Eu__2 = val;
					((AsyncUniTaskMethodBuilder)(ref _003C_003Et__builder)).AwaitUnsafeOnCompleted<Awaiter, _003CRunAsync_003Ed__7>(ref val, ref this);
					return;
				}
				goto IL_015c;
				end_IL_000e:;
			}
			catch (Exception exception)
			{
				_003C_003E1__state = -2;
				_003CtreeTask_003E5__2 = default(UniTask);
				((AsyncUniTaskMethodBuilder)(ref _003C_003Et__builder)).SetException(exception);
				return;
			}
			_003C_003E1__state = -2;
			_003CtreeTask_003E5__2 = default(UniTask);
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

	private const string TAG = "[Presentation][CauseTreeTask] ";

	private const float EncouragementDelaySeconds = 0.2f;

	private readonly PresentationNodeBase rootNode;

	private readonly RotateRingOperationScoreFacade scoreFacade;

	private readonly CharacterExpressionEncourageService expressionService;

	private readonly IReadOnlyList<IPresentationResult> results;

	public CauseTreePresentationTask(string debugName, PresentationNodeBase rootNode, RotateRingOperationScoreFacade scoreFacade, CharacterExpressionEncourageService expressionService, IReadOnlyList<IPresentationResult> results)
		: base(debugName, PresentationTaskKind.Parallel)
	{
		this.rootNode = rootNode;
		this.scoreFacade = scoreFacade;
		this.expressionService = expressionService;
		this.results = results;
	}

	[AsyncStateMachine(typeof(_003CRunAsync_003Ed__7))]
	public override UniTask RunAsync(CancellationToken ct)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		_003CRunAsync_003Ed__7 _003CRunAsync_003Ed__ = default(_003CRunAsync_003Ed__7);
		_003CRunAsync_003Ed__._003C_003Et__builder = AsyncUniTaskMethodBuilder.Create();
		_003CRunAsync_003Ed__._003C_003E4__this = this;
		_003CRunAsync_003Ed__.ct = ct;
		_003CRunAsync_003Ed__._003C_003E1__state = -1;
		((AsyncUniTaskMethodBuilder)(ref _003CRunAsync_003Ed__._003C_003Et__builder)).Start<_003CRunAsync_003Ed__7>(ref _003CRunAsync_003Ed__);
		return ((AsyncUniTaskMethodBuilder)(ref _003CRunAsync_003Ed__._003C_003Et__builder)).Task;
	}

	[AsyncStateMachine(typeof(_003CPlayNodeSafeAsync_003Ed__8))]
	private UniTask PlayNodeSafeAsync(IPresentationNode node, CancellationToken ct)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		_003CPlayNodeSafeAsync_003Ed__8 _003CPlayNodeSafeAsync_003Ed__ = default(_003CPlayNodeSafeAsync_003Ed__8);
		_003CPlayNodeSafeAsync_003Ed__._003C_003Et__builder = AsyncUniTaskMethodBuilder.Create();
		_003CPlayNodeSafeAsync_003Ed__._003C_003E4__this = this;
		_003CPlayNodeSafeAsync_003Ed__.node = node;
		_003CPlayNodeSafeAsync_003Ed__.ct = ct;
		_003CPlayNodeSafeAsync_003Ed__._003C_003E1__state = -1;
		((AsyncUniTaskMethodBuilder)(ref _003CPlayNodeSafeAsync_003Ed__._003C_003Et__builder)).Start<_003CPlayNodeSafeAsync_003Ed__8>(ref _003CPlayNodeSafeAsync_003Ed__);
		return ((AsyncUniTaskMethodBuilder)(ref _003CPlayNodeSafeAsync_003Ed__._003C_003Et__builder)).Task;
	}

	[AsyncStateMachine(typeof(_003CPlayNodeAsync_003Ed__9))]
	private UniTask PlayNodeAsync(IPresentationNode node, CancellationToken ct)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		_003CPlayNodeAsync_003Ed__9 _003CPlayNodeAsync_003Ed__ = default(_003CPlayNodeAsync_003Ed__9);
		_003CPlayNodeAsync_003Ed__._003C_003Et__builder = AsyncUniTaskMethodBuilder.Create();
		_003CPlayNodeAsync_003Ed__._003C_003E4__this = this;
		_003CPlayNodeAsync_003Ed__.node = node;
		_003CPlayNodeAsync_003Ed__.ct = ct;
		_003CPlayNodeAsync_003Ed__._003C_003E1__state = -1;
		((AsyncUniTaskMethodBuilder)(ref _003CPlayNodeAsync_003Ed__._003C_003Et__builder)).Start<_003CPlayNodeAsync_003Ed__9>(ref _003CPlayNodeAsync_003Ed__);
		return ((AsyncUniTaskMethodBuilder)(ref _003CPlayNodeAsync_003Ed__._003C_003Et__builder)).Task;
	}

	private void PlayComboEncouragements()
	{
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		if (scoreFacade == null || results == null)
		{
			return;
		}
		for (int i = 0; i < results.Count; i++)
		{
			if (results[i] is ComboEncouragementResult { Evaluation: not GameplayOperationEvaluation.None } comboEncouragementResult)
			{
				scoreFacade.PlayEncouragementFromResult(comboEncouragementResult.Evaluation, comboEncouragementResult.WorldPosition);
			}
		}
	}

	private void PlayExpressionEncouragements()
	{
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		if (expressionService == null || results == null)
		{
			return;
		}
		for (int i = 0; i < results.Count; i++)
		{
			if (results[i] is CharacterExpressionEncouragementResult { Conditions: not null } characterExpressionEncouragementResult && characterExpressionEncouragementResult.Conditions.Count > 0)
			{
				expressionService.PlayExpressionFromResult(characterExpressionEncouragementResult.EffectiveFeedback, characterExpressionEncouragementResult.WorldPosition);
			}
		}
	}
}
