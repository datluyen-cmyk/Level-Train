using System.Collections.Generic;
using Lean.Touch;
using RotateRings.GamePlay.Events;
using RotateRings.GamePlay.Services;
using UnityEngine;

public class GameplayInputDispatcher
{
	private const string TAG = "[Input][GameplayInputDispatcher] ";

	private Camera gameplayCamera;

	private LayerMask inputLayerMask;

	private bool isEnabled;

	private int capturedFingerId = -1;

	private IGameplayPointerHandler capturedHandler;

	private GameplayPointerData lastCapturedPointerData;

	private bool isGameplayInputSuppressedByMultiTouch;

	private bool isGuideSuppressingGameplay;

	private bool isPendingSelection;

	private int pendingFingerId = -1;

	private readonly HashSet<IGameplayPointerHandler> pendingAttemptedHandlers = new HashSet<IGameplayPointerHandler>();

	public void Enable(Camera camera, LayerMask layerMask)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		gameplayCamera = camera;
		inputLayerMask = layerMask;
		isEnabled = true;
		isGameplayInputSuppressedByMultiTouch = false;
		LeanTouch.OnFingerDown -= HandleRawFingerDown;
		LeanTouch.OnFingerDown += HandleRawFingerDown;
		LeanTouch.OnFingerUp -= HandleRawFingerUp;
		LeanTouch.OnFingerUp += HandleRawFingerUp;
	}

	public void Disable(bool notifyCapturedHandler = true)
	{
		LeanTouch.OnFingerDown -= HandleRawFingerDown;
		LeanTouch.OnFingerUp -= HandleRawFingerUp;
		if (capturedHandler != null)
		{
			ReleaseCapture(notifyCapturedHandler);
		}
		ClearPendingSelection();
		isGameplayInputSuppressedByMultiTouch = false;
		isGuideSuppressingGameplay = false;
		isEnabled = false;
		gameplayCamera = null;
	}

	public void SetGuideSuppressingGameplay(bool suppress)
	{
		isGuideSuppressingGameplay = suppress;
		if (suppress)
		{
			ClearCapture();
			ClearPendingSelection();
		}
	}

	public void ClearCapture()
	{
		ReleaseCapture(notifyPointerUp: false);
		ClearPendingSelection();
	}

	private void ReleaseCapture(bool notifyPointerUp)
	{
		if (capturedHandler != null)
		{
			IGameplayPointerHandler gameplayPointerHandler = capturedHandler;
			_ = capturedFingerId;
			GameplayPointerData data = lastCapturedPointerData;
			_ = gameplayPointerHandler.GetType().Name;
			capturedFingerId = -1;
			capturedHandler = null;
			lastCapturedPointerData = default(GameplayPointerData);
			if (notifyPointerUp && IsHandlerAlive(gameplayPointerHandler))
			{
				gameplayPointerHandler.OnPointerUp(data);
			}
			GameApp.Event.Send<GameplayPointerCaptureChangedEvent>(new GameplayPointerCaptureChangedEvent(isCaptured: false, gameplayPointerHandler));
		}
	}

	public void HandleFingerDown(FingerData fingerData)
	{
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		if (!isEnabled || fingerData == null || (Object)(object)gameplayCamera == (Object)null || isGuideSuppressingGameplay || isGameplayInputSuppressedByMultiTouch)
		{
			return;
		}
		if (HasMultiFingerGesture())
		{
			SuppressGameplayInputForMultiTouch();
		}
		else
		{
			if (capturedHandler != null || isPendingSelection)
			{
				return;
			}
			IGameplayPointerHandler gameplayPointerHandler = RaycastHandler(fingerData.ScreenPosition);
			GameApp.Event.Send<GameplayPointerDownEvent>(new GameplayPointerDownEvent(gameplayPointerHandler));
			if (gameplayPointerHandler == null)
			{
				BeginPendingSelection(fingerData);
				return;
			}
			GameplayPointerData pointerData = CreatePointerData(fingerData);
			if (!TryBindHandler(fingerData, gameplayPointerHandler, pointerData, fromPendingMove: false))
			{
				BeginPendingSelection(fingerData, gameplayPointerHandler);
			}
		}
	}

	public void HandleFingerUpdate(FingerData fingerData)
	{
		if (!isEnabled || fingerData == null || isGuideSuppressingGameplay || isGameplayInputSuppressedByMultiTouch)
		{
			return;
		}
		GameplayPointerData pointerData;
		if (HasMultiFingerGesture())
		{
			SuppressGameplayInputForMultiTouch();
		}
		else if (capturedHandler != null)
		{
			if (fingerData.FingerId == capturedFingerId)
			{
				if (!IsHandlerAlive(capturedHandler))
				{
					ClearCapture();
					return;
				}
				lastCapturedPointerData = CreatePointerData(fingerData);
				capturedHandler.OnPointerDrag(lastCapturedPointerData);
			}
		}
		else if (isPendingSelection && fingerData.FingerId == pendingFingerId && TryAcquireOnMove(fingerData, out pointerData))
		{
			capturedHandler.OnPointerDrag(pointerData);
		}
	}

	public void HandleFingerUp(FingerData fingerData)
	{
		if (!isEnabled || fingerData == null || isGameplayInputSuppressedByMultiTouch)
		{
			return;
		}
		if (capturedHandler != null)
		{
			if (fingerData.FingerId != capturedFingerId)
			{
				ReleaseCapture(notifyPointerUp: true);
				return;
			}
			if (!IsHandlerAlive(capturedHandler))
			{
				ReleaseCapture(notifyPointerUp: false);
				return;
			}
			capturedHandler.OnPointerUp(CreatePointerData(fingerData));
			ReleaseCapture(notifyPointerUp: false);
		}
		else if (isPendingSelection)
		{
			_ = fingerData.FingerId;
			_ = pendingFingerId;
			ClearPendingSelection();
		}
	}

	private void SuppressGameplayInputForMultiTouch()
	{
		if ((RotateCameraGestureController.ZoomEnabled || RotateCameraGestureController.PanEnabled) && !isGameplayInputSuppressedByMultiTouch)
		{
			isGameplayInputSuppressedByMultiTouch = true;
			ClearPendingSelection();
			ReleaseCapture(notifyPointerUp: true);
		}
	}

	private void HandleRawFingerDown(LeanFinger finger)
	{
		if (isEnabled && IsGameplayFinger(finger))
		{
			if (capturedHandler != null && finger.Index != capturedFingerId)
			{
				ReleaseCapture(notifyPointerUp: true);
			}
			if (!isGameplayInputSuppressedByMultiTouch && HasMultiFingerGesture())
			{
				SuppressGameplayInputForMultiTouch();
			}
		}
	}

	private void HandleRawFingerUp(LeanFinger finger)
	{
		if (isGameplayInputSuppressedByMultiTouch && !HasActiveGameplayFingerExcept(finger))
		{
			if (capturedHandler != null)
			{
				ReleaseCapture(notifyPointerUp: true);
			}
			isGameplayInputSuppressedByMultiTouch = false;
		}
	}

	private static bool HasActiveGameplayFingerExcept(LeanFinger excludedFinger)
	{
		for (int i = 0; i < LeanTouch.Fingers.Count; i++)
		{
			LeanFinger val = LeanTouch.Fingers[i];
			if (val != excludedFinger && IsGameplayFinger(val))
			{
				return true;
			}
		}
		return false;
	}

	private void BeginPendingSelection(FingerData fingerData, IGameplayPointerHandler attemptedHandler = null)
	{
		isPendingSelection = true;
		pendingFingerId = fingerData.FingerId;
		MarkPendingAttempted(attemptedHandler);
	}

	private void ClearPendingSelection()
	{
		isPendingSelection = false;
		pendingFingerId = -1;
		pendingAttemptedHandlers.Clear();
	}

	private void MarkPendingAttempted(IGameplayPointerHandler handler)
	{
		if (handler != null)
		{
			pendingAttemptedHandlers.Add(handler);
		}
	}

	private bool IsPendingAttempted(IGameplayPointerHandler handler)
	{
		if (handler != null)
		{
			return pendingAttemptedHandlers.Contains(handler);
		}
		return false;
	}

	private bool TryAcquireOnMove(FingerData fingerData, out GameplayPointerData pointerData)
	{
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		pointerData = default(GameplayPointerData);
		RingInputBlockService ringInputBlockService = SingletonComponent<GameplayManager>.Instance?.InputBlock;
		if (ringInputBlockService != null && ringInputBlockService.IsBlocked(fingerData.ScreenPosition))
		{
			return false;
		}
		IGameplayPointerHandler gameplayPointerHandler = RaycastHandler(fingerData.ScreenPosition);
		if (gameplayPointerHandler == null)
		{
			return false;
		}
		if (IsPendingAttempted(gameplayPointerHandler))
		{
			return false;
		}
		pointerData = CreatePointerData(fingerData);
		return TryBindHandler(fingerData, gameplayPointerHandler, pointerData, fromPendingMove: true);
	}

	private bool TryBindHandler(FingerData fingerData, IGameplayPointerHandler handler, GameplayPointerData pointerData, bool fromPendingMove)
	{
		if (!handler.CanReceivePointer())
		{
			MarkPendingAttempted(handler);
			return false;
		}
		handler.OnPointerDown(pointerData);
		if (!handler.IsPointerCaptured())
		{
			MarkPendingAttempted(handler);
			return false;
		}
		capturedFingerId = fingerData.FingerId;
		capturedHandler = handler;
		lastCapturedPointerData = pointerData;
		isPendingSelection = false;
		pendingFingerId = -1;
		pendingAttemptedHandlers.Clear();
		Component val = (Component)((handler is Component) ? handler : null);
		if (val == null)
		{
			_ = handler.GetType().Name;
		}
		else
		{
			_ = ((Object)val.gameObject).name;
		}
		GameApp.Event.Send<GameplayPointerCaptureChangedEvent>(new GameplayPointerCaptureChangedEvent(isCaptured: true, handler));
		return true;
	}

	private static bool IsHandlerAlive(IGameplayPointerHandler handler)
	{
		if (handler == null)
		{
			return false;
		}
		Component val = (Component)((handler is Component) ? handler : null);
		if (val != null)
		{
			return (Object)(object)val != (Object)null;
		}
		return true;
	}

	private static bool IsGameplayFinger(LeanFinger finger)
	{
		if (finger == null || !finger.Set || finger.StartedOverGui)
		{
			return false;
		}
		if (finger.Index == -42)
		{
			return false;
		}
		return true;
	}

	private static bool HasMultiFingerGesture()
	{
		if (!RotateCameraGestureController.ZoomEnabled && !RotateCameraGestureController.PanEnabled)
		{
			return false;
		}
		int num = 0;
		for (int i = 0; i < LeanTouch.Fingers.Count; i++)
		{
			if (IsGameplayFinger(LeanTouch.Fingers[i]))
			{
				num++;
				if (num >= 2)
				{
					return true;
				}
			}
		}
		return false;
	}

	private IGameplayPointerHandler RaycastHandler(Vector2 screenPosition)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		RaycastHit2D rayIntersection = Physics2D.GetRayIntersection(gameplayCamera.ScreenPointToRay(Vector2.op_Implicit(screenPosition)), float.PositiveInfinity, LayerMask.op_Implicit(inputLayerMask));
		if ((Object)(object)((RaycastHit2D)(ref rayIntersection)).collider == (Object)null)
		{
			return null;
		}
		IGameplayPointerHandler componentInParent = ((Component)((RaycastHit2D)(ref rayIntersection)).collider).GetComponentInParent<IGameplayPointerHandler>();
		if (componentInParent != null)
		{
			return componentInParent;
		}
		return TryResolveCatchGuideDragHandler(((RaycastHit2D)(ref rayIntersection)).collider);
	}

	private static IGameplayPointerHandler TryResolveCatchGuideDragHandler(Collider2D hitCollider)
	{
		FixedObject componentInParent = ((Component)hitCollider).GetComponentInParent<FixedObject>();
		if ((Object)(object)componentInParent == (Object)null)
		{
			return null;
		}
		RingLockPassiveService ringLockPassiveService = SingletonComponent<GameplayManager>.Instance?.LockPassive;
		if (ringLockPassiveService == null || !ringLockPassiveService.TryGetLinkedPullObject(componentInParent, out var pullObject))
		{
			return null;
		}
		if (!pullObject.CanReceivePointer())
		{
			return null;
		}
		return pullObject;
	}

	private GameplayPointerData CreatePointerData(FingerData fingerData)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		Vector3 worldPosition = ScreenToWorldPoint(fingerData.ScreenPosition);
		return new GameplayPointerData(fingerData.FingerId, fingerData.ScreenPosition, worldPosition);
	}

	private Vector3 ScreenToWorldPoint(Vector2 screenPosition)
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		float num = Mathf.Abs(((Component)gameplayCamera).transform.position.z);
		Vector3 val = default(Vector3);
		((Vector3)(ref val))._002Ector(screenPosition.x, screenPosition.y, num);
		Vector3 result = gameplayCamera.ScreenToWorldPoint(val);
		result.z = 0f;
		return result;
	}
}
