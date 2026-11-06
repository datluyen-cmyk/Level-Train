using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.CompilerServices;
using UniLog;

namespace UnlockRing.Presentation;

public sealed class PresentationScheduler : IDisposable
{
	[StructLayout(LayoutKind.Auto)]
	[CompilerGenerated]
	private struct _003CRunTrackedAsync_003Ed__20 : IAsyncStateMachine
	{
		public int _003C_003E1__state;

		public AsyncUniTaskVoidMethodBuilder _003C_003Et__builder;

		public IPresentationTask task;

		public PresentationScheduler _003C_003E4__this;

		public bool isSerial;

		private Awaiter _003C_003Eu__1;

		private void MoveNext()
		{
			//IL_005b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0060: Unknown result type (might be due to invalid IL or missing references)
			//IL_0067: Unknown result type (might be due to invalid IL or missing references)
			//IL_0020: Unknown result type (might be due to invalid IL or missing references)
			//IL_0025: Unknown result type (might be due to invalid IL or missing references)
			//IL_0028: Unknown result type (might be due to invalid IL or missing references)
			//IL_002d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0041: Unknown result type (might be due to invalid IL or missing references)
			//IL_0042: Unknown result type (might be due to invalid IL or missing references)
			int num = _003C_003E1__state;
			PresentationScheduler presentationScheduler = _003C_003E4__this;
			try
			{
				try
				{
					Awaiter val2;
					if (num != 0)
					{
						UniTask val = task.RunAsync(presentationScheduler.lifecycleToken);
						val2 = ((UniTask)(ref val)).GetAwaiter();
						if (!((Awaiter)(ref val2)).IsCompleted)
						{
							num = (_003C_003E1__state = 0);
							_003C_003Eu__1 = val2;
							((AsyncUniTaskVoidMethodBuilder)(ref _003C_003Et__builder)).AwaitUnsafeOnCompleted<Awaiter, _003CRunTrackedAsync_003Ed__20>(ref val2, ref this);
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
					Log.LogError("[Presentation][PresentationScheduler] ", "任务 {0} 执行异常: {1}", new object[2] { task.DebugName, ex2 });
				}
				finally
				{
					if (num < 0)
					{
						if (isSerial)
						{
							presentationScheduler.isSerialPlaying = false;
							presentationScheduler.TryStartSerialFlush();
						}
						else
						{
							presentationScheduler.parallelRunning.Remove(task);
						}
					}
				}
			}
			catch (Exception exception)
			{
				_003C_003E1__state = -2;
				((AsyncUniTaskVoidMethodBuilder)(ref _003C_003Et__builder)).SetException(exception);
				return;
			}
			_003C_003E1__state = -2;
			((AsyncUniTaskVoidMethodBuilder)(ref _003C_003Et__builder)).SetResult();
		}

		void IAsyncStateMachine.MoveNext()
		{
			//ILSpy generated this explicit interface implementation from .override directive in MoveNext
			this.MoveNext();
		}

		[DebuggerHidden]
		private void SetStateMachine(IAsyncStateMachine stateMachine)
		{
			((AsyncUniTaskVoidMethodBuilder)(ref _003C_003Et__builder)).SetStateMachine(stateMachine);
		}

		void IAsyncStateMachine.SetStateMachine(IAsyncStateMachine stateMachine)
		{
			//ILSpy generated this explicit interface implementation from .override directive in SetStateMachine
			this.SetStateMachine(stateMachine);
		}
	}

	[StructLayout(LayoutKind.Auto)]
	[CompilerGenerated]
	private struct _003CWaitAllIdleAsync_003Ed__15 : IAsyncStateMachine
	{
		public int _003C_003E1__state;

		public AsyncUniTaskMethodBuilder<bool> _003C_003Et__builder;

		public CancellationToken ct;

		public PresentationScheduler _003C_003E4__this;

		private Awaiter _003C_003Eu__1;

		private void MoveNext()
		{
			//IL_0076: Unknown result type (might be due to invalid IL or missing references)
			//IL_007b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0082: Unknown result type (might be due to invalid IL or missing references)
			//IL_003d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0042: Unknown result type (might be due to invalid IL or missing references)
			//IL_0046: Unknown result type (might be due to invalid IL or missing references)
			//IL_004b: Unknown result type (might be due to invalid IL or missing references)
			//IL_005f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0060: Unknown result type (might be due to invalid IL or missing references)
			int num = _003C_003E1__state;
			PresentationScheduler presentationScheduler = _003C_003E4__this;
			bool result;
			try
			{
				if (num != 0)
				{
					goto IL_0098;
				}
				Awaiter val = _003C_003Eu__1;
				_003C_003Eu__1 = default(Awaiter);
				num = (_003C_003E1__state = -1);
				goto IL_0091;
				IL_0091:
				((Awaiter)(ref val)).GetResult();
				goto IL_0098;
				IL_0098:
				if (!presentationScheduler.IsIdle)
				{
					if (!ct.IsCancellationRequested && !presentationScheduler.lifecycleToken.IsCancellationRequested)
					{
						UniTask val2 = UniTask.Yield(ct);
						val = ((UniTask)(ref val2)).GetAwaiter();
						if (!((Awaiter)(ref val)).IsCompleted)
						{
							num = (_003C_003E1__state = 0);
							_003C_003Eu__1 = val;
							_003C_003Et__builder.AwaitUnsafeOnCompleted<Awaiter, _003CWaitAllIdleAsync_003Ed__15>(ref val, ref this);
							return;
						}
						goto IL_0091;
					}
					result = false;
				}
				else
				{
					result = true;
				}
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

	private const string TAG = "[Presentation][PresentationScheduler] ";

	private readonly Queue<IPresentationTask> serialQueue = new Queue<IPresentationTask>(4);

	private readonly List<IPresentationTask> parallelRunning = new List<IPresentationTask>(4);

	private readonly CancellationTokenSource lifecycleCts;

	private readonly CancellationToken lifecycleToken;

	private bool isSerialPlaying;

	private bool disposed;

	public bool HasSerialPlaying => isSerialPlaying;

	public bool HasParallelPlaying => parallelRunning.Count > 0;

	public bool IsIdle
	{
		get
		{
			if (!isSerialPlaying)
			{
				return parallelRunning.Count == 0;
			}
			return false;
		}
	}

	public PresentationScheduler(CancellationToken lifecycleToken)
	{
		lifecycleCts = CancellationTokenSource.CreateLinkedTokenSource(new CancellationToken[1] { lifecycleToken });
		this.lifecycleToken = lifecycleCts.Token;
	}

	public void Submit(IPresentationTask task)
	{
		if (task != null && !disposed)
		{
			switch (task.Kind)
			{
			case PresentationTaskKind.Serial:
				serialQueue.Enqueue(task);
				TryStartSerialFlush();
				break;
			case PresentationTaskKind.Parallel:
				StartParallel(task);
				break;
			default:
				serialQueue.Enqueue(task);
				TryStartSerialFlush();
				break;
			}
		}
	}

	[AsyncStateMachine(typeof(_003CWaitAllIdleAsync_003Ed__15))]
	public UniTask<bool> WaitAllIdleAsync(CancellationToken ct)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		_003CWaitAllIdleAsync_003Ed__15 _003CWaitAllIdleAsync_003Ed__ = default(_003CWaitAllIdleAsync_003Ed__15);
		_003CWaitAllIdleAsync_003Ed__._003C_003Et__builder = AsyncUniTaskMethodBuilder<bool>.Create();
		_003CWaitAllIdleAsync_003Ed__._003C_003E4__this = this;
		_003CWaitAllIdleAsync_003Ed__.ct = ct;
		_003CWaitAllIdleAsync_003Ed__._003C_003E1__state = -1;
		_003CWaitAllIdleAsync_003Ed__._003C_003Et__builder.Start<_003CWaitAllIdleAsync_003Ed__15>(ref _003CWaitAllIdleAsync_003Ed__);
		return _003CWaitAllIdleAsync_003Ed__._003C_003Et__builder.Task;
	}

	public void ResetAndCancel()
	{
		if (!disposed)
		{
			disposed = true;
			serialQueue.Clear();
			isSerialPlaying = false;
			parallelRunning.Clear();
			lifecycleCts.Cancel();
			lifecycleCts.Dispose();
		}
	}

	public void Dispose()
	{
		if (!disposed)
		{
			disposed = true;
			serialQueue.Clear();
			parallelRunning.Clear();
			isSerialPlaying = false;
			lifecycleCts.Cancel();
			lifecycleCts.Dispose();
		}
	}

	private void TryStartSerialFlush()
	{
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		if (!isSerialPlaying && serialQueue.Count != 0 && !disposed)
		{
			isSerialPlaying = true;
			IPresentationTask task = serialQueue.Dequeue();
			UniTaskVoid val = RunTrackedAsync(task, isSerial: true);
			((UniTaskVoid)(ref val)).Forget();
		}
	}

	private void StartParallel(IPresentationTask task)
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		if (!disposed)
		{
			parallelRunning.Add(task);
			UniTaskVoid val = RunTrackedAsync(task, isSerial: false);
			((UniTaskVoid)(ref val)).Forget();
		}
	}

	[AsyncStateMachine(typeof(_003CRunTrackedAsync_003Ed__20))]
	private UniTaskVoid RunTrackedAsync(IPresentationTask task, bool isSerial)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		_003CRunTrackedAsync_003Ed__20 _003CRunTrackedAsync_003Ed__ = default(_003CRunTrackedAsync_003Ed__20);
		_003CRunTrackedAsync_003Ed__._003C_003Et__builder = AsyncUniTaskVoidMethodBuilder.Create();
		_003CRunTrackedAsync_003Ed__._003C_003E4__this = this;
		_003CRunTrackedAsync_003Ed__.task = task;
		_003CRunTrackedAsync_003Ed__.isSerial = isSerial;
		_003CRunTrackedAsync_003Ed__._003C_003E1__state = -1;
		((AsyncUniTaskVoidMethodBuilder)(ref _003CRunTrackedAsync_003Ed__._003C_003Et__builder)).Start<_003CRunTrackedAsync_003Ed__20>(ref _003CRunTrackedAsync_003Ed__);
		return ((AsyncUniTaskVoidMethodBuilder)(ref _003CRunTrackedAsync_003Ed__._003C_003Et__builder)).Task;
	}
}
