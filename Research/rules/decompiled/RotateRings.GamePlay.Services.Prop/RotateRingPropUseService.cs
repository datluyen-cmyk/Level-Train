using System;
using System.Collections.Generic;
using Game.DataTrack;
using Game.Input;
using RotateRings.GamePlay.Services.Prop.Handlers;
using RotateRings.Prop;
using UnityEngine;
using UnlockRing;
using XingYun.Core.Singletons;

namespace RotateRings.GamePlay.Services.Prop;

public sealed class RotateRingPropUseService : GameplayServiceBase<GameplayManager>, IRotateRingPropHost
{
	private const string Tag = "[RotateRingPropUseService] ";

	private readonly Dictionary<EPropType, IRotateRingPropUse> handlers;

	private IRotateRingPropUse activeHandler;

	private bool isListening;

	private Action<HammerRemoveSelection> hammerRemoveCallback;

	public bool IsActive => activeHandler != null;

	public bool HasSkyrocketInFlight
	{
		get
		{
			if (TryGetHandler(EPropType.Skyrocket) is RocketPropHandler rocketPropHandler)
			{
				return rocketPropHandler.IsAnyFlightActive;
			}
			return false;
		}
	}

	public bool IsLevelCompleted => base.Host.IsLevelCompleted;

	public RotateRingPropUseService(GameplayManager host)
		: base(host)
	{
		handlers = new Dictionary<EPropType, IRotateRingPropUse>
		{
			{
				EPropType.Hammer,
				new RotateRingHammerPropHandler()
			},
			{
				EPropType.Skyrocket,
				new RocketPropHandler()
			}
		};
		EnableEventListening();
	}

	public IRotateRingPropUse TryGetHandler(EPropType propType)
	{
		if (!handlers.TryGetValue(propType, out var value))
		{
			return null;
		}
		return value;
	}

	public bool Begin(EPropType propType, IRotateRingPropUseCallback callback)
	{
		End();
		if (!handlers.TryGetValue(propType, out var value))
		{
			return false;
		}
		activeHandler = value;
		activeHandler.Begin(this, callback);
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

	public void OnUseButtonClicked()
	{
		activeHandler?.OnUseButtonClicked();
	}

	public bool TryFireSkyrocket(IRotateRingPropUseCallback callback, TrackPropUseReason reason)
	{
		if (!handlers.TryGetValue(EPropType.Skyrocket, out var value) || !(value is RocketPropHandler rocketPropHandler))
		{
			return false;
		}
		if (activeHandler != rocketPropHandler)
		{
			End();
			activeHandler = rocketPropHandler;
			rocketPropHandler.Begin(this, callback);
		}
		return rocketPropHandler.Fire(reason);
	}

	public void OnGuideDismissed(bool cancelled)
	{
		activeHandler?.OnGuideDismissed(cancelled);
	}

	public void End()
	{
		if (activeHandler != null)
		{
			activeHandler.End();
			activeHandler = null;
		}
	}

	public override void ClearLevel()
	{
		foreach (IRotateRingPropUse value in handlers.Values)
		{
			value.End();
		}
		activeHandler = null;
	}

	public override void Dispose()
	{
		if (base.IsDisposed)
		{
			return;
		}
		base.Dispose();
		if (isListening)
		{
			GameApp.Event.Unlisten<PropUseSessionStateChangedEvent>((Action<PropUseSessionStateChangedEvent>)OnPropUseSessionStateChanged);
			isListening = false;
		}
		End();
		foreach (IRotateRingPropUse value in handlers.Values)
		{
			if (value is IDisposable disposable)
			{
				disposable.Dispose();
			}
		}
	}

	public bool CanUseHammer()
	{
		if (!base.Host.IsLevelCompleted)
		{
			return base.Host.HasUnsolvedGameplayObjects;
		}
		return false;
	}

	public bool CanUseRocket()
	{
		if (!base.Host.IsLevelCompleted)
		{
			return base.Host.HasUnsolvedGameplayObjects;
		}
		return false;
	}

	public void BeginHammerRemoveMode(Action<HammerRemoveSelection> onSelected)
	{
		RRDataTrack.Instance.LegacySessionCollector.RecordFirstOperation();
		hammerRemoveCallback = onSelected;
		if ((Object)(object)base.Host.CurrentLevelObject == (Object)null)
		{
			return;
		}
		IReadOnlyList<IGameplayObject> gameplayObjects = base.Host.ObjectsService.GameplayObjects;
		for (int i = 0; i < gameplayObjects.Count; i++)
		{
			IGameplayObject gameplayObject = gameplayObjects[i];
			if (gameplayObject != null && !gameplayObject.IsSolved)
			{
				gameplayObject.EnableRemoveMode(OnHammerRemovePiece, deferSolve: true);
			}
		}
	}

	public void EndHammerRemoveMode()
	{
		hammerRemoveCallback = null;
		if (!((Object)(object)base.Host.CurrentLevelObject == (Object)null))
		{
			IReadOnlyList<IGameplayObject> gameplayObjects = base.Host.ObjectsService.GameplayObjects;
			for (int i = 0; i < gameplayObjects.Count; i++)
			{
				gameplayObjects[i]?.DisableRemoveMode();
			}
		}
	}

	public bool TryExecuteHammerRemove(IGameplayObject target)
	{
		if (target == null || target.IsSolved)
		{
			return false;
		}
		base.Host.BeginHammerLegacyOperation(target);
		target.CompleteHammerRemove();
		base.Host.ObjectsSolveAndCheckVictory();
		IReadOnlyList<GameplayReleasedPart> readOnlyList = base.Host.EndLegacyOperation(target, LegacyOperationSource.Hammer);
		base.Host.CompleteLegacyOperation(target, LegacyOperationSource.Hammer, readOnlyList);
		return readOnlyList.Count > 0;
	}

	public void ResetPropGestureCapture()
	{
		Singleton<GameInputManager>.Instance?.ClearGameplayCapture();
	}

	private void EnableEventListening()
	{
		if (!isListening)
		{
			GameApp.Event.Listen<PropUseSessionStateChangedEvent>((Action<PropUseSessionStateChangedEvent>)OnPropUseSessionStateChanged);
			isListening = true;
		}
	}

	private void OnPropUseSessionStateChanged(PropUseSessionStateChangedEvent eventData)
	{
		ResetPropGestureCapture();
	}

	private void OnHammerRemovePiece(IGameplayObject removed, Vector3 pointerWorld)
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		Action<HammerRemoveSelection> action = hammerRemoveCallback;
		EndHammerRemoveMode();
		if (removed != null)
		{
			action?.Invoke(new HammerRemoveSelection(removed, pointerWorld));
		}
	}
}
