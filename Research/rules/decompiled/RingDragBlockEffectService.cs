using System;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.CompilerServices;
using UniLog;
using UnityEngine;
using UnlockRing.Core.Domain;
using UnlockRing.Core.Queries;
using UnlockRing.Core.Runtime;
using Vibration;

public sealed class RingDragBlockEffectService : IDisposable
{
	private readonly struct DragBlockEffectKey : IEquatable<DragBlockEffectKey>
	{
		private readonly PartId movingPart;

		private readonly FeatureId movingFeature;

		private readonly PartId blockingPart;

		private readonly FeatureId blockingFeature;

		private readonly int requestedSign;

		public DragBlockEffectKey(CollisionWitness witness, double requestedAmount)
		{
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			//IL_0007: Unknown result type (might be due to invalid IL or missing references)
			//IL_000e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0013: Unknown result type (might be due to invalid IL or missing references)
			//IL_001a: Unknown result type (might be due to invalid IL or missing references)
			//IL_001f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0026: Unknown result type (might be due to invalid IL or missing references)
			//IL_002b: Unknown result type (might be due to invalid IL or missing references)
			movingPart = witness.MovingPart;
			movingFeature = witness.MovingFeature;
			blockingPart = witness.BlockingPart;
			blockingFeature = witness.BlockingFeature;
			requestedSign = Math.Sign(requestedAmount);
		}

		public bool Equals(DragBlockEffectKey other)
		{
			//IL_0007: Unknown result type (might be due to invalid IL or missing references)
			//IL_001a: Unknown result type (might be due to invalid IL or missing references)
			//IL_002d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0040: Unknown result type (might be due to invalid IL or missing references)
			if (((PartId)(ref movingPart)).Equals(other.movingPart) && ((FeatureId)(ref movingFeature)).Equals(other.movingFeature) && ((PartId)(ref blockingPart)).Equals(other.blockingPart) && ((FeatureId)(ref blockingFeature)).Equals(other.blockingFeature))
			{
				return requestedSign == other.requestedSign;
			}
			return false;
		}

		public override bool Equals(object obj)
		{
			if (obj is DragBlockEffectKey other)
			{
				return Equals(other);
			}
			return false;
		}

		public override int GetHashCode()
		{
			//IL_000b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0018: Unknown result type (might be due to invalid IL or missing references)
			//IL_0025: Unknown result type (might be due to invalid IL or missing references)
			//IL_0032: Unknown result type (might be due to invalid IL or missing references)
			HashCode hashCode = default(HashCode);
			hashCode.Add<PartId>(movingPart);
			hashCode.Add<FeatureId>(movingFeature);
			hashCode.Add<PartId>(blockingPart);
			hashCode.Add<FeatureId>(blockingFeature);
			hashCode.Add(requestedSign);
			return hashCode.ToHashCode();
		}
	}

	[StructLayout(LayoutKind.Auto)]
	[CompilerGenerated]
	private struct _003CPlayDragBlockEffectAsync_003Ed__22 : IAsyncStateMachine
	{
		public int _003C_003E1__state;

		public AsyncUniTaskVoidMethodBuilder _003C_003Et__builder;

		public CoreBoardRenderer board;

		public CollisionWitness witness;

		public RingDragBlockEffectService _003C_003E4__this;

		private GameObject _003Cinstance_003E5__2;

		private Awaiter<bool> _003C_003Eu__1;

		private void MoveNext()
		{
			//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
			//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
			//IL_002b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0035: Unknown result type (might be due to invalid IL or missing references)
			//IL_003a: Unknown result type (might be due to invalid IL or missing references)
			//IL_003c: Unknown result type (might be due to invalid IL or missing references)
			//IL_006b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0070: Unknown result type (might be due to invalid IL or missing references)
			//IL_0074: Unknown result type (might be due to invalid IL or missing references)
			//IL_0079: Unknown result type (might be due to invalid IL or missing references)
			//IL_007d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0082: Unknown result type (might be due to invalid IL or missing references)
			//IL_0096: Unknown result type (might be due to invalid IL or missing references)
			//IL_0097: Unknown result type (might be due to invalid IL or missing references)
			int num = _003C_003E1__state;
			RingDragBlockEffectService ringDragBlockEffectService = _003C_003E4__this;
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
						goto IL_00cb;
					}
					Vector3 worldPos = board.CorePointToUnity(witness.ContactPoint);
					if (ringDragBlockEffectService.TrySpawnDragBlockEffect(worldPos, board, out _003Cinstance_003E5__2))
					{
						UniTask val2 = UniTask.Delay(TimeSpan.FromSeconds(1.0), false, (PlayerLoopTiming)8, ringDragBlockEffectService.cancelToken);
						val = ((UniTask)(ref val2)).SuppressCancellationThrow().GetAwaiter();
						if (!val.IsCompleted)
						{
							num = (_003C_003E1__state = 0);
							_003C_003Eu__1 = val;
							((AsyncUniTaskVoidMethodBuilder)(ref _003C_003Et__builder)).AwaitUnsafeOnCompleted<Awaiter<bool>, _003CPlayDragBlockEffectAsync_003Ed__22>(ref val, ref this);
							return;
						}
						goto IL_00cb;
					}
					goto end_IL_0019;
					IL_00cb:
					bool result = val.GetResult();
					RecycleEffect(_003Cinstance_003E5__2);
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
						RecycleEffect(_003Cinstance_003E5__2);
					}
					Log.LogError("[RingDragBlockEffectService] ", "播放阻挡特效失败: {0}", new object[1] { ex.Message });
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

