using UnityEngine;
using XingYun.UI.Controller;
using XingYun.UI.Core;

namespace RotateRings.UI.Game;

public static class UIGameRingRectUiClamp
{
	private const string HitContentNodeName = "HitContent";

	public static RectTransform TryResolveRingRect()
	{
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		IController val = default(IController);
		if (!GameApp.UI.IsOpen("UIGame", ref val) || !(val is UIGameController uIGameController))
		{
			return null;
		}
		RectTransform val2 = uIGameController.View?.RingRect;
		if (!((Object)(object)val2 == (Object)null))
		{
			Rect rect = val2.rect;
			if (!(((Rect)(ref rect)).width <= 0.01f))
			{
				rect = val2.rect;
				if (!(((Rect)(ref rect)).height <= 0.01f))
				{
					return val2;
				}
			}
		}
		return null;
	}

	public static bool TryResolveEncouragementMountParent(out RectTransform mountParent)
	{
		mountParent = null;
		RectTransform val = TryResolveRingRect();
		if ((Object)(object)((val != null) ? ((Transform)val).parent : null) != (Object)null)
		{
			Transform obj = ((Transform)val).parent.Find("HitContent");
			RectTransform val2 = (RectTransform)(object)((obj is RectTransform) ? obj : null);
			if (val2 != null)
			{
				mountParent = val2;
				return true;
			}
		}
		mountParent = (((Object)(object)UIRoot.Instance != (Object)null) ? UIRoot.Instance.topRect : null);
		return (Object)(object)mountParent != (Object)null;
	}

