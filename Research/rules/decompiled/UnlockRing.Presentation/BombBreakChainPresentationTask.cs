using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.CompilerServices;
using RotateRings.GamePlay.Services;
using RotateRings.GamePlay.Services.CharacterExpression;
using UniLog;
using UnityEngine;

namespace UnlockRing.Presentation;

public sealed class BombBreakChainPresentationTask : PresentationTaskBase
{
	[StructLayout(LayoutKind.Auto)]
	[CompilerGenerated]
	private struct _003CPlayBombBoomAsync_003Ed__8 : IAsyncStateMachine
	{
		public int _003C_003E1__state;

		public AsyncUniTaskMethodBuilder _003C_003Et__builder;

		public BombBreakChainPresentationTask _003C_003E4__this;

		public CancellationToken cancellationToken;

		private Awaiter<bool> _003C_003Eu__1;

		private void MoveNext()
		{
			//IL_00fe: Unknown result type (might be due to invalid IL or missing references)
			//IL_0103: Unknown result type (might be due to invalid IL or missing references)
			//IL_010b: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
			//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
			//IL_00cc: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d1: Unknown result type (might be due to invalid IL or missing references)
			//IL_00e6: Unknown result type (might be due to invalid IL or missing references)
			//IL_00e8: Unknown result type (might be due to invalid IL or missing references)
			int num = _003C_003E1__state;
			BombBreakChainPresentationTask bombBreakChainPresentationTask = _003C_003E4__this;
			try
			{
				Awaiter<bool> val;
				if (num == 0)
				{
					val = _003C_003Eu__1;
					_003C_003Eu__1 = default(Awaiter<bool>);
					num = (_003C_003E1__state = -1);
					goto IL_011a;
				}
				if (PresentationMode.UseTaskPresentation)
				{
					IReadOnlyList<BombBoomResult> readOnlyList = bombBreakChainPresentationTask.FindResults<BombBoomResult>();
					if (readOnlyList.Count != 0 && bombBreakChainPresentationTask.bombBoomService != null)
					{
						float num2 = 0f;
						for (int i = 0; i < readOnlyList.Count; i++)
						{
							BombBoomResult bombBoomResult = readOnlyList[i];
							if (!((Object)(object)bombBoomResult?.Bomb == (Object)null))
							{
								bombBreakChainPresentationTask.bombBoomService.PlayBoom(bombBoomResult.Bomb);
								if (bombBreakChainPresentationTask.bombBoomService.TargetBreakDelay > num2)
								{
									num2 = bombBreakChainPresentationTask.bombBoomService.TargetBreakDelay;
								}
							}
						}
						if (num2 > 0f)
						{
							UniTask val2 = UniTask.Delay(TimeSpan.FromSeconds(num2), false, (PlayerLoopTiming)8, cancellationToken);
							val = ((UniTask)(ref val2)).SuppressCancellationThrow().GetAwaiter();
							if (!val.IsCompleted)
							{
								num = (_003C_003E1__state = 0);
								_003C_003Eu__1 = val;
								((AsyncUniTaskMethodBuilder)(ref _003C_003Et__builder)).AwaitUnsafeOnCompleted<Awaiter<bool>, _003CPlayBombBoomAsync_003Ed__8>(ref val, ref this);
								return;
							}
							goto IL_011a;
						}
					}
				}
				goto end_IL_000e;
				IL_011a:
				val.GetResult();
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
	private struct _003CPlayRingBreaksAsync_003Ed__9 : IAsyncStateMachine
	{
		public int _003C_003E1__state;

		public AsyncUniTaskMethodBuilder _003C_003Et__builder;

		public BombBreakChainPresentationTask _003C_003E4__this;

		public CancellationToken cancellationToken;

		private IReadOnlyList<RingBreakResult> _003CringResults_003E5__2;

		private int _003Ci_003E5__3;

		private Awaiter _003C_003Eu__1;

		private void MoveNext()
		{
			//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c9: Unknown result type (might be due to invalid IL or missing references)
			//IL_0082: Unknown result type (might be due to invalid IL or missing references)
			//IL_0087: Unknown result type (might be due to invalid IL or missing references)
			//IL_008a: Unknown result type (might be due to invalid IL or missing references)
			//IL_008f: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
			int num = _003C_003E1__state;
			BombBreakChainPresentationTask bombBreakChainPresentationTask = _003C_003E4__this;
			try
			{
				if (num == 0)
				{
					goto IL_0062;
				}
				if (PresentationMode.UseTaskPresentation)
				{
					_003CringResults_003E5__2 = bombBreakChainPresentationTask.FindResults<RingBreakResult>();
					if (_003CringResults_003E5__2.Count != 0 && bombBreakChainPresentationTask.solveSuccessEffectService != null)
					{
						_003Ci_003E5__3 = 0;
						goto IL_0111;
					}
				}
				goto end_IL_000e;
				IL_0111:
				if (_003Ci_003E5__3 < _003CringResults_003E5__2.Count && !cancellationToken.IsCancellationRequested)
				{
					goto IL_0062;
				}
				goto end_IL_000e;
				IL_0062:
				try
				{
					Awaiter val2;
					if (num != 0)
					{
						UniTask val = bombBreakChainPresentationTask.solveSuccessEffectService.PlayBreakFromResultAsync(_003CringResults_003E5__2[_003Ci_003E5__3], cancellationToken);
						val2 = ((UniTask)(ref val)).GetAwaiter();
						if (!((Awaiter)(ref val2)).IsCompleted)
						{
							num = (_003C_003E1__state = 0);
							_003C_003Eu__1 = val2;
							((AsyncUniTaskMethodBuilder)(ref _003C_003Et__builder)).AwaitUnsafeOnCompleted<Awaiter, _003CPlayRingBreaksAsync_003Ed__9>(ref val2, ref this);
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
				catch (Exception ex)
				{
					Log.LogError("[Presentation][BombBreakChainTask] ", "碎环播放异常: {0}", new object[1] { ex });
				}
				_003Ci_003E5__3++;
				goto IL_0111;
				end_IL_000e:;
			}
			catch (Exception exception)
			{
				_003C_003E1__state = -2;
				_003CringResults_003E5__2 = null;
				((AsyncUniTaskMethodBuilder)(ref _003C_003Et__builder)).SetException(exception);
				return;
			}
			_003C_003E1__state = -2;
			_003CringResults_003E5__2 = null;
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

	private const string TAG = "[Presentation][BombBreakChainTask] ";

	private readonly IReadOnlyList<IPresentationResult> results;

	private readonly RingBoomService bombBoomService;

	private readonly RingSolveSuccessEffectService solveSuccessEffectService;

	private readonly RotateRingOperationScoreFacade scoreFacade;

	private readonly CharacterExpressionEncourageService expressionService;

	public BombBreakChainPresentationTask(string debugName, RingBoomService bombBoomService, RingSolveSuccessEffectService solveSuccessEffectService, RotateRingOperationScoreFacade scoreFacade, CharacterExpressionEncourageService expressionService, IReadOnlyList<IPresentationResult> results)
		: base(debugName, PresentationTaskKind.Serial)
	{
		this.bombBoomService = bombBoomService;
		this.solveSuccessEffectService = solveSuccessEffectService;
		this.scoreFacade = scoreFacade;
		this.expressionService = expressionService;
		this.results = results;
		ComposeSteps();
	}

	private void ComposeSteps()
	{
		Then(PlayBombBoomAsync);
		Then(PlayRingBreaksAsync);
		Then(PlayComboEncouragementsAsync);
		Then(PlayExpressionEncouragementsAsync);
	}

	[AsyncStateMachine(typeof(_003CPlayBombBoomAsync_003Ed__8))]
	private UniTask PlayBombBoomAsync(CancellationToken cancellationToken)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		_003CPlayBombBoomAsync_003Ed__8 _003CPlayBombBoomAsync_003Ed__ = default(_003CPlayBombBoomAsync_003Ed__8);
		_003CPlayBombBoomAsync_003Ed__._003C_003Et__builder = AsyncUniTaskMethodBuilder.Create();
		_003CPlayBombBoomAsync_003Ed__._003C_003E4__this = this;
		_003CPlayBombBoomAsync_003Ed__.cancellationToken = cancellationToken;
		_003CPlayBombBoomAsync_003Ed__._003C_003E1__state = -1;
		((AsyncUniTaskMethodBuilder)(ref _003CPlayBombBoomAsync_003Ed__._003C_003Et__builder)).Start<_003CPlayBombBoomAsync_003Ed__8>(ref _003CPlayBombBoomAsync_003Ed__);
		return ((AsyncUniTaskMethodBuilder)(ref _003CPlayBombBoomAsync_003Ed__._003C_003Et__builder)).Task;
	}

	[AsyncStateMachine(typeof(_003CPlayRingBreaksAsync_003Ed__9))]
	private UniTask PlayRingBreaksAsync(CancellationToken cancellationToken)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		_003CPlayRingBreaksAsync_003Ed__9 _003CPlayRingBreaksAsync_003Ed__ = default(_003CPlayRingBreaksAsync_003Ed__9);
		_003CPlayRingBreaksAsync_003Ed__._003C_003Et__builder = AsyncUniTaskMethodBuilder.Create();
		_003CPlayRingBreaksAsync_003Ed__._003C_003E4__this = this;
		_003CPlayRingBreaksAsync_003Ed__.cancellationToken = cancellationToken;
		_003CPlayRingBreaksAsync_003Ed__._003C_003E1__state = -1;
		((AsyncUniTaskMethodBuilder)(ref _003CPlayRingBreaksAsync_003Ed__._003C_003Et__builder)).Start<_003CPlayRingBreaksAsync_003Ed__9>(ref _003CPlayRingBreaksAsync_003Ed__);
		return ((AsyncUniTaskMethodBuilder)(ref _003CPlayRingBreaksAsync_003Ed__._003C_003Et__builder)).Task;
	}

	private UniTask PlayComboEncouragementsAsync(CancellationToken cancellationToken)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		if (!PresentationMode.UseTaskPresentation)
		{
			return UniTask.CompletedTask;
		}
		IReadOnlyList<ComboEncouragementResult> readOnlyList = FindResults<ComboEncouragementResult>();
		if (readOnlyList.Count == 0 || scoreFacade == null)
		{
			return UniTask.CompletedTask;
		}
		for (int i = 0; i < readOnlyList.Count; i++)
		{
			ComboEncouragementResult comboEncouragementResult = readOnlyList[i];
			if (comboEncouragementResult.Evaluation != GameplayOperationEvaluation.None)
			{
				scoreFacade.PlayEncouragementFromResult(comboEncouragementResult.Evaluation, comboEncouragementResult.WorldPosition);
			}
		}
		return UniTask.CompletedTask;
	}

	private UniTask PlayExpressionEncouragementsAsync(CancellationToken cancellationToken)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		if (!PresentationMode.UseTaskPresentation)
		{
			return UniTask.CompletedTask;
		}
		IReadOnlyList<CharacterExpressionEncouragementResult> readOnlyList = FindResults<CharacterExpressionEncouragementResult>();
		if (readOnlyList.Count == 0 || expressionService == null)
		{
			return UniTask.CompletedTask;
		}
		for (int i = 0; i < readOnlyList.Count; i++)
		{
			CharacterExpressionEncouragementResult characterExpressionEncouragementResult = readOnlyList[i];
			if (characterExpressionEncouragementResult.Conditions != null && characterExpressionEncouragementResult.Conditions.Count != 0)
			{
				expressionService.PlayExpressionFromResult(characterExpressionEncouragementResult.EffectiveFeedback, characterExpressionEncouragementResult.WorldPosition);
			}
		}
		return UniTask.CompletedTask;
	}

	private TResult FindResult<TResult>() where TResult : class, IPresentationResult
	{
		if (results == null)
		{
			return null;
		}
		for (int i = 0; i < results.Count; i++)
		{
			if (results[i] is TResult result)
			{
				return result;
			}
		}
		return null;
	}

	private IReadOnlyList<TResult> FindResults<TResult>() where TResult : class, IPresentationResult
	{
		List<TResult> list = new List<TResult>();
		if (results == null)
		{
			return list;
		}
		for (int i = 0; i < results.Count; i++)
		{
			if (results[i] is TResult item)
			{
				list.Add(item);
			}
		}
		return list;
	}
}
