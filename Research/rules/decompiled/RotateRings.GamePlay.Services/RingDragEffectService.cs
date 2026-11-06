using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.CompilerServices;
using RotateRings.GamePlay.Events;
using UniLog;
using UnityEngine;
using Vibration;

namespace RotateRings.GamePlay.Services;

public sealed class RingDragEffectService : GameplayServiceBase<GameplayManager>
{
	private readonly struct ColliderContactKey : IEquatable<ColliderContactKey>
	{
		private readonly int movingId;

		private readonly int otherId;

		public int MovingId => movingId;

		public ColliderContactKey(int movingId, int otherId)
		{
			this.movingId = movingId;
			this.otherId = otherId;
		}

		public bool Equals(ColliderContactKey other)
		{
			if (movingId == other.movingId)
			{
				return otherId == other.otherId;
			}
			return false;
		}

		public override bool Equals(object obj)
		{
			if (obj is ColliderContactKey other)
			{
				return Equals(other);
			}
			return false;
		}

		public override int GetHashCode()
		{
			return (movingId * 397) ^ otherId;
		}
	}

	[StructLayout(LayoutKind.Auto)]
	[CompilerGenerated]
	private struct _003CPlayDragStopEffectAsync_003Ed__15 : IAsyncStateMachine
	{
		public int _003C_003E1__state;

		public AsyncUniTaskVoidMethodBuilder _003C_003Et__builder;

		public RingDragEffectService _003C_003E4__this;

		public Vector2 contactWorld;

		private GameObject _003Cinstance_003E5__2;

		private Awaiter<bool> _003C_003Eu__1;

