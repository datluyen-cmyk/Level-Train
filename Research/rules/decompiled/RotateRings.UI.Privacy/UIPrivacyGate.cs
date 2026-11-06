using System;
using RotateRings.WeakGuide.Setup;
using UniLog;
using UnityEngine;
using XingYun.Aot;

namespace RotateRings.UI.Privacy;

public static class UIPrivacyGate
{
	private const string TAG = "XYFW-Privacy";

	private static bool isShowing;

	public const string PrefabLocation = "UIPrivacy";

	private const float PrewarmDelaySeconds = 0.5f;

	public static bool HasAgreed()
	{
		if (!SDKManager.HasAgreedPrivacyDialog())
		{
			return GameApp.Proxy.Get<PrivacyProxy>().HasAgreedStartupPrivacy();
		}
		return true;
	}

	public static void MarkAgreed()
	{
		GameApp.Proxy.Get<PrivacyProxy>().MarkStartupPrivacyAgreed();
		SDKManager.NotifyPrivacyDialogAgreed();
	}

	public static void WaitIfNeeded(Action onComplete)
	{
		if (HasAgreed())
		{
			onComplete?.Invoke();
		}
		else
		{
			Show(onComplete);
		}
	}

	public static void Show(Action onComplete)
	{
		if (isShowing)
		{
			onComplete?.Invoke();
			return;
		}
		isShowing = true;
		UIPrivacyController controller = null;
		GameObject instance = null;
		try
		{
			GameObject val = GameApp.Res.LoadAsset<GameObject>("UIPrivacy", (string)null);
			if ((Object)(object)val == (Object)null)
			{
				Log.LogError("XYFW-Privacy", "Prefab 加载失败: {0}", new object[1] { "UIPrivacy" });
				CleanupPrivacyDialog(ref controller, ref instance);
				onComplete?.Invoke();
				return;
			}
			instance = Object.Instantiate<GameObject>(val);
			Object.DontDestroyOnLoad((Object)(object)instance);
			UIPrivacyView component = instance.GetComponent<UIPrivacyView>();
			if ((Object)(object)component == (Object)null)
			{
				Log.LogError("XYFW-Privacy", "Prefab 缺少 UIPrivacyView 组件");
				CleanupPrivacyDialog(ref controller, ref instance);
				onComplete?.Invoke();
				return;
			}
			controller = new UIPrivacyController(component, delegate
			{
				CleanupPrivacyDialog(ref controller, ref instance);
				onComplete?.Invoke();
			});
			controller.OnShow();
			GameApp.Timer.Delay(0.5f, (Action)GenericWeakGuideSetup.PrewarmJsonDeserialization);
		}
		catch (Exception ex)
		{
			Log.LogError("XYFW-Privacy", "隐私弹窗展示异常: {0}", new object[1] { ex });
			CleanupPrivacyDialog(ref controller, ref instance);
			onComplete?.Invoke();
		}
	}

	private static void CleanupPrivacyDialog(ref UIPrivacyController controller, ref GameObject instance)
	{
		controller?.OnDispose();
		controller = null;
		if ((Object)(object)instance != (Object)null)
		{
			Object.Destroy((Object)(object)instance);
			instance = null;
		}
		isShowing = false;
	}
}
