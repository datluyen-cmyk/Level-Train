using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.CompilerServices;
using Game.DataTrack;
using Lean.Touch;
using RotateRings.GamePlay.Events;
using RotateRings.GamePlay.MultiTouchGuide;
using UnityEngine;

namespace RotateRings.GamePlay.Services;

public sealed class RotateCameraGestureController : GameplayServiceBase<GameplayManager>
{
	private enum GestureMode
	{
		Undetermined,
		Zoom,
		Pan
	}

	[StructLayout(LayoutKind.Auto)]
	[CompilerGenerated]
	private struct _003CFollowLoopAsync_003Ed__76 : IAsyncStateMachine
	{
		public int _003C_003E1__state;

		public AsyncUniTaskVoidMethodBuilder _003C_003Et__builder;

		public CancellationToken token;

		public Camera camera;

		public RotateCameraGestureController _003C_003E4__this;

		private Awaiter<bool> _003C_003Eu__1;

		private void MoveNext()
		{
			//IL_005c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0061: Unknown result type (might be due to invalid IL or missing references)
			//IL_0068: Unknown result type (might be due to invalid IL or missing references)
			//IL_0018: Unknown result type (might be due to invalid IL or missing references)
			//IL_001d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0020: Unknown result type (might be due to invalid IL or missing references)
			//IL_0025: Unknown result type (might be due to invalid IL or missing references)
			//IL_0029: Unknown result type (might be due to invalid IL or missing references)
			//IL_002e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0042: Unknown result type (might be due to invalid IL or missing references)
			//IL_0043: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d6: Unknown result type (might be due to invalid IL or missing references)
			int num = _003C_003E1__state;
			RotateCameraGestureController rotateCameraGestureController = _003C_003E4__this;
			try
			{
				if (num != 0)
				{
					goto IL_0011;
				}
				Awaiter<bool> val = _003C_003Eu__1;
				_003C_003Eu__1 = default(Awaiter<bool>);
				num = (_003C_003E1__state = -1);
				goto IL_0077;
				IL_0077:
				if (!val.GetResult())
				{
					if ((Object)(object)camera == (Object)null)
					{
						rotateCameraGestureController.CancelFollow();
					}
					else
					{
						rotateCameraGestureController.StepFollow(camera);
						if (rotateCameraGestureController.isGesturing || !rotateCameraGestureController.IsFollowConverged(camera))
						{
							goto IL_0011;
						}
						rotateCameraGestureController.ApplyCameraState(camera, rotateCameraGestureController.targetOrthoSize, rotateCameraGestureController.targetPosition);
						rotateCameraGestureController.CancelFollow();
						rotateCameraGestureController.TryStartSnapBack();
					}
				}
				goto end_IL_000e;
				IL_0011:
				UniTask val2 = UniTask.Yield((PlayerLoopTiming)8, token);
				val = ((UniTask)(ref val2)).SuppressCancellationThrow().GetAwaiter();
				if (!val.IsCompleted)
				{
					num = (_003C_003E1__state = 0);
					_003C_003Eu__1 = val;
					((AsyncUniTaskVoidMethodBuilder)(ref _003C_003Et__builder)).AwaitUnsafeOnCompleted<Awaiter<bool>, _003CFollowLoopAsync_003Ed__76>(ref val, ref this);
					return;
				}
				goto IL_0077;
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
	private struct _003CRestoreInitialFramingAsync_003Ed__88 : IAsyncStateMachine
	{
		public int _003C_003E1__state;

		public AsyncUniTaskVoidMethodBuilder _003C_003Et__builder;

		public Camera camera;

		public RotateCameraGestureController _003C_003E4__this;

		public CancellationToken token;

		private float _003CfromSize_003E5__2;

		private float _003CtoSize_003E5__3;

		private Vector3 _003CfromPosition_003E5__4;

		private Vector3 _003CtoPosition_003E5__5;

		private float _003Celapsed_003E5__6;

		private Awaiter _003C_003Eu__1;

		private void MoveNext()
		{
			//IL_00d2: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d7: Unknown result type (might be due to invalid IL or missing references)
			//IL_00df: Unknown result type (might be due to invalid IL or missing references)
			//IL_0042: Unknown result type (might be due to invalid IL or missing references)
			//IL_0047: Unknown result type (might be due to invalid IL or missing references)
			//IL_0053: Unknown result type (might be due to invalid IL or missing references)
			//IL_0058: Unknown result type (might be due to invalid IL or missing references)
			//IL_0178: Unknown result type (might be due to invalid IL or missing references)
			//IL_017e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0184: Unknown result type (might be due to invalid IL or missing references)
			//IL_0094: Unknown result type (might be due to invalid IL or missing references)
			//IL_0099: Unknown result type (might be due to invalid IL or missing references)
			//IL_009d: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
			//IL_01e4: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
			int num = _003C_003E1__state;
			RotateCameraGestureController rotateCameraGestureController = _003C_003E4__this;
			try
			{
				if (num != 0)
				{
					_003CfromSize_003E5__2 = camera.orthographicSize;
					_003CtoSize_003E5__3 = rotateCameraGestureController.adaptation.CachedOrthographicSize;
					_003CfromPosition_003E5__4 = ((Component)camera).transform.position;
					_003CtoPosition_003E5__5 = rotateCameraGestureController.adaptation.CachedCameraPosition;
					_003Celapsed_003E5__6 = 0f;
					goto IL_019a;
				}
				Awaiter val = _003C_003Eu__1;
				_003C_003Eu__1 = default(Awaiter);
				num = (_003C_003E1__state = -1);
				goto IL_00ee;
				IL_00ee:
				((Awaiter)(ref val)).GetResult();
				if (!token.IsCancellationRequested && !((Object)(object)camera == (Object)null))
				{
					_003Celapsed_003E5__6 += Time.deltaTime;
					float num2 = Mathf.Clamp01(_003Celapsed_003E5__6 / SnapBackDuration);
					float num3 = 1f - (1f - num2) * (1f - num2);
					camera.orthographicSize = Mathf.LerpUnclamped(_003CfromSize_003E5__2, _003CtoSize_003E5__3, num3);
					((Component)camera).transform.position = Vector3.LerpUnclamped(_003CfromPosition_003E5__4, _003CtoPosition_003E5__5, num3);
					rotateCameraGestureController.BroadcastCameraChanged(camera);
					goto IL_019a;
				}
				goto end_IL_000e;
				IL_019a:
				if (_003Celapsed_003E5__6 < SnapBackDuration)
				{
					if (!token.IsCancellationRequested && !((Object)(object)camera == (Object)null))
					{
						UniTask val2 = UniTask.Yield((PlayerLoopTiming)8, token);
						val = ((UniTask)(ref val2)).GetAwaiter();
						if (!((Awaiter)(ref val)).IsCompleted)
						{
							num = (_003C_003E1__state = 0);
							_003C_003Eu__1 = val;
							((AsyncUniTaskVoidMethodBuilder)(ref _003C_003Et__builder)).AwaitUnsafeOnCompleted<Awaiter, _003CRestoreInitialFramingAsync_003Ed__88>(ref val, ref this);
							return;
						}
						goto IL_00ee;
					}
				}
				else if (!token.IsCancellationRequested && !((Object)(object)camera == (Object)null))
				{
					camera.orthographicSize = _003CtoSize_003E5__3;
					((Component)camera).transform.position = _003CtoPosition_003E5__5;
					rotateCameraGestureController.BroadcastCameraChanged(camera);
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
	private struct _003CSnapBackAsync_003Ed__87 : IAsyncStateMachine
	{
		public int _003C_003E1__state;

		public AsyncUniTaskVoidMethodBuilder _003C_003Et__builder;

		public Camera camera;

		public CancellationToken token;

		public float fromSize;

		public float toSize;

		public RotateCameraGestureController _003C_003E4__this;

		private float _003Celapsed_003E5__2;

		private Vector3 _003CstartPosition_003E5__3;

		private Awaiter _003C_003Eu__1;

		private void MoveNext()
		{
			//IL_009f: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
			//IL_002b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0030: Unknown result type (might be due to invalid IL or missing references)
			//IL_014c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0151: Unknown result type (might be due to invalid IL or missing references)
			//IL_0061: Unknown result type (might be due to invalid IL or missing references)
			//IL_0066: Unknown result type (might be due to invalid IL or missing references)
			//IL_006a: Unknown result type (might be due to invalid IL or missing references)
			//IL_006f: Unknown result type (might be due to invalid IL or missing references)
			//IL_01b8: Unknown result type (might be due to invalid IL or missing references)
			//IL_01bd: Unknown result type (might be due to invalid IL or missing references)
			//IL_0084: Unknown result type (might be due to invalid IL or missing references)
			//IL_0086: Unknown result type (might be due to invalid IL or missing references)
			int num = _003C_003E1__state;
			RotateCameraGestureController rotateCameraGestureController = _003C_003E4__this;
			try
			{
				if (num != 0)
				{
					_003Celapsed_003E5__2 = 0f;
					_003CstartPosition_003E5__3 = ((Component)camera).transform.position;
					goto IL_0167;
				}
				Awaiter val = _003C_003Eu__1;
				_003C_003Eu__1 = default(Awaiter);
				num = (_003C_003E1__state = -1);
				goto IL_00bb;
				IL_00bb:
				((Awaiter)(ref val)).GetResult();
				if (!token.IsCancellationRequested && !((Object)(object)camera == (Object)null))
				{
					_003Celapsed_003E5__2 += Time.deltaTime;
					float num2 = Mathf.Clamp01(_003Celapsed_003E5__2 / SnapBackDuration);
					float num3 = 1f - (1f - num2) * (1f - num2);
					camera.orthographicSize = Mathf.LerpUnclamped(fromSize, toSize, num3);
					((Component)camera).transform.position = rotateCameraGestureController.ClampPosition(camera, _003CstartPosition_003E5__3);
					rotateCameraGestureController.BroadcastCameraChanged(camera);
					goto IL_0167;
				}
				goto end_IL_000e;
				IL_0167:
				if (_003Celapsed_003E5__2 < SnapBackDuration)
				{
					if (!token.IsCancellationRequested && !((Object)(object)camera == (Object)null))
					{
						UniTask val2 = UniTask.Yield((PlayerLoopTiming)8, token);
						val = ((UniTask)(ref val2)).GetAwaiter();
						if (!((Awaiter)(ref val)).IsCompleted)
						{
							num = (_003C_003E1__state = 0);
							_003C_003Eu__1 = val;
							((AsyncUniTaskVoidMethodBuilder)(ref _003C_003Et__builder)).AwaitUnsafeOnCompleted<Awaiter, _003CSnapBackAsync_003Ed__87>(ref val, ref this);
							return;
						}
						goto IL_00bb;
					}
				}
				else if (!token.IsCancellationRequested && !((Object)(object)camera == (Object)null))
				{
					camera.orthographicSize = toSize;
					((Component)camera).transform.position = rotateCameraGestureController.ClampPosition(camera, _003CstartPosition_003E5__3);
					rotateCameraGestureController.BroadcastCameraChanged(camera);
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

	private const string TAG = "[Input][CameraGesture] ";

	public static float MinSize = 15f;

	public static float MinSizeOut = 0.5f;

	public static float MaxSizeOut = 0.5f;

	public static float SnapBackDuration = 0.1f;

	public static float PanMarginFactor = 0.1f;

	public static float GestureEnableInitialSizeThreshold = 15f;

	public static int CameraZoomAndPanOverride = -1;

	private const float MinPinchDistance = 1f;

	private const float StandardDpi = 160f;

	public static float PanSensitivity = 1f;

	public static float ZoomSensitivity = 1f;

	public static float GestureFollowSmoothTime = 0.06f;

	private const float FollowSizeEpsilon = 0.001f;

	private const float FollowPositionSqrEpsilon = 1E-06f;

	public static bool ZoomEnabled = false;

	public static bool PanEnabled = false;

	public static float PinchZoomThresholdDp = 10f;

	public static float PanThresholdDp = 8f;

	private RotateCameraAdaptation adaptation;

	private const float FallbackMaxSize = 15f;

	private bool isGesturing;

	private int fingerIdA = -1;

	private int fingerIdB = -1;

	private float lastPinchDistance;

	private Vector2 lastMidpoint;

	private CancellationTokenSource snapBackCts;

	private GestureMode currentMode;

	private float startPinchDistance;

	private Vector2 startMidpoint;

	private Vector2 zoomLockedAnchor;

	private MultiTouchGuideStepPolicy? activeGuidePolicy;

	private float guideGestureStartOrthoSize;

	private float guidePeakZoomDelta;

	private float guideLatestZoomTargetOrthoSize;

	private float guidePeakPanDeltaPx;

	private float targetOrthoSize;

	private Vector3 targetPosition;

	private float smoothOrthoVelocity;

	private Vector3 smoothPosVelocity;

	private CancellationTokenSource followCts;

	private static readonly GameCameraAdaptationEndEvent sharedCameraEvent = new GameCameraAdaptationEndEvent();

	private static readonly CameraControlChangedEvent sharedCameraControlChangedEvent = new CameraControlChangedEvent();

	private float MaxSize
	{
		get
		{
			if (adaptation == null)
			{
				return 15f;
			}
			return adaptation.CachedOrthographicSize;
		}
	}

	public bool IsGuideModeActive => activeGuidePolicy.HasValue;

	private bool EffectiveZoomEnabled
	{
		get
		{
			if (ZoomEnabled)
			{
				if (activeGuidePolicy.HasValue)
				{
					return activeGuidePolicy.Value.AllowZoom;
				}
				return true;
			}
			return false;
		}
	}

	private bool EffectivePanEnabled
	{
		get
		{
			if (PanEnabled)
			{
				if (activeGuidePolicy.HasValue)
				{
					return activeGuidePolicy.Value.AllowPan;
				}
				return true;
			}
			return false;
		}
	}

	public void SetGuideStepPolicy(MultiTouchGuideStepPolicy policy)
	{
		activeGuidePolicy = policy;
	}

	public void ClearGuideStepPolicy()
	{
		activeGuidePolicy = null;
		guidePeakZoomDelta = 0f;
		guideLatestZoomTargetOrthoSize = 0f;
		guidePeakPanDeltaPx = 0f;
	}

	private bool IsInitialSizeEligibleForGesture()
	{
		if (TryResolveAdaptation())
		{
			return adaptation.CachedOrthographicSize > GestureEnableInitialSizeThreshold;
		}
		return false;
	}

	public void RefreshGestureEnabledState()
	{
		PanEnabled = (ZoomEnabled = ((CameraZoomAndPanOverride >= 0) ? CameraZoomAndPanOverride : RingRotationFeelResolver.Resolve().CameraZoomAndPan) == 1 && IsInitialSizeEligibleForGesture());
	}

	public void OnLegacySessionTrackBegan()
	{
		float num = GetTrackedOrthographicSize();
		if (num <= 0f && TryResolveAdaptation())
		{
			num = adaptation.CachedOrthographicSize;
		}
		if (!(num <= 0f))
		{
			RRDataTrack.Instance.LegacySessionCollector.SetBoardZoomInitial(num);
		}
	}

	public void OnLegacySessionTrackEnding()
	{
		float trackedOrthographicSize = GetTrackedOrthographicSize();
		if (!(trackedOrthographicSize <= 0f))
		{
			RRDataTrack.Instance.LegacySessionCollector.FinalizeBoardZoomEnd(trackedOrthographicSize);
		}
	}

	private float GetTrackedOrthographicSize()
	{
		Camera gameplayCam = base.Host.GameplayCam;
		if ((Object)(object)gameplayCam == (Object)null)
		{
			return 0f;
		}
		if (followCts == null)
		{
			return gameplayCam.orthographicSize;
		}
		return targetOrthoSize;
	}

	private void RecordBoardZoomPinchEndedForTrack()
	{
		float trackedOrthographicSize = GetTrackedOrthographicSize();
		if (!(trackedOrthographicSize <= 0f) && !(guideGestureStartOrthoSize <= 0f))
		{
			RRDataTrack.Instance.LegacySessionCollector.RecordBoardZoomPinchEnded(guideGestureStartOrthoSize, trackedOrthographicSize);
		}
	}

	public RotateCameraGestureController(GameplayManager host)
		: base(host)
	{
		LeanTouch.OnFingerDown += HandleFingerDown;
		LeanTouch.OnFingerUp += HandleFingerUp;
		LeanTouch.OnFingerUpdate += HandleFingerUpdate;
	}

	public override void Dispose()
	{
		if (!base.IsDisposed)
		{
			LeanTouch.OnFingerDown -= HandleFingerDown;
			LeanTouch.OnFingerUp -= HandleFingerUp;
			LeanTouch.OnFingerUpdate -= HandleFingerUpdate;
			ResetGesture();
			CancelFollow();
			CancelSnapBack();
			base.Dispose();
		}
	}

	public override void ClearLevel()
	{
		ClearGuideStepPolicy();
		ResetGesture();
		CancelFollow();
		CancelSnapBack();
		ZoomEnabled = false;
		PanEnabled = false;
	}

	public bool ReleaseCameraControl()
	{
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		ResetGesture();
		CancelFollow();
		CancelSnapBack();
		Camera gameplayCam = base.Host.GameplayCam;
		if ((Object)(object)gameplayCam == (Object)null || !TryResolveAdaptation())
		{
			return false;
		}
		if (adaptation.CachedOrthographicSize <= 0f)
		{
			return false;
		}
		snapBackCts = new CancellationTokenSource();
		RestoreInitialFramingAsync(gameplayCam, snapBackCts.Token);
		return true;
	}

	private void HandleFingerDown(LeanFinger finger)
	{
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		if (IsPropUseActive() || !IsUsableFinger(finger) || isGesturing || !TryGetTwoActiveFingers(out var first, out var second))
		{
			return;
		}
		Camera gameplayCam = base.Host.GameplayCam;
		if (!((Object)(object)gameplayCam == (Object)null) && TryResolveAdaptation() && (EffectiveZoomEnabled || EffectivePanEnabled))
		{
			fingerIdA = first.Index;
			fingerIdB = second.Index;
			lastPinchDistance = Vector2.Distance(first.ScreenPosition, second.ScreenPosition);
			lastMidpoint = (first.ScreenPosition + second.ScreenPosition) * 0.5f;
			startPinchDistance = lastPinchDistance;
			startMidpoint = lastMidpoint;
			currentMode = GestureMode.Undetermined;
			isGesturing = true;
			guidePeakZoomDelta = 0f;
			guideLatestZoomTargetOrthoSize = 0f;
			guidePeakPanDeltaPx = 0f;
			if ((Object)(object)gameplayCam != (Object)null)
			{
				guideGestureStartOrthoSize = ((followCts != null) ? targetOrthoSize : gameplayCam.orthographicSize);
			}
			CancelSnapBack();
		}
	}

	private void HandleFingerUp(LeanFinger finger)
	{
		if (isGesturing && (finger.Index == fingerIdA || finger.Index == fingerIdB))
		{
			if (currentMode == GestureMode.Zoom)
			{
				RecordBoardZoomPinchEndedForTrack();
			}
			ResetGesture();
			if (followCts == null)
			{
				TryStartSnapBack();
			}
		}
	}

	private void HandleFingerUpdate(LeanFinger finger)
	{
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_0167: Unknown result type (might be due to invalid IL or missing references)
		//IL_0169: Unknown result type (might be due to invalid IL or missing references)
		//IL_017d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0182: Unknown result type (might be due to invalid IL or missing references)
		//IL_0188: Unknown result type (might be due to invalid IL or missing references)
		//IL_018a: Unknown result type (might be due to invalid IL or missing references)
		if (!isGesturing)
		{
			return;
		}
		if (IsPropUseActive())
		{
			ResetGesture();
			CancelFollow();
			TryStartSnapBack();
		}
		else
		{
			if ((finger.Index != fingerIdA && finger.Index != fingerIdB) || !TryGetFingerByIndex(fingerIdA, out var finger2) || !TryGetFingerByIndex(fingerIdB, out var finger3))
			{
				return;
			}
			Camera gameplayCam = base.Host.GameplayCam;
			if ((Object)(object)gameplayCam == (Object)null || !TryResolveAdaptation())
			{
				ResetGesture();
				CancelFollow();
				return;
			}
			float num = Vector2.Distance(finger2.ScreenPosition, finger3.ScreenPosition);
			Vector2 val = (finger2.ScreenPosition + finger3.ScreenPosition) * 0.5f;
			if (currentMode == GestureMode.Undetermined)
			{
				float num2 = Mathf.Abs(num - startPinchDistance);
				float num3 = Vector2.Distance(val, startMidpoint);
				float num4 = DpToPixels(PinchZoomThresholdDp);
				float num5 = DpToPixels(PanThresholdDp);
				bool flag = EffectiveZoomEnabled && num2 >= num4;
				bool flag2 = EffectivePanEnabled && num3 >= num5;
				if (flag && flag2)
				{
					currentMode = ((num2 / num4 >= num3 / num5) ? GestureMode.Zoom : GestureMode.Pan);
				}
				else if (flag)
				{
					currentMode = GestureMode.Zoom;
				}
				else
				{
					if (!flag2)
					{
						lastPinchDistance = num;
						lastMidpoint = val;
						return;
					}
					currentMode = GestureMode.Pan;
				}
				lastPinchDistance = startPinchDistance;
				lastMidpoint = startMidpoint;
				zoomLockedAnchor = val;
				BeginFollow(gameplayCam);
			}
			switch (currentMode)
			{
			case GestureMode.Zoom:
				ApplyPinchZoom(gameplayCam, finger2, finger3);
				break;
			case GestureMode.Pan:
				ApplyTwoFingerPan(gameplayCam, finger2, finger3);
				break;
			}
			UpdateGuideGestureMetrics(val);
		}
	}

	private void UpdateGuideGestureMetrics(Vector2 currentMidpointPx)
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		if (activeGuidePolicy.HasValue)
		{
			float num = Vector2.Distance(currentMidpointPx, startMidpoint);
			if (num > guidePeakPanDeltaPx)
			{
				guidePeakPanDeltaPx = num;
			}
			TryCompleteGuideGesture();
		}
	}

	private void TryCompleteGuideGesture()
	{
		if (activeGuidePolicy.HasValue)
		{
			MultiTouchGuideStepPolicy value = activeGuidePolicy.Value;
			bool flag = false;
			switch (value.ExpectedStep)
			{
			case MultiTouchGuideStep.ZoomIn:
			{
				float num2 = ResolveGuideZoomStepThreshold(MultiTouchGuideStep.ZoomIn);
				flag = currentMode == GestureMode.Zoom && guidePeakZoomDelta >= num2 && guideLatestZoomTargetOrthoSize < guideGestureStartOrthoSize - Mathf.Epsilon;
				break;
			}
			case MultiTouchGuideStep.ZoomOut:
			{
				float num = ResolveGuideZoomStepThreshold(MultiTouchGuideStep.ZoomOut);
				flag = currentMode == GestureMode.Zoom && guidePeakZoomDelta >= num && guideLatestZoomTargetOrthoSize > guideGestureStartOrthoSize + Mathf.Epsilon;
				break;
			}
			case MultiTouchGuideStep.Pan:
				flag = currentMode == GestureMode.Pan && guidePeakPanDeltaPx >= MultiTouchGuideConfig.PanStepThresholdPixels;
				break;
			}
			if (flag)
			{
				activeGuidePolicy = null;
				GameApp.Event.Send<MultiTouchGuideGestureSucceededEvent>(new MultiTouchGuideGestureSucceededEvent
				{
					Step = (int)value.ExpectedStep
				});
			}
		}
	}

	private float ResolveGuideZoomStepThreshold(MultiTouchGuideStep step)
	{
		float zoomStepThreshold = MultiTouchGuideConfig.ZoomStepThreshold;
		float num = guideGestureStartOrthoSize;
		float num2 = step switch
		{
			MultiTouchGuideStep.ZoomIn => Mathf.Max(0f, num - (MinSize - MinSizeOut)), 
			MultiTouchGuideStep.ZoomOut => Mathf.Max(0f, MaxSize + MaxSizeOut - num), 
			_ => zoomStepThreshold, 
		};
		if (num2 <= Mathf.Epsilon)
		{
			return zoomStepThreshold;
		}
		return Mathf.Min(zoomStepThreshold, num2);
	}

	private void UpdateGuideZoomMetrics(float targetOrthoSize)
	{
		if (activeGuidePolicy.HasValue && !(guideGestureStartOrthoSize <= Mathf.Epsilon))
		{
			guideLatestZoomTargetOrthoSize = targetOrthoSize;
			float num = Mathf.Abs(targetOrthoSize - guideGestureStartOrthoSize);
			if (num > guidePeakZoomDelta)
			{
				guidePeakZoomDelta = num;
			}
		}
	}

	private void ApplyPinchZoom(Camera camera, LeanFinger first, LeanFinger second)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
		float num = Vector2.Distance(first.ScreenPosition, second.ScreenPosition);
		if (num < 1f || lastPinchDistance < 1f)
		{
			lastPinchDistance = num;
			return;
		}
		float num2 = lastPinchDistance / num;
		if (!Mathf.Approximately(num2, 1f))
		{
			num2 = 1f + (num2 - 1f) * ZoomSensitivity;
			float num3 = targetOrthoSize;
			float num4 = MinSize - MinSizeOut;
			float num5 = MaxSize + MaxSizeOut;
			float num6 = Mathf.Clamp(num3 * num2, num4, num5);
			if (Mathf.Approximately(num6, num3))
			{
				lastPinchDistance = num;
				return;
			}
			UpdateGuideZoomMetrics(num6);
			targetPosition = ComputeZoomTargetPosition(camera, num3, targetPosition, num6, zoomLockedAnchor);
			targetOrthoSize = num6;
			CommitGestureTarget(camera);
			lastPinchDistance = num;
		}
	}

	private void ApplyTwoFingerPan(Camera camera, LeanFinger first, LeanFinger second)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		Vector2 val = (first.ScreenPosition + second.ScreenPosition) * 0.5f;
		Vector2 val2 = val - lastMidpoint;
		if (((Vector2)(ref val2)).sqrMagnitude < Mathf.Epsilon)
		{
			lastMidpoint = val;
			return;
		}
		float num = 2f * targetOrthoSize / (float)Screen.height * PanSensitivity;
		Vector3 val3 = default(Vector3);
		((Vector3)(ref val3))._002Ector(val2.x * num, val2.y * num, 0f);
		Vector3 val4 = ClampPosition(camera, targetPosition - val3);
		if (val4 == targetPosition)
		{
			lastMidpoint = val;
			return;
		}
		targetPosition = val4;
		CommitGestureTarget(camera);
		lastMidpoint = val;
	}

	private Vector3 ComputeZoomTargetPosition(Camera camera, float fromOrthoSize, Vector3 fromPosition, float toOrthoSize, Vector2 anchorScreen)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		Vector3 val = ScreenToWorldWithOrtho(camera, anchorScreen, fromOrthoSize, fromPosition);
		Vector3 val2 = ScreenToWorldWithOrtho(camera, anchorScreen, toOrthoSize, fromPosition);
		return ClampPosition(camera, fromPosition + (val - val2));
	}

	private Vector3 ScreenToWorldWithOrtho(Camera camera, Vector2 screenPosition, float orthoSize, Vector3 cameraPosition)
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		float orthographicSize = camera.orthographicSize;
		Vector3 position = ((Component)camera).transform.position;
		camera.orthographicSize = orthoSize;
		((Component)camera).transform.position = cameraPosition;
		Vector3 result = ScreenToWorld(camera, screenPosition);
		camera.orthographicSize = orthographicSize;
		((Component)camera).transform.position = position;
		return result;
	}

	private void ApplyCameraState(Camera camera, float? targetOrthoSize, Vector3 targetPosition)
	{
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		if (targetOrthoSize.HasValue)
		{
			camera.orthographicSize = targetOrthoSize.Value;
		}
		((Component)camera).transform.position = targetPosition;
		BroadcastCameraChanged(camera);
	}

	private void BeginFollow(Camera camera)
	{
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		if (followCts == null)
		{
			targetOrthoSize = camera.orthographicSize;
			targetPosition = ((Component)camera).transform.position;
			smoothOrthoVelocity = 0f;
			smoothPosVelocity = Vector3.zero;
			followCts = new CancellationTokenSource();
			UniTaskVoid val = FollowLoopAsync(camera, followCts.Token);
			((UniTaskVoid)(ref val)).Forget();
		}
	}

	private void CommitGestureTarget(Camera camera)
	{
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		if (GestureFollowSmoothTime <= 0f)
		{
			ApplyCameraState(camera, targetOrthoSize, targetPosition);
		}
		NotifyCameraControlChanged();
	}

	[AsyncStateMachine(typeof(_003CFollowLoopAsync_003Ed__76))]
	private UniTaskVoid FollowLoopAsync(Camera camera, CancellationToken token)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		_003CFollowLoopAsync_003Ed__76 _003CFollowLoopAsync_003Ed__ = default(_003CFollowLoopAsync_003Ed__76);
		_003CFollowLoopAsync_003Ed__._003C_003Et__builder = AsyncUniTaskVoidMethodBuilder.Create();
		_003CFollowLoopAsync_003Ed__._003C_003E4__this = this;
		_003CFollowLoopAsync_003Ed__.camera = camera;
		_003CFollowLoopAsync_003Ed__.token = token;
		_003CFollowLoopAsync_003Ed__._003C_003E1__state = -1;
		((AsyncUniTaskVoidMethodBuilder)(ref _003CFollowLoopAsync_003Ed__._003C_003Et__builder)).Start<_003CFollowLoopAsync_003Ed__76>(ref _003CFollowLoopAsync_003Ed__);
		return ((AsyncUniTaskVoidMethodBuilder)(ref _003CFollowLoopAsync_003Ed__._003C_003Et__builder)).Task;
	}

	private void StepFollow(Camera camera)
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		if (camera.orthographicSize != targetOrthoSize || !(((Component)camera).transform.position == targetPosition))
		{
			float gestureFollowSmoothTime = GestureFollowSmoothTime;
			if (gestureFollowSmoothTime <= 0f)
			{
				ApplyCameraState(camera, targetOrthoSize, targetPosition);
				return;
			}
			float value = Mathf.SmoothDamp(camera.orthographicSize, targetOrthoSize, ref smoothOrthoVelocity, gestureFollowSmoothTime);
			Vector3 val = Vector3.SmoothDamp(((Component)camera).transform.position, targetPosition, ref smoothPosVelocity, gestureFollowSmoothTime);
			ApplyCameraState(camera, value, val);
		}
	}

	private bool IsFollowConverged(Camera camera)
	{
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		if (Mathf.Abs(camera.orthographicSize - targetOrthoSize) < 0.001f)
		{
			Vector3 val = ((Component)camera).transform.position - targetPosition;
			return ((Vector3)(ref val)).sqrMagnitude < 1E-06f;
		}
		return false;
	}

	private void CancelFollow()
	{
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		if (followCts != null)
		{
			followCts.Cancel();
			followCts.Dispose();
			followCts = null;
		}
		smoothOrthoVelocity = 0f;
		smoothPosVelocity = Vector3.zero;
	}

	private Vector3 ScreenToWorld(Camera camera, Vector2 screenPosition)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		float num = Mathf.Abs(((Component)camera).transform.position.z);
		Vector3 val = default(Vector3);
		((Vector3)(ref val))._002Ector(screenPosition.x, screenPosition.y, num);
		Vector3 result = camera.ScreenToWorldPoint(val);
		result.z = ((Component)camera).transform.position.z;
		return result;
	}

	private Vector3 ClampPosition(Camera camera, Vector3 targetPosition)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		Rect worldBounds = base.Host.WorldBounds;
		float num = ((Rect)(ref worldBounds)).width * PanMarginFactor;
		float num2 = ((Rect)(ref worldBounds)).height * PanMarginFactor;
		float num3 = ((Rect)(ref worldBounds)).xMin - num;
		float num4 = ((Rect)(ref worldBounds)).xMax + num;
		float num5 = ((Rect)(ref worldBounds)).yMin - num2;
		float num6 = ((Rect)(ref worldBounds)).yMax + num2;
		targetPosition.x = Mathf.Clamp(targetPosition.x, num3, num4);
		targetPosition.y = Mathf.Clamp(targetPosition.y, num5, num6);
		targetPosition.z = ((Component)camera).transform.position.z;
		return targetPosition;
	}

	private void BroadcastCameraChanged(Camera camera)
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		sharedCameraEvent.CameraPosition = ((Component)camera).transform.position;
		sharedCameraEvent.Source = GameCameraFramingSource.UserGesture;
		GameApp.Event.Send<GameCameraAdaptationEndEvent>(sharedCameraEvent);
	}

