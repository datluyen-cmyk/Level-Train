using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.CompilerServices;
using RotateRings.Common;
using RotateRings.Prop;
using RotateRings.Services;
using RotateRings.UI.Game;
using RotateRings.UI.PropGuide;
using UniLog;
using UnityEngine;
using UnlockRing.Prop;
using Vibration;
using XingYun.UI.Controller;

namespace RotateRings.GamePlay.Services.Prop.Handlers;

public sealed class RotateRingHammerPropHandler : IRotateRingPropUse
{
	[StructLayout(LayoutKind.Auto)]
	[CompilerGenerated]
	private struct _003CPlayHammerEffectThenFinishAsync_003Ed__23 : IAsyncStateMachine
	{
		public int _003C_003E1__state;

		public AsyncUniTaskVoidMethodBuilder _003C_003Et__builder;

		public RotateRingHammerPropHandler _003C_003E4__this;

		public Vector3 worldPos;

		public IGameplayObject rotateTarget;

		private Animation _003Canimation_003E5__2;

		private Awaiter<bool> _003C_003Eu__1;

		private void MoveNext()
		{
			//IL_00e9: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ee: Unknown result type (might be due to invalid IL or missing references)
			//IL_00f6: Unknown result type (might be due to invalid IL or missing references)
			//IL_003d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0081: Unknown result type (might be due to invalid IL or missing references)
			//IL_0086: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ce: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d0: Unknown result type (might be due to invalid IL or missing references)
			int num = _003C_003E1__state;
			RotateRingHammerPropHandler rotateRingHammerPropHandler = _003C_003E4__this;
			try
			{
				CancellationToken token = default(CancellationToken);
				if (num != 0)
				{
					rotateRingHammerPropHandler.CancelHammerAsync();
					rotateRingHammerPropHandler.hammerCts = new CancellationTokenSource();
					token = rotateRingHammerPropHandler.hammerCts.Token;
					rotateRingHammerPropHandler.isHammerExecuting = true;
				}
				try
				{
					Awaiter<bool> val3;
					if (num != 0)
					{
						if (TrySpawnHammerEffect(worldPos, rotateRingHammerPropHandler.callback?.PropGuidePanel, out var instance, out _003Canimation_003E5__2))
						{
							rotateRingHammerPropHandler.hammerEffectInstance = instance;
							VibrateUtility.VibrateContinuous(1103);
							_003Canimation_003E5__2.Play();
						}
						UniTaskVoid val = PlayHammerHitSoundDelayedAsync(token);
						((UniTaskVoid)(ref val)).Forget();
						UniTask val2 = UniTask.Delay(TimeSpan.FromSeconds(UnlockRingPropConfig.Instance.GetHammerBreakDelaySeconds()), false, (PlayerLoopTiming)8, token);
						val3 = ((UniTask)(ref val2)).SuppressCancellationThrow().GetAwaiter();
						if (!val3.IsCompleted)
						{
							num = (_003C_003E1__state = 0);
							_003C_003Eu__1 = val3;
							((AsyncUniTaskVoidMethodBuilder)(ref _003C_003Et__builder)).AwaitUnsafeOnCompleted<Awaiter<bool>, _003CPlayHammerEffectThenFinishAsync_003Ed__23>(ref val3, ref this);
							return;
						}
					}
					else
					{
						val3 = _003C_003Eu__1;
						_003C_003Eu__1 = default(Awaiter<bool>);
						num = (_003C_003E1__state = -1);
					}
					if (!val3.GetResult())
					{
						rotateRingHammerPropHandler.DetachHammerEffectToUIGame(_003Canimation_003E5__2);
						VibrateUtility.StopContinuous();
						VibrateUtility.Vibrate(1103);
						SingletonComponent<GameplayManager>.Instance?.ScreenShake?.RequestShake(ScreenShakeTriggerType.HammerHit);
						if (rotateTarget != null && !rotateRingHammerPropHandler.host.TryExecuteHammerRemove(rotateTarget))
						{
							rotateRingHammerPropHandler.RequestFinish(RotateRingPropUseResult.Failed, consumeOnSuccess: false);
						}
						else
						{
							rotateRingHammerPropHandler.RequestFinish(RotateRingPropUseResult.Success, consumeOnSuccess: true);
							_003Canimation_003E5__2 = null;
						}
					}
				}
				finally
				{
					if (num < 0)
					{
						rotateRingHammerPropHandler.isHammerExecuting = false;
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
	private struct _003CPlayHammerHitSoundDelayedAsync_003Ed__26 : IAsyncStateMachine
	{
		public int _003C_003E1__state;

		public AsyncUniTaskVoidMethodBuilder _003C_003Et__builder;

		public CancellationToken ct;

		private Awaiter<bool> _003C_003Eu__1;

		private void MoveNext()
		{
			//IL_0062: Unknown result type (might be due to invalid IL or missing references)
			//IL_0067: Unknown result type (might be due to invalid IL or missing references)
			//IL_006e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0022: Unknown result type (might be due to invalid IL or missing references)
			//IL_0027: Unknown result type (might be due to invalid IL or missing references)
			//IL_002a: Unknown result type (might be due to invalid IL or missing references)
			//IL_002f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0032: Unknown result type (might be due to invalid IL or missing references)
			//IL_0037: Unknown result type (might be due to invalid IL or missing references)
			//IL_004b: Unknown result type (might be due to invalid IL or missing references)
			//IL_004c: Unknown result type (might be due to invalid IL or missing references)
			int num = _003C_003E1__state;
			try
			{
				Awaiter<bool> val2;
				if (num != 0)
				{
					UniTask val = UniTask.Delay(TimeSpan.FromSeconds(UnlockRingPropConfig.Instance.GetHammerHitSoundDelaySeconds()), false, (PlayerLoopTiming)8, ct);
					val2 = ((UniTask)(ref val)).SuppressCancellationThrow().GetAwaiter();
					if (!val2.IsCompleted)
					{
						num = (_003C_003E1__state = 0);
						_003C_003Eu__1 = val2;
						((AsyncUniTaskVoidMethodBuilder)(ref _003C_003Et__builder)).AwaitUnsafeOnCompleted<Awaiter<bool>, _003CPlayHammerHitSoundDelayedAsync_003Ed__26>(ref val2, ref this);
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
					AudioService.PlayUiSound(AudioKeys.ItemHammerHit.Value);
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
	private struct _003CRecycleEffectAfterDelayAsync_003Ed__27 : IAsyncStateMachine
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
								((AsyncUniTaskVoidMethodBuilder)(ref _003C_003Et__builder)).AwaitUnsafeOnCompleted<Awaiter<bool>, _003CRecycleEffectAfterDelayAsync_003Ed__27>(ref val2, ref this);
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
						Log.LogError("[RotateRingHammerPropHandler] ", "特效回收异常: {0}", new object[1] { ex.Message });
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

	private const string Tag = "[RotateRingHammerPropHandler] ";

	private const string BoomEffectLocation = "eff_chuizi_boom_01";

	private const float HammerAnimationFallbackSeconds = 2f;

	private static GameObject boomEffectPrefab;

	private IRotateRingPropHost host;

	private IRotateRingPropUseCallback callback;

	private bool isFinishing;

	private bool isHammerExecuting;

	private GameObject hammerEffectInstance;

	private CancellationTokenSource hammerCts;

	public EPropType PropType => EPropType.Hammer;

	public bool IsHammerExecuting => isHammerExecuting;

	public bool CanStart(IRotateRingPropHost propHost)
	{
		return propHost?.CanUseHammer() ?? false;
	}

	public void Begin(IRotateRingPropHost propHost, IRotateRingPropUseCallback useCallback)
	{
		host = propHost;
		callback = useCallback;
		isFinishing = false;
		isHammerExecuting = false;
		if ((Object)(object)hammerEffectInstance != (Object)null)
		{
			GameApp.Pool.Recycle(hammerEffectInstance);
			hammerEffectInstance = null;
		}
		CancelHammerAsync();
	}

	public void OnPropGuideOpened()
	{
		SetChainLockReparentToGuide(reparent: true);
	}

	public void OnGuideReady()
	{
		host?.BeginHammerRemoveMode(OnHammerTargetSelected);
	}

	public void OnUseButtonClicked()
	{
	}

	public void OnGuideDismissed(bool cancelled)
	{
		SetChainLockReparentToGuide(reparent: false);
		if (cancelled && !isFinishing && callback != null && !callback.HasPropConsumed)
		{
			RequestFinish(RotateRingPropUseResult.Cancel, consumeOnSuccess: false);
		}
	}

	public void End()
	{
		isHammerExecuting = false;
		SetChainLockReparentToGuide(reparent: false);
		if ((Object)(object)hammerEffectInstance != (Object)null)
		{
			GameApp.Pool.Recycle(hammerEffectInstance);
			hammerEffectInstance = null;
		}
		CancelHammerAsync();
		VibrateUtility.StopContinuous();
		host?.EndHammerRemoveMode();
		host = null;
		callback = null;
	}

	private void SetChainLockReparentToGuide(bool reparent)
	{
		IController val = default(IController);
		if (GameApp.UI.IsOpen("UIGame", ref val) && val is UIGameController { View: { } view })
		{
			IController val2 = default(IController);
			Transform guideRoot;
			RectTransform insertBefore;
			if (!reparent)
			{
				view.SetChainLockReparent(null, null, reparent: false);
			}
			else if (GameApp.UI.IsOpen("UIPropGuide", ref val2) && val2 is UIPropGuideController uIPropGuideController && uIPropGuideController.View.TryGetChainLockAnchor(out guideRoot, out insertBefore))
			{
				view.SetChainLockReparent(guideRoot, insertBefore, reparent: true);
			}
		}
	}

	private void OnHammerTargetSelected(HammerRemoveSelection selection)
	{
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		if (callback != null && !callback.HasPropConsumed && !isFinishing)
		{
			UniTaskVoid val = PlayHammerEffectThenFinishAsync(selection.WorldPosition, selection.Target);
			((UniTaskVoid)(ref val)).Forget();
		}
	}

	[AsyncStateMachine(typeof(_003CPlayHammerEffectThenFinishAsync_003Ed__23))]
	private UniTaskVoid PlayHammerEffectThenFinishAsync(Vector3 worldPos, IGameplayObject rotateTarget)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		_003CPlayHammerEffectThenFinishAsync_003Ed__23 _003CPlayHammerEffectThenFinishAsync_003Ed__ = default(_003CPlayHammerEffectThenFinishAsync_003Ed__23);
		_003CPlayHammerEffectThenFinishAsync_003Ed__._003C_003Et__builder = AsyncUniTaskVoidMethodBuilder.Create();
		_003CPlayHammerEffectThenFinishAsync_003Ed__._003C_003E4__this = this;
		_003CPlayHammerEffectThenFinishAsync_003Ed__.worldPos = worldPos;
		_003CPlayHammerEffectThenFinishAsync_003Ed__.rotateTarget = rotateTarget;
		_003CPlayHammerEffectThenFinishAsync_003Ed__._003C_003E1__state = -1;
		((AsyncUniTaskVoidMethodBuilder)(ref _003CPlayHammerEffectThenFinishAsync_003Ed__._003C_003Et__builder)).Start<_003CPlayHammerEffectThenFinishAsync_003Ed__23>(ref _003CPlayHammerEffectThenFinishAsync_003Ed__);
		return ((AsyncUniTaskVoidMethodBuilder)(ref _003CPlayHammerEffectThenFinishAsync_003Ed__._003C_003Et__builder)).Task;
	}

	private static bool TrySpawnHammerEffect(Vector3 worldPos, RectTransform panel, out GameObject instance, out Animation animation)
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		instance = null;
		animation = null;
		if ((Object)(object)panel == (Object)null)
		{
			return false;
		}
		if (!WorldToUIPostionUtil.WorldToLocalPoint(worldPos, panel, out var localPoint))
		{
			return false;
		}
		if (boomEffectPrefab == null)
		{
			boomEffectPrefab = GameApp.Res.LoadAsset<GameObject>("eff_chuizi_boom_01", (string)null);
		}
		if ((Object)(object)boomEffectPrefab == (Object)null)
		{
			return false;
		}
		instance = GameApp.Pool.Spawn(boomEffectPrefab, (Transform)(object)panel);
		if ((Object)(object)instance == (Object)null)
		{
			return false;
		}
		instance.transform.localPosition = Vector2.op_Implicit(localPoint);
		instance.transform.localRotation = Quaternion.identity;
		instance.transform.localScale = Vector3.one;
		animation = instance.GetComponent<Animation>();
		if ((Object)(object)animation == (Object)null)
		{
			GameApp.Pool.Recycle(instance);
			instance = null;
			return false;
		}
		return true;
	}

	private void DetachHammerEffectToUIGame(Animation animation)
	{
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		IController val = default(IController);
		if ((Object)(object)hammerEffectInstance == (Object)null || !hammerEffectInstance.activeInHierarchy)
		{
			hammerEffectInstance = null;
		}
		else if (GameApp.UI.IsOpen("UIGame", ref val) && val is UIGameController uIGameController)
		{
			RectTransform val2 = uIGameController.View?.CenterContent;
			if (!((Object)(object)val2 == (Object)null))
			{
				hammerEffectInstance.transform.SetParent((Transform)(object)val2, true);
				hammerEffectInstance.transform.SetAsLastSibling();
				float delaySeconds = (((Object)(object)animation != (Object)null) ? GetHammerAnimationDuration(animation) : 2f);
				UniTaskVoid val3 = RecycleEffectAfterDelayAsync(hammerEffectInstance, delaySeconds);
				((UniTaskVoid)(ref val3)).Forget();
				hammerEffectInstance = null;
			}
		}
	}

	[AsyncStateMachine(typeof(_003CPlayHammerHitSoundDelayedAsync_003Ed__26))]
	private static UniTaskVoid PlayHammerHitSoundDelayedAsync(CancellationToken ct)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		_003CPlayHammerHitSoundDelayedAsync_003Ed__26 _003CPlayHammerHitSoundDelayedAsync_003Ed__ = default(_003CPlayHammerHitSoundDelayedAsync_003Ed__26);
		_003CPlayHammerHitSoundDelayedAsync_003Ed__._003C_003Et__builder = AsyncUniTaskVoidMethodBuilder.Create();
		_003CPlayHammerHitSoundDelayedAsync_003Ed__.ct = ct;
		_003CPlayHammerHitSoundDelayedAsync_003Ed__._003C_003E1__state = -1;
		((AsyncUniTaskVoidMethodBuilder)(ref _003CPlayHammerHitSoundDelayedAsync_003Ed__._003C_003Et__builder)).Start<_003CPlayHammerHitSoundDelayedAsync_003Ed__26>(ref _003CPlayHammerHitSoundDelayedAsync_003Ed__);
		return ((AsyncUniTaskVoidMethodBuilder)(ref _003CPlayHammerHitSoundDelayedAsync_003Ed__._003C_003Et__builder)).Task;
	}

	[AsyncStateMachine(typeof(_003CRecycleEffectAfterDelayAsync_003Ed__27))]
	private static UniTaskVoid RecycleEffectAfterDelayAsync(GameObject instance, float delaySeconds)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		_003CRecycleEffectAfterDelayAsync_003Ed__27 _003CRecycleEffectAfterDelayAsync_003Ed__ = default(_003CRecycleEffectAfterDelayAsync_003Ed__27);
		_003CRecycleEffectAfterDelayAsync_003Ed__._003C_003Et__builder = AsyncUniTaskVoidMethodBuilder.Create();
		_003CRecycleEffectAfterDelayAsync_003Ed__.instance = instance;
		_003CRecycleEffectAfterDelayAsync_003Ed__.delaySeconds = delaySeconds;
		_003CRecycleEffectAfterDelayAsync_003Ed__._003C_003E1__state = -1;
		((AsyncUniTaskVoidMethodBuilder)(ref _003CRecycleEffectAfterDelayAsync_003Ed__._003C_003Et__builder)).Start<_003CRecycleEffectAfterDelayAsync_003Ed__27>(ref _003CRecycleEffectAfterDelayAsync_003Ed__);
		return ((AsyncUniTaskVoidMethodBuilder)(ref _003CRecycleEffectAfterDelayAsync_003Ed__._003C_003Et__builder)).Task;
	}

	private static float GetHammerAnimationDuration(Animation animation)
	{
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Expected O, but got Unknown
		if ((Object)(object)animation.clip != (Object)null)
		{
			return animation.clip.length;
		}
		foreach (AnimationState item in animation)
		{
			AnimationState val = item;
			if ((TrackedReference)(object)val != (TrackedReference)null && val.length > 0f)
			{
				return val.length;
			}
		}
		return 2f;
	}

	private void RequestFinish(RotateRingPropUseResult result, bool consumeOnSuccess)
	{
		if (!isFinishing && callback != null)
		{
			isFinishing = true;
			if (result == RotateRingPropUseResult.Success && consumeOnSuccess)
			{
				callback.PrepareFreePropIfNeeded();
			}
			callback.RequestFinish(result, consumeOnSuccess);
		}
	}

	private void CancelHammerAsync()
	{
		hammerCts?.Cancel();
		hammerCts?.Dispose();
		hammerCts = null;
	}
}
