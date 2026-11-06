using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.CompilerServices;
using RotateRings.GamePlay.Events;
using RotateRings.UI.Game;
using UniLog;
using UnityEngine;
using XY.Common.ParticleImage;
using XingYun.UI.Controller;

namespace RotateRings.GamePlay.Services;

internal sealed class RingPaletteLockKeyUnlockFly : IDisposable
{
	private struct UnlockFlyHandle
	{
		public RingObject TargetRing;

		public GameObject Instance;

		public RectTransform Attractor;

		public int Lease;

		public bool Completed;
	}

	private struct PreparedFly
	{
		public RingObject[] LockedRings;

		public Vector3[] LockHeartWorldPositions;

		public PaletteLockKeyColor KeyColor;
	}

	[StructLayout(LayoutKind.Auto)]
	[CompilerGenerated]
	private struct _003CRunKeyUnlockFliesAsync_003Ed__27 : IAsyncStateMachine
	{
		public int _003C_003E1__state;

		public AsyncUniTaskVoidMethodBuilder _003C_003Et__builder;

		public IReadOnlyList<RingObject> lockedRings;

		public CancellationTokenSource runCts;

		public RingPaletteLockKeyUnlockFly _003C_003E4__this;

		public Vector3[] lockHeartWorldPositions;

		public Vector3 keyWorldPosition;

		public PaletteLockKeyColor keyColor;

		private CancellationToken _003Cct_003E5__2;

		private RingObject[] _003CringQueue_003E5__3;

		private Vector3[] _003ClockHeartQueue_003E5__4;

		private int _003Ci_003E5__5;

		private UIGameView _003CgameView_003E5__6;

		private Awaiter<bool> _003C_003Eu__1;

