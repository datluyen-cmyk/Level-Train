using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace XingYun.Render;

[ExecuteAlways]
public sealed class RingStyleLineRectMask2D : MonoBehaviour
{
	private struct MaskData
	{
		public Vector4 rect;

		public Vector4 softness;
	}

	[Tooltip("Optional explicit mask. When null, the nearest RectMask2D on this object or a parent is used.")]
	public RectMask2D overrideMask;

	[Tooltip("Re-scan children for new/removed StyleLine renderers every frame. Leave off for static hierarchies; call Rescan() manually after spawning children.")]
	public bool rescanEveryFrame;

	[Tooltip("Flip Y to match fragment SV_POSITION origin (top-left, D3D/Metal). Leave on for Windows/DX. Turn off if the mask appears vertically inverted.")]
	public bool flipY = true;

	private static readonly int IdRect = Shader.PropertyToID("_StyleLineRectMaskRect");

	private static readonly int IdSoftness = Shader.PropertyToID("_StyleLineRectMaskSoftness");

	private static readonly Vector4 DefaultOpenRect = new Vector4(-100000f, -100000f, 100000f, 100000f);

	private readonly List<Renderer> _managed = new List<Renderer>();

	private readonly List<Renderer> _scanBuffer = new List<Renderer>();

	private MaterialPropertyBlock _mpb;

	private RectMask2D _activeMask;

	private static readonly Vector3[] s_corners = (Vector3[])(object)new Vector3[4];

	private void OnEnable()
	{
		_activeMask = ResolveMask();
		Collect();
	}

	private void OnDisable()
	{
		ClearMaskOnAllManaged();
	}

	private void OnDestroy()
	{
		ClearMaskOnAllManaged();
	}

	private void LateUpdate()
	{
		RectMask2D val = ResolveMask();
		if ((Object)(object)val != (Object)(object)_activeMask)
		{
			ClearMaskOnAllManaged();
			_activeMask = val;
			Collect();
		}
		else if (rescanEveryFrame)
		{
			Collect();
		}
		else if (NeedsRecollect())
		{
			Collect();
		}
	}

	public void Rescan()
	{
		_activeMask = ResolveMask();
		Collect();
	}

	private RectMask2D ResolveMask()
	{
		if ((Object)(object)overrideMask != (Object)null)
		{
			if (!((Behaviour)overrideMask).enabled)
			{
				return null;
			}
			return overrideMask;
		}
		RectMask2D componentInParent = ((Component)this).GetComponentInParent<RectMask2D>();
		if (!((Object)(object)componentInParent != (Object)null) || !((Behaviour)componentInParent).enabled)
		{
			return null;
		}
		return componentInParent;
	}

	private bool NeedsRecollect()
	{
		for (int i = 0; i < _managed.Count; i++)
		{
			Renderer val = _managed[i];
			if ((Object)(object)val == (Object)null)
			{
				return true;
			}
			Material sharedMaterial = val.sharedMaterial;
			if ((Object)(object)sharedMaterial == (Object)null || !sharedMaterial.HasProperty(IdRect))
			{
				return true;
			}
		}
		return false;
	}

	private void Collect()
	{
		ClearMaskOnAllManaged();
		_managed.Clear();
		if ((Object)(object)_activeMask == (Object)null)
		{
			return;
		}
		_scanBuffer.Clear();
		((Component)this).GetComponentsInChildren<Renderer>(true, _scanBuffer);
		for (int i = 0; i < _scanBuffer.Count; i++)
		{
			Renderer val = _scanBuffer[i];
			if (!((Object)(object)val == (Object)null))
			{
				Material sharedMaterial = val.sharedMaterial;
				if (!((Object)(object)sharedMaterial == (Object)null) && sharedMaterial.HasProperty(IdRect) && (!((Object)(object)sharedMaterial.shader != (Object)null) || !(((Object)sharedMaterial.shader).name == "Hidden/HsApp/Effect/StyleLineUnderlay")))
				{
					_managed.Add(val);
				}
			}
		}
		PushMaskData();
	}

