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

public abstract class PresentationTaskBase : IPresentationTask
{
	[CompilerGenerated]
	private sealed class _003C_003Ec__DisplayClass10_0
	{
		[StructLayout(LayoutKind.Auto)]
		private struct _003C_003CDelay_003Eb__0_003Ed : IAsyncStateMachine
		{
			public int _003C_003E1__state;

			public AsyncUniTaskMethodBuilder _003C_003Et__builder;

			public _003C_003Ec__DisplayClass10_0 _003C_003E4__this;

			public CancellationToken ct;

			private Awaiter _003C_003Eu__1;

			private void MoveNext()
			{
				//IL_006f: Unknown result type (might be due to invalid IL or missing references)
				//IL_0074: Unknown result type (might be due to invalid IL or missing references)
				//IL_007b: Unknown result type (might be due to invalid IL or missing references)
				//IL_0037: Unknown result type (might be due to invalid IL or missing references)
				//IL_003c: Unknown result type (might be due to invalid IL or missing references)
				//IL_003f: Unknown result type (might be due to invalid IL or missing references)
				//IL_0044: Unknown result type (might be due to invalid IL or missing references)
				//IL_0058: Unknown result type (might be due to invalid IL or missing references)
				//IL_0059: Unknown result type (might be due to invalid IL or missing references)
				int num = _003C_003E1__state;
				_003C_003Ec__DisplayClass10_0 _003C_003Ec__DisplayClass10_ = _003C_003E4__this;
				try
				{
					Awaiter val;
					if (num == 0)
					{
						val = _003C_003Eu__1;
						_003C_003Eu__1 = default(Awaiter);
						num = (_003C_003E1__state = -1);
						goto IL_008a;
					}
					if (!(_003C_003Ec__DisplayClass10_.seconds <= 0f))
					{
						UniTask val2 = UniTask.Delay(TimeSpan.FromSeconds(_003C_003Ec__DisplayClass10_.seconds), false, (PlayerLoopTiming)8, ct);
						val = ((UniTask)(ref val2)).GetAwaiter();
						if (!((Awaiter)(ref val)).IsCompleted)
						{
							num = (_003C_003E1__state = 0);
							_003C_003Eu__1 = val;
							((AsyncUniTaskMethodBuilder)(ref _003C_003Et__builder)).AwaitUnsafeOnCompleted<Awaiter, _003C_003CDelay_003Eb__0_003Ed>(ref val, ref this);
							return;
						}
						goto IL_008a;
					}
					goto end_IL_000e;
					IL_008a:
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

		public float seconds;

		[AsyncStateMachine(typeof(_003C_003CDelay_003Eb__0_003Ed))]
		internal UniTask _003CDelay_003Eb__0(CancellationToken ct)
		{
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			//IL_0007: Unknown result type (might be due to invalid IL or missing references)
			//IL_0039: Unknown result type (might be due to invalid IL or missing references)
			_003C_003CDelay_003Eb__0_003Ed _003C_003CDelay_003Eb__0_003Ed = default(_003C_003CDelay_003Eb__0_003Ed);
			_003C_003CDelay_003Eb__0_003Ed._003C_003Et__builder = AsyncUniTaskMethodBuilder.Create();
			_003C_003CDelay_003Eb__0_003Ed._003C_003E4__this = this;
			_003C_003CDelay_003Eb__0_003Ed.ct = ct;
			_003C_003CDelay_003Eb__0_003Ed._003C_003E1__state = -1;
			((AsyncUniTaskMethodBuilder)(ref _003C_003CDelay_003Eb__0_003Ed._003C_003Et__builder)).Start<_003C_003CDelay_003Eb__0_003Ed>(ref _003C_003CDelay_003Eb__0_003Ed);
			return ((AsyncUniTaskMethodBuilder)(ref _003C_003CDelay_003Eb__0_003Ed._003C_003Et__builder)).Task;
		}
	}

	[StructLayout(LayoutKind.Auto)]
	[CompilerGenerated]
	private struct _003CRunAsync_003Ed__13 : IAsyncStateMachine
	{
		public int _003C_003E1__state;

		public AsyncUniTaskMethodBuilder _003C_003Et__builder;

		public CancellationToken ct;

		public PresentationTaskBase _003C_003E4__this;

		private List<Func<CancellationToken, UniTask>>.Enumerator _003C_003E7__wrap1;

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
			int num = _003C_003E1__state;
			PresentationTaskBase presentationTaskBase = _003C_003E4__this;
			try
			{
				if (num == 0)
				{
					goto IL_0035;
				}
				if (!ct.IsCancellationRequested)
				{
					_003C_003E7__wrap1 = presentationTaskBase.steps.GetEnumerator();
					goto IL_0035;
				}
				goto end_IL_000e;
				IL_0035:
				try
				{
					if (num == 0)
					{
						goto IL_004a;
					}
					goto IL_00de;
					IL_00de:
					Func<CancellationToken, UniTask> current = default(Func<CancellationToken, UniTask>);
					if (_003C_003E7__wrap1.MoveNext())
					{
						current = _003C_003E7__wrap1.Current;
						goto IL_004a;
					}
					goto end_IL_0035;
					IL_004a:
					try
					{
						Awaiter val2;
						if (num != 0)
						{
							UniTask val = current(ct);
							val2 = ((UniTask)(ref val)).GetAwaiter();
							if (!((Awaiter)(ref val2)).IsCompleted)
							{
								num = (_003C_003E1__state = 0);
								_003C_003Eu__1 = val2;
								((AsyncUniTaskMethodBuilder)(ref _003C_003Et__builder)).AwaitUnsafeOnCompleted<Awaiter, _003CRunAsync_003Ed__13>(ref val2, ref this);
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
						goto end_IL_000e;
					}
					catch (Exception ex2)
					{
						Log.LogError("[Presentation][PresentationTaskBase] ", "任务 {0} 步骤执行异常，继续后续步骤: {1}", new object[2] { presentationTaskBase.DebugName, ex2 });
					}
					goto IL_00de;
					end_IL_0035:;
				}
				finally
				{
					if (num < 0)
					{
						((IDisposable)_003C_003E7__wrap1/*cast due to .constrained prefix*/).Dispose();
					}
				}
				_003C_003E7__wrap1 = default(List<Func<CancellationToken, UniTask>>.Enumerator);
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

	private const string TAG = "[Presentation][PresentationTaskBase] ";

	private readonly List<Func<CancellationToken, UniTask>> steps = new List<Func<CancellationToken, UniTask>>(8);

	public string DebugName { get; }

	public PresentationTaskKind Kind { get; }

	protected PresentationTaskBase(string debugName, PresentationTaskKind kind)
	{
		DebugName = debugName;
		Kind = kind;
	}

	protected void Then(Func<CancellationToken, UniTask> step)
	{
		if (step != null)
		{
			steps.Add(step);
		}
	}

	protected void Delay(float seconds)
	{
		_003C_003Ec__DisplayClass10_0 CS_0024_003C_003E8__locals2 = new _003C_003Ec__DisplayClass10_0();
		CS_0024_003C_003E8__locals2.seconds = seconds;
		steps.Add([AsyncStateMachine(typeof(_003C_003Ec__DisplayClass10_0._003C_003CDelay_003Eb__0_003Ed))] (CancellationToken ct) =>
		{
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			//IL_0007: Unknown result type (might be due to invalid IL or missing references)
			//IL_0039: Unknown result type (might be due to invalid IL or missing references)
			_003C_003Ec__DisplayClass10_0._003C_003CDelay_003Eb__0_003Ed _003C_003CDelay_003Eb__0_003Ed = default(_003C_003Ec__DisplayClass10_0._003C_003CDelay_003Eb__0_003Ed);
			_003C_003CDelay_003Eb__0_003Ed._003C_003Et__builder = AsyncUniTaskMethodBuilder.Create();
			_003C_003CDelay_003Eb__0_003Ed._003C_003E4__this = CS_0024_003C_003E8__locals2;
			_003C_003CDelay_003Eb__0_003Ed.ct = ct;
			_003C_003CDelay_003Eb__0_003Ed._003C_003E1__state = -1;
			((AsyncUniTaskMethodBuilder)(ref _003C_003CDelay_003Eb__0_003Ed._003C_003Et__builder)).Start<_003C_003Ec__DisplayClass10_0._003C_003CDelay_003Eb__0_003Ed>(ref _003C_003CDelay_003Eb__0_003Ed);
			return ((AsyncUniTaskMethodBuilder)(ref _003C_003CDelay_003Eb__0_003Ed._003C_003Et__builder)).Task;
		});
	}

	protected void Call(Action action)
	{
		steps.Add(delegate
		{
			//IL_003d: Unknown result type (might be due to invalid IL or missing references)
			try
			{
				action?.Invoke();
			}
			catch (Exception ex)
			{
				Log.LogError("[Presentation][PresentationTaskBase] ", "任务 {0} 同步步骤异常: {1}", new object[2] { DebugName, ex });
			}
			return UniTask.CompletedTask;
		});
	}

	protected void WaitUntil(Func<bool> condition)
	{
		if (condition != null)
		{
			steps.Add((CancellationToken ct) => UniTask.WaitUntil(condition, (PlayerLoopTiming)8, ct));
		}
	}

	[AsyncStateMachine(typeof(_003CRunAsync_003Ed__13))]
	public virtual UniTask RunAsync(CancellationToken ct)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		_003CRunAsync_003Ed__13 _003CRunAsync_003Ed__ = default(_003CRunAsync_003Ed__13);
		_003CRunAsync_003Ed__._003C_003Et__builder = AsyncUniTaskMethodBuilder.Create();
		_003CRunAsync_003Ed__._003C_003E4__this = this;
		_003CRunAsync_003Ed__.ct = ct;
		_003CRunAsync_003Ed__._003C_003E1__state = -1;
		((AsyncUniTaskMethodBuilder)(ref _003CRunAsync_003Ed__._003C_003Et__builder)).Start<_003CRunAsync_003Ed__13>(ref _003CRunAsync_003Ed__);
		return ((AsyncUniTaskMethodBuilder)(ref _003CRunAsync_003Ed__._003C_003Et__builder)).Task;
	}
}