		private void MoveNext()
		{
			//IL_01bc: Unknown result type (might be due to invalid IL or missing references)
			//IL_01c1: Unknown result type (might be due to invalid IL or missing references)
			//IL_01c9: Unknown result type (might be due to invalid IL or missing references)
			//IL_0223: Unknown result type (might be due to invalid IL or missing references)
			//IL_0235: Unknown result type (might be due to invalid IL or missing references)
			//IL_0175: Unknown result type (might be due to invalid IL or missing references)
			//IL_017a: Unknown result type (might be due to invalid IL or missing references)
			//IL_017e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0183: Unknown result type (might be due to invalid IL or missing references)
			//IL_0187: Unknown result type (might be due to invalid IL or missing references)
			//IL_018c: Unknown result type (might be due to invalid IL or missing references)
			//IL_00de: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ec: Unknown result type (might be due to invalid IL or missing references)
			//IL_01a1: Unknown result type (might be due to invalid IL or missing references)
			//IL_01a3: Unknown result type (might be due to invalid IL or missing references)
			//IL_00f1: Unknown result type (might be due to invalid IL or missing references)
			int num = _003C_003E1__state;
			RingPaletteLockKeyUnlockFly ringPaletteLockKeyUnlockFly = _003C_003E4__this;
			try
			{
				if (num == 0)
				{
					goto IL_010c;
				}
				if (lockedRings != null && lockedRings.Count != 0 && runCts != null)
				{
					_003Cct_003E5__2 = runCts.Token;
					_003CringQueue_003E5__3 = new RingObject[lockedRings.Count];
					_003ClockHeartQueue_003E5__4 = (Vector3[])(object)new Vector3[lockedRings.Count];
					for (int i = 0; i < lockedRings.Count; i++)
					{
						_003CringQueue_003E5__3[i] = lockedRings[i];
						_003ClockHeartQueue_003E5__4[i] = ((lockHeartWorldPositions != null && i < lockHeartWorldPositions.Length) ? lockHeartWorldPositions[i] : (((Object)(object)lockedRings[i] != (Object)null) ? ((Component)lockedRings[i]).transform.position : keyWorldPosition));
					}
					goto IL_010c;
				}
				ringPaletteLockKeyUnlockFly.ReleaseKeyUnlockFlyRunCts(runCts);
				goto end_IL_000e;
				IL_010c:
				try
				{
					if (num != 0)
					{
						_003Ci_003E5__5 = 0;
						goto IL_0278;
					}
					Awaiter<bool> val = _003C_003Eu__1;
					_003C_003Eu__1 = default(Awaiter<bool>);
					num = (_003C_003E1__state = -1);
					goto IL_01d8;
					IL_01e6:
					RingObject ringObject = _003CringQueue_003E5__3[_003Ci_003E5__5];
					if (!((Object)(object)ringObject == (Object)null))
					{
						if (ringObject.IsSolved && !ringPaletteLockKeyUnlockFly.owner.IsKeyUnlockFlyPending(ringObject))
						{
							ringPaletteLockKeyUnlockFly.owner.CompleteUnlockForRing(ringObject);
						}
						else
						{
							if (!ringPaletteLockKeyUnlockFly.TryPlayFlyToLock(keyWorldPosition, ringObject, _003ClockHeartQueue_003E5__4[_003Ci_003E5__5], keyColor, _003CgameView_003E5__6, _003Cct_003E5__2))
							{
								ringPaletteLockKeyUnlockFly.owner.CompleteUnlockForRing(ringObject);
							}
							_003CgameView_003E5__6 = null;
						}
					}
					_003Ci_003E5__5++;
					goto IL_0278;
					IL_01d8:
					if (!val.GetResult())
					{
						goto IL_01e6;
					}
					goto end_IL_010c;
					IL_0278:
					if (_003Ci_003E5__5 < _003CringQueue_003E5__3.Length && !_003Cct_003E5__2.IsCancellationRequested)
					{
						if (ringPaletteLockKeyUnlockFly.TryResolveGameView(out _003CgameView_003E5__6))
						{
							if (_003Ci_003E5__5 > 0)
							{
								UniTask val2 = UniTask.Delay(TimeSpan.FromSeconds(ringPaletteLockKeyUnlockFly.FlyStartIntervalSeconds), false, (PlayerLoopTiming)8, _003Cct_003E5__2);
								val = ((UniTask)(ref val2)).SuppressCancellationThrow().GetAwaiter();
								if (!val.IsCompleted)
								{
									num = (_003C_003E1__state = 0);
									_003C_003Eu__1 = val;
									((AsyncUniTaskVoidMethodBuilder)(ref _003C_003Et__builder)).AwaitUnsafeOnCompleted<Awaiter<bool>, _003CRunKeyUnlockFliesAsync_003Ed__27>(ref val, ref this);
									return;
								}
								goto IL_01d8;
							}
							goto IL_01e6;
						}
						ringPaletteLockKeyUnlockFly.CompleteUnlockForRemaining(_003CringQueue_003E5__3, _003Ci_003E5__5);
					}
					end_IL_010c:;
				}
				catch (Exception ex) when (!(ex is OperationCanceledException))
				{
					Log.LogError("[RingPaletteLockKeyUnlockFly] ", "钥匙解锁飞行队列异常: {0}", new object[1] { ex });
					ringPaletteLockKeyUnlockFly.CompleteUnlockForRemaining(_003CringQueue_003E5__3, 0);
				}
				finally
				{
					if (num < 0)
					{
						ringPaletteLockKeyUnlockFly.ReleaseKeyUnlockFlyRunCts(runCts);
					}
				}
				end_IL_000e:;
			}
			catch (Exception exception)
			{
				_003C_003E1__state = -2;
				_003Cct_003E5__2 = default(CancellationToken);
				_003CringQueue_003E5__3 = null;
				_003ClockHeartQueue_003E5__4 = null;
				((AsyncUniTaskVoidMethodBuilder)(ref _003C_003Et__builder)).SetException(exception);
				return;
			}
			_003C_003E1__state = -2;
			_003Cct_003E5__2 = default(CancellationToken);
			_003CringQueue_003E5__3 = null;
			_003ClockHeartQueue_003E5__4 = null;
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
	private struct _003CWaitFlyTimeoutAsync_003Ed__30 : IAsyncStateMachine
	{
		public int _003C_003E1__state;

		public AsyncUniTaskVoidMethodBuilder _003C_003Et__builder;

		public float timeoutSeconds;

		public CancellationToken ct;

		public RingPaletteLockKeyUnlockFly _003C_003E4__this;

		public int lease;

		private Awaiter<bool> _003C_003Eu__1;

		private void MoveNext()
		{
			//IL_0098: Unknown result type (might be due to invalid IL or missing references)
			//IL_009d: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
			//IL_0057: Unknown result type (might be due to invalid IL or missing references)
			//IL_005c: Unknown result type (might be due to invalid IL or missing references)
			//IL_005f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0064: Unknown result type (might be due to invalid IL or missing references)
			//IL_0068: Unknown result type (might be due to invalid IL or missing references)
			//IL_006d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0081: Unknown result type (might be due to invalid IL or missing references)
			//IL_0082: Unknown result type (might be due to invalid IL or missing references)
			int num = _003C_003E1__state;
			RingPaletteLockKeyUnlockFly ringPaletteLockKeyUnlockFly = _003C_003E4__this;
			try
			{
				Awaiter<bool> val2;
				if (num != 0)
				{
					if (float.IsNaN(timeoutSeconds) || float.IsInfinity(timeoutSeconds))
					{
						timeoutSeconds = 3f;
					}
					UniTask val = UniTask.Delay(TimeSpan.FromSeconds(Mathf.Max(timeoutSeconds, 0.1f)), false, (PlayerLoopTiming)8, ct);
					val2 = ((UniTask)(ref val)).SuppressCancellationThrow().GetAwaiter();
					if (!val2.IsCompleted)
					{
						num = (_003C_003E1__state = 0);
						_003C_003Eu__1 = val2;
						((AsyncUniTaskVoidMethodBuilder)(ref _003C_003Et__builder)).AwaitUnsafeOnCompleted<Awaiter<bool>, _003CWaitFlyTimeoutAsync_003Ed__30>(ref val2, ref this);
						return;
					}
				}
				else
				{
					val2 = _003C_003Eu__1;
					_003C_003Eu__1 = default(Awaiter<bool>);
					num = (_003C_003E1__state = -1);
				}
				if (!val2.GetResult())
				{
					ringPaletteLockKeyUnlockFly.FinishUnlockFlyHandle(lease, fromTimeout: true);
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

	private const string Tag = "[RingPaletteLockKeyUnlockFly] ";

	private const string FlyEffectLocation = "eff_GoCollection_fly_key";

	private const string FlySequenceSpriteBlue = "tex_sequence_blue";

	private const string FlySequenceSpriteGreen = "tex_sequence_green";

	private const string FlySequenceSpriteRed = "tex_sequence_red";

	private const string FlySequenceSpriteYellow = "tex_sequence_yellow";

	private const int FlyEmitCount = 1;

	private const int KeyUnlockFlySortOffset = 10;

	private readonly RingPaletteLockService owner;

	private readonly List<UnlockFlyHandle> unlockFlyHandles = new List<UnlockFlyHandle>(4);

	private readonly List<CancellationTokenSource> keyUnlockFlyRunCtsList = new List<CancellationTokenSource>(4);

	private readonly Stack<RectTransform> flyTargetAnchorPool = new Stack<RectTransform>(8);

	private readonly Dictionary<RingObject, PreparedFly> preparedFliesByKeyRing = new Dictionary<RingObject, PreparedFly>(4);

	private GameObject flyEffectPrefab;

	private int flyLeaseCounter;

	private float FlyStartIntervalSeconds => RingLevelConfig.Instance.paletteLockKeyUnlockFlyStartIntervalSeconds;

	private float FlyTimeoutPaddingSeconds => RingLevelConfig.Instance.paletteLockKeyUnlockFlyTimeoutPaddingSeconds;

	public RingPaletteLockKeyUnlockFly(RingPaletteLockService owner)
	{
		this.owner = owner ?? throw new ArgumentNullException("owner");
		GameApp.Event.Listen<UIGameRemovedEvent>((Action<UIGameRemovedEvent>)OnUIGameRemoved);
	}

	internal bool PrepareKeyUnlockFly(RingObject keyRing, IReadOnlyList<RingObject> lockedRings)
	{
		if (lockedRings == null || lockedRings.Count == 0 || (Object)(object)keyRing == (Object)null)
		{
			return false;
		}
		if (!TryResolveGameView(out var gameView) || !EnsureFlyPrefab(gameView))
		{
			FallbackUnlockAll(lockedRings);
			return false;
		}
		if (!owner.TryGetKeyColor(keyRing, out var color))
		{
			color = PaletteLockKeyColor.Red;
		}
		RingObject[] array = new RingObject[lockedRings.Count];
		for (int i = 0; i < lockedRings.Count; i++)
		{
			array[i] = lockedRings[i];
			owner.MarkKeyUnlockFlyPending(lockedRings[i]);
		}
		Vector3[] array2 = (Vector3[])(object)new Vector3[lockedRings.Count];
		for (int j = 0; j < lockedRings.Count; j++)
		{
			owner.TryGetLockHeartWorldPosition(lockedRings[j], out array2[j]);
		}
		preparedFliesByKeyRing[keyRing] = new PreparedFly
		{
			LockedRings = array,
			LockHeartWorldPositions = array2,
			KeyColor = color
		};
		return true;
	}

	internal void PlayPreparedKeyUnlockFly(RingObject keyRing)
	{
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		if (!((Object)(object)keyRing == (Object)null) && preparedFliesByKeyRing.TryGetValue(keyRing, out var value))
		{
			preparedFliesByKeyRing.Remove(keyRing);
			CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
			keyUnlockFlyRunCtsList.Add(cancellationTokenSource);
			Vector3 position = ((Component)keyRing).transform.position;
			UniTaskVoid val = RunKeyUnlockFliesAsync(position, value.LockedRings, value.LockHeartWorldPositions, value.KeyColor, cancellationTokenSource);
			((UniTaskVoid)(ref val)).Forget();
		}
	}

	internal void ClearLevel()
	{
		CancelAllKeyUnlockFlyRuns();
		CancelAllActiveSessions(completeUnlock: false);
		preparedFliesByKeyRing.Clear();
		owner.ClearKeyUnlockFlyPending();
	}

	public void Dispose()
	{
		GameApp.Event.Unlisten<UIGameRemovedEvent>((Action<UIGameRemovedEvent>)OnUIGameRemoved);
		ClearLevel();
	}

	private void OnUIGameRemoved(UIGameRemovedEvent evt)
	{
		ClearFlyTargetAnchorPoolReferences();
	}

	private void CancelAllKeyUnlockFlyRuns()
	{
		for (int num = keyUnlockFlyRunCtsList.Count - 1; num >= 0; num--)
		{
			CancellationTokenSource cancellationTokenSource = keyUnlockFlyRunCtsList[num];
			if (cancellationTokenSource != null)
			{
				cancellationTokenSource.Cancel();
				cancellationTokenSource.Dispose();
			}
		}
		keyUnlockFlyRunCtsList.Clear();
	}

	private void ReleaseKeyUnlockFlyRunCts(CancellationTokenSource runCts)
	{
		if (runCts != null)
		{
			keyUnlockFlyRunCtsList.Remove(runCts);
			_ = runCts.IsCancellationRequested;
			runCts.Dispose();
		}
	}

	[AsyncStateMachine(typeof(_003CRunKeyUnlockFliesAsync_003Ed__27))]
	private UniTaskVoid RunKeyUnlockFliesAsync(Vector3 keyWorldPosition, IReadOnlyList<RingObject> lockedRings, Vector3[] lockHeartWorldPositions, PaletteLockKeyColor keyColor, CancellationTokenSource runCts)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		_003CRunKeyUnlockFliesAsync_003Ed__27 _003CRunKeyUnlockFliesAsync_003Ed__ = default(_003CRunKeyUnlockFliesAsync_003Ed__27);
		_003CRunKeyUnlockFliesAsync_003Ed__._003C_003Et__builder = AsyncUniTaskVoidMethodBuilder.Create();
		_003CRunKeyUnlockFliesAsync_003Ed__._003C_003E4__this = this;
		_003CRunKeyUnlockFliesAsync_003Ed__.keyWorldPosition = keyWorldPosition;
		_003CRunKeyUnlockFliesAsync_003Ed__.lockedRings = lockedRings;
		_003CRunKeyUnlockFliesAsync_003Ed__.lockHeartWorldPositions = lockHeartWorldPositions;
		_003CRunKeyUnlockFliesAsync_003Ed__.keyColor = keyColor;
		_003CRunKeyUnlockFliesAsync_003Ed__.runCts = runCts;
		_003CRunKeyUnlockFliesAsync_003Ed__._003C_003E1__state = -1;
		((AsyncUniTaskVoidMethodBuilder)(ref _003CRunKeyUnlockFliesAsync_003Ed__._003C_003Et__builder)).Start<_003CRunKeyUnlockFliesAsync_003Ed__27>(ref _003CRunKeyUnlockFliesAsync_003Ed__);
		return ((AsyncUniTaskVoidMethodBuilder)(ref _003CRunKeyUnlockFliesAsync_003Ed__._003C_003Et__builder)).Task;
	}

	private void CompleteUnlockForRemaining(RingObject[] rings, int fromIndex)
	{
		if (rings != null)
		{
			for (int i = fromIndex; i < rings.Length; i++)
			{
				owner.CompleteUnlockForRing(rings[i]);
			}
		}
	}

	private bool TryPlayFlyToLock(Vector3 keyWorldPosition, RingObject lockedRing, Vector3 lockHeartWorldPosition, PaletteLockKeyColor keyColor, UIGameView gameView, CancellationToken ct)
	{
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d4: Unknown result type (might be due to invalid IL or missing references)
		RectTransform val = gameView.CenterContent ?? gameView.RingRect;
		if ((Object)(object)val == (Object)null)
		{
			return false;
		}
		if (!TryWorldToFlyParentLocal(keyWorldPosition, val, out var localPosition) || !TryWorldToFlyParentLocal(lockHeartWorldPosition, val, out var localPosition2))
		{
			return false;
		}
		RectTransform val2 = RentFlyTargetAnchor(val, localPosition2);
		GameObject val3 = GameApp.Pool.Spawn(flyEffectPrefab, (Transform)(object)val);
		if ((Object)(object)val3 == (Object)null)
		{
			RecycleFlyTargetAnchor(val2);
			return false;
		}
		Transform transform = val3.transform;
		ApplyKeyUnlockFlySortOrder(val3, val, unlockFlyHandles.Count);
		SetFlyParentLocalPosition(transform, localPosition);
		transform.localRotation = Quaternion.identity;
		transform.localScale = flyEffectPrefab.transform.localScale;
		val3.SetActive(false);
		XYParticleImage component = val3.GetComponent<XYParticleImage>();
		if ((Object)(object)component == (Object)null)
		{
			RecycleFlyInstance(val3);
			RecycleFlyTargetAnchor(val2);
			return false;
		}
		int lease = ++flyLeaseCounter;
		unlockFlyHandles.Add(new UnlockFlyHandle
		{
			TargetRing = lockedRing,
			Instance = val3,
			Attractor = val2,
			Lease = lease
		});
		component.PlayMode = PlayMode.None;
		if (!TryApplyFlySequenceSprite(component, keyColor))
		{
			RecycleFlyInstance(val3);
			RecycleFlyTargetAnchor(val2);
			return false;
		}
		val3.SetActive(true);
		component.Stop(stopAndClear: true);
		component.Play(1, (Transform)(object)val2, null, null, delegate
		{
			FinishUnlockFlyHandle(lease, fromTimeout: false);
		});
		if (!component.isPlaying && !component.isEmitting)
		{
			FinishUnlockFlyHandle(lease, fromTimeout: true);
			return false;
		}
		float timeoutSeconds = component.GetTotalPlaybackDurationForFly(transform, (Transform)(object)val2) + FlyTimeoutPaddingSeconds;
		UniTaskVoid val4 = WaitFlyTimeoutAsync(lease, timeoutSeconds, ct);
		((UniTaskVoid)(ref val4)).Forget();
		return true;
	}

	[AsyncStateMachine(typeof(_003CWaitFlyTimeoutAsync_003Ed__30))]
	private UniTaskVoid WaitFlyTimeoutAsync(int lease, float timeoutSeconds, CancellationToken ct)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		_003CWaitFlyTimeoutAsync_003Ed__30 _003CWaitFlyTimeoutAsync_003Ed__ = default(_003CWaitFlyTimeoutAsync_003Ed__30);
		_003CWaitFlyTimeoutAsync_003Ed__._003C_003Et__builder = AsyncUniTaskVoidMethodBuilder.Create();
		_003CWaitFlyTimeoutAsync_003Ed__._003C_003E4__this = this;
		_003CWaitFlyTimeoutAsync_003Ed__.lease = lease;
		_003CWaitFlyTimeoutAsync_003Ed__.timeoutSeconds = timeoutSeconds;
		_003CWaitFlyTimeoutAsync_003Ed__.ct = ct;
		_003CWaitFlyTimeoutAsync_003Ed__._003C_003E1__state = -1;
		((AsyncUniTaskVoidMethodBuilder)(ref _003CWaitFlyTimeoutAsync_003Ed__._003C_003Et__builder)).Start<_003CWaitFlyTimeoutAsync_003Ed__30>(ref _003CWaitFlyTimeoutAsync_003Ed__);
		return ((AsyncUniTaskVoidMethodBuilder)(ref _003CWaitFlyTimeoutAsync_003Ed__._003C_003Et__builder)).Task;
	}

	private void FinishUnlockFlyHandle(int lease, bool fromTimeout)
	{
		if (TryTakeUnlockFlyHandle(lease, out var handle, out var handleIndex))
		{
			RingObject targetRing = handle.TargetRing;
			RecycleFlyInstance(handle.Instance);
			RecycleFlyTargetAnchor(handle.Attractor);
			unlockFlyHandles.RemoveAt(handleIndex);
			owner.CompleteUnlockForRing(targetRing);
		}
	}

	private bool TryTakeUnlockFlyHandle(int lease, out UnlockFlyHandle handle, out int handleIndex)
	{
		for (int i = 0; i < unlockFlyHandles.Count; i++)
		{
			if (unlockFlyHandles[i].Lease == lease)
			{
				handle = unlockFlyHandles[i];
				if (handle.Completed)
				{
					handle = default(UnlockFlyHandle);
					handleIndex = -1;
					return false;
				}
				handle.Completed = true;
				unlockFlyHandles[i] = handle;
				handleIndex = i;
				return true;
			}
		}
		handle = default(UnlockFlyHandle);
		handleIndex = -1;
		return false;
	}

	private static void ApplyKeyUnlockFlySortOrder(GameObject instance, RectTransform flyParent, int activeConcurrentCount)
	{
		if (!((Object)(object)instance == (Object)null))
		{
			Canvas val = default(Canvas);
			if (!instance.TryGetComponent<Canvas>(ref val))
			{
				val = instance.AddComponent<Canvas>();
			}
			val.overrideSorting = true;
			Canvas val2 = (((Object)(object)flyParent != (Object)null) ? ((Component)flyParent).GetComponentInParent<Canvas>() : null);
			int num = (((Object)(object)val2 != (Object)null) ? val2.sortingOrder : 0);
			val.sortingOrder = num + 10 - activeConcurrentCount;
		}
	}

	private bool TryWorldToFlyParentLocal(Vector3 worldPosition, RectTransform flyParent, out Vector2 localPosition)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		localPosition = default(Vector2);
		if ((Object)(object)flyParent == (Object)null)
		{
			return false;
		}
		Camera gameplayCamera = GameplayCameraManager.GameplayCamera;
		if ((Object)(object)gameplayCamera == (Object)null)
		{
			return false;
		}
		Vector2 val = RectTransformUtility.WorldToScreenPoint(gameplayCamera, worldPosition);
		Canvas componentInParent = ((Component)flyParent).GetComponentInParent<Canvas>();
		Camera val2 = (((Object)(object)componentInParent != (Object)null) ? componentInParent.rootCanvas.worldCamera : null);
		return RectTransformUtility.ScreenPointToLocalPointInRectangle(flyParent, val, val2, ref localPosition);
	}

	private void SetFlyParentLocalPosition(Transform target, Vector2 localPosition)
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		RectTransform val = (RectTransform)(object)((target is RectTransform) ? target : null);
		if (val != null)
		{
			val.anchoredPosition = localPosition;
		}
		else
		{
			target.localPosition = new Vector3(localPosition.x, localPosition.y, target.localPosition.z);
		}
	}

	private RectTransform RentFlyTargetAnchor(RectTransform flyParent, Vector2 localPosition)
	{
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		RectTransform obj = ((flyTargetAnchorPool.Count > 0) ? flyTargetAnchorPool.Pop() : CreateFlyTargetAnchor(flyParent));
		((Transform)obj).SetParent((Transform)(object)flyParent, false);
		obj.anchorMin = new Vector2(0.5f, 0.5f);
		obj.anchorMax = new Vector2(0.5f, 0.5f);
		obj.pivot = new Vector2(0.5f, 0.5f);
		obj.sizeDelta = Vector2.zero;
		obj.anchoredPosition = localPosition;
		((Component)obj).gameObject.SetActive(true);
		return obj;
	}

	private void RecycleFlyTargetAnchor(RectTransform anchor)
	{
		if (!((Object)(object)anchor == (Object)null))
		{
			((Component)anchor).gameObject.SetActive(false);
			flyTargetAnchorPool.Push(anchor);
		}
	}

	private void ClearFlyTargetAnchorPoolReferences()
	{
		flyTargetAnchorPool.Clear();
	}

	private static RectTransform CreateFlyTargetAnchor(RectTransform flyParent)
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		RectTransform component = new GameObject("PaletteLockKeyFlyTarget", new Type[1] { typeof(RectTransform) }).GetComponent<RectTransform>();
		if ((Object)(object)flyParent != (Object)null)
		{
			((Transform)component).SetParent((Transform)(object)flyParent, false);
		}
		return component;
	}

