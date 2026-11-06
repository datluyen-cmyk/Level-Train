using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.CompilerServices;
using UniLog;

namespace RotateRings.GamePlay.Services;

public sealed class LevelVictorySettlementService : GameplayServiceBase<GameplayManager>
{
	[StructLayout(LayoutKind.Auto)]
	[CompilerGenerated]
	private struct _003CRunSettlementPipelineAsync_003Ed__9 : IAsyncStateMachine
	{
		public int _003C_003E1__state;

		public AsyncUniTaskVoidMethodBuilder _003C_003Et__builder;

		public LevelVictorySettlementService _003C_003E4__this;

		public CancellationToken ct;

		public float postSolveDelaySeconds;

		private Awaiter<bool> _003C_003Eu__1;

		private void MoveNext()
		{
			//IL_005f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0064: Unknown result type (might be due to invalid IL or missing references)
			//IL_006b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0115: Unknown result type (might be due to invalid IL or missing references)
			//IL_011a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0121: Unknown result type (might be due to invalid IL or missing references)
			//IL_0024: Unknown result type (might be due to invalid IL or missing references)
			//IL_0029: Unknown result type (might be due to invalid IL or missing references)
			//IL_002c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0031: Unknown result type (might be due to invalid IL or missing references)
			//IL_0045: Unknown result type (might be due to invalid IL or missing references)
			//IL_0046: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d1: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d6: Unknown result type (might be due to invalid IL or missing references)
			//IL_00da: Unknown result type (might be due to invalid IL or missing references)
			//IL_00df: Unknown result type (might be due to invalid IL or missing references)
			//IL_00e2: Unknown result type (might be due to invalid IL or missing references)
			//IL_00e7: Unknown result type (might be due to invalid IL or missing references)
			//IL_00fb: Unknown result type (might be due to invalid IL or missing references)
			//IL_00fc: Unknown result type (might be due to invalid IL or missing references)
			int num = _003C_003E1__state;
			LevelVictorySettlementService levelVictorySettlementService = _003C_003E4__this;
			try
			{
				_ = 1;
				try
				{
					Awaiter<bool> val;
					if (num != 0)
					{
						if (num == 1)
						{
							val = _003C_003Eu__1;
							_003C_003Eu__1 = default(Awaiter<bool>);
							num = (_003C_003E1__state = -1);
							goto IL_0130;
						}
						val = levelVictorySettlementService.WaitPreSettlementPresentationsAsync(ct).GetAwaiter();
						if (!val.IsCompleted)
						{
							num = (_003C_003E1__state = 0);
							_003C_003Eu__1 = val;
							((AsyncUniTaskVoidMethodBuilder)(ref _003C_003Et__builder)).AwaitUnsafeOnCompleted<Awaiter<bool>, _003CRunSettlementPipelineAsync_003Ed__9>(ref val, ref this);
							return;
						}
					}
					else
					{
						val = _003C_003Eu__1;
						_003C_003Eu__1 = default(Awaiter<bool>);
						num = (_003C_003E1__state = -1);
					}
					if (!val.GetResult() && !ct.IsCancellationRequested && levelVictorySettlementService.Host.ShouldAttemptVictorySettlement())
					{
						levelVictorySettlementService.Host.MarkVictorySettlementStarted();
						if (postSolveDelaySeconds > 0f)
						{
							UniTask val2 = UniTask.Delay(TimeSpan.FromSeconds(postSolveDelaySeconds), false, (PlayerLoopTiming)8, ct);
							val = ((UniTask)(ref val2)).SuppressCancellationThrow().GetAwaiter();
							if (!val.IsCompleted)
							{
								num = (_003C_003E1__state = 1);
								_003C_003Eu__1 = val;
								((AsyncUniTaskVoidMethodBuilder)(ref _003C_003Et__builder)).AwaitUnsafeOnCompleted<Awaiter<bool>, _003CRunSettlementPipelineAsync_003Ed__9>(ref val, ref this);
								return;
							}
							goto IL_0130;
						}
						goto IL_0148;
					}
					goto end_IL_0013;
					IL_0130:
					if (!val.GetResult() && !ct.IsCancellationRequested)
					{
						goto IL_0148;
					}
					goto end_IL_0013;
					IL_0148:
					levelVictorySettlementService.Host.RunVictorySettlementContinuation(ct);
					end_IL_0013:;
				}
				catch (Exception ex) when (!(ex is OperationCanceledException))
				{
					Log.LogError("[GamePlay][LevelVictorySettlement] ", "胜利结算管线异常: {0}", new object[1] { ex });
				}
				finally
				{
					if (num < 0)
					{
						levelVictorySettlementService.isPipelineRunning = false;
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
	private struct _003CWaitPreSettlementPresentationsAsync_003Ed__10 : IAsyncStateMachine
	{
		public int _003C_003E1__state;

		public AsyncUniTaskMethodBuilder<bool> _003C_003Et__builder;

		public LevelVictorySettlementService _003C_003E4__this;

		public CancellationToken ct;

		private Awaiter<bool> _003C_003Eu__1;

		private void MoveNext()
		{
			//IL_0066: Unknown result type (might be due to invalid IL or missing references)
			//IL_006b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0072: Unknown result type (might be due to invalid IL or missing references)
			//IL_0024: Unknown result type (might be due to invalid IL or missing references)
			//IL_0029: Unknown result type (might be due to invalid IL or missing references)
			//IL_002d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0032: Unknown result type (might be due to invalid IL or missing references)
			//IL_0036: Unknown result type (might be due to invalid IL or missing references)
			//IL_003b: Unknown result type (might be due to invalid IL or missing references)
			//IL_004f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0050: Unknown result type (might be due to invalid IL or missing references)
			int num = _003C_003E1__state;
			LevelVictorySettlementService CS_0024_003C_003E8__locals1 = _003C_003E4__this;
			bool result;
			try
			{
				Awaiter<bool> val2;
				if (num != 0)
				{
					UniTask val = UniTask.WaitUntil((Func<bool>)(() => !CS_0024_003C_003E8__locals1.HasBlockingKeyUnlockPresentation()), (PlayerLoopTiming)8, ct);
					val2 = ((UniTask)(ref val)).SuppressCancellationThrow().GetAwaiter();
					if (!val2.IsCompleted)
					{
						num = (_003C_003E1__state = 0);
						_003C_003Eu__1 = val2;
						_003C_003Et__builder.AwaitUnsafeOnCompleted<Awaiter<bool>, _003CWaitPreSettlementPresentationsAsync_003Ed__10>(ref val2, ref this);
						return;
					}
				}
				else
				{
					val2 = _003C_003Eu__1;
					_003C_003Eu__1 = default(Awaiter<bool>);
					num = (_003C_003E1__state = -1);
				}
				result = val2.GetResult();
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

	private const string Tag = "[GamePlay][LevelVictorySettlement] ";

	private CancellationTokenSource settlementCts;

	private bool isPipelineRunning;

	public LevelVictorySettlementService(GameplayManager host)
		: base(host)
	{
	}

	public override void ClearLevel()
	{
		CancelSettlementPipeline();
		base.ClearLevel();
	}

	public override void Dispose()
	{
		CancelSettlementPipeline();
		base.Dispose();
	}

	public void TryScheduleSettlement(float postSolveDelaySeconds)
	{
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		if (!base.IsDisposed && base.Host.ShouldAttemptVictorySettlement())
		{
			RingObjectSolveService solveService = base.Host.SolveService;
			if (solveService != null && solveService.UnsolvedCount == 0 && !isPipelineRunning)
			{
				isPipelineRunning = true;
				CancelAndRenewSettlementCts();
				UniTaskVoid val = RunSettlementPipelineAsync(postSolveDelaySeconds, settlementCts.Token);
				((UniTaskVoid)(ref val)).Forget();
			}
		}
	}

	private void CancelSettlementPipeline()
	{
		isPipelineRunning = false;
		if (settlementCts != null)
		{
			settlementCts.Cancel();
			settlementCts.Dispose();
			settlementCts = null;
		}
	}

	private void CancelAndRenewSettlementCts()
	{
		settlementCts?.Cancel();
		settlementCts?.Dispose();
		settlementCts = new CancellationTokenSource();
	}

	[AsyncStateMachine(typeof(_003CRunSettlementPipelineAsync_003Ed__9))]
	private UniTaskVoid RunSettlementPipelineAsync(float postSolveDelaySeconds, CancellationToken ct)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		_003CRunSettlementPipelineAsync_003Ed__9 _003CRunSettlementPipelineAsync_003Ed__ = default(_003CRunSettlementPipelineAsync_003Ed__9);
		_003CRunSettlementPipelineAsync_003Ed__._003C_003Et__builder = AsyncUniTaskVoidMethodBuilder.Create();
		_003CRunSettlementPipelineAsync_003Ed__._003C_003E4__this = this;
		_003CRunSettlementPipelineAsync_003Ed__.postSolveDelaySeconds = postSolveDelaySeconds;
		_003CRunSettlementPipelineAsync_003Ed__.ct = ct;
		_003CRunSettlementPipelineAsync_003Ed__._003C_003E1__state = -1;
		((AsyncUniTaskVoidMethodBuilder)(ref _003CRunSettlementPipelineAsync_003Ed__._003C_003Et__builder)).Start<_003CRunSettlementPipelineAsync_003Ed__9>(ref _003CRunSettlementPipelineAsync_003Ed__);
		return ((AsyncUniTaskVoidMethodBuilder)(ref _003CRunSettlementPipelineAsync_003Ed__._003C_003Et__builder)).Task;
	}

	[AsyncStateMachine(typeof(_003CWaitPreSettlementPresentationsAsync_003Ed__10))]
	private UniTask<bool> WaitPreSettlementPresentationsAsync(CancellationToken ct)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		_003CWaitPreSettlementPresentationsAsync_003Ed__10 _003CWaitPreSettlementPresentationsAsync_003Ed__ = default(_003CWaitPreSettlementPresentationsAsync_003Ed__10);
		_003CWaitPreSettlementPresentationsAsync_003Ed__._003C_003Et__builder = AsyncUniTaskMethodBuilder<bool>.Create();
		_003CWaitPreSettlementPresentationsAsync_003Ed__._003C_003E4__this = this;
		_003CWaitPreSettlementPresentationsAsync_003Ed__.ct = ct;
		_003CWaitPreSettlementPresentationsAsync_003Ed__._003C_003E1__state = -1;
		_003CWaitPreSettlementPresentationsAsync_003Ed__._003C_003Et__builder.Start<_003CWaitPreSettlementPresentationsAsync_003Ed__10>(ref _003CWaitPreSettlementPresentationsAsync_003Ed__);
		return _003CWaitPreSettlementPresentationsAsync_003Ed__._003C_003Et__builder.Task;
	}

	private bool HasBlockingKeyUnlockPresentation()
	{
		return base.Host.PaletteLock?.HasActiveKeyUnlockPresentation ?? false;
	}
}
