using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.CompilerServices;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using RotateRings.Common;
using RotateRings.Prop;
using RotateRings.Services;
using RotateRings.UI.Game;
using RotateRings.UI.Props;
using Spine.Unity;
using UniLog;
using UnityEngine;
using UnityEngine.UI;
using UnlockRing.Core.Domain;
using UnlockRing.Prop;
using XingYun.UI.Controller;

namespace RotateRings.GamePlay.Services.Prop.Handlers;

public sealed class RocketPropHandler : IRotateRingPropUse, IDisposable
{
	private sealed class RocketFlightTask
	{
		public IGameplayObject target;

		public RectTransform rocketRect;

		public Tween rocketTween;

		public CancellationTokenSource cts;

		public TrackPropUseReason reason;

		public bool isFastFire;

		public bool isFlying;
	}

	[StructLayout(LayoutKind.Auto)]
	[CompilerGenerated]
	private struct _003CRecycleBoomEffectAfterDelayAsync_003Ed__68 : IAsyncStateMachine
	{
		public int _003C_003E1__state;

		public AsyncUniTaskVoidMethodBuilder _003C_003Et__builder;

		public GameObject instance;

		public float delaySeconds;

		private Awaiter<bool> _003C_003Eu__1;

		private void MoveNext()
		{
			//IL_007d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0082: Unknown result type (might be due to invalid IL or missing references)
			//IL_0089: Unknown result type (might be due to invalid IL or missing references)
			//IL_003a: Unknown result type (might be due to invalid IL or missing references)
			//IL_003f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0042: Unknown result type (might be due to invalid IL or missing references)
			//IL_0047: Unknown result type (might be due to invalid IL or missing references)
			//IL_004a: Unknown result type (might be due to invalid IL or missing references)
			//IL_004f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0063: Unknown result type (might be due to invalid IL or missing references)
			//IL_0064: Unknown result type (might be due to invalid IL or missing references)
			int num = _003C_003E1__state;
			try
			{
				if (num == 0 || !((Object)(object)instance == (Object)null))
				{
					try
					{
						Awaiter<bool> val2;
						if (num != 0)
						{
							UniTask val = UniTask.Delay(TimeSpan.FromSeconds(delaySeconds), false, (PlayerLoopTiming)8, UniTaskCancellationExtensions.GetCancellationTokenOnDestroy(instance));
							val2 = ((UniTask)(ref val)).SuppressCancellationThrow().GetAwaiter();
							if (!val2.IsCompleted)
							{
								num = (_003C_003E1__state = 0);
								_003C_003Eu__1 = val2;
								((AsyncUniTaskVoidMethodBuilder)(ref _003C_003Et__builder)).AwaitUnsafeOnCompleted<Awaiter<bool>, _003CRecycleBoomEffectAfterDelayAsync_003Ed__68>(ref val2, ref this);
								return;
							}
						}
						else
						{
							val2 = _003C_003Eu__1;
							_003C_003Eu__1 = default(Awaiter<bool>);
							num = (_003C_003E1__state = -1);
						}
						if (!val2.GetResult() && (Object)(object)instance != (Object)null)
						{
							GameApp.Pool.Recycle(instance);
						}
					}
					catch (Exception ex)
					{
						Log.LogError("[RocketPropHandler] ", "爆炸特效回收异常: {0}", new object[1] { ex.Message });
						if ((Object)(object)instance != (Object)null)
						{
							GameApp.Pool.Recycle(instance);
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
	private struct _003CRocketFlyThenFinishAsync_003Ed__57 : IAsyncStateMachine
	{
		public int _003C_003E1__state;

		public AsyncUniTaskVoidMethodBuilder _003C_003Et__builder;

		public IGameplayObject target;

		public TrackPropUseReason reason;

		public bool isFastFire;

		public RocketPropHandler _003C_003E4__this;

		public Vector3[] path;

		public RectTransform flyParent;

		public bool isNearCurve;

		private RocketFlightTask _003Ctask_003E5__2;

		private CancellationToken _003Cct_003E5__3;

		private RectTransform _003CrocketRect_003E5__4;

		private Awaiter<bool> _003C_003Eu__1;

		private void MoveNext()
		{
			//IL_0165: Unknown result type (might be due to invalid IL or missing references)
			//IL_016a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0172: Unknown result type (might be due to invalid IL or missing references)
			//IL_0298: Unknown result type (might be due to invalid IL or missing references)
			//IL_029d: Unknown result type (might be due to invalid IL or missing references)
			//IL_02a5: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
			//IL_0093: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
			//IL_011e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0123: Unknown result type (might be due to invalid IL or missing references)
			//IL_0127: Unknown result type (might be due to invalid IL or missing references)
			//IL_012c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0130: Unknown result type (might be due to invalid IL or missing references)
			//IL_0135: Unknown result type (might be due to invalid IL or missing references)
			//IL_014a: Unknown result type (might be due to invalid IL or missing references)
			//IL_014c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0251: Unknown result type (might be due to invalid IL or missing references)
			//IL_0256: Unknown result type (might be due to invalid IL or missing references)
			//IL_025a: Unknown result type (might be due to invalid IL or missing references)
			//IL_025f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0263: Unknown result type (might be due to invalid IL or missing references)
			//IL_0268: Unknown result type (might be due to invalid IL or missing references)
			//IL_03a0: Unknown result type (might be due to invalid IL or missing references)
			//IL_0389: Unknown result type (might be due to invalid IL or missing references)
			//IL_027d: Unknown result type (might be due to invalid IL or missing references)
			//IL_027f: Unknown result type (might be due to invalid IL or missing references)
			//IL_03a5: Unknown result type (might be due to invalid IL or missing references)
			//IL_03a9: Unknown result type (might be due to invalid IL or missing references)
			//IL_03b0: Unknown result type (might be due to invalid IL or missing references)
			//IL_03c8: Unknown result type (might be due to invalid IL or missing references)
			int num = _003C_003E1__state;
			RocketPropHandler rocketPropHandler = _003C_003E4__this;
			try
			{
				if ((uint)num > 1u)
				{
					_003Ctask_003E5__2 = new RocketFlightTask
					{
						target = target,
						reason = reason,
						isFastFire = isFastFire,
						cts = new CancellationTokenSource()
					};
					rocketPropHandler.activeTasks.Add(_003Ctask_003E5__2);
					_003Cct_003E5__3 = _003Ctask_003E5__2.cts.Token;
				}
				try
				{
					Awaiter<bool> val;
					if (num == 0)
					{
						val = _003C_003Eu__1;
						_003C_003Eu__1 = default(Awaiter<bool>);
						num = (_003C_003E1__state = -1);
						goto IL_0181;
					}
					if (num == 1)
					{
						val = _003C_003Eu__1;
						_003C_003Eu__1 = default(Awaiter<bool>);
						num = (_003C_003E1__state = -1);
						goto IL_02b4;
					}
					Vector3 startUiLocal = ((path.Length >= 4) ? path[1] : path[0]);
					UniTask val2;
					if (TrySpawnRocket(flyParent, startUiLocal, out _003CrocketRect_003E5__4))
					{
						_003Ctask_003E5__2.rocketRect = _003CrocketRect_003E5__4;
						_003Ctask_003E5__2.isFlying = true;
						AudioService.PlayUiSound(AudioKeys.ItemRocketFire.Value);
						val2 = UniTask.Delay(TimeSpan.FromSeconds(UnlockRingPropConfig.Instance.GetRocketPrepareDuration()), false, (PlayerLoopTiming)8, _003Cct_003E5__3);
						val = ((UniTask)(ref val2)).SuppressCancellationThrow().GetAwaiter();
						if (!val.IsCompleted)
						{
							num = (_003C_003E1__state = 0);
							_003C_003Eu__1 = val;
							((AsyncUniTaskVoidMethodBuilder)(ref _003C_003Et__builder)).AwaitUnsafeOnCompleted<Awaiter<bool>, _003CRocketFlyThenFinishAsync_003Ed__57>(ref val, ref this);
							return;
						}
						goto IL_0181;
					}
					rocketPropHandler.FinishFlight(_003Ctask_003E5__2, reason, isFastFire, hitSuccess: false);
					goto end_IL_0074;
					IL_0181:
					if (!val.GetResult() && !_003Cct_003E5__3.IsCancellationRequested && target != null)
					{
						float num2 = CalcPathLength((path.Length >= 4) ? ExtractPassPoints(path) : path);
						float num3 = (isNearCurve ? UnlockRingPropConfig.Instance.GetRocketNearFlyDurationMax() : UnlockRingPropConfig.Instance.GetRocketFlyDurationMax());
						float num4 = Mathf.Min(num2 / UnlockRingPropConfig.Instance.GetRocketFlySpeed(), num3);
						_003Ctask_003E5__2.rocketTween = rocketPropHandler.BuildRocketFlyTween(_003CrocketRect_003E5__4, path, num4);
						AudioService.PlayUiSound(AudioKeys.ItemRocketShoot.Value);
						val2 = UniTask.Delay(TimeSpan.FromSeconds(num4), false, (PlayerLoopTiming)8, _003Cct_003E5__3);
						val = ((UniTask)(ref val2)).SuppressCancellationThrow().GetAwaiter();
						if (!val.IsCompleted)
						{
							num = (_003C_003E1__state = 1);
							_003C_003Eu__1 = val;
							((AsyncUniTaskVoidMethodBuilder)(ref _003C_003Et__builder)).AwaitUnsafeOnCompleted<Awaiter<bool>, _003CRocketFlyThenFinishAsync_003Ed__57>(ref val, ref this);
							return;
						}
						goto IL_02b4;
					}
					rocketPropHandler.FinishFlight(_003Ctask_003E5__2, reason, isFastFire, hitSuccess: false);
					goto end_IL_0074;
					IL_02b4:
					if (val.GetResult() || _003Cct_003E5__3.IsCancellationRequested || target == null)
					{
						rocketPropHandler.FinishFlight(_003Ctask_003E5__2, reason, isFastFire, hitSuccess: false);
					}
					else if ((Object)(object)_003CrocketRect_003E5__4 == (Object)null || !((Component)_003CrocketRect_003E5__4).gameObject.activeInHierarchy)
					{
						rocketPropHandler.RecycleRocketInstance(_003CrocketRect_003E5__4);
						_003Ctask_003E5__2.rocketRect = null;
						rocketPropHandler.FinishFlight(_003Ctask_003E5__2, reason, isFastFire, hitSuccess: false);
					}
					else
					{
						rocketPropHandler.RecycleRocketInstance(_003CrocketRect_003E5__4);
						_003Ctask_003E5__2.rocketRect = null;
						AudioService.PlayUiSound(AudioKeys.ItemRocketHit.Value);
						Vector3 val3 = ((path.Length >= 4) ? path[path.Length - 2] : path[path.Length - 1]);
						Vector3 localPos = default(Vector3);
						((Vector3)(ref localPos))._002Ector(val3.x, val3.y, -1f);
						rocketPropHandler.PlayBoomEffect(flyParent, localPos);
						bool hitSuccess = rocketPropHandler.host != null && rocketPropHandler.host.TryExecuteHammerRemove(target);
						rocketPropHandler.FinishFlight(_003Ctask_003E5__2, reason, isFastFire, hitSuccess);
						_003CrocketRect_003E5__4 = null;
					}
					end_IL_0074:;
				}
				catch (Exception ex)
				{
					Log.LogError("[RocketPropHandler] ", "小礼炮飞行异常: {0}", new object[1] { ex.Message });
					if ((Object)(object)_003Ctask_003E5__2.rocketRect != (Object)null)
					{
						rocketPropHandler.RecycleRocketInstance(_003Ctask_003E5__2.rocketRect);
						_003Ctask_003E5__2.rocketRect = null;
					}
					rocketPropHandler.FinishFlight(_003Ctask_003E5__2, reason, isFastFire, hitSuccess: false);
				}
				finally
				{
					if (num < 0)
					{
						_003Ctask_003E5__2.isFlying = false;
						rocketPropHandler.activeTasks.Remove(_003Ctask_003E5__2);
						_003Ctask_003E5__2.cts?.Cancel();
						_003Ctask_003E5__2.cts?.Dispose();
						_003Ctask_003E5__2.cts = null;
						if (isFastFire && rocketPropHandler.activeTasks.Count == 0)
						{
							rocketPropHandler.callback?.OnFastFireSessionComplete();
						}
					}
				}
			}
			catch (Exception exception)
			{
				_003C_003E1__state = -2;
				_003Ctask_003E5__2 = null;
				_003Cct_003E5__3 = default(CancellationToken);
				((AsyncUniTaskVoidMethodBuilder)(ref _003C_003Et__builder)).SetException(exception);
				return;
			}
			_003C_003E1__state = -2;
			_003Ctask_003E5__2 = null;
			_003Cct_003E5__3 = default(CancellationToken);
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

	private const string Tag = "[RocketPropHandler] ";

	private const string RocketEffectLocation = "eff_rocket";

	private const string BoomEffectLocation = "eff_rocket_boom_01";

	private const float BoomEffectDurationFallback = 2f;

	private const float FlyUiZ = -1f;

	private const string RocketSpineAnimationName = "zhuan";

	private static GameObject rocketPrefab;

	private static GameObject boomEffectPrefab;

	private IRotateRingPropHost host;

	private IRocketPropUseCallback callback;

	private bool isReady;

	private bool isFinishing;

	private readonly HashSet<IGameplayObject> reservedTargets = new HashSet<IGameplayObject>();

	private readonly Dictionary<PartId, HashSet<PartId>> lockerToLockedMap = new Dictionary<PartId, HashSet<PartId>>();

	private readonly HashSet<PartId> cascadeExcluded = new HashSet<PartId>();

	private readonly List<RocketFlightTask> activeTasks = new List<RocketFlightTask>();

	public bool IsAnyFlightActive => activeTasks.Count > 0;

	public EPropType PropType => EPropType.Skyrocket;

	public void Dispose()
	{
	}

	public bool CanStart(IRotateRingPropHost propHost)
	{
		if (propHost == null || !propHost.CanUseRocket())
		{
			return false;
		}
		IReadOnlyList<IGameplayObject> candidates = SingletonComponent<GameplayManager>.Instance?.GetGameplayObjects();
		PrepareCascadeExclusion();
		return SkyrocketTargetSelector.HasSelectable(candidates, reservedTargets, cascadeExcluded);
	}

	public void Begin(IRotateRingPropHost propHost, IRotateRingPropUseCallback useCallback)
	{
		host = propHost;
		callback = useCallback as IRocketPropUseCallback;
		isReady = false;
		isFinishing = false;
		ClearReservedTargets();
		ClearCascadeData();
		CancelAllFlights();
	}

	public void OnPropGuideOpened()
	{
	}

	public void OnGuideReady()
	{
		isReady = true;
	}

	public void OnUseButtonClicked()
	{
		if (isReady && callback != null && !callback.HasPropConsumed && !isFinishing)
		{
			Fire(TrackPropUseReason.None, isFastFire: false);
		}
	}

	public bool Fire(TrackPropUseReason reason)
	{
		return Fire(reason, isFastFire: true);
	}

	private bool Fire(TrackPropUseReason reason, bool isFastFire)
	{
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
		IReadOnlyList<IGameplayObject> readOnlyList = SingletonComponent<GameplayManager>.Instance?.GetGameplayObjects();
		if (readOnlyList == null || readOnlyList.Count == 0)
		{
			if (!isFastFire)
			{
				RequestFinish(RotateRingPropUseResult.Cancel, consumeOnSuccess: false);
			}
			return false;
		}
		PrepareCascadeExclusion();
		IGameplayObject gameplayObject = SkyrocketTargetSelector.Select(readOnlyList, reservedTargets, cascadeExcluded);
		if (gameplayObject == null)
		{
			if (!isFastFire)
			{
				RequestFinish(RotateRingPropUseResult.Cancel, consumeOnSuccess: false);
			}
			return false;
		}
		Vector3 worldPosition = CalculateExplosionPoint(gameplayObject);
		if (!TryGetFlyStartUiPosition(out var startLocal, out var flyParent))
		{
			if (!isFastFire)
			{
				RequestFinish(RotateRingPropUseResult.Cancel, consumeOnSuccess: false);
			}
			return false;
		}
		if (!WorldToUIPostionUtil.WorldToLocalPoint(worldPosition, flyParent, out var localPoint))
		{
			if (!isFastFire)
			{
				RequestFinish(RotateRingPropUseResult.Cancel, consumeOnSuccess: false);
			}
			return false;
		}
		RectTransform ringRect = TryResolveRingRect();
		bool isNearCurve;
		Vector3[] path = BuildFlyPath(startLocal, localPoint, flyParent.rect, flyParent, ringRect, out isNearCurve);
		reservedTargets.Add(gameplayObject);
		UniTaskVoid val = RocketFlyThenFinishAsync(gameplayObject, flyParent, path, isNearCurve, reason, isFastFire);
		((UniTaskVoid)(ref val)).Forget();
		return true;
	}

	public void OnGuideDismissed(bool cancelled)
	{
		if (cancelled && !isFinishing && !IsAnyFlightActive && callback != null && !callback.HasPropConsumed)
		{
			CancelAllFlights();
			RequestFinish(RotateRingPropUseResult.Cancel, consumeOnSuccess: false);
		}
	}

	public void End()
	{
		isReady = false;
		isFinishing = false;
		ClearReservedTargets();
		ClearCascadeData();
		CancelAllFlights();
		host = null;
		callback = null;
	}

	private void PrepareCascadeExclusion()
	{
		if (lockerToLockedMap.Count == 0)
		{
			BuildLockerToLockedMap();
		}
		SyncCascadeExcluded();
	}

	private void BuildLockerToLockedMap()
	{
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		lockerToLockedMap.Clear();
		IReadOnlyList<IGameplayObject> readOnlyList = SingletonComponent<GameplayManager>.Instance?.GetGameplayObjects();
		if (readOnlyList == null)
		{
			return;
		}
		RingLockActiveService lockActive = SingletonComponent<GameplayManager>.Instance?.LockActive;
		RingLockPassiveService heldLockedRelation = SingletonComponent<GameplayManager>.Instance?.LockPassive;
		for (int i = 0; i < readOnlyList.Count; i++)
		{
			IGameplayObject gameplayObject = readOnlyList[i];
			if (gameplayObject != null && !gameplayObject.IsSolved && gameplayObject.TryGetPartId(out var partId))
			{
				CollectLockerEntries(lockActive, heldLockedRelation, gameplayObject, partId);
			}
		}
	}

	private void CollectLockerEntries(RingLockActiveService lockActive, RingLockPassiveService heldLockedRelation, IGameplayObject obj, PartId lockedPartId)
	{
		//IL_0004: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
		if (lockActive != null && lockActive.TryGetActiveLockers(lockedPartId, out var lockers))
		{
			foreach (PartId item in lockers)
			{
				AddLockerEntry(item, lockedPartId);
			}
		}
		if (heldLockedRelation == null)
		{
			return;
		}
		if (obj is RingObject ring && heldLockedRelation.TryGetActiveHeldLockers(ring, out var lockers2))
		{
			foreach (PartId item2 in lockers2)
			{
				AddLockerEntry(item2, lockedPartId);
			}
			return;
		}
		if (!(obj is PullObject pull) || !heldLockedRelation.TryGetActiveHeldLockers(pull, out var lockers3))
		{
			return;
		}
		foreach (PartId item3 in lockers3)
		{
			AddLockerEntry(item3, lockedPartId);
		}
	}

	private void AddLockerEntry(PartId lockerPartId, PartId lockedPartId)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		if (!lockerToLockedMap.TryGetValue(lockerPartId, out var value))
		{
			value = new HashSet<PartId>();
			lockerToLockedMap[lockerPartId] = value;
		}
		value.Add(lockedPartId);
	}

	private void SyncCascadeExcluded()
	{
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		cascadeExcluded.Clear();
		foreach (IGameplayObject reservedTarget in reservedTargets)
		{
			if (reservedTarget != null && reservedTarget.TryGetPartId(out var partId))
			{
				cascadeExcluded.Add(partId);
			}
		}
		ExpandCascadeExcluded();
	}

	private void ExpandCascadeExcluded()
	{
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		RingLockActiveService lockActive = SingletonComponent<GameplayManager>.Instance?.LockActive;
		RingLockPassiveService heldLockedRelation = SingletonComponent<GameplayManager>.Instance?.LockPassive;
		RingObjectsService objectsService = SingletonComponent<GameplayManager>.Instance?.ObjectsService;
		bool flag = true;
		while (flag)
		{
			flag = false;
			List<PartId> list = null;
			foreach (PartId item in cascadeExcluded)
			{
				if (!lockerToLockedMap.TryGetValue(item, out var value))
				{
					continue;
				}
				foreach (PartId item2 in value)
				{
					if (!cascadeExcluded.Contains(item2) && AreAllLockersExcluded(lockActive, heldLockedRelation, objectsService, item2))
					{
						if (list == null)
						{
							list = new List<PartId>();
						}
						list.Add(item2);
					}
				}
			}
			if (list == null)
			{
				continue;
			}
			for (int i = 0; i < list.Count; i++)
			{
				if (cascadeExcluded.Add(list[i]))
				{
					flag = true;
				}
			}
		}
	}

	private bool AreAllLockersExcluded(RingLockActiveService lockActive, RingLockPassiveService heldLockedRelation, RingObjectsService objectsService, PartId lockedPartId)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cd: Unknown result type (might be due to invalid IL or missing references)
		bool result = false;
		if (lockActive != null && lockActive.TryGetActiveLockers(lockedPartId, out var lockers))
		{
			result = true;
			foreach (PartId item in lockers)
			{
				if (!cascadeExcluded.Contains(item))
				{
					return false;
				}
			}
		}
		if (heldLockedRelation != null && objectsService != null && objectsService.TryGetPartObject(lockedPartId, out var gameplayObject) && !gameplayObject.IsSolved)
		{
			IReadOnlyCollection<PartId> lockers2 = null;
			if (gameplayObject is RingObject ring)
			{
				heldLockedRelation.TryGetActiveHeldLockers(ring, out lockers2);
			}
			else if (gameplayObject is PullObject pull)
			{
				heldLockedRelation.TryGetActiveHeldLockers(pull, out lockers2);
			}
			if (lockers2 != null && lockers2.Count > 0)
			{
				result = true;
				foreach (PartId item2 in lockers2)
				{
					if (!cascadeExcluded.Contains(item2))
					{
						return false;
					}
				}
			}
		}
		return result;
	}

	private void ClearCascadeData()
	{
		lockerToLockedMap.Clear();
		cascadeExcluded.Clear();
	}

	private static Vector3 CalculateExplosionPoint(IGameplayObject target)
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		if (target is PullObject pull)
		{
			return CalculatePullExplosionPoint(pull);
		}
		if (!(target is RingObject))
		{
			_ = target is FixedObject;
		}
		return target.GetPosition();
	}

	private static Vector3 CalculatePullExplosionPoint(PullObject pull)
	{
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_0103: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_0105: Unknown result type (might be due to invalid IL or missing references)
		//IL_010e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0113: Unknown result type (might be due to invalid IL or missing references)
		//IL_0115: Unknown result type (might be due to invalid IL or missing references)
		//IL_011f: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)pull == (Object)null)
		{
			return Vector3.zero;
		}
		Collider2D val = pull.Overlap2DHandler?.Collider ?? ((Component)pull).GetComponent<Collider2D>();
		if ((Object)(object)val == (Object)null)
		{
			return pull.GetPosition();
		}
		Bounds bounds = val.bounds;
		Vector3 center = ((Bounds)(ref bounds)).center;
		Vector3 size = ((Bounds)(ref bounds)).size;
		Vector3 val2 = Vector3.right;
		float num = size.x;
		if (size.y >= size.x)
		{
			val2 = Vector3.up;
			num = size.y;
		}
		Vector3 val3 = center - val2 * (num * 0.5f);
		Vector3 val4 = center + val2 * (num * 0.5f);
		FixedObject hostGuide = pull.HostGuide;
		if ((Object)(object)hostGuide == (Object)null)
		{
			return center;
		}
		Vector3 val5 = val3 - ((Component)hostGuide).transform.position;
		float sqrMagnitude = ((Vector3)(ref val5)).sqrMagnitude;
		val5 = val4 - ((Component)hostGuide).transform.position;
		float sqrMagnitude2 = ((Vector3)(ref val5)).sqrMagnitude;
		Vector3 val6 = ((sqrMagnitude >= sqrMagnitude2) ? val3 : val4);
		return (((Component)hostGuide).transform.position + val6) * 0.5f;
	}

	private bool TryGetFlyStartUiPosition(out Vector2 startLocal, out RectTransform flyParent)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		startLocal = default(Vector2);
		flyParent = null;
		RectTransform val = TryGetUIGameCenterContent() ?? callback?.PropGuidePanel;
		if ((Object)(object)val == (Object)null)
		{
			return false;
		}
		Vector3 worldPosition = GetPropsBarRocketIconWorldPosition();
		if (worldPosition == Vector3.zero)
		{
			worldPosition = GetGuidePropItemWorldPosition();
		}
		if (worldPosition == Vector3.zero && !TryGetSourceAnchorWorldPosition(out worldPosition))
		{
			return false;
		}
		if (!TryUiWorldToLocalPoint(worldPosition, val, out var localPoint) || !TryGetScreenCenterLocalPoint(val, out var centerLocal))
		{
			return false;
		}
		startLocal = new Vector2(centerLocal.x, localPoint.y + UnlockRingPropConfig.Instance.GetRocketStartOffsetY());
		flyParent = val;
		return true;
	}

	private static bool TryUiWorldToLocalPoint(Vector3 uiWorldPos, RectTransform parent, out Vector2 localPoint)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		localPoint = default(Vector2);
		if ((Object)(object)parent == (Object)null)
		{
			return false;
		}
		localPoint = Vector2.op_Implicit(((Transform)parent).InverseTransformPoint(uiWorldPos));
		return true;
	}

	private Vector3 GetPropsBarRocketIconWorldPosition()
	{
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		IController val = default(IController);
		if (!GameApp.UI.IsOpen("UIGame", ref val) || !(val is UIGameController uIGameController))
		{
			return Vector3.zero;
		}
		Transform val2 = uIGameController.View?.PropsView?.GetPropFlyTarget(EPropType.Skyrocket);
		if ((Object)(object)val2 == (Object)null)
		{
			return Vector3.zero;
		}
		return val2.position;
	}

	private static bool TryGetScreenCenterLocalPoint(RectTransform parent, out Vector2 centerLocal)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		centerLocal = default(Vector2);
		if ((Object)(object)parent == (Object)null)
		{
			return false;
		}
		Canvas componentInParent = ((Component)parent).GetComponentInParent<Canvas>();
		Camera val = ((componentInParent != null) ? componentInParent.rootCanvas.worldCamera : null);
		return RectTransformUtility.ScreenPointToLocalPointInRectangle(parent, new Vector2((float)Screen.width * 0.5f, (float)Screen.height * 0.5f), val, ref centerLocal);
	}

	private Vector3 GetGuidePropItemWorldPosition()
	{
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		RectTransform val = callback?.PropGuidePanel;
		if ((Object)(object)val == (Object)null)
		{
			return Vector3.zero;
		}
		Transform val2 = ((Transform)val).Find("PropItem");
		if ((Object)(object)val2 == (Object)null)
		{
			return Vector3.zero;
		}
		Image componentInChildren = ((Component)val2).GetComponentInChildren<Image>(true);
		if ((Object)(object)componentInChildren == (Object)null)
		{
			return Vector3.zero;
		}
		return ((Component)componentInChildren).transform.position;
	}

	private bool TryGetSourceAnchorWorldPosition(out Vector3 worldPosition)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		worldPosition = default(Vector3);
		IController val = default(IController);
		if (!GameApp.UI.IsOpen("UIGame", ref val) || !(val is UIGameController uIGameController))
		{
			return false;
		}
		UIPropsView uIPropsView = uIGameController.View?.PropsView;
		if (uIPropsView == null)
		{
			return false;
		}
		RectTransform propItemAlignAnchor = uIPropsView.GetPropItemAlignAnchor(EPropType.Skyrocket);
		if ((Object)(object)propItemAlignAnchor == (Object)null)
		{
			return false;
		}
		worldPosition = ((Transform)propItemAlignAnchor).position;
		return true;
	}

