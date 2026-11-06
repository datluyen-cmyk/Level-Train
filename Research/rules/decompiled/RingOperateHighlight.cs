using UnityEngine;
using XingYun.Rendering;

public sealed class RingOperateHighlight
{
	internal static readonly Color OperateHighlightColor = new Color(0.5215686f, 1f, 0.9411765f, 1f);

	private const string KwOutlineOn = "_OUTLINE_ON";

	private static readonly int IdRectMaskRect = Shader.PropertyToID("_StyleLineRectMaskRect");

	private static readonly int IdRectMaskSoftness = Shader.PropertyToID("_StyleLineRectMaskSoftness");

	private readonly MeshRenderer _renderer;

	private readonly Material _sourceSharedMaterial;

	private readonly Color _outlineColor;

	private Material _runtimeMaterial;

	private MaterialPropertyBlock _maskMpbScratch;

	private bool _highlighted;

	private StyleLineUnderlayProxy underlayProxy;

	private RingOperateHighlight(MeshRenderer renderer, Material sourceSharedMaterial, Color outlineColor)
	{
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		_renderer = renderer;
		underlayProxy = ((Component)renderer).GetComponent<StyleLineUnderlayProxy>();
		_sourceSharedMaterial = sourceSharedMaterial;
		_outlineColor = outlineColor;
	}

	internal static RingOperateHighlight Create(MeshRenderer renderer, Color outlineColor)
	{
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)renderer == (Object)null || (Object)(object)((Renderer)renderer).sharedMaterial == (Object)null)
		{
			return null;
		}
		return new RingOperateHighlight(renderer, ((Renderer)renderer).sharedMaterial, outlineColor);
	}

	internal void SetHighlight(bool highlight)
	{
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)_renderer == (Object)null || _highlighted == highlight)
		{
			return;
		}
		_highlighted = highlight;
		if (highlight)
		{
			Vector4 rect;
			Vector4 softness;
			bool num = TryReadRectMaskFromRenderer(out rect, out softness);
			EnsureRuntimeMaterial();
			ApplyOperateOutline(_runtimeMaterial);
			((Renderer)_renderer).sharedMaterial = _runtimeMaterial;
			if (num)
			{
				ApplyRectMaskToMaterialAndRenderer(_runtimeMaterial, rect, softness);
			}
			StyleLineUnderlayProxy obj = underlayProxy;
			if (obj != null)
			{
				obj.Sync();
			}
		}
		else
		{
			Vector4 rect2;
			Vector4 softness2;
			bool num2 = TryReadRectMaskFromRenderer(out rect2, out softness2);
			((Renderer)_renderer).sharedMaterial = _sourceSharedMaterial;
			if (num2)
			{
				ApplyRectMaskToMaterialAndRenderer(null, rect2, softness2);
			}
			StyleLineUnderlayProxy obj2 = underlayProxy;
			if (obj2 != null)
			{
				obj2.Sync();
			}
		}
	}

	internal void Dispose()
	{
		if (_highlighted && (Object)(object)_renderer != (Object)null)
		{
			((Renderer)_renderer).sharedMaterial = _sourceSharedMaterial;
			_highlighted = false;
		}
		if (!((Object)(object)_runtimeMaterial == (Object)null))
		{
			if (Application.isPlaying)
			{
				Object.Destroy((Object)(object)_runtimeMaterial);
			}
			else
			{
				Object.DestroyImmediate((Object)(object)_runtimeMaterial);
			}
			_runtimeMaterial = null;
		}
	}

	private void EnsureRuntimeMaterial()
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Expected O, but got Unknown
		if (!((Object)(object)_runtimeMaterial != (Object)null))
		{
			_runtimeMaterial = new Material(_sourceSharedMaterial)
			{
				name = ((Object)_sourceSharedMaterial).name + " (OperateHighlight)"
			};
		}
	}

	private void ApplyOperateOutline(Material mat)
	{
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		mat.EnableKeyword("_OUTLINE_ON");
		mat.SetFloat("_OutlineEnable", 1f);
		mat.SetColor("_OutlineColor", _outlineColor);
		mat.SetFloat("_OutlineWidth", 0.1f);
		mat.SetFloat("_OutlineSoftness", 0.01f);
		mat.SetFloat("_OutlineMode", 2f);
		if (mat.HasProperty("_OutlineUseVertColor"))
		{
			mat.SetFloat("_OutlineUseVertColor", 0f);
		}
		if (mat.HasProperty("_ShadowTintColor"))
		{
			Color color = mat.GetColor("_ShadowTintColor");
			color.a = 0f;
			mat.SetColor("_ShadowTintColor", color);
		}
	}

	private bool TryReadRectMaskFromRenderer(out Vector4 rect, out Vector4 softness)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00db: Unknown result type (might be due to invalid IL or missing references)
		rect = default(Vector4);
		softness = Vector4.zero;
		if ((Object)(object)_renderer == (Object)null)
		{
			return false;
		}
		Material sharedMaterial = ((Renderer)_renderer).sharedMaterial;
		if ((Object)(object)sharedMaterial == (Object)null || !sharedMaterial.HasProperty(IdRectMaskRect))
		{
			return false;
		}
		EnsureMaskMpbScratch();
		((Renderer)_renderer).GetPropertyBlock(_maskMpbScratch);
		if (_maskMpbScratch.HasVector(IdRectMaskRect))
		{
			rect = _maskMpbScratch.GetVector(IdRectMaskRect);
		}
		else
		{
			rect = sharedMaterial.GetVector(IdRectMaskRect);
		}
		if (_maskMpbScratch.HasVector(IdRectMaskSoftness))
		{
			softness = _maskMpbScratch.GetVector(IdRectMaskSoftness);
		}
		else if (sharedMaterial.HasProperty(IdRectMaskSoftness))
		{
			softness = sharedMaterial.GetVector(IdRectMaskSoftness);
		}
		return true;
	}

	private void ApplyRectMaskToMaterialAndRenderer(Material mat, Vector4 rect, Vector4 softness)
	{
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)mat != (Object)null && mat.HasProperty(IdRectMaskRect))
		{
			mat.SetVector(IdRectMaskRect, rect);
			if (mat.HasProperty(IdRectMaskSoftness))
			{
				mat.SetVector(IdRectMaskSoftness, softness);
			}
		}
		if (!((Object)(object)_renderer == (Object)null))
		{
			EnsureMaskMpbScratch();
			((Renderer)_renderer).GetPropertyBlock(_maskMpbScratch);
			_maskMpbScratch.SetVector(IdRectMaskRect, rect);
			_maskMpbScratch.SetVector(IdRectMaskSoftness, softness);
			((Renderer)_renderer).SetPropertyBlock(_maskMpbScratch);
		}
	}

	private void EnsureMaskMpbScratch()
	{
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Expected O, but got Unknown
		if (_maskMpbScratch == null)
		{
			_maskMpbScratch = new MaterialPropertyBlock();
		}
	}
}