	private const string TAG = "[RingDragBlockEffectService] ";

	private const string EffectLocation = "eff_rotatecollision_boom";

	private const string ContinuousEffectLocation = "eff_rotatecollisionrub_boom";

	private const float EffectZOffset = -0.01f;

	private const float EffectRecycleDelay = 1f;

	private const float MinRepeatEffectInterval = 0.12f;

	private readonly CancellationToken cancelToken;

	private DragBlockEffectKey? activeDragBlockContactKey;

	private DragBlockEffectKey? lastPlayedDragBlockKey;

	private bool dragBlockEffectArmed;

	private float lastEffectPlayUnscaledTime;

	private GameObject effectPrefab;

	private GameObject rubEffectPrefab;

	private bool inStickRingCollision;

	private GameObject activeRubInstance;

	private bool disposed;

	public RingDragBlockEffectService(RingGameManager gameManager)
	{
		if ((Object)(object)gameManager == (Object)null)
		{
			throw new ArgumentNullException("gameManager");
		}
		cancelToken = UniTaskCancellationExtensions.GetCancellationTokenOnDestroy((Component)(object)gameManager);
	}

	public void Dispose()
	{
		if (!disposed)
		{
			disposed = true;
			ResetDragBlockTracking();
			effectPrefab = null;
			rubEffectPrefab = null;
		}
	}

	public void ResetDragBlockTracking()
	{
		activeDragBlockContactKey = null;
		lastPlayedDragBlockKey = null;
		dragBlockEffectArmed = false;
		lastEffectPlayUnscaledTime = 0f;
		RecycleRubEffect();
	}

	public void OnDragBlockPreview(CoreBoardRenderer board, GestureMotionDomain domain, MotionPreview fullPreview, double requestedAmount, bool suppressFeedback = false)
	{
		if (!suppressFeedback && !((Object)(object)board == (Object)null) && domain != null && fullPreview != null)
		{
			TryUpdateStickRingRubEffect(board, domain, fullPreview, requestedAmount);
			TryPlayDragBlockEffect(board, domain, fullPreview, requestedAmount);
		}
	}

	private void TryUpdateStickRingRubEffect(CoreBoardRenderer board, GestureMotionDomain domain, MotionPreview preview, double requestedAmount)
	{
		if (IsPartReleasedPreview(domain, preview))
		{
			if (inStickRingCollision)
			{
				RecycleRubEffect();
			}
			return;
		}
		LevelDefinition val = board.Controller?.Level;
		if (val == null)
		{
			return;
		}
		CollisionWitness val2 = ResolveDragBlockWitness(preview, requestedAmount);
		if (val2 != null && IsStickRingWitness(val, val2))
		{
			if (!inStickRingCollision)
			{
				inStickRingCollision = true;
				if (!TrySpawnRubEffect(board, val2))
				{
					inStickRingCollision = false;
				}
			}
			else
			{
				UpdateRubEffectPosition(board, val2);
			}
		}
		else if (inStickRingCollision)
		{
			RecycleRubEffect();
		}
	}