	private void ClearMaskOnAllManaged()
	{
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		if (_managed.Count == 0)
		{
			return;
		}
		EnsureMpb();
		for (int i = 0; i < _managed.Count; i++)
		{
			Renderer val = _managed[i];
			if (!((Object)(object)val == (Object)null))
			{
				val.GetPropertyBlock(_mpb);
				_mpb.SetVector(IdRect, DefaultOpenRect);
				_mpb.SetVector(IdSoftness, Vector4.zero);
				val.SetPropertyBlock(_mpb);
			}
		}
	}

	private void PushMaskData()
	{
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)_activeMask == (Object)null || _managed.Count == 0)
		{
			return;
		}
		MaskData maskData = ComputeMaskData(_activeMask);
		EnsureMpb();
		for (int i = 0; i < _managed.Count; i++)
		{
			Renderer val = _managed[i];
			if (!((Object)(object)val == (Object)null))
			{
				val.GetPropertyBlock(_mpb);
				_mpb.SetVector(IdRect, maskData.rect);
				_mpb.SetVector(IdSoftness, maskData.softness);
				val.SetPropertyBlock(_mpb);
			}
		}
	}

	private void EnsureMpb()
	{
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Expected O, but got Unknown
		if (_mpb == null)
		{
			_mpb = new MaterialPropertyBlock();
		}
	}

	private MaskData ComputeMaskData(RectMask2D mask)
	{
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_012e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0133: Unknown result type (might be due to invalid IL or missing references)
		//IL_0137: Unknown result type (might be due to invalid IL or missing references)
		//IL_0146: Unknown result type (might be due to invalid IL or missing references)
		//IL_0155: Unknown result type (might be due to invalid IL or missing references)
		//IL_0164: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bf: Unknown result type (might be due to invalid IL or missing references)
		RectTransform rectTransform = mask.rectTransform;
		Canvas componentInParent = ((Component)mask).GetComponentInParent<Canvas>();
		Camera val = null;
		if ((Object)(object)componentInParent != (Object)null && (int)componentInParent.renderMode != 0)
		{
			val = (((Object)(object)componentInParent.worldCamera != (Object)null) ? componentInParent.worldCamera : Camera.main);
		}
		Vector3[] array = s_corners;
		rectTransform.GetWorldCorners(array);
		Vector2 val2 = RectTransformUtility.WorldToScreenPoint(val, array[0]);
		Vector2 val3 = RectTransformUtility.WorldToScreenPoint(val, array[2]);
		float num = Mathf.Min(val2.x, val3.x);
		float num2 = Mathf.Max(val2.x, val3.x);
		float num3 = Mathf.Min(val2.y, val3.y);
		float num4 = Mathf.Max(val2.y, val3.y);
		Vector2Int softness = mask.softness;
		Rect rect = rectTransform.rect;
		float num5 = ((((Rect)(ref rect)).width > 0.0001f) ? ((num2 - num) / ((Rect)(ref rect)).width) : 1f);
		float num6 = ((((Rect)(ref rect)).height > 0.0001f) ? ((num4 - num3) / ((Rect)(ref rect)).height) : 1f);
		Vector4 softness2 = default(Vector4);
		((Vector4)(ref softness2))._002Ector((float)((Vector2Int)(ref softness)).x * num5, (float)((Vector2Int)(ref softness)).y * num6, 0f, 0f);
		Vector4 padding = mask.padding;
		num += padding.x * num5;
		num3 += padding.y * num6;
		num2 -= padding.z * num5;
		num4 -= padding.w * num6;
		if (flipY)
		{
			float num7 = (((Object)(object)val != (Object)null) ? val.pixelHeight : Screen.height);
			float num8 = num7 - num4;
			float num9 = num7 - num3;
			num3 = num8;
			num4 = num9;
		}
		return new MaskData
		{
			rect = new Vector4(num, num3, num2, num4),
			softness = softness2
		};
	}
}