		private void MoveNext()
		{
			//IL_0098: Unknown result type (might be due to invalid IL or missing references)
			//IL_009d: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
			//IL_001e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0023: Unknown result type (might be due to invalid IL or missing references)
			//IL_0028: Unknown result type (might be due to invalid IL or missing references)
			//IL_002a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0053: Unknown result type (might be due to invalid IL or missing references)
			//IL_0058: Unknown result type (might be due to invalid IL or missing references)
			//IL_005c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0061: Unknown result type (might be due to invalid IL or missing references)
			//IL_0065: Unknown result type (might be due to invalid IL or missing references)
			//IL_006a: Unknown result type (might be due to invalid IL or missing references)
			//IL_007e: Unknown result type (might be due to invalid IL or missing references)
			//IL_007f: Unknown result type (might be due to invalid IL or missing references)
			int num = _003C_003E1__state;
			RingDragEffectService ringDragEffectService = _003C_003E4__this;
			try
			{
				if (num != 0)
				{
					_003Cinstance_003E5__2 = null;
				}
				try
				{
					Awaiter<bool> val;
					if (num == 0)
					{
						val = _003C_003Eu__1;
						_003C_003Eu__1 = default(Awaiter<bool>);
						num = (_003C_003E1__state = -1);
						goto IL_00b3;
					}
					Vector3 worldPos = ringDragEffectService.ToEffectWorldPos(contactWorld);
					if (ringDragEffectService.TrySpawnDragStopEffect(worldPos, out _003Cinstance_003E5__2))
					{
						UniTask val2 = UniTask.Delay(TimeSpan.FromSeconds(0.5), false, (PlayerLoopTiming)8, ringDragEffectService.cancelToken);
						val = ((UniTask)(ref val2)).SuppressCancellationThrow().GetAwaiter();
						if (!val.IsCompleted)
						{
							num = (_003C_003E1__state = 0);
							_003C_003Eu__1 = val;
							((AsyncUniTaskVoidMethodBuilder)(ref _003C_003Et__builder)).AwaitUnsafeOnCompleted<Awaiter<bool>, _003CPlayDragStopEffectAsync_003Ed__15>(ref val, ref this);
							return;
						}
						goto IL_00b3;
					}
					goto end_IL_0019;
					IL_00b3:
					bool result = val.GetResult();
					ringDragEffectService.RecycleEffect(_003Cinstance_003E5__2);
					_003Cinstance_003E5__2 = null;
					if (!result)
					{
					}
					end_IL_0019:;
				}
				catch (Exception ex)
				{
					if ((Object)(object)_003Cinstance_003E5__2 != (Object)null)
					{
						ringDragEffectService.RecycleEffect(_003Cinstance_003E5__2);
					}
					Log.LogError("[GameplayDragBlockEffectService] ", "播放阻挡特效失败: {0}", new object[1] { ex.Message });
				}
			}
			catch (Exception exception)
			{
				_003C_003E1__state = -2;
				_003Cinstance_003E5__2 = null;
				((AsyncUniTaskVoidMethodBuilder)(ref _003C_003Et__builder)).SetException(exception);
				return;
			}
			_003C_003E1__state = -2;
			_003Cinstance_003E5__2 = null;
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

	private const string TAG = "[GameplayDragBlockEffectService] ";

	private const string EffectLocation = "eff_rotatecollision_boom";

	private const float EffectRecycleDelay = 0.5f;

	private readonly CancellationToken cancelToken;

	private readonly HashSet<ColliderContactKey> enteredColliderKeys = new HashSet<ColliderContactKey>();

	private GameObject effectPrefab;

	private Transform effectRoot;

	public RingDragEffectService(GameplayManager host)
		: base(host)
	{
		cancelToken = UniTaskCancellationExtensions.GetCancellationTokenOnDestroy((Component)(object)host);
		effectRoot = host.EffectRoot;
		GameApp.Event.Listen<RingDragCollisionEnterEvent>((Action<RingDragCollisionEnterEvent>)OnCollisionEnter);
		GameApp.Event.Listen<RingDragCollisionExitEvent>((Action<RingDragCollisionExitEvent>)OnCollisionExit);
		GameApp.Event.Listen<RingDragCollisionStopEvent>((Action<RingDragCollisionStopEvent>)OnCollisionStop);
		GameApp.Event.Listen<RingDragCollisionResetEvent>((Action<RingDragCollisionResetEvent>)OnDragCollisionReset);
	}

	public override void Dispose()
	{
		if (!base.IsDisposed)
		{
			base.Dispose();
			GameApp.Event.Unlisten<RingDragCollisionEnterEvent>((Action<RingDragCollisionEnterEvent>)OnCollisionEnter);
			GameApp.Event.Unlisten<RingDragCollisionExitEvent>((Action<RingDragCollisionExitEvent>)OnCollisionExit);
			GameApp.Event.Unlisten<RingDragCollisionStopEvent>((Action<RingDragCollisionStopEvent>)OnCollisionStop);
			GameApp.Event.Unlisten<RingDragCollisionResetEvent>((Action<RingDragCollisionResetEvent>)OnDragCollisionReset);
			enteredColliderKeys.Clear();
			effectPrefab = null;
			effectRoot = null;
		}
	}

	private void OnDragCollisionReset(RingDragCollisionResetEvent evt)
	{
		if (!base.IsDisposed)
		{
			enteredColliderKeys.Clear();
		}
	}

	private void OnCollisionEnter(RingDragCollisionEnterEvent evt)
	{
		if (!base.IsDisposed && evt != null && IsValidCollider(evt.MovingRoot, evt.OtherOwner))
		{
			ColliderContactKey item = CreateContactKey(evt.MovingRoot, evt.OtherCollider);
			enteredColliderKeys.Add(item);
		}
	}

	private void OnCollisionExit(RingDragCollisionExitEvent evt)
	{
		if (!base.IsDisposed && evt != null && IsValidCollider(evt.MovingRoot, evt.OtherOwner))
		{
			ColliderContactKey item = CreateContactKey(evt.MovingRoot, evt.OtherCollider);
			enteredColliderKeys.Remove(item);
		}
	}

	private void OnCollisionStop(RingDragCollisionStopEvent evt)
	{
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		if (!base.IsDisposed && evt != null)
		{
			ColliderContactKey item = CreateContactKey(evt.MovingRoot, evt.OtherCollider);
			if (enteredColliderKeys.Contains(item))
			{
				UniTaskVoid val = PlayDragStopEffectAsync(evt.PosWorld);
				((UniTaskVoid)(ref val)).Forget();
			}
		}
	}

	private bool IsValidCollider(Component movingRoot, IGameplayObject otherOwner)
	{
		if ((Object)(object)movingRoot == (Object)null)
		{
			return false;
		}
		if (!(movingRoot is RingObject) && !(movingRoot is PullObject))
		{
			return false;
		}
		if (otherOwner != null)
		{
			return otherOwner != movingRoot;
		}
		return false;
	}

	private Vector3 ToEffectWorldPos(Vector2 contactWorld)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		return new Vector3(contactWorld.x, contactWorld.y, -5f);
	}

