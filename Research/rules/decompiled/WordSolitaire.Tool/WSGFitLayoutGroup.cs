using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace WordSolitaire.Tool;

[ExecuteAlways]
[AddComponentMenu("Layout/WSG Fit Layout Group", 154)]
public class WSGFitLayoutGroup : LayoutGroup
{
	public enum FlowAxis
	{
		Vertical,
		Horizontal
	}

	public enum FlowCorner
	{
		UpperLeft,
		UpperRight,
		LowerLeft,
		LowerRight
	}

	[Serializable]
	public struct CellTemplateItem
	{
		public RectTransform template;

		public int count;
	}

	[Serializable]
	private struct GroupSettings
	{
		public bool useGroup;

		public float groupSpacingX;

		public float groupSpacingY;
	}

	private struct MeasuredCell
	{
		public RectTransform Rect;

		public float Width;

		public float Height;
	}

	private struct Line
	{
		public float MainTotal;

		public float CrossMax;

		public int StartIndex;

		public int Count;
	}

	private struct TrackedCellState
	{
		public bool Active;

		public Vector2 SizeDelta;
	}

	[SerializeField]
	private FlowAxis startAxis;

	[SerializeField]
	private FlowCorner startCorner;

	[SerializeField]
	private Vector2 spacing = Vector2.zero;

	[SerializeField]
	private GroupSettings groupSettings;

	[SerializeField]
	private List<CellTemplateItem> cellItems = new List<CellTemplateItem>();

	private readonly HashSet<int> groupBreaks = new HashSet<int>();

	private readonly List<MeasuredCell> measuredCells = new List<MeasuredCell>();

	private readonly List<Line> lines = new List<Line>();

	private float totalExpandSize;

	private readonly Dictionary<RectTransform, Vector2> _naturalSizes = new Dictionary<RectTransform, Vector2>();

	private DrivenRectTransformTracker transientSizeTracker;

	private WSGContentSizeFitter _contentSizeFitter;

	private float _fixedAxisContentAvailable = -1f;

	private float _resolvedFixedAxisOuterSize = -1f;

	private bool _didExpandFixedAxis;

	private readonly List<TrackedCellState> trackedStates = new List<TrackedCellState>();

	private int _lastChildCount;

