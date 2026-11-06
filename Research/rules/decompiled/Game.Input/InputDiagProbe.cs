using System.Collections.Generic;
using Lean.Touch;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Game.Input;

public static class InputDiagProbe
{
	private const string Tag = "[InputDiag] ";

	private static bool installed;

	public static void EnsureInstalled()
	{
		if (!installed)
		{
			installed = true;
			LeanTouch.OnFingerDown += OnFingerDown;
			LeanTouch.OnFingerUp += OnFingerUp;
		}
	}

	private static void OnFingerDown(LeanFinger finger)
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		if (finger != null)
		{
			bool flag = LeanTouch.PointOverGui(finger.ScreenPosition);
			_ = InputManager.EnableStartedOverGUI && flag;
		}
	}

	private static void OnFingerUp(LeanFinger finger)
	{
	}

	private static int CountActiveFingers()
	{
		int num = 0;
		List<LeanFinger> fingers = LeanTouch.Fingers;
		for (int i = 0; i < fingers.Count; i++)
		{
			LeanFinger val = fingers[i];
			if (val != null && val.Set && val.Index != -42)
			{
				num++;
			}
		}
		return num;
	}

	private static string DescribeTopRaycastTarget(Vector2 screenPosition)
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)EventSystem.current == (Object)null)
		{
			return "noEventSystem";
		}
		List<RaycastResult> list = LeanTouch.RaycastGui(screenPosition);
		if (list == null || list.Count == 0)
		{
			return "none";
		}
		RaycastResult val = list[0];
		GameObject gameObject = ((RaycastResult)(ref val)).gameObject;
		if ((Object)(object)gameObject == (Object)null)
		{
			return "unknown";
		}
		Transform root = gameObject.transform.root;
		return ((Object)gameObject).name + "@" + (((Object)(object)root != (Object)null) ? ((Object)root).name : "?");
	}
}
