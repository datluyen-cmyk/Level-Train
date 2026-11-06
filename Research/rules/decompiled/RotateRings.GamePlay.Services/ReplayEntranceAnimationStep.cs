using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.CompilerServices;
using Game.Input;
using UnityEngine;
using XingYun.Core.Singletons;

namespace RotateRings.GamePlay.Services;

internal sealed class ReplayEntranceAnimationStep : ILevelEnterPipelineStep
{
	[StructLayout(LayoutKind.Auto)]
	[CompilerGenerated]
	private struct _003CExecuteAsync_003Ed__4 : IAsyncStateMachine
	{
		public int _003C_003E1__state;

		public AsyncUniTaskMethodBuilder _003C_003Et__builder;

		public LevelEnterPipelineContext ctx;

		public CancellationToken ct;

		private GameplayManager _003Cmgr_003E5__2;

		private CancellationToken _003Csession_003E5__3;

		private Camera _003CmainCamera_003E5__4;

		private Awaiter _003C_003Eu__1;

		private void MoveNext()
		{
			//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
			//IL_00cf: Unknown result type (might be due to invalid IL or missing references)
			//IL_01a9: Unknown result type (might be due to invalid IL or missing references)
			//IL_01ae: Unknown result type (might be due to invalid IL or missing references)
			//IL_01b5: Unknown result type (might be due to invalid IL or missing references)
			//IL_016d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0172: Unknown result type (might be due to invalid IL or missing references)
			//IL_0176: Unknown result type (might be due to invalid IL or missing references)
			//IL_017b: Unknown result type (might be due to invalid IL or missing references)
			//IL_018f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0190: Unknown result type (might be due to invalid IL or missing references)
			//IL_0087: Unknown result type (might be due to invalid IL or missing references)
			//IL_008c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0090: Unknown result type (might be due to invalid IL or missing references)
			//IL_0095: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
			//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
			int num = _003C_003E1__state;
			try
			{
				Awaiter val;
				if (num == 0)
				{
					val = _003C_003Eu__1;
					_003C_003Eu__1 = default(Awaiter);
					num = (_003C_003E1__state = -1);
					goto IL_00de;
				}
				if (num == 1)
				{
					goto IL_0153;
				}
				_003Cmgr_003E5__2 = ctx.Manager;
				UniTask val2;
				if (!((Object)(object)_003Cmgr_003E5__2 == (Object)null) && _003Cmgr_003E5__2.LevelEnterService != null && !((Object)(object)_003Cmgr_003E5__2.CurrentLevelObject == (Object)null))
				{
					LevelEnterPipelineService gameService = _003Cmgr_003E5__2.GetGameService<LevelEnterPipelineService>();
					CollectLevelEnterObjectsStep collectLevelEnterObjectsStep = gameService?.GetStep<CollectLevelEnterObjectsStep>();
					if (collectLevelEnterObjectsStep != null)
					{
						val2 = gameService.ExecuteStepAsync(collectLevelEnterObjectsStep, ctx, ct);
						val = ((UniTask)(ref val2)).GetAwaiter();
						if (!((Awaiter)(ref val)).IsCompleted)
						{
							num = (_003C_003E1__state = 0);
							_003C_003Eu__1 = val;
							((AsyncUniTaskMethodBuilder)(ref _003C_003Et__builder)).AwaitUnsafeOnCompleted<Awaiter, _003CExecuteAsync_003Ed__4>(ref val, ref this);
							return;
						}
						goto IL_00de;
					}
				}
				goto end_IL_0007;
				IL_0153:
				try
				{
					if (num != 1)
					{
						val2 = _003Cmgr_003E5__2.LevelEnterService.PlayAsync(ctx.LevelEnterObjects);
						val = ((UniTask)(ref val2)).GetAwaiter();
						if (!((Awaiter)(ref val)).IsCompleted)
						{
							num = (_003C_003E1__state = 1);
							_003C_003Eu__1 = val;
							((AsyncUniTaskMethodBuilder)(ref _003C_003Et__builder)).AwaitUnsafeOnCompleted<Awaiter, _003CExecuteAsync_003Ed__4>(ref val, ref this);
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
				finally
				{
					if (num < 0)
					{
						_003Cmgr_003E5__2.IsLevelEnterPlaying = false;
					}
				}
				if (!_003Csession_003E5__3.IsCancellationRequested)
				{
					Singleton<GameInputManager>.Instance.EnableGameplayInput(_003CmainCamera_003E5__4);
				}
				goto end_IL_0007;
				IL_00de:
				((Awaiter)(ref val)).GetResult();
				if (ctx.LevelEnterObjects != null && ctx.LevelEnterObjects.Count != 0)
				{
					_003Csession_003E5__3 = _003Cmgr_003E5__2.LevelEnterService.BeginSession();
					ctx.LoadSessionToken = _003Csession_003E5__3;
					_003CmainCamera_003E5__4 = GameplayCameraManager.GameplayCamera;
					Singleton<GameInputManager>.Instance.DisableGameplayInput();
					_003Cmgr_003E5__2.IsLevelEnterPlaying = true;
					goto IL_0153;
				}
				end_IL_0007:;
			}
			catch (Exception exception)
			{
				_003C_003E1__state = -2;
				_003Cmgr_003E5__2 = null;
				_003Csession_003E5__3 = default(CancellationToken);
				_003CmainCamera_003E5__4 = null;
				((AsyncUniTaskMethodBuilder)(ref _003C_003Et__builder)).SetException(exception);
				return;
			}
			_003C_003E1__state = -2;
			_003Cmgr_003E5__2 = null;
			_003Csession_003E5__3 = default(CancellationToken);
			_003CmainCamera_003E5__4 = null;
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

	public bool IncludeInRunPipeline => false;

	public int Priority => 910;

	[AsyncStateMachine(typeof(_003CExecuteAsync_003Ed__4))]
	public UniTask ExecuteAsync(LevelEnterPipelineContext ctx, CancellationToken ct)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		_003CExecuteAsync_003Ed__4 _003CExecuteAsync_003Ed__ = default(_003CExecuteAsync_003Ed__4);
		_003CExecuteAsync_003Ed__._003C_003Et__builder = AsyncUniTaskMethodBuilder.Create();
		_003CExecuteAsync_003Ed__.ctx = ctx;
		_003CExecuteAsync_003Ed__.ct = ct;
		_003CExecuteAsync_003Ed__._003C_003E1__state = -1;
		((AsyncUniTaskMethodBuilder)(ref _003CExecuteAsync_003Ed__._003C_003Et__builder)).Start<_003CExecuteAsync_003Ed__4>(ref _003CExecuteAsync_003Ed__);
		return ((AsyncUniTaskMethodBuilder)(ref _003CExecuteAsync_003Ed__._003C_003Et__builder)).Task;
	}
}