	public Vector2 Spacing
	{
		get
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			return spacing;
		}
		set
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			spacing = value;
			SetDirty();
		}
	}

	public FlowAxis StartAxis
	{
		get
		{
			return startAxis;
		}
		set
		{
			startAxis = value;
			SetDirty();
		}
	}

	public FlowCorner StartCorner
	{
		get
		{
			return startCorner;
		}
		set
		{
			startCorner = value;
			SetDirty();
		}
	}

	public List<CellTemplateItem> CellItems
	{
		get
		{
			return cellItems;
		}
		set
		{
			cellItems = value;
			SetDirty();
		}
	}

	public bool UseGroup
	{
		get
		{
			return groupSettings.useGroup;
		}
		set
		{
			groupSettings.useGroup = value;
			SetDirty();
		}
	}

	public Vector2 GroupSpacing
	{
		get
		{
			//IL_0016: Unknown result type (might be due to invalid IL or missing references)
			return new Vector2(groupSettings.groupSpacingX, groupSettings.groupSpacingY);
		}
		set
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_0017: Unknown result type (might be due to invalid IL or missing references)
			groupSettings.groupSpacingX = value.x;
			groupSettings.groupSpacingY = value.y;
			SetDirty();
		}
	}

	public void SetGroups(IList<int> breaks)
	{
		groupBreaks.Clear();
		if (breaks != null)
		{
			foreach (int @break in breaks)
			{
				groupBreaks.Add(@break);
			}
		}
		SetDirty();
	}

	private void OnInspectorLayoutChanged()
	{
		SetDirty();
	}

	private int GetFixedAxis()
	{
		if (startAxis != FlowAxis.Vertical)
		{
			return 1;
		}
		return 0;
	}

	private int GetFlowAxis()
	{
		if (startAxis != FlowAxis.Vertical)
		{
			return 0;
		}
		return 1;
	}

	private bool IsAxisHandledByContentSizeFitter(int axis)
	{
		if ((Object)(object)_contentSizeFitter == (Object)null)
		{
			_contentSizeFitter = ((Component)this).GetComponent<WSGContentSizeFitter>();
		}
		if ((Object)(object)_contentSizeFitter == (Object)null || !((Behaviour)_contentSizeFitter).isActiveAndEnabled)
		{
			return false;
		}
		return _contentSizeFitter.IsFittingAxis((Axis)axis);
	}

	private bool IsAxisPreferredSize(int axis)
	{
		if ((Object)(object)_contentSizeFitter == (Object)null)
		{
			_contentSizeFitter = ((Component)this).GetComponent<WSGContentSizeFitter>();
		}
		if ((Object)(object)_contentSizeFitter == (Object)null || !((Behaviour)_contentSizeFitter).isActiveAndEnabled)
		{
			return false;
		}
		return _contentSizeFitter.IsPreferredSizeAxis((Axis)axis);
	}

	private bool IsAxisMinSize(int axis)
	{
		if ((Object)(object)_contentSizeFitter == (Object)null)
		{
			_contentSizeFitter = ((Component)this).GetComponent<WSGContentSizeFitter>();
		}
		if ((Object)(object)_contentSizeFitter == (Object)null || !((Behaviour)_contentSizeFitter).isActiveAndEnabled)
		{
			return false;
		}
		return _contentSizeFitter.IsMinSizeAxis((Axis)axis);
	}

	private static float ClampOuterSize(float outer, float minOuter, float maxOuter)
	{
		if (minOuter >= 0f)
		{
			outer = Mathf.Max(outer, minOuter);
		}
		if (maxOuter >= 0f)
		{
			outer = Mathf.Min(outer, maxOuter);
		}
		return outer;
	}

	private bool WillDriveFixedAxisThisFrame()
	{
		if (_didExpandFixedAxis && !IsAxisHandledByContentSizeFitter(GetFixedAxis()))
		{
			return ((LayoutGroup)this).rectChildren.Count > 0;
		}
		return false;
	}

	private void MeasureCells()
	{
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_00da: Unknown result type (might be due to invalid IL or missing references)
		//IL_00df: Unknown result type (might be due to invalid IL or missing references)
		//IL_010e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0113: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f3: Unknown result type (might be due to invalid IL or missing references)
		//IL_013a: Unknown result type (might be due to invalid IL or missing references)
		//IL_013f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0143: Unknown result type (might be due to invalid IL or missing references)
		//IL_0148: Unknown result type (might be due to invalid IL or missing references)
		//IL_014a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0124: Unknown result type (might be due to invalid IL or missing references)
		//IL_0129: Unknown result type (might be due to invalid IL or missing references)
		//IL_0165: Unknown result type (might be due to invalid IL or missing references)
		//IL_015a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0187: Unknown result type (might be due to invalid IL or missing references)
		//IL_0175: Unknown result type (might be due to invalid IL or missing references)
		measuredCells.Clear();
		CleanupNaturalSizes();
		RectTransform val;
		float num;
		float num2;
		for (int i = 0; i < ((LayoutGroup)this).rectChildren.Count; measuredCells.Add(new MeasuredCell
		{
			Rect = val,
			Width = Mathf.Max(num, 0f),
			Height = Mathf.Max(num2, 0f)
		}), i++)
		{
			val = ((LayoutGroup)this).rectChildren[i];
			Rect rect;
			if (!_naturalSizes.TryGetValue(val, out var value))
			{
				rect = val.rect;
				value = ((Rect)(ref rect)).size;
				if (value.x <= 0f)
				{
					value.x = Mathf.Abs(val.sizeDelta.x);
				}
				if (value.y <= 0f)
				{
					value.y = Mathf.Abs(val.sizeDelta.y);
				}
				_naturalSizes[val] = value;
			}
			num = LayoutUtility.GetPreferredWidth(val);
			if (num <= 0f)
			{
				rect = val.rect;
				num = ((Rect)(ref rect)).width;
			}
			if (num <= 0f)
			{
				num = value.x;
			}
			num2 = LayoutUtility.GetPreferredHeight(val);
			if (num2 <= 0f)
			{
				rect = val.rect;
				num2 = ((Rect)(ref rect)).height;
			}
			if (num2 <= 0f)
			{
				num2 = value.y;
			}
			num = WSGLayoutUtility.ClampWidth(val, num);
			num2 = WSGLayoutUtility.ClampHeight(val, num2);
			rect = val.rect;
			if (!(((Rect)(ref rect)).width > 0f))
			{
				rect = val.rect;
				if (!(((Rect)(ref rect)).height > 0f))
				{
					continue;
				}
			}
			rect = val.rect;
			Vector2 size = ((Rect)(ref rect)).size;
			if (size.x <= 0f)
			{
				size.x = value.x;
			}
			if (size.y <= 0f)
			{
				size.y = value.y;
			}
			_naturalSizes[val] = size;
		}
	}

	private void CleanupNaturalSizes()
	{
		if (_naturalSizes.Count <= ((LayoutGroup)this).rectChildren.Count * 2)
		{
			return;
		}
		List<RectTransform> list = new List<RectTransform>();
		foreach (KeyValuePair<RectTransform, Vector2> naturalSize in _naturalSizes)
		{
			if ((Object)(object)naturalSize.Key == (Object)null)
			{
				list.Add(naturalSize.Key);
			}
		}
		foreach (RectTransform item in list)
		{
			_naturalSizes.Remove(item);
		}
	}

	public void UpdateNaturalSize(RectTransform child)
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		if (!((Object)(object)child == (Object)null))
		{
			Rect rect = child.rect;
			Vector2 size = ((Rect)(ref rect)).size;
			if (size.x <= 0f)
			{
				size.x = Mathf.Abs(child.sizeDelta.x);
			}
			if (size.y <= 0f)
			{
				size.y = Mathf.Abs(child.sizeDelta.y);
			}
			_naturalSizes[child] = size;
		}
	}

	public void ClearNaturalSizes()
	{
		_naturalSizes.Clear();
	}

	private void FlowVertical(float availableWidth)
	{
		lines.Clear();
		totalExpandSize = 0f;
		if (measuredCells.Count == 0)
		{
			return;
		}
		int num = 0;
		float num2 = 0f;
		float mainTotal = 0f;
		float num3 = 0f;
		for (int i = 0; i < measuredCells.Count; i++)
		{
			MeasuredCell measuredCell = measuredCells[i];
			float gap = GetGap(i, spacing.x, 0);
			if (ShouldStartNewLine(num, i, num2, gap, measuredCell.Width, availableWidth))
			{
				lines.Add(new Line
				{
					MainTotal = mainTotal,
					CrossMax = num3,
					StartIndex = num,
					Count = i - num
				});
				totalExpandSize += num3;
				if (lines.Count > 1)
				{
					totalExpandSize += GetLineGap(i, spacing.y, 1);
				}
				num = i;
				num2 = 0f;
				mainTotal = 0f;
				num3 = 0f;
			}
			float num4 = ((i > num) ? gap : 0f);
			num2 += num4 + measuredCell.Width;
			mainTotal = num2;
			if (measuredCell.Height > num3)
			{
				num3 = measuredCell.Height;
			}
		}
		if (num < measuredCells.Count)
		{
			lines.Add(new Line
			{
				MainTotal = mainTotal,
				CrossMax = num3,
				StartIndex = num,
				Count = measuredCells.Count - num
			});
			totalExpandSize += num3;
			if (lines.Count > 1)
			{
				totalExpandSize += GetLineGap(num, spacing.y, 1);
			}
		}
	}

	private void FlowHorizontal(float availableHeight)
	{
		lines.Clear();
		totalExpandSize = 0f;
		if (measuredCells.Count == 0)
		{
			return;
		}
		int num = 0;
		float num2 = 0f;
		float mainTotal = 0f;
		float num3 = 0f;
		for (int i = 0; i < measuredCells.Count; i++)
		{
			MeasuredCell measuredCell = measuredCells[i];
			float gap = GetGap(i, spacing.y, 1);
			if (ShouldStartNewLine(num, i, num2, gap, measuredCell.Height, availableHeight))
			{
				lines.Add(new Line
				{
					MainTotal = mainTotal,
					CrossMax = num3,
					StartIndex = num,
					Count = i - num
				});
				totalExpandSize += num3;
				if (lines.Count > 1)
				{
					totalExpandSize += GetLineGap(i, spacing.x, 0);
				}
				num = i;
				num2 = 0f;
				mainTotal = 0f;
				num3 = 0f;
			}
			float num4 = ((i > num) ? gap : 0f);
			num2 += num4 + measuredCell.Height;
			mainTotal = num2;
			if (measuredCell.Width > num3)
			{
				num3 = measuredCell.Width;
			}
		}
		if (num < measuredCells.Count)
		{
			lines.Add(new Line
			{
				MainTotal = mainTotal,
				CrossMax = num3,
				StartIndex = num,
				Count = measuredCells.Count - num
			});
			totalExpandSize += num3;
			if (lines.Count > 1)
			{
				totalExpandSize += GetLineGap(num, spacing.x, 0);
			}
		}
	}

	private bool ShouldStartNewLine(int lineStart, int index, float cursor, float gap, float cellMainSize, float available)
	{
		if (index <= lineStart)
		{
			return false;
		}
		if (float.IsPositiveInfinity(available))
		{
			return false;
		}
		return cursor + gap + cellMainSize > available + 0.001f;
	}

	private float GetMaxLineMainTotal()
	{
		float num = 0f;
		for (int i = 0; i < lines.Count; i++)
		{
			if (lines[i].MainTotal > num)
			{
				num = lines[i].MainTotal;
			}
		}
		return num;
	}

	private void GetFixedAxisBounds(int axis, out float minOuter, out float maxOuter, out float currentOuter)
	{
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		RectTransform rectTransform = ((LayoutGroup)this).rectTransform;
		Rect rect;
		if (axis == 0)
		{
			minOuter = WSGLayoutUtility.GetMinWidth(rectTransform);
			maxOuter = WSGLayoutUtility.GetMaxWidth(rectTransform);
			rect = rectTransform.rect;
			currentOuter = ((Rect)(ref rect)).width;
		}
		else
		{
			minOuter = WSGLayoutUtility.GetMinHeight(rectTransform);
			maxOuter = WSGLayoutUtility.GetMaxHeight(rectTransform);
			rect = rectTransform.rect;
			currentOuter = ((Rect)(ref rect)).height;
		}
	}

	private float GetRequiredFixedAxisContentSize()
	{
		if (startAxis != FlowAxis.Vertical)
		{
			return totalExpandSize;
		}
		return GetMaxLineMainTotal();
	}

	private float MeasureIdealFixedAxisContentSize()
	{
		if (measuredCells.Count == 0)
		{
			return 0f;
		}
		if (startAxis == FlowAxis.Vertical)
		{
			FlowVertical(float.PositiveInfinity);
			return GetMaxLineMainTotal();
		}
		FlowHorizontal(float.PositiveInfinity);
		return totalExpandSize;
	}

	private void FlowWithFixedAxisContent(float contentSize)
	{
		if (startAxis == FlowAxis.Vertical)
		{
			FlowVertical(contentSize);
		}
		else
		{
			FlowHorizontal(contentSize);
		}
	}

	private static float ResolveTargetFixedAxisOuterSize(float requiredContent, float minOuter, float maxOuter, float paddingSum)
	{
		float num = requiredContent + paddingSum;
		if (minOuter >= 0f)
		{
			num = Mathf.Max(num, minOuter);
		}
		if (maxOuter >= 0f)
		{
			num = Mathf.Min(num, maxOuter);
		}
		return num;
	}

	private float GetPaddingSumOnAxis(int axis)
	{
		return (axis == 0) ? (((LayoutGroup)this).padding.left + ((LayoutGroup)this).padding.right) : (((LayoutGroup)this).padding.top + ((LayoutGroup)this).padding.bottom);
	}

	private void PerformFlowLayout()
	{
		_fixedAxisContentAvailable = -1f;
		_resolvedFixedAxisOuterSize = -1f;
		_didExpandFixedAxis = false;
		int fixedAxis = GetFixedAxis();
		GetFixedAxisBounds(fixedAxis, out var minOuter, out var maxOuter, out var currentOuter);
		float paddingSumOnAxis = GetPaddingSumOnAxis(fixedAxis);
		float num = currentOuter - paddingSumOnAxis;
		if (!IsAxisHandledByContentSizeFitter(fixedAxis))
		{
			FlowWithFixedAxisContent(num);
			_resolvedFixedAxisOuterSize = currentOuter;
			_fixedAxisContentAvailable = num;
			return;
		}
		if (IsAxisMinSize(fixedAxis))
		{
			float num2 = ((minOuter >= 0f) ? (minOuter - paddingSumOnAxis) : num);
			if (maxOuter >= 0f)
			{
				num2 = Mathf.Min(num2, maxOuter - paddingSumOnAxis);
			}
			FlowWithFixedAxisContent(num2);
			_resolvedFixedAxisOuterSize = ((minOuter >= 0f) ? minOuter : currentOuter);
			_resolvedFixedAxisOuterSize = ClampOuterSize(_resolvedFixedAxisOuterSize, -1f, maxOuter);
			_fixedAxisContentAvailable = _resolvedFixedAxisOuterSize - paddingSumOnAxis;
			return;
		}
		float num3 = ((measuredCells.Count == 0) ? 0f : MeasureIdealFixedAxisContentSize());
		if (maxOuter >= 0f)
		{
			num3 = Mathf.Min(num3, maxOuter - paddingSumOnAxis);
		}
		if (minOuter >= 0f)
		{
			num3 = Mathf.Max(num3, minOuter - paddingSumOnAxis);
		}
		FlowWithFixedAxisContent(num3);
		float requiredContent = ((measuredCells.Count == 0) ? 0f : GetRequiredFixedAxisContentSize());
		_resolvedFixedAxisOuterSize = ResolveTargetFixedAxisOuterSize(requiredContent, minOuter, maxOuter, paddingSumOnAxis);
		_fixedAxisContentAvailable = _resolvedFixedAxisOuterSize - paddingSumOnAxis;
		_didExpandFixedAxis = Mathf.Abs(_resolvedFixedAxisOuterSize - currentOuter) > 0.001f;
	}

	private float GetReportedFixedAxisOuterSize(int axis)
	{
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		Rect rect;
		if (GetFixedAxis() != axis)
		{
			if (axis != 0)
			{
				rect = ((LayoutGroup)this).rectTransform.rect;
				return ((Rect)(ref rect)).height;
			}
			rect = ((LayoutGroup)this).rectTransform.rect;
			return ((Rect)(ref rect)).width;
		}
		if (!IsAxisHandledByContentSizeFitter(axis))
		{
			if (axis != 0)
			{
				rect = ((LayoutGroup)this).rectTransform.rect;
				return ((Rect)(ref rect)).height;
			}
			rect = ((LayoutGroup)this).rectTransform.rect;
			return ((Rect)(ref rect)).width;
		}
		if (IsAxisMinSize(axis))
		{
			GetFixedAxisBounds(axis, out var minOuter, out var maxOuter, out var currentOuter);
			if (minOuter < 0f)
			{
				return currentOuter;
			}
			return ClampOuterSize(minOuter, -1f, maxOuter);
		}
		if (_resolvedFixedAxisOuterSize >= 0f)
		{
			return _resolvedFixedAxisOuterSize;
		}
		if (axis != 0)
		{
			rect = ((LayoutGroup)this).rectTransform.rect;
			return ((Rect)(ref rect)).height;
		}
		rect = ((LayoutGroup)this).rectTransform.rect;
		return ((Rect)(ref rect)).width;
	}

	private float GetReportedFlowAxisOuterSize(int axis)
	{
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		Rect rect;
		if (!IsAxisHandledByContentSizeFitter(axis))
		{
			if (axis != 0)
			{
				rect = ((LayoutGroup)this).rectTransform.rect;
				return ((Rect)(ref rect)).height;
			}
			rect = ((LayoutGroup)this).rectTransform.rect;
			return ((Rect)(ref rect)).width;
		}
		if (IsAxisMinSize(axis))
		{
			GetFixedAxisBounds(axis, out var minOuter, out var maxOuter, out var currentOuter);
			if (minOuter < 0f)
			{
				return currentOuter;
			}
			return ClampOuterSize(minOuter, -1f, maxOuter);
		}
		if (measuredCells.Count > 0)
		{
			return totalExpandSize + GetPaddingSumOnAxis(axis);
		}
		if (axis != 0)
		{
			rect = ((LayoutGroup)this).rectTransform.rect;
			return ((Rect)(ref rect)).height;
		}
		rect = ((LayoutGroup)this).rectTransform.rect;
		return ((Rect)(ref rect)).width;
	}

	private float GetFixedAxisContentAvailable()
	{
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		if (_fixedAxisContentAvailable >= 0f)
		{
			return _fixedAxisContentAvailable;
		}
		int fixedAxis = GetFixedAxis();
		Rect rect;
		float num;
		if (fixedAxis != 0)
		{
			rect = ((LayoutGroup)this).rectTransform.rect;
			num = ((Rect)(ref rect)).height;
		}
		else
		{
			rect = ((LayoutGroup)this).rectTransform.rect;
			num = ((Rect)(ref rect)).width;
		}
		return num - GetPaddingSumOnAxis(fixedAxis);
	}

	private void SetContainerAxisSize(int axis, float size)
	{
		((DrivenRectTransformTracker)(ref transientSizeTracker)).Add((Object)(object)this, ((LayoutGroup)this).rectTransform, (DrivenTransformProperties)((axis == 0) ? 4096 : 8192));
		((LayoutGroup)this).rectTransform.SetSizeWithCurrentAnchors((Axis)axis, size);
	}

	private void ClearTransientContainerSize()
	{
		((DrivenRectTransformTracker)(ref transientSizeTracker)).Clear();
	}

	private float GetGap(int index, float normalGap, int axis)
	{
		if (!groupSettings.useGroup)
		{
			return normalGap;
		}
		if (index > 0 && groupBreaks.Contains(index - 1))
		{
			if (axis != 0)
			{
				return groupSettings.groupSpacingY;
			}
			return groupSettings.groupSpacingX;
		}
		return normalGap;
	}

	private float GetLineGap(int lineStartIndex, float normalGap, int axis)
	{
		if (!groupSettings.useGroup || lineStartIndex <= 0)
		{
			return normalGap;
		}
		if (groupBreaks.Contains(lineStartIndex - 1))
		{
			if (axis != 0)
			{
				return groupSettings.groupSpacingY;
			}
			return groupSettings.groupSpacingX;
		}
		return normalGap;
	}

	public override void CalculateLayoutInputHorizontal()
	{
		((LayoutGroup)this).CalculateLayoutInputHorizontal();
		MeasureCells();
		PerformFlowLayout();
		if (startAxis == FlowAxis.Vertical)
		{
			float reportedFixedAxisOuterSize = GetReportedFixedAxisOuterSize(0);
			((LayoutGroup)this).SetLayoutInputForAxis(reportedFixedAxisOuterSize, reportedFixedAxisOuterSize, -1f, 0);
		}
		else
		{
			float reportedFlowAxisOuterSize = GetReportedFlowAxisOuterSize(0);
			((LayoutGroup)this).SetLayoutInputForAxis(reportedFlowAxisOuterSize, reportedFlowAxisOuterSize, -1f, 0);
		}
	}

	public override void CalculateLayoutInputVertical()
	{
		if (startAxis == FlowAxis.Vertical)
		{
			float reportedFlowAxisOuterSize = GetReportedFlowAxisOuterSize(1);
			((LayoutGroup)this).SetLayoutInputForAxis(reportedFlowAxisOuterSize, reportedFlowAxisOuterSize, -1f, 1);
		}
		else
		{
			float reportedFixedAxisOuterSize = GetReportedFixedAxisOuterSize(1);
			((LayoutGroup)this).SetLayoutInputForAxis(reportedFixedAxisOuterSize, reportedFixedAxisOuterSize, -1f, 1);
		}
	}

	public override void SetLayoutHorizontal()
	{
		if (!WillDriveFixedAxisThisFrame())
		{
			ClearTransientContainerSize();
		}
		if (startAxis == FlowAxis.Vertical)
		{
			if (WillDriveFixedAxisThisFrame())
			{
				SetContainerAxisSize(0, _resolvedFixedAxisOuterSize);
			}
			LayoutVerticalHorizontal();
		}
		else
		{
			LayoutHorizontalHorizontal();
		}
	}

	public override void SetLayoutVertical()
	{
		if (!WillDriveFixedAxisThisFrame())
		{
			ClearTransientContainerSize();
		}
		if (startAxis == FlowAxis.Vertical)
		{
			LayoutVerticalVertical();
			return;
		}
		if (WillDriveFixedAxisThisFrame())
		{
			SetContainerAxisSize(1, _resolvedFixedAxisOuterSize);
		}
		LayoutHorizontalVertical();
	}

	private void LayoutVerticalHorizontal()
	{
		float fixedAxisContentAvailable = GetFixedAxisContentAvailable();
		bool flag = startCorner == FlowCorner.UpperLeft || startCorner == FlowCorner.LowerLeft;
		for (int i = 0; i < lines.Count; i++)
		{
			Line line = lines[i];
			float alignmentOffset = GetAlignmentOffset(fixedAxisContentAvailable, line.MainTotal, 0);
			float num = (float)((LayoutGroup)this).padding.left + alignmentOffset;
			if (flag)
			{
				float num2 = 0f;
				for (int j = 0; j < line.Count; j++)
				{
					int index = line.StartIndex + j;
					MeasuredCell measuredCell = measuredCells[index];
					float num3 = ((j > 0) ? GetGap(index, spacing.x, 0) : 0f);
					num2 += num3;
					((LayoutGroup)this).SetChildAlongAxis(measuredCell.Rect, 0, num + num2);
					num2 += measuredCell.Width;
				}
				continue;
			}
			float num4 = 0f;
			for (int k = 0; k < line.Count; k++)
			{
				int index2 = line.StartIndex + k;
				MeasuredCell measuredCell2 = measuredCells[index2];
				float num5 = ((k > 0) ? GetGap(index2, spacing.x, 0) : 0f);
				num4 += num5;
				float num6 = num + line.MainTotal - num4 - measuredCell2.Width;
				((LayoutGroup)this).SetChildAlongAxis(measuredCell2.Rect, 0, num6);
				num4 += measuredCell2.Width;
			}
		}
	}

	private void LayoutVerticalVertical()
	{
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		bool flag = startCorner == FlowCorner.UpperLeft || startCorner == FlowCorner.UpperRight;
		Rect rect = ((LayoutGroup)this).rectTransform.rect;
		float height = ((Rect)(ref rect)).height;
		float num = 0f;
		for (int i = 0; i < lines.Count; i++)
		{
			Line line = lines[i];
			float num2 = ((i > 0) ? GetLineGap(line.StartIndex, spacing.y, 1) : 0f);
			num += num2;
			float crossMax = line.CrossMax;
			for (int j = 0; j < line.Count; j++)
			{
				int index = line.StartIndex + j;
				MeasuredCell measuredCell = measuredCells[index];
				float num3 = ((!flag) ? (height - (float)((LayoutGroup)this).padding.bottom - num - crossMax) : ((float)((LayoutGroup)this).padding.top + num));
				((LayoutGroup)this).SetChildAlongAxis(measuredCell.Rect, 1, num3);
			}
			num += crossMax;
		}
	}

	private void LayoutHorizontalHorizontal()
	{
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		bool flag = startCorner == FlowCorner.UpperLeft || startCorner == FlowCorner.LowerLeft;
		Rect rect = ((LayoutGroup)this).rectTransform.rect;
		float width = ((Rect)(ref rect)).width;
		float num = 0f;
		for (int i = 0; i < lines.Count; i++)
		{
			Line line = lines[i];
			float num2 = ((i > 0) ? GetLineGap(line.StartIndex, spacing.x, 0) : 0f);
			num += num2;
			float crossMax = line.CrossMax;
			for (int j = 0; j < line.Count; j++)
			{
				int index = line.StartIndex + j;
				MeasuredCell measuredCell = measuredCells[index];
				float num3 = ((!flag) ? (width - (float)((LayoutGroup)this).padding.right - num - crossMax) : ((float)((LayoutGroup)this).padding.left + num));
				((LayoutGroup)this).SetChildAlongAxis(measuredCell.Rect, 0, num3);
			}
			num += crossMax;
		}
	}

	private void LayoutHorizontalVertical()
	{
		float fixedAxisContentAvailable = GetFixedAxisContentAvailable();
		bool flag = startCorner == FlowCorner.UpperLeft || startCorner == FlowCorner.UpperRight;
		for (int i = 0; i < lines.Count; i++)
		{
			Line line = lines[i];
			float alignmentOffset = GetAlignmentOffset(fixedAxisContentAvailable, line.MainTotal, 1);
			float num = (float)((LayoutGroup)this).padding.top + alignmentOffset;
			if (flag)
			{
				float num2 = 0f;
				for (int j = 0; j < line.Count; j++)
				{
					int index = line.StartIndex + j;
					MeasuredCell measuredCell = measuredCells[index];
					float num3 = ((j > 0) ? GetGap(index, spacing.y, 1) : 0f);
					num2 += num3;
					((LayoutGroup)this).SetChildAlongAxis(measuredCell.Rect, 1, num + num2);
					num2 += measuredCell.Height;
				}
				continue;
			}
			float num4 = 0f;
			for (int k = 0; k < line.Count; k++)
			{
				int index2 = line.StartIndex + k;
				MeasuredCell measuredCell2 = measuredCells[index2];
				float num5 = ((k > 0) ? GetGap(index2, spacing.y, 1) : 0f);
				num4 += num5;
				float num6 = num + line.MainTotal - num4 - measuredCell2.Height;
				((LayoutGroup)this).SetChildAlongAxis(measuredCell2.Rect, 1, num6);
				num4 += measuredCell2.Height;
			}
		}
	}

	private float GetAlignmentOffset(float available, float content, int axis)
	{
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Invalid comparison between Unknown and I4
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Expected I4, but got Unknown
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Invalid comparison between Unknown and I4
		float num = available - content;
		if (num <= 0f)
		{
			return 0f;
		}
		TextAnchor childAlignment;
		if (axis == 0)
		{
			childAlignment = ((LayoutGroup)this).childAlignment;
			switch (childAlignment - 1)
			{
			case 0:
			case 3:
			case 6:
				return num * 0.5f;
			case 1:
			case 4:
			case 7:
				return num;
			default:
				return 0f;
			}
		}
		childAlignment = ((LayoutGroup)this).childAlignment;
		if (childAlignment - 3 > 2)
		{
			if (childAlignment - 6 <= 2)
			{
				return num;
			}
			return 0f;
		}
		return num * 0.5f;
	}

	public void Rebuild()
	{
		LayoutRebuilder.ForceRebuildLayoutImmediate(((LayoutGroup)this).rectTransform);
	}

	public List<RectTransform> CreateFromTemplate(int templateIndex, int count)
	{
		List<RectTransform> list = new List<RectTransform>();
		if (count <= 0)
		{
			return list;
		}
		if (!TryGetCellTemplate(templateIndex, out var template))
		{
			return list;
		}
		list.Capacity = count;
		for (int i = 0; i < count; i++)
		{
			RectTransform val = InstantiateCell(template);
			if ((Object)(object)val != (Object)null)
			{
				list.Add(val);
			}
		}
		if (list.Count > 0)
		{
			Rebuild();
		}
		return list;
	}

	public List<T> CreateFromTemplate<T>(int templateIndex, int count) where T : Component
	{
		List<RectTransform> list = CreateFromTemplate(templateIndex, count);
		List<T> list2 = new List<T>(list.Count);
		for (int i = 0; i < list.Count; i++)
		{
			T component = ((Component)list[i]).GetComponent<T>();
			if ((Object)(object)component != (Object)null)
			{
				list2.Add(component);
			}
		}
		return list2;
	}

	private bool TryGetCellTemplate(int templateIndex, out RectTransform template)
	{
		template = null;
		if (cellItems == null || cellItems.Count == 0)
		{
			return false;
		}
		if (templateIndex < 0 || templateIndex >= cellItems.Count)
		{
			return false;
		}
		template = cellItems[templateIndex].template;
		if ((Object)(object)template == (Object)null)
		{
			return false;
		}
		return true;
	}

	private RectTransform InstantiateCell(RectTransform template)
	{
		if ((Object)(object)template == (Object)null)
		{
			return null;
		}
		GameObject obj = Object.Instantiate<GameObject>(((Component)template).gameObject, ((Component)this).transform, false);
		obj.SetActive(true);
		Transform transform = obj.transform;
		return (RectTransform)(object)((transform is RectTransform) ? transform : null);
	}

	protected void SetDirty()
	{
		if (((UIBehaviour)this).IsActive())
		{
			if (!CanvasUpdateRegistry.IsRebuildingLayout())
			{
				LayoutRebuilder.MarkLayoutForRebuild(((LayoutGroup)this).rectTransform);
			}
			else
			{
				((MonoBehaviour)this).StartCoroutine(DelayedSetDirtyCoroutine());
			}
		}
	}

	private IEnumerator DelayedSetDirtyCoroutine()
	{
		yield return null;
		if (!((Object)(object)this == (Object)null))
		{
			LayoutRebuilder.MarkLayoutForRebuild(((LayoutGroup)this).rectTransform);
		}
	}

	protected override void OnRectTransformDimensionsChange()
	{
		((LayoutGroup)this).OnRectTransformDimensionsChange();
		SetDirty();
	}

	protected override void OnTransformChildrenChanged()
	{
		((LayoutGroup)this).OnTransformChildrenChanged();
		_lastChildCount = ((Component)this).transform.childCount;
		RecordNewChildSizes();
	}

	private void RecordNewChildSizes()
	{
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		for (int i = 0; i < ((Component)this).transform.childCount; i++)
		{
			Transform child = ((Component)this).transform.GetChild(i);
			RectTransform val = (RectTransform)(object)((child is RectTransform) ? child : null);
			if (!((Object)(object)val == (Object)null) && !_naturalSizes.ContainsKey(val))
			{
				Rect rect = val.rect;
				Vector2 size = ((Rect)(ref rect)).size;
				if (size.x <= 0f)
				{
					size.x = Mathf.Abs(val.sizeDelta.x);
				}
				if (size.y <= 0f)
				{
					size.y = Mathf.Abs(val.sizeDelta.y);
				}
				if (size.x > 0f || size.y > 0f)
				{
					_naturalSizes[val] = size;
				}
			}
		}
	}

	protected override void OnEnable()
	{
		((LayoutGroup)this).OnEnable();
		_contentSizeFitter = ((Component)this).GetComponent<WSGContentSizeFitter>();
		_lastChildCount = ((Component)this).transform.childCount;
		SnapshotTrackedStates();
	}

	protected override void OnDisable()
	{
		ClearTransientContainerSize();
		((LayoutGroup)this).OnDisable();
	}

	private void LateUpdate()
	{
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		bool flag = false;
		int childCount = ((Component)this).transform.childCount;
		if (childCount != _lastChildCount)
		{
			_lastChildCount = childCount;
			flag = true;
		}
		else if (trackedStates.Count != ((LayoutGroup)this).rectChildren.Count)
		{
			flag = true;
		}
		else
		{
			for (int i = 0; i < ((LayoutGroup)this).rectChildren.Count; i++)
			{
				RectTransform val = ((LayoutGroup)this).rectChildren[i];
				if ((Object)(object)val == (Object)null)
				{
					flag = true;
					break;
				}
				TrackedCellState trackedCellState = trackedStates[i];
				bool activeInHierarchy = ((Component)val).gameObject.activeInHierarchy;
				if (activeInHierarchy != trackedCellState.Active)
				{
					flag = true;
					break;
				}
				if (activeInHierarchy && val.sizeDelta != trackedCellState.SizeDelta)
				{
					UpdateNaturalSize(val);
					flag = true;
					break;
				}
			}
		}
		if (flag)
		{
			LayoutRebuilder.ForceRebuildLayoutImmediate(((LayoutGroup)this).rectTransform);
			SnapshotTrackedStates();
		}
	}

	private void SnapshotTrackedStates()
	{
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		trackedStates.Clear();
		for (int i = 0; i < ((LayoutGroup)this).rectChildren.Count; i++)
		{
			RectTransform val = ((LayoutGroup)this).rectChildren[i];
			if (!((Object)(object)val == (Object)null))
			{
				trackedStates.Add(new TrackedCellState
				{
					Active = ((Component)val).gameObject.activeInHierarchy,
					SizeDelta = val.sizeDelta
				});
			}
		}
	}
}
