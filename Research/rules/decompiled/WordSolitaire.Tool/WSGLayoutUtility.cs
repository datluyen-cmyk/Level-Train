using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.UI;

namespace WordSolitaire.Tool;

public static class WSGLayoutUtility
{
	public static float GetMaxWidth(RectTransform rect)
	{
		return GetMaxProperty(rect, (IWSGLayoutElement e) => e.maxWidth);
	}

	public static float GetMaxHeight(RectTransform rect)
	{
		return GetMaxProperty(rect, (IWSGLayoutElement e) => e.maxHeight);
	}

	public static float GetMinWidth(RectTransform rect)
	{
		return GetMinProperty(rect, (ILayoutElement e) => e.minWidth);
	}

	public static float GetMinHeight(RectTransform rect)
	{
		return GetMinProperty(rect, (ILayoutElement e) => e.minHeight);
	}

	public static float ClampMinWidth(RectTransform rect, float measuredWidth)
	{
		float minWidth = GetMinWidth(rect);
		if (minWidth < 0f)
		{
			return measuredWidth;
		}
		return Mathf.Max(measuredWidth, minWidth);
	}

	public static float ClampMinHeight(RectTransform rect, float measuredHeight)
	{
		float minHeight = GetMinHeight(rect);
		if (minHeight < 0f)
		{
			return measuredHeight;
		}
		return Mathf.Max(measuredHeight, minHeight);
	}

	public static float ClampWidth(RectTransform rect, float measuredWidth)
	{
		float maxWidth = GetMaxWidth(rect);
		if (maxWidth < 0f)
		{
			return measuredWidth;
		}
		return Mathf.Min(measuredWidth, maxWidth);
	}

	public static float ClampHeight(RectTransform rect, float measuredHeight)
	{
		float maxHeight = GetMaxHeight(rect);
		if (maxHeight < 0f)
		{
			return measuredHeight;
		}
		return Mathf.Min(measuredHeight, maxHeight);
	}

	public static void ClampSize(RectTransform rect, ref float width, ref float height)
	{
		width = ClampWidth(rect, width);
		height = ClampHeight(rect, height);
	}

	public static float GetSiblingLayoutProperty(RectTransform rect, ILayoutElement self, Func<ILayoutElement, float> property, float defaultValue = 0f)
	{
		if ((Object)(object)rect == (Object)null)
		{
			return defaultValue;
		}
		float num = defaultValue;
		int num2 = int.MinValue;
		List<Component> list = CollectionPool<List<Component>, Component>.Get();
		((Component)rect).GetComponents(typeof(ILayoutElement), list);
		int count = list.Count;
		for (int i = 0; i < count; i++)
		{
			Component obj = list[i];
			ILayoutElement val = (ILayoutElement)(object)((obj is ILayoutElement) ? obj : null);
			if (val == null || val == self)
			{
				continue;
			}
			Behaviour val2 = (Behaviour)(object)((val is Behaviour) ? val : null);
			if (val2 != null && !val2.isActiveAndEnabled)
			{
				continue;
			}
			int layoutPriority = val.layoutPriority;
			if (layoutPriority < num2)
			{
				continue;
			}
			float num3 = property(val);
			if (!(num3 < 0f))
			{
				if (layoutPriority > num2)
				{
					num = num3;
					num2 = layoutPriority;
				}
				else if (num3 > num)
				{
					num = num3;
				}
			}
		}
		CollectionPool<List<Component>, Component>.Release(list);
		return num;
	}

	public static float GetSiblingPreferredWidth(RectTransform rect, ILayoutElement self)
	{
		return Mathf.Max(GetSiblingLayoutProperty(rect, self, (ILayoutElement e) => e.minWidth), GetSiblingLayoutProperty(rect, self, (ILayoutElement e) => e.preferredWidth));
	}

	public static float GetSiblingPreferredHeight(RectTransform rect, ILayoutElement self)
	{
		return Mathf.Max(GetSiblingLayoutProperty(rect, self, (ILayoutElement e) => e.minHeight), GetSiblingLayoutProperty(rect, self, (ILayoutElement e) => e.preferredHeight));
	}

	private static float GetMaxProperty(RectTransform rect, Func<IWSGLayoutElement, float> property)
	{
		if ((Object)(object)rect == (Object)null)
		{
			return -1f;
		}
		float num = -1f;
		int num2 = int.MinValue;
		List<Component> list = CollectionPool<List<Component>, Component>.Get();
		((Component)rect).GetComponents(typeof(IWSGLayoutElement), list);
		int count = list.Count;
		for (int i = 0; i < count; i++)
		{
			if (!(list[i] is IWSGLayoutElement iWSGLayoutElement))
			{
				continue;
			}
			Behaviour val = (Behaviour)((iWSGLayoutElement is Behaviour) ? iWSGLayoutElement : null);
			if (val != null && !val.isActiveAndEnabled)
			{
				continue;
			}
			float num3 = property(iWSGLayoutElement);
			if (!(num3 < 0f))
			{
				ILayoutElement val2 = (ILayoutElement)((iWSGLayoutElement is ILayoutElement) ? iWSGLayoutElement : null);
				int num4 = ((val2 == null) ? 1 : val2.layoutPriority);
				if (num4 > num2)
				{
					num = num3;
					num2 = num4;
				}
				else if (num4 == num2 && num3 < num)
				{
					num = num3;
				}
			}
		}
		CollectionPool<List<Component>, Component>.Release(list);
		return num;
	}

	private static float GetMinProperty(RectTransform rect, Func<ILayoutElement, float> property)
	{
		if ((Object)(object)rect == (Object)null)
		{
			return -1f;
		}
		float num = -1f;
		int num2 = int.MinValue;
		List<Component> list = CollectionPool<List<Component>, Component>.Get();
		((Component)rect).GetComponents(typeof(ILayoutElement), list);
		int count = list.Count;
		for (int i = 0; i < count; i++)
		{
			Component obj = list[i];
			ILayoutElement val = (ILayoutElement)(object)((obj is ILayoutElement) ? obj : null);
			if (val == null)
			{
				continue;
			}
			Behaviour val2 = (Behaviour)(object)((val is Behaviour) ? val : null);
			if (val2 != null && !val2.isActiveAndEnabled)
			{
				continue;
			}
			float num3 = property(val);
			if (!(num3 < 0f))
			{
				int layoutPriority = val.layoutPriority;
				if (layoutPriority > num2)
				{
					num = num3;
					num2 = layoutPriority;
				}
				else if (layoutPriority == num2 && num3 > num)
				{
					num = num3;
				}
			}
		}
		CollectionPool<List<Component>, Component>.Release(list);
		return num;
	}
}
