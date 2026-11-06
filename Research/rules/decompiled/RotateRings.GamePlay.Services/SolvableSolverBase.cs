using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.CompilerServices;
using RotateRings.GamePlay.Events;
using UnityEngine;
using UnlockRing;
using UnlockRing.Core.Domain;
using UnlockRing.Presentation;

namespace RotateRings.GamePlay.Services;

public abstract class SolvableSolverBase<T> : ISolvableSolver where T : class, IGameplayObject
{
	[StructLayout(LayoutKind.Auto)]
	[CompilerGenerated]
	private struct _003CWaitBreakThenDispatchRemoveAsync_003Ed__9 : IAsyncStateMachine
	{
		public int _003C_003E1__state;

		public AsyncUniTaskVoidMethodBuilder _003C_003Et__builder;

		public GameObject partGo;

		public SolvableSolverBase<T> _003C_003E4__this;

		public RingAfterSolveEvent evt;

		public IGameplayObject sourcePart;

		private RingSolveBreakDelayService _003Cdelay_003E5__2;

		private Awaiter _003C_003Eu__1;

		private void MoveNext()
		{
			//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
			//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c4: Unknown result type (might be due to invalid IL or missing references)
			//IL_014e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0153: Unknown result type (might be due to invalid IL or missing references)
			//IL_015a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0113: Unknown result type (might be due to invalid IL or missing references)
			//IL_0118: Unknown result type (might be due to invalid IL or missing references)
			//IL_011b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0120: Unknown result type (might be due to invalid IL or missing references)
			//IL_0134: Unknown result type (might be due to invalid IL or missing references)
			//IL_0135: Unknown result type (might be due to invalid IL or missing references)
			//IL_007d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0082: Unknown result type (might be due to invalid IL or missing references)
			//IL_0085: Unknown result type (might be due to invalid IL or missing references)
			//IL_008a: Unknown result type (might be due to invalid IL or missing references)
			//IL_009e: Unknown result type (might be due to invalid IL or missing references)
			//IL_009f: Unknown result type (might be due to invalid IL or missing references)
			int num = _003C_003E1__state;
			SolvableSolverBase<T> solvableSolverBase = _003C_003E4__this;
			try
			{
				Awaiter val;
				if (num == 0)
				{
					val = _003C_003Eu__1;
					_003C_003Eu__1 = default(Awaiter);
					num = (_003C_003E1__state = -1);
					goto IL_00d3;
				}
				if (num == 1)
				{
					val = _003C_003Eu__1;
					_003C_003Eu__1 = default(Awaiter);
					num = (_003C_003E1__state = -1);
					goto IL_0169;
				}
				UniTask val2;
				if (!PresentationMode.UseTaskPresentation)
				{
					_003Cdelay_003E5__2 = SingletonComponent<GameplayManager>.Instance?.SolveBreakDelay;
					if (_003Cdelay_003E5__2 != null)
					{
						val2 = _003Cdelay_003E5__2.WaitBreakReadyGateAsync(evt);
						val = ((UniTask)(ref val2)).GetAwaiter();
						if (!((Awaiter)(ref val)).IsCompleted)
						{
							num = (_003C_003E1__state = 0);
							_003C_003Eu__1 = val;
							((AsyncUniTaskVoidMethodBuilder)(ref _003C_003Et__builder)).AwaitUnsafeOnCompleted<Awaiter, _003CWaitBreakThenDispatchRemoveAsync_003Ed__9>(ref val, ref this);
							return;
						}
						goto IL_00d3;
					}
					goto IL_00da;
				}
				if ((Object)(object)partGo != (Object)null)
				{
					solvableSolverBase.DispatchRemoveEffect(partGo, evt.IsFromBomb, sourcePart);
				}
				goto end_IL_000e;
				IL_0170:
				if ((Object)(object)partGo != (Object)null)
				{
					solvableSolverBase.DispatchRemoveEffect(partGo, isFromBomb: false, sourcePart);
				}
				goto end_IL_000e;
				IL_00d3:
				((Awaiter)(ref val)).GetResult();
				goto IL_00da;
				IL_00da:
				GameApp.Event.Send<RingBreakReadyEvent>(new RingBreakReadyEvent(evt.GameplayObject, evt.IsFromBomb));
				if (_003Cdelay_003E5__2 != null)
				{
					val2 = _003Cdelay_003E5__2.WaitRemoveDelayAsync(evt);
					val = ((UniTask)(ref val2)).GetAwaiter();
					if (!((Awaiter)(ref val)).IsCompleted)
					{
						num = (_003C_003E1__state = 1);
						_003C_003Eu__1 = val;
						((AsyncUniTaskVoidMethodBuilder)(ref _003C_003Et__builder)).AwaitUnsafeOnCompleted<Awaiter, _003CWaitBreakThenDispatchRemoveAsync_003Ed__9>(ref val, ref this);
						return;
					}
					goto IL_0169;
				}
				goto IL_0170;
				IL_0169:
				((Awaiter)(ref val)).GetResult();
				goto IL_0170;
				end_IL_000e:;
			}
			catch (Exception exception)
			{
				_003C_003E1__state = -2;
				_003Cdelay_003E5__2 = null;
				((AsyncUniTaskVoidMethodBuilder)(ref _003C_003Et__builder)).SetException(exception);
				return;
			}
			_003C_003E1__state = -2;
			_003Cdelay_003E5__2 = null;
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

	private const string SolveReasonLogTag = "[GamePlay][SolveReason] ";

	bool ISolvableSolver.CanSolve(IGameplayObject obj)
	{
		if (obj is T obj2)
		{
			return CanSolve(obj2);
		}
		return false;
	}

	void ISolvableSolver.Solve(IGameplayObject obj, BombRelative sourceBomb = null, GameplaySolveReasonContext releaseContext = default(GameplaySolveReasonContext))
	{
		if (obj is T obj2)
		{
			Solve(obj2, sourceBomb, releaseContext);
		}
	}

	public abstract bool CanSolve(T obj);

	public void Solve(T obj, BombRelative sourceBomb = null, GameplaySolveReasonContext releaseContext = default(GameplaySolveReasonContext))
	{
		//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
		GameplayManager instance = SingletonComponent<GameplayManager>.Instance;
		if ((instance == null || !instance.IsLevelEnterPlaying) && !obj.IsSolved)
		{
			bool isFromBomb = (Object)(object)sourceBomb != (Object)null;
			GameApp.Event.Send<RingPreSolveEvent>(new RingPreSolveEvent(obj, isFromBomb));
			GameplaySolveReasonContext reasonContext = releaseContext.WithDefaultReasonIfNone(sourceBomb);
			ApplySolveReason(obj, reasonContext);
			MarkSolved(obj);
			Component val = (Component)(object)((obj is Component) ? obj : null);
			if (val != null)
			{
				GameplayGuideRtLayerExclusion.ExcludeFromGuideRender(val.transform);
			}
			OnPreSolve(obj, isFromBomb);
			SolveRingRelatives(obj);
			GameObject partGameObject = GetPartGameObject(obj);
			RingAfterSolveEvent ringAfterSolveEvent = new RingAfterSolveEvent(obj, isFromBomb);
			GameApp.Event.Send<RingAfterSolveEvent>(ringAfterSolveEvent);
			UniTaskVoid val2 = WaitBreakThenDispatchRemoveAsync(ringAfterSolveEvent, partGameObject, obj);
			((UniTaskVoid)(ref val2)).Forget();
			OnPostSolve(obj);
		}
	}

	protected abstract void MarkSolved(T obj);

	protected abstract void OnPreSolve(T obj, bool isFromBomb);

	protected virtual void SolveRingRelatives(T obj)
	{
	}

	protected abstract void OnPostSolve(T obj);

	protected abstract GameObject GetPartGameObject(T obj);

	[AsyncStateMachine(typeof(SolvableSolverBase<>._003CWaitBreakThenDispatchRemoveAsync_003Ed__9))]
	private UniTaskVoid WaitBreakThenDispatchRemoveAsync(RingAfterSolveEvent evt, GameObject partGo, IGameplayObject sourcePart)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		_003CWaitBreakThenDispatchRemoveAsync_003Ed__9 _003CWaitBreakThenDispatchRemoveAsync_003Ed__ = default(_003CWaitBreakThenDispatchRemoveAsync_003Ed__9);
		_003CWaitBreakThenDispatchRemoveAsync_003Ed__._003C_003Et__builder = AsyncUniTaskVoidMethodBuilder.Create();
		_003CWaitBreakThenDispatchRemoveAsync_003Ed__._003C_003E4__this = this;
		_003CWaitBreakThenDispatchRemoveAsync_003Ed__.evt = evt;
		_003CWaitBreakThenDispatchRemoveAsync_003Ed__.partGo = partGo;
		_003CWaitBreakThenDispatchRemoveAsync_003Ed__.sourcePart = sourcePart;
		_003CWaitBreakThenDispatchRemoveAsync_003Ed__._003C_003E1__state = -1;
		((AsyncUniTaskVoidMethodBuilder)(ref _003CWaitBreakThenDispatchRemoveAsync_003Ed__._003C_003Et__builder)).Start<_003CWaitBreakThenDispatchRemoveAsync_003Ed__9>(ref _003CWaitBreakThenDispatchRemoveAsync_003Ed__);
		return ((AsyncUniTaskVoidMethodBuilder)(ref _003CWaitBreakThenDispatchRemoveAsync_003Ed__._003C_003Et__builder)).Task;
	}

