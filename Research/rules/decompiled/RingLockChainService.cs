using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.CompilerServices;
using RotateRings.Common;
using RotateRings.GamePlay.Events;
using RotateRings.GamePlay.Services;
using RotateRings.Services;
using UniLog;
using UnityEngine;

public class RingLockChainService : GameplayServiceBase<GameplayManager>
{
	private readonly struct RingOrderEntry
	{
		public readonly int Frame;

		public readonly int Order;

		public RingOrderEntry(int frame, int order)
		{
			Frame = frame;
			Order = order;
		}
	}

	[StructLayout(LayoutKind.Auto)]
	[CompilerGenerated]
	private struct _003CPlayChainShakeSequentially_003Ed__39 : IAsyncStateMachine
	{
		public int _003C_003E1__state;

		public AsyncUniTaskMethodBuilder _003C_003Et__builder;

		public RingLockChainService _003C_003E4__this;

		public List<RingObject> rings;

		private int _003Ci_003E5__2;

		private Awaiter<bool> _003C_003Eu__1;

		private void MoveNext()
		{
			//IL_008d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0092: Unknown result type (might be due to invalid IL or missing references)
			//IL_0099: Unknown result type (might be due to invalid IL or missing references)
			//IL_0049: Unknown result type (might be due to invalid IL or missing references)
			//IL_004e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0051: Unknown result type (might be due to invalid IL or missing references)
			//IL_0056: Unknown result type (might be due to invalid IL or missing references)
			//IL_005a: Unknown result type (might be due to invalid IL or missing references)
			//IL_005f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0073: Unknown result type (might be due to invalid IL or missing references)
			//IL_0074: Unknown result type (might be due to invalid IL or missing references)
			int num = _003C_003E1__state;
			RingLockChainService ringLockChainService = _003C_003E4__this;
			try
			{
				try
				{
					if (num != 0)
					{
						_003Ci_003E5__2 = 0;
						goto IL_0118;
					}
					Awaiter<bool> val = _003C_003Eu__1;
					_003C_003Eu__1 = default(Awaiter<bool>);
					num = (_003C_003E1__state = -1);
					goto IL_00a8;
					IL_00a8:
					if (!val.GetResult())
					{
						goto IL_00b6;
					}
					goto end_IL_0011;
					IL_00b6:
					if (_003Ci_003E5__2 == 0)
					{
						AudioService.PlaySound(AudioKeys.ChainShack.Value);
					}
					ringLockChainService.chainEffect.PlayChainShakeNoSound(rings[_003Ci_003E5__2]);
					ringLockChainService.chainLockUi.PlayHuangdongAnim(rings[_003Ci_003E5__2]);
					_003Ci_003E5__2++;
					goto IL_0118;
					IL_0118:
					if (_003Ci_003E5__2 < rings.Count)
					{
						if (_003Ci_003E5__2 > 0)
						{
							UniTask val2 = UniTask.Delay(TimeSpan.FromSeconds(Random.Range(0.1f, 0.3f)), false, (PlayerLoopTiming)8, ringLockChainService.DestroyToken);
							val = ((UniTask)(ref val2)).SuppressCancellationThrow().GetAwaiter();
							if (!val.IsCompleted)
							{
								num = (_003C_003E1__state = 0);
								_003C_003Eu__1 = val;
								((AsyncUniTaskMethodBuilder)(ref _003C_003Et__builder)).AwaitUnsafeOnCompleted<Awaiter<bool>, _003CPlayChainShakeSequentially_003Ed__39>(ref val, ref this);
								return;
							}
							goto IL_00a8;
						}
						goto IL_00b6;
					}
					end_IL_0011:;
				}
				catch (Exception ex)
				{
					Log.LogError("RingLockChain", "锁链晃动异常: {0}", new object[1] { ex.Message });
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
	private struct _003CPlayChainUnlockSoundDelayed_003Ed__41 : IAsyncStateMachine
	{
		public int _003C_003E1__state;

		public AsyncUniTaskVoidMethodBuilder _003C_003Et__builder;

		public RingLockChainService _003C_003E4__this;

		private Awaiter<bool> _003C_003Eu__1;

		private void MoveNext()
		{
			//IL_0079: Unknown result type (might be due to invalid IL or missing references)
			//IL_007e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0085: Unknown result type (might be due to invalid IL or missing references)
			//IL_0037: Unknown result type (might be due to invalid IL or missing references)
			//IL_003c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0040: Unknown result type (might be due to invalid IL or missing references)
			//IL_0045: Unknown result type (might be due to invalid IL or missing references)
			//IL_0049: Unknown result type (might be due to invalid IL or missing references)
			//IL_004e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0062: Unknown result type (might be due to invalid IL or missing references)
			//IL_0063: Unknown result type (might be due to invalid IL or missing references)
			int num = _003C_003E1__state;
			RingLockChainService ringLockChainService = _003C_003E4__this;
			try
			{
				Awaiter<bool> val;
				if (num == 0)
				{
					val = _003C_003Eu__1;
					_003C_003Eu__1 = default(Awaiter<bool>);
					num = (_003C_003E1__state = -1);
					goto IL_0094;
				}
				CancellationToken destroyToken = ringLockChainService.DestroyToken;
				if (!destroyToken.IsCancellationRequested)
				{
					UniTask val2 = UniTask.Delay(TimeSpan.FromSeconds(0.6000000238418579), false, (PlayerLoopTiming)8, destroyToken);
					val = ((UniTask)(ref val2)).SuppressCancellationThrow().GetAwaiter();
					if (!val.IsCompleted)
					{
						num = (_003C_003E1__state = 0);
						_003C_003Eu__1 = val;
						((AsyncUniTaskVoidMethodBuilder)(ref _003C_003Et__builder)).AwaitUnsafeOnCompleted<Awaiter<bool>, _003CPlayChainUnlockSoundDelayed_003Ed__41>(ref val, ref this);
						return;
					}
					goto IL_0094;
				}
				goto end_IL_000e;
				IL_0094:
				if (!val.GetResult())
				{
					AudioService.PlaySound(AudioKeys.ChainUnlock.Value);
				}
				end_IL_000e:;
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
	private struct _003CPlayDecrementShakeAsync_003Ed__37 : IAsyncStateMachine
	{
		public int _003C_003E1__state;

		public AsyncUniTaskVoidMethodBuilder _003C_003Et__builder;

		public List<RingObject> changedRings;

		public RingLockChainService _003C_003E4__this;

		private List<RingObject> _003CheadShakeRings_003E5__2;

		private Awaiter _003C_003Eu__1;

		private void MoveNext()
		{
			//IL_00fa: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ff: Unknown result type (might be due to invalid IL or missing references)
			//IL_0107: Unknown result type (might be due to invalid IL or missing references)
			//IL_0175: Unknown result type (might be due to invalid IL or missing references)
			//IL_017a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0182: Unknown result type (might be due to invalid IL or missing references)
			//IL_013a: Unknown result type (might be due to invalid IL or missing references)
			//IL_013f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0143: Unknown result type (might be due to invalid IL or missing references)
			//IL_0148: Unknown result type (might be due to invalid IL or missing references)
			//IL_015d: Unknown result type (might be due to invalid IL or missing references)
			//IL_015f: Unknown result type (might be due to invalid IL or missing references)
			//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c5: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ca: Unknown result type (might be due to invalid IL or missing references)
			//IL_00df: Unknown result type (might be due to invalid IL or missing references)
			//IL_00e1: Unknown result type (might be due to invalid IL or missing references)
			int num = _003C_003E1__state;
			RingLockChainService ringLockChainService = _003C_003E4__this;
			try
			{
				_ = 1;
				try
				{
					Awaiter val;
					if (num == 0)
					{
						val = _003C_003Eu__1;
						_003C_003Eu__1 = default(Awaiter);
						num = (_003C_003E1__state = -1);
						goto IL_0116;
					}
					if (num == 1)
					{
						val = _003C_003Eu__1;
						_003C_003Eu__1 = default(Awaiter);
						num = (_003C_003E1__state = -1);
						goto IL_0191;
					}
					UniTask val2;
					if (changedRings != null && changedRings.Count != 0)
					{
						List<RingObject> list = null;
						_003CheadShakeRings_003E5__2 = null;
						for (int i = 0; i < changedRings.Count; i++)
						{
							RingObject ringObject = changedRings[i];
							if ((Object)(object)ringObject == (Object)null)
							{
								continue;
							}
							if (ringObject.LockCount > 3)
							{
								if (list == null)
								{
									list = new List<RingObject>();
								}
								list.Add(ringObject);
							}
							else
							{
								if (_003CheadShakeRings_003E5__2 == null)
								{
									_003CheadShakeRings_003E5__2 = new List<RingObject>();
								}
								_003CheadShakeRings_003E5__2.Add(ringObject);
							}
						}
						if (list != null && list.Count > 0)
						{
							val2 = ringLockChainService.PlayChainShakeSequentially(list);
							val = ((UniTask)(ref val2)).GetAwaiter();
							if (!((Awaiter)(ref val)).IsCompleted)
							{
								num = (_003C_003E1__state = 0);
								_003C_003Eu__1 = val;
								((AsyncUniTaskVoidMethodBuilder)(ref _003C_003Et__builder)).AwaitUnsafeOnCompleted<Awaiter, _003CPlayDecrementShakeAsync_003Ed__37>(ref val, ref this);
								return;
							}
							goto IL_0116;
						}
						goto IL_011d;
					}
					goto end_IL_0013;
					IL_0191:
					((Awaiter)(ref val)).GetResult();
					goto IL_0198;
					IL_0198:
					_003CheadShakeRings_003E5__2 = null;
					goto end_IL_0013;
					IL_0116:
					((Awaiter)(ref val)).GetResult();
					goto IL_011d;
					IL_011d:
					if (_003CheadShakeRings_003E5__2 != null && _003CheadShakeRings_003E5__2.Count > 0)
					{
						val2 = ringLockChainService.PlayLockHeadShakeSequentially(_003CheadShakeRings_003E5__2);
						val = ((UniTask)(ref val2)).GetAwaiter();
						if (!((Awaiter)(ref val)).IsCompleted)
						{
							num = (_003C_003E1__state = 1);
							_003C_003Eu__1 = val;
							((AsyncUniTaskVoidMethodBuilder)(ref _003C_003Et__builder)).AwaitUnsafeOnCompleted<Awaiter, _003CPlayDecrementShakeAsync_003Ed__37>(ref val, ref this);
							return;
						}
						goto IL_0191;
					}
					goto IL_0198;
					end_IL_0013:;
				}
				catch (Exception ex)
				{
					Log.LogError("RingLockChain", "减1晃动异常: {0}", new object[1] { ex.Message });
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
	private struct _003CPlayLockHeadShakeSequentially_003Ed__38 : IAsyncStateMachine
	{
		public int _003C_003E1__state;

		public AsyncUniTaskMethodBuilder _003C_003Et__builder;

		public RingLockChainService _003C_003E4__this;

		public List<RingObject> rings;

		private int _003Ci_003E5__2;

		private Awaiter<bool> _003C_003Eu__1;

		private void MoveNext()
		{
			//IL_008d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0092: Unknown result type (might be due to invalid IL or missing references)
			//IL_0099: Unknown result type (might be due to invalid IL or missing references)
			//IL_0049: Unknown result type (might be due to invalid IL or missing references)
			//IL_004e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0051: Unknown result type (might be due to invalid IL or missing references)
			//IL_0056: Unknown result type (might be due to invalid IL or missing references)
			//IL_005a: Unknown result type (might be due to invalid IL or missing references)
			//IL_005f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0073: Unknown result type (might be due to invalid IL or missing references)
			//IL_0074: Unknown result type (might be due to invalid IL or missing references)
			int num = _003C_003E1__state;
			RingLockChainService ringLockChainService = _003C_003E4__this;
			try
			{
				try
				{
					if (num != 0)
					{
						_003Ci_003E5__2 = 0;
						goto IL_00e4;
					}
					Awaiter<bool> val = _003C_003Eu__1;
					_003C_003Eu__1 = default(Awaiter<bool>);
					num = (_003C_003E1__state = -1);
					goto IL_00a8;
					IL_00b6:
					ringLockChainService.chainLockUi.PlayHuangdongAnim(rings[_003Ci_003E5__2]);
					_003Ci_003E5__2++;
					goto IL_00e4;
					IL_00a8:
					if (!val.GetResult())
					{
						goto IL_00b6;
					}
					goto end_IL_0011;
					IL_00e4:
					if (_003Ci_003E5__2 < rings.Count)
					{
						if (_003Ci_003E5__2 > 0)
						{
							UniTask val2 = UniTask.Delay(TimeSpan.FromSeconds(Random.Range(0.1f, 0.3f)), false, (PlayerLoopTiming)8, ringLockChainService.DestroyToken);
							val = ((UniTask)(ref val2)).SuppressCancellationThrow().GetAwaiter();
							if (!val.IsCompleted)
							{
								num = (_003C_003E1__state = 0);
								_003C_003Eu__1 = val;
								((AsyncUniTaskMethodBuilder)(ref _003C_003Et__builder)).AwaitUnsafeOnCompleted<Awaiter<bool>, _003CPlayLockHeadShakeSequentially_003Ed__38>(ref val, ref this);
								return;
							}
							goto IL_00a8;
						}
						goto IL_00b6;
					}
					end_IL_0011:;
				}
				catch (Exception ex)
				{
					Log.LogError("RingLockChain", "锁头晃动异常: {0}", new object[1] { ex.Message });
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
	private struct _003CPlayPendingChainBreakAsync_003Ed__34 : IAsyncStateMachine
	{
		public int _003C_003E1__state;

		public AsyncUniTaskMethodBuilder _003C_003Et__builder;

		public IGameplayObject gameplayObject;

		public RingLockChainService _003C_003E4__this;

		public CancellationToken ct;

		private Awaiter<bool> _003C_003Eu__1;

		private void MoveNext()
		{
			//IL_012f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0134: Unknown result type (might be due to invalid IL or missing references)
			//IL_013c: Unknown result type (might be due to invalid IL or missing references)
			//IL_00eb: Unknown result type (might be due to invalid IL or missing references)
			//IL_00f0: Unknown result type (might be due to invalid IL or missing references)
			//IL_00f4: Unknown result type (might be due to invalid IL or missing references)
			//IL_00f9: Unknown result type (might be due to invalid IL or missing references)
			//IL_00fd: Unknown result type (might be due to invalid IL or missing references)
			//IL_0102: Unknown result type (might be due to invalid IL or missing references)
			//IL_0117: Unknown result type (might be due to invalid IL or missing references)
			//IL_0119: Unknown result type (might be due to invalid IL or missing references)
			int num = _003C_003E1__state;
			RingLockChainService ringLockChainService = _003C_003E4__this;
			try
			{
				Awaiter<bool> val;
				if (num == 0)
				{
					val = _003C_003Eu__1;
					_003C_003Eu__1 = default(Awaiter<bool>);
					num = (_003C_003E1__state = -1);
					goto IL_014b;
				}
				if (gameplayObject != null && !ringLockChainService.IsDisposed && ringLockChainService.pendingChainBreaks.TryGetValue(gameplayObject, out var value) && value != null)
				{
					ringLockChainService.pendingChainBreaks.Remove(gameplayObject);
					int num2 = 0;
					for (int i = 0; i < value.Count; i++)
					{
						if (value[i].RingOrder > num2)
						{
							num2 = value[i].RingOrder;
						}
					}
					ringLockChainService.DispatchChainBreaks(value, gameplayObject);
					float num3 = 0f;
					if (num2 > 0)
					{
						num3 = (float)num2 * Random.Range(RingLevelConfig.Instance.chainRingBreakIntervalSecondsMin, RingLevelConfig.Instance.chainRingBreakIntervalSecondsMax);
					}
					float num4 = RingLevelConfig.Instance.chainBreakStartDelaySeconds + num3;
					if (num4 > 0f)
					{
						UniTask val2 = UniTask.Delay(TimeSpan.FromSeconds(num4), false, (PlayerLoopTiming)8, ct);
						val = ((UniTask)(ref val2)).SuppressCancellationThrow().GetAwaiter();
						if (!val.IsCompleted)
						{
							num = (_003C_003E1__state = 0);
							_003C_003Eu__1 = val;
							((AsyncUniTaskMethodBuilder)(ref _003C_003Et__builder)).AwaitUnsafeOnCompleted<Awaiter<bool>, _003CPlayPendingChainBreakAsync_003Ed__34>(ref val, ref this);
							return;
						}
						goto IL_014b;
					}
				}
				goto end_IL_000e;
				IL_014b:
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

	private bool eventsSubscribed;

	private const float ChainUnlockSoundDelaySeconds = 0.6f;

	private readonly Dictionary<IGameplayObject, List<ChainBreakRequest>> pendingChainBreaks = new Dictionary<IGameplayObject, List<ChainBreakRequest>>();

	private readonly Dictionary<IGameplayObject, int> ringBreakSlots = new Dictionary<IGameplayObject, int>();

	private int nextRingOrder;

	private int ringOrderAssignFrame = -1;

	private readonly Dictionary<IGameplayObject, RingOrderEntry> ringLastBreakOrder = new Dictionary<IGameplayObject, RingOrderEntry>();

	private readonly RingChainEffect chainEffect;

	private readonly RingChainLockUi chainLockUi;

	internal GameplayManager HostInstance => base.Host;

	internal CancellationToken CancelToken => base.DestroyToken;

	internal RingChainLockUi ChainLockUi => chainLockUi;

	public RingLockChainService(GameplayManager host)
		: base(host)
	{
		chainEffect = new RingChainEffect(this);
		chainLockUi = new RingChainLockUi(this);
		RegisterEvents();
	}

	internal void ResetBreakSlot()
	{
		ringBreakSlots.Clear();
		nextRingOrder = 0;
		ringOrderAssignFrame = -1;
		ringLastBreakOrder.Clear();
	}

	private void ResetRingOrderIfNewFrame()
	{
		if (Time.frameCount != ringOrderAssignFrame)
		{
			ringOrderAssignFrame = Time.frameCount;
			nextRingOrder = 0;
		}
	}

	private int GetRingBreakSlot(IGameplayObject obj)
	{
		if (!ringBreakSlots.TryGetValue(obj, out var value))
		{
			return 0;
		}
		return value;
	}

	private int AssignRingBreakOrder(IGameplayObject obj)
	{
		int num = GetRingLastBreakOrder(obj);
		if (num >= 0)
		{
			return num;
		}
		ResetRingOrderIfNewFrame();
		int num2 = nextRingOrder++;
		ringLastBreakOrder[obj] = new RingOrderEntry(ringOrderAssignFrame, num2);
		return num2;
	}

	private int GetRingLastBreakOrder(IGameplayObject obj)
	{
		if (!ringLastBreakOrder.TryGetValue(obj, out var value) || value.Frame != Time.frameCount)
		{
			return -1;
		}
		return value.Order;
	}

	private void RegisterEvents()
	{
		if (!eventsSubscribed)
		{
			GameApp.Event.Listen<RingPreSolveEvent>((Action<RingPreSolveEvent>)OnPreSolve);
			GameApp.Event.Listen<RingAfterSolveEvent>((Action<RingAfterSolveEvent>)OnObjectAfterSolve);
			GameApp.Event.Listen<RingBreakReadyEvent>((Action<RingBreakReadyEvent>)OnBreakReady);
			GameApp.Event.Listen<LevelEnterCompletedEvent>((Action<LevelEnterCompletedEvent>)OnLevelEnterCompleted);
			eventsSubscribed = true;
		}
	}

	public bool IsLockBlocked(IGameplayObject obj)
	{
		if (obj != null)
		{
			return obj.LockCount > 0;
		}
		return false;
	}

	internal void CollectLockedRings(List<RingObject> output)
	{
		chainEffect.CollectLockedRings(output);
	}

	public void SetupChainOnLevelLoaded()
	{
		chainEffect.SetupChainOnLevelLoaded();
	}

	public void SetupChainLockUiOnLevelLoaded()
	{
		chainLockUi.SetupChainLockUiOnLevelLoaded();
	}

	public void NotifyRingScaleFactor(RingObject ring, float factor)
	{
		chainLockUi.OnRingScaleFactor(ring, factor);
	}

	public void PlayChainShake(RingObject ring)
	{
		if (!((Object)(object)ring == (Object)null) && !base.IsDisposed)
		{
			chainEffect.PlayChainShake(ring);
			chainLockUi.PlayHuangdongAnim(ring);
		}
	}

	public void ClearChainLockUi()
	{
		chainLockUi.ClearChainLockUi();
	}

	private void OnPreSolve(RingPreSolveEvent evt)
	{
		if (evt?.GameplayObject is RingObject ring && !base.IsDisposed)
		{
			chainEffect.DetachChainRoot(ring);
		}
	}

	private void OnObjectAfterSolve(RingAfterSolveEvent evt)
	{
		//IL_0287: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ff: Unknown result type (might be due to invalid IL or missing references)
		if (evt?.GameplayObject == null || evt.GameplayObject is FixedObject)
		{
			return;
		}
		IReadOnlyList<IGameplayObject> readOnlyList = base.Host.ObjectsService?.GameplayObjects;
		if (readOnlyList == null)
		{
			return;
		}
		IGameplayObject gameplayObject = evt.GameplayObject;
		List<ChainInstanceData> instances = null;
		RingObject ringObject = gameplayObject as RingObject;
		bool flag = (Object)(object)ringObject != (Object)null && chainEffect.TryGetInstances(ringObject, out instances) && instances != null && instances.Count > 0;
		int num = (flag ? Math.Min(gameplayObject.LockCount, instances.Count) : 0);
		int ringBreakSlot = GetRingBreakSlot(gameplayObject);
		int num2;
		if (num > 0)
		{
			num2 = AssignRingBreakOrder(gameplayObject);
		}
		else
		{
			num2 = GetRingLastBreakOrder(gameplayObject);
			if (num2 < 0)
			{
				num2 = 0;
			}
		}
		List<ChainBreakRequest> list = null;
		for (int i = 0; i < num; i++)
		{
			if (list == null)
			{
				list = new List<ChainBreakRequest>();
			}
			int num3 = num - 1 - i;
			RingObject lockOpenRing = ((num3 == 0) ? ringObject : null);
			list.Add(new ChainBreakRequest(instances, num3, ringBreakSlot + i, num2, muteBreakSound: false, lockOpenRing));
		}
		if (num > 0)
		{
			ringBreakSlots[gameplayObject] = ringBreakSlot + num;
		}
		bool flag2 = chainEffect.AllChainsUnlocked();
		List<RingObject> list2 = null;
		for (int j = 0; j < readOnlyList.Count; j++)
		{
			IGameplayObject gameplayObject2 = readOnlyList[j];
			if (gameplayObject2 == null || gameplayObject2.LockCount <= 0)
			{
				continue;
			}
			bool flag3 = gameplayObject2 == gameplayObject;
			gameplayObject2.LockCount = ((!flag3) ? (gameplayObject2.LockCount - 1) : 0);
			if (!flag3 && gameplayObject2 is RingObject item)
			{
				if (list2 == null)
				{
					list2 = new List<RingObject>();
				}
				list2.Add(item);
			}
			if (!flag3 && gameplayObject2.LockCount <= 0 && gameplayObject2 is RingObject ring)
			{
				chainEffect.DetachChainRoot(ring);
			}
			if (flag3 || !(gameplayObject2 is RingObject ringObject2) || !chainEffect.TryGetInstances(ringObject2, out var instances2) || instances2 == null || instances2.Count <= 0)
			{
				continue;
			}
			int lockCount = ringObject2.LockCount;
			if (lockCount >= 0 && lockCount < instances2.Count)
			{
				if (list == null)
				{
					list = new List<ChainBreakRequest>();
				}
				int ringBreakSlot2 = GetRingBreakSlot(ringObject2);
				int ringOrder = AssignRingBreakOrder(ringObject2);
				RingObject lockOpenRing2 = ((ringObject2.LockCount == 0) ? ringObject2 : null);
				list.Add(new ChainBreakRequest(instances2, lockCount, ringBreakSlot2, ringOrder, muteBreakSound: false, lockOpenRing2));
				ringBreakSlots[ringObject2] = ringBreakSlot2 + 1;
			}
		}
		if (!flag2 && chainEffect.AllChainsUnlocked())
		{
			PlayChainUnlockSoundDelayed();
			if (list != null && list.Count > 0)
			{
				int index = list.Count - 1;
				ChainBreakRequest chainBreakRequest = list[index];
				list[index] = new ChainBreakRequest(chainBreakRequest.Instances, chainBreakRequest.BreakIndex, chainBreakRequest.Slot, chainBreakRequest.RingOrder, muteBreakSound: true, chainBreakRequest.LockOpenRing);
			}
		}
		chainLockUi.RefreshChainLockUi();
		if (list2 != null && list2.Count > 0)
		{
			PlayDecrementShakeAsync(list2);
		}
		bool hasChainBreak = num > 0 || GetRingLastBreakOrder(gameplayObject) >= 0;
		evt.HasChainBreak = hasChainBreak;
		evt.BreakRingOrder = (flag ? num2 : Math.Max(0, GetRingLastBreakOrder(gameplayObject)));
		if (list != null)
		{
			pendingChainBreaks[evt.GameplayObject] = list;
		}
	}

	private void OnBreakReady(RingBreakReadyEvent evt)
	{
		if (evt?.GameplayObject != null && !base.IsDisposed && pendingChainBreaks.TryGetValue(evt.GameplayObject, out var value))
		{
			pendingChainBreaks.Remove(evt.GameplayObject);
			DispatchChainBreaks(value, evt.GameplayObject);
		}
	}

	public bool HasPendingChainBreak(IGameplayObject gameplayObject)
	{
		if (gameplayObject != null)
		{
			return pendingChainBreaks.ContainsKey(gameplayObject);
		}
		return false;
	}

	[AsyncStateMachine(typeof(_003CPlayPendingChainBreakAsync_003Ed__34))]
	public UniTask PlayPendingChainBreakAsync(IGameplayObject gameplayObject, CancellationToken ct)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		_003CPlayPendingChainBreakAsync_003Ed__34 _003CPlayPendingChainBreakAsync_003Ed__ = default(_003CPlayPendingChainBreakAsync_003Ed__34);
		_003CPlayPendingChainBreakAsync_003Ed__._003C_003Et__builder = AsyncUniTaskMethodBuilder.Create();
		_003CPlayPendingChainBreakAsync_003Ed__._003C_003E4__this = this;
		_003CPlayPendingChainBreakAsync_003Ed__.gameplayObject = gameplayObject;
		_003CPlayPendingChainBreakAsync_003Ed__.ct = ct;
		_003CPlayPendingChainBreakAsync_003Ed__._003C_003E1__state = -1;
		((AsyncUniTaskMethodBuilder)(ref _003CPlayPendingChainBreakAsync_003Ed__._003C_003Et__builder)).Start<_003CPlayPendingChainBreakAsync_003Ed__34>(ref _003CPlayPendingChainBreakAsync_003Ed__);
		return ((AsyncUniTaskMethodBuilder)(ref _003CPlayPendingChainBreakAsync_003Ed__._003C_003Et__builder)).Task;
	}

	private void DispatchChainBreaks(List<ChainBreakRequest> breaks, IGameplayObject gameplayObject)
	{
		for (int i = 0; i < breaks.Count; i++)
		{
			ChainBreakRequest chainBreakRequest = breaks[i];
			if (chainBreakRequest.BreakIndex >= 0)
			{
				chainEffect.PlayChainBreakDelayed(chainBreakRequest.Instances[chainBreakRequest.BreakIndex], chainBreakRequest.Slot, chainBreakRequest.MuteBreakSound, chainBreakRequest.LockOpenRing, chainBreakRequest.RingOrder);
				continue;
			}
			for (int j = 0; j < chainBreakRequest.Instances.Count; j++)
			{
				RingObject lockOpenRing = ((j == chainBreakRequest.Instances.Count - 1) ? chainBreakRequest.LockOpenRing : null);
				chainEffect.PlayChainBreakDelayed(chainBreakRequest.Instances[j], chainBreakRequest.Slot + j, chainBreakRequest.MuteBreakSound, lockOpenRing, chainBreakRequest.RingOrder);
			}
		}
	}

	private void OnLevelEnterCompleted(LevelEnterCompletedEvent evt)
	{
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		if (!base.IsDisposed && evt != null)
		{
			List<RingObject> list = new List<RingObject>(8);
			chainEffect.CollectLockedRings(list);
			if (list.Count != 0)
			{
				UniTaskExtensions.Forget(PlayChainShakeSequentially(list));
			}
		}
	}

	[AsyncStateMachine(typeof(_003CPlayDecrementShakeAsync_003Ed__37))]
	private UniTaskVoid PlayDecrementShakeAsync(List<RingObject> changedRings)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		_003CPlayDecrementShakeAsync_003Ed__37 _003CPlayDecrementShakeAsync_003Ed__ = default(_003CPlayDecrementShakeAsync_003Ed__37);
		_003CPlayDecrementShakeAsync_003Ed__._003C_003Et__builder = AsyncUniTaskVoidMethodBuilder.Create();
		_003CPlayDecrementShakeAsync_003Ed__._003C_003E4__this = this;
		_003CPlayDecrementShakeAsync_003Ed__.changedRings = changedRings;
		_003CPlayDecrementShakeAsync_003Ed__._003C_003E1__state = -1;
		((AsyncUniTaskVoidMethodBuilder)(ref _003CPlayDecrementShakeAsync_003Ed__._003C_003Et__builder)).Start<_003CPlayDecrementShakeAsync_003Ed__37>(ref _003CPlayDecrementShakeAsync_003Ed__);
		return ((AsyncUniTaskVoidMethodBuilder)(ref _003CPlayDecrementShakeAsync_003Ed__._003C_003Et__builder)).Task;
	}

	[AsyncStateMachine(typeof(_003CPlayLockHeadShakeSequentially_003Ed__38))]
	private UniTask PlayLockHeadShakeSequentially(List<RingObject> rings)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		_003CPlayLockHeadShakeSequentially_003Ed__38 _003CPlayLockHeadShakeSequentially_003Ed__ = default(_003CPlayLockHeadShakeSequentially_003Ed__38);
		_003CPlayLockHeadShakeSequentially_003Ed__._003C_003Et__builder = AsyncUniTaskMethodBuilder.Create();
		_003CPlayLockHeadShakeSequentially_003Ed__._003C_003E4__this = this;
		_003CPlayLockHeadShakeSequentially_003Ed__.rings = rings;
		_003CPlayLockHeadShakeSequentially_003Ed__._003C_003E1__state = -1;
		((AsyncUniTaskMethodBuilder)(ref _003CPlayLockHeadShakeSequentially_003Ed__._003C_003Et__builder)).Start<_003CPlayLockHeadShakeSequentially_003Ed__38>(ref _003CPlayLockHeadShakeSequentially_003Ed__);
		return ((AsyncUniTaskMethodBuilder)(ref _003CPlayLockHeadShakeSequentially_003Ed__._003C_003Et__builder)).Task;
	}

	[AsyncStateMachine(typeof(_003CPlayChainShakeSequentially_003Ed__39))]
	private UniTask PlayChainShakeSequentially(List<RingObject> rings)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		_003CPlayChainShakeSequentially_003Ed__39 _003CPlayChainShakeSequentially_003Ed__ = default(_003CPlayChainShakeSequentially_003Ed__39);
		_003CPlayChainShakeSequentially_003Ed__._003C_003Et__builder = AsyncUniTaskMethodBuilder.Create();
		_003CPlayChainShakeSequentially_003Ed__._003C_003E4__this = this;
		_003CPlayChainShakeSequentially_003Ed__.rings = rings;
		_003CPlayChainShakeSequentially_003Ed__._003C_003E1__state = -1;
		((AsyncUniTaskMethodBuilder)(ref _003CPlayChainShakeSequentially_003Ed__._003C_003Et__builder)).Start<_003CPlayChainShakeSequentially_003Ed__39>(ref _003CPlayChainShakeSequentially_003Ed__);
		return ((AsyncUniTaskMethodBuilder)(ref _003CPlayChainShakeSequentially_003Ed__._003C_003Et__builder)).Task;
	}

	public override void Dispose()
	{
		if (eventsSubscribed)
		{
			GameApp.Event.Unlisten<RingPreSolveEvent>((Action<RingPreSolveEvent>)OnPreSolve);
			GameApp.Event.Unlisten<RingAfterSolveEvent>((Action<RingAfterSolveEvent>)OnObjectAfterSolve);
			GameApp.Event.Unlisten<RingBreakReadyEvent>((Action<RingBreakReadyEvent>)OnBreakReady);
			GameApp.Event.Unlisten<LevelEnterCompletedEvent>((Action<LevelEnterCompletedEvent>)OnLevelEnterCompleted);
			eventsSubscribed = false;
		}
		pendingChainBreaks.Clear();
		ringBreakSlots.Clear();
		ringLastBreakOrder.Clear();
		nextRingOrder = 0;
		ringOrderAssignFrame = -1;
		chainLockUi.Dispose();
		chainEffect.Dispose();
		base.Dispose();
	}

	[AsyncStateMachine(typeof(_003CPlayChainUnlockSoundDelayed_003Ed__41))]
	private UniTaskVoid PlayChainUnlockSoundDelayed()
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		_003CPlayChainUnlockSoundDelayed_003Ed__41 _003CPlayChainUnlockSoundDelayed_003Ed__ = default(_003CPlayChainUnlockSoundDelayed_003Ed__41);
		_003CPlayChainUnlockSoundDelayed_003Ed__._003C_003Et__builder = AsyncUniTaskVoidMethodBuilder.Create();
		_003CPlayChainUnlockSoundDelayed_003Ed__._003C_003E4__this = this;
		_003CPlayChainUnlockSoundDelayed_003Ed__._003C_003E1__state = -1;
		((AsyncUniTaskVoidMethodBuilder)(ref _003CPlayChainUnlockSoundDelayed_003Ed__._003C_003Et__builder)).Start<_003CPlayChainUnlockSoundDelayed_003Ed__41>(ref _003CPlayChainUnlockSoundDelayed_003Ed__);
		return ((AsyncUniTaskVoidMethodBuilder)(ref _003CPlayChainUnlockSoundDelayed_003Ed__._003C_003Et__builder)).Task;
	}
}
