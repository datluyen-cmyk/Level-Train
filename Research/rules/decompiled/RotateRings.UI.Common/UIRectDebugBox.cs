using System;
using UnityEngine;
using UnityEngine.UI;

namespace RotateRings.UI.Common;

public sealed class UIRectDebugBox : MonoBehaviour
{
	private const float LineThickness = 4f;

	private static readonly Color LineColor = new Color(1f, 0.2f, 0.1f, 0.95f);

	private GameObject topLine;

	private GameObject bottomLine;

	private GameObject leftLine;

	private GameObject rightLine;

	private void Awake()
	{
		if (((Component)this).transform is RectTransform)
		{
			topLine = CreateLine("Top");
			bottomLine = CreateLine("Bottom");
			leftLine = CreateLine("Left");
			rightLine = CreateLine("Right");
		}
	}

	private GameObject CreateLine(string lineName)
	{
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Expected O, but got Unknown
		GameObject val = new GameObject(lineName, new Type[3]
		{
			typeof(RectTransform),
			typeof(CanvasRenderer),
			typeof(Image)
		});
		val.transform.SetParent(((Component)this).transform, false);
		Image component = val.GetComponent<Image>();
		((Graphic)component).color = LineColor;
		((Graphic)component).raycastTarget = false;
		RectTransform component2 = val.GetComponent<RectTransform>();
		((Transform)component2).localScale = Vector3.one;
		((Transform)component2).localRotation = Quaternion.identity;
		ConfigureLineRect(lineName, component2);
		return val;
	}

	private static void ConfigureLineRect(string lineName, RectTransform lineRect)
	{
		switch (lineName)
		{
		case "Top":
			SetHorizontalLine(lineRect, 1f);
			break;
		case "Bottom":
			SetHorizontalLine(lineRect, 0f);
			break;
		case "Left":
			SetVerticalLine(lineRect, 0f);
			break;
		case "Right":
			SetVerticalLine(lineRect, 1f);
			break;
		}
	}

	private static void SetHorizontalLine(RectTransform lineRect, float anchorY)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		lineRect.anchorMin = new Vector2(0f, anchorY);
		lineRect.anchorMax = new Vector2(1f, anchorY);
		lineRect.pivot = new Vector2(0.5f, anchorY);
		lineRect.sizeDelta = new Vector2(0f, 4f);
		lineRect.anchoredPosition = Vector2.zero;
	}

	private static void SetVerticalLine(RectTransform lineRect, float anchorX)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		lineRect.anchorMin = new Vector2(anchorX, 0f);
		lineRect.anchorMax = new Vector2(anchorX, 1f);
		lineRect.pivot = new Vector2(anchorX, 0.5f);
		lineRect.sizeDelta = new Vector2(4f, 0f);
		lineRect.anchoredPosition = Vector2.zero;
	}

	public void SetVisible(bool visible)
	{
		if ((Object)(object)topLine != (Object)null)
		{
			topLine.SetActive(visible);
		}
		if ((Object)(object)bottomLine != (Object)null)
		{
			bottomLine.SetActive(visible);
		}
		if ((Object)(object)leftLine != (Object)null)
		{
			leftLine.SetActive(visible);
		}
		if ((Object)(object)rightLine != (Object)null)
		{
			rightLine.SetActive(visible);
		}
	}

	private void OnDestroy()
	{
		DestroyLine(ref topLine);
		DestroyLine(ref bottomLine);
		DestroyLine(ref leftLine);
		DestroyLine(ref rightLine);
	}

	private static void DestroyLine(ref GameObject line)
	{
		if (!((Object)(object)line == (Object)null))
		{
			Object.Destroy((Object)(object)line);
			line = null;
		}
	}
}
