using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.CompilerServices;
using RotateRings.Common;
using RotateRings.GamePlay.Events;
using RotateRings.Services;
using UniLog;
using UnityEngine;
using UnlockRing;
using UnlockRing.Core.Domain;
using UnlockRing.Presentation;
using Vibration;

namespace RotateRings.GamePlay.Services;

public sealed class RingBoomService : GameplayServiceBase<GameplayManager>
{
	[CompilerGenerated]
	private sealed class _003C_003Ec__DisplayClass58_0
	{
		public Animation animation;

		internal bool _003CPlayInThenIdle_003Eb__0()
		{
			if (!((Object)(object)animation == (Object)null))
			{
				return !animation.isPlaying;
			}
			return true;
		}
	}

	[StructLayout(LayoutKind.Auto)]
	[CompilerGenerated]
	private struct _003CPlayInThenIdle_003Ed__58 : IAsyncStateMachine
	{
		public int _003C_003E1__state;

		public AsyncUniTaskVoidMethodBuilder _003C_003Et__builder;

		public GameObject instance;

		public RingBoomService _003C_003E4__this;

		public BombRelative bomb;

		private Awaiter<bool> _003C_003Eu__1;

		private void MoveNext()
		{
			//IL_011a: Unknown result type (might be due to invalid IL or missing references)
			//IL_011f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0127: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d3: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d8: Unknown result type (might be due to invalid IL or missing references)
			//IL_00dc: Unknown result type (might be due to invalid IL or missing references)
			//IL_00e1: Unknown result type (might be due to invalid IL or missing references)
			//IL_00e5: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ea: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ff: Unknown result type (might be due to invalid IL or missing references)
			//IL_0101: Unknown result type (might be due to invalid IL or missing references)
			int num = _003C_003E1__state;
			RingBoomService ringBoomService = _003C_003E4__this;
			try
			{
				if (num == 0)
				{
					goto IL_00a0;
				}
				_003C_003Ec__DisplayClass58_0 _003C_003Ec__DisplayClass58_ = new _003C_003Ec__DisplayClass58_0();
				if (!((Object)(object)instance == (Object)null))
				{
					_003C_003Ec__DisplayClass58_.animation = instance.GetComponent<Animation>();
					if ((Object)(object)_003C_003Ec__DisplayClass58_.animation == (Object)null)
					{
						PlayAnimation(instance, "anim_rotatebomb_boom_idle", loop: true);
					}
					else
					{
						AnimationClip clip = _003C_003Ec__DisplayClass58_.animation.GetClip("anim_rotatebomb_boom_in");
						if (!((Object)(object)clip == (Object)null) && !(clip.length <= 0f))
						{
							goto IL_00a0;
						}
						PlayAnimation(instance, "anim_rotatebomb_boom_idle", loop: true);
					}
				}
				goto end_IL_000e;
				IL_00a0:
				try
				{
					Awaiter<bool> val2;
					if (num != 0)
					{
						_003C_003Ec__DisplayClass58_.animation.wrapMode = (WrapMode)1;
						_003C_003Ec__DisplayClass58_.animation.Play("anim_rotatebomb_boom_in");
						UniTask val = UniTask.WaitUntil((Func<bool>)_003C_003Ec__DisplayClass58_._003CPlayInThenIdle_003Eb__0, (PlayerLoopTiming)8, ringBoomService.DestroyToken);
						val2 = ((UniTask)(ref val)).SuppressCancellationThrow().GetAwaiter();
						if (!val2.IsCompleted)
						{
							num = (_003C_003E1__state = 0);
							_003C_003Eu__1 = val2;
							((AsyncUniTaskVoidMethodBuilder)(ref _003C_003Et__builder)).AwaitUnsafeOnCompleted<Awaiter<bool>, _003CPlayInThenIdle_003Ed__58>(ref val2, ref this);
							return;
						}
					}
					else
					{
						val2 = _003C_003Eu__1;
						_003C_003Eu__1 = default(Awaiter<bool>);
						num = (_003C_003E1__state = -1);
					}
					if (!val2.GetResult() && !ringBoomService.IsDisposed && !((Object)(object)instance == (Object)null) && !((Object)(object)bomb == (Object)null) && !bomb.IsSolvedForTrack)
					{
						PlayAnimation(instance, "anim_rotatebomb_boom_idle", loop: true);
					}
				}
				catch (Exception ex) when (!(ex is OperationCanceledException))
				{
					Log.LogError("[BombBoomEffectService] ", "入场动画衔接 idle 失败: {0}", new object[1] { ex });
					if ((Object)(object)instance != (Object)null && (Object)(object)bomb != (Object)null && !bomb.IsSolvedForTrack)
					{
						PlayAnimation(instance, "anim_rotatebomb_boom_idle", loop: true);
					}
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
	private struct _003CRecycleAfterDelayAsync_003Ed__62 : IAsyncStateMachine
	{
		public int _003C_003E1__state;

		public AsyncUniTaskVoidMethodBuilder _003C_003Et__builder;

		public GameObject instance;

		public RingBoomService _003C_003E4__this;

		private Awaiter<bool> _003C_003Eu__1;

		private void MoveNext()
		{
			//IL_0086: Unknown result type (might be due to invalid IL or missing references)
			//IL_008b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0092: Unknown result type (might be due to invalid IL or missing references)
			//IL_0042: Unknown result type (might be due to invalid IL or missing references)
			//IL_0047: Unknown result type (might be due to invalid IL or missing references)
			//IL_004a: Unknown result type (might be due to invalid IL or missing references)
			//IL_004f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0053: Unknown result type (might be due to invalid IL or missing references)
			//IL_0058: Unknown result type (might be due to invalid IL or missing references)
			//IL_006c: Unknown result type (might be due to invalid IL or missing references)
			//IL_006d: Unknown result type (might be due to invalid IL or missing references)
			int num = _003C_003E1__state;
			RingBoomService ringBoomService = _003C_003E4__this;
			try
			{
				if (num == 0 || !((Object)(object)instance == (Object)null))
				{
					try
					{
						Awaiter<bool> val2;
						if (num != 0)
						{
							UniTask val = UniTask.Delay(TimeSpan.FromSeconds(ringBoomService.targetBreakDelay + 1f), false, (PlayerLoopTiming)8, ringBoomService.DestroyToken);
							val2 = ((UniTask)(ref val)).SuppressCancellationThrow().GetAwaiter();
							if (!val2.IsCompleted)
							{
								num = (_003C_003E1__state = 0);
								_003C_003Eu__1 = val2;
								((AsyncUniTaskVoidMethodBuilder)(ref _003C_003Et__builder)).AwaitUnsafeOnCompleted<Awaiter<bool>, _003CRecycleAfterDelayAsync_003Ed__62>(ref val2, ref this);
								return;
							}
						}
						else
						{
							val2 = _003C_003Eu__1;
							_003C_003Eu__1 = default(Awaiter<bool>);
							num = (_003C_003E1__state = -1);
						}
						val2.GetResult();
						RecycleEffect(instance);
					}
					catch (Exception ex) when (!(ex is OperationCanceledException))
					{
						Log.LogError("[BombBoomEffectService] ", "回收炸弹特效失败: {0}", new object[1] { ex });
						RecycleEffect(instance);
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
	private struct _003CSendBreakTickAfterDelay_003Ed__61 : IAsyncStateMachine
	{
		public int _003C_003E1__state;

		public AsyncUniTaskVoidMethodBuilder _003C_003Et__builder;

		public RingBoomService _003C_003E4__this;

		private Awaiter<bool> _003C_003Eu__1;

		private void MoveNext()
		{
			//IL_006c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0071: Unknown result type (might be due to invalid IL or missing references)
			//IL_0078: Unknown result type (might be due to invalid IL or missing references)
			//IL_0028: Unknown result type (might be due to invalid IL or missing references)
			//IL_002d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0030: Unknown result type (might be due to invalid IL or missing references)
			//IL_0035: Unknown result type (might be due to invalid IL or missing references)
			//IL_0039: Unknown result type (might be due to invalid IL or missing references)
			//IL_003e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0052: Unknown result type (might be due to invalid IL or missing references)
			//IL_0053: Unknown result type (might be due to invalid IL or missing references)
			int num = _003C_003E1__state;
			RingBoomService ringBoomService = _003C_003E4__this;
			try
			{
				try
				{
					Awaiter<bool> val2;
					if (num != 0)
					{
						UniTask val = UniTask.Delay(TimeSpan.FromSeconds(ringBoomService.targetBreakDelay), false, (PlayerLoopTiming)8, ringBoomService.DestroyToken);
						val2 = ((UniTask)(ref val)).SuppressCancellationThrow().GetAwaiter();
						if (!val2.IsCompleted)
						{
							num = (_003C_003E1__state = 0);
							_003C_003Eu__1 = val2;
							((AsyncUniTaskVoidMethodBuilder)(ref _003C_003Et__builder)).AwaitUnsafeOnCompleted<Awaiter<bool>, _003CSendBreakTickAfterDelay_003Ed__61>(ref val2, ref this);
							return;
						}
					}
					else
					{
						val2 = _003C_003Eu__1;
						_003C_003Eu__1 = default(Awaiter<bool>);
						num = (_003C_003E1__state = -1);
					}
					val2.GetResult();
					if (!ringBoomService.IsDisposed)
					{
						GameApp.Event.Send<BombBreakTickEvent>(new BombBreakTickEvent());
						if (ringBoomService.pendingBoomCount > 0)
						{
							ringBoomService.pendingBoomCount--;
						}
					}
				}
				catch (Exception ex) when (!(ex is OperationCanceledException))
				{
					Log.LogError("[BombBoomEffectService] ", "发送碎裂信号失败: {0}", new object[1] { ex });
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
	private struct _003CShowIdleEffectWithDelay_003Ed__56 : IAsyncStateMachine
	{
		public int _003C_003E1__state;

		public AsyncUniTaskVoidMethodBuilder _003C_003Et__builder;

		public float delay;

		public RingBoomService _003C_003E4__this;

		public BombRelative bomb;

		private Awaiter<bool> _003C_003Eu__1;

		private void MoveNext()
		{
			//IL_006c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0071: Unknown result type (might be due to invalid IL or missing references)
			//IL_0078: Unknown result type (might be due to invalid IL or missing references)
			//IL_0028: Unknown result type (might be due to invalid IL or missing references)
			//IL_002d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0030: Unknown result type (might be due to invalid IL or missing references)
			//IL_0035: Unknown result type (might be due to invalid IL or missing references)
			//IL_0039: Unknown result type (might be due to invalid IL or missing references)
			//IL_003e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0052: Unknown result type (might be due to invalid IL or missing references)
			//IL_0053: Unknown result type (might be due to invalid IL or missing references)
			int num = _003C_003E1__state;
			RingBoomService ringBoomService = _003C_003E4__this;
			try
			{
				try
				{
					Awaiter<bool> val2;
					if (num != 0)
					{
						UniTask val = UniTask.Delay(TimeSpan.FromSeconds(delay), false, (PlayerLoopTiming)8, ringBoomService.DestroyToken);
						val2 = ((UniTask)(ref val)).SuppressCancellationThrow().GetAwaiter();
						if (!val2.IsCompleted)
						{
							num = (_003C_003E1__state = 0);
							_003C_003Eu__1 = val2;
							((AsyncUniTaskVoidMethodBuilder)(ref _003C_003Et__builder)).AwaitUnsafeOnCompleted<Awaiter<bool>, _003CShowIdleEffectWithDelay_003Ed__56>(ref val2, ref this);
							return;
						}
					}
					else
					{
						val2 = _003C_003Eu__1;
						_003C_003Eu__1 = default(Awaiter<bool>);
						num = (_003C_003E1__state = -1);
					}
					if (!val2.GetResult() && !ringBoomService.IsDisposed && !((Object)(object)bomb == (Object)null) && !bomb.IsSolvedForTrack)
					{
						ringBoomService.ShowIdleEffect(bomb);
					}
				}
				catch (Exception ex) when (!(ex is OperationCanceledException))
				{
					Log.LogError("[BombBoomEffectService] ", "延迟显示炸弹待机特效失败: {0}", new object[1] { ex });
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

	private const string TAG = "[BombBoomEffectService] ";

	private const string EffectLocation = "eff_rotatebomb_boom";

	private const string EffectResGroup = "UnlockRing_Effect";

	private const string InClipName = "anim_rotatebomb_boom_in";

	private const string IdleClipName = "anim_rotatebomb_boom_idle";

	private const string BoomClipName = "anim_rotatebomb_boom_start";

	private const float BoomClipLengthFallback = 0.38f;

	private const float BreakExtraDelay = 0.1f;

	private const float BoomAfterDelay = 1f;

	private float targetBreakDelay = 0.48f;

	private const float EffectZOffset = -0.1f;

	private const string EffectChildName = "eff_rotatebomb_boom";

	private const float BombAppearStaggerDelay = 0.2f;

	private const string EffectRootName = "[BombBoomEffects]";

	private GameObject effectPrefab;

	private Transform effectRoot;

	private readonly HashSet<BombRelative> trackedBombs = new HashSet<BombRelative>();

	private readonly List<BombRelative> pendingDisplayBombs = new List<BombRelative>(8);

	private int aliveBombCount;

	private int pendingBoomCount;

	private readonly Stack<BombRelative> bombCascadeScopeStack = new Stack<BombRelative>();

	private readonly Dictionary<IGameplayObject, List<BombRelative>> pendingHostBombVisuals = new Dictionary<IGameplayObject, List<BombRelative>>(4);

	private readonly HashSet<IGameplayObject> registeredBombExplosionObjects = new HashSet<IGameplayObject>();

	private readonly List<IGameplayObject> registeredBombExternalTargets = new List<IGameplayObject>();

	private IGameplayObject dragBombProbeSource;

	public float TargetBreakDelay => targetBreakDelay;

	public bool IsInBombCascadeScope => bombCascadeScopeStack.Count > 0;

	public bool IsAnyBoomPending => pendingBoomCount > 0;

	public BombRelative ActiveBombCascadeSource
	{
		get
		{
			if (bombCascadeScopeStack.Count <= 0)
			{
				return null;
			}
			return bombCascadeScopeStack.Peek();
		}
	}

	public RingBoomService(GameplayManager host)
		: base(host)
	{
		CreateEffectRoot();
		GameApp.Event.Listen<BombExplodeStartEvent>((Action<BombExplodeStartEvent>)OnBombExplodeStart);
		GameApp.Event.Listen<BombRemoveWithoutExplosionEvent>((Action<BombRemoveWithoutExplosionEvent>)OnBombRemoveWithoutExplosion);
		GameApp.Event.Listen<LevelEnterCompletedEvent>((Action<LevelEnterCompletedEvent>)OnLevelEnterCompleted);
		GameApp.Event.Listen<RingPreSolveEvent>((Action<RingPreSolveEvent>)OnPreSolve);
		GameApp.Event.Listen<RingRemoveEffectEvent>((Action<RingRemoveEffectEvent>)OnHostRemoveEffect);
	}

	public override void Dispose()
	{
		if (!base.IsDisposed)
		{
			base.Dispose();
			GameApp.Event.Unlisten<BombExplodeStartEvent>((Action<BombExplodeStartEvent>)OnBombExplodeStart);
			GameApp.Event.Unlisten<BombRemoveWithoutExplosionEvent>((Action<BombRemoveWithoutExplosionEvent>)OnBombRemoveWithoutExplosion);
			GameApp.Event.Unlisten<LevelEnterCompletedEvent>((Action<LevelEnterCompletedEvent>)OnLevelEnterCompleted);
			GameApp.Event.Unlisten<RingPreSolveEvent>((Action<RingPreSolveEvent>)OnPreSolve);
			GameApp.Event.Unlisten<RingRemoveEffectEvent>((Action<RingRemoveEffectEvent>)OnHostRemoveEffect);
			FlushPendingHostBombVisuals();
			ClearRegisteredBombExplosionObjects();
			StopBombIdleSound();
			aliveBombCount = 0;
			trackedBombs.Clear();
			pendingDisplayBombs.Clear();
			if ((Object)(object)effectRoot != (Object)null)
			{
				Object.Destroy((Object)(object)((Component)effectRoot).gameObject);
				effectRoot = null;
			}
			effectPrefab = null;
		}
	}

	public override void ClearLevel()
	{
		aliveBombCount = 0;
		pendingBoomCount = 0;
		trackedBombs.Clear();
		pendingDisplayBombs.Clear();
		bombCascadeScopeStack.Clear();
		FlushPendingHostBombVisuals();
		ClearRegisteredBombExplosionObjects();
		dragBombProbeSource = null;
		StopBombIdleSound();
		base.ClearLevel();
	}

	public void PushBombCascadeScope(BombRelative sourceBomb)
	{
		if ((Object)(object)sourceBomb != (Object)null)
		{
			bombCascadeScopeStack.Push(sourceBomb);
		}
	}

	public void PopBombCascadeScope(BombRelative sourceBomb)
	{
		if ((Object)(object)sourceBomb != (Object)null && bombCascadeScopeStack.Count > 0 && bombCascadeScopeStack.Peek() == sourceBomb)
		{
			bombCascadeScopeStack.Pop();
		}
	}

	public void RunInBombCascadeScope(BombRelative sourceBomb, Action action)
	{
		if (action == null)
		{
			return;
		}
		bool flag = (Object)(object)sourceBomb != (Object)null && !IsInBombCascadeScope;
		if (flag)
		{
			PushBombCascadeScope(sourceBomb);
		}
		try
		{
			action();
		}
		finally
		{
			if (flag)
			{
				PopBombCascadeScope(sourceBomb);
			}
		}
	}

	internal void RegisterBombExplosionObjects(IGameplayObject bombHost, IReadOnlyList<IGameplayObject> externalTargets)
	{
		registeredBombExplosionObjects.Clear();
		registeredBombExternalTargets.Clear();
		if (bombHost != null)
		{
			registeredBombExplosionObjects.Add(bombHost);
		}
		if (externalTargets == null)
		{
			return;
		}
		for (int i = 0; i < externalTargets.Count; i++)
		{
			IGameplayObject gameplayObject = externalTargets[i];
			if (gameplayObject != null)
			{
				registeredBombExplosionObjects.Add(gameplayObject);
				registeredBombExternalTargets.Add(gameplayObject);
			}
		}
	}

	internal bool TryGetRegisteredBombExternalCascadeTriggerPartId(RingObjectsService objects, out PartId partId)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		partId = default(PartId);
		if (objects == null)
		{
			return false;
		}
		for (int i = 0; i < registeredBombExternalTargets.Count; i++)
		{
			IGameplayObject gameplayObject = registeredBombExternalTargets[i];
			if (gameplayObject != null && objects.TryGetPartId(gameplayObject, out partId))
			{
				return true;
			}
		}
		return false;
	}

	internal bool TryGetDragBombProbeCascadeTriggerPartId(RingObjectsService objects, out PartId partId)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		partId = default(PartId);
		if (dragBombProbeSource != null && objects != null)
		{
			return objects.TryGetPartId(dragBombProbeSource, out partId);
		}
		return false;
	}

	internal bool IsRegisteredBombExplosionObject(IGameplayObject obj)
	{
		if (obj != null)
		{
			return registeredBombExplosionObjects.Contains(obj);
		}
		return false;
	}

	internal void ClearRegisteredBombExplosionObjects()
	{
		registeredBombExplosionObjects.Clear();
		registeredBombExternalTargets.Clear();
		dragBombProbeSource = null;
	}

	public void RemoveChildBombs(Component host, BombRelative sourceBomb = null)
	{
		if ((Object)(object)host == (Object)null)
		{
			return;
		}
		BombRelative[] componentsInChildren = host.GetComponentsInChildren<BombRelative>(true);
		foreach (BombRelative bombRelative in componentsInChildren)
		{
			if ((Object)(object)bombRelative != (Object)null && (Object)(object)bombRelative != (Object)(object)sourceBomb)
			{
				bombRelative.RemoveWithoutExplosion();
			}
		}
	}

	public void ProbeBombContactsDuringDrag(IReadOnlyList<Collider2D> contactProbes, IGameplayObject dragSource = null)
	{
		if (contactProbes == null || contactProbes.Count == 0 || base.Host.ObjectsService.BombCount == 0)
		{
			return;
		}
		if (dragSource != null)
		{
			dragBombProbeSource = dragSource;
		}
		IReadOnlyList<BombRelative> bombs = base.Host.ObjectsService.Bombs;
		for (int i = 0; i < bombs.Count; i++)
		{
			BombRelative bombRelative = bombs[i];
			if ((Object)(object)bombRelative != (Object)null && bombRelative.TryExplodeFromDragProbes(contactProbes))
			{
				break;
			}
		}
	}

	private void OnPreSolve(RingPreSolveEvent evt)
	{
		if (base.IsDisposed)
		{
			return;
		}
		IGameplayObject obj = evt?.GameplayObject;
		Component val = (Component)((obj is Component) ? obj : null);
		if (val != null)
		{
			if (evt.IsFromBomb)
			{
				BombRelative activeBombCascadeSource = ActiveBombCascadeSource;
				DeferChildBombVisuals(evt.GameplayObject, val, activeBombCascadeSource);
			}
			else
			{
				RemoveChildBombs(val);
			}
		}
	}

	private void DeferChildBombVisuals(IGameplayObject hostGameplay, Component host, BombRelative sourceBomb)
	{
		if ((Object)(object)host == (Object)null || hostGameplay == null)
		{
			return;
		}
		BombRelative[] componentsInChildren = host.GetComponentsInChildren<BombRelative>(true);
		List<BombRelative> list = null;
		foreach (BombRelative bombRelative in componentsInChildren)
		{
			if (!((Object)(object)bombRelative == (Object)null) && !((Object)(object)bombRelative == (Object)(object)sourceBomb))
			{
				bombRelative.MarkRemovedWithoutExplosion();
				UntrackBomb(bombRelative);
				if (list == null)
				{
					list = GetOrCreatePendingHostBombList(hostGameplay);
				}
				list.Add(bombRelative);
			}
		}
	}

	private List<BombRelative> GetOrCreatePendingHostBombList(IGameplayObject hostGameplay)
	{
		if (!pendingHostBombVisuals.TryGetValue(hostGameplay, out var value))
		{
			value = new List<BombRelative>(2);
			pendingHostBombVisuals[hostGameplay] = value;
		}
		return value;
	}

	private void OnHostRemoveEffect(RingRemoveEffectEvent evt)
	{
		if (base.IsDisposed || evt?.SourcePart == null)
		{
			return;
		}
		IGameplayObject sourcePart = evt.SourcePart;
		Component val = (Component)((sourcePart is Component) ? sourcePart : null);
		if (val == null || (Object)(object)evt.PartGo != (Object)(object)val.gameObject || !pendingHostBombVisuals.TryGetValue(evt.SourcePart, out var value))
		{
			return;
		}
		pendingHostBombVisuals.Remove(evt.SourcePart);
		for (int i = 0; i < value.Count; i++)
		{
			BombRelative bombRelative = value[i];
			if ((Object)(object)bombRelative != (Object)null)
			{
				bombRelative.FinishRemoveWithoutExplosion();
			}
		}
	}

	private void FlushPendingHostBombVisuals()
	{
		foreach (KeyValuePair<IGameplayObject, List<BombRelative>> pendingHostBombVisual in pendingHostBombVisuals)
		{
			List<BombRelative> value = pendingHostBombVisual.Value;
			if (value == null)
			{
				continue;
			}
			for (int i = 0; i < value.Count; i++)
			{
				BombRelative bombRelative = value[i];
				if ((Object)(object)bombRelative != (Object)null)
				{
					bombRelative.FinishRemoveWithoutExplosion();
				}
			}
		}
		pendingHostBombVisuals.Clear();
	}

	public void AttachToBomb(BombRelative bomb)
	{
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		if (base.IsDisposed || (Object)(object)bomb == (Object)null)
		{
			return;
		}
		Transform transform = ((Component)bomb).transform;
		if ((Object)(object)transform.Find("eff_rotatebomb_boom") != (Object)null || !TryLoadEffectPrefab())
		{
			return;
		}
		GameObject val = GameApp.Pool.Spawn(effectPrefab, transform);
		if (!((Object)(object)val == (Object)null))
		{
			((Object)val).name = "eff_rotatebomb_boom";
			Transform transform2 = val.transform;
			transform2.localPosition = new Vector3(0f, 0f, -0.1f);
			transform2.localRotation = Quaternion.identity;
			transform2.localScale = Vector3.one;
			BoardFlipEffectUtility.ApplyFlipCompensation(transform2, base.Host);
			MeshRenderer component = ((Component)bomb).GetComponent<MeshRenderer>();
			if ((Object)(object)component != (Object)null)
			{
				((Renderer)component).enabled = false;
			}
			val.SetActive(false);
			pendingDisplayBombs.Add(bomb);
		}
	}

	private void CreateEffectRoot()
	{
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Expected O, but got Unknown
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		GameObject val = GameObject.Find("[BombBoomEffects]");
		if ((Object)(object)val != (Object)null)
		{
			effectRoot = val.transform;
			return;
		}
		GameObject val2 = new GameObject("[BombBoomEffects]");
		effectRoot = val2.transform;
		effectRoot.SetParent((Transform)null);
		effectRoot.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
		effectRoot.localScale = Vector3.one;
	}

	private bool TryLoadEffectPrefab()
	{
		if ((Object)(object)effectPrefab != (Object)null)
		{
			return true;
		}
		effectPrefab = GameApp.Res.LoadAsset<GameObject>("eff_rotatebomb_boom", "UnlockRing_Effect");
		if ((Object)(object)effectPrefab == (Object)null)
		{
			return false;
		}
		TryUpdateTargetBreakDelay();
		return true;
	}

	private void TryUpdateTargetBreakDelay()
	{
		if ((Object)(object)effectPrefab == (Object)null)
		{
			return;
		}
		Animation component = effectPrefab.GetComponent<Animation>();
		if (!((Object)(object)component == (Object)null))
		{
			AnimationClip clip = component.GetClip("anim_rotatebomb_boom_start");
			if (!((Object)(object)clip == (Object)null) && !(clip.length <= 0f))
			{
				targetBreakDelay = clip.length + 0.1f;
			}
		}
	}

	private void OnLevelEnterCompleted(LevelEnterCompletedEvent evt)
	{
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		if (base.IsDisposed || evt == null)
		{
			return;
		}
		for (int num = pendingDisplayBombs.Count - 1; num >= 0; num--)
		{
			BombRelative bomb = pendingDisplayBombs[num];
			pendingDisplayBombs.RemoveAt(num);
			float num2 = (float)num * 0.2f;
			if (num2 <= 0f)
			{
				ShowIdleEffect(bomb);
			}
			else
			{
				UniTaskVoid val = ShowIdleEffectWithDelay(bomb, num2);
				((UniTaskVoid)(ref val)).Forget();
			}
		}
	}

	[AsyncStateMachine(typeof(_003CShowIdleEffectWithDelay_003Ed__56))]
	private UniTaskVoid ShowIdleEffectWithDelay(BombRelative bomb, float delay)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		_003CShowIdleEffectWithDelay_003Ed__56 _003CShowIdleEffectWithDelay_003Ed__ = default(_003CShowIdleEffectWithDelay_003Ed__56);
		_003CShowIdleEffectWithDelay_003Ed__._003C_003Et__builder = AsyncUniTaskVoidMethodBuilder.Create();
		_003CShowIdleEffectWithDelay_003Ed__._003C_003E4__this = this;
		_003CShowIdleEffectWithDelay_003Ed__.bomb = bomb;
		_003CShowIdleEffectWithDelay_003Ed__.delay = delay;
		_003CShowIdleEffectWithDelay_003Ed__._003C_003E1__state = -1;
		((AsyncUniTaskVoidMethodBuilder)(ref _003CShowIdleEffectWithDelay_003Ed__._003C_003Et__builder)).Start<_003CShowIdleEffectWithDelay_003Ed__56>(ref _003CShowIdleEffectWithDelay_003Ed__);
		return ((AsyncUniTaskVoidMethodBuilder)(ref _003CShowIdleEffectWithDelay_003Ed__._003C_003Et__builder)).Task;
	}

	private void ShowIdleEffect(BombRelative bomb)
	{
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		if (!((Object)(object)bomb == (Object)null))
		{
			Transform obj = ((Component)bomb).transform.Find("eff_rotatebomb_boom");
			GameObject val = ((obj != null) ? ((Component)obj).gameObject : null);
			if (!((Object)(object)val == (Object)null))
			{
				val.SetActive(true);
				TryStartBombIdleSound(bomb);
				UniTaskVoid val2 = PlayInThenIdle(bomb, val);
				((UniTaskVoid)(ref val2)).Forget();
			}
		}
	}

	[AsyncStateMachine(typeof(_003CPlayInThenIdle_003Ed__58))]
	private UniTaskVoid PlayInThenIdle(BombRelative bomb, GameObject instance)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		_003CPlayInThenIdle_003Ed__58 _003CPlayInThenIdle_003Ed__ = default(_003CPlayInThenIdle_003Ed__58);
		_003CPlayInThenIdle_003Ed__._003C_003Et__builder = AsyncUniTaskVoidMethodBuilder.Create();
		_003CPlayInThenIdle_003Ed__._003C_003E4__this = this;
		_003CPlayInThenIdle_003Ed__.bomb = bomb;
		_003CPlayInThenIdle_003Ed__.instance = instance;
		_003CPlayInThenIdle_003Ed__._003C_003E1__state = -1;
		((AsyncUniTaskVoidMethodBuilder)(ref _003CPlayInThenIdle_003Ed__._003C_003Et__builder)).Start<_003CPlayInThenIdle_003Ed__58>(ref _003CPlayInThenIdle_003Ed__);
		return ((AsyncUniTaskVoidMethodBuilder)(ref _003CPlayInThenIdle_003Ed__._003C_003Et__builder)).Task;
	}

	private void OnBombExplodeStart(BombExplodeStartEvent evt)
	{
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		if (!base.IsDisposed && !((Object)(object)evt?.Bomb == (Object)null))
		{
			pendingBoomCount++;
			UntrackBomb(evt.Bomb);
			if (!PresentationMode.UseTaskPresentation)
			{
				PlayBoom(evt.Bomb);
				UniTaskVoid val = SendBreakTickAfterDelay();
				((UniTaskVoid)(ref val)).Forget();
			}
		}
	}

	public void PlayBoom(BombRelative bomb)
	{
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00df: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fe: Unknown result type (might be due to invalid IL or missing references)
		//IL_0136: Unknown result type (might be due to invalid IL or missing references)
		//IL_013b: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d4: Unknown result type (might be due to invalid IL or missing references)
		if (base.IsDisposed || (Object)(object)bomb == (Object)null || (Object)(object)effectRoot == (Object)null)
		{
			return;
		}
		Transform obj = ((Component)bomb).transform.Find("eff_rotatebomb_boom");
		GameObject val = ((obj != null) ? ((Component)obj).gameObject : null);
		if (!((Object)(object)val == (Object)null))
		{
			Transform transform = val.transform;
			PullObject componentInParent = ((Component)bomb).GetComponentInParent<PullObject>();
			Vector2 val2 = default(Vector2);
			if ((Object)(object)componentInParent != (Object)null && (Object)(object)componentInParent.Rb != (Object)null)
			{
				val2 = componentInParent.Rb.position;
			}
			Vector3 val3 = transform.position;
			if ((Object)(object)componentInParent != (Object)null && (Object)(object)componentInParent.Rb != (Object)null)
			{
				Vector3 val4 = transform.position - ((Component)componentInParent).transform.position;
				val3 = new Vector3(val2.x, val2.y, 0f) + val4;
			}
			Quaternion rotation = transform.rotation;
			Vector3 lossyScale = transform.lossyScale;
			transform.SetParent(effectRoot, false);
			transform.SetPositionAndRotation(val3, rotation);
			transform.localScale = lossyScale;
			val.SetActive(true);
			PlayAnimation(val, "anim_rotatebomb_boom_start", loop: false);
			VibrateUtility.Vibrate(1106);
			base.Host.ScreenShake.RequestShake(ScreenShakeTriggerType.BombExplosion);
			UniTaskVoid val5 = RecycleAfterDelayAsync(val);
			((UniTaskVoid)(ref val5)).Forget();
		}
	}

	[AsyncStateMachine(typeof(_003CSendBreakTickAfterDelay_003Ed__61))]
	private UniTaskVoid SendBreakTickAfterDelay()
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		_003CSendBreakTickAfterDelay_003Ed__61 _003CSendBreakTickAfterDelay_003Ed__ = default(_003CSendBreakTickAfterDelay_003Ed__61);
		_003CSendBreakTickAfterDelay_003Ed__._003C_003Et__builder = AsyncUniTaskVoidMethodBuilder.Create();
		_003CSendBreakTickAfterDelay_003Ed__._003C_003E4__this = this;
		_003CSendBreakTickAfterDelay_003Ed__._003C_003E1__state = -1;
		((AsyncUniTaskVoidMethodBuilder)(ref _003CSendBreakTickAfterDelay_003Ed__._003C_003Et__builder)).Start<_003CSendBreakTickAfterDelay_003Ed__61>(ref _003CSendBreakTickAfterDelay_003Ed__);
		return ((AsyncUniTaskVoidMethodBuilder)(ref _003CSendBreakTickAfterDelay_003Ed__._003C_003Et__builder)).Task;
	}

	[AsyncStateMachine(typeof(_003CRecycleAfterDelayAsync_003Ed__62))]
	private UniTaskVoid RecycleAfterDelayAsync(GameObject instance)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		_003CRecycleAfterDelayAsync_003Ed__62 _003CRecycleAfterDelayAsync_003Ed__ = default(_003CRecycleAfterDelayAsync_003Ed__62);
		_003CRecycleAfterDelayAsync_003Ed__._003C_003Et__builder = AsyncUniTaskVoidMethodBuilder.Create();
		_003CRecycleAfterDelayAsync_003Ed__._003C_003E4__this = this;
		_003CRecycleAfterDelayAsync_003Ed__.instance = instance;
		_003CRecycleAfterDelayAsync_003Ed__._003C_003E1__state = -1;
		((AsyncUniTaskVoidMethodBuilder)(ref _003CRecycleAfterDelayAsync_003Ed__._003C_003Et__builder)).Start<_003CRecycleAfterDelayAsync_003Ed__62>(ref _003CRecycleAfterDelayAsync_003Ed__);
		return ((AsyncUniTaskVoidMethodBuilder)(ref _003CRecycleAfterDelayAsync_003Ed__._003C_003Et__builder)).Task;
	}

	private void OnBombRemoveWithoutExplosion(BombRemoveWithoutExplosionEvent evt)
	{
		if (!base.IsDisposed && !((Object)(object)evt?.Bomb == (Object)null))
		{
			Transform obj = ((Component)evt.Bomb).transform.Find("eff_rotatebomb_boom");
			GameObject val = ((obj != null) ? ((Component)obj).gameObject : null);
			if ((Object)(object)val != (Object)null)
			{
				RecycleEffect(val);
			}
			UntrackBomb(evt.Bomb);
		}
	}

	private void UntrackBomb(BombRelative bomb)
	{
		if (!((Object)(object)bomb == (Object)null) && trackedBombs.Remove(bomb))
		{
			aliveBombCount--;
			if (aliveBombCount == 0)
			{
				StopBombIdleSound();
			}
		}
	}

	private static void StopBombIdleSound()
	{
		AudioService.StopSound(AudioKeys.ItemBombIdle.Value);
	}

	private void TryStartBombIdleSound(BombRelative bomb)
	{
		if (!((Object)(object)bomb == (Object)null) && trackedBombs.Add(bomb))
		{
			aliveBombCount++;
			if (aliveBombCount == 1)
			{
				AudioService.PlayLoopIfNotPlaying(AudioKeys.ItemBombIdle.Value);
			}
		}
	}

	private static void PlayAnimation(GameObject instance, string clipName, bool loop)
	{
		Animation component = instance.GetComponent<Animation>();
		if (!((Object)(object)component == (Object)null) && !((Object)(object)component.GetClip(clipName) == (Object)null))
		{
			component.wrapMode = (WrapMode)((!loop) ? 1 : 2);
			component.Play(clipName);
		}
	}

	private static void PlayParticles(GameObject instance)
	{
		ParticleSystem[] componentsInChildren = instance.GetComponentsInChildren<ParticleSystem>(true);
		foreach (ParticleSystem val in componentsInChildren)
		{
			if (!((Object)(object)val == (Object)null))
			{
				val.Play(true);
			}
		}
	}

	private static void RecycleEffect(GameObject instance)
	{
		if ((Object)(object)instance == (Object)null)
		{
			return;
		}
		ParticleSystem[] componentsInChildren = instance.GetComponentsInChildren<ParticleSystem>(true);
		foreach (ParticleSystem val in componentsInChildren)
		{
			if (!((Object)(object)val == (Object)null))
			{
				val.Stop(true, (ParticleSystemStopBehavior)0);
				val.Clear();
			}
		}
		if (Application.isPlaying)
		{
			GameApp.Pool.Recycle(instance);
		}
		else
		{
			Object.DestroyImmediate((Object)(object)instance);
		}
	}
}