	private void FallbackUnlockAll(IReadOnlyList<RingObject> lockedRings)
	{
		for (int i = 0; i < lockedRings.Count; i++)
		{
			owner.CompleteUnlockForRing(lockedRings[i]);
		}
	}

	private void CancelAllActiveSessions(bool completeUnlock)
	{
		for (int num = unlockFlyHandles.Count - 1; num >= 0; num--)
		{
			UnlockFlyHandle unlockFlyHandle = unlockFlyHandles[num];
			if (completeUnlock)
			{
				owner.CompleteUnlockForRing(unlockFlyHandle.TargetRing);
			}
			if ((Object)(object)unlockFlyHandle.Instance != (Object)null)
			{
				unlockFlyHandle.Instance.GetComponent<XYParticleImage>()?.Stop(stopAndClear: true);
				RecycleFlyInstance(unlockFlyHandle.Instance);
			}
			RecycleFlyTargetAnchor(unlockFlyHandle.Attractor);
		}
		unlockFlyHandles.Clear();
	}

	private void RecycleFlyInstance(GameObject instance)
	{
		if (!((Object)(object)instance == (Object)null))
		{
			instance.GetComponent<XYParticleImage>()?.Stop(stopAndClear: true);
			GameApp.Pool.Recycle(instance);
		}
	}

