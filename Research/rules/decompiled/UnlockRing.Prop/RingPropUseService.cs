using System;
using System.Collections.Generic;
using RotateRings.Prop;
using UnityEngine;
using UnlockRing.Core.Domain;
using UnlockRing.Prop.Handlers;

namespace UnlockRing.Prop;

public sealed class RingPropUseService : IBoardGestureOverride, IDisposable
{
	private const string Tag = "[RingPropUseService] ";

	private readonly IPropGameManager host;

	private readonly Dictionary<EPropType, IUnlockRingPropUse> handlers;

	private IUnlockRingPropUse activeHandler;

	private Vector2 pointerDownScreen;

	private bool suppressBlockedFeedback;

	private bool isListening;

	public bool IsActive => activeHandler != null;

	public bool SuppressBlockedFeedback => suppressBlockedFeedback;

	public RingPropUseService(IPropGameManager host)
	{
		this.host = host;
		handlers = new Dictionary<EPropType, IUnlockRingPropUse> { 
		{
			EPropType.Hammer,
			new HammerPropHandler()
		} };
		EnableEventListening();
	}

	public IUnlockRingPropUse TryGetHandler(EPropType propType)
	{
		if (!handlers.TryGetValue(propType, out var value))
		{
			return null;
		}
		return value;
	}

	public bool Begin(EPropType propType, IUnlockRingPropUseCallback callback)
	{
		End();
		if (!handlers.TryGetValue(propType, out var value))
		{
			return false;
		}
		activeHandler = value;
		activeHandler.Begin(host, callback);
		return true;
	}

	public void OnPropGuideOpened()
	{
		activeHandler?.OnPropGuideOpened();
	}

	public void OnGuideReady()
	{
		if (activeHandler != null)
		{
			activeHandler.OnGuideReady();
		}
	}

	public void OnGuideDismissed(bool cancelled)
	{
		activeHandler?.OnGuideDismissed(cancelled);
	}

	public bool TryOverrideBegin(CoreBoardRenderer board, Vector2 screen, out BoardGestureOverrideResult result)
	{
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		if (!IsActive || (Object)(object)board == (Object)null)
		{
			result = default(BoardGestureOverrideResult);
			return false;
		}
		pointerDownScreen = screen;
		Vector2 world = board.ScreenToWorld(screen);
		PartId? hit = ResolveHitPart(board, world);
		return TryHandlePointer(board, PropPointerPhase.Begin, screen, world, hit, out result);
	}

	public bool TryOverrideUpdate(CoreBoardRenderer board, Vector2 screen, Vector2 world, PartId? activePart, out BoardGestureOverrideResult result)
	{
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		if (!IsActive || (Object)(object)board == (Object)null)
		{
			result = default(BoardGestureOverrideResult);
			return false;
		}
		return TryHandlePointer(board, PropPointerPhase.Update, screen, world, activePart, out result);
	}

	public bool TryOverrideEnd(CoreBoardRenderer board, Vector2 screen, Vector2 world, PartId? activePart, out BoardGestureOverrideResult result)
	{
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		if (!IsActive || (Object)(object)board == (Object)null)
		{
			result = default(BoardGestureOverrideResult);
			return false;
		}
		return TryHandlePointer(board, PropPointerPhase.End, screen, world, activePart, out result);
	}

	public void End()
	{
		if (activeHandler != null)
		{
			activeHandler.End();
			activeHandler = null;
		}
	}

	public void Dispose()
	{
		if (isListening)
		{
			GameApp.Event.Unlisten<PropUseSessionStateChangedEvent>((Action<PropUseSessionStateChangedEvent>)OnPropUseSessionStateChanged);
			isListening = false;
		}
	}

	private void EnableEventListening()
	{
		if (!isListening)
		{
			GameApp.Event.Listen<PropUseSessionStateChangedEvent>((Action<PropUseSessionStateChangedEvent>)OnPropUseSessionStateChanged);
			isListening = true;
		}
	}

	private PartId? ResolveHitPart(CoreBoardRenderer board, Vector2 world)
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		return activeHandler?.ResolvePointerHit(board, world);
	}

	private bool TryHandlePointer(CoreBoardRenderer board, PropPointerPhase phase, Vector2 screen, Vector2 world, PartId? hit, out BoardGestureOverrideResult result)
	{
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		if (phase == PropPointerPhase.Begin)
		{
			suppressBlockedFeedback = false;
		}
		PropPointerSnapshot snap = new PropPointerSnapshot
		{
			Board = board,
			ScreenPosition = screen,
			WorldPosition = world,
			PointerDownScreen = pointerDownScreen,
			HitPart = hit
		};
		Vector2 val = screen - pointerDownScreen;
		snap.ScreenDistanceFromDown = ((Vector2)(ref val)).magnitude;
		PropPointerResult propPointerResult = DispatchPointer(phase, in snap);
		if (propPointerResult.SuppressBlockedFeedback)
		{
			suppressBlockedFeedback = true;
		}
		bool flag = propPointerResult.Type != PropPointerResult.ResultType.PassThrough;
		result = new BoardGestureOverrideResult
		{
			Consumed = flag,
			InitPointerTracking = (phase == PropPointerPhase.Begin)
		};
		return flag;
	}

	private PropPointerResult DispatchPointer(PropPointerPhase phase, in PropPointerSnapshot snap)
	{
		if (activeHandler == null)
		{
			return PropPointerResult.PassThrough;
		}
		return activeHandler.OnPointer(phase, in snap);
	}

	private void OnPropUseSessionStateChanged(PropUseSessionStateChangedEvent eventData)
	{
		suppressBlockedFeedback = false;
		host?.ResetPropGestureCapture();
	}
}