	private static Vector3[] BuildFlyPath(Vector2 start, Vector2 end, Rect bounds, RectTransform flyParent, RectTransform ringRect, out bool isNearCurve)
	{
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_00de: Unknown result type (might be due to invalid IL or missing references)
		//IL_00df: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0100: Unknown result type (might be due to invalid IL or missing references)
		//IL_0101: Unknown result type (might be due to invalid IL or missing references)
		//IL_0106: Unknown result type (might be due to invalid IL or missing references)
		//IL_010b: Unknown result type (might be due to invalid IL or missing references)
		//IL_010d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0112: Unknown result type (might be due to invalid IL or missing references)
		//IL_0117: Unknown result type (might be due to invalid IL or missing references)
		//IL_011c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0122: Unknown result type (might be due to invalid IL or missing references)
		//IL_0129: Unknown result type (might be due to invalid IL or missing references)
		//IL_0135: Unknown result type (might be due to invalid IL or missing references)
		//IL_013a: Unknown result type (might be due to invalid IL or missing references)
		if (!((Object)(object)ringRect == (Object)null) && !((Object)(object)flyParent == (Object)null))
		{
			Rect rect = ringRect.rect;
			if (!(((Rect)(ref rect)).width <= 0.01f))
			{
				rect = ringRect.rect;
				if (!(((Rect)(ref rect)).height <= 0.01f))
				{
					Vector2 local = Vector2.op_Implicit(((Transform)ringRect).InverseTransformPoint(((Transform)flyParent).TransformPoint(Vector2.op_Implicit(start))));
					Vector2 local2 = Vector2.op_Implicit(((Transform)ringRect).InverseTransformPoint(((Transform)flyParent).TransformPoint(Vector2.op_Implicit(end))));
					Rect rect2 = ringRect.rect;
					Vector2 p0Norm = ToNormalized(local, rect2);
					Vector2 val = ToNormalized(local2, rect2);
					float rocketNearThresholdY = UnlockRingPropConfig.Instance.GetRocketNearThresholdY();
					bool flag = val.y >= rocketNearThresholdY;
					isNearCurve = !flag;
					Vector2[] array = (flag ? BuildUpperControlPoints(p0Norm, val) : BuildLowerControlPoints(p0Norm, val));
					Vector3[] array2 = (Vector3[])(object)new Vector3[array.Length];
					for (int i = 0; i < array.Length; i++)
					{
						Vector2 val2 = FromNormalized(array[i], rect2);
						Vector3 val3 = ((Transform)flyParent).InverseTransformPoint(((Transform)ringRect).TransformPoint(Vector2.op_Implicit(val2)));
						array2[i] = new Vector3(val3.x, val3.y, -1f);
					}
					return array2;
				}
			}
		}
		isNearCurve = false;
		return (Vector3[])(object)new Vector3[2]
		{
			Vector2.op_Implicit(start),
			Vector2.op_Implicit(end)
		};
	}

