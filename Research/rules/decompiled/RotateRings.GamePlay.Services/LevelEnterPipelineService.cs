using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.CompilerServices;
using RotateRings.GamePlay.Services.LevelStepLimit;
using UniLog;

namespace RotateRings.GamePlay.Services;

public sealed class LevelEnterPipelineService : GameplayServiceBase<GameplayManager>
{
	[StructLayout(LayoutKind.Auto)]
	[CompilerGenerated]
	private struct _003CExecuteOneStepAsync_003Ed__13 : IAsyncStateMachine
	{
		public int _003C_003E1__state;

		public AsyncUniTaskMethodBuilder _003C_003Et__builder;

		public ILevelEnterPipelineStep step;

		public LevelEnterPipelineContext ctx;

		public CancellationToken destroyCt;

		private Awaiter _003C_003Eu__1;

		private void MoveNext()
		{
			//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
			//IL_0066: Unknown result type (might be due to invalid IL or missing references)
			//IL_006b: Unknown result type (might be due to invalid IL or missing references)
			//IL_006e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0073: Unknown result type (might be due to invalid IL or missing references)
			//IL_0045: Unknown result type (might be due to invalid IL or missing references)
			//IL_0087: Unknown result type (might be due to invalid IL or missing references)
			//IL_0088: Unknown result type (might be due to invalid IL or missing references)
			int num = _003C_003E1__state;
			try
			{
				if (num == 0 || step.ShouldRun(ctx))
				{
					try
					{
						Awaiter val;
						if (num == 0)
						{
							val = _003C_003Eu__1;
							_003C_003Eu__1 = default(Awaiter);
							num = (_003C_003E1__state = -1);
							goto IL_00bc;
						}
						if (step.AwaitCompletion)
						{
							UniTask val2 = step.ExecuteAsync(ctx, destroyCt);
							val = ((UniTask)(ref val2)).GetAwaiter();
							if (!((Awaiter)(ref val)).IsCompleted)
							{
								num = (_003C_003E1__state = 0);
								_003C_003Eu__1 = val;
								((AsyncUniTaskMethodBuilder)(ref _003C_003Et__builder)).AwaitUnsafeOnCompleted<Awaiter, _003CExecuteOneStepAsync_003Ed__13>(ref val, ref this);
								return;
							}
							goto IL_00bc;
						}
						UniTaskExtensions.Forget(step.ExecuteAsync(ctx, destroyCt));
						goto end_IL_0023;
						IL_00bc:
						((Awaiter)(ref val)).GetResult();
						end_IL_0023:;
					}
					catch (Exception ex)
					{
						if (step.Priority < 210)
						{
							ctx.Aborted = true;
							Log.LogError("[LevelEnterPipeline] ", "加载段 step({0}) 异常，进关已中止: {1}", new object[2]
							{
								step.GetType().Name,
								ex
							});
						}
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
	private struct _003CExecutePipelineRangeAsync_003Ed__12 : IAsyncStateMachine
	{
		public int _003C_003E1__state;

		public AsyncUniTaskMethodBuilder _003C_003Et__builder;

		public int start;

		public LevelEnterPipelineService _003C_003E4__this;

		public LevelEnterPipelineContext ctx;

		public CancellationToken destroyCt;

		public int end;

		private int _003Ci_003E5__2;

		private Awaiter _003C_003Eu__1;

		private void MoveNext()
		{
			//IL_007a: Unknown result type (might be due to invalid IL or missing references)
			//IL_007f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0086: Unknown result type (might be due to invalid IL or missing references)
			//IL_003f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0044: Unknown result type (might be due to invalid IL or missing references)
			//IL_0047: Unknown result type (might be due to invalid IL or missing references)
			//IL_004c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0060: Unknown result type (might be due to invalid IL or missing references)
			//IL_0061: Unknown result type (might be due to invalid IL or missing references)
			int num = _003C_003E1__state;
			LevelEnterPipelineService levelEnterPipelineService = _003C_003E4__this;
			try
			{
				if (num != 0)
				{
					_003Ci_003E5__2 = start;
					goto IL_00ca;
				}
				Awaiter val = _003C_003Eu__1;
				_003C_003Eu__1 = default(Awaiter);
				num = (_003C_003E1__state = -1);
				goto IL_0095;
				IL_0095:
				((Awaiter)(ref val)).GetResult();
				if (!ctx.Aborted && !destroyCt.IsCancellationRequested)
				{
					_003Ci_003E5__2++;
					goto IL_00ca;
				}
				goto end_IL_000e;
				IL_00ca:
				if (_003Ci_003E5__2 < end)
				{
					UniTask val2 = ExecuteOneStepAsync(levelEnterPipelineService.runPipeline[_003Ci_003E5__2], ctx, destroyCt);
					val = ((UniTask)(ref val2)).GetAwaiter();
					if (!((Awaiter)(ref val)).IsCompleted)
					{
						num = (_003C_003E1__state = 0);
						_003C_003Eu__1 = val;
						((AsyncUniTaskMethodBuilder)(ref _003C_003Et__builder)).AwaitUnsafeOnCompleted<Awaiter, _003CExecutePipelineRangeAsync_003Ed__12>(ref val, ref this);
						return;
					}
					goto IL_0095;
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
	private struct _003CExecuteStepAsync_003Ed__6 : IAsyncStateMachine
	{
		public int _003C_003E1__state;

		public AsyncUniTaskMethodBuilder _003C_003Et__builder;

		public ILevelEnterPipelineStep step;

		public LevelEnterPipelineContext ctx;

		public CancellationToken cancellationToken;

		private Awaiter _003C_003Eu__1;

		private void MoveNext()
		{
			//IL_0069: Unknown result type (might be due to invalid IL or missing references)
			//IL_006e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0075: Unknown result type (might be due to invalid IL or missing references)
			//IL_0031: Unknown result type (might be due to invalid IL or missing references)
			//IL_0036: Unknown result type (might be due to invalid IL or missing references)
			//IL_0039: Unknown result type (might be due to invalid IL or missing references)
			//IL_003e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0052: Unknown result type (might be due to invalid IL or missing references)
			//IL_0053: Unknown result type (might be due to invalid IL or missing references)
			int num = _003C_003E1__state;
			try
			{
				Awaiter val;
				if (num == 0)
				{
					val = _003C_003Eu__1;
					_003C_003Eu__1 = default(Awaiter);
					num = (_003C_003E1__state = -1);
					goto IL_0084;
				}
				if (step != null && ctx != null)
				{
					UniTask val2 = ExecuteOneStepAsync(step, ctx, cancellationToken);
					val = ((UniTask)(ref val2)).GetAwaiter();
					if (!((Awaiter)(ref val)).IsCompleted)
					{
						num = (_003C_003E1__state = 0);
						_003C_003Eu__1 = val;
						((AsyncUniTaskMethodBuilder)(ref _003C_003Et__builder)).AwaitUnsafeOnCompleted<Awaiter, _003CExecuteStepAsync_003Ed__6>(ref val, ref this);
						return;
					}
					goto IL_0084;
				}
				goto end_IL_0007;
				IL_0084:
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

	[StructLayout(LayoutKind.Auto)]
	[CompilerGenerated]
	private struct _003CRunAsync_003Ed__10 : IAsyncStateMachine
	{
		public int _003C_003E1__state;

		public AsyncUniTaskMethodBuilder _003C_003Et__builder;

		public LevelEnterPipelineService _003C_003E4__this;

		public int level;

		public bool releaseGroupOnClear;

		public string loadSource;

		public CancellationToken destroyCt;

		private LevelEnterPipelineContext _003Cctx_003E5__2;

		private Awaiter _003C_003Eu__1;

		private void MoveNext()
		{
			//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
			//IL_012e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0133: Unknown result type (might be due to invalid IL or missing references)
			//IL_013a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0068: Unknown result type (might be due to invalid IL or missing references)
			//IL_006d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0071: Unknown result type (might be due to invalid IL or missing references)
			//IL_0076: Unknown result type (might be due to invalid IL or missing references)
			//IL_008a: Unknown result type (might be due to invalid IL or missing references)
			//IL_008b: Unknown result type (might be due to invalid IL or missing references)
			//IL_01d6: Unknown result type (might be due to invalid IL or missing references)
			//IL_01db: Unknown result type (might be due to invalid IL or missing references)
			//IL_01e2: Unknown result type (might be due to invalid IL or missing references)
			//IL_019d: Unknown result type (might be due to invalid IL or missing references)
			//IL_01a2: Unknown result type (might be due to invalid IL or missing references)
			//IL_01a6: Unknown result type (might be due to invalid IL or missing references)
			//IL_01ab: Unknown result type (might be due to invalid IL or missing references)
			//IL_00f2: Unknown result type (might be due to invalid IL or missing references)
			//IL_00f7: Unknown result type (might be due to invalid IL or missing references)
			//IL_00fb: Unknown result type (might be due to invalid IL or missing references)
			//IL_0100: Unknown result type (might be due to invalid IL or missing references)
			//IL_01bf: Unknown result type (might be due to invalid IL or missing references)
			//IL_01c0: Unknown result type (might be due to invalid IL or missing references)
			//IL_0114: Unknown result type (might be due to invalid IL or missing references)
			//IL_0115: Unknown result type (might be due to invalid IL or missing references)
			int num = _003C_003E1__state;
			LevelEnterPipelineService levelEnterPipelineService = _003C_003E4__this;
			try
			{
				UniTask val2;
				Awaiter val;
				int num2 = default(int);
				switch (num)
				{
				default:
					_003Cctx_003E5__2 = new LevelEnterPipelineContext
					{
						Manager = levelEnterPipelineService.Host,
						Level = level,
						ReleaseGroupOnClear = releaseGroupOnClear,
						LoadSource = loadSource
					};
					val2 = levelEnterPipelineService.RunLoadPrefabPhaseAsync(_003Cctx_003E5__2, destroyCt);
					val = ((UniTask)(ref val2)).GetAwaiter();
					if (!((Awaiter)(ref val)).IsCompleted)
					{
						num = (_003C_003E1__state = 0);
						_003C_003Eu__1 = val;
						((AsyncUniTaskMethodBuilder)(ref _003C_003Et__builder)).AwaitUnsafeOnCompleted<Awaiter, _003CRunAsync_003Ed__10>(ref val, ref this);
						return;
					}
					goto IL_00bf;
				case 0:
					val = _003C_003Eu__1;
					_003C_003Eu__1 = default(Awaiter);
					num = (_003C_003E1__state = -1);
					goto IL_00bf;
				case 1:
					val = _003C_003Eu__1;
					_003C_003Eu__1 = default(Awaiter);
					num = (_003C_003E1__state = -1);
					goto IL_0149;
				case 2:
					break;
					IL_0149:
					((Awaiter)(ref val)).GetResult();
					if (!_003Cctx_003E5__2.Aborted && !destroyCt.IsCancellationRequested)
					{
						num2 = levelEnterPipelineService.FindEnterPhaseStartIndex();
						if (num2 >= 0)
						{
							break;
						}
					}
					goto end_IL_000e;
					IL_00bf:
					((Awaiter)(ref val)).GetResult();
					if (!_003Cctx_003E5__2.Aborted && !destroyCt.IsCancellationRequested)
					{
						val2 = levelEnterPipelineService.RunLevelReadyPhaseAsync(_003Cctx_003E5__2, destroyCt);
						val = ((UniTask)(ref val2)).GetAwaiter();
						if (!((Awaiter)(ref val)).IsCompleted)
						{
							num = (_003C_003E1__state = 1);
							_003C_003Eu__1 = val;
							((AsyncUniTaskMethodBuilder)(ref _003C_003Et__builder)).AwaitUnsafeOnCompleted<Awaiter, _003CRunAsync_003Ed__10>(ref val, ref this);
							return;
						}
						goto IL_0149;
					}
					goto end_IL_000e;
				}
				try
				{
					if (num != 2)
					{
						val2 = levelEnterPipelineService.ExecutePipelineRangeAsync(num2, levelEnterPipelineService.runPipeline.Count, _003Cctx_003E5__2, destroyCt);
						val = ((UniTask)(ref val2)).GetAwaiter();
						if (!((Awaiter)(ref val)).IsCompleted)
						{
							num = (_003C_003E1__state = 2);
							_003C_003Eu__1 = val;
							((AsyncUniTaskMethodBuilder)(ref _003C_003Et__builder)).AwaitUnsafeOnCompleted<Awaiter, _003CRunAsync_003Ed__10>(ref val, ref this);
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
				catch (Exception ex)
				{
					Log.LogError("[LevelEnterPipeline] ", "入场管线异常: {0}", new object[1] { ex });
				}
				end_IL_000e:;
			}
			catch (Exception exception)
			{
				_003C_003E1__state = -2;
				_003Cctx_003E5__2 = null;
				((AsyncUniTaskMethodBuilder)(ref _003C_003Et__builder)).SetException(exception);
				return;
			}
			_003C_003E1__state = -2;
			_003Cctx_003E5__2 = null;
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
	private struct _003CRunLevelReadyPhaseAsync_003Ed__8 : IAsyncStateMachine
	{
		public int _003C_003E1__state;

		public AsyncUniTaskMethodBuilder _003C_003Et__builder;

		public LevelEnterPipelineService _003C_003E4__this;

		public LevelEnterPipelineContext ctx;

		public CancellationToken cancellationToken;

		private Awaiter _003C_003Eu__1;

		private void MoveNext()
		{
			//IL_005d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0062: Unknown result type (might be due to invalid IL or missing references)
			//IL_0069: Unknown result type (might be due to invalid IL or missing references)
			//IL_0025: Unknown result type (might be due to invalid IL or missing references)
			//IL_002a: Unknown result type (might be due to invalid IL or missing references)
			//IL_002d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0032: Unknown result type (might be due to invalid IL or missing references)
			//IL_0046: Unknown result type (might be due to invalid IL or missing references)
			//IL_0047: Unknown result type (might be due to invalid IL or missing references)
			int num = _003C_003E1__state;
			LevelEnterPipelineService levelEnterPipelineService = _003C_003E4__this;
			try
			{
				Awaiter val2;
				if (num != 0)
				{
					UniTask val = levelEnterPipelineService.RunPipelinePriorityRangeAsync(70, 210, ctx, cancellationToken);
					val2 = ((UniTask)(ref val)).GetAwaiter();
					if (!((Awaiter)(ref val2)).IsCompleted)
					{
						num = (_003C_003E1__state = 0);
						_003C_003Eu__1 = val2;
						((AsyncUniTaskMethodBuilder)(ref _003C_003Et__builder)).AwaitUnsafeOnCompleted<Awaiter, _003CRunLevelReadyPhaseAsync_003Ed__8>(ref val2, ref this);
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
	private struct _003CRunLoadPrefabPhaseAsync_003Ed__7 : IAsyncStateMachine
	{
		public int _003C_003E1__state;

		public AsyncUniTaskMethodBuilder _003C_003Et__builder;

		public LevelEnterPipelineService _003C_003E4__this;

		public LevelEnterPipelineContext ctx;

		public CancellationToken cancellationToken;

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
			LevelEnterPipelineService levelEnterPipelineService = _003C_003E4__this;
			try
			{
				Awaiter val2;
				if (num != 0)
				{
					UniTask val = levelEnterPipelineService.RunPipelinePriorityRangeAsync(5, 70, ctx, cancellationToken);
					val2 = ((UniTask)(ref val)).GetAwaiter();
					if (!((Awaiter)(ref val2)).IsCompleted)
					{
						num = (_003C_003E1__state = 0);
						_003C_003Eu__1 = val2;
						((AsyncUniTaskMethodBuilder)(ref _003C_003Et__builder)).AwaitUnsafeOnCompleted<Awaiter, _003CRunLoadPrefabPhaseAsync_003Ed__7>(ref val2, ref this);
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
	private struct _003CRunPipelinePriorityRangeAsync_003Ed__11 : IAsyncStateMachine
	{
		public int _003C_003E1__state;

		public AsyncUniTaskMethodBuilder _003C_003Et__builder;

		public LevelEnterPipelineService _003C_003E4__this;

		public int minPriorityInclusive;

		public int maxPriorityExclusive;

		public LevelEnterPipelineContext ctx;

		public CancellationToken cancellationToken;

		private int _003Ci_003E5__2;

		private Awaiter _003C_003Eu__1;

		private void MoveNext()
		{
			//IL_009d: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
			//IL_0061: Unknown result type (might be due to invalid IL or missing references)
			//IL_0066: Unknown result type (might be due to invalid IL or missing references)
			//IL_006a: Unknown result type (might be due to invalid IL or missing references)
			//IL_006f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0083: Unknown result type (might be due to invalid IL or missing references)
			//IL_0084: Unknown result type (might be due to invalid IL or missing references)
			int num = _003C_003E1__state;
			LevelEnterPipelineService levelEnterPipelineService = _003C_003E4__this;
			try
			{
				if (num != 0)
				{
					_003Ci_003E5__2 = 0;
					goto IL_00ed;
				}
				Awaiter val = _003C_003Eu__1;
				_003C_003Eu__1 = default(Awaiter);
				num = (_003C_003E1__state = -1);
				goto IL_00b8;
				IL_00db:
				_003Ci_003E5__2++;
				goto IL_00ed;
				IL_00b8:
				((Awaiter)(ref val)).GetResult();
				if (!ctx.Aborted && !cancellationToken.IsCancellationRequested)
				{
					goto IL_00db;
				}
				goto end_IL_000e;
				IL_00ed:
				if (_003Ci_003E5__2 < levelEnterPipelineService.runPipeline.Count)
				{
					ILevelEnterPipelineStep levelEnterPipelineStep = levelEnterPipelineService.runPipeline[_003Ci_003E5__2];
					if (levelEnterPipelineStep.Priority >= minPriorityInclusive && levelEnterPipelineStep.Priority < maxPriorityExclusive)
					{
						UniTask val2 = ExecuteOneStepAsync(levelEnterPipelineStep, ctx, cancellationToken);
						val = ((UniTask)(ref val2)).GetAwaiter();
						if (!((Awaiter)(ref val)).IsCompleted)
						{
							num = (_003C_003E1__state = 0);
							_003C_003Eu__1 = val;
							((AsyncUniTaskMethodBuilder)(ref _003C_003Et__builder)).AwaitUnsafeOnCompleted<Awaiter, _003CRunPipelinePriorityRangeAsync_003Ed__11>(ref val, ref this);
							return;
						}
						goto IL_00b8;
					}
					goto IL_00db;
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
	private struct _003CRunPreplacedLevelAsync_003Ed__9 : IAsyncStateMachine
	{
		public int _003C_003E1__state;

		public AsyncUniTaskMethodBuilder _003C_003Et__builder;

		public LevelEnterPipelineService _003C_003E4__this;

		public LevelEnterPipelineContext ctx;

		public CancellationToken cancellationToken;

		private Awaiter _003C_003Eu__1;

		private void MoveNext()
		{
			//IL_0073: Unknown result type (might be due to invalid IL or missing references)
			//IL_0078: Unknown result type (might be due to invalid IL or missing references)
			//IL_007f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0106: Unknown result type (might be due to invalid IL or missing references)
			//IL_010b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0112: Unknown result type (might be due to invalid IL or missing references)
			//IL_0195: Unknown result type (might be due to invalid IL or missing references)
			//IL_019a: Unknown result type (might be due to invalid IL or missing references)
			//IL_01a1: Unknown result type (might be due to invalid IL or missing references)
			//IL_0202: Unknown result type (might be due to invalid IL or missing references)
			//IL_0207: Unknown result type (might be due to invalid IL or missing references)
			//IL_020e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0038: Unknown result type (might be due to invalid IL or missing references)
			//IL_003d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0040: Unknown result type (might be due to invalid IL or missing references)
			//IL_0045: Unknown result type (might be due to invalid IL or missing references)
			//IL_01ca: Unknown result type (might be due to invalid IL or missing references)
			//IL_01cf: Unknown result type (might be due to invalid IL or missing references)
			//IL_01d2: Unknown result type (might be due to invalid IL or missing references)
			//IL_01d7: Unknown result type (might be due to invalid IL or missing references)
			//IL_0059: Unknown result type (might be due to invalid IL or missing references)
			//IL_005a: Unknown result type (might be due to invalid IL or missing references)
			//IL_01eb: Unknown result type (might be due to invalid IL or missing references)
			//IL_01ec: Unknown result type (might be due to invalid IL or missing references)
			//IL_00cb: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d0: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d3: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d8: Unknown result type (might be due to invalid IL or missing references)
			//IL_015a: Unknown result type (might be due to invalid IL or missing references)
			//IL_015f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0162: Unknown result type (might be due to invalid IL or missing references)
			//IL_0167: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ec: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ed: Unknown result type (might be due to invalid IL or missing references)
			//IL_017b: Unknown result type (might be due to invalid IL or missing references)
			//IL_017c: Unknown result type (might be due to invalid IL or missing references)
			int num = _003C_003E1__state;
			LevelEnterPipelineService levelEnterPipelineService = _003C_003E4__this;
			try
			{
				UniTask val2;
				Awaiter val;
				switch (num)
				{
				default:
					val2 = levelEnterPipelineService.RunPipelinePriorityRangeAsync(70, 145, ctx, cancellationToken);
					val = ((UniTask)(ref val2)).GetAwaiter();
					if (!((Awaiter)(ref val)).IsCompleted)
					{
						num = (_003C_003E1__state = 0);
						_003C_003Eu__1 = val;
						((AsyncUniTaskMethodBuilder)(ref _003C_003Et__builder)).AwaitUnsafeOnCompleted<Awaiter, _003CRunPreplacedLevelAsync_003Ed__9>(ref val, ref this);
						return;
					}
					goto IL_008e;
				case 0:
					val = _003C_003Eu__1;
					_003C_003Eu__1 = default(Awaiter);
					num = (_003C_003E1__state = -1);
					goto IL_008e;
				case 1:
					val = _003C_003Eu__1;
					_003C_003Eu__1 = default(Awaiter);
					num = (_003C_003E1__state = -1);
					goto IL_0121;
				case 2:
					val = _003C_003Eu__1;
					_003C_003Eu__1 = default(Awaiter);
					num = (_003C_003E1__state = -1);
					goto IL_01b0;
				case 3:
					{
						val = _003C_003Eu__1;
						_003C_003Eu__1 = default(Awaiter);
						num = (_003C_003E1__state = -1);
						break;
					}
					IL_0121:
					((Awaiter)(ref val)).GetResult();
					if (!ctx.Aborted && !cancellationToken.IsCancellationRequested)
					{
						val2 = levelEnterPipelineService.ExecuteStepAsync(levelEnterPipelineService.GetStep<StartSceneEmbeddedWeakGuideStep>(), ctx, cancellationToken);
						val = ((UniTask)(ref val2)).GetAwaiter();
						if (!((Awaiter)(ref val)).IsCompleted)
						{
							num = (_003C_003E1__state = 2);
							_003C_003Eu__1 = val;
							((AsyncUniTaskMethodBuilder)(ref _003C_003Et__builder)).AwaitUnsafeOnCompleted<Awaiter, _003CRunPreplacedLevelAsync_003Ed__9>(ref val, ref this);
							return;
						}
						goto IL_01b0;
					}
					goto end_IL_000e;
					IL_008e:
					((Awaiter)(ref val)).GetResult();
					if (!ctx.Aborted && !cancellationToken.IsCancellationRequested)
					{
						val2 = levelEnterPipelineService.RunPipelinePriorityRangeAsync(146, 210, ctx, cancellationToken);
						val = ((UniTask)(ref val2)).GetAwaiter();
						if (!((Awaiter)(ref val)).IsCompleted)
						{
							num = (_003C_003E1__state = 1);
							_003C_003Eu__1 = val;
							((AsyncUniTaskMethodBuilder)(ref _003C_003Et__builder)).AwaitUnsafeOnCompleted<Awaiter, _003CRunPreplacedLevelAsync_003Ed__9>(ref val, ref this);
							return;
						}
						goto IL_0121;
					}
					goto end_IL_000e;
					IL_01b0:
					((Awaiter)(ref val)).GetResult();
					val2 = levelEnterPipelineService.ExecuteStepAsync(levelEnterPipelineService.GetStep<InitInterlockWhitelistStep>(), ctx, cancellationToken);
					val = ((UniTask)(ref val2)).GetAwaiter();
					if (!((Awaiter)(ref val)).IsCompleted)
					{
						num = (_003C_003E1__state = 3);
						_003C_003Eu__1 = val;
						((AsyncUniTaskMethodBuilder)(ref _003C_003Et__builder)).AwaitUnsafeOnCompleted<Awaiter, _003CRunPreplacedLevelAsync_003Ed__9>(ref val, ref this);
						return;
					}
					break;
				}
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

	private const string Tag = "[LevelEnterPipeline] ";

	private readonly Dictionary<Type, ILevelEnterPipelineStep> stepRegistry = new Dictionary<Type, ILevelEnterPipelineStep>();

	private readonly List<ILevelEnterPipelineStep> runPipeline = new List<ILevelEnterPipelineStep>();

	public LevelEnterPipelineService(GameplayManager host)
		: base(host)
	{
		RegisterStep(new LevelLoadingStartedEventStep());
		RegisterStep(new PrepareLevelLoadStep());
		RegisterStep(new ClearLevelAndWaitFrameStep());
		RegisterStep(new LoadLevelPrefabStep());
		RegisterStep(new ValidateLoadedLevelPrefabStep());
		RegisterStep(new ResetLevelStartPipelineStateStep());
		RegisterStep(new InstantiateLevelRootStep());
		RegisterStep(new CollectLevelGameplayObjectsStep());
		RegisterStep(new ApplyLevelConfigAndStyleStep());
		RegisterStep(new ApplyLevelPartStyleStep());
		RegisterStep(new ApplyBoardFlipStep());
		RegisterStep(new PrepareLevelGameplayCameraStep());
		RegisterStep(new AdaptCoreLevelRenderStep());
		RegisterStep(new ResetLevelSessionStateStep());
		RegisterStep(new RegisterGameplaySolveHandlersStep());
		RegisterStep(new RegisterButterflySlotGuidesStep());
		RegisterStep(new RegisterLevelInteractablesStep());
		RegisterStep(new RebuildLevelManifestsAndScoreStep());
		RegisterStep(new ResetDragCollisionOnLevelStep());
		RegisterStep(new AttachLevelBombEffectsStep());
		RegisterStep(new AssignPaletteLockKeyColorsStep());
		RegisterStep(new ApplyLevelSaveStateStep());
		RegisterStep(new ApplyLevelPartDataStep());
		RegisterStep(new SpawnPaletteLockGapsStep());
		RegisterStep(new NotifyLevelLoadedStep());
		RegisterStep(new PreEnterPipelineStep());
		RegisterStep(new LevelStepLimitInitStep());
		RegisterStep(new StartMainLevelStartPipelineStep());
		RegisterStep(new ShowLevelObjectStep());
		RegisterStep(new CollectLevelEnterObjectsStep());
		RegisterStep(new PlayLevelEnterAnimationStep());
		RegisterStep(new LevelEnterCompletedEventStep());
		RegisterStep(new StartSceneEmbeddedWeakGuideStep());
		RegisterStep(new ReportGameStartTrackStep());
		RegisterStep(new InitInterlockWhitelistStep());
		RegisterStep(new FinalizeLevelEnterPostStep());
		RegisterStep(new ReplayEntranceAnimationStep());
	}

	private void RegisterStep(ILevelEnterPipelineStep step)
	{
		stepRegistry[step.GetType()] = step;
		if (step.IncludeInRunPipeline)
		{
			runPipeline.Add(step);
			runPipeline.Sort((ILevelEnterPipelineStep a, ILevelEnterPipelineStep b) => a.Priority.CompareTo(b.Priority));
		}
	}

	public T GetStep<T>() where T : class, ILevelEnterPipelineStep
	{
		if (!stepRegistry.TryGetValue(typeof(T), out var value))
		{
			return null;
		}
		return value as T;
	}

	[AsyncStateMachine(typeof(_003CExecuteStepAsync_003Ed__6))]
	public UniTask ExecuteStepAsync(ILevelEnterPipelineStep step, LevelEnterPipelineContext ctx, CancellationToken cancellationToken)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		_003CExecuteStepAsync_003Ed__6 _003CExecuteStepAsync_003Ed__ = default(_003CExecuteStepAsync_003Ed__6);
		_003CExecuteStepAsync_003Ed__._003C_003Et__builder = AsyncUniTaskMethodBuilder.Create();
		_003CExecuteStepAsync_003Ed__.step = step;
		_003CExecuteStepAsync_003Ed__.ctx = ctx;
		_003CExecuteStepAsync_003Ed__.cancellationToken = cancellationToken;
		_003CExecuteStepAsync_003Ed__._003C_003E1__state = -1;
		((AsyncUniTaskMethodBuilder)(ref _003CExecuteStepAsync_003Ed__._003C_003Et__builder)).Start<_003CExecuteStepAsync_003Ed__6>(ref _003CExecuteStepAsync_003Ed__);
		return ((AsyncUniTaskMethodBuilder)(ref _003CExecuteStepAsync_003Ed__._003C_003Et__builder)).Task;
	}

	[AsyncStateMachine(typeof(_003CRunLoadPrefabPhaseAsync_003Ed__7))]
	public UniTask RunLoadPrefabPhaseAsync(LevelEnterPipelineContext ctx, CancellationToken cancellationToken)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		_003CRunLoadPrefabPhaseAsync_003Ed__7 _003CRunLoadPrefabPhaseAsync_003Ed__ = default(_003CRunLoadPrefabPhaseAsync_003Ed__7);
		_003CRunLoadPrefabPhaseAsync_003Ed__._003C_003Et__builder = AsyncUniTaskMethodBuilder.Create();
		_003CRunLoadPrefabPhaseAsync_003Ed__._003C_003E4__this = this;
		_003CRunLoadPrefabPhaseAsync_003Ed__.ctx = ctx;
		_003CRunLoadPrefabPhaseAsync_003Ed__.cancellationToken = cancellationToken;
		_003CRunLoadPrefabPhaseAsync_003Ed__._003C_003E1__state = -1;
		((AsyncUniTaskMethodBuilder)(ref _003CRunLoadPrefabPhaseAsync_003Ed__._003C_003Et__builder)).Start<_003CRunLoadPrefabPhaseAsync_003Ed__7>(ref _003CRunLoadPrefabPhaseAsync_003Ed__);
		return ((AsyncUniTaskMethodBuilder)(ref _003CRunLoadPrefabPhaseAsync_003Ed__._003C_003Et__builder)).Task;
	}

	[AsyncStateMachine(typeof(_003CRunLevelReadyPhaseAsync_003Ed__8))]
	public UniTask RunLevelReadyPhaseAsync(LevelEnterPipelineContext ctx, CancellationToken cancellationToken)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		_003CRunLevelReadyPhaseAsync_003Ed__8 _003CRunLevelReadyPhaseAsync_003Ed__ = default(_003CRunLevelReadyPhaseAsync_003Ed__8);
		_003CRunLevelReadyPhaseAsync_003Ed__._003C_003Et__builder = AsyncUniTaskMethodBuilder.Create();
		_003CRunLevelReadyPhaseAsync_003Ed__._003C_003E4__this = this;
		_003CRunLevelReadyPhaseAsync_003Ed__.ctx = ctx;
		_003CRunLevelReadyPhaseAsync_003Ed__.cancellationToken = cancellationToken;
		_003CRunLevelReadyPhaseAsync_003Ed__._003C_003E1__state = -1;
		((AsyncUniTaskMethodBuilder)(ref _003CRunLevelReadyPhaseAsync_003Ed__._003C_003Et__builder)).Start<_003CRunLevelReadyPhaseAsync_003Ed__8>(ref _003CRunLevelReadyPhaseAsync_003Ed__);
		return ((AsyncUniTaskMethodBuilder)(ref _003CRunLevelReadyPhaseAsync_003Ed__._003C_003Et__builder)).Task;
	}

	[AsyncStateMachine(typeof(_003CRunPreplacedLevelAsync_003Ed__9))]
	public UniTask RunPreplacedLevelAsync(LevelEnterPipelineContext ctx, CancellationToken cancellationToken)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		_003CRunPreplacedLevelAsync_003Ed__9 _003CRunPreplacedLevelAsync_003Ed__ = default(_003CRunPreplacedLevelAsync_003Ed__9);
		_003CRunPreplacedLevelAsync_003Ed__._003C_003Et__builder = AsyncUniTaskMethodBuilder.Create();
		_003CRunPreplacedLevelAsync_003Ed__._003C_003E4__this = this;
		_003CRunPreplacedLevelAsync_003Ed__.ctx = ctx;
		_003CRunPreplacedLevelAsync_003Ed__.cancellationToken = cancellationToken;
		_003CRunPreplacedLevelAsync_003Ed__._003C_003E1__state = -1;
		((AsyncUniTaskMethodBuilder)(ref _003CRunPreplacedLevelAsync_003Ed__._003C_003Et__builder)).Start<_003CRunPreplacedLevelAsync_003Ed__9>(ref _003CRunPreplacedLevelAsync_003Ed__);
		return ((AsyncUniTaskMethodBuilder)(ref _003CRunPreplacedLevelAsync_003Ed__._003C_003Et__builder)).Task;
	}

	[AsyncStateMachine(typeof(_003CRunAsync_003Ed__10))]
	public UniTask RunAsync(int level, bool releaseGroupOnClear, string loadSource, CancellationToken destroyCt)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		_003CRunAsync_003Ed__10 _003CRunAsync_003Ed__ = default(_003CRunAsync_003Ed__10);
		_003CRunAsync_003Ed__._003C_003Et__builder = AsyncUniTaskMethodBuilder.Create();
		_003CRunAsync_003Ed__._003C_003E4__this = this;
		_003CRunAsync_003Ed__.level = level;
		_003CRunAsync_003Ed__.releaseGroupOnClear = releaseGroupOnClear;
		_003CRunAsync_003Ed__.loadSource = loadSource;
		_003CRunAsync_003Ed__.destroyCt = destroyCt;
		_003CRunAsync_003Ed__._003C_003E1__state = -1;
		((AsyncUniTaskMethodBuilder)(ref _003CRunAsync_003Ed__._003C_003Et__builder)).Start<_003CRunAsync_003Ed__10>(ref _003CRunAsync_003Ed__);
		return ((AsyncUniTaskMethodBuilder)(ref _003CRunAsync_003Ed__._003C_003Et__builder)).Task;
	}

	[AsyncStateMachine(typeof(_003CRunPipelinePriorityRangeAsync_003Ed__11))]
	private UniTask RunPipelinePriorityRangeAsync(int minPriorityInclusive, int maxPriorityExclusive, LevelEnterPipelineContext ctx, CancellationToken cancellationToken)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		_003CRunPipelinePriorityRangeAsync_003Ed__11 _003CRunPipelinePriorityRangeAsync_003Ed__ = default(_003CRunPipelinePriorityRangeAsync_003Ed__11);
		_003CRunPipelinePriorityRangeAsync_003Ed__._003C_003Et__builder = AsyncUniTaskMethodBuilder.Create();
		_003CRunPipelinePriorityRangeAsync_003Ed__._003C_003E4__this = this;
		_003CRunPipelinePriorityRangeAsync_003Ed__.minPriorityInclusive = minPriorityInclusive;
		_003CRunPipelinePriorityRangeAsync_003Ed__.maxPriorityExclusive = maxPriorityExclusive;
		_003CRunPipelinePriorityRangeAsync_003Ed__.ctx = ctx;
		_003CRunPipelinePriorityRangeAsync_003Ed__.cancellationToken = cancellationToken;
		_003CRunPipelinePriorityRangeAsync_003Ed__._003C_003E1__state = -1;
		((AsyncUniTaskMethodBuilder)(ref _003CRunPipelinePriorityRangeAsync_003Ed__._003C_003Et__builder)).Start<_003CRunPipelinePriorityRangeAsync_003Ed__11>(ref _003CRunPipelinePriorityRangeAsync_003Ed__);
		return ((AsyncUniTaskMethodBuilder)(ref _003CRunPipelinePriorityRangeAsync_003Ed__._003C_003Et__builder)).Task;
	}

	[AsyncStateMachine(typeof(_003CExecutePipelineRangeAsync_003Ed__12))]
	private UniTask ExecutePipelineRangeAsync(int start, int end, LevelEnterPipelineContext ctx, CancellationToken destroyCt)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		_003CExecutePipelineRangeAsync_003Ed__12 _003CExecutePipelineRangeAsync_003Ed__ = default(_003CExecutePipelineRangeAsync_003Ed__12);
		_003CExecutePipelineRangeAsync_003Ed__._003C_003Et__builder = AsyncUniTaskMethodBuilder.Create();
		_003CExecutePipelineRangeAsync_003Ed__._003C_003E4__this = this;
		_003CExecutePipelineRangeAsync_003Ed__.start = start;
		_003CExecutePipelineRangeAsync_003Ed__.end = end;
		_003CExecutePipelineRangeAsync_003Ed__.ctx = ctx;
		_003CExecutePipelineRangeAsync_003Ed__.destroyCt = destroyCt;
		_003CExecutePipelineRangeAsync_003Ed__._003C_003E1__state = -1;
		((AsyncUniTaskMethodBuilder)(ref _003CExecutePipelineRangeAsync_003Ed__._003C_003Et__builder)).Start<_003CExecutePipelineRangeAsync_003Ed__12>(ref _003CExecutePipelineRangeAsync_003Ed__);
		return ((AsyncUniTaskMethodBuilder)(ref _003CExecutePipelineRangeAsync_003Ed__._003C_003Et__builder)).Task;
	}

	[AsyncStateMachine(typeof(_003CExecuteOneStepAsync_003Ed__13))]
	private static UniTask ExecuteOneStepAsync(ILevelEnterPipelineStep step, LevelEnterPipelineContext ctx, CancellationToken destroyCt)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		_003CExecuteOneStepAsync_003Ed__13 _003CExecuteOneStepAsync_003Ed__ = default(_003CExecuteOneStepAsync_003Ed__13);
		_003CExecuteOneStepAsync_003Ed__._003C_003Et__builder = AsyncUniTaskMethodBuilder.Create();
		_003CExecuteOneStepAsync_003Ed__.step = step;
		_003CExecuteOneStepAsync_003Ed__.ctx = ctx;
		_003CExecuteOneStepAsync_003Ed__.destroyCt = destroyCt;
		_003CExecuteOneStepAsync_003Ed__._003C_003E1__state = -1;
		((AsyncUniTaskMethodBuilder)(ref _003CExecuteOneStepAsync_003Ed__._003C_003Et__builder)).Start<_003CExecuteOneStepAsync_003Ed__13>(ref _003CExecuteOneStepAsync_003Ed__);
		return ((AsyncUniTaskMethodBuilder)(ref _003CExecuteOneStepAsync_003Ed__._003C_003Et__builder)).Task;
	}

	private int FindEnterPhaseStartIndex()
	{
		for (int i = 0; i < runPipeline.Count; i++)
		{
			if (runPipeline[i].Priority >= 210)
			{
				return i;
			}
		}
		return -1;
	}
}
