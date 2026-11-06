using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.CompilerServices;
using RotateRings.UI.Game;
using Spine;
using Spine.Unity;
using UniLog;
using UnityEngine;

namespace RotateRings.GamePlay.Services.CharacterExpression;

public sealed class CharacterExpressionEffectPlayer : IDisposable
{
	private readonly struct PendingPlayRequest
	{
		public string EffectLocation { get; }

		public Vector3 WorldPosition { get; }

		public PendingPlayRequest(string effectLocation, Vector3 worldPosition)
		{
			//IL_0008: Unknown result type (might be due to invalid IL or missing references)
			//IL_0009: Unknown result type (might be due to invalid IL or missing references)
			EffectLocation = effectLocation;
			WorldPosition = worldPosition;
		}
	}

	[StructLayout(LayoutKind.Auto)]
	[CompilerGenerated]
	private struct _003CPlayAsync_003Ed__18 : IAsyncStateMachine
	{
		public int _003C_003E1__state;

		public AsyncUniTaskVoidMethodBuilder _003C_003Et__builder;

		public CharacterExpressionEffectPlayer _003C_003E4__this;

		public string effectLocation;

		public CancellationToken ct;

		public Vector3 worldPosition;

		private GameObject _003Cprefab_003E5__2;

		private GameObject _003Cinstance_003E5__3;

		private Awaiter<GameObject> _003C_003Eu__1;

		private Awaiter<bool> _003C_003Eu__2;

