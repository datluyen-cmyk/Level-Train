using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.CompilerServices;
using UnityEngine;

namespace RotateRings.GamePlay.Services;

internal sealed class SpawnPaletteLockGapsStep : ILevelEnterPipelineStep
{
	[StructLayout(LayoutKind.Auto)]
	[CompilerGenerated]
	private struct _003CExecuteAsync_003Ed__3 : IAsyncStateMachine
	{
		public int _003C_003E1__state;

		public AsyncUniTaskMethodBuilder _003C_003Et__builder;

		public LevelEnterPipelineContext ctx;

		private Awaiter _003C_003Eu__1;

		private void MoveNext()
		{
			//IL_0078: Unknown result type (might be due to invalid IL or missing references)
			//IL_007d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0084: Unknown result type (might be due to invalid IL or missing references)
			//IL_003f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0044: Unknown result type (might be due to invalid IL or missing references)
			//IL_0048: Unknown result type (might be due to invalid IL or missing references)
			//IL_004d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0061: Unknown result type (might be due to invalid IL or missing references)
			//IL_0062: Unknown result type (might be due to invalid IL or missing references)
			int num = _003C_003E1__state;
			try
			{
				Awaiter val;
				if (num == 0)
				{
					val = _003C_003Eu__1;
					_003C_003Eu__1 = default(Awaiter);
					num = (_003C_003E1__state = -1);
					goto IL_0093;
				}
				GameplayManager manager = ctx.Manager;
				RingPaletteLockService paletteLock = manager.PaletteLock;
				if (paletteLock != null && !((Object)(object)manager.CurrentLevelObject == (Object)null))
				{
					UniTask val2 = paletteLock.SpawnForNewGameAsync(manager.ObjectsService.Rings);
					val = ((UniTask)(ref val2)).GetAwaiter();
					if (!((Awaiter)(ref val)).IsCompleted)
					{
						num = (_003C_003E1__state = 0);
						_003C_003Eu__1 = val;
						((AsyncUniTaskMethodBuilder)(ref _003C_003Et__builder)).AwaitUnsafeOnCompleted<Awaiter, _003CExecuteAsync_003Ed__3>(ref val, ref this);
						return;
					}
					goto IL_0093;
				}
				goto end_IL_0007;
				IL_0093:
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

	public int Priority => 147;

	public bool ShouldRun(LevelEnterPipelineContext ctx)
	{
		if (!ctx.Aborted)
		{
			return ctx.Manager.HasActiveLevelRoot;
		}
		return false;
	}

	[AsyncStateMachine(typeof(_003CExecuteAsync_003Ed__3))]
	public UniTask ExecuteAsync(LevelEnterPipelineContext ctx, CancellationToken ct)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		_003CExecuteAsync_003Ed__3 _003CExecuteAsync_003Ed__ = default(_003CExecuteAsync_003Ed__3);
		_003CExecuteAsync_003Ed__._003C_003Et__builder = AsyncUniTaskMethodBuilder.Create();
		_003CExecuteAsync_003Ed__.ctx = ctx;
		_003CExecuteAsync_003Ed__._003C_003E1__state = -1;
		((AsyncUniTaskMethodBuilder)(ref _003CExecuteAsync_003Ed__._003C_003Et__builder)).Start<_003CExecuteAsync_003Ed__3>(ref _003CExecuteAsync_003Ed__);
		return ((AsyncUniTaskMethodBuilder)(ref _003CExecuteAsync_003Ed__._003C_003Et__builder)).Task;
	}
}