	private static void NotifyCameraControlChanged()
	{
		GameApp.Event.Send<CameraControlChangedEvent>(sharedCameraControlChangedEvent);
	}

	private void TryStartSnapBack()
	{
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		Camera gameplayCam = base.Host.GameplayCam;
		if (!((Object)(object)gameplayCam == (Object)null))
		{
			float orthographicSize = gameplayCam.orthographicSize;
			bool flag = orthographicSize < MinSize;
			bool flag2 = orthographicSize > MaxSize;
			if (flag || flag2)
			{
				float toSize = (flag ? MinSize : MaxSize);
				CancelSnapBack();
				snapBackCts = new CancellationTokenSource();
				SnapBackAsync(gameplayCam, orthographicSize, toSize, snapBackCts.Token);
			}
		}
	}

	[AsyncStateMachine(typeof(_003CSnapBackAsync_003Ed__87))]
	private UniTaskVoid SnapBackAsync(Camera camera, float fromSize, float toSize, CancellationToken token)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		_003CSnapBackAsync_003Ed__87 _003CSnapBackAsync_003Ed__ = default(_003CSnapBackAsync_003Ed__87);
		_003CSnapBackAsync_003Ed__._003C_003Et__builder = AsyncUniTaskVoidMethodBuilder.Create();
		_003CSnapBackAsync_003Ed__._003C_003E4__this = this;
		_003CSnapBackAsync_003Ed__.camera = camera;
		_003CSnapBackAsync_003Ed__.fromSize = fromSize;
		_003CSnapBackAsync_003Ed__.toSize = toSize;
		_003CSnapBackAsync_003Ed__.token = token;
		_003CSnapBackAsync_003Ed__._003C_003E1__state = -1;
		((AsyncUniTaskVoidMethodBuilder)(ref _003CSnapBackAsync_003Ed__._003C_003Et__builder)).Start<_003CSnapBackAsync_003Ed__87>(ref _003CSnapBackAsync_003Ed__);
		return ((AsyncUniTaskVoidMethodBuilder)(ref _003CSnapBackAsync_003Ed__._003C_003Et__builder)).Task;
	}