	[AsyncStateMachine(typeof(_003CPlayDragStopEffectAsync_003Ed__15))]
	private UniTaskVoid PlayDragStopEffectAsync(Vector2 contactWorld)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		_003CPlayDragStopEffectAsync_003Ed__15 _003CPlayDragStopEffectAsync_003Ed__ = default(_003CPlayDragStopEffectAsync_003Ed__15);
		_003CPlayDragStopEffectAsync_003Ed__._003C_003Et__builder = AsyncUniTaskVoidMethodBuilder.Create();
		_003CPlayDragStopEffectAsync_003Ed__._003C_003E4__this = this;
		_003CPlayDragStopEffectAsync_003Ed__.contactWorld = contactWorld;
		_003CPlayDragStopEffectAsync_003Ed__._003C_003E1__state = -1;
		((AsyncUniTaskVoidMethodBuilder)(ref _003CPlayDragStopEffectAsync_003Ed__._003C_003Et__builder)).Start<_003CPlayDragStopEffectAsync_003Ed__15>(ref _003CPlayDragStopEffectAsync_003Ed__);
		return ((AsyncUniTaskVoidMethodBuilder)(ref _003CPlayDragStopEffectAsync_003Ed__._003C_003Et__builder)).Task;
	}

	private bool TrySpawnEffectPrefab(string location, ref GameObject prefabCache, Vector3 worldPos, out GameObject instance)
	{
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		instance = null;
		if ((Object)(object)effectRoot == (Object)null)
		{
			return false;
		}
		if (prefabCache == null)
		{
			prefabCache = GameApp.Res.LoadAsset<GameObject>(location, "UnlockRing_Effect");
		}
		if ((Object)(object)prefabCache == (Object)null)
		{
			return false;
		}
		instance = GameApp.Pool.Spawn(prefabCache, effectRoot);
		if ((Object)(object)instance == (Object)null)
		{
			return false;
		}
		Transform transform = instance.transform;
		worldPos.z = -5f;
		transform.position = worldPos;
		transform.rotation = Quaternion.identity;
		transform.localScale = Vector3.one;
		ApplyDefaultLayer(instance);
		return true;
	}

	private bool TrySpawnDragStopEffect(Vector3 worldPos, out GameObject instance)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		if (!TrySpawnEffectPrefab("eff_rotatecollision_boom", ref effectPrefab, worldPos, out instance))
		{
			return false;
		}
		VibrateUtility.Vibrate(1100);
		return true;
	}

	private void RecycleEffect(GameObject instance)
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

	private void ApplyDefaultLayer(GameObject instance)
	{
		if (!((Object)(object)instance == (Object)null))
		{
			ApplyLayerRecursively(instance.transform, LayerMask.NameToLayer("Default"));
		}
	}

	private void ApplyLayerRecursively(Transform root, int layer)
	{
		((Component)root).gameObject.layer = layer;
		for (int i = 0; i < root.childCount; i++)
		{
			ApplyLayerRecursively(root.GetChild(i), layer);
		}
	}

	private ColliderContactKey CreateContactKey(Component movingRoot, Collider2D otherCollider)
	{
		return new ColliderContactKey(((Object)(object)movingRoot != (Object)null) ? ((Object)movingRoot).GetInstanceID() : 0, ((Object)(object)otherCollider != (Object)null) ? ((Object)otherCollider).GetInstanceID() : 0);
	}
}