		private void MoveNext()
		{
			//IL_0096: Unknown result type (might be due to invalid IL or missing references)
			//IL_009b: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
			//IL_0156: Unknown result type (might be due to invalid IL or missing references)
			//IL_015b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0163: Unknown result type (might be due to invalid IL or missing references)
			//IL_02af: Unknown result type (might be due to invalid IL or missing references)
			//IL_02b4: Unknown result type (might be due to invalid IL or missing references)
			//IL_02bc: Unknown result type (might be due to invalid IL or missing references)
			//IL_0058: Unknown result type (might be due to invalid IL or missing references)
			//IL_005d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0061: Unknown result type (might be due to invalid IL or missing references)
			//IL_0066: Unknown result type (might be due to invalid IL or missing references)
			//IL_007b: Unknown result type (might be due to invalid IL or missing references)
			//IL_007d: Unknown result type (might be due to invalid IL or missing references)
			//IL_010f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0114: Unknown result type (might be due to invalid IL or missing references)
			//IL_0118: Unknown result type (might be due to invalid IL or missing references)
			//IL_011d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0121: Unknown result type (might be due to invalid IL or missing references)
			//IL_0126: Unknown result type (might be due to invalid IL or missing references)
			//IL_013b: Unknown result type (might be due to invalid IL or missing references)
			//IL_013d: Unknown result type (might be due to invalid IL or missing references)
			//IL_01e5: Unknown result type (might be due to invalid IL or missing references)
			//IL_01f6: Unknown result type (might be due to invalid IL or missing references)
			//IL_0268: Unknown result type (might be due to invalid IL or missing references)
			//IL_026d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0271: Unknown result type (might be due to invalid IL or missing references)
			//IL_0276: Unknown result type (might be due to invalid IL or missing references)
			//IL_027a: Unknown result type (might be due to invalid IL or missing references)
			//IL_027f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0294: Unknown result type (might be due to invalid IL or missing references)
			//IL_0296: Unknown result type (might be due to invalid IL or missing references)
			int num = _003C_003E1__state;
			CharacterExpressionEffectPlayer characterExpressionEffectPlayer = _003C_003E4__this;
			try
			{
				if ((uint)num > 2u)
				{
					characterExpressionEffectPlayer.isPlaying = true;
				}
				try
				{
					Awaiter<GameObject> val2;
					Awaiter<bool> val;
					RectTransform mountParent;
					UniTask val3;
					GameObject result;
					switch (num)
					{
					default:
						if (!characterExpressionEffectPlayer.effectPrefabs.TryGetValue(effectLocation, out _003Cprefab_003E5__2))
						{
							val2 = GameApp.Res.LoadAssetAsync<GameObject>(effectLocation, "UnlockRing_Effect").GetAwaiter();
							if (!val2.IsCompleted)
							{
								num = (_003C_003E1__state = 0);
								_003C_003Eu__1 = val2;
								((AsyncUniTaskVoidMethodBuilder)(ref _003C_003Et__builder)).AwaitUnsafeOnCompleted<Awaiter<GameObject>, _003CPlayAsync_003Ed__18>(ref val2, ref this);
								return;
							}
							goto IL_00b2;
						}
						goto IL_00ed;
					case 0:
						val2 = _003C_003Eu__1;
						_003C_003Eu__1 = default(Awaiter<GameObject>);
						num = (_003C_003E1__state = -1);
						goto IL_00b2;
					case 1:
						val = _003C_003Eu__2;
						_003C_003Eu__2 = default(Awaiter<bool>);
						num = (_003C_003E1__state = -1);
						goto IL_0172;
					case 2:
						{
							val = _003C_003Eu__2;
							_003C_003Eu__2 = default(Awaiter<bool>);
							num = (_003C_003E1__state = -1);
							break;
						}
						IL_0172:
						if (!val.GetResult() && !ct.IsCancellationRequested && !characterExpressionEffectPlayer.disposed && UIGameRingRectUiClamp.TryResolveEncouragementMountParent(out mountParent))
						{
							_003Cinstance_003E5__3 = GameApp.Pool.Spawn(_003Cprefab_003E5__2, (Transform)(object)mountParent);
							if (!((Object)(object)_003Cinstance_003E5__3 == (Object)null))
							{
								_003Cinstance_003E5__3.SetActive(false);
								if (TrySetUiPosition(_003Cinstance_003E5__3.transform, worldPosition, mountParent, _003Cprefab_003E5__2.transform.localScale))
								{
									ResetAndPlayAppear(_003Cinstance_003E5__3);
									TrackFrontLayerInstance(_003Cinstance_003E5__3);
									_003Cinstance_003E5__3.transform.SetAsLastSibling();
									_003Cinstance_003E5__3.SetActive(true);
									BringActiveInstancesToFront(mountParent);
									val3 = UniTask.Delay(TimeSpan.FromSeconds(GetSpineDuration(_003Cinstance_003E5__3)), false, (PlayerLoopTiming)8, ct);
									val = ((UniTask)(ref val3)).SuppressCancellationThrow().GetAwaiter();
									if (!val.IsCompleted)
									{
										num = (_003C_003E1__state = 2);
										_003C_003Eu__2 = val;
										((AsyncUniTaskVoidMethodBuilder)(ref _003C_003Et__builder)).AwaitUnsafeOnCompleted<Awaiter<bool>, _003CPlayAsync_003Ed__18>(ref val, ref this);
										return;
									}
									break;
								}
								GameApp.Pool.Recycle(_003Cinstance_003E5__3);
							}
						}
						goto end_IL_001a;
						IL_00ed:
						if (!ct.IsCancellationRequested && !characterExpressionEffectPlayer.disposed)
						{
							val3 = UniTask.Yield((PlayerLoopTiming)13, ct);
							val = ((UniTask)(ref val3)).SuppressCancellationThrow().GetAwaiter();
							if (!val.IsCompleted)
							{
								num = (_003C_003E1__state = 1);
								_003C_003Eu__2 = val;
								((AsyncUniTaskVoidMethodBuilder)(ref _003C_003Et__builder)).AwaitUnsafeOnCompleted<Awaiter<bool>, _003CPlayAsync_003Ed__18>(ref val, ref this);
								return;
							}
							goto IL_0172;
						}
						goto end_IL_001a;
						IL_00b2:
						result = val2.GetResult();
						_003Cprefab_003E5__2 = result;
						if (!((Object)(object)_003Cprefab_003E5__2 == (Object)null))
						{
							characterExpressionEffectPlayer.effectPrefabs[effectLocation] = _003Cprefab_003E5__2;
							goto IL_00ed;
						}
						goto end_IL_001a;
					}
					bool result2 = val.GetResult();
					if ((Object)(object)_003Cinstance_003E5__3 != (Object)null)
					{
						UntrackFrontLayerInstance(_003Cinstance_003E5__3);
						if (!result2)
						{
							GameApp.Pool.Recycle(_003Cinstance_003E5__3);
						}
					}
					_003Cprefab_003E5__2 = null;
					_003Cinstance_003E5__3 = null;
					end_IL_001a:;
				}
				catch (OperationCanceledException)
				{
				}
				catch (Exception ex2)
				{
					Log.LogError("[CharacterExpression][EffectPlayer] ", "表情包播放失败 location={0} error={1}", new object[2] { effectLocation, ex2 });
				}
				finally
				{
					if (num < 0)
					{
						characterExpressionEffectPlayer.isPlaying = false;
						characterExpressionEffectPlayer.FlushQueuedRequest();
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

	private const string TAG = "[CharacterExpression][EffectPlayer] ";

	private const string EffectResGroup = "UnlockRing_Effect";

	private const string AnimAppear = "appear";

	private const float EffectZOffset = -0.02f;

	private const float EmojiDisplayScale = 0.85f;

	private const float EffectDurationFallback = 2f;

	private readonly CancellationToken cancellationToken;

	private readonly Dictionary<string, GameObject> effectPrefabs = new Dictionary<string, GameObject>();

	private static readonly List<GameObject> frontLayerInstances = new List<GameObject>(4);

	private bool disposed;

	private bool isPlaying;

	private PendingPlayRequest? queuedRequest;

	public static void BringActiveInstancesToFront(RectTransform mountParent)
	{
		if ((Object)(object)mountParent == (Object)null)
		{
			return;
		}
		PruneFrontLayerInstances();
		for (int i = 0; i < frontLayerInstances.Count; i++)
		{
			GameObject val = frontLayerInstances[i];
			if ((Object)(object)val != (Object)null && val.activeInHierarchy && (Object)(object)val.transform.parent == (Object)(object)mountParent)
			{
				val.transform.SetAsLastSibling();
			}
		}
	}

	public CharacterExpressionEffectPlayer(CancellationToken cancellationToken)
	{
		this.cancellationToken = cancellationToken;
	}

	public void Dispose()
	{
		disposed = true;
		queuedRequest = null;
		effectPrefabs.Clear();
	}

	public void ClearPending()
	{
		queuedRequest = null;
	}

	public void Play(string effectLocation, Vector3 worldPosition)
	{
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		if (!disposed && !cancellationToken.IsCancellationRequested && !string.IsNullOrEmpty(effectLocation))
		{
			if (isPlaying)
			{
				queuedRequest = new PendingPlayRequest(effectLocation, worldPosition);
				return;
			}
			UniTaskVoid val = PlayAsync(effectLocation, worldPosition, cancellationToken);
			((UniTaskVoid)(ref val)).Forget();
		}
	}

	[AsyncStateMachine(typeof(_003CPlayAsync_003Ed__18))]
	private UniTaskVoid PlayAsync(string effectLocation, Vector3 worldPosition, CancellationToken ct)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		_003CPlayAsync_003Ed__18 _003CPlayAsync_003Ed__ = default(_003CPlayAsync_003Ed__18);
		_003CPlayAsync_003Ed__._003C_003Et__builder = AsyncUniTaskVoidMethodBuilder.Create();
		_003CPlayAsync_003Ed__._003C_003E4__this = this;
		_003CPlayAsync_003Ed__.effectLocation = effectLocation;
		_003CPlayAsync_003Ed__.worldPosition = worldPosition;
		_003CPlayAsync_003Ed__.ct = ct;
		_003CPlayAsync_003Ed__._003C_003E1__state = -1;
		((AsyncUniTaskVoidMethodBuilder)(ref _003CPlayAsync_003Ed__._003C_003Et__builder)).Start<_003CPlayAsync_003Ed__18>(ref _003CPlayAsync_003Ed__);
		return ((AsyncUniTaskVoidMethodBuilder)(ref _003CPlayAsync_003Ed__._003C_003Et__builder)).Task;
	}

	private void FlushQueuedRequest()
	{
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		if (!disposed && queuedRequest.HasValue)
		{
			PendingPlayRequest value = queuedRequest.Value;
			queuedRequest = null;
			Play(value.EffectLocation, value.WorldPosition);
		}
	}

	private static void ResetAndPlayAppear(GameObject instance)
	{
		SkeletonGraphic componentInChildren = instance.GetComponentInChildren<SkeletonGraphic>(true);
		if (!((Object)(object)componentInChildren == (Object)null))
		{
			componentInChildren.startingLoop = false;
			componentInChildren.startingAnimation = string.Empty;
			componentInChildren.AnimationState.ClearTracks();
			componentInChildren.AnimationState.SetAnimation(0, "appear", false);
		}
	}

	private static float GetSpineDuration(GameObject instance)
	{
		SkeletonGraphic componentInChildren = instance.GetComponentInChildren<SkeletonGraphic>(true);
		if ((Object)(object)componentInChildren == (Object)null)
		{
			return 2f;
		}
		TrackEntry current = componentInChildren.AnimationState.GetCurrent(0);
		if (((current != null) ? current.Animation : null) == null)
		{
			return 2f;
		}
		if (!(current.Animation.Duration > 0f))
		{
			return 2f;
		}
		return current.Animation.Duration;
	}

	private static void UntrackFrontLayerInstance(GameObject instance)
	{
		frontLayerInstances.Remove(instance);
	}

	private static void TrackFrontLayerInstance(GameObject instance)
	{
		PruneFrontLayerInstances();
		if ((Object)(object)instance != (Object)null && !frontLayerInstances.Contains(instance))
		{
			frontLayerInstances.Add(instance);
		}
	}

	private static void PruneFrontLayerInstances()
	{
		for (int num = frontLayerInstances.Count - 1; num >= 0; num--)
		{
			if ((Object)(object)frontLayerInstances[num] == (Object)null)
			{
				frontLayerInstances.RemoveAt(num);
			}
		}
	}

	private static bool TrySetUiPosition(Transform effectTransform, Vector3 worldPosition, RectTransform uiParent, Vector3 prefabScale)
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		RectTransform val = (RectTransform)(object)((effectTransform is RectTransform) ? effectTransform : null);
		if (val == null)
		{
			return false;
		}
		return UIGameEncouragementLayout.TryApplyEmojiAlignedToEncouragementLeft(val, uiParent, worldPosition, prefabScale, 0.85f, UIGameEncouragementLayout.DefaultEncouragementClampHalfSize, -0.02f);
	}
}