	[AsyncStateMachine(typeof(_003CRestoreInitialFramingAsync_003Ed__88))]
	private UniTaskVoid RestoreInitialFramingAsync(Camera camera, CancellationToken token)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		_003CRestoreInitialFramingAsync_003Ed__88 _003CRestoreInitialFramingAsync_003Ed__ = default(_003CRestoreInitialFramingAsync_003Ed__88);
		_003CRestoreInitialFramingAsync_003Ed__._003C_003Et__builder = AsyncUniTaskVoidMethodBuilder.Create();
		_003CRestoreInitialFramingAsync_003Ed__._003C_003E4__this = this;
		_003CRestoreInitialFramingAsync_003Ed__.camera = camera;
		_003CRestoreInitialFramingAsync_003Ed__.token = token;
		_003CRestoreInitialFramingAsync_003Ed__._003C_003E1__state = -1;
		((AsyncUniTaskVoidMethodBuilder)(ref _003CRestoreInitialFramingAsync_003Ed__._003C_003Et__builder)).Start<_003CRestoreInitialFramingAsync_003Ed__88>(ref _003CRestoreInitialFramingAsync_003Ed__);
		return ((AsyncUniTaskVoidMethodBuilder)(ref _003CRestoreInitialFramingAsync_003Ed__._003C_003Et__builder)).Task;
	}

	private void CancelSnapBack()
	{
		if (snapBackCts != null)
		{
			snapBackCts.Cancel();
			snapBackCts.Dispose();
			snapBackCts = null;
		}
	}

	private bool TryResolveAdaptation()
	{
		if (adaptation != null)
		{
			return true;
		}
		adaptation = base.Host.GetGameService<RotateCameraAdaptation>();
		if (adaptation == null)
		{
			return false;
		}
		return true;
	}

	private void ResetGesture()
	{
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		isGesturing = false;
		fingerIdA = -1;
		fingerIdB = -1;
		lastPinchDistance = 0f;
		lastMidpoint = default(Vector2);
		startPinchDistance = 0f;
		startMidpoint = default(Vector2);
		zoomLockedAnchor = default(Vector2);
		currentMode = GestureMode.Undetermined;
	}

	private static float DpToPixels(float dp)
	{
		float num = ((Screen.dpi > 0f) ? Screen.dpi : 160f);
		return dp * num / 160f;
	}

	private bool IsPropUseActive()
	{
		return base.Host.PropUseService?.IsActive ?? false;
	}

	private static bool IsUsableFinger(LeanFinger finger)
	{
		if (finger != null && finger.Set && finger.Index != -42)
		{
			return !finger.StartedOverGui;
		}
		return false;
	}

	private static bool TryGetTwoActiveFingers(out LeanFinger first, out LeanFinger second)
	{
		first = null;
		second = null;
		List<LeanFinger> fingers = LeanTouch.Fingers;
		for (int i = 0; i < fingers.Count; i++)
		{
			LeanFinger val = fingers[i];
			if (IsUsableFinger(val))
			{
				if (first != null)
				{
					second = val;
					return true;
				}
				first = val;
			}
		}
		return false;
	}

	private static bool TryGetFingerByIndex(int index, out LeanFinger finger)
	{
		List<LeanFinger> fingers = LeanTouch.Fingers;
		for (int i = 0; i < fingers.Count; i++)
		{
			LeanFinger val = fingers[i];
			if (val.Index == index && val.Set)
			{
				finger = val;
				return true;
			}
		}
		finger = null;
		return false;
	}
}