	private static Vector2[] BuildUpperControlPoints(Vector2 p0Norm, Vector2 pnNorm)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		UnlockRingPropConfig instance = UnlockRingPropConfig.Instance;
		Vector2 rocketUpperP1XRange = instance.GetRocketUpperP1XRange();
		float rocketUpperP1YConst = instance.GetRocketUpperP1YConst();
		float num = Random.Range(rocketUpperP1XRange.x, rocketUpperP1XRange.y);
		Vector2 val = default(Vector2);
		((Vector2)(ref val))._002Ector(num, 0.5f * pnNorm.y + rocketUpperP1YConst);
		Vector2 val2 = default(Vector2);
		((Vector2)(ref val2))._002Ector(val.x, p0Norm.y - 1f);
		Vector2 val3 = default(Vector2);
		((Vector2)(ref val3))._002Ector(val.x, pnNorm.y);
		return (Vector2[])(object)new Vector2[5] { val2, p0Norm, val, pnNorm, val3 };
	}

	private static Vector2[] BuildLowerControlPoints(Vector2 p0Norm, Vector2 pnNorm)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f3: Unknown result type (might be due to invalid IL or missing references)
		UnlockRingPropConfig instance = UnlockRingPropConfig.Instance;
		Vector2 rocketLowerP1XRange = instance.GetRocketLowerP1XRange();
		Vector2 rocketLowerP2XRange = instance.GetRocketLowerP2XRange();
		float rocketLowerP1YConst = instance.GetRocketLowerP1YConst();
		Vector2 rocketLowerP2YJitterRange = instance.GetRocketLowerP2YJitterRange();
		float num = Random.Range(rocketLowerP1XRange.x, rocketLowerP1XRange.y);
		Vector2 val = default(Vector2);
		((Vector2)(ref val))._002Ector(num, 0.5f * pnNorm.y + rocketLowerP1YConst);
		float num2 = Random.Range(rocketLowerP2XRange.x, rocketLowerP2XRange.y);
		float num3 = Random.Range(rocketLowerP2YJitterRange.x, rocketLowerP2YJitterRange.y);
		Vector2 val2 = default(Vector2);
		((Vector2)(ref val2))._002Ector(num2, 0.5f * (val.y + pnNorm.y) + num3);
		Vector2 val3 = default(Vector2);
		((Vector2)(ref val3))._002Ector(val.x, p0Norm.y - 1f);
		Vector2 val4 = default(Vector2);
		((Vector2)(ref val4))._002Ector(val2.x, pnNorm.y);
		return (Vector2[])(object)new Vector2[6] { val3, p0Norm, val, val2, pnNorm, val4 };
	}

	private static Vector2 ToNormalized(Vector2 local, Rect rect)
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		return new Vector2((((Rect)(ref rect)).width > 0.01f) ? ((local.x - ((Rect)(ref rect)).xMin) / ((Rect)(ref rect)).width) : 0f, (((Rect)(ref rect)).height > 0.01f) ? ((local.y - ((Rect)(ref rect)).yMin) / ((Rect)(ref rect)).height) : 0f);
	}

	private static Vector2 FromNormalized(Vector2 normalized, Rect rect)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		return new Vector2(((Rect)(ref rect)).xMin + normalized.x * ((Rect)(ref rect)).width, ((Rect)(ref rect)).yMin + normalized.y * ((Rect)(ref rect)).height);
	}

	private static Vector3 CatmullRom(Vector3 pMinus1, Vector3 p0, Vector3 p1, Vector3 p2, float t)
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		float num = t * t;
		float num2 = num * t;
		return 0.5f * (2f * p0 + (-pMinus1 + p1) * t + (2f * pMinus1 - 5f * p0 + 4f * p1 - p2) * num + (-pMinus1 + 3f * p0 - 3f * p1 + p2) * num2);
	}

	private static Vector3 SampleCatmullRomPath(Vector3[] path, float t)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		if (path == null || path.Length == 0)
		{
			return Vector3.zero;
		}
		if (path.Length == 1)
		{
			return path[0];
		}
		if (path.Length == 2)
		{
			return Vector3.Lerp(path[0], path[1], t);
		}
		int num = path.Length - 2 - 1;
		if (num < 1)
		{
			return path[1];
		}
		float num2 = t * (float)num;
		int num3 = Mathf.FloorToInt(num2);
		if (num3 >= num)
		{
			num3 = num - 1;
		}
		float t2 = num2 - (float)num3;
		return CatmullRom(path[num3], path[num3 + 1], path[num3 + 2], path[num3 + 3], t2);
	}

	private static float CalcPathLength(Vector3[] path)
	{
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		if (path == null || path.Length <= 1)
		{
			return 0f;
		}
		float num = 0f;
		for (int i = 1; i < path.Length; i++)
		{
			num += Vector3.Distance(path[i - 1], path[i]);
		}
		return num;
	}

	private static Vector3[] ExtractPassPoints(Vector3[] path)
	{
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		if (path == null || path.Length <= 2)
		{
			return path;
		}
		Vector3[] array = (Vector3[])(object)new Vector3[path.Length - 2];
		for (int i = 0; i < array.Length; i++)
		{
			array[i] = path[i + 1];
		}
		return array;
	}

	[AsyncStateMachine(typeof(_003CRocketFlyThenFinishAsync_003Ed__57))]
	private UniTaskVoid RocketFlyThenFinishAsync(IGameplayObject target, RectTransform flyParent, Vector3[] path, bool isNearCurve, TrackPropUseReason reason, bool isFastFire)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		_003CRocketFlyThenFinishAsync_003Ed__57 _003CRocketFlyThenFinishAsync_003Ed__ = default(_003CRocketFlyThenFinishAsync_003Ed__57);
		_003CRocketFlyThenFinishAsync_003Ed__._003C_003Et__builder = AsyncUniTaskVoidMethodBuilder.Create();
		_003CRocketFlyThenFinishAsync_003Ed__._003C_003E4__this = this;
		_003CRocketFlyThenFinishAsync_003Ed__.target = target;
		_003CRocketFlyThenFinishAsync_003Ed__.flyParent = flyParent;
		_003CRocketFlyThenFinishAsync_003Ed__.path = path;
		_003CRocketFlyThenFinishAsync_003Ed__.isNearCurve = isNearCurve;
		_003CRocketFlyThenFinishAsync_003Ed__.reason = reason;
		_003CRocketFlyThenFinishAsync_003Ed__.isFastFire = isFastFire;
		_003CRocketFlyThenFinishAsync_003Ed__._003C_003E1__state = -1;
		((AsyncUniTaskVoidMethodBuilder)(ref _003CRocketFlyThenFinishAsync_003Ed__._003C_003Et__builder)).Start<_003CRocketFlyThenFinishAsync_003Ed__57>(ref _003CRocketFlyThenFinishAsync_003Ed__);
		return ((AsyncUniTaskVoidMethodBuilder)(ref _003CRocketFlyThenFinishAsync_003Ed__._003C_003Et__builder)).Task;
	}

	private void FinishFlight(RocketFlightTask task, TrackPropUseReason reason, bool isFastFire, bool hitSuccess)
	{
		if (task.target != null)
		{
			reservedTargets.Remove(task.target);
		}
		if (isFastFire)
		{
			callback?.OnRocketShotHit(reason, hitSuccess);
		}
		else
		{
			RequestFinish((!hitSuccess) ? RotateRingPropUseResult.Failed : RotateRingPropUseResult.Success, hitSuccess);
		}
	}

	private static bool TrySpawnRocket(RectTransform flyParent, Vector3 startUiLocal, out RectTransform rocketRect)
	{
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
		rocketRect = null;
		if ((Object)(object)flyParent == (Object)null)
		{
			return false;
		}
		if (rocketPrefab == null)
		{
			rocketPrefab = GameApp.Res.LoadAsset<GameObject>("eff_rocket", (string)null);
		}
		if ((Object)(object)rocketPrefab == (Object)null)
		{
			return false;
		}
		GameObject val = GameApp.Pool.Spawn(rocketPrefab, (Transform)(object)flyParent);
		if ((Object)(object)val == (Object)null)
		{
			return false;
		}
		Transform transform = val.transform;
		rocketRect = (RectTransform)(object)((transform is RectTransform) ? transform : null);
		if ((Object)(object)rocketRect == (Object)null)
		{
			GameApp.Pool.Recycle(val);
			return false;
		}
		((Transform)rocketRect).localPosition = new Vector3(startUiLocal.x, startUiLocal.y, -1f);
		((Transform)rocketRect).localRotation = Quaternion.identity;
		((Transform)rocketRect).localScale = Vector3.one * UnlockRingPropConfig.Instance.GetRocketScaleCurve().Evaluate(0f);
		ReplayRocketAppearAnimation(val);
		return true;
	}

	private static void ReplayRocketAppearAnimation(GameObject instance)
	{
		Animation componentInChildren = instance.GetComponentInChildren<Animation>();
		if ((Object)(object)componentInChildren != (Object)null)
		{
			componentInChildren.Rewind();
			componentInChildren.Play();
		}
		ParticleSystem[] componentsInChildren = instance.GetComponentsInChildren<ParticleSystem>(true);
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			if ((Object)(object)componentsInChildren[i] != (Object)null)
			{
				componentsInChildren[i].Clear(true);
				componentsInChildren[i].Play(true);
			}
		}
		SkeletonGraphic componentInChildren2 = instance.GetComponentInChildren<SkeletonGraphic>(true);
		if (!((Object)(object)componentInChildren2 != (Object)null))
		{
			return;
		}
		try
		{
			if (componentInChildren2.AnimationState != null)
			{
				componentInChildren2.AnimationState.ClearTrack(0);
			}
			componentInChildren2.startingAnimation = string.Empty;
			componentInChildren2.AnimationState.SetAnimation(0, "zhuan", false);
		}
		catch (Exception)
		{
		}
	}

	private Tween BuildRocketFlyTween(RectTransform rocketRect, Vector3[] path, float duration)
	{
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00be: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
		Vector3[] localPath = NormalizePathZ(path, -1f);
		Vector3 direction = ((localPath.Length >= 4) ? (localPath[2] - localPath[1]) : ((localPath.Length < 2) ? Vector3.up : (localPath[1] - localPath[0])));
		if (((Vector3)(ref direction)).sqrMagnitude > 0.0001f)
		{
			RotateRocketToDirection(rocketRect, direction);
		}
		Vector3 val = ((localPath.Length >= 4) ? localPath[1] : ((localPath.Length >= 2) ? localPath[0] : ((Transform)rocketRect).localPosition));
		((Transform)rocketRect).localPosition = val;
		AnimationCurve flyEase = UnlockRingPropConfig.Instance.GetRocketFlyEase();
		Vector3 lastPosition = val;
		Tween val2 = (Tween)(object)TweenSettingsExtensions.SetEase<Tweener>(DOVirtual.Float(0f, 1f, duration, (TweenCallback<float>)delegate(float t)
		{
			//IL_0014: Unknown result type (might be due to invalid IL or missing references)
			//IL_0019: Unknown result type (might be due to invalid IL or missing references)
			//IL_0020: Unknown result type (might be due to invalid IL or missing references)
			//IL_0026: Unknown result type (might be due to invalid IL or missing references)
			//IL_0028: Unknown result type (might be due to invalid IL or missing references)
			//IL_002d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0032: Unknown result type (might be due to invalid IL or missing references)
			//IL_0034: Unknown result type (might be due to invalid IL or missing references)
			//IL_0035: Unknown result type (might be due to invalid IL or missing references)
			//IL_004e: Unknown result type (might be due to invalid IL or missing references)
			try
			{
				float t2 = flyEase.Evaluate(t);
				Vector3 val4 = SampleCatmullRomPath(localPath, t2);
				((Transform)rocketRect).localPosition = val4;
				Vector3 direction2 = val4 - lastPosition;
				lastPosition = val4;
				if (((Vector3)(ref direction2)).sqrMagnitude > 0.0001f)
				{
					RotateRocketToDirection(rocketRect, direction2);
				}
			}
			catch (Exception ex)
			{
				Log.LogError("[RocketPropHandler] ", "礼炮飞行采样异常: {0}", new object[1] { ex.Message });
			}
		}), (Ease)1);
		AnimationCurve scaleCurve = UnlockRingPropConfig.Instance.GetRocketScaleCurve();
		float scaleT = 0f;
		Tween val3 = (Tween)(object)TweenSettingsExtensions.SetEase<TweenerCore<float, float, FloatOptions>>(DOTween.To((DOGetter<float>)(() => scaleT), (DOSetter<float>)delegate(float t)
		{
			//IL_000d: Unknown result type (might be due to invalid IL or missing references)
			//IL_001e: Unknown result type (might be due to invalid IL or missing references)
			scaleT = t;
			try
			{
				((Transform)rocketRect).localScale = Vector3.one * scaleCurve.Evaluate(t);
			}
			catch (Exception ex)
			{
				Log.LogError("[RocketPropHandler] ", "礼炮缩放异常: {0}", new object[1] { ex.Message });
			}
		}, 1f, duration), (Ease)1);
		return (Tween)(object)TweenSettingsExtensions.Join(TweenSettingsExtensions.Join(TweenSettingsExtensions.SetLink<Sequence>(TweenSettingsExtensions.SetTarget<Sequence>(DOTween.Sequence(), (object)((Component)rocketRect).gameObject), ((Component)rocketRect).gameObject, (LinkBehaviour)5), val2), val3);
	}

	private static void RotateRocketToDirection(RectTransform rocketRect, Vector3 direction)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		float num = Mathf.Atan2(direction.y, direction.x) * 57.29578f - 90f;
		((Transform)rocketRect).localRotation = Quaternion.Euler(0f, 0f, num);
	}

	private static Vector3[] NormalizePathZ(Vector3[] path, float z)
	{
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		Vector3[] array = (Vector3[])(object)new Vector3[path.Length];
		for (int i = 0; i < path.Length; i++)
		{
			array[i] = new Vector3(path[i].x, path[i].y, z);
		}
		return array;
	}

	private static RectTransform TryGetUIGameCenterContent()
	{
		IController val = default(IController);
		if (!GameApp.UI.IsOpen("UIGame", ref val) || !(val is UIGameController uIGameController))
		{
			return null;
		}
		return uIGameController.View?.CenterContent;
	}

	private static RectTransform TryResolveRingRect()
	{
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		IController val = default(IController);
		if (!GameApp.UI.IsOpen("UIGame", ref val) || !(val is UIGameController uIGameController))
		{
			return null;
		}
		RectTransform val2 = uIGameController.View?.RingRect;
		if ((Object)(object)val2 != (Object)null)
		{
			Rect rect = val2.rect;
			if (((Rect)(ref rect)).width > 0.01f)
			{
				rect = val2.rect;
				if (((Rect)(ref rect)).height > 0.01f)
				{
					return val2;
				}
			}
		}
		return null;
	}

	private void PlayBoomEffect(RectTransform parent, Vector3 localPos)
	{
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d0: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)parent == (Object)null)
		{
			return;
		}
		if (boomEffectPrefab == null)
		{
			boomEffectPrefab = GameApp.Res.LoadAsset<GameObject>("eff_rocket_boom_01", (string)null);
		}
		if ((Object)(object)boomEffectPrefab == (Object)null)
		{
			return;
		}
		GameObject val = GameApp.Pool.Spawn(boomEffectPrefab, (Transform)(object)parent);
		if ((Object)(object)val == (Object)null)
		{
			return;
		}
		val.transform.localPosition = localPos;
		val.transform.localRotation = Quaternion.identity;
		val.transform.localScale = Vector3.one;
		ParticleSystem[] componentsInChildren = val.GetComponentsInChildren<ParticleSystem>(true);
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			if ((Object)(object)componentsInChildren[i] != (Object)null)
			{
				componentsInChildren[i].Play(true);
			}
		}
		float particleSystemDuration = GetParticleSystemDuration(val);
		SingletonComponent<GameplayManager>.Instance?.ScreenShake?.RequestShake(ScreenShakeTriggerType.RocketHit);
		UniTaskVoid val2 = RecycleBoomEffectAfterDelayAsync(val, particleSystemDuration);
		((UniTaskVoid)(ref val2)).Forget();
	}

	private static float GetParticleSystemDuration(GameObject instance)
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Invalid comparison between Unknown and I4
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		float num = 0f;
		ParticleSystem[] componentsInChildren = instance.GetComponentsInChildren<ParticleSystem>(true);
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			MainModule main = componentsInChildren[i].main;
			MinMaxCurve startLifetime = ((MainModule)(ref main)).startLifetime;
			float num2;
			if ((int)((MinMaxCurve)(ref startLifetime)).mode != 3)
			{
				startLifetime = ((MainModule)(ref main)).startLifetime;
				num2 = ((MinMaxCurve)(ref startLifetime)).constant;
			}
			else
			{
				startLifetime = ((MainModule)(ref main)).startLifetime;
				num2 = ((MinMaxCurve)(ref startLifetime)).constantMax;
			}
			float num3 = num2;
			num = Mathf.Max(num, ((MainModule)(ref main)).duration + num3);
		}
		if (!(num > 0f))
		{
			return 2f;
		}
		return num;
	}

	[AsyncStateMachine(typeof(_003CRecycleBoomEffectAfterDelayAsync_003Ed__68))]
	private static UniTaskVoid RecycleBoomEffectAfterDelayAsync(GameObject instance, float delaySeconds)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		_003CRecycleBoomEffectAfterDelayAsync_003Ed__68 _003CRecycleBoomEffectAfterDelayAsync_003Ed__ = default(_003CRecycleBoomEffectAfterDelayAsync_003Ed__68);
		_003CRecycleBoomEffectAfterDelayAsync_003Ed__._003C_003Et__builder = AsyncUniTaskVoidMethodBuilder.Create();
		_003CRecycleBoomEffectAfterDelayAsync_003Ed__.instance = instance;
		_003CRecycleBoomEffectAfterDelayAsync_003Ed__.delaySeconds = delaySeconds;
		_003CRecycleBoomEffectAfterDelayAsync_003Ed__._003C_003E1__state = -1;
		((AsyncUniTaskVoidMethodBuilder)(ref _003CRecycleBoomEffectAfterDelayAsync_003Ed__._003C_003Et__builder)).Start<_003CRecycleBoomEffectAfterDelayAsync_003Ed__68>(ref _003CRecycleBoomEffectAfterDelayAsync_003Ed__);
		return ((AsyncUniTaskVoidMethodBuilder)(ref _003CRecycleBoomEffectAfterDelayAsync_003Ed__._003C_003Et__builder)).Task;
	}

	private void RecycleRocketInstance(RectTransform rocketRect)
	{
		if (!((Object)(object)rocketRect == (Object)null))
		{
			DOTween.Kill((object)((Component)rocketRect).gameObject, false);
			GameApp.Pool.Recycle(((Component)rocketRect).gameObject);
		}
	}

	private void ClearReservedTargets()
	{
		reservedTargets.Clear();
	}

	private void CancelAllFlights()
	{
		for (int num = activeTasks.Count - 1; num >= 0; num--)
		{
			RocketFlightTask rocketFlightTask = activeTasks[num];
			if (rocketFlightTask != null)
			{
				rocketFlightTask.isFlying = false;
				rocketFlightTask.cts?.Cancel();
				rocketFlightTask.cts?.Dispose();
				rocketFlightTask.cts = null;
				if ((Object)(object)rocketFlightTask.rocketRect != (Object)null)
				{
					RecycleRocketInstance(rocketFlightTask.rocketRect);
					rocketFlightTask.rocketRect = null;
				}
			}
		}
		activeTasks.Clear();
	}

	private void RequestFinish(RotateRingPropUseResult result, bool consumeOnSuccess)
	{
		if (!isFinishing && callback != null)
		{
			isFinishing = true;
			callback.RequestFinish(result, consumeOnSuccess);
		}
	}
}
