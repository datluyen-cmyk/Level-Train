using System;
using UnityEngine;
using UnityEngine.UI;
using XingYun.Core.Singletons;
using XingYun.Event;
using XingYun.UI.Adapt;
using XingYun.UI.Core;
using XingYun.UI.Event;

namespace Rotaterings.Tools;

public static class CanvasMatchAdapter
{
	private const string Tag = "CanvasMatch";

	private const float Aspect16By9 = 1.7777778f;

	private const float Aspect18By9 = 2f;

	private static readonly Vector2 RefRes18By9 = new Vector2(1080f, 2160f);

	private static readonly Vector2 RefRes16By9 = new Vector2(1080f, 1920f);

	private const bool Enabled = true;

	private const bool Use16By9RefOnSquareScreen = false;

	private const float SafeAreaBottomBugThreshold = 1f;

	private static bool eventsRegistered;

	private static bool screenModeSubscribed;

	[RuntimeInitializeOnLoadMethod(/*Could not decode attribute arguments.*/)]
	private static void ResetStatics()
	{
		eventsRegistered = false;
		screenModeSubscribed = false;
	}

	public static void EnsureInitialized()
	{
		RegisterEvents();
		TrySubscribeScreenMode();
		Apply();
	}

	private static void RegisterEvents()
	{
		if (!eventsRegistered && Singleton<EventManager>.Instance != null)
		{
			Singleton<EventManager>.Instance.Listen<UIRootReady>((Action<UIRootReady>)OnUIRootReady);
			eventsRegistered = true;
		}
	}

	private static void TrySubscribeScreenMode()
	{
		if (!screenModeSubscribed && !((Object)(object)MonoSingleton<ScreenManager>.Instance == (Object)null))
		{
			ScreenManager instance = MonoSingleton<ScreenManager>.Instance;
			instance.ScreenModeChanged = (Action<EScreenMode, int, int>)Delegate.Combine(instance.ScreenModeChanged, new Action<EScreenMode, int, int>(OnScreenModeChanged));
			screenModeSubscribed = true;
		}
	}

	private static void OnUIRootReady(UIRootReady evt)
	{
		if (evt.IsReady)
		{
			TrySubscribeScreenMode();
			Apply();
		}
	}

	private static void OnScreenModeChanged(EScreenMode screenMode, int width, int height)
	{
		Apply();
	}

	private static void Apply()
	{
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)UIRoot.Instance == (Object)null)
		{
			return;
		}
		CanvasScaler uICanvasScaler = UIRoot.Instance.UICanvasScaler;
		if ((Object)(object)uICanvasScaler != (Object)null)
		{
			Vector2 screenSize = ScreenManager.ScreenSize;
			float num = ((GetAspect(screenSize.x, screenSize.y) < 1.7777778f) ? 1f : 0f);
			Vector2 refRes18By = RefRes18By9;
			bool flag = !Mathf.Approximately(uICanvasScaler.matchWidthOrHeight, num);
			bool flag2 = uICanvasScaler.referenceResolution != refRes18By;
			if (flag || flag2)
			{
				uICanvasScaler.matchWidthOrHeight = num;
				uICanvasScaler.referenceResolution = refRes18By;
				Canvas.ForceUpdateCanvases();
			}
		}
		TryRepairSafeAreaBottom();
	}

	private static void TryRepairSafeAreaBottom()
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Invalid comparison between Unknown and I4
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		if ((int)ScreenManager.CurDetailScreenMode != 1)
		{
			return;
		}
		SafeArea val = UIRoot.Instance?.UISafeArea;
		if ((Object)(object)val == (Object)null)
		{
			return;
		}
		Transform transform = ((Component)val).transform;
		RectTransform val2 = (RectTransform)(object)((transform is RectTransform) ? transform : null);
		if (!((Object)(object)val2 == (Object)null))
		{
			Rect safeArea = Screen.safeArea;
			float num = Mathf.Max(0f, ((Rect)(ref safeArea)).yMin);
			if (!(val2.offsetMin.y <= num + 1f))
			{
				Vector2 offsetMin = val2.offsetMin;
				offsetMin.y = num;
				val2.offsetMin = offsetMin;
			}
		}
	}

	private static float GetAspect(float width, float height)
	{
		float num = Mathf.Min(width, height);
		if (num < 1f)
		{
			return 2f;
		}
		return Mathf.Max(width, height) / num;
	}
}
