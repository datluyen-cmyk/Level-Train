using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace WordSolitaire.Tool;

[AddComponentMenu("Layout/WSG Layout Element", 141)]
[RequireComponent(typeof(RectTransform))]
[ExecuteAlways]
[DisallowMultipleComponent]
public class WSGLayoutElement : UIBehaviour, ILayoutElement, ILayoutIgnorer, IWSGLayoutElement
{
	[HideInInspector]
	[SerializeField]
	private bool m_IgnoreLayout;

	[HideInInspector]
	[SerializeField]
	private float m_MinWidth = -1f;

	[HideInInspector]
	[SerializeField]
	private float m_MinHeight = -1f;

	[HideInInspector]
	[SerializeField]
	private float m_MaxWidth = -1f;

	[HideInInspector]
	[SerializeField]
	private float m_MaxHeight = -1f;

	[HideInInspector]
	[SerializeField]
	private float m_PreferredWidth = -1f;

	[HideInInspector]
	[SerializeField]
	private float m_PreferredHeight = -1f;

	[HideInInspector]
	[SerializeField]
	private float m_FlexibleWidth = -1f;

	[HideInInspector]
	[SerializeField]
	private float m_FlexibleHeight = -1f;

	[HideInInspector]
	[SerializeField]
	private int m_LayoutPriority = 1;

	public virtual bool ignoreLayout
	{
		get
		{
			return m_IgnoreLayout;
		}
		set
		{
			if (m_IgnoreLayout != value)
			{
				m_IgnoreLayout = value;
				SetDirty();
			}
		}
	}

	public virtual float minWidth
	{
		get
		{
			return ClampAxisValue(m_MinWidth, m_MaxWidth);
		}
		set
		{
			if (!Mathf.Approximately(m_MinWidth, value))
			{
				m_MinWidth = value;
				SetDirty();
			}
		}
	}

	public virtual float minHeight
	{
		get
		{
			return ClampAxisValue(m_MinHeight, m_MaxHeight);
		}
		set
		{
			if (!Mathf.Approximately(m_MinHeight, value))
			{
				m_MinHeight = value;
				SetDirty();
			}
		}
	}

	public virtual float preferredWidth
	{
		get
		{
			return ResolvePreferredAxis(m_PreferredWidth, m_MaxWidth, horizontal: true);
		}
		set
		{
			if (!Mathf.Approximately(m_PreferredWidth, value))
			{
				m_PreferredWidth = value;
				SetDirty();
			}
		}
	}

	public virtual float preferredHeight
	{
		get
		{
			return ResolvePreferredAxis(m_PreferredHeight, m_MaxHeight, horizontal: false);
		}
		set
		{
			if (!Mathf.Approximately(m_PreferredHeight, value))
			{
				m_PreferredHeight = value;
				SetDirty();
			}
		}
	}

	public virtual float flexibleWidth
	{
		get
		{
			return m_FlexibleWidth;
		}
		set
		{
			if (!Mathf.Approximately(m_FlexibleWidth, value))
			{
				m_FlexibleWidth = value;
				SetDirty();
			}
		}
	}

	public virtual float flexibleHeight
	{
		get
		{
			return m_FlexibleHeight;
		}
		set
		{
			if (!Mathf.Approximately(m_FlexibleHeight, value))
			{
				m_FlexibleHeight = value;
				SetDirty();
			}
		}
	}

	public virtual int layoutPriority
	{
		get
		{
			return m_LayoutPriority;
		}
		set
		{
			if (m_LayoutPriority != value)
			{
				m_LayoutPriority = value;
				SetDirty();
			}
		}
	}

	public virtual float maxWidth
	{
		get
		{
			return m_MaxWidth;
		}
		set
		{
			if (!Mathf.Approximately(m_MaxWidth, value))
			{
				m_MaxWidth = value;
				SetDirty();
			}
		}
	}

	public virtual float maxHeight
	{
		get
		{
			return m_MaxHeight;
		}
		set
		{
			if (!Mathf.Approximately(m_MaxHeight, value))
			{
				m_MaxHeight = value;
				SetDirty();
			}
		}
	}

	private RectTransform LayoutRect
	{
		get
		{
			Transform transform = ((Component)this).transform;
			return (RectTransform)(object)((transform is RectTransform) ? transform : null);
		}
	}

	public virtual void CalculateLayoutInputHorizontal()
	{
	}

	public virtual void CalculateLayoutInputVertical()
	{
	}

	private float GetRectWidth()
	{
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		if (!((Object)(object)LayoutRect != (Object)null))
		{
			return 0f;
		}
		Rect rect = LayoutRect.rect;
		return ((Rect)(ref rect)).width;
	}

	private float GetRectHeight()
	{
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		if (!((Object)(object)LayoutRect != (Object)null))
		{
			return 0f;
		}
		Rect rect = LayoutRect.rect;
		return ((Rect)(ref rect)).height;
	}

	protected override void OnEnable()
	{
		((UIBehaviour)this).OnEnable();
		SetDirty();
	}

	protected override void OnTransformParentChanged()
	{
		SetDirty();
	}

	protected override void OnDisable()
	{
		SetDirty();
		((UIBehaviour)this).OnDisable();
	}

	protected override void OnDidApplyAnimationProperties()
	{
		SetDirty();
	}

	protected override void OnBeforeTransformParentChanged()
	{
		SetDirty();
	}

	protected void SetDirty()
	{
		if (((UIBehaviour)this).IsActive())
		{
			Transform transform = ((Component)this).transform;
			LayoutRebuilder.MarkLayoutForRebuild((RectTransform)(object)((transform is RectTransform) ? transform : null));
		}
	}

	private static float ClampAxisValue(float value, float max)
	{
		if (value < 0f || max < 0f)
		{
			return value;
		}
		return Mathf.Min(value, max);
	}

	private float ResolvePreferredAxis(float preferred, float max, bool horizontal)
	{
		if (preferred >= 0f)
		{
			return ClampAxisValue(preferred, max);
		}
		if (max < 0f)
		{
			return -1f;
		}
		RectTransform layoutRect = LayoutRect;
		if ((Object)(object)layoutRect == (Object)null)
		{
			return -1f;
		}
		float num = (horizontal ? WSGLayoutUtility.GetSiblingPreferredWidth(layoutRect, (ILayoutElement)(object)this) : WSGLayoutUtility.GetSiblingPreferredHeight(layoutRect, (ILayoutElement)(object)this));
		if (num <= 0f)
		{
			return -1f;
		}
		return Mathf.Min(num, max);
	}
}