	public static Camera ResolveEventCamera(RectTransform rectTransform)
	{
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)rectTransform == (Object)null)
		{
			return null;
		}
		Canvas componentInParent = ((Component)rectTransform).GetComponentInParent<Canvas>();
		if ((Object)(object)componentInParent == (Object)null)
		{
			return null;
		}
		Canvas rootCanvas = componentInParent.rootCanvas;
		if ((int)rootCanvas.renderMode == 0)
		{
			return null;
		}
		return rootCanvas.worldCamera;
	}

	public static Vector2 ClampEncouragement(Vector2 targetPosition, RectTransform uiParent, Camera uiCamera, Vector2 effectHalfSize)
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_010a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0118: Unknown result type (might be due to invalid IL or missing references)
		//IL_0116: Unknown result type (might be due to invalid IL or missing references)
		RectTransform val = TryResolveRingRect();
		if ((Object)(object)val == (Object)null || (Object)(object)uiParent == (Object)null)
		{
			return targetPosition;
		}
		if (!TryGetRingRectScreenRect(val, out var screenRect))
		{
			return targetPosition;
		}
		Camera val2 = ResolveEventCamera(uiParent);
		Vector3 val3 = ((Transform)uiParent).TransformPoint(new Vector3(targetPosition.x, targetPosition.y, 0f));
		Vector2 val4 = RectTransformUtility.WorldToScreenPoint(val2, val3);
		Vector2 val5 = ConvertLocalHalfSizeToScreen(uiParent, targetPosition, effectHalfSize, val2);
		float num = ((Rect)(ref screenRect)).xMin + val5.x;
		float num2 = ((Rect)(ref screenRect)).xMax - val5.x;
		float num3 = ((Rect)(ref screenRect)).yMin + val5.y;
		float num4 = ((Rect)(ref screenRect)).yMax - val5.y;
		if (num > num2)
		{
			num = (num2 = (((Rect)(ref screenRect)).xMin + ((Rect)(ref screenRect)).xMax) * 0.5f);
		}
		if (num3 > num4)
		{
			num3 = (num4 = (((Rect)(ref screenRect)).yMin + ((Rect)(ref screenRect)).yMax) * 0.5f);
		}
		((Vector2)(ref val4))._002Ector(Mathf.Clamp(val4.x, num, num2), Mathf.Clamp(val4.y, num3, num4));
		Vector2 result = default(Vector2);
		if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(uiParent, val4, val2, ref result))
		{
			return targetPosition;
		}
		return result;
	}

	public static Vector2 ClampEncouragementWithEmojiStack(Vector2 encouragementCenterLocal, RectTransform mountParent, Vector2 encouragementHalfSize, float emojiOffsetAboveEncouragementY, float emojiUpperHalfHeight)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		float num = encouragementCenterLocal.y - encouragementHalfSize.y;
		float num2 = encouragementCenterLocal.y + emojiOffsetAboveEncouragementY + emojiUpperHalfHeight;
		float num3 = (num + num2) * 0.5f;
		float num4 = (num2 - num) * 0.5f;
		Vector2 effectHalfSize = default(Vector2);
		((Vector2)(ref effectHalfSize))._002Ector(encouragementHalfSize.x, num4);
		Vector2 val = ClampEncouragement(new Vector2(encouragementCenterLocal.x, num3), mountParent, null, effectHalfSize);
		float num5 = (emojiOffsetAboveEncouragementY + emojiUpperHalfHeight - encouragementHalfSize.y) * 0.5f;
		((Vector2)(ref encouragementCenterLocal))._002Ector(val.x, val.y - num5);
		return encouragementCenterLocal;
	}

	private static bool TryGetRingRectVerticalRangeInMountLocal(RectTransform mountParent, out float minY, out float maxY)
	{
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		minY = (maxY = 0f);
		RectTransform val = TryResolveRingRect();
		if ((Object)(object)val == (Object)null || (Object)(object)mountParent == (Object)null)
		{
			return false;
		}
		Vector3[] array = (Vector3[])(object)new Vector3[4];
		val.GetWorldCorners(array);
		minY = float.PositiveInfinity;
		maxY = float.NegativeInfinity;
		for (int i = 0; i < array.Length; i++)
		{
			Vector3 val2 = ((Transform)mountParent).InverseTransformPoint(array[i]);
			minY = Mathf.Min(minY, val2.y);
			maxY = Mathf.Max(maxY, val2.y);
		}
		return maxY > minY;
	}

	public static void ShiftToKeepBoundsInsideRing(RectTransform target, RectTransform mountParent)
	{
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		if (!((Object)(object)target == (Object)null) && !((Object)(object)mountParent == (Object)null))
		{
			Canvas.ForceUpdateCanvases();
			Bounds val = RectTransformUtility.CalculateRelativeRectTransformBounds((Transform)(object)mountParent, (Transform)(object)target);
			Vector2 val2 = default(Vector2);
			((Vector2)(ref val2))._002Ector(((Bounds)(ref val)).center.x, ((Bounds)(ref val)).center.y);
			Vector2 effectHalfSize = default(Vector2);
			((Vector2)(ref effectHalfSize))._002Ector(((Bounds)(ref val)).extents.x, ((Bounds)(ref val)).extents.y);
			Vector2 val3 = ClampEncouragement(val2, mountParent, null, effectHalfSize) - val2;
			if (!(((Vector2)(ref val3)).sqrMagnitude <= 0.01f))
			{
				Vector3 localPosition = ((Transform)target).localPosition;
				((Transform)target).localPosition = new Vector3(localPosition.x + val3.x, localPosition.y + val3.y, localPosition.z);
			}
		}
	}

	private static bool TryGetRingRectScreenRect(RectTransform ringRect, out Rect screenRect)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		screenRect = default(Rect);
		if ((Object)(object)ringRect == (Object)null)
		{
			return false;
		}
		Camera val = ResolveEventCamera(ringRect);
		Vector3[] array = (Vector3[])(object)new Vector3[4];
		ringRect.GetWorldCorners(array);
		float num = float.PositiveInfinity;
		float num2 = float.PositiveInfinity;
		float num3 = float.NegativeInfinity;
		float num4 = float.NegativeInfinity;
		for (int i = 0; i < array.Length; i++)
		{
			Vector2 val2 = RectTransformUtility.WorldToScreenPoint(val, array[i]);
			num = Mathf.Min(num, val2.x);
			num2 = Mathf.Min(num2, val2.y);
			num3 = Mathf.Max(num3, val2.x);
			num4 = Mathf.Max(num4, val2.y);
		}
		if (num3 <= num || num4 <= num2)
		{
			return false;
		}
		screenRect = Rect.MinMaxRect(num, num2, num3, num4);
		return true;
	}

	private static Vector2 ConvertLocalHalfSizeToScreen(RectTransform uiParent, Vector2 anchorLocal, Vector2 halfLocal, Camera parentCamera)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		Vector3 val = ((Transform)uiParent).TransformPoint(new Vector3(anchorLocal.x, anchorLocal.y, 0f));
		Vector3 val2 = ((Transform)uiParent).TransformPoint(new Vector3(anchorLocal.x + halfLocal.x, anchorLocal.y, 0f));
		Vector3 val3 = ((Transform)uiParent).TransformPoint(new Vector3(anchorLocal.x, anchorLocal.y + halfLocal.y, 0f));
		Vector2 val4 = RectTransformUtility.WorldToScreenPoint(parentCamera, val);
		Vector2 val5 = RectTransformUtility.WorldToScreenPoint(parentCamera, val2);
		Vector2 val6 = RectTransformUtility.WorldToScreenPoint(parentCamera, val3);
		return new Vector2(Mathf.Abs(val5.x - val4.x), Mathf.Abs(val6.y - val4.y));
	}
}