	private void TryPlayDragBlockEffect(CoreBoardRenderer board, GestureMotionDomain domain, MotionPreview preview, double requestedAmount)
	{
		//IL_00ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f1: Unknown result type (might be due to invalid IL or missing references)
		if (IsPartReleasedPreview(domain, preview))
		{
			return;
		}
		if (!IsDragBlockTruncated(preview, requestedAmount))
		{
			dragBlockEffectArmed = true;
			activeDragBlockContactKey = null;
		}
		else
		{
			if (!dragBlockEffectArmed)
			{
				return;
			}
			CollisionWitness val = ResolveDragBlockWitness(preview, requestedAmount);
			if (val == null)
			{
				return;
			}
			LevelDefinition val2 = board.Controller?.Level;
			if (val2 != null && IsStickRingWitness(val2, val))
			{
				return;
			}
			DragBlockEffectKey dragBlockEffectKey = new DragBlockEffectKey(val, requestedAmount);
			if (!activeDragBlockContactKey.HasValue || !activeDragBlockContactKey.Value.Equals(dragBlockEffectKey))
			{
				float unscaledTime = Time.unscaledTime;
				if (!lastPlayedDragBlockKey.HasValue || !lastPlayedDragBlockKey.Value.Equals(dragBlockEffectKey) || !(unscaledTime - lastEffectPlayUnscaledTime < 0.12f))
				{
					activeDragBlockContactKey = dragBlockEffectKey;
					lastPlayedDragBlockKey = dragBlockEffectKey;
					lastEffectPlayUnscaledTime = unscaledTime;
					UniTaskVoid val3 = PlayDragBlockEffectAsync(board, val);
					((UniTaskVoid)(ref val3)).Forget();
				}
			}
		}
	}

