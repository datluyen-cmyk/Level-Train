using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;
using XingYun.Core.Singletons;
using XingYun.Event;
using XingYun.UI.Core;
using XingYun.UI.Event;

namespace Game.UI;

public class UICameraOverlayHandler : MonoBehaviour
{
	private const string TAG = "UICameraOverlay";

	private const string DiagTag = "[UICAMFIX]";

	private const float RetryDelaySeconds = 0.5f;

	private const int MaxRetryCount = 3;

	private static UICameraOverlayHandler _instance;

	private Camera _uiCamera;

	private Coroutine _setupCoroutine;

	private bool _eventsRegistered;

	[RuntimeInitializeOnLoadMethod(/*Could not decode attribute arguments.*/)]
	private static void AutoInit()
	{
		EnsureInitialized();
	}

	public static void EnsureInitialized()
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Expected O, but got Unknown
		if ((Object)(object)_instance == (Object)null)
		{
			GameObject val = new GameObject("[UICameraOverlayHandler]");
			_instance = val.AddComponent<UICameraOverlayHandler>();
			Object.DontDestroyOnLoad((Object)val);
		}
		_instance.RegisterEvents();
	}

	public static bool TryAttachOverlay()
	{
		EnsureInitialized();
		if ((Object)(object)_instance != (Object)null)
		{
			return _instance.SetupOverlay();
		}
		return false;
	}

	public static bool ValidateAndRepair(string reason)
	{
		EnsureInitialized();
		if ((Object)(object)_instance != (Object)null)
		{
			return _instance.ValidateAndRepairInternal(reason);
		}
		return false;
	}

	private bool ValidateAndRepairInternal(string reason)
	{
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Invalid comparison between Unknown and I4
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Invalid comparison between Unknown and I4
		if ((Object)(object)UIRoot.Instance == (Object)null)
		{
			return false;
		}
		_uiCamera = UIRoot.Instance.UICamera;
		if ((Object)(object)_uiCamera == (Object)null)
		{
			return false;
		}
		bool num = (int)CameraExtensions.GetUniversalAdditionalCameraData(_uiCamera).renderType == 1;
		Camera gameplayCamera = GameplayCameraManager.GameplayCamera;
		UniversalAdditionalCameraData val = null;
		bool flag = (Object)(object)gameplayCamera != (Object)null && ((Component)gameplayCamera).TryGetComponent<UniversalAdditionalCameraData>(ref val) && (int)val.renderType == 0 && (Object)(object)val != (Object)null && val.cameraStack.Contains(_uiCamera);
		if (num && flag)
		{
			_ = Debug.isDebugBuild;
			return true;
		}
		LogAllCameras("before-repair:" + reason);
		bool num2 = SetupOverlay();
		if (!num2)
		{
			LogAllCameras("repair-failed:" + reason);
		}
		return num2;
	}

	private void RegisterEvents()
	{
		if (!_eventsRegistered && Singleton<EventManager>.Instance != null)
		{
			Singleton<EventManager>.Instance.Listen<UIRootReady>((Action<UIRootReady>)OnUIRootReady);
			SceneManager.sceneLoaded += OnSceneLoaded;
			_eventsRegistered = true;
		}
	}

	private void OnDisable()
	{
		if (_eventsRegistered && Singleton<EventManager>.Instance != null)
		{
			Singleton<EventManager>.Instance.Unlisten<UIRootReady>((Action<UIRootReady>)OnUIRootReady);
			SceneManager.sceneLoaded -= OnSceneLoaded;
			_eventsRegistered = false;
		}
	}

	private void OnUIRootReady(UIRootReady evt)
	{
		if (evt.IsReady)
		{
			RequestSetupOverlay();
		}
	}

	private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
	{
		RequestSetupOverlay();
	}

	private void RequestSetupOverlay()
	{
		if (_setupCoroutine != null)
		{
			((MonoBehaviour)this).StopCoroutine(_setupCoroutine);
		}
		_setupCoroutine = ((MonoBehaviour)this).StartCoroutine(SetupOverlayWithRetry());
	}

	private IEnumerator SetupOverlayWithRetry()
	{
		for (int attempt = 0; attempt <= 3; attempt++)
		{
			if (attempt > 0)
			{
				yield return (object)new WaitForSeconds(0.5f);
			}
			else
			{
				yield return null;
			}
			if (SetupOverlay())
			{
				LogAllCameras($"attached attempt={attempt}");
				_setupCoroutine = null;
				yield break;
			}
		}
		LogAllCameras("attach-failed");
		_setupCoroutine = null;
	}

	private bool SetupOverlay()
	{
		if ((Object)(object)UIRoot.Instance == (Object)null)
		{
			return false;
		}
		_uiCamera = UIRoot.Instance.UICamera;
		if ((Object)(object)_uiCamera == (Object)null)
		{
			return false;
		}
		CameraExtensions.GetUniversalAdditionalCameraData(_uiCamera).renderType = (CameraRenderType)1;
		return AttachToSceneBaseCamera();
	}

	private bool AttachToSceneBaseCamera()
	{
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)_uiCamera == (Object)null)
		{
			return false;
		}
		Camera[] allCameras = Camera.allCameras;
		foreach (Camera val in allCameras)
		{
			if (!((Object)(object)val == (Object)(object)_uiCamera))
			{
				UniversalAdditionalCameraData universalAdditionalCameraData = CameraExtensions.GetUniversalAdditionalCameraData(val);
				if ((int)universalAdditionalCameraData.renderType == 0 && universalAdditionalCameraData.cameraStack.Contains(_uiCamera))
				{
					universalAdditionalCameraData.cameraStack.Remove(_uiCamera);
				}
			}
		}
		Camera val2 = ResolveBaseCamera();
		if ((Object)(object)val2 == (Object)null)
		{
			return false;
		}
		UniversalAdditionalCameraData universalAdditionalCameraData2 = CameraExtensions.GetUniversalAdditionalCameraData(val2);
		if (!universalAdditionalCameraData2.cameraStack.Contains(_uiCamera))
		{
			universalAdditionalCameraData2.cameraStack.Add(_uiCamera);
		}
		return true;
	}

	private Camera ResolveBaseCamera()
	{
		Camera gameplayCamera = GameplayCameraManager.GameplayCamera;
		if (IsValidBaseCamera(gameplayCamera))
		{
			return gameplayCamera;
		}
		Camera val = null;
		Camera[] allCameras = Camera.allCameras;
		foreach (Camera val2 in allCameras)
		{
			if (IsValidBaseCamera(val2) && ((Object)(object)val == (Object)null || val2.depth < val.depth))
			{
				val = val2;
			}
		}
		return val;
	}

	private bool IsValidBaseCamera(Camera cam)
	{
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Invalid comparison between Unknown and I4
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)cam == (Object)null || (Object)(object)cam == (Object)(object)_uiCamera || !((Behaviour)cam).enabled)
		{
			return false;
		}
		if ((Object)(object)_uiCamera != (Object)null && ((Component)cam).gameObject.scene == ((Component)_uiCamera).gameObject.scene)
		{
			return false;
		}
		return (int)CameraExtensions.GetUniversalAdditionalCameraData(cam).renderType == 0;
	}

	internal static void LogAllCameras(string reason)
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		Camera[] allCameras = Camera.allCameras;
		UniversalAdditionalCameraData val = default(UniversalAdditionalCameraData);
		for (int i = 0; i < allCameras.Length; i++)
		{
			if (((Component)allCameras[i]).TryGetComponent<UniversalAdditionalCameraData>(ref val) && (int)val.renderType == 0)
			{
				_ = val.cameraStack.Count;
			}
		}
	}
}