	private bool EnsureFlyPrefab(UIGameView gameView)
	{
		if ((Object)(object)flyEffectPrefab != (Object)null)
		{
			return true;
		}
		flyEffectPrefab = GameApp.Res.LoadAsset<GameObject>("eff_GoCollection_fly_key", gameView.LoadAssetGroup);
		return (Object)(object)flyEffectPrefab != (Object)null;
	}

	private bool TryApplyFlySequenceSprite(XYParticleImage particle, PaletteLockKeyColor keyColor)
	{
		if ((Object)(object)particle == (Object)null)
		{
			return false;
		}
		string flySequenceSpriteLocation = GetFlySequenceSpriteLocation(keyColor);
		if (!string.IsNullOrEmpty(flySequenceSpriteLocation))
		{
			return particle.LoadRuntimeSprite(flySequenceSpriteLocation);
		}
		return false;
	}

	private string GetFlySequenceSpriteLocation(PaletteLockKeyColor keyColor)
	{
		return keyColor switch
		{
			PaletteLockKeyColor.Blue => "tex_sequence_blue", 
			PaletteLockKeyColor.Green => "tex_sequence_green", 
			PaletteLockKeyColor.Red => "tex_sequence_red", 
			PaletteLockKeyColor.Yellow => "tex_sequence_yellow", 
			_ => "tex_sequence_red", 
		};
	}

	private bool TryResolveGameView(out UIGameView gameView)
	{
		gameView = null;
		IController val = default(IController);
		if (!GameApp.UI.IsOpen("UIGame", ref val) || !(val is UIGameController uIGameController))
		{
			return false;
		}
		gameView = uIGameController.View;
		return gameView != null;
	}
}