	[AsyncStateMachine(typeof(_003CPlayDragBlockEffectAsync_003Ed__22))]
	private UniTaskVoid PlayDragBlockEffectAsync(CoreBoardRenderer board, CollisionWitness witness)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		_003CPlayDragBlockEffectAsync_003Ed__22 _003CPlayDragBlockEffectAsync_003Ed__ = default(_003CPlayDragBlockEffectAsync_003Ed__22);
		_003CPlayDragBlockEffectAsync_003Ed__._003C_003Et__builder = AsyncUniTaskVoidMethodBuilder.Create();
		_003CPlayDragBlockEffectAsync_003Ed__._003C_003E4__this = this;
		_003CPlayDragBlockEffectAsync_003Ed__.board = board;
		_003CPlayDragBlockEffectAsync_003Ed__.witness = witness;
		_003CPlayDragBlockEffectAsync_003Ed__._003C_003E1__state = -1;
		((AsyncUniTaskVoidMethodBuilder)(ref _003CPlayDragBlockEffectAsync_003Ed__._003C_003Et__builder)).Start<_003CPlayDragBlockEffectAsync_003Ed__22>(ref _003CPlayDragBlockEffectAsync_003Ed__);
		return ((AsyncUniTaskVoidMethodBuilder)(ref _003CPlayDragBlockEffectAsync_003Ed__._003C_003Et__builder)).Task;
	}

	private bool TrySpawnRubEffect(CoreBoardRenderer board, CollisionWitness witness)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		Vector3 worldPos = board.CorePointToUnity(witness.ContactPoint);
		if (!TrySpawnRubEffectAt(worldPos, board, out var instance))
		{
			return false;
		}
		activeRubInstance = instance;
		VibrateUtility.Vibrate(1100);
		return true;
	}

	private void UpdateRubEffectPosition(CoreBoardRenderer board, CollisionWitness witness)
	{
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		if (!((Object)(object)activeRubInstance == (Object)null))
		{
			activeRubInstance.transform.position = board.CorePointToUnity(witness.ContactPoint);
		}
	}

	private void RecycleRubEffect()
	{
		if ((Object)(object)activeRubInstance == (Object)null)
		{
			inStickRingCollision = false;
			return;
		}
		RecycleEffect(activeRubInstance);
		activeRubInstance = null;
		inStickRingCollision = false;
	}

	private bool TrySpawnDragBlockEffect(Vector3 worldPos, CoreBoardRenderer board, out GameObject instance)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		if (!TrySpawnEffectPrefab("eff_rotatecollision_boom", ref effectPrefab, worldPos, board, out instance))
		{
			return false;
		}
		VibrateUtility.Vibrate(1100);
		return true;
	}

	private bool TrySpawnRubEffectAt(Vector3 worldPos, CoreBoardRenderer board, out GameObject instance)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		return TrySpawnEffectPrefab("eff_rotatecollisionrub_boom", ref rubEffectPrefab, worldPos, board, out instance);
	}

	private bool TrySpawnEffectPrefab(string location, ref GameObject prefabCache, Vector3 worldPos, CoreBoardRenderer board, out GameObject instance)
	{
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		instance = null;
		if ((Object)(object)board.BoardRoot == (Object)null)
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
		instance = GameApp.Pool.Spawn(prefabCache, board.BoardRoot.transform);
		if ((Object)(object)instance == (Object)null)
		{
			return false;
		}
		Transform transform = instance.transform;
		transform.position = worldPos;
		transform.rotation = Quaternion.identity;
		transform.localScale = Vector3.one;
		return true;
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

	private static bool IsStickRingWitness(LevelDefinition level, CollisionWitness witness)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		if ((int)witness.ContactKind != 0)
		{
			return false;
		}
		PartDefinition val = TryFindPart(level, witness.MovingPart);
		PartDefinition val2 = TryFindPart(level, witness.BlockingPart);
		if (val == null || val2 == null)
		{
			return false;
		}
		return IsStickRingPair(val, val2);
	}

	private static bool IsStickRingPair(PartDefinition first, PartDefinition second)
	{
		if (!IsRing(first) || !IsStick(second))
		{
			if (IsStick(first))
			{
				return IsRing(second);
			}
			return false;
		}
		return true;
	}

	private static bool IsRing(PartDefinition part)
	{
		return part.Behaviors.Any((BehaviorDefinition behavior) => behavior is RotatableBehaviorDefinition);
	}

	private static bool IsStick(PartDefinition part)
	{
		return BoardRenderUtils.IsSliding(part);
	}

	private static PartDefinition? TryFindPart(LevelDefinition level, PartId partId)
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		foreach (PartDefinition part in level.Parts)
		{
			PartId id = part.Id;
			if (((PartId)(ref id)).Equals(partId))
			{
				return part;
			}
		}
		return null;
	}

	private CollisionWitness? ResolveDragBlockWitness(MotionPreview preview, double requestedAmount)
	{
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		if (!IsDragBlockTruncated(preview, requestedAmount))
		{
			return null;
		}
		CollisionWitness collision = preview.Result.Collision;
		if (collision != null && (int)collision.ContactKind == 0 && IsDragBlockWitnessWithinPath(collision, preview, requestedAmount))
		{
			return collision;
		}
		double num = (preview.CanApply ? Math.Max(Math.Abs(preview.Result.ActualAmount), Math.Abs(requestedAmount)) : Math.Abs(requestedAmount));
		double num2 = Math.Abs(preview.CanApply ? preview.Result.ActualAmount : requestedAmount);
		CollisionWitness val = null;
		double num3 = double.MaxValue;
		foreach (CollisionWitness criticalEvent in preview.CriticalEvents)
		{
			if ((int)criticalEvent.ContactKind == 0 && !(criticalEvent.MotionParameter > num + 0.001))
			{
				double num4 = Math.Abs(criticalEvent.MotionParameter - num2);
				if (val == null || num4 < num3)
				{
					val = criticalEvent;
					num3 = num4;
				}
			}
		}
		return val;
	}

	private bool IsDragBlockWitnessWithinPath(CollisionWitness witness, MotionPreview preview, double requestedAmount)
	{
		if (!preview.CanApply)
		{
			return false;
		}
		double num = Math.Abs(requestedAmount);
		if (num < 1E-09)
		{
			return false;
		}
		double num2 = Math.Min(1.0, Math.Max(Math.Abs(preview.Result.ActualAmount), num) / num);
		if (witness.MotionParameter <= 1.001)
		{
			return witness.MotionParameter <= num2 + 0.001;
		}
		double num3 = Math.Max(Math.Abs(preview.Result.ActualAmount), num);
		return witness.MotionParameter <= num3 + 0.001;
	}

	private bool IsPartReleasedPreview(GestureMotionDomain domain, MotionPreview preview)
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		if (!preview.Result.State.Parts.TryGetValue(domain.PartId, out var value))
		{
			return false;
		}
		if (!value.Released)
		{
			return value.Removed;
		}
		return true;
	}

	private bool IsDragBlockTruncated(MotionPreview preview, double requestedAmount)
	{
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		if (IsMotionTruncated(preview, requestedAmount))
		{
			return true;
		}
		if (!preview.CanApply)
		{
			return false;
		}
		double num = Math.Max(Math.Abs(preview.Result.ActualAmount), Math.Abs(requestedAmount));
		foreach (CollisionWitness criticalEvent in preview.CriticalEvents)
		{
			if ((int)criticalEvent.ContactKind == 0 && criticalEvent.MotionParameter <= num + 0.001)
			{
				return true;
			}
		}
		return false;
	}

	private bool IsMotionTruncated(MotionPreview preview, double requestedAmount)
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Invalid comparison between Unknown and I4
		if (!preview.CanApply || (int)preview.Result.Status != 1)
		{
			return false;
		}
		return Math.Abs(preview.Result.ActualAmount) + 1E-06 < Math.Abs(requestedAmount);
	}
}
