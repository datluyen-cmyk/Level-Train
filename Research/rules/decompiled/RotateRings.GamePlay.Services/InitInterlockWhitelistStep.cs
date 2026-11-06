using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.CompilerServices;
using UnityEngine;

namespace RotateRings.GamePlay.Services;

internal sealed class InitInterlockWhitelistStep : ILevelEnterPipelineStep
{
	[StructLayout(LayoutKind.Auto)]
	[CompilerGenerated]
	private struct _003CExecuteAsync_003Ed__4 : IAsyncStateMachine
	{
		public int _003C_003E1__state;

		public AsyncUniTaskMethodBuilder _003C_003Et__builder;

		public LevelEnterPipelineContext ctx;

		public InitInterlockWhitelistStep _003C_003E4__this;

		public CancellationToken ct;

		private GameplayManager _003Cmgr_003E5__2;

		private GameObject _003CtargetLevelObject_003E5__3;

		private Awaiter<bool> _003C_003Eu__1;

		private void MoveNext()
		{
			//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
			//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
			//IL_0142: Unknown result type (might be due to invalid IL or missing references)
			//IL_0147: Unknown result type (might be due to invalid IL or missing references)
			//IL_014f: Unknown result type (might be due to invalid IL or missing references)
			//IL_005e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0063: Unknown result type (might be due to invalid IL or missing references)
			//IL_0067: Unknown result type (might be due to invalid IL or missing references)
			//IL_006c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0070: Unknown result type (might be due to invalid IL or missing references)
			//IL_0075: Unknown result type (might be due to invalid IL or missing references)
			//IL_00fb: Unknown result type (might be due to invalid IL or missing references)
			//IL_0100: Unknown result type (might be due to invalid IL or missing references)
			//IL_0104: Unknown result type (might be due to invalid IL or missing references)
			//IL_0109: Unknown result type (might be due to invalid IL or missing references)
			//IL_010d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0112: Unknown result type (might be due to invalid IL or missing references)
			//IL_008a: Unknown result type (might be due to invalid IL or missing references)
			//IL_008c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0127: Unknown result type (might be due to invalid IL or missing references)
			//IL_0129: Unknown result type (might be due to invalid IL or missing references)
			int num = _003C_003E1__state;
			InitInterlockWhitelistStep initInterlockWhitelistStep = _003C_003E4__this;
			try
			{
				Awaiter<bool> val;
				if (num == 0)
				{
					val = _003C_003Eu__1;
					_003C_003Eu__1 = default(Awaiter<bool>);
					num = (_003C_003E1__state = -1);
					goto IL_00c1;
				}
				if (num == 1)
				{
					val = _003C_003Eu__1;
					_003C_003Eu__1 = default(Awaiter<bool>);
					num = (_003C_003E1__state = -1);
					goto IL_015e;
				}
				_003Cmgr_003E5__2 = ctx.Manager;
				_003CtargetLevelObject_003E5__3 = _003Cmgr_003E5__2.CurrentLevelObject;
				UniTask val2;
				if (initInterlockWhitelistStep.IsCurrentInterlockInitialization(_003Cmgr_003E5__2, _003CtargetLevelObject_003E5__3))
				{
					val2 = UniTask.Yield((PlayerLoopTiming)13, ct);
					val = ((UniTask)(ref val2)).SuppressCancellationThrow().GetAwaiter();
					if (!val.IsCompleted)
					{
						num = (_003C_003E1__state = 0);
						_003C_003Eu__1 = val;
						((AsyncUniTaskMethodBuilder)(ref _003C_003Et__builder)).AwaitUnsafeOnCompleted<Awaiter<bool>, _003CExecuteAsync_003Ed__4>(ref val, ref this);
						return;
					}
					goto IL_00c1;
				}
				goto end_IL_000e;
				IL_015e:
				if (!val.GetResult() && initInterlockWhitelistStep.IsCurrentInterlockInitialization(_003Cmgr_003E5__2, _003CtargetLevelObject_003E5__3))
				{
					RingLockPassiveService gameService = _003Cmgr_003E5__2.GetGameService<RingLockPassiveService>();
					GameLevelInfo component = _003CtargetLevelObject_003E5__3.GetComponent<GameLevelInfo>();
					gameService?.InitPassiveLockBindings(_003Cmgr_003E5__2.ObjectsService.Rings, _003Cmgr_003E5__2.ObjectsService.Pulls, _003Cmgr_003E5__2.ObjectsService.FixedObjects, component?.RingLinks, component?.GuideLinks);
					RingLockActiveService gameService2 = _003Cmgr_003E5__2.GetGameService<RingLockActiveService>();
					if (gameService2 != null)
					{
						if ((Object)(object)component != (Object)null)
						{
							gameService2.InitFromRingLinks(component.RingLinks);
						}
						else
						{
							gameService2.ClearLevel();
						}
					}
					_003Cmgr_003E5__2.LockChain?.SetupChainLockUiOnLevelLoaded();
					_003Cmgr_003E5__2.PaletteLock?.SetupKeyUiOnLevelLoaded();
					_003Cmgr_003E5__2.RefreshRingDragAreaLockZ();
					_003Cmgr_003E5__2.EnableLevelGameplayInput(ctx.MainCamera);
				}
				goto end_IL_000e;
				IL_00c1:
				if (!val.GetResult() && initInterlockWhitelistStep.IsCurrentInterlockInitialization(_003Cmgr_003E5__2, _003CtargetLevelObject_003E5__3))
				{
					Physics2D.SyncTransforms();
					_003Cmgr_003E5__2.PurgeDestroyedGameplayObjects();
					val2 = UniTask.Yield((PlayerLoopTiming)13, ct);
					val = ((UniTask)(ref val2)).SuppressCancellationThrow().GetAwaiter();
					if (!val.IsCompleted)
					{
						num = (_003C_003E1__state = 1);
						_003C_003Eu__1 = val;
						((AsyncUniTaskMethodBuilder)(ref _003C_003Et__builder)).AwaitUnsafeOnCompleted<Awaiter<bool>, _003CExecuteAsync_003Ed__4>(ref val, ref this);
						return;
					}
					goto IL_015e;
				}
				end_IL_000e:;
			}
			catch (Exception exception)
			{
				_003C_003E1__state = -2;
				_003Cmgr_003E5__2 = null;
				_003CtargetLevelObject_003E5__3 = null;
				((AsyncUniTaskMethodBuilder)(ref _003C_003Et__builder)).SetException(exception);
				return;
			}
			_003C_003E1__state = -2;
			_003Cmgr_003E5__2 = null;
			_003CtargetLevelObject_003E5__3 = null;
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

	public int Priority => 280;

	public bool AwaitCompletion => false;

	[AsyncStateMachine(typeof(_003CExecuteAsync_003Ed__4))]
	public UniTask ExecuteAsync(LevelEnterPipelineContext ctx, CancellationToken ct)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		_003CExecuteAsync_003Ed__4 _003CExecuteAsync_003Ed__ = default(_003CExecuteAsync_003Ed__4);
		_003CExecuteAsync_003Ed__._003C_003Et__builder = AsyncUniTaskMethodBuilder.Create();
		_003CExecuteAsync_003Ed__._003C_003E4__this = this;
		_003CExecuteAsync_003Ed__.ctx = ctx;
		_003CExecuteAsync_003Ed__.ct = ct;
		_003CExecuteAsync_003Ed__._003C_003E1__state = -1;
		((AsyncUniTaskMethodBuilder)(ref _003CExecuteAsync_003Ed__._003C_003Et__builder)).Start<_003CExecuteAsync_003Ed__4>(ref _003CExecuteAsync_003Ed__);
		return ((AsyncUniTaskMethodBuilder)(ref _003CExecuteAsync_003Ed__._003C_003Et__builder)).Task;
	}

	private bool IsCurrentInterlockInitialization(GameplayManager mgr, GameObject targetLevelObject)
	{
		if ((Object)(object)targetLevelObject != (Object)null)
		{
			return (Object)(object)targetLevelObject == (Object)(object)mgr.CurrentLevelObject;
		}
		return false;
	}
}
