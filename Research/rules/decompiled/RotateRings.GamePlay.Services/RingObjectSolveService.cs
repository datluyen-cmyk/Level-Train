using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.CompilerServices;
using RotateRings.GamePlay.Objects;
using UnityEngine;
using UnlockRing.Core.Domain;

namespace RotateRings.GamePlay.Services;

public sealed class RingObjectSolveService : GameplayServiceBase<GameplayManager>
{
	[StructLayout(LayoutKind.Auto)]
	[CompilerGenerated]
	private struct _003CDeferSolveUntilGateReadyAsync_003Ed__14 : IAsyncStateMachine
	{
		public int _003C_003E1__state;

		public AsyncUniTaskVoidMethodBuilder _003C_003Et__builder;

		public RingSolveBreakDelayService delay;

		public IGameplayObject obj;

		public RingObjectSolveService _003C_003E4__this;

		public GameplaySolveReasonContext releaseContext;

		private Awaiter _003C_003Eu__1;

		private void MoveNext()
		{
			//IL_0055: Unknown result type (might be due to invalid IL or missing references)
			//IL_005a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0061: Unknown result type (might be due to invalid IL or missing references)
			//IL_001d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0022: Unknown result type (might be due to invalid IL or missing references)
			//IL_0025: Unknown result type (might be due to invalid IL or missing references)
			//IL_002a: Unknown result type (might be due to invalid IL or missing references)
			//IL_003e: Unknown result type (might be due to invalid IL or missing references)
			//IL_003f: Unknown result type (might be due to invalid IL or missing references)
			int num = _003C_003E1__state;
			RingObjectSolveService ringObjectSolveService = _003C_003E4__this;
			try
			{
				Awaiter val2;
				if (num != 0)
				{
					UniTask val = delay.WaitSolveGateAsync(obj);
					val2 = ((UniTask)(ref val)).GetAwaiter();
					if (!((Awaiter)(ref val2)).IsCompleted)
					{
						num = (_003C_003E1__state = 0);
						_003C_003Eu__1 = val2;
						((AsyncUniTaskVoidMethodBuilder)(ref _003C_003Et__builder)).AwaitUnsafeOnCompleted<Awaiter, _003CDeferSolveUntilGateReadyAsync_003Ed__14>(ref val2, ref this);
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
				if (!ringObjectSolveService.DestroyToken.IsCancellationRequested)
				{
					ringObjectSolveService.SolveObject(obj, null, releaseContext);
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

	private const string TAG = "[GamePlay][ObjectSolveService] ";

	private readonly Dictionary<Type, ISolvableSolver> solversByType = new Dictionary<Type, ISolvableSolver>();

	private readonly List<IGameplayObject> registeredObjects = new List<IGameplayObject>();

	private bool isResolving;

	private bool resolveDirty;

	public int UnsolvedCount => registeredObjects.Count;

	public RingObjectSolveService(GameplayManager host)
		: base(host)
	{
		RegisterSolver<RingObject>(new RingObjectSolver());
		RegisterSolver<PullObject>(new PullObjectSolver());
		RegisterSolver<FixedButterflyObject>(new FixedButterflyObjectSolver());
		RegisterSolver<FixedObject>(new FixedObjectSolver());
		RegisterSolver<RingPaletteLockGapObject>(new RingPaletteLockGapSolver());
	}

	public void RegisterObject(IGameplayObject obj)
	{
		if (obj != null && !obj.IsSolved)
		{
			registeredObjects.Add(obj);
		}
	}

	public override void ClearLevel()
	{
		_ = registeredObjects.Count;
		registeredObjects.Clear();
		isResolving = false;
		resolveDirty = false;
		ClearAllSolveReasons();
	}

	private void ClearAllSolveReasons()
	{
		RingObjectsService objectsService = base.Host.ObjectsService;
		if (objectsService?.GameplayObjects == null)
		{
			return;
		}
		IReadOnlyList<IGameplayObject> gameplayObjects = objectsService.GameplayObjects;
		for (int i = 0; i < gameplayObjects.Count; i++)
		{
			if (gameplayObjects[i] is IGameplaySolveReason gameplaySolveReason)
			{
				gameplaySolveReason.ClearSolveReason();
			}
		}
	}

	private void RegisterSolver<T>(ISolvableSolver solver) where T : IGameplayObject
	{
		if (solver != null)
		{
			solversByType[typeof(T)] = solver;
		}
	}

	public bool CanSolve(IGameplayObject obj)
	{
		GameplayManager instance = SingletonComponent<GameplayManager>.Instance;
		if (instance != null && instance.IsLevelEnterPlaying)
		{
			return false;
		}
		if (obj == null || obj.IsSolved)
		{
			return false;
		}
		if (solversByType.TryGetValue(obj.GetType(), out var value))
		{
			return value.CanSolve(obj);
		}
		return false;
	}

	public void SolveObject(IGameplayObject obj, BombRelative sourceBomb = null, GameplaySolveReasonContext releaseContext = default(GameplaySolveReasonContext))
	{
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		GameplayManager instance = SingletonComponent<GameplayManager>.Instance;
		if ((instance != null && instance.IsLevelEnterPlaying) || obj == null || obj.IsSolved)
		{
			return;
		}
		RingSolveBreakDelayService ringSolveBreakDelayService = SingletonComponent<GameplayManager>.Instance?.SolveBreakDelay;
		if (ringSolveBreakDelayService != null && ringSolveBreakDelayService.IsNeedToDeferSolve(obj, sourceBomb))
		{
			PartId passiveByPartId = ResolveCascadeTriggerPartId(sourceBomb);
			GameplaySolveReasonContext releaseContext2 = releaseContext.WithDefaultReasonIfNone(sourceBomb).ToDeferredPassive(passiveByPartId);
			UniTaskVoid val = DeferSolveUntilGateReadyAsync(obj, ringSolveBreakDelayService, releaseContext2);
			((UniTaskVoid)(ref val)).Forget();
		}
		else
		{
			if (!solversByType.TryGetValue(obj.GetType(), out var solver))
			{
				return;
			}
			RingBoomService ringBoomService = SingletonComponent<GameplayManager>.Instance?.RingBoom;
			if (sourceBomb == null)
			{
				sourceBomb = ringBoomService?.ActiveBombCascadeSource;
			}
			if (ringBoomService != null && (Object)(object)sourceBomb != (Object)null)
			{
				ringBoomService.RunInBombCascadeScope(sourceBomb, delegate
				{
					solver.Solve(obj, sourceBomb, releaseContext);
				});
			}
			else
			{
				solver.Solve(obj, sourceBomb, releaseContext);
			}
		}
	}

	[AsyncStateMachine(typeof(_003CDeferSolveUntilGateReadyAsync_003Ed__14))]
	private UniTaskVoid DeferSolveUntilGateReadyAsync(IGameplayObject obj, RingSolveBreakDelayService delay, GameplaySolveReasonContext releaseContext)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		_003CDeferSolveUntilGateReadyAsync_003Ed__14 _003CDeferSolveUntilGateReadyAsync_003Ed__ = default(_003CDeferSolveUntilGateReadyAsync_003Ed__14);
		_003CDeferSolveUntilGateReadyAsync_003Ed__._003C_003Et__builder = AsyncUniTaskVoidMethodBuilder.Create();
		_003CDeferSolveUntilGateReadyAsync_003Ed__._003C_003E4__this = this;
		_003CDeferSolveUntilGateReadyAsync_003Ed__.obj = obj;
		_003CDeferSolveUntilGateReadyAsync_003Ed__.delay = delay;
		_003CDeferSolveUntilGateReadyAsync_003Ed__.releaseContext = releaseContext;
		_003CDeferSolveUntilGateReadyAsync_003Ed__._003C_003E1__state = -1;
		((AsyncUniTaskVoidMethodBuilder)(ref _003CDeferSolveUntilGateReadyAsync_003Ed__._003C_003Et__builder)).Start<_003CDeferSolveUntilGateReadyAsync_003Ed__14>(ref _003CDeferSolveUntilGateReadyAsync_003Ed__);
		return ((AsyncUniTaskVoidMethodBuilder)(ref _003CDeferSolveUntilGateReadyAsync_003Ed__._003C_003Et__builder)).Task;
	}

	public void TryUpdateAll()
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fd: Unknown result type (might be due to invalid IL or missing references)
		//IL_0102: Unknown result type (might be due to invalid IL or missing references)
		if (isResolving)
		{
			resolveDirty = true;
			return;
		}
		isResolving = true;
		PartId cascadeTriggerPartId = ResolveCascadeTriggerPartId();
		try
		{
			do
			{
				resolveDirty = false;
				for (int num = registeredObjects.Count - 1; num >= 0; num--)
				{
					IGameplayObject gameplayObject = registeredObjects[num];
					if (gameplayObject == null)
					{
						registeredObjects.RemoveAt(num);
					}
					else if (gameplayObject.IsSolved)
					{
						registeredObjects.RemoveAt(num);
					}
					else if (CanSolve(gameplayObject))
					{
						BombRelative sourceBomb = (SingletonComponent<GameplayManager>.Instance?.RingBoom)?.ActiveBombCascadeSource;
						GameplaySolveReasonContext releaseContext = BuildTryUpdateAllReleaseContext(gameplayObject, sourceBomb, cascadeTriggerPartId);
						SolveObject(gameplayObject, sourceBomb, releaseContext);
						registeredObjects.RemoveAt(num);
						PartId partId;
						if (gameplayObject is FixedObject guide && TryGetCatchGuideCascadeAnchor(guide, out var lockedPartId))
						{
							cascadeTriggerPartId = lockedPartId;
						}
						else if (base.Host.ObjectsService != null && base.Host.ObjectsService.TryGetPartId(gameplayObject, out partId))
						{
							cascadeTriggerPartId = gameplayObject.ResolveCascadeTriggerAfterSolved(partId);
						}
					}
				}
			}
			while (resolveDirty);
		}
		finally
		{
			isResolving = false;
		}
	}

	private static GameplaySolveReasonContext BuildTryUpdateAllReleaseContext(IGameplayObject obj, BombRelative sourceBomb, PartId cascadeTriggerPartId)
	{
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		GameplayManager instance = SingletonComponent<GameplayManager>.Instance;
		if ((Object)(object)instance != (Object)null && obj == instance.ActiveGameplayObject)
		{
			if (instance.ActiveOperationSource == LegacyOperationSource.Hammer)
			{
				return GameplaySolveReasonContext.Hammer();
			}
			if (instance.ActiveOperationSource == LegacyOperationSource.PointerUp)
			{
				return GameplaySolveReasonContext.PointerUp();
			}
		}
		RingBoomService ringBoomService = instance?.RingBoom;
		if (ringBoomService != null && ringBoomService.IsRegisteredBombExplosionObject(obj))
		{
			return GameplaySolveReasonContext.Bomb();
		}
		if (obj is FixedObject guide && TryGetCatchGuideCascadeAnchor(guide, out var lockedPartId))
		{
			return GameplaySolveReasonContext.PassivePointerUp(lockedPartId);
		}
		return GameplaySolveReasonContext.PassivePointerUp(ResolvePassiveTriggerPartId(cascadeTriggerPartId));
	}

	private static bool TryGetCatchGuideCascadeAnchor(FixedObject guide, out PartId lockedPartId)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		lockedPartId = default(PartId);
		return (SingletonComponent<GameplayManager>.Instance?.LockPassive)?.TryGetCatchGuideLockedPartId(guide, out lockedPartId) ?? false;
	}

	private static PartId ResolvePassiveTriggerPartId(PartId cascadeTriggerPartId)
	{
		//IL_0004: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		if (!((PartId)(ref cascadeTriggerPartId)).Equals(default(PartId)))
		{
			return cascadeTriggerPartId;
		}
		return ResolveCascadeTriggerPartId();
	}

	private static PartId ResolveCascadeTriggerPartId(BombRelative sourceBomb = null)
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00de: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		GameplayManager instance = SingletonComponent<GameplayManager>.Instance;
		RingObjectsService ringObjectsService = instance?.ObjectsService;
		if (ringObjectsService == null)
		{
			return default(PartId);
		}
		RingBoomService ringBoomService = instance?.RingBoom;
		if (sourceBomb == null)
		{
			sourceBomb = ringBoomService?.ActiveBombCascadeSource;
		}
		if ((Object)(object)sourceBomb != (Object)null)
		{
			RingObject componentInParent = ((Component)sourceBomb).GetComponentInParent<RingObject>();
			if ((Object)(object)componentInParent != (Object)null && ringObjectsService.TryGetPartId(componentInParent, out var partId))
			{
				return partId;
			}
			PullObject componentInParent2 = ((Component)sourceBomb).GetComponentInParent<PullObject>();
			if ((Object)(object)componentInParent2 != (Object)null && ringObjectsService.TryGetPartId(componentInParent2, out var partId2))
			{
				return partId2;
			}
			if (ringBoomService != null && ringBoomService.TryGetRegisteredBombExternalCascadeTriggerPartId(ringObjectsService, out var partId3))
			{
				return partId3;
			}
		}
		if (ringBoomService != null && (ringBoomService.IsInBombCascadeScope || instance.ActiveGameplayObject == null) && ringBoomService.TryGetDragBombProbeCascadeTriggerPartId(ringObjectsService, out var partId4))
		{
			return partId4;
		}
		if (instance.ActiveGameplayObject != null && ringObjectsService.TryGetPartId(instance.ActiveGameplayObject, out var partId5))
		{
			return partId5;
		}
		return default(PartId);
	}
}
