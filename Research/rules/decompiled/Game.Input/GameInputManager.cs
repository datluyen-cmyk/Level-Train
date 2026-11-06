using System;
using Hot.Model;
using Lean.Touch;
using UnityEngine;
using XingYun.Core.Singletons;

namespace Game.Input;

public class GameInputManager : Singleton<GameInputManager>
{
	private readonly InputManager inputManager = new InputManager();

	private readonly GameplayInputDispatcher gameplayDispatcher = new GameplayInputDispatcher();

	private bool eventsRegistered;

	private bool gameplayInputEnabled;

	public void EnsureInitialized()
	{
		ConfigureLeanTouch();
		if (!eventsRegistered)
		{
			OnRegisterEvents();
		}
		InputDiagProbe.EnsureInstalled();
	}

	private static void ConfigureLeanTouch()
	{
		if (LeanTouch.Instances.Count > 0)
		{
			LeanTouch.Instances[0].UseHover = false;
		}
	}

	public void OnRegisterEvents()
	{
		if (!eventsRegistered)
		{
			InputManager obj = inputManager;
			obj.OnFingerDownCallback = (Action<FingerData>)Delegate.Combine(obj.OnFingerDownCallback, new Action<FingerData>(OnFingerDown));
			InputManager obj2 = inputManager;
			obj2.OnFingerUpdateCallback = (Action<FingerData>)Delegate.Combine(obj2.OnFingerUpdateCallback, new Action<FingerData>(OnFingerUpdate));
			InputManager obj3 = inputManager;
			obj3.OnFingerUpCallback = (Action<FingerData>)Delegate.Combine(obj3.OnFingerUpCallback, new Action<FingerData>(OnFingerUp));
			eventsRegistered = true;
		}
	}

	public void OnUnregisterEvents()
	{
		if (eventsRegistered)
		{
			InputManager obj = inputManager;
			obj.OnFingerDownCallback = (Action<FingerData>)Delegate.Remove(obj.OnFingerDownCallback, new Action<FingerData>(OnFingerDown));
			InputManager obj2 = inputManager;
			obj2.OnFingerUpdateCallback = (Action<FingerData>)Delegate.Remove(obj2.OnFingerUpdateCallback, new Action<FingerData>(OnFingerUpdate));
			InputManager obj3 = inputManager;
			obj3.OnFingerUpCallback = (Action<FingerData>)Delegate.Remove(obj3.OnFingerUpCallback, new Action<FingerData>(OnFingerUp));
			eventsRegistered = false;
		}
	}

	public void EnableGameplayInput(Camera camera = null)
	{
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		EnsureInitialized();
		if (camera == null)
		{
			camera = GameplayCameraManager.GameplayCamera;
		}
		if (!((Object)(object)camera == (Object)null))
		{
			LayerMask layerMask = GameplayInputSettings.InputLayerMask;
			GameplayCameraSettings component = ((Component)camera).GetComponent<GameplayCameraSettings>();
			if ((Object)(object)component != (Object)null)
			{
				layerMask = component.InputLayerMask;
			}
			if (((LayerMask)(ref layerMask)).value == 0)
			{
				layerMask = GameplayInputSettings.DefaultRingInputLayerMask;
			}
			gameplayInputEnabled = true;
			gameplayDispatcher.Enable(camera, layerMask);
		}
	}

	public void DisableGameplayInput(bool notifyCapturedHandler = true)
	{
		gameplayInputEnabled = false;
		gameplayDispatcher.Disable(notifyCapturedHandler);
	}

	public void ClearGameplayCapture()
	{
		gameplayDispatcher.ClearCapture();
	}

	public void SetMultiTouchGuideSuppressingGameplay(bool suppress)
	{
		gameplayDispatcher.SetGuideSuppressingGameplay(suppress);
	}

	public void SetGameplayInputEnabled(bool enabled)
	{
		if (enabled)
		{
			EnableGameplayInput();
		}
		else
		{
			DisableGameplayInput();
		}
	}

	private void OnFingerDown(FingerData data)
	{
		if (!GameInfo.IsGameUILock && gameplayInputEnabled)
		{
			gameplayDispatcher.HandleFingerDown(data);
		}
	}

	private void OnFingerUpdate(FingerData data)
	{
		if (!GameInfo.IsGameUILock && gameplayInputEnabled)
		{
			gameplayDispatcher.HandleFingerUpdate(data);
		}
	}

	private void OnFingerUp(FingerData data)
	{
		if (!GameInfo.IsGameUILock && gameplayInputEnabled)
		{
			gameplayDispatcher.HandleFingerUp(data);
		}
	}
}