	private void DispatchRemoveEffect(GameObject partGo, bool isFromBomb, IGameplayObject sourcePart)
	{
		Mesh val = null;
		MeshFilter val2 = partGo.GetComponent<MeshFilter>() ?? partGo.GetComponentInChildren<MeshFilter>();
		if ((Object)(object)val2 != (Object)null)
		{
			val = val2.sharedMesh;
		}
		if ((Object)(object)val == (Object)null)
		{
			if ((Object)(object)partGo != (Object)null && Application.isPlaying)
			{
				Object.Destroy((Object)(object)partGo);
			}
			return;
		}
		RingRemoveEffectEvent ringRemoveEffectEvent = new RingRemoveEffectEvent
		{
			SourceMesh = val,
			SourceTransform = partGo.transform,
			PartGo = partGo,
			EffectParent = partGo.transform.parent,
			SourcePart = sourcePart,
			IsFromBomb = isFromBomb
		};
		GameApp.Event.Send<RingRemoveEffectEvent>(ringRemoveEffectEvent);
		if (!ringRemoveEffectEvent.Handled && (Object)(object)partGo != (Object)null && Application.isPlaying)
		{
			Object.Destroy((Object)(object)partGo);
		}
	}

	private void ApplySolveReason(T obj, GameplaySolveReasonContext reasonContext)
	{
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		if (obj is IGameplaySolveReason gameplaySolveReason)
		{
			GameplaySolveReason reason = reasonContext.Reason;
			PartId passiveByPartId = (PartId)(reason.IsPassive() ? reasonContext.PassiveByPartId : default(PartId));
			PartId stepBatchPartId = ResolveStepBatchPartId(obj, reason, passiveByPartId, reasonContext);
			gameplaySolveReason.SetSolveReason(reason, passiveByPartId, stepBatchPartId);
		}
	}

	private static PartId ResolveStepBatchPartId(IGameplayObject obj, GameplaySolveReason reason, PartId passiveByPartId, GameplaySolveReasonContext reasonContext)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		PartId stepBatchPartId = reasonContext.StepBatchPartId;
		if (!((PartId)(ref stepBatchPartId)).Equals(default(PartId)))
		{
			return stepBatchPartId;
		}
		GameplayManager instance = SingletonComponent<GameplayManager>.Instance;
		RingObjectsService ringObjectsService = instance?.ObjectsService;
		if (ringObjectsService != null && instance.ActiveGameplayObject != null && ringObjectsService.TryGetPartId(instance.ActiveGameplayObject, out var partId))
		{
			return partId;
		}
		if (!reason.IsPassive() && obj != null && ringObjectsService != null && ringObjectsService.TryGetPartId(obj, out var partId2))
		{
			return partId2;
		}
		if (reason.IsPassive() && !((PartId)(ref passiveByPartId)).Equals(default(PartId)))
		{
			return passiveByPartId;
		}
		if (ringObjectsService != null && obj != null && ringObjectsService.TryGetPartId(obj, out var partId3))
		{
			return partId3;
		}
		return default(PartId);
	}
}
